"""Render native head tile 1001 as a texture-authoring guide, no GPU UI needed."""
import bpy,math,json
from pathlib import Path
from mathutils import Vector
art=Path(__file__).resolve().parents[1];out=art/'textures/head-study';out.mkdir(parents=True,exist_ok=True)
body=bpy.data.objects['Studio Ramirez continuous sculpt'];source=body.data;uv=source.uv_layers.active
points=[];faces=[];colors=[];landmarks={}
targets={'eye':(.035,-.130,1.676),'lip':(.012,-.144,1.612),'nose':(.008,-.17,1.646),'brow':(.035,-.12,1.694),'chin':(.015,-.12,1.583)}
nearest={k:1e9 for k in targets}
for poly in source.polygons:
    q=uv.data[poly.loop_start].uv
    if math.floor(q.x)!=0 or math.floor(q.y)!=0:continue
    f=[]
    for li in poly.loop_indices:
        vertex=source.vertices[source.loops[li].vertex_index];p=body.matrix_world @ vertex.co;t=uv.data[li].uv
        f.append(len(points));points.append((t.x,t.y,0))
        x,y,z=p;n=vertex.normal
        value=.60+.38*max(0,-n.y)
        color=(.30*value,.15*value,.085*value,1)
        if z>1.737:color=(.035,.025,.018,1)
        if abs(x)<.028 and 1.607<z<1.619 and y<-.105:color=(.20,.067,.05,1)
        colors.append(color)
        for k,target in targets.items():
            distance=(Vector((abs(x),y,z))-Vector(target)).length
            if distance<nearest[k]:nearest[k]=distance;landmarks[k]=[float(t.x),float(t.y)]
    faces.append(f)
scene=bpy.data.scenes.new('UV guide authoring');bpy.context.window.scene=scene
data=bpy.data.meshes.new('Native head tile');data.from_pydata(points,[],faces);data.update()
ob=bpy.data.objects.new(data.name,data);scene.collection.objects.link(ob)
attr=data.color_attributes.new(name='Guide pigment',type='FLOAT_COLOR',domain='POINT')
for a,c in zip(attr.data,colors):a.color=c
mat=bpy.data.materials.new('UV guide emission');mat.use_nodes=True;nodes=mat.node_tree.nodes;links=mat.node_tree.links
nodes.clear();output=nodes.new('ShaderNodeOutputMaterial');em=nodes.new('ShaderNodeEmission');a=nodes.new('ShaderNodeAttribute');a.attribute_name=attr.name
links.new(a.outputs['Color'],em.inputs[0]);links.new(em.outputs[0],output.inputs['Surface']);data.materials.append(mat)
camdata=bpy.data.cameras.new('UV orthographic');cam=bpy.data.objects.new('UV orthographic',camdata);scene.collection.objects.link(cam);cam.location=(.5,.5,2);cam.rotation_euler=(0,0,0);camdata.type='ORTHO';camdata.ortho_scale=1;scene.camera=cam
world=bpy.data.worlds.new('UV background');world.use_nodes=True;world.node_tree.nodes.get('Background').inputs[0].default_value=(.30,.15,.085,1);scene.world=world
scene.render.engine='CYCLES';scene.cycles.samples=1;scene.cycles.use_denoising=False;scene.cycles.device='CPU'
scene.view_settings.view_transform='Standard';scene.render.resolution_x=1536;scene.render.resolution_y=1536;scene.render.resolution_percentage=100
scene.render.filepath=str(out/'native-head-template.png');bpy.ops.render.render(write_still=True)
(out/'native-head-template.json').write_text(json.dumps({'tile':[0,0],'mirrored_partner_tile':[0,2],'landmarks_uv':landmarks,'note':'UV guide only; includes one half of face/scalp/neck. Opposite half uses the mirrored native tile. Ear is a separate island.'},indent=2))
print('HEAD_UV_TEMPLATE_COMPLETE',flush=True)
