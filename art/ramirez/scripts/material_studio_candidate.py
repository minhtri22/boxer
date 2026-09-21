"""Material/UV experiment on candidate B, retaining the intact sculpt."""
import bpy,sys,math,json
from pathlib import Path
from mathutils import Vector,Matrix
from mathutils.bvhtree import BVHTree
from mathutils.geometry import barycentric_transform

root=Path(__file__).resolve().parents[3];art=root/'art/ramirez';out=art/'reviews/studio14-material';out.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(Path(__file__).parent));from ramirez_pose import apply_pose
rig=bpy.data.objects['RAMIREZ_RIG'];body=bpy.data.objects['Studio Ramirez continuous sculpt'];scene=bpy.context.scene
for p in rig.pose.bones:p.matrix_basis=Matrix.Identity(4)
bpy.context.view_layer.update()
with bpy.data.libraries.load(str(art/'reviews/detail07/ramirez.blend'),link=False) as (a,b):
    b.objects=['Ramirez continuous anatomical skin','Short individual curls','Individual beard stubble','Individual eyebrows']
donor=b.objects[0];donor.data.calc_loop_triangles();triangles=list(donor.data.loop_triangles)
tree=BVHTree.FromPolygons([p.co for p in donor.data.vertices],[t.vertices for t in triangles])
uvsrc=donor.data.uv_layers[0];uv=body.data.uv_layers.new(name='TransferredSkinUV')
positions=[body.matrix_world @ p.co for p in body.data.vertices]
maxdistance=0
for poly in body.data.polygons:
    for loop in poly.loop_indices:
        idx=body.data.loops[loop].vertex_index;p=positions[idx].copy()
        if p.z>1.55:p.z+=.009*max(0,min(1,(1.79-p.z)/.06))
        hit,n,index,dist=tree.find_nearest(p);maxdistance=max(maxdistance,dist)
        tri=triangles[index];verts=[donor.data.vertices[i].co for i in tri.vertices]
        coords=[Vector((*uvsrc.data[i].uv,0)) for i in tri.loops]
        transformed=barycentric_transform(hit,*verts,*coords);uv.data[loop].uv=transformed.xy
body.data.uv_layers.active=uv
skin=donor.data.materials[0].copy();skin.name='Studio sculpt skin - transferred atlas study';body.data.materials.clear();body.data.materials.append(skin)
nodes=skin.node_tree.nodes;links=skin.node_tree.links
uvnode=nodes.new('ShaderNodeUVMap');uvnode.uv_map=uv.name
for n in nodes:
    if n.type=='TEX_IMAGE':links.new(uvnode.outputs['UV'],n.inputs['Vector'])
rest=body.data.attributes.get('rest_position') or body.data.attributes.new('rest_position','FLOAT_VECTOR','POINT')
for p,d in zip(positions,rest.data):d.vector=p
scalp=body.data.attributes.get('scalp_density') or body.data.attributes.new('scalp_density','FLOAT','POINT')
for p,d in zip(positions,scalp.data):
    front=max(0,min(1,(-p.y-.02)/.09));side=min(1,(abs(p.x)/.065)**2)
    threshold=max(1.686,1.697+.038*front-.021*side)
    d.value=max(0,min(1,(p.z-threshold)/.008))
# The donor material has no scalp field yet; add a smooth scalp mix explicitly.
principled=nodes.get('Principled BSDF');source=principled.inputs['Base Color'].links[0].from_socket
attribute=nodes.new('ShaderNodeAttribute');attribute.attribute_name='scalp_density'
mix=nodes.new('ShaderNodeMixRGB');mix.inputs[2].default_value=(.01,.006,.003,1)
links.new(attribute.outputs['Fac'],mix.inputs[0]);links.new(source,mix.inputs[1]);links.new(mix.outputs[0],principled.inputs['Base Color'])
principled.inputs['Roughness'].default_value=.5

target=BVHTree.FromObject(body,bpy.context.evaluated_depsgraph_get())
# Target BVH is in object coordinates; retain B's unapplied uniform source scale.
inv=body.matrix_world.inverted()
for hair in b.objects[1:]:
    scene.collection.objects.link(hair)
    for p in hair.data.vertices:
        a,n,index,d=tree.find_nearest(p.co)
        query=a.copy()
        if query.z>1.55:query.z-=.009*max(0,min(1,(1.79-query.z)/.06))
        hit,normal,_,_=target.find_nearest(inv @ query)
        p.co=(body.matrix_world @ hit)+(p.co-a)
    for m in list(hair.modifiers):hair.modifiers.remove(m)
    hair.vertex_groups.clear();g=hair.vertex_groups.new(name='head');g.add(list(range(len(hair.data.vertices))),1,'REPLACE');m=hair.modifiers.new('Head attachment','ARMATURE');m.object=rig

# Reuse authored equipment materials only; do not import or alter the old body.
with bpy.data.libraries.load(str(art/'reviews/detail07/ramirez.blend'),link=False) as (a,c):
    c.materials=['Oxblood satin','Woven gold','Ivory cotton','Red glove leather','Waist label black']
mats={m.name.split('.')[0]:m for m in c.materials if m}
for ob in scene.objects:
    if ob.type!='MESH' or ob==body or ob in b.objects[1:]:continue
    name=ob.name
    mat=None
    if name.startswith('Ramirez shorts'):mat=mats['Oxblood satin']
    elif name.startswith(('Glove padded','Glove attached','Glove panel','Red rubber','Boot lace')):mat=mats['Red glove leather']
    elif name.startswith(('Glove gold','Gold elastic','Continuous woven','Sewn gold','Waistband','RAMIREZ name')):mat=mats['Woven gold']
    elif name.startswith('Ramirez waistband'):mat=mats['Waist label black']
    elif name.startswith(('Hand wrap','Wrap overlaps','White leather','Boot cotton')):mat=mats['Ivory cotton']
    if mat:
        ob.data.materials.clear();ob.data.materials.append(mat)

for side,sign in [('L',1),('R',-1)]:
    eye=bpy.data.objects['Sculpt eye.'+side]
    # Correct fitted sclera centre from source eye landmarks, not the head box.
    old=Vector((sign*.035,-.135,1.683));center=Vector((sign*.035,-.130,1.676))
    for p in eye.data.vertices:p.co+=center-old
    sclera=bpy.data.materials.new('Warm sclera B.'+side);sclera.use_nodes=True
    bsdf=sclera.node_tree.nodes.get('Principled BSDF');bsdf.inputs['Base Color'].default_value=(.52,.49,.45,1);bsdf.inputs['Roughness'].default_value=.2
    eye.data.materials.clear();eye.data.materials.append(sclera)
    iris=bpy.data.materials.new('Brown iris B.'+side);iris.use_nodes=True;iris.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.045,.018,.007,1)
    pupil=bpy.data.materials.new('Pupil B.'+side);pupil.diffuse_color=(.001,.001,.001,1)
    pts=[];fs=[]
    for r in [.0001,.0025,.0029,.0057,.006]:
        for j in range(64):
            a=j/64*2*math.pi;pts.append(center+Vector((r*math.cos(a),-math.sqrt(.013**2-r*r)-.00025,r*math.sin(a))))
    for row in range(4):
        for j in range(64):fs.append((row*64+j,row*64+(j+1)%64,(row+1)*64+(j+1)%64,(row+1)*64+j))
    data=bpy.data.meshes.new('Iris B.'+side);data.from_pydata(pts,[],fs);ob=bpy.data.objects.new(data.name,data);scene.collection.objects.link(ob);data.materials.append(iris);data.materials.append(pupil)
    for p in data.polygons:p.material_index=1 if p.index<64 or p.index>=192 else 0;p.use_smooth=True
    g=ob.vertex_groups.new(name='head');g.add(list(range(len(pts))),1,'REPLACE');m=ob.modifiers.new('Head attachment','ARMATURE');m.object=rig

prefs=bpy.context.preferences.addons['cycles'].preferences;prefs.compute_device_type='ONEAPI';prefs.get_devices()
for d in prefs.devices:d.use=d.type=='ONEAPI'
scene.cycles.device='GPU';scene.cycles.samples=36
scene.render.resolution_x=1100;scene.render.resolution_y=1100;scene.render.resolution_percentage=100
apply_pose(rig,'guard')
bpy.ops.wm.save_as_mainfile(filepath=str(out/'studio-ramirez-material.blend'),compress=True)
(out/'material-report.json').write_text(json.dumps({'status':'UNAPPROVED_UV_TRANSFER_EXPERIMENT','uv_layer':uv.name,'original_uv_preserved':True,'max_nearest_surface_distance_m':maxdistance,'limitations':['UV transfer requires face/nipple/neck seam review.','Hair placement is projected and needs silhouette review.']},indent=2))
for name,pos,look,size in [('threequarter',(3,-5,1.2),(0,0,.88),2.02),('portrait',(.30,-3,1.65),(0,-.02,1.56),.57)]:
    scene.camera.location=pos;scene.camera.rotation_euler=(Vector(look)-scene.camera.location).to_track_quat('-Z','Y').to_euler();scene.camera.data.ortho_scale=size;scene.render.filepath=str(out/(name+'.png'));bpy.ops.render.render(write_still=True)
print('STUDIO_MATERIAL_REVIEW_COMPLETE',flush=True)
