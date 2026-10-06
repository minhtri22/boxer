"""Preserve verified geometry changes in one editable, explicitly unfinished asset."""
import bpy,bmesh,sys,json,math,hashlib,time
from pathlib import Path
from mathutils import Matrix,Vector
from mathutils.bvhtree import BVHTree
art=Path(__file__).resolve().parents[1];out=art/'working';out.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(Path(__file__).parent));from ramirez_pose import apply_pose
scene=bpy.context.scene;rig=bpy.data.objects['RAMIREZ_RIG'];body=bpy.data.objects['Studio Ramirez continuous sculpt']
for p in rig.pose.bones:p.matrix_basis=Matrix.Identity(4)
rig.update_tag(refresh={'OBJECT'});bpy.context.view_layer.update()
with bpy.data.libraries.load(str(art/'reviews/studio23-wrap-surface/studio-ramirez-rig.blend'),link=False) as (a,b):
    b.objects=[n for n in a.objects if n.startswith(('Hand wrap','Wrap overlaps'))]
for ob in b.objects:
    if not ob:continue
    # The appended object has Blender's numeric suffix, so use its semantic stem.
    side='L' if '.L' in ob.name else 'R';prefix='Hand wrap' if ob.name.startswith('Hand wrap') else 'Wrap overlaps';name=prefix+'.'+side
    old=bpy.data.objects.get(name)
    if old:bpy.data.objects.remove(old,do_unlink=True)
    scene.collection.objects.link(ob);ob.name=name
    for m in ob.modifiers:
        if m.type=='ARMATURE':m.object=rig
for side in ['L','R']:
    shell=bpy.data.objects['Glove padded shell.'+side]
    bm=bmesh.new();bm.from_mesh(shell.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(shell.data);bm.free()
    bpy.context.view_layer.update();surface=BVHTree.FromObject(shell,bpy.context.evaluated_depsgraph_get())
    center=sum((v.co for v in shell.data.vertices),Vector())/len(shell.data.vertices)
    for prefix,offset in [('Glove panel seam.',.0005),('Glove gold crown.',.0008)]:
        ob=bpy.data.objects.get(prefix+side)
        if ob:
            for v in ob.data.vertices:
                hit,n,_,_=surface.find_nearest(v.co)
                if n.dot(hit-center)<0:n=-n
                v.co=hit+n*offset
before_lengths={b.name:b.length for b in rig.data.bones}
report={'status':'NOT_APPROVED_BLENDER_WORKBENCH','render_style':'Neutral clay for geometry review; not final appearance','body_source':'studio21-reference-forms','wrap_source':'studio23-wrap-surface','excluded':['studio22 invalid skin correspondence','studio24 failed cloth drape'],'imagegen_attempt':'Built-in imagegen failed with HTTP 404; no generated texture exists. No API fallback used.','pose_checks':[]}
names=['guard','jab','cross','hook','uppercut','overhand','slip','roll','advance','retreat']
for frame,name in enumerate(names,1):
    scene.frame_set(frame);apply_pose(rig,name);dg=bpy.context.evaluated_depsgraph_get()
    eb=body.evaluated_get(dg);points=[eb.matrix_world @ v.co for v in eb.data.vertices]
    record={'pose':name,'finite_body':all(math.isfinite(c) for p in points for c in p),'max_bone_length_error_m':max(abs((b.tail-b.head).length-before_lengths[b.name]) for b in rig.pose.bones),'gloves':{},'soles':{}}
    for side in ['L','R']:
        glove=bpy.data.objects['Glove padded shell.'+side].evaluated_get(dg)
        center=sum((glove.matrix_world @ v.co for v in glove.data.vertices),Vector())/len(glove.data.vertices);record['gloves'][side]=list(center)
        sole=bpy.data.objects['Red rubber outsole.'+side].evaluated_get(dg)
        record['soles'][side]=min((sole.matrix_world @ v.co).z for v in sole.data.vertices)
    report['pose_checks'].append(record)
    print('WORKBENCH_POSE',name,flush=True)
report['jab_guard_glove_delta_m']=(Vector(report['pose_checks'][1]['gloves']['L'])-Vector(report['pose_checks'][0]['gloves']['L'])).length
assert report['jab_guard_glove_delta_m']>.2
assert all(r['finite_body'] and r['max_bone_length_error_m']<1e-5 for r in report['pose_checks'])
scene.frame_set(1);apply_pose(rig,'guard');scene['review_status']='NOT_APPROVED_REFERENCE_FIDELITY_UNRESOLVED'
notes=bpy.data.texts.get('RAMIREZ WORKBENCH STATUS') or bpy.data.texts.new('RAMIREZ WORKBENCH STATUS')
notes.clear();notes.write('UNFINISHED BLENDER CHARACTER. NOT A UAT CANDIDATE.\nNative sculpt and packed user reference boards retained.\nFace, skin, hair, glove silhouette and garment detail do not yet meet reference quality.\nNo Unity integration or Human UAT PASS.\nPose checks validate numerical stability only, not boxing motion quality.\n')
scene.camera.location=(3,-5,1.2);scene.camera.rotation_euler=(Vector((0,0,.88))-scene.camera.location).to_track_quat('-Z','Y').to_euler();scene.camera.data.ortho_scale=2.02
scene.render.resolution_x=900;scene.render.resolution_y=1050;scene.cycles.samples=28
prefs=bpy.context.preferences.addons['cycles'].preferences;prefs.compute_device_type='ONEAPI';prefs.get_devices()
for d in prefs.devices:d.use=d.type=='ONEAPI'
scene.cycles.device='GPU';path=out/'ramirez-reference-workbench.blend'
bpy.ops.wm.save_as_mainfile(filepath=str(path),compress=True)
report['blend_sha256']=hashlib.sha256(path.read_bytes()).hexdigest()
for frame,name in [(1,'guard'),(2,'jab')]:
    scene.frame_set(frame);apply_pose(rig,name);scene.render.filepath=str(out/(name+'.png'));start=time.perf_counter();bpy.ops.render.render(write_still=True)
    report.setdefault('render_seconds',{})[name]=time.perf_counter()-start
(out/'workbench-report.json').write_text(json.dumps(report,indent=2))
print('WORKBENCH_COMPLETE_NOT_APPROVED',flush=True)
