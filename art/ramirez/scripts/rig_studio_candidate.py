"""Candidate B: rig the intact CC0 Blender sculpt without changing its anatomy."""
import bpy,json,math,sys,argparse
import numpy as np
from pathlib import Path
from mathutils import Vector,Matrix
from mathutils.kdtree import KDTree

parser=argparse.ArgumentParser()
parser.add_argument('--out',default='studio17-equipment')
parser.add_argument('--poses',default='guard,jab')
args=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
root=Path(__file__).resolve().parents[3];art=root/'art/ramirez';out=art/'reviews'/args.out;out.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(Path(__file__).parent));from ramirez_pose import apply_pose
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene
with bpy.data.libraries.load(str(art/'source/studio-male-cc0.blend'),link=False) as (a,b):b.objects=['GEO-body_male_realistic']
body=b.objects[0];scene.collection.objects.link(body);body.parent=None;body.matrix_world=Matrix.Identity(4);body.hide_render=False;body.hide_viewport=False;body.hide_set(False)
for m in body.modifiers:
    if m.type=='MULTIRES':m.levels=2;m.render_levels=2
bpy.context.view_layer.update();factor=1.8/body.dimensions.z;body.scale=(factor,)*3
bpy.context.view_layer.update()
rigdata=bpy.data.armatures.new('Sculpt aligned skeleton');rig=bpy.data.objects.new('RAMIREZ_RIG',rigdata);scene.collection.objects.link(rig)
bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT')
specs={}
def bone(name,h,t,parent=None):
    p=rigdata.edit_bones.new(name);p.head=h;p.tail=t;p.align_roll(Vector((0,-1,0)))
    if parent:p.parent=rigdata.edit_bones[parent]
    specs[name]={'head':list(h),'tail':list(t),'parent':parent}
bone('root',(0,0,.87),(0,0,1.01))
heights=[.94,1.03,1.12,1.19,1.31,1.49,1.535,1.572,1.615,1.78]
names=['spine05','spine04','spine03','spine02','spine01','neck01','neck02','neck03','head']
for i,n in enumerate(names):bone(n,(0,0 if i<5 else -.025,heights[i]),(0,0 if i<4 else -.025,heights[i+1]),names[i-1] if i else 'root')
for side,sign in [('L',1),('R',-1)]:
    bone('clavicle.'+side,(sign*.035,0,1.485),(sign*.20,0,1.47),'spine01')
    bone('upperarm.'+side,(sign*.20,0,1.47),(sign*.31,-.006,1.195),'clavicle.'+side)
    bone('forearm.'+side,(sign*.31,-.006,1.195),(sign*.389,-.054,.974),'upperarm.'+side)
    bone('hand.'+side,(sign*.389,-.054,.974),(sign*.415,-.105,.87),'forearm.'+side)
    bone('thigh.'+side,(sign*.105,0,.915),(sign*.144,.015,.497),'root')
    bone('shin.'+side,(sign*.144,.015,.497),(sign*.184,.052,.100),'thigh.'+side)
    bone('foot.'+side,(sign*.184,.052,.100),(sign*.184,-.110,.040),'shin.'+side)
bpy.ops.object.mode_set(mode='OBJECT')
bpy.ops.object.select_all(action='DESELECT');body.select_set(True);rig.select_set(True);bpy.context.view_layer.objects.active=rig
bpy.ops.object.parent_set(type='ARMATURE_AUTO')
arm=next(m for m in body.modifiers if m.type=='ARMATURE');arm.use_deform_preserve_volume=True
body.name='Studio Ramirez continuous sculpt'
unweighted=sum(not p.groups for p in body.data.vertices)

# Occluded skin is masked after multires, retaining the editable sculpt topology.
visible=body.vertex_groups.new(name='Visible sporting skin')
mask_report={'masked_forearm_vertices':0,'masked_vertices':0,'glove_skin_policy':'Glove is sole visible hand representation; distal skin omitted beyond wrist inside cuff.'}
for p in body.data.vertices:
    x,y,z=body.matrix_world @ p.co
    covered_by_shorts=.70<z<1.05 and abs(x)<.24
    covered_by_boot=z<.07
    side='L' if x>0 else 'R';hb=rigdata.bones['hand.'+side]
    hand_axis=(hb.tail_local-hb.head_local).normalized()
    covered_by_glove=abs(x)>.29 and (Vector((x,y,z))-hb.head_local).dot(hand_axis)>.005
    if not (covered_by_shorts or covered_by_boot or covered_by_glove):visible.add([p.index],1,'REPLACE')
    else:
        mask_report['masked_vertices']+=1
        if abs(x)>.27 and z>1.025:mask_report['masked_forearm_vertices']+=1
assert mask_report['masked_forearm_vertices']==0
mask=body.modifiers.new('Skin covered by equipment','MASK');mask.vertex_group=visible.name;mask.threshold=.5

def mat(name,col,rough=.62):
    m=bpy.data.materials.new(name);m.diffuse_color=(*col,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*col,1);p.inputs['Roughness'].default_value=rough;return m
clay=mat('Sculpt evaluation clay',(.32,.33,.34));body.data.materials.clear();body.data.materials.append(clay)
tree=KDTree(len(body.data.vertices))
for p in body.data.vertices:tree.insert(body.matrix_world @ p.co,p.index)
tree.balance()
weight_names={g.index:g.name for g in body.vertex_groups if g.name!='Visible sporting skin'}
weights=[{weight_names[g.group]:g.weight for g in p.groups if g.group in weight_names} for p in body.data.vertices]

# Fit the removable boot shaft to measured calf sections. Keep sole height
# independent of the changed anatomical ankle centre, and retain rigid soles.
rest_points=np.array([tuple(body.matrix_world @ p.co) for p in body.data.vertices])
boot_sections={}
for side,sign in [('L',1),('R',-1)]:
    sections=[]
    for z in [.12,.15,.20,.24,.285]:
        pts=rest_points[(rest_points[:,0]*sign>.055)&(abs(rest_points[:,2]-z)<.018)]
        lo=pts[:,:2].min(axis=0);hi=pts[:,:2].max(axis=0)
        sections.append([z,*((lo+hi)*.5),*((hi-lo)*.5+.006)])
    boot_sections[side]=np.array(sections)

with bpy.data.libraries.load(str(art/'reviews/detail07/ramirez.blend'),link=False) as (a,c):
    c.objects=[n for n in a.objects if n.startswith(('Ramirez shorts','Continuous woven','Sewn gold','Gold elastic','Waistband','Ramirez waistband','RAMIREZ name','Glove ','Hand wrap','Wrap overlaps','White leather','Red rubber','Boot cotton','Boot lace'))]
for ob in c.objects:
    if not ob or ob.type!='MESH':continue
    scene.collection.objects.link(ob)
    oldarm=next((m for m in ob.modifiers if m.type=='ARMATURE'),None)
    if oldarm is None:continue
    oldrig=oldarm.object
    rigid=None
    if ob.name.startswith(('Glove ','Hand wrap','Wrap overlaps')):
        side='L' if ob.name.endswith('.L') else 'R';rigid='hand.'+side
        transform=rigdata.bones[rigid].matrix_local @ oldrig.data.bones[rigid].matrix_local.inverted()
        for p in ob.data.vertices:p.co=transform @ p.co
    elif ob.name.startswith(('White leather','Red rubber','Boot cotton','Boot lace')):
        side='L' if ob.name.endswith('.L') else 'R'
        delta=rigdata.bones['foot.'+side].head_local-oldrig.data.bones['foot.'+side].head_local
        oldankle=oldrig.data.bones['foot.'+side].head_local
        sections=boot_sections[side]
        for p in ob.data.vertices:
            x,y,z=p.co
            blend=max(0,min(1,(z-.105)/.065));blend=blend*blend*(3-2*blend)
            oldrx=float(np.interp(z,[.123,.17,.27,.285],[.037,.037,.043,.044]))
            oldry=float(np.interp(z,[.123,.17,.27,.285],[.058,.044,.043,.044]))
            oldcy=oldankle.y+float(np.interp(z,[.123,.17],[ -.005,.002]))
            cx,cy,rx,ry=[float(np.interp(z,sections[:,0],sections[:,j])) for j in range(1,5)]
            fitted=Vector((cx+(x-oldankle.x)*rx/oldrx,cy+(y-oldcy)*ry/oldry,z))
            translated=Vector((x+delta.x,y+delta.y,z))
            p.co=translated.lerp(fitted,blend);p.co.z-=.007
    ob.modifiers.remove(oldarm);ob.vertex_groups.clear()
    groups={n:ob.vertex_groups.new(name=n) for n in rigdata.bones.keys()}
    for p in ob.data.vertices:
        if rigid:groups[rigid].add([p.index],1,'REPLACE');continue
        if ob.name.startswith(('White leather','Red rubber','Boot cotton','Boot lace')):
            # Foot is rigid below ankle; shaft follows the shin continuously.
            t=max(0,min(1,(p.co.z-.065)/.105));t=t*t*(3-2*t)
            groups['foot.'+side].add([p.index],1-t,'REPLACE')
            groups['shin.'+side].add([p.index],t,'REPLACE');continue
        ns=tree.find_n(p.co,3);mix={}
        for _,idx,distance in ns:
            for n,w in weights[idx].items():mix[n]=mix.get(n,0)+w/max(.002,distance)**2
        total=sum(mix.values())
        if total:
            for n,w in mix.items():groups[n].add([p.index],w/total,'REPLACE')
    m=ob.modifiers.new('Sculpt aligned deformation','ARMATURE');m.object=rig;m.use_deform_preserve_volume=True
    for i in range(len(ob.data.materials)):ob.data.materials[i]=clay

# The inherited hem was projected twice onto an open cloth boundary, selecting
# alternating side normals and producing sawtooth offsets. Rebuild it from the
# evaluated, undeformed cloth boundary with a consistent radial outward offset.
oldhem=bpy.data.objects.get('Continuous woven gold hems')
if oldhem:bpy.data.objects.remove(oldhem,do_unlink=True)
shorts=bpy.data.objects['Ramirez shorts - two leg pattern']
for m in shorts.modifiers:
    if m.type in {'ARMATURE','SOLIDIFY'}:m.show_viewport=False
bpy.context.view_layer.update()
evaluated=shorts.evaluated_get(bpy.context.evaluated_depsgraph_get());cloth=evaluated.to_mesh()
edges={}
for poly in cloth.polygons:
    ids=list(poly.vertices)
    for a,b in zip(ids,ids[1:]+ids[:1]):
        k=tuple(sorted((a,b)));edges[k]=edges.get(k,0)+1
hem_edges=[k for k,v in edges.items() if v==1 and all(cloth.vertices[i].co.z<.66 for i in k)]
ids=sorted({i for e in hem_edges for i in e});lookup={k:i for i,k in enumerate(ids)}
pts=[]
for dz in [0,.018]:
    for i in ids:
        p=cloth.vertices[i].co.copy();radial=Vector((p.x-math.copysign(.12,p.x),p.y,0)).normalized()
        p+=radial*.003;p.z+=dz;pts.append(p)
n=len(ids);faces=[(lookup[a],lookup[b],lookup[b]+n,lookup[a]+n) for a,b in hem_edges]
data=bpy.data.meshes.new('Continuous hem topology');data.from_pydata(pts,[],faces);data.update()
hem=bpy.data.objects.new('Continuous woven gold hems',data);scene.collection.objects.link(hem);data.materials.append(clay)
groups={n:hem.vertex_groups.new(name=n) for n in rigdata.bones.keys()}
for p in data.vertices:
    ns=tree.find_n(p.co,3);mix={}
    for _,idx,distance in ns:
        for name,w in weights[idx].items():mix[name]=mix.get(name,0)+w/max(.002,distance)**2
    total=sum(mix.values())
    for name,w in mix.items():groups[name].add([p.index],w/total,'REPLACE')
for p in data.polygons:p.use_smooth=True
m=hem.modifiers.new('Sculpt aligned deformation','ARMATURE');m.object=rig;m.use_deform_preserve_volume=True
m=hem.modifiers.new('Woven tape thickness','SOLIDIFY');m.thickness=.0008
evaluated.to_mesh_clear()
for m in shorts.modifiers:m.show_viewport=True

for side,sign in [('L',1),('R',-1)]:
    bpy.ops.mesh.primitive_uv_sphere_add(segments=32,ring_count=16,radius=.013,location=(sign*.035,-.135,1.683))
    eye=bpy.context.object;eye.name='Sculpt eye.'+side
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    eye.data.materials.append(clay);g=eye.vertex_groups.new(name='head');g.add(list(range(len(eye.data.vertices))),1,'REPLACE');m=eye.modifiers.new('Head deformation','ARMATURE');m.object=rig

scene.render.engine='CYCLES';scene.cycles.samples=28;scene.cycles.use_denoising=True
prefs=bpy.context.preferences.addons['cycles'].preferences;prefs.compute_device_type='ONEAPI';prefs.get_devices()
for d in prefs.devices:d.use=d.type=='ONEAPI'
scene.cycles.device='GPU';scene.render.resolution_x=1050;scene.render.resolution_y=1050;scene.render.resolution_percentage=100;scene.world.color=(.12,.12,.12)
def aim(ob,at):ob.rotation_euler=(Vector(at)-ob.location).to_track_quat('-Z','Y').to_euler()
for n,loc,power,size in [('Key',(-2.2,-3,3.5),450,2.5),('Fill',(2,-2,2),190,2),('Rim',(1,2,3),500,2)]:
    data=bpy.data.lights.new(n,'AREA');data.energy=power;data.size=size;data.shape='DISK';ob=bpy.data.objects.new(n,data);scene.collection.objects.link(ob);ob.location=loc;aim(ob,(0,0,1))
bpy.ops.mesh.primitive_plane_add(size=200);bpy.context.object.data.materials.append(mat('Floor',(.055,.06,.065),.8))
data=bpy.data.cameras.new('Camera');cam=bpy.data.objects.new('Camera',data);scene.collection.objects.link(cam);scene.camera=cam;data.type='ORTHO';data.ortho_scale=2.02;cam.location=(3,-5,1.2);aim(cam,(0,0,.88))
apply_pose(rig,'guard')
bpy.ops.wm.save_as_mainfile(filepath=str(out/'studio-ramirez-rig.blend'),compress=True)
report={'candidate':'B_INTACT_SCULPT_RIG','unweighted_base_vertices':unweighted,'bones':specs,'uniform_scale':factor,'mask':mask_report,'boot_sections_m':{k:v.tolist() for k,v in boot_sections.items()},'status':'NOT_APPROVED'}
(out/'rig-report.json').write_text(json.dumps(report,indent=2))
for pose in args.poses.split(','):
    apply_pose(rig,pose);scene.render.filepath=str(out/(pose+'.png'));bpy.ops.render.render(write_still=True)
print('STUDIO_RIG_REVIEW_COMPLETE',flush=True)
