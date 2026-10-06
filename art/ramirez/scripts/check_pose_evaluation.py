"""Discriminate pose-authoring state from evaluated/rendered armature state."""
import bpy,sys,json
from pathlib import Path
from mathutils import Vector
sys.path.insert(0,str(Path(__file__).parent));from ramirez_pose import apply_pose
art=Path(__file__).resolve().parents[1];out=art/'reviews/studio19-pose-audit';out.mkdir(parents=True,exist_ok=True)
rig=bpy.data.objects['RAMIREZ_RIG'];scene=bpy.context.scene
def snapshot():
    dg=bpy.context.evaluated_depsgraph_get();ob=bpy.data.objects['Glove padded shell.L'].evaluated_get(dg)
    center=sum((ob.matrix_world @ v.co for v in ob.data.vertices),Vector())/len(ob.data.vertices)
    return {'wrist':list(rig.pose.bones['hand.L'].head),'evaluated_glove_center':list(center)}
records=[]
for frame,pose in [(1,'guard'),(2,'jab')]:
    scene.frame_set(frame);apply_pose(rig,pose);rig.update_tag(refresh={'OBJECT'});bpy.context.view_layer.update()
    records.append({'pose':pose,**snapshot()})
scene.render.resolution_x=650;scene.render.resolution_y=650;scene.cycles.samples=16
prefs=bpy.context.preferences.addons['cycles'].preferences;prefs.compute_device_type='ONEAPI';prefs.get_devices()
for d in prefs.devices:d.use=d.type=='ONEAPI'
scene.cycles.device='GPU';scene.render.filepath=str(out/'jab.png');bpy.ops.render.render(write_still=True)
records.append({'pose':'jab_after_render',**snapshot()})
(out/'pose-evaluation.json').write_text(json.dumps(records,indent=2));print(json.dumps(records),flush=True)
