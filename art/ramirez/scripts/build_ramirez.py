"""Editable Ramirez art study. Run with Blender --background --python this_file.

All source inputs are the versioned CC0 archive. Metres, Z-up, front -Y.
No gameplay dimensions or runtime mesh are used to shape the human.
"""
import argparse
import hashlib
import json
import math
import random
import sys
import zipfile
from pathlib import Path

import bpy
import bmesh
import numpy as np
from mathutils import Vector, Matrix, Quaternion
from mathutils.kdtree import KDTree
from mathutils.bvhtree import BVHTree

ROOT = Path(__file__).resolve().parents[3]
ART = ROOT / 'art/ramirez'
parser = argparse.ArgumentParser()
parser.add_argument('--out', default='study01')
parser.add_argument('--samples', type=int, default=32)
parser.add_argument('--size', type=int, default=1000)
parser.add_argument('--views', default='front,threequarter,side,back,portrait')
parser.add_argument('--clay', action='store_true')
parser.add_argument('--rest', action='store_true')
parser.add_argument('--mass', type=float, default=.30)
parser.add_argument('--sculpt-torso', action='store_true')
args = parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
OUT = ART / 'reviews' / args.out
OUT.mkdir(parents=True, exist_ok=True)
random.seed(20260920)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
scene = bpy.context.scene
scene.unit_settings.system = 'METRIC'
scene.render.engine = 'CYCLES'
scene.cycles.samples = args.samples
scene.cycles.use_denoising = True
try:
    prefs = bpy.context.preferences.addons['cycles'].preferences
    prefs.compute_device_type = 'ONEAPI'
    prefs.get_devices()
    for device in prefs.devices: device.use = device.type == 'ONEAPI'
    scene.cycles.device = 'GPU'
except Exception as exc:
    print('CPU fallback:', exc)
scene.render.resolution_x = args.size
scene.render.resolution_y = args.size
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = 'PNG'
scene.view_settings.view_transform = 'AgX'
scene.world.color = (.12, .12, .12)

def material(name, color, roughness=.5, metallic=0):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    p = m.node_tree.nodes.get('Principled BSDF')
    p.inputs['Base Color'].default_value = (*color, 1)
    p.inputs['Roughness'].default_value = roughness
    p.inputs['Metallic'].default_value = metallic
    return m

clay = material('QA neutral clay', (.32, .33, .34), .62)
skin = material('Skin - warm olive', (.34, .17, .105), .47)
cloth = material('Oxblood satin', (.19, .008, .018), .34)
gold = material('Woven gold', (.42, .25, .075), .53, .28)
white = material('Ivory cotton', (.72, .69, .62), .78)
leather = material('Red glove leather', (.26, .011, .018), .29)
hairmat = material('Dark brown hair', (.016, .009, .006), .57)
hairmat.node_tree.nodes.get('Principled BSDF').inputs['Specular IOR Level'].default_value=.18

def mesh(name, points, polygons, mat, uvfaces=None, uvcoords=None):
    data = bpy.data.meshes.new(name)
    data.from_pydata(points, [], polygons)
    data.update()
    ob = bpy.data.objects.new(name, data)
    scene.collection.objects.link(ob)
    data.materials.append(mat)
    for p in data.polygons: p.use_smooth = True
    rest=data.attributes.new('rest_position','FLOAT_VECTOR','POINT')
    for p,co in zip(rest.data,points):p.vector=co
    if uvfaces is not None:
        layer = data.uv_layers.new(name='SourceUV')
        for p, ids in zip(data.polygons, uvfaces):
            for loop, u in zip(p.loop_indices, ids): layer.data[loop].uv = uvcoords[u]
    return ob

def subdiv(ob, levels=2):
    mod = ob.modifiers.new('Surface subdivision - editable cage retained', 'SUBSURF')
    mod.levels = levels
    mod.render_levels = levels

archive = ROOT / 'tools/ev-art-source/makehuman-cc0-inputs.zip'
with zipfile.ZipFile(archive) as z:
    verts, tex, faces, uvfaces, groups = [], [], [], [], []
    group = ''
    for line in z.read('base.obj').decode().splitlines():
        t = line.split()
        if not t: continue
        if t[0] == 'v': verts.append(list(map(float, t[1:4])))
        elif t[0] == 'vt': tex.append(list(map(float, t[1:3])))
        elif t[0] == 'g': group = t[1]
        elif t[0] == 'f':
            parts = [x.split('/') for x in t[1:]]
            faces.append([int(x[0])-1 for x in parts])
            uvfaces.append([int(x[1])-1 for x in parts])
            groups.append(group)
    native = np.array(verts)
    for filename, amount in [('caucasian-male-young.target', 1), ('universal-male-young-maxmuscle-averageweight.target', 1-args.mass)]:
        for line in z.read(filename).decode().splitlines():
            t = line.split()
            if len(t) == 4 and not line.startswith('#'):
                native[int(t[0])] += np.array(list(map(float, t[1:]))) * amount
    skeleton = json.loads(z.read('default.mhskel'))
    source_weights = json.loads(z.read('default_weights.mhw'))['weights']
for line in (ART/'source/universal-male-young-maxmuscle-maxweight.target').read_text().splitlines():
    t=line.split()
    if len(t)==4 and not line.startswith('#'):native[int(t[0])]+=np.array(list(map(float,t[1:])))*args.mass
body_ids = sorted(set(i for f, g in zip(faces, groups) if g == 'body' for i in f))
ground = native[body_ids, 1].min()
scale = 1.8 / (native[body_ids, 1].max()-ground)
v = np.stack((native[:,0]*scale, -native[:,2]*scale, (native[:,1]-ground)*scale), axis=1)

# Art sculpt offsets are local surface changes, never changes to limb lengths.
# Smooth analytic fields keep one continuous cage and can be revised independently.
def bell(value, center, width): return np.exp(-((value-center)/width)**2)
q = v[body_ids].copy()
x,y,z = q.T.copy()
front = np.clip((-y-.012)/.055,0,1)
back = np.clip((y-.01)/.06,0,1)
pec = bell(abs(x),.089,.066)*bell(z,1.405,.054)
q[:,1] -= .018*pec*front
q[:,1] += .013*bell(abs(x),.10,.08)*bell(z,1.40,.115)*back
for height,depth in [(1.307,.006),(1.246,.008),(1.189,.007),(1.138,.004)]:
    q[:,1] -= depth*bell(abs(x),.036,.025)*bell(z,height,.022)*front
q[:,0] += np.sign(x)*(.013*bell(abs(x),.14,.05)*bell(z,1.36,.10)+.008*bell(abs(x),.06,.035)*bell(z,1.58,.055))
q[:,0] += np.sign(x)*.006*bell(abs(x),.06,.025)*bell(z,1.638,.038)
q[:,1] -= .0025*bell(abs(x),.032,.025)*bell(z,1.710,.011)*front
v[body_ids] = q

def joint(key): return Vector(v[skeleton['joints'][key]].mean(axis=0))
def head(name): return joint(skeleton['bones'][name]['head'])
def tail(name): return joint(skeleton['bones'][name]['tail'])

# Minimal deformation rig: retain native positions, aggregate twist segments.
rigdata = bpy.data.armatures.new('Ramirez anatomical skeleton')
rig = bpy.data.objects.new('RAMIREZ_RIG', rigdata)
scene.collection.objects.link(rig)
bpy.context.view_layer.objects.active = rig
rig.select_set(True)
bpy.ops.object.mode_set(mode='EDIT')
bone_specs = {}
def bone(name, start, end, parent=None):
    b = rigdata.edit_bones.new(name)
    b.head, b.tail = start, end
    if parent: b.parent = rigdata.edit_bones[parent]
    # Consistent roll, then pole directions are solved from actual rest matrices.
    b.align_roll(Vector((0, -1, 0)))
    bone_specs[name] = {'head': list(start), 'tail': list(end), 'parent': parent}

bone('root', (0, 0, .02), (0, 0, .18))
native_order = ['spine05', 'spine04', 'spine03', 'spine02', 'spine01', 'neck01', 'neck02', 'neck03', 'head']
for i, name in enumerate(native_order):
    bone(name, head(name), tail(name), native_order[i-1] if i else 'root')
for side in ['L', 'R']:
    bone('clavicle.'+side, head('clavicle.'+side), tail('clavicle.'+side), 'spine01')
    bone('upperarm.'+side, head('upperarm01.'+side), head('lowerarm01.'+side), 'clavicle.'+side)
    bone('forearm.'+side, head('lowerarm01.'+side), head('wrist.'+side), 'upperarm.'+side)
    bone('hand.'+side, head('wrist.'+side), tail('wrist.'+side), 'forearm.'+side)
    bone('thigh.'+side, head('upperleg01.'+side), head('lowerleg01.'+side), 'root')
    bone('shin.'+side, head('lowerleg01.'+side), head('foot.'+side), 'thigh.'+side)
    bone('foot.'+side, head('foot.'+side), tail('foot.'+side), 'shin.'+side)
bpy.ops.object.mode_set(mode='OBJECT')
rig.show_in_front = True

def remap_bone(name):
    side = name[-2:]
    if name.startswith('upperarm'): return 'upperarm'+side
    if name.startswith('lowerarm'): return 'forearm'+side
    if name.startswith(('wrist', 'finger', 'metacarpal')): return 'hand'+side
    if name.startswith('upperleg'): return 'thigh'+side
    if name.startswith('lowerleg'): return 'shin'+side
    if name.startswith(('foot', 'toe')): return 'foot'+side
    if name.startswith('breast'): return 'spine02'
    if name.startswith('pelvis'): return 'root'
    if name.startswith('shoulder'): return 'clavicle'+side
    if name in rigdata.bones: return name
    return 'head'

weights = [{} for _ in v]
for name, pairs in source_weights.items():
    key = remap_bone(name)
    for i, w in pairs: weights[i][key] = weights[i].get(key, 0) + w
for d in weights:
    s = sum(d.values())
    if s:
        for k in d: d[k] /= s

# Muscle volume around the actual humerus/radius/femur, not a shoulder proxy.
for i in body_ids:
    offset=Vector((0,0,0))
    for name,amount in weights[i].items():
        gain=.19 if name.startswith('upperarm.') else .10 if name.startswith('forearm.') else .07 if name.startswith('thigh.') else 0
        if not gain:continue
        spec=bone_specs[name];a=Vector(spec['head']);b=Vector(spec['tail']);axis=(b-a).normalized()
        local=Vector(v[i])-a;along=local.dot(axis)
        t=max(0,min(1,along/(b-a).length))
        offset+=(local-axis*along)*gain*amount*math.sin(math.pi*t)**.7
    v[i]+=np.array(offset)

# Candidate C: transfer only the continuous chest/abdomen/back surface from the
# independently sculpted CC0 male. Keep our mesh topology, UVs and limb dimensions.
sculpt_transfer=[]
if args.sculpt_torso:
    with bpy.data.libraries.load(str(ART/'source/studio-male-cc0.blend'),link=False) as (src,dst):dst.objects=['GEO-body_male_realistic']
    sculpt=dst.objects[0];scene.collection.objects.link(sculpt)
    for mod in sculpt.modifiers:
        if mod.type=='MULTIRES':mod.levels=2;mod.render_levels=2
    bpy.context.view_layer.update()
    evaluated=sculpt.evaluated_get(bpy.context.evaluated_depsgraph_get());data=evaluated.to_mesh()
    pts=np.array([p.co[:] for p in data.vertices]);lo=pts[:,2].min();fac=1.8/(pts[:,2].max()-lo);pts[:,2]-=lo;pts*=fac
    bvh=BVHTree.FromPolygons([Vector(p) for p in pts],[p.vertices[:] for p in data.polygons])
    for i in body_ids:
        x,y,z=v[i]
        influence=min(1,max(0,(z-1.10)/.075))*min(1,max(0,(1.53-z)/.065))*min(1,max(0,(.19-abs(x))/.045))
        if influence<=0:continue
        front=y<.01
        hit,normal,_,_=bvh.ray_cast(Vector((x,-.5 if front else .5,z)),Vector((0,1 if front else -1,0)))
        if hit is not None:
            delta=(hit.y-y)*influence;v[i,1]+=delta;sculpt_transfer.append(float(delta))
    evaluated.to_mesh_clear();bpy.data.objects.remove(sculpt,do_unlink=True)

tree = KDTree(len(body_ids))
for i in body_ids: tree.insert(v[i], i)
tree.balance()

def bind(ob, indices=None, rigid=None):
    buckets = {}
    for i, vertex in enumerate(ob.data.vertices):
        if rigid: w = {rigid: 1}
        elif indices is not None: w = weights[indices[i]]
        else:
            nearest = tree.find_n(vertex.co, 3)
            w = {}
            for _, index, dist in nearest:
                for key, amount in weights[index].items(): w[key] = w.get(key, 0)+amount/max(dist,.001)**2
            total = sum(w.values())
            if total: w = {key: amount/total for key, amount in w.items()}
        for name, amount in w.items(): buckets.setdefault(name, []).append((i, amount))
    for name, entries in buckets.items():
        vg = ob.vertex_groups.new(name=name)
        for index, amount in entries: vg.add([index], amount, 'REPLACE')
    arm = ob.modifiers.new('Anatomical deformation', 'ARMATURE')
    arm.object = rig
    arm.use_deform_preserve_volume = True

def source_mesh(name, group, mat, predicate=None):
    selected = [i for i, g in enumerate(groups) if g == group and (predicate is None or predicate(v[faces[i]]))]
    if group=='body':
        selected=[i for i in selected if np.mean([sum(amount for name,amount in weights[k].items() if name.startswith('hand.')) for k in faces[i]])<.4 and v[faces[i],2].mean()>.17]
    ids = sorted(set(k for i in selected for k in faces[i]))
    mapping = {k:j for j,k in enumerate(ids)}
    ob = mesh(name, v[ids], [[mapping[k] for k in faces[i]] for i in selected], mat, [uvfaces[i] for i in selected], tex)
    return ob, ids

# Skin under briefs is retained in the source mesh but not exposed in reviews.
def outside_glove(points):
    return True
body, indices = source_mesh('Ramirez continuous anatomical skin', 'body', skin, outside_glove)
bind(body, indices)
subdiv(body)

shorts, _ = source_mesh('Ramirez shorts - two leg pattern', 'helper-tights', cloth)
bm = bmesh.new(); bm.from_mesh(shorts.data)
for z, normal in [(1.035, (0,0,1)), (.625, (0,0,-1))]:
    bmesh.ops.bisect_plane(bm, geom=list(bm.verts)+list(bm.edges)+list(bm.faces), plane_co=(0,0,z), plane_no=normal, clear_outer=True, dist=.00001)
bm.to_mesh(shorts.data); bm.free()
for p in shorts.data.vertices:
    x,y,z = p.co
    t = max(0,min(1,(1.035-z)/.41))
    # Garment ease about each thigh, with joined native crotch topology.
    center = math.copysign(.12, x)
    p.co.x = x + (x-center)*(.16+.20*t)
    p.co.y = y*(1.14+.22*t)
# Subdivide the sewing pattern before sculpting fabric folds. Waist and hem remain
# constrained; folds vary across each panel instead of isotropic surface noise.
subdiv(shorts,2)
bpy.context.view_layer.objects.active=shorts
bpy.ops.object.modifier_apply(modifier=shorts.modifiers[0].name)
shorts.data.materials.append(gold)
for vertex in shorts.data.vertices:
    x,y,z=vertex.co;side=1 if x>0 else -1
    a=math.atan2(y,(x-side*.12))
    t=max(0,min(1,(1.035-z)/.41))
    fade=math.sin(math.pi*t)**.65
    wrinkle=(.006*math.sin(a*10+z*8)+.003*math.sin(a*17-z*19)+.002*math.sin(a*27+z*21))*fade
    vertex.co.x+=math.cos(a)*wrinkle;vertex.co.y+=math.sin(a)*wrinkle
# Separate sewn panels have continuous boundaries; material-per-face thresholds
# created visible stair steps in review03 and are not retained.
edge_count={}
for poly in shorts.data.polygons:
    ids=list(poly.vertices)
    for a,b in zip(ids,ids[1:]+ids[:1]):
        edge=tuple(sorted((a,b)));edge_count[edge]=edge_count.get(edge,0)+1
hem_edges=[edge for edge,count in edge_count.items() if count==1 and all(shorts.data.vertices[i].co.z<.66 for i in edge)]
hem_ids=sorted(set(i for e in hem_edges for i in e));lookup={k:i for i,k in enumerate(hem_ids)}
short_bvh=BVHTree.FromPolygons([p.co.copy() for p in shorts.data.vertices],[p.vertices[:] for p in shorts.data.polygons])
hem_points=[]
for dz in [0,.019]:
    for i in hem_ids:
        p=shorts.data.vertices[i].co.copy();p.z+=dz
        hit,n,_,_=short_bvh.find_nearest(p)
        hem_points.append(hit+n*.004)
hn=len(hem_ids)
hem=mesh('Continuous woven gold hems',hem_points,[(lookup[a],lookup[b],lookup[b]+hn,lookup[a]+hn) for a,b in hem_edges],gold)
bind(hem)
for sign in [-1,1]:
    points=[];fs=[]
    for z in np.linspace(.642,1.033,100):
        for y in np.linspace(-.045,.045,33):
            hit,n,_,_=short_bvh.ray_cast(Vector((sign*.7,y,z)),Vector((-sign,0,0)))
            if hit is None:hit=Vector((sign*.17,y,z));n=Vector((sign,0,0))
            points.append(hit+n*.004)
    for row in range(99):
        for col in range(32):fs.append((row*33+col,row*33+col+1,(row+1)*33+col+1,(row+1)*33+col))
    panel=mesh('Sewn gold side panel.'+str(sign),points,fs,gold);bind(panel)
bind(shorts)
subdiv(shorts,1)
solid = shorts.modifiers.new('Cloth edge thickness', 'SOLIDIFY'); solid.thickness=.002

# Small opaque sclera keep sockets visible even during the clay anatomy review.
for side, group in [('L','helper-l-eye'),('R','helper-r-eye')]:
    sclera=material('Warm sclera.'+side,(.52,.49,.44),.23)
    sclera.node_tree.nodes.get('Principled BSDF').inputs['Coat Weight'].default_value=.35
    eye, _ = source_mesh('Eye.'+side, group, sclera)
    bind(eye, rigid='head'); subdiv(eye)
    coords=np.array([p.co[:] for p in eye.data.vertices]);center=(coords.min(0)+coords.max(0))/2
    radius=(coords.max(0)[1]-coords.min(0)[1])/2
    iris_mat=material('Brown iris.'+side,(.065,.027,.011),.27)
    pupil_mat=material('Pupil.'+side,(.001,.001,.001),.16)
    rings=[]
    for r in [.0001,.0027,.0031,.0058,.0061]:
        rings.append([(center[0]+r*math.cos(a),center[1]-math.sqrt(max(0,radius*radius-r*r))-.00025,center[2]+r*math.sin(a)) for a in np.linspace(0,2*math.pi,64,endpoint=False)])
    points=[p for ring in rings for p in ring];fs=[]
    for row in range(4):
        for j in range(64):fs.append((row*64+j,row*64+(j+1)%64,(row+1)*64+(j+1)%64,(row+1)*64+j))
    iris=mesh('Iris and pupil.'+side,points,fs,iris_mat);iris.data.materials.append(pupil_mat)
    for p in iris.data.polygons:
        if p.index<64 or p.index>=192:p.material_index=1
    bind(iris,rigid='head')

def tube(name, paths, radius, mat, rigid=None):
    data=bpy.data.curves.new(name,'CURVE');data.dimensions='3D';data.resolution_u=2
    data.bevel_depth=radius;data.bevel_resolution=2
    for path in paths:
        spline=data.splines.new('POLY');spline.points.add(len(path)-1)
        for p,co in zip(spline.points,path):p.co=(*co,1)
    ob=bpy.data.objects.new(name,data);scene.collection.objects.link(ob);data.materials.append(mat)
    bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob
    bpy.ops.object.convert(target='MESH')
    if rigid: bind(ob,rigid=rigid)
    else: bind(ob)
    return ob

def loft(name, rings, mat, transform=None, closed=True):
    n=len(rings[0]);points=[tuple(transform @ Vector(p)) if transform else p for ring in rings for p in ring]
    fs=[]
    for row in range(len(rings)-1):
        for j in range(n):fs.append((row*n+j,row*n+(j+1)%n,(row+1)*n+(j+1)%n,(row+1)*n+j))
    if closed:fs += [tuple(reversed(range(n))),tuple((len(rings)-1)*n+j for j in range(n))]
    ob=mesh(name,points,fs,mat)
    bm=bmesh.new();bm.from_mesh(ob.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(ob.data);bm.free()
    return ob

def oval_rings(profile, axis='Y', count=48):
    result=[]
    for height,rx,ry,offset in profile:
        if axis=='Y':result.append([(rx*math.cos(a),height,ry*math.sin(a)+offset) for a in np.linspace(0,2*math.pi,count,endpoint=False)])
        else:result.append([(rx*math.cos(a),ry*math.sin(a)+offset,height) for a in np.linspace(0,2*math.pi,count,endpoint=False)])
    return result

waistcoords=np.array([p.co[:] for p in shorts.data.vertices if p.co.z>1.023])
waistx=max(abs(waistcoords[:,0]));waisty=(waistcoords[:,1].max()-waistcoords[:,1].min())/2;waistcy=(waistcoords[:,1].max()+waistcoords[:,1].min())/2
bandprofile=[(1.026,waistx+.002,waisty+.002,waistcy),(1.030,waistx+.004,waisty+.004,waistcy),(1.081,waistx-.008,waisty-.009,waistcy),(1.086,waistx-.009,waisty-.010,waistcy)]
band=loft('Gold elastic waistband',oval_rings(bandprofile,'Z',96),gold,closed=False);bind(band);subdiv(band,1)
bandpaths=[]
for height in np.linspace(1.032,1.081,10):
    t=(height-1.03)/.055;rx=waistx+.005-.012*t;ry=waisty+.005-.014*t
    bandpaths.append([(rx*math.cos(a),ry*math.sin(a)+waistcy,height) for a in np.linspace(0,2*math.pi,129)])
tube('Waistband stitched elastic rows',bandpaths,.00075,gold)
labelmat=material('Waist label black',(.010,.009,.007),.6)
label=mesh('Ramirez waistband label',[(-.062,-waisty-.006+waistcy,1.035),(.062,-waisty-.006+waistcy,1.035),(.062,-waisty-.002+waistcy,1.079),(-.062,-waisty-.002+waistcy,1.079)],[(0,1,2,3)],labelmat);bind(label)
font=bpy.data.curves.new('RAMIREZ embroidery','FONT');font.body='RAMIREZ';font.align_x='CENTER';font.align_y='CENTER';font.size=.026;font.extrude=.0002
textob=bpy.data.objects.new('RAMIREZ name embroidery',font);scene.collection.objects.link(textob)
textob.location=(0,-waisty-.008+waistcy,1.056);textob.rotation_euler=(math.pi/2,0,0);font.materials.append(gold)
bpy.ops.object.select_all(action='DESELECT');textob.select_set(True);bpy.context.view_layer.objects.active=textob;bpy.ops.object.convert(target='MESH');bpy.ops.object.transform_apply(location=True,rotation=True,scale=True);bind(textob)

for side, sign in [('L',1),('R',-1)]:
    transform=rigdata.bones['hand.'+side].matrix_local
    rings=oval_rings([(.005,.035,.029,0),(.015,.043,.034,0),(.048,.053,.040,.002),(.088,.060,.048,.003),(.122,.059,.044,.006),(.144,.046,.032,.005),(.153,.010,.009,.002)])
    glove=loft('Glove padded shell.'+side,rings,leather,transform)
    bind(glove,rigid='hand.'+side);subdiv(glove)
    # Attached thumb follows its own bent profile instead of a spherical mitten.
    thumb_rings=[]
    for cy,cx,cz,rx,rz in [(.024,-.025,-.012,.012,.014),(.050,-.045,-.022,.019,.022),(.086,-.049,-.022,.019,.023),(.117,-.039,-.018,.015,.020),(.128,-.030,-.015,.003,.004)]:
        thumb_rings.append([(sign*(cx+rx*math.cos(a)),cy,cz+rz*math.sin(a)) for a in np.linspace(0,2*math.pi,32,endpoint=False)])
    thumb=loft('Glove attached thumb.'+side,thumb_rings,leather,transform)
    bind(thumb,rigid='hand.'+side);subdiv(thumb)
    cuff=loft('Hand wrap.'+side,oval_rings([(-.083,.033,.028,0),(-.079,.034,.029,0),(-.035,.034,.03,0),(.018,.035,.029,0),(.022,.034,.028,0)]),white,transform)
    bind(cuff,rigid='hand.'+side);subdiv(cuff,1)
    paths=[]
    for h in np.linspace(-.079,.014,12):
        paths.append([transform @ Vector((.035*math.cos(a),h+.003*math.sin(a),.030*math.sin(a))) for a in np.linspace(0,2*math.pi,65)])
    tube('Wrap overlaps.'+side,paths,.00065,white,'hand.'+side)
    # Seam is sampled on the shell itself, not copied from an obsolete profile.
    bpy.context.view_layer.update()
    shell_bvh=BVHTree.FromObject(glove,bpy.context.evaluated_depsgraph_get())
    seam=[]
    for ring in rings[1:-1]:
        p=Vector(ring[26]);p.x*=1.005;p.z*=1.005
        hit,n,_,_=shell_bvh.find_nearest(transform @ p)
        seam.append(hit+n*.00035)
    tube('Glove panel seam.'+side,[seam],.0005,leather,'hand.'+side)
    crown_points=[(-.023,.074),(.023,.074),(.027,.098),(.014,.087),(0,.108),(-.014,.087),(-.027,.098)]
    crown_surface=[]
    for x,h in crown_points:
        hit,n,_,_=shell_bvh.find_nearest(transform @ Vector((x,h,.051)))
        crown_surface.append(hit+n*.0007)
    crown=mesh('Glove gold crown.'+side,crown_surface,[tuple(range(len(crown_points)))],gold)
    bind(crown,rigid='hand.'+side)

    # Foot-shaped last: toe box / instep / ankle shaft, separately readable sole.
    ankle=head('foot.'+side)
    bootprofile=[(.018,.047,.121,-.055),(.026,.048,.125,-.055),(.048,.050,.126,-.055),(.071,.049,.115,-.046),(.095,.045,.092,-.030),(.123,.037,.058,-.005),(.17,.037,.044,.002),(.27,.043,.043,.002),(.285,.044,.044,.002)]
    rings=oval_rings(bootprofile,'Z')
    rings=[[(x+ankle.x,y+ankle.y,z) for x,y,z in ring] for ring in rings]
    boot=loft('White leather boxing boot.'+side,rings,white)
    bind(boot);subdiv(boot)
    soleprofile=[(.007,.047,.123,-.055),(.011,.050,.129,-.055),(.025,.050,.129,-.055),(.031,.048,.126,-.055)]
    sole=loft('Red rubber outsole.'+side,[[(x+ankle.x,y+ankle.y,z) for x,y,z in ring] for ring in oval_rings(soleprofile,'Z')],leather)
    bind(sole);subdiv(sole,1)
    lacepaths=[];panelpaths=[]
    for i,z in enumerate(np.linspace(.085,.27,13)):
        front=np.interp(z,[.085,.123,.17,.285],[-.15,-.063,-.043,-.043])+ankle.y
        for flip in [-1,1]:
            lacepaths.append([(ankle.x+flip*.021,front,z),(ankle.x-flip*.020,front-.002,z+.011)])
    tube('Boot cotton laces.'+side,lacepaths,.0018,white)
    for sign2 in [-1,1]:
        panelpaths.append([(ankle.x+sign2*.026,np.interp(z,[.08,.123,.17,.285],[-.15,-.060,-.041,-.041])+ankle.y,z) for z in np.linspace(.08,.28,32)])
    tube('Boot lace panel piping.'+side,panelpaths,.0025,leather)

def surface_hair():
    # Sample native skin triangles; explicit deterministic curl geometry is editable.
    def scalp_density(point):
        x,y,z=point
        front=max(0,min(1,(-y-.02)/.09));side=min(1,(abs(x)/.06)**2)
        threshold=max(1.685,1.696+.043*front-.025*side)
        return max(0,min(1,(z-threshold)/.006))
    scalpfaces=[i for i,g in enumerate(groups) if g=='body' and any(scalp_density(v[k])>0 for k in faces[i])]
    # One head surface owns scalp colour; a duplicate scalp shell is redundant.
    density=body.data.attributes.new('scalp_density','FLOAT','POINT')
    for vertex,d in zip(body.data.vertices,density.data):d.value=scalp_density(vertex.co)
    triangles=[];areas=[]
    for index in scalpfaces:
        f=faces[index]
        for j in range(1,len(f)-1):
            a,b,c=(Vector(v[k]) for k in [f[0],f[j],f[j+1]])
            triangles.append((a,b,c));areas.append((b-a).cross(c-a).length/2)
    strands=[]
    for a,b,c in random.choices(triangles,weights=areas,k=5200):
        u,w=random.random(),random.random()
        if u+w>1:u,w=1-u,1-w
        p=a+(b-a)*u+(c-a)*w
        if random.random()>scalp_density(p):continue
        normal=(b-a).cross(c-a).normalized()
        # Outward normal is checked against the head centre.
        if normal.dot(p-Vector((0,-.015,1.73)))<0:normal=-normal
        top=max(0,min(1,(p.z-1.725)/.060))
        length=.003+top*random.uniform(.012,.029)
        curl=.0007+top*.0032
        tangent=normal.cross(Vector((0,1,.1))).normalized();bitangent=normal.cross(tangent)
        phase=random.uniform(0,6.28)
        growth=(normal+tangent*random.uniform(-.5,.5)+bitangent*random.uniform(-.4,.4)).normalized()
        strand=[]
        for t in np.linspace(0,1,13):
            angle=phase+t*math.pi*3.5
            strand.append(p+growth*(length*t)+curl*(math.sin(angle)*tangent+math.cos(angle)*bitangent)*min(1,t*5))
        strands.append(strand)
    tube('Short individual curls',strands,.00021,hairmat,'head')
surface_hair()

skin_bvh=BVHTree.FromPolygons([Vector(p) for p in v],[f for f,g in zip(faces,groups) if g=='body'])
def facial_hair():
    paths=[]
    # Short beard: sparse cheek edge, dense jaw, separate moustache, open lips.
    for _ in range(16000):
        x=random.uniform(-.075,.075);z=random.uniform(1.568,1.672)
        upper=1.594+.75*abs(x)
        moustache=abs(x)<.030 and 1.630<z<1.640
        if not (z<upper or moustache):continue
        if abs(x)<.030 and 1.612<z<1.630:continue
        hit,n,_,_=skin_bvh.ray_cast(Vector((x,-.4,z)),Vector((0,1,0)))
        if hit is None or hit.y>-.040:continue
        length=random.uniform(.001,.0032)
        paths.append([hit+n*.0002,hit+n*length+Vector((0,0,-length*.5))])
    tube('Individual beard stubble',paths,.00015,hairmat,'head')
    brows=[]
    for sign in [-1,1]:
        for _ in range(650):
            x=sign*random.uniform(.017,.060)
            z=1.701+.007*math.sin((abs(x)-.017)/.043*math.pi)+random.uniform(-.0025,.0025)
            hit,n,_,_=skin_bvh.ray_cast(Vector((x,-.4,z)),Vector((0,1,0)))
            if hit is None:continue
            brows.append([hit+n*.0005,hit+n*.001+Vector((sign*.0025,0,.0014))])
    tube('Individual eyebrows',brows,.00022,hairmat,'head')
facial_hair()

def noise_surface(mat, colors, scale, distance, roughness, skin_scatter=False):
    nodes=mat.node_tree.nodes;links=mat.node_tree.links;p=nodes.get('Principled BSDF')
    coord=nodes.new('ShaderNodeAttribute');coord.attribute_name='rest_position'
    noise=nodes.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=scale;noise.inputs['Detail'].default_value=3
    links.new(coord.outputs['Vector'],noise.inputs['Vector'])
    bump=nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.32;bump.inputs['Distance'].default_value=distance
    links.new(noise.outputs['Fac'],bump.inputs['Height']);links.new(bump.outputs['Normal'],p.inputs['Normal'])
    macro=nodes.new('ShaderNodeTexNoise');macro.inputs['Scale'].default_value=85 if skin_scatter else 140;macro.inputs['Detail'].default_value=3
    links.new(coord.outputs['Vector'],macro.inputs['Vector'])
    ramp=nodes.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].color=(*colors[0],1);ramp.color_ramp.elements[1].color=(*colors[1],1)
    links.new(macro.outputs['Fac'],ramp.inputs[0]);links.new(ramp.outputs['Color'],p.inputs['Base Color'])
    p.inputs['Roughness'].default_value=roughness
    if skin_scatter:
        p.inputs['Subsurface Weight'].default_value=.13;p.inputs['Subsurface Radius'].default_value=(1,.45,.24);p.inputs['Subsurface Scale'].default_value=.012
        p.inputs['Coat Weight'].default_value=.035;p.inputs['Coat Roughness'].default_value=.4
noise_surface(skin,[(.22,.095,.048),(.27,.133,.075)],1700,.00017,.53,True)
# Verified CC0 atlas follows the original face/body UVs without projected faces.
nodes=skin.node_tree.nodes;links=skin.node_tree.links;p=nodes.get('Principled BSDF')
atlas=nodes.new('ShaderNodeTexImage');atlas.name='CC0 skin atlas - original UV'
atlas.image=bpy.data.images.load(str(ART/'source/skin/young_lightskinned_male_diffuse_Bronze.png'));atlas.image.pack()
tint=nodes.new('ShaderNodeMixRGB');tint.blend_type='MULTIPLY';tint.inputs[0].default_value=1;tint.inputs[2].default_value=(.72,.70,.65,1)
links.new(atlas.outputs['Color'],tint.inputs[1])
scalp=nodes.new('ShaderNodeAttribute');scalp.attribute_name='scalp_density'
mixscalp=nodes.new('ShaderNodeMixRGB');mixscalp.inputs[2].default_value=(.012,.006,.003,1)
links.new(scalp.outputs['Fac'],mixscalp.inputs[0]);links.new(tint.outputs[0],mixscalp.inputs[1]);links.new(mixscalp.outputs[0],p.inputs['Base Color'])
normaltex=nodes.new('ShaderNodeTexImage');normaltex.image=bpy.data.images.load(str(ART/'source/skin/Aksel_Skin_NRM.png'));normaltex.image.colorspace_settings.name='Non-Color';normaltex.image.pack()
normal=nodes.new('ShaderNodeNormalMap');normal.inputs['Strength'].default_value=.35
links.new(normaltex.outputs['Color'],normal.inputs['Color'])
bump=next(n for n in nodes if n.type=='BUMP');links.new(normal.outputs['Normal'],bump.inputs['Normal'])
noise_surface(leather,[(.105,.002,.005),(.22,.008,.014)],1350,.00030,.35)
noise_surface(white,[(.57,.55,.49),(.78,.75,.68)],2100,.00015,.66)
noise_surface(cloth,[(.075,.002,.006),(.16,.006,.016)],2300,.0001,.42)
cloth.node_tree.nodes.get('Principled BSDF').inputs['Sheen Weight'].default_value=.05
cloth.node_tree.nodes.get('Principled BSDF').inputs['Specular IOR Level'].default_value=.24
cloth.node_tree.nodes.get('Principled BSDF').inputs['Anisotropic'].default_value=.18

sys.path.insert(0,str(Path(__file__).parent))
from ramirez_pose import apply_pose
if not args.rest: apply_pose(rig,'guard')

def aim(ob, at): ob.rotation_euler=(Vector(at)-ob.location).to_track_quat('-Z','Y').to_euler()
def area(name, loc, at, power, size, color=(1,1,1)):
    data=bpy.data.lights.new(name,'AREA'); data.energy=power; data.shape='DISK'; data.size=size; data.color=color
    ob=bpy.data.objects.new(name,data);scene.collection.objects.link(ob);ob.location=loc;aim(ob,at)
area('Large neutral key',(-2.2,-3,3.5),(0,0,1),450,2.5)
area('Soft fill',(2,-2,2),(0,0,1),190,2)
area('Shoulder rim',(1,2,3),(0,0,1.2),500,2)
floor=material('Studio charcoal',(.055,.06,.065),.8)
bpy.ops.mesh.primitive_plane_add(size=200)
bpy.context.object.name='Neutral studio floor';bpy.context.object.data.materials.append(floor)
camera_data=bpy.data.cameras.new('Review camera');camera=bpy.data.objects.new('Review camera',camera_data);scene.collection.objects.link(camera);scene.camera=camera
camera_data.type='ORTHO';camera_data.ortho_scale=2.06;camera_data.lens=70
if args.clay:
    for ob in scene.objects:
        if ob.type=='MESH' and ob.name!='Neutral studio floor':
            for i in range(len(ob.data.materials)):ob.data.materials[i]=clay
views={'front':((0,-5,.94),(0,0,.88),2.02),'threequarter':((3,-5,1.25),(0,0,.90),2.02),'side':((5,0,1),(0,0,.88),2.02),'back':((0,5,1),(0,0,.88),2.02),'portrait':((.30,-3,1.65),(0,-.02,1.56),.57),'torso':((1,-4,1.55),(0,0,1.38),.90)}
camera.location=views['threequarter'][0];aim(camera,views['threequarter'][1])
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'ramirez.blend'),compress=True)
metrics={'status':'STUDY_NOT_APPROVED','height_rest_m':1.8,'uniform_source_scale':scale,'mass_morph':args.mass,'sculpt_torso':args.sculpt_torso,'sculpt_transfer_vertices':len(sculpt_transfer),'sculpt_transfer_max_m':max(map(abs,sculpt_transfer),default=0),'builder_sha256':hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),'source_archive_sha256':hashlib.sha256(archive.read_bytes()).hexdigest(),'bones':bone_specs,'body_cage_vertices':len(body.data.vertices),'body_cage_faces':len(body.data.polygons),'notes':['No runtime rig stretching.','Reference fidelity requires rendered inspection; numbers do not constitute visual approval.']}
(OUT/'metrics.json').write_text(json.dumps(metrics,indent=2))
for name in args.views.split(','):
    position,target,ortho=views[name]
    camera.location=position;aim(camera,target);camera_data.ortho_scale=ortho
    scene.render.filepath=str(OUT/(name+'.png'))
    bpy.ops.render.render(write_still=True)
print('RAMIREZ_REVIEW_COMPLETE',OUT,flush=True)
