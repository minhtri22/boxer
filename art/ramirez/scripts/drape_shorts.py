"""Offline rest-pose cloth study; no runtime cloth or gameplay changes.

Simulate only the garment, retain its vertex IDs/weights, then evaluate the
existing boxing pose. Gold trim is refitted to the solved garment surface.
"""
import bpy,sys,json,time,math
from pathlib import Path
from mathutils import Matrix,Vector
from mathutils.bvhtree import BVHTree
art=Path(__file__).resolve().parents[1];out=art/'reviews/studio24-cloth-drape';out.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(Path(__file__).parent));from ramirez_pose import apply_pose
scene=bpy.context.scene;rig=bpy.data.objects['RAMIREZ_RIG'];body=bpy.data.objects['Studio Ramirez continuous sculpt'];shorts=bpy.data.objects['Ramirez shorts - two leg pattern']
for p in rig.pose.bones:p.matrix_basis=Matrix.Identity(4)
rig.update_tag(refresh={'OBJECT'});bpy.context.view_layer.update()
mask=next(m for m in body.modifiers if m.type=='MASK');mask.show_viewport=False
evaluated=body.evaluated_get(bpy.context.evaluated_depsgraph_get())
data=bpy.data.meshes.new_from_object(evaluated);data.transform(body.matrix_world)
collider=bpy.data.objects.new('TEMP offline cloth body collision',data);scene.collection.objects.link(collider);collider.hide_render=True
collider.modifiers.new('Offline cloth collision','COLLISION');collider.collision.thickness_outer=.003;collider.collision.thickness_inner=.001
mask.show_viewport=True
surface=BVHTree.FromPolygons([v.co for v in data.vertices],[p.vertices[:] for p in data.polygons])
data=shorts.data.copy();sim=bpy.data.objects.new('TEMP offline cloth solver',data);scene.collection.objects.link(sim);sim.hide_render=True
before=[v.co.copy() for v in data.vertices];resolved=0
for v in data.vertices:
    hit,n,_,distance=surface.find_nearest(v.co)
    if (v.co-hit).dot(n)<.006 and distance<.06:
        v.co=hit+n*.006;resolved+=1
pin=sim.vertex_groups.new(name='Waist seam pins')
for v in data.vertices:
    t=max(0,min(1,(v.co.z-.99)/.035))
    if t:pin.add([v.index],t*t*(3-2*t),'REPLACE')
cloth=sim.modifiers.new('Offline satin drape','CLOTH');settings=cloth.settings
settings.quality=6;settings.mass=.085;settings.air_damping=2
settings.tension_stiffness=28;settings.compression_stiffness=28;settings.shear_stiffness=16;settings.bending_stiffness=.18
settings.vertex_group_mass=pin.name
cloth.collision_settings.use_collision=True;cloth.collision_settings.distance_min=.004
cloth.collision_settings.use_self_collision=True;cloth.collision_settings.self_distance_min=.003
cloth.point_cache.frame_start=1;cloth.point_cache.frame_end=36
scene.frame_start=1;scene.frame_end=36;scene.render.fps=24
start=time.perf_counter()
for frame in range(1,37):
    scene.frame_set(frame);bpy.context.view_layer.update()
    result=sim.evaluated_get(bpy.context.evaluated_depsgraph_get())
    if frame%6==0:print('DRAPE_FRAME',frame,'seconds',round(time.perf_counter()-start,2),flush=True)
result=sim.evaluated_get(bpy.context.evaluated_depsgraph_get());solved=result.to_mesh()
assert len(solved.vertices)==len(shorts.data.vertices)
after=[v.co.copy() for v in solved.vertices];result.to_mesh_clear()
finite=all(math.isfinite(c) for p in after for c in p)
maxmove=max((a-b).length for a,b in zip(after,before))
if not finite or maxmove>.15:raise RuntimeError('Offline drape unstable: '+str(maxmove))
for v,p in zip(shorts.data.vertices,after):v.co=p
seconds=time.perf_counter()-start
bpy.data.objects.remove(sim,do_unlink=True);bpy.data.objects.remove(collider,do_unlink=True)
scene.frame_set(1)
for m in shorts.modifiers:
    if m.type=='ARMATURE':m.show_viewport=False
bpy.context.view_layer.update();surface=BVHTree.FromObject(shorts,bpy.context.evaluated_depsgraph_get())
for ob in scene.objects:
    if ob.type=='MESH' and ob.name.startswith(('Continuous woven','Sewn gold')):
        for v in ob.data.vertices:
            hit,n,_,_=surface.find_nearest(v.co);v.co=hit+n*.0018
for m in shorts.modifiers:m.show_viewport=True
apply_pose(rig,'guard');scene['review_status']='UNAPPROVED_OFFLINE_CLOTH_STUDY'
(out/'cloth-report.json').write_text(json.dumps({'status':'UNAPPROVED_OFFLINE_CLOTH_STUDY','simulation_frames':36,'simulation_seconds':seconds,'vertex_count':len(after),'initial_clearance_vertices':resolved,'max_rest_displacement_m':maxmove,'finite':finite,'runtime_cloth_added':False,'limitations':['Satin appearance and intersections need rendered review.','Existing rig weights preserved; moving poses remain to be evaluated.']},indent=2))
prefs=bpy.context.preferences.addons['cycles'].preferences;prefs.compute_device_type='ONEAPI';prefs.get_devices()
for d in prefs.devices:d.use=d.type=='ONEAPI'
scene.cycles.device='GPU';scene.cycles.samples=24;scene.render.resolution_x=900;scene.render.resolution_y=1050
bpy.ops.wm.save_as_mainfile(filepath=str(out/'ramirez-cloth-study.blend'),compress=True)
for name,pos in [('front',(0,-5,1.0)),('side',(5,0,1.0))]:
    scene.camera.location=pos;scene.camera.rotation_euler=(Vector((0,0,.87))-scene.camera.location).to_track_quat('-Z','Y').to_euler();scene.camera.data.ortho_scale=1.98
    scene.render.filepath=str(out/(name+'.png'));bpy.ops.render.render(write_still=True)
print('OFFLINE_DRAPE_COMPLETE',flush=True)
