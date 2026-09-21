"""Controlled neutral-light inspection of Blender's CC0 realistic male sculpt."""
from pathlib import Path
import bpy,json
from mathutils import Vector,Matrix

root=Path(__file__).resolve().parents[3];art=root/'art/ramirez';out=art/'reviews/sculpt-candidate09';out.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
source=art/'source/blender-base-1.4.1/human_base_meshes_bundle.blend'
with bpy.data.libraries.load(str(source),link=False) as (a,b):b.objects=['GEO-body_male_realistic','GEO-body_male_realistic.eye.L','GEO-body_male_realistic.eye.R'];b.texts=['README']
body=b.objects[0]
offset=body.location.copy()
for ob in b.objects:
    if ob:
        bpy.context.scene.collection.objects.link(ob);ob.hide_render=False;ob.hide_viewport=False;ob.hide_set(False);ob.location-=offset
body=bpy.data.objects['GEO-body_male_realistic']
for m in body.modifiers:
    if m.type=='MULTIRES':m.levels=2;m.render_levels=2
bpy.context.view_layer.update()
height=body.dimensions.z;factor=1.8/height
for ob in b.objects:ob.location*=factor;ob.scale*=factor

def mat(name,color,rough):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=rough;return m
clay=mat('Neutral comparison clay',(.32,.33,.34),.62)
for ob in b.objects:ob.data.materials.clear();ob.data.materials.append(clay)
with bpy.data.libraries.load(str(art/'reviews/detail07/ramirez.blend'),link=False) as (a,c):c.objects=['Ramirez shorts - two leg pattern']
shorts=c.objects[0];bpy.context.scene.collection.objects.link(shorts)
for m in list(shorts.modifiers):
    if m.type=='ARMATURE':shorts.modifiers.remove(m)
shorts.data.materials.clear();shorts.data.materials.append(clay)

scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True
prefs=bpy.context.preferences.addons['cycles'].preferences;prefs.compute_device_type='ONEAPI';prefs.get_devices()
for d in prefs.devices:d.use=d.type=='ONEAPI'
scene.cycles.device='GPU';scene.render.resolution_x=1000;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.world.color=(.12,.12,.12)
def aim(ob,target):ob.rotation_euler=(Vector(target)-ob.location).to_track_quat('-Z','Y').to_euler()
for name,loc,power,size in [('Key',(-2.2,-3,3.5),450,2.5),('Fill',(2,-2,2),190,2),('Rim',(1,2,3),500,2)]:
    data=bpy.data.lights.new(name,'AREA');data.energy=power;data.shape='DISK';data.size=size;ob=bpy.data.objects.new(name,data);scene.collection.objects.link(ob);ob.location=loc;aim(ob,(0,0,1))
bpy.ops.mesh.primitive_plane_add(size=200);bpy.context.object.data.materials.append(mat('Floor',(.055,.06,.065),.8))
data=bpy.data.cameras.new('Camera');cam=bpy.data.objects.new('Camera',data);scene.collection.objects.link(cam);scene.camera=cam;data.type='ORTHO'
out.joinpath('source-readme.txt').write_text(bpy.data.texts.get('README').as_string())
report={'candidate':'B_STUDIO_SCULPT','source_object':'GEO-body_male_realistic','uniform_scale':factor,'cage_vertices':len(body.data.vertices),'multires_render_level':2,'status':'UNASSESSED_NOT_APPROVED','modifiers':[(m.name,m.type) for m in body.modifiers]}
out.joinpath('candidate.json').write_text(json.dumps(report,indent=2))
bpy.ops.wm.save_as_mainfile(filepath=str(out/'anatomy-comparison.blend'),compress=True)
for name,pos,target,size in [('front',(0,-5,1),(0,0,.9),2.04),('threequarter',(3,-5,1.2),(0,0,.9),2.04),('torso',(1,-4,1.55),(0,0,1.38),.90)]:
    cam.location=pos;aim(cam,target);data.ortho_scale=size;scene.render.filepath=str(out/(name+'.png'));bpy.ops.render.render(write_still=True)
print('SCULPT_CANDIDATE_COMPLETE',flush=True)
