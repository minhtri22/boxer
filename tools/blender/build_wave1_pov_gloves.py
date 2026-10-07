"""Export player gloves only. Never rebuild/overwrite the accepted Ramirez."""
import importlib.util
import json
import math
import hashlib
from pathlib import Path

import bpy

repo = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("boxer_asset_helpers", repo / "tools/blender/build_boxer_assets.py")
helpers = importlib.util.module_from_spec(spec)
spec.loader.exec_module(helpers)
out = repo / "art/blender/wave1-pov"
out.mkdir(parents=True, exist_ok=True)
unity = repo / "unity/BoxerP0/Assets/Resources/Boxer3D"
report = {"scope": "PLAYER_PRESENTATION_ONLY_NO_COLLIDER_CHANGE", "blender": bpy.app.version_string, "exports": []}

def seam(name, points, mat, arm, radius=.0008):
    curve = bpy.data.curves.new(name, 'CURVE')
    curve.dimensions = '3D'; curve.bevel_depth = radius; curve.bevel_resolution = 2
    spline = curve.splines.new('POLY'); spline.points.add(len(points)-1)
    for point, xyz in zip(spline.points, points): point.co = (*xyz, 1)
    obj = bpy.data.objects.new(name, curve); bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat)
    bpy.context.view_layer.objects.active = obj; obj.select_set(True)
    bpy.ops.object.convert(target='MESH'); obj = bpy.context.object; obj.select_set(False)
    helpers.bind_rigid(obj, arm, 'hand')

for side, mirror in [('Left', 1), ('Right', -1)]:
    helpers.clear_scene()
    arm = helpers.build_pov_glove()
    for name in ['POVGlovePalm', 'POVGloveKnuckles', 'POVGoldBand', 'POVBadge']:
        bpy.data.objects.remove(bpy.data.objects[name], do_unlink=True)
    leather = helpers.material('POVBlackLeather', (.023,.019,.015), .015, .48)
    thread = helpers.material('POVStitch', (.14,.11,.075), 0, .65)
    gold = helpers.material('POVGold', (.55,.31,.08), .42, .42)
    # One continuous pad/palm surface, rounded knuckle front and narrowed wrist.
    rings = [(-.045,.070,.065,-.002),(-.025,.082,.073,-.008),(.015,.096,.081,-.012),
             (.055,.105,.085,-.016),(.095,.098,.079,-.016),(.123,.075,.059,-.016),(.14,.025,.019,-.016)]
    count=64; verts=[]; faces=[]
    for z, rx, ry, cy in rings:
        for j in range(count):
            a=2*math.pi*j/count
            verts.append((rx*math.cos(a),cy+ry*math.sin(a),z))
    for i in range(len(rings)-1):
        for j in range(count): faces.append((i*count+j,i*count+(j+1)%count,(i+1)*count+(j+1)%count,(i+1)*count+j))
    faces += [tuple(reversed(range(count))), tuple((len(rings)-1)*count+j for j in range(count))]
    mesh=bpy.data.meshes.new('POVContinuousPad'); mesh.from_pydata(verts,[],faces); mesh.update()
    obj=bpy.data.objects.new('POVGloveContinuousPad',mesh); bpy.context.collection.objects.link(obj); obj.data.materials.append(leather)
    helpers.smooth(obj)
    mod=obj.modifiers.new('Smooth pad transitions','SUBSURF'); mod.levels=2
    bpy.context.view_layer.objects.active=obj; obj.select_set(True); bpy.ops.object.modifier_apply(modifier=mod.name); obj.select_set(False)
    # Generate stable UVs before rigid bind. No raster fighter overlay.
    bpy.context.view_layer.objects.active=obj; obj.select_set(True); bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT'); bpy.ops.uv.smart_project(island_margin=.02)
    bpy.ops.object.mode_set(mode='OBJECT'); obj.select_set(False)
    helpers.bind_rigid(obj,arm,'hand')
    helpers.ellipsoid('POVGloveThumb',(.104,-.050,-.006),(.035,.046,.064),leather,arm,'hand',32,24)
    badge_mesh=bpy.data.meshes.new('POVFlatCrown')
    badge_mesh.from_pydata([(-.025,-.018,-.046),(-.025,.009,-.046),(-.010,-.001,-.046),
        (0,.018,-.046),(.010,-.001,-.046),(.025,.009,-.046),(.025,-.018,-.046)],[],[(0,1,2,3,4,5,6)])
    badge=bpy.data.objects.new('POVCrownEmboss',badge_mesh);bpy.context.collection.objects.link(badge);badge.data.materials.append(gold)
    helpers.bind_rigid(badge,arm,'hand')
    # Curved piping and stitching, not the old projecting gold box.
    seam('POVCuffGoldPiping',[(.081*math.cos(a),.081*math.sin(a),-.072) for a in [j*2*math.pi/64 for j in range(65)]],gold,arm,.0018)
    seam('POVGlovePalmSeam',[(-.080,-.050,-.006),(-.094,-.055,.028),(-.096,-.055,.060),(-.086,-.051,.095),(-.064,-.044,.122)],thread,arm)
    seam('POVGloveThumbSeam',[(.084,-.084,-.046),(.120,-.084,-.025),(.136,-.071,.002),(.120,-.069,.031)],thread,arm)
    for j in range(10):
        z=.01+j*.009
        seam('POVStitch_%02d'%j,[(-.092,-.054,z),(-.087,-.058,z+.001)],thread,arm,.00045)
    # Mirror geometry, not combat anchors or the bone chain.
    for obj in list(bpy.context.scene.objects):
        if obj.type=='MESH':
            for v in obj.data.vertices: v.co.x *= mirror
            if mirror<0:
                for p in obj.data.polygons: p.flip()
            obj.data.update()
    bpy.ops.wm.save_as_mainfile(filepath=str(out / ('PlayerPOVGlove_'+side+'_W1.blend')))
    target=unity / ('PlayerPOVGlove_'+side+'_W1.fbx')
    helpers.export_fbx(target,list(bpy.context.scene.objects))
    report['exports'].append({'file':str(target.relative_to(repo)), 'sha256':hashlib.sha256(target.read_bytes()).hexdigest(),
        'vertices':sum(len(o.data.vertices) for o in bpy.context.scene.objects if o.type=='MESH'),
        'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in bpy.context.scene.objects if o.type=='MESH')})
(out/'export-report.json').write_text(json.dumps(report,indent=2),encoding='utf8',newline='\n')
print(json.dumps(report,indent=2))
