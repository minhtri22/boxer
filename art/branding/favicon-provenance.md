# Boxer favicon provenance

Owner supplied `Gemini_Generated_Image_l6rtzll6rtzll6rt.png`, then explicitly
requested removal of the Gemini mark before ICO conversion.

- Original source preserved as `boxer-favicon-source.png`, SHA256
  `764f27084a64c20ef060676121aab398c3ad198a4ecf3372ab1c4021629d012d`.
- Cleaned square image: `boxer-favicon-clean.png`, SHA256
  `e3dab425abee6aaac4bc5e8ac38f840d08a64a20f78c32bd471911c131957d7f`.
- Edit used the built-in image generation/editing tool, not CLI/API fallback.
  Visual inspection confirms the bottom-right sparkle is absent. The edited
  output is not claimed pixel-identical to the original outside that area.
- Final `TemplateData/favicon.ico`, SHA256
  `4dcbf263f83d1ac3b33812c51a6145f12809ec52ee6056c739dd16e2248201b2`,
  contains PNG-compressed 32-bit frames at 16, 32, 48, 64, 128 and 256 pixels.
  `tools/build_boxer_favicon.ps1` converts the cleaned image without a crop.
- Original-image ICO and preview are retained locally in Wave1 evidence;
  they are not the deployed favicon.

## Exact edit prompt

Use case: precise-object-edit. Image 1 is the exact edit target, a square black
boxer silhouette on textured golden background. Remove ONLY the small pale
four-point Gemini sparkle/logo near the bottom-right corner (around x=904,y=904
on the 1024px original). Reconstruct that tiny area's golden textured background
seamlessly. Keep the boxer silhouette, gloves, pose, proportions, framing, golden
tones and all other areas unchanged. No cropping, no redrawing of the boxer, no
new symbols, no text, no added objects. Maintain square composition and opaque
background. This cleaned original will be converted to a favicon.ico.
