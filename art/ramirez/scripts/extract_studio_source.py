"""Keep only the unmodified CC0 male sculpt and its source README as an input."""
import bpy,hashlib,json
from pathlib import Path
root=Path(__file__).resolve().parents[3];src=root/'art/ramirez/source'
bundle=src/'blender-base-1.4.1/human_base_meshes_bundle.blend'
with bpy.data.libraries.load(str(bundle),link=False) as (a,b):
    b.objects=['GEO-body_male_realistic'];b.texts=['README']
out=src/'studio-male-cc0.blend'
bpy.data.libraries.write(str(out),set(b.objects+b.texts),fake_user=True,compress=True)
readme=next(t for t in b.texts if t)
(src/'studio-male-source-readme.txt').write_text(readme.as_string())
(src/'studio-male-extraction.json').write_text(json.dumps({'source_bundle_sha256':hashlib.sha256(bundle.read_bytes()).hexdigest(),'extracted_sha256':hashlib.sha256(out.read_bytes()).hexdigest(),'object':'GEO-body_male_realistic','changes':'None. Source mesh, multires and object transform preserved.','license':'CC0 per bundled README'},indent=2))
print('EXTRACTED',out,out.stat().st_size)
