"""Export the approved workbench at rest; never save or replace its .blend."""
import bpy, hashlib, json, math, datetime
from pathlib import Path
from mathutils import Matrix, Vector

root = Path(__file__).resolve().parents[3]
source = root/'art/ramirez/working/ramirez-reference-workbench.blend'
expected = '419466859d7f119f98d7e69dc2673f0b378dbf5a892f0e2a6d7254cbc974af12'
assert hashlib.sha256(source.read_bytes()).hexdigest() == expected
assert bpy.app.version_string == '5.2.1 LTS'
rig = bpy.data.objects['RAMIREZ_RIG']
for p in rig.pose.bones: p.matrix_basis = Matrix.Identity(4)
rig.data.pose_position = 'REST'
bpy.context.view_layer.update()
prefixes = ('Studio Ramirez continuous sculpt', 'Sculpt eye.', 'Glove ', 'Hand wrap.',
            'Wrap overlaps.', 'Ramirez shorts', 'Gold elastic', 'Continuous woven',
            'Sewn gold', 'Waistband', 'Ramirez waistband', 'RAMIREZ name',
            'White leather', 'Red rubber', 'Boot cotton', 'Boot lace')
objects = sorted((ob for ob in bpy.context.scene.objects if ob.type=='MESH'
    and not ob.hide_render and ob.name.startswith(prefixes)
    and any(m.type=='ARMATURE' and m.object==rig for m in ob.modifiers)), key=lambda ob:ob.name)
assert len([ob for ob in objects if ob.name=='Studio Ramirez continuous sculpt']) == 1
assert len(objects)>20
palette = {'Skin':(.60,.36,.235,1),'Red':(.48,.025,.035,1),
           'Gold':(.78,.52,.16,1),'White':(.82,.79,.71,1),'Eye':(.11,.07,.035,1)}
materials = {}
for name,color in palette.items():
    m=bpy.data.materials.new('Ramirez Unity '+name); m.diffuse_color=color
    m.use_nodes=True; m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=color
    materials[name]=m
records=[]
for ob in objects:
    name=ob.name
    kind='Skin' if name.startswith('Studio') else 'Eye' if name.startswith('Sculpt eye') else 'White' if name.startswith(('White','Boot cotton','Hand wrap','Wrap overlaps')) else 'Gold' if ('gold' in name.lower() or 'embroidery' in name or 'label' in name) else 'Red'
    slots=max(1,len(ob.data.materials)); ob.data.materials.clear()
    for i in range(slots): ob.data.materials.append(materials[kind])
    ob.hide_viewport=False; ob.hide_set(False)
    ev=ob.evaluated_get(bpy.context.evaluated_depsgraph_get())
    mesh=ev.to_mesh(preserve_all_data_layers=True, depsgraph=bpy.context.evaluated_depsgraph_get())
    mesh.calc_loop_triangles()
    points=[ev.matrix_world @ v.co for v in mesh.vertices]
    assert all(math.isfinite(c) for p in points for c in p)
    unweighted=sum(not any(g.group<len(ob.vertex_groups) and ob.vertex_groups[g.group].name in rig.data.bones and g.weight>0 for g in v.groups) for v in mesh.vertices)
    assert unweighted==0, (name,unweighted)
    center=sum(points,Vector())/len(points)
    records.append(dict(name=name,vertices=len(mesh.vertices),triangles=len(mesh.loop_triangles),
        unweighted=unweighted,center_blender_m=list(center),
        bounds_blender_m=[[min(p[i] for p in points) for i in range(3)], [max(p[i] for p in points) for i in range(3)]],
        material_slots=[m.name for m in ob.data.materials],
        modifiers=[dict(type=m.type,name=m.name) for m in ob.modifiers]))
    ev.to_mesh_clear()
    print('EXPORT_MESH',name,records[-1]['vertices'],records[-1]['triangles'],flush=True)
bpy.ops.object.select_all(action='DESELECT')
rig.hide_viewport=False;rig.hide_set(False);rig.select_set(True)
for ob in objects: ob.select_set(True)
bpy.context.view_layer.objects.active=rig
output=root/'unity/BoxerP0/Assets/Resources/Boxer3D/Ramirez_UAT3.fbx'
settings=dict(use_selection=True, object_types={'ARMATURE','MESH'},add_leaf_bones=False,
    bake_anim=False,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,
    apply_scale_options='FBX_SCALE_UNITS',global_scale=1.0,use_space_transform=True,
    use_mesh_modifiers=True,mesh_smooth_type='FACE',path_mode='STRIP',embed_textures=False)
# Blender's stock exporter uses salted Python hashes and the wall clock. Lock
# those metadata inputs in this process only; leave the installed addon intact.
from io_scene_fbx import export_fbx_bin, fbx_utils
original_header=export_fbx_bin.fbx_header_elements
fixed_time=datetime.datetime(2026,9,30,0,0,0)
export_fbx_bin.fbx_header_elements=lambda root,data,time=None: original_header(root,data,fixed_time)
def stable_uuid(used,key):
    value=key if isinstance(key,int) and 0<=key<2**63 else int.from_bytes(hashlib.sha256(str(key).encode()).digest()[:8],'big') % 1000000000
    while value in used: value+=1
    return fbx_utils.UUID(value)
fbx_utils._key_to_uuid=stable_uuid
bpy.ops.export_scene.fbx(filepath=str(output),**settings)
manifest=dict(source_sha256=expected,art_source_sha='3e4e13c4b4451cb542e068652508ec12e248225f',
    blender=bpy.app.version_string,blender_build=bpy.app.build_hash.decode(),
    fbx_sha256=hashlib.sha256(output.read_bytes()).hexdigest(),
    settings={k:sorted(v) if isinstance(v,set) else v for k,v in settings.items()},
    deterministic_metadata=dict(timestamp=fixed_time.isoformat(),uuid='sha256 semantic key'),
    bones=[dict(name=b.name,parent=b.parent.name if b.parent else None,
        head_blender_m=list(b.head_local),tail_blender_m=list(b.tail_local),length_m=b.length) for b in rig.data.bones],
    rig_matrix_world=list(map(list,rig.matrix_world)),objects=records,
    vertices=sum(r['vertices'] for r in records),triangles=sum(r['triangles'] for r in records),
    excluded=[ob.name for ob in bpy.context.scene.objects if ob not in objects and ob!=rig])
out=root/'evidence/ramirez-unity-round2/export';out.mkdir(parents=True,exist_ok=True)
(out/'manifest.json').write_text(json.dumps(manifest,indent=2))
print('APPROVED_RAMIREZ_EXPORT_COMPLETE',manifest['fbx_sha256'],manifest['triangles'],flush=True)
