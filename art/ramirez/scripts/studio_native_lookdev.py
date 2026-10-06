"""Candidate B look development on native geometry, without foreign UV transfer.

Open studio17-equipment/studio-ramirez-rig.blend before this script.
This is a Blender study, not a production/WebGL material or likeness approval.
"""
import bpy, math, random, sys, json, argparse, hashlib
import numpy as np
from pathlib import Path
from mathutils import Vector, Matrix
from mathutils.bvhtree import BVHTree
from mathutils.geometry import barycentric_transform

parser=argparse.ArgumentParser();parser.add_argument('--out',default='studio18-native-lookdev')
parser.add_argument('--views',default='threequarter,portrait')
parser.add_argument('--sample-diffuse',action='store_true')
args=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
art=Path(__file__).resolve().parents[1];out=art/'reviews'/args.out;out.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(Path(__file__).parent));from ramirez_pose import apply_pose
random.seed(20260921)
scene=bpy.context.scene;rig=bpy.data.objects['RAMIREZ_RIG'];body=bpy.data.objects['Studio Ramirez continuous sculpt']
for p in rig.pose.bones:p.matrix_basis=Matrix.Identity(4)
bpy.context.view_layer.update()
sampled_colors=None
if args.sample_diffuse:
    # Resolve texture colours before interpolation. Transferring UV coordinates
    # across different topologies interpolates through unrelated atlas islands.
    # Dense vertex colours avoid that failure and leave native UVs unchanged.
    bpy.ops.object.select_all(action='DESELECT');body.select_set(True);bpy.context.view_layer.objects.active=body
    multires=next(m for m in body.modifiers if m.type=='MULTIRES');multires.levels=2
    bpy.ops.object.modifier_apply(modifier=multires.name)
    with bpy.data.libraries.load(str(art/'reviews/detail07/ramirez.blend'),link=False) as (a,b):b.objects=['Ramirez continuous anatomical skin']
    donor=b.objects[0];donor.data.calc_loop_triangles();triangles=list(donor.data.loop_triangles)
    tree=BVHTree.FromPolygons([p.co for p in donor.data.vertices],[t.vertices for t in triangles])
    source_uv=donor.data.uv_layers[0]
    atlas=bpy.data.images.load(str(art/'source/skin/young_lightskinned_male_diffuse_Bronze.png'))
    width,height=atlas.size;pixels=np.empty(width*height*4,dtype=np.float32);atlas.pixels.foreach_get(pixels);pixels=pixels.reshape(height,width,4)
    sampled_colors=[]
    for v in body.data.vertices:
        p=body.matrix_world @ v.co
        if p.z>1.55:p.z+=.009*max(0,min(1,(1.79-p.z)/.06))
        hit,_,index,_=tree.find_nearest(p);tri=triangles[index]
        uv=barycentric_transform(hit,*[donor.data.vertices[i].co for i in tri.vertices],*[Vector((*source_uv.data[i].uv,0)) for i in tri.loops])
        x=max(0,min(width-1.001,uv.x*(width-1)));y=max(0,min(height-1.001,uv.y*(height-1)));i,j=int(x),int(y);u,v=x-i,y-j
        c=((1-u)*(1-v)*pixels[j,i,:3]+u*(1-v)*pixels[j,i+1,:3]+(1-u)*v*pixels[j+1,i,:3]+u*v*pixels[j+1,i+1,:3])
        c=np.where(c<=.04045,c/12.92,((c+.055)/1.055)**2.4)*np.array([.72,.70,.66])
        sampled_colors.append(Vector(c.tolist()))
positions=[body.matrix_world @ v.co for v in body.data.vertices]

def smooth(a,b,v):
    t=max(0,min(1,(v-a)/(b-a)));return t*t*(3-2*t)

def scalp(p):
    x,y,z=p;front=smooth(-.035,-.11,y);side=min(1,(abs(x)/.071)**2)
    return smooth(0,.009,z-(1.700+.038*front-.016*side))

def beard(p):
    x,y,z=p
    edge=1.595+.62*abs(x)
    lower=smooth(1.555,1.576,z)*(1-smooth(edge-.008,edge+.006,z))
    moustache=(1-smooth(.027,.035,abs(x)))*smooth(1.623,1.629,z)*(1-smooth(1.638,1.643,z))
    return max(lower,moustache)*(1-smooth(-.065,-.015,y))

def newmat(name,color,rough=.45):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=rough
    return m

skin=newmat('B native skin pigment and microstructure',(.235,.111,.060),.46)
nodes=skin.node_tree.nodes;links=skin.node_tree.links;bsdf=nodes.get('Principled BSDF')
rest=body.data.attributes.get('rest_position') or body.data.attributes.new('rest_position','FLOAT_VECTOR','POINT')
pigment=body.data.color_attributes.new(name='Native anatomical pigment',type='FLOAT_COLOR',domain='POINT')
for index,(p,r,c) in enumerate(zip(positions,rest.data,pigment.data)):
    r.vector=p;x,y,z=p
    base=sampled_colors[index] if sampled_colors is not None else Vector((.245,.119,.070))
    # Pigment changes are authored on this mesh; no other character's UVs.
    face=smooth(1.52,1.59,z)*(1-smooth(-.07,-.025,y))
    cheek=math.exp(-((abs(x)-.046)/.025)**2-((z-1.655)/.030)**2)*face
    base=base.lerp(Vector((.28,.103,.065)),cheek*.35)
    lip=math.exp(-(x/.025)**6-((z-1.613)/.0075)**4)*face
    base=base.lerp(Vector((.215,.068,.056)),lip*.75)
    nipple=math.exp(-((abs(x)-.109)/.009)**4-((z-1.363)/.008)**4)*(1-smooth(-.07,-.02,y))
    base=base.lerp(Vector((.16,.060,.032)),nipple*.65)
    base=base.lerp(Vector((.024,.016,.012)),beard(p)*.47)
    base=base.lerp(Vector((.012,.007,.004)),scalp(p)*.96)
    c.color=(*base,1)
attribute=nodes.new('ShaderNodeAttribute');attribute.attribute_name=pigment.name
coord=nodes.new('ShaderNodeAttribute');coord.attribute_name='rest_position'
macro=nodes.new('ShaderNodeTexNoise');macro.inputs['Scale'].default_value=95;macro.inputs['Detail'].default_value=3
links.new(coord.outputs['Vector'],macro.inputs['Vector'])
ramp=nodes.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].color=(.65,.56,.48,1);ramp.color_ramp.elements[1].color=(1.05,1.0,.94,1)
links.new(macro.outputs['Fac'],ramp.inputs[0])
mix=nodes.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=.65
links.new(attribute.outputs['Color'],mix.inputs[1]);links.new(ramp.outputs[0],mix.inputs[2]);links.new(mix.outputs[0],bsdf.inputs['Base Color'])
fine=nodes.new('ShaderNodeTexNoise');fine.inputs['Scale'].default_value=2300;fine.inputs['Detail'].default_value=2
links.new(coord.outputs['Vector'],fine.inputs['Vector'])
bump=nodes.new('ShaderNodeBump');bump.inputs['Distance'].default_value=.00013;bump.inputs['Strength'].default_value=.28
links.new(fine.outputs['Fac'],bump.inputs['Height']);links.new(bump.outputs['Normal'],bsdf.inputs['Normal'])
rough=nodes.new('ShaderNodeMapRange');rough.inputs['To Min'].default_value=.35;rough.inputs['To Max'].default_value=.53
links.new(macro.outputs['Fac'],rough.inputs['Value']);links.new(rough.outputs[0],bsdf.inputs['Roughness'])
bsdf.inputs['Subsurface Weight'].default_value=.17;bsdf.inputs['Subsurface Radius'].default_value=(1,.42,.22);bsdf.inputs['Subsurface Scale'].default_value=.008
bsdf.inputs['Coat Weight'].default_value=.025;bsdf.inputs['Coat Roughness'].default_value=.38
bsdf.inputs['Specular IOR Level'].default_value=.28
body.data.materials.clear();body.data.materials.append(skin)

# Sample the intact evaluated sculpt. Explicit hairs stay rooted on that surface.
mask=next(m for m in body.modifiers if m.type=='MASK');mask.show_viewport=False
evaluated=body.evaluated_get(bpy.context.evaluated_depsgraph_get());mesh=evaluated.to_mesh();mesh.calc_loop_triangles()
pts=[body.matrix_world @ p.co for p in mesh.vertices]
bvh=BVHTree.FromPolygons(pts,[t.vertices for t in mesh.loop_triangles])
triangles=[];areas=[]
for t in mesh.loop_triangles:
    a,b,c=[pts[i] for i in t.vertices]
    if max(a.z,b.z,c.z)<1.69:continue
    if not any(scalp(p)>.05 for p in [a,b,c]):continue
    triangles.append((a,b,c));areas.append((b-a).cross(c-a).length*.5)
hairmat=newmat('B dark brown hair',(.006,.003,.0015),.59)
hairmat.node_tree.nodes.get('Principled BSDF').inputs['Specular IOR Level'].default_value=.20

def hair_object(name,paths,radius):
    data=bpy.data.curves.new(name,'CURVE');data.dimensions='3D';data.resolution_u=1;data.bevel_depth=radius;data.bevel_resolution=1
    for path in paths:
        s=data.splines.new('POLY');s.points.add(len(path)-1)
        for p,v in zip(s.points,path):p.co=(*v,1)
    ob=bpy.data.objects.new(name,data);scene.collection.objects.link(ob);data.materials.append(hairmat)
    bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob;bpy.ops.object.convert(target='MESH')
    g=ob.vertex_groups.new(name='head');g.add(list(range(len(ob.data.vertices))),1,'REPLACE');m=ob.modifiers.new('Single head attachment','ARMATURE');m.object=rig
    return ob

paths=[]
for a,b,c in random.choices(triangles,weights=areas,k=9500):
    u,v=random.random(),random.random()
    if u+v>1:u,v=1-u,1-v
    p=a+(b-a)*u+(c-a)*v
    if random.random()>scalp(p):continue
    n=(b-a).cross(c-a).normalized()
    if n.dot(p-Vector((0,-.01,1.73)))<0:n=-n
    top=smooth(1.716,1.781,p.z);length=.002+top*random.uniform(.009,.021)
    tangent=n.cross(Vector((0,1,.05))).normalized();bitangent=n.cross(tangent)
    growth=(n+tangent*random.uniform(-.65,.65)+bitangent*random.uniform(-.35,.35)).normalized()
    phase=random.uniform(0,2*math.pi);radius=.0003+top*random.uniform(.0018,.0038)
    path=[]
    for j in range(14):
        t=j/13;angle=phase+t*math.pi*2.2
        path.append(p+growth*(length*t)+radius*(math.sin(angle)*tangent+math.cos(angle)*bitangent)*min(1,t*5))
    paths.append(path)
hair_object('B surface rooted short curls',paths,.00015)
paths=[]
for _ in range(22000):
    x=random.uniform(-.084,.084);z=random.uniform(1.561,1.672)
    hit,n,_,_=bvh.ray_cast(Vector((x,-.35,z)),Vector((0,1,0)))
    if hit is None or random.random()>beard(hit):continue
    length=random.uniform(.0009,.0028);paths.append([hit+n*.00015,hit+n*length+Vector((x*.01,0,-length*.5))])
hair_object('B short jaw and moustache stubble',paths,.00009)
paths=[]
for sign in [-1,1]:
    for _ in range(900):
        x=sign*random.uniform(.012,.060);z=1.688+.004*math.sin((abs(x)-.012)/.048*math.pi)+random.uniform(-.0023,.0023)
        hit,n,_,_=bvh.ray_cast(Vector((x,-.35,z)),Vector((0,1,0)))
        if hit is not None:paths.append([hit+n*.0001,hit+n*.0008+Vector((sign*.0024,0,.0005))])
hair_object('B individual eyebrows',paths,.00013)
evaluated.to_mesh_clear();mask.show_viewport=True

with bpy.data.libraries.load(str(art/'reviews/detail07/ramirez.blend'),link=False) as (a,b):
    b.materials=['Oxblood satin','Woven gold','Ivory cotton','Red glove leather','Waist label black']
mats={m.name.split('.')[0]:m for m in b.materials if m}
for ob in scene.objects:
    if ob.type!='MESH':continue
    name=ob.name;mat=None
    if name.startswith('Ramirez shorts'):mat=mats['Oxblood satin']
    elif name.startswith(('Glove padded','Glove attached','Glove panel','Red rubber','Boot lace')):mat=mats['Red glove leather']
    elif name.startswith(('Glove gold','Gold elastic','Continuous woven','Sewn gold','Waistband','RAMIREZ name')):mat=mats['Woven gold']
    elif name.startswith('Ramirez waistband'):mat=mats['Waist label black']
    elif name.startswith(('Hand wrap','Wrap overlaps','White leather','Boot cotton')):mat=mats['Ivory cotton']
    if mat:
        ob.data.materials.clear();ob.data.materials.append(mat)
        r=ob.data.attributes.get('rest_position') or ob.data.attributes.new('rest_position','FLOAT_VECTOR','POINT')
        for p,d in zip(ob.data.vertices,r.data):d.vector=p.co

# Fit pupils/iris to the source's actual eyeball centres. A dark limbus and
# nonuniform radial pigment avoid the flat sticker appearance of the old discs.
for side,sign in [('L',1),('R',-1)]:
    eye=bpy.data.objects['Sculpt eye.'+side];center=Vector((sign*.03504,-.12974,1.67589))
    old=Vector((sign*.035,-.135,1.683))
    for p in eye.data.vertices:p.co=center+(p.co-old)*.92
    sclera=newmat('Natural sclera '+side,(.37,.34,.30),.25);eye.data.materials.clear();eye.data.materials.append(sclera)
    iris=newmat('Radial brown iris '+side,(.060,.028,.010),.25);pupil=newmat('Dark pupil '+side,(.0005,.0003,.0002),.18);limbus=newmat('Iris limbus '+side,(.007,.004,.002),.22)
    verts=[];faces=[];radii=[0,.0020,.0022,.0046,.0054,.0056]
    for r in radii:
        for j in range(96):
            a=j/96*2*math.pi;verts.append(center+Vector((r*math.cos(a),-math.sqrt((.013*.92)**2-r*r)-.00015,r*math.sin(a))))
    for row in range(len(radii)-1):
        for j in range(96):faces.append((row*96+j,row*96+(j+1)%96,(row+1)*96+(j+1)%96,(row+1)*96+j))
    data=bpy.data.meshes.new('Iris '+side);data.from_pydata(verts,[],faces);ob=bpy.data.objects.new(data.name,data);scene.collection.objects.link(ob)
    for m in [iris,pupil,limbus]:data.materials.append(m)
    for p in data.polygons:p.material_index=1 if p.index<96 else (2 if p.index>=384 else 0);p.use_smooth=True
    colors=data.color_attributes.new(name='Iris fibres',type='FLOAT_COLOR',domain='POINT')
    for i,c in enumerate(colors.data):
        val=.60+.30*math.sin((i%96)*2.4)+.20*math.sin((i%96)*.8);c.color=(.07*val,.033*val,.012*val,1)
    attr=iris.node_tree.nodes.new('ShaderNodeAttribute');attr.attribute_name=colors.name;iris.node_tree.links.new(attr.outputs['Color'],iris.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])
    g=ob.vertex_groups.new(name='head');g.add(list(range(len(verts))),1,'REPLACE');m=ob.modifiers.new('Head attachment','ARMATURE');m.object=rig

prefs=bpy.context.preferences.addons['cycles'].preferences;prefs.compute_device_type='ONEAPI';prefs.get_devices()
for d in prefs.devices:d.use=d.type=='ONEAPI'
scene.cycles.device='GPU';scene.cycles.samples=48;scene.render.resolution_x=1100;scene.render.resolution_y=1100
apply_pose(rig,'guard');scene['review_status']='NOT_APPROVED_REFERENCE_FIDELITY_UNRESOLVED'
bpy.ops.wm.save_as_mainfile(filepath=str(out/'studio-native-lookdev.blend'),compress=True)
views={'threequarter':((3,-5,1.2),(0,0,.88),2.02),'portrait':((.30,-3,1.65),(0,-.02,1.56),.57),'front':((0,-5,1.1),(0,0,.88),2.02),'back':((0,5,1.1),(0,0,.88),2.02)}
for name in args.views.split(','):
    pos,at,size=views[name];scene.camera.location=pos;scene.camera.rotation_euler=(Vector(at)-scene.camera.location).to_track_quat('-Z','Y').to_euler();scene.camera.data.ortho_scale=size
    scene.render.filepath=str(out/(name+'.png'));bpy.ops.render.render(write_still=True)
(out/'lookdev-report.json').write_text(json.dumps({'status':'UNAPPROVED_NATIVE_MATERIAL_STUDY','foreign_uv_transfer':False,'sampled_diffuse_colours':args.sample_diffuse,'colour_sampling_vertices':len(sampled_colors or []),'native_uv_layers':[u.name for u in body.data.uv_layers],'script_sha256':hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),'limitations':['Dense colour-sampled mesh is a Blender study; source multires is preserved in the preceding checkpoint.','Hair and garment realism require rendered reference comparison.','Blender shader complexity has not been evaluated for WebGL.']},indent=2))
print('STUDIO_NATIVE_LOOKDEV_COMPLETE',flush=True)
