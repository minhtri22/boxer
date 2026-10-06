"""Read-only source inventory for an explicit game export selection."""
import bpy, json
rig = bpy.data.objects['RAMIREZ_RIG']
rig.data.pose_position = 'REST'
bpy.context.view_layer.update()
for ob in bpy.context.scene.objects:
    if ob.type == 'MESH':
        print('SOURCE_MESH', json.dumps(dict(name=ob.name, hidden=ob.hide_render,
            vertices=len(ob.data.vertices), polygons=len(ob.data.polygons),
            modifiers=[(m.name,m.type) for m in ob.modifiers],
            parent=ob.parent.name if ob.parent else None,
            groups=[g.name for g in ob.vertex_groups],
            materials=[m.name if m else None for m in ob.data.materials])))
for b in rig.data.bones:
    print('SOURCE_BONE', json.dumps(dict(name=b.name,parent=b.parent.name if b.parent else None,
        head=list(b.head_local),tail=list(b.tail_local),length=b.length)))
print('RIG_MATRIX', list(map(list, rig.matrix_world)))
