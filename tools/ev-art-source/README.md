# Ramirez anatomical mesh inputs

Only graphical data from MakeHuman is used. No MakeHuman application code is
included in Boxer. These bundled base mesh, morphology targets and skinning
weights are CC0 under the upstream asset license:
https://github.com/makehumancommunity/makehuman/blob/master/LICENSE.md
https://github.com/makehumancommunity/makehuman/blob/master/LICENSE.ASSETS.md

Downloaded 2026-09-14 from the official `makehumancommunity/makehuman` repository:
- makehuman/data/3dobjs/base.obj
- makehuman/data/rigs/default.mhskel
- makehuman/data/rigs/default_weights.mhw
- makehuman/data/targets/macrodetails/caucasian-male-young.target
- makehuman/data/targets/macrodetails/universal-male-young-maxmuscle-averageweight.target

`makehuman-cc0-inputs.zip` preserves the exact inputs and both original license
files. `input-hashes.json` identifies the bytes used; upstream master is not a
reproduction dependency. Extract the archive to a temporary directory, then run:

```powershell
python tools/build_ev_anatomy.py <extracted-directory> unity/BoxerP0/Assets/Resources/EV/RamirezAnatomy.json
```

The offline builder requires Python and NumPy. Runtime has no Python, Blender,
MakeHuman, third-party rig, Animator, or physics-engine dependency. A ten-bone
Unity skinned mesh follows the already solved Round2 anatomical anchors.
Head dimensions use uniform scaling, not a spherical wrap. Fingers and hidden
anatomy under trunks are removed from the exported sporting character.

Generated material assets: RamirezHead.png and GloveLeather.png were created by
OpenAI image generation for this task. They are surface textures, never opaque
full-body pose overlays. Existing approved reference JPGs are unchanged.
