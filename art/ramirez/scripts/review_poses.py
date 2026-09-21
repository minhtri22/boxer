"""Reopen the saved .blend, inspect deformation poses, and retain honest evidence."""
import argparse
import hashlib
import json
import math
import sys
import time
from pathlib import Path

import bpy
from mathutils import Vector

sys.path.insert(0,str(Path(__file__).parent))
from ramirez_pose import apply_pose

parser=argparse.ArgumentParser()
parser.add_argument('--out',required=True)
parser.add_argument('--poses',default='guard,jab,cross,hook,uppercut,overhand,slip,roll,advance,retreat')
parser.add_argument('--size',type=int,default=850)
args=parser.parse_args(sys.argv[sys.argv.index('--')+1:])
out=Path(args.out).resolve();out.mkdir(parents=True,exist_ok=True)
scene=bpy.context.scene;rig=bpy.data.objects['RAMIREZ_RIG']
scene.render.resolution_x=args.size;scene.render.resolution_y=args.size
scene.cycles.samples=28
prefs=bpy.context.preferences.addons['cycles'].preferences
prefs.compute_device_type='ONEAPI';prefs.get_devices()
for device in prefs.devices:device.use=device.type=='ONEAPI'
scene.cycles.device='GPU'
cam=scene.camera;cam.location=(3,-5,1.2);target=Vector((0,-.08,.88));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=2.05
scene.render.fps=24
records=[]
for index,name in enumerate(args.poses.split(',')):
    frame=1+index*24
    scene.frame_set(frame);apply_pose(rig,name)
    for p in rig.pose.bones:
        p.keyframe_insert('location',frame=frame,group=p.name)
        p.keyframe_insert('rotation_euler' if p.rotation_mode!='QUATERNION' else 'rotation_quaternion',frame=frame,group=p.name)
        p.keyframe_insert('scale',frame=frame,group=p.name)
    scene.timeline_markers.new(name,frame=frame)
    dg=bpy.context.evaluated_depsgraph_get()
    body_source=bpy.data.objects.get('Studio Ramirez continuous sculpt') or bpy.data.objects.get('Ramirez continuous anatomical skin')
    body=body_source.evaluated_get(dg)
    coords=[body.matrix_world @ p.co for p in body.data.vertices]
    finite=all(math.isfinite(c) for p in coords for c in p)
    lengths={n:(rig.pose.bones[n].tail-rig.pose.bones[n].head).length for n in ['upperarm.L','upperarm.R','forearm.L','forearm.R','thigh.L','shin.L']}
    rest_error=max(abs(lengths[n]-rig.data.bones[n].length) for n in lengths)
    foot_bounds={}
    for side in ['L','R']:
        ob=bpy.data.objects['Red rubber outsole.'+side].evaluated_get(dg)
        zz=[(ob.matrix_world @ p.co).z for p in ob.data.vertices]
        foot_bounds[side]={'min_z_m':min(zz),'max_z_m':max(zz)}
    start=time.perf_counter();scene.render.filepath=str(out/(name+'.png'));bpy.ops.render.render(write_still=True)
    records.append({'pose':name,'frame':frame,'finite_skin_coordinates':finite,'bone_length_max_error_m':rest_error,'outsole_bounds':foot_bounds,'render_seconds':time.perf_counter()-start})
scene.frame_end=1+(len(records)-1)*24;scene.frame_set(1)
rig.animation_data.action.name='Anatomical pose studies - not gameplay timing'
scene['review_status']='NOT_APPROVED_REFERENCE_FIDELITY_UNRESOLVED'
bpy.ops.wm.save_as_mainfile(filepath=str(out/'ramirez-pose-study.blend'),compress=True)
report={'status':'POSE_STUDY_NOT_HUMAN_UAT','blender':bpy.app.version_string,'records':records,'limitations':['Pose samples and automated bounds are not a reference-fidelity pass.','Interpolation between pose studies is not a production boxing animation.','Art rig dimensions are not yet reconciled with frozen gameplay rig dimensions.']}
(out/'pose-review.json').write_text(json.dumps(report,indent=2))
print('POSE_REVIEW_COMPLETE',out,flush=True)
