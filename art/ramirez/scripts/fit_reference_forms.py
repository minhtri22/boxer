"""Local sculpt/equipment fit against the user's 27 September multi-view set.

Retains armature lengths, native multires sculpt, and uniform human scale.
Open studio20-wrap-follow/studio-ramirez-rig.blend before running.
"""
import bpy, math, json, sys, argparse
from pathlib import Path
from mathutils import Vector, Matrix

art=Path(__file__).resolve().parents[1];out=art/'reviews/studio21-reference-forms';out.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(Path(__file__).parent));from ramirez_pose import apply_pose
rig=bpy.data.objects['RAMIREZ_RIG'];body=bpy.data.objects['Studio Ramirez continuous sculpt'];scene=bpy.context.scene
for p in rig.pose.bones:p.matrix_basis=Matrix.Identity(4)
rig.update_tag(refresh={'OBJECT'});bpy.context.view_layer.update()
before_lengths={b.name:b.length for b in rig.data.bones}
inv=body.matrix_world.inverted();displacements=[]

def g(v,c,r):return math.exp(-sum(((v[i]-c[i])/r[i])**2 for i in range(3)))
def smooth(a,b,v):
    t=max(0,min(1,(v-a)/(b-a)));return t*t*(3-2*t)

for v in body.data.vertices:
    p=body.matrix_world @ v.co;x,y,z=p;s=1 if x>=0 else -1;a=Vector((abs(x),y,z));d=Vector()
    # Volume additions follow existing anatomical forms; no whole-axis scaling.
    for center,radii,amount in [((.215,-.008,1.445),(.075,.085,.080),.015),
                                ((.270,-.015,1.325),(.055,.072,.100),.010)]:
        c=Vector(center);direction=a-c
        if direction.length>.005:d+=Vector((s*direction.x,direction.y,direction.z)).normalized()*g(a,center,radii)*amount
    d.y-=.010*g(a,(.104,-.105,1.393),(.092,.07,.078))
    d.x+=s*.010*g(a,(.137,.062,1.345),(.066,.08,.120))
    d.y+=.009*g(a,(.095,.060,1.433),(.085,.07,.078))
    d.y-=.004*g(a,(.131,-.065,.735),(.070,.055,.17))
    # Brow ridge and mandibular plane, confined to the facial surface.
    front=1-smooth(-.10,-.045,y)
    d.y-=.0045*g(a,(.034,-.12,1.694),(.027,.06,.010))*front
    d.z-=.0015*g(a,(.028,-.13,1.692),(.022,.05,.009))*front
    d.x+=s*.003*g(a,(.053,-.079,1.602),(.024,.075,.028))
    v.co=inv @ (p+d);displacements.append(d.length)

# Raise the hem by 35 mm, preserving waist height and joined crotch topology.
for ob in scene.objects:
    if ob.type!='MESH':continue
    if ob.name.startswith(('Ramirez shorts','Continuous woven','Sewn gold')):
        for p in ob.data.vertices:
            z=p.co.z;p.co.z+=.035*(1-smooth(.64,1.035,z))
    if ob.name.startswith(('White leather','Boot cotton','Boot lace')):
        for p in ob.data.vertices:
            # Shorter shafts expose the calf as in reference 21/25. Foot last,
            # outsole, ankle pivot and planted-contact height remain untouched.
            if p.co.z>.125:p.co.z=.125+(p.co.z-.125)*.66

# A glove's padded fist should have a broad knuckle dome and a curled palm,
# rather than a uniformly oval balloon. Work in each hand's anatomical frame.
for side in ['L','R']:
    shell=bpy.data.objects['Glove padded shell.'+side]
    frame=rig.data.bones['hand.'+side].matrix_local;local=frame.inverted()
    for v in shell.data.vertices:
        p=local @ v.co
        width=1+.10*math.exp(-((p.y-.10)/.052)**2)
        p.x*=width
        if p.z<0:p.z*=.86
        v.co=frame @ p

# Reference boards are separate, non-rendering image empties, never overlays on
# the fighter. Images are packed unchanged in the editable Blender workbench.
refs=bpy.data.collections.new('REFERENCE - user supplied 20260927');scene.collection.children.link(refs)
for index,(name,file) in enumerate([('Front textured','21.png'),('Side textured','25.png'),('Back textured','23.png'),('Front wire','13.png'),('Side wire','12.png'),('Top textured','22.png')]):
    image=bpy.data.images.load(str(art/'reference/20260927'/file));image.pack()
    ob=bpy.data.objects.new('REFERENCE '+name,None);refs.objects.link(ob);ob.empty_display_type='IMAGE';ob.data=image;ob.empty_display_size=1.8
    ob.location=(-2.2-index%3*1.5,1.0,1.0-index//3*2.0);ob.rotation_euler=(math.pi/2,0,0);ob.hide_render=True;ob['source_png']=file
refs.hide_render=True

def ready_pose():
    rig['reference_chin_tuck_degrees']=9
    apply_pose(rig,'guard')
    rig.update_tag(refresh={'OBJECT'});bpy.context.view_layer.update()

ready_pose();scene['review_status']='NOT_APPROVED_REFERENCE_FORM_STUDY'
assert all(abs(b.length-before_lengths[b.name])<1e-8 for b in rig.data.bones)
report={'status':'UNAPPROVED_REFERENCE_FORM_STUDY','reference':'art/ramirez/reference/20260927/manifest.json','changed_cage_vertices':sum(d>1e-6 for d in displacements),'max_local_displacement_m':max(displacements),'armature_lengths_unchanged':True,'uniform_body_scale':list(body.scale),'changes':['Local shoulder, chest, back, biceps and brow/jaw volume; no body-axis stretch.','Shorter shorts and boot shafts.','Broader knuckle dome, less spherical glove palm.','Additional nine-degree world-space chin tuck.'],'limitations':['Viewport screenshots are not calibrated orthographic cameras.','Reference face, hair, surface material and cloth detail remain unfinished.','Clothing overlap and rig deformation require re-evaluation after local sculpt.']}
(out/'form-report.json').write_text(json.dumps(report,indent=2))
scene.render.resolution_x=900;scene.render.resolution_y=1050;scene.cycles.samples=28
prefs=bpy.context.preferences.addons['cycles'].preferences;prefs.compute_device_type='ONEAPI';prefs.get_devices()
for d in prefs.devices:d.use=d.type=='ONEAPI'
scene.cycles.device='GPU'
bpy.ops.wm.save_as_mainfile(filepath=str(out/'ramirez-reference-workbench.blend'),compress=True)
for name,pos,at in [('front',(0,-5,1.0),(0,0,.87)),('side',(5,0,1.0),(0,0,.87)),('back',(0,5,1.0),(0,0,.87))]:
    scene.camera.location=pos;scene.camera.rotation_euler=(Vector(at)-scene.camera.location).to_track_quat('-Z','Y').to_euler();scene.camera.data.ortho_scale=1.98
    scene.render.filepath=str(out/(name+'.png'));bpy.ops.render.render(write_still=True)
print('REFERENCE_FORMS_REVIEW_COMPLETE',flush=True)
