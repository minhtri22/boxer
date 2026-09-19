from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[4]
R13 = ROOT / "art" / "blender" / "ramirez" / "renders" / "review13-guard-leg-correction"
R14 = ROOT / "art" / "blender" / "ramirez" / "renders" / "review14-glove-reference-correction"


def fit_cover(image: Image.Image, size: tuple[int, int], crop_box=None) -> Image.Image:
    if crop_box is not None:
        image = image.crop(crop_box)
    target_w, target_h = size
    scale = max(target_w / image.width, target_h / image.height)
    resized = image.resize((round(image.width * scale), round(image.height * scale)), Image.Resampling.LANCZOS)
    left = max(0, (resized.width - target_w) // 2)
    top = max(0, (resized.height - target_h) // 2)
    return resized.crop((left, top, left + target_w, top + target_h))


def label(panel: Image.Image, text: str) -> None:
    draw = ImageDraw.Draw(panel)
    font = ImageFont.load_default(size=22)
    pad = 10
    box = draw.textbbox((0, 0), text, font=font)
    w = box[2] - box[0] + pad * 2
    h = box[3] - box[1] + pad * 2
    draw.rectangle((10, 10, 10 + w, 10 + h), fill=(0, 0, 0, 210))
    draw.text((10 + pad, 10 + pad), text, fill="white", font=font)


def crop_guard(path: Path) -> Image.Image:
    image = Image.open(path).convert("RGB")
    return image.crop((0, 0, image.width, round(image.height * 0.64)))


def main() -> None:
    panel_size = (520, 720)
    reference_sheet = Image.open(R13 / "qa-gloves-reference-vs-review13.jpg").convert("RGB")
    reference = reference_sheet.crop((0, 0, reference_sheet.width // 2, reference_sheet.height))
    ref_panel = fit_cover(reference, panel_size)
    old_panel = fit_cover(crop_guard(R13 / "combat" / "01_neutral_guard.png"), panel_size)
    new_panel = fit_cover(crop_guard(R14 / "combat" / "01_neutral_guard.png"), panel_size)
    label(ref_panel, "APPROVED REFERENCE")
    label(old_panel, "REVIEW13 - REJECTED GLOVE")
    label(new_panel, "REVIEW14 - LEATHER GLOVE REBUILD")

    sheet = Image.new("RGB", (panel_size[0] * 3, panel_size[1]), "white")
    sheet.paste(ref_panel, (0, 0))
    sheet.paste(old_panel, (panel_size[0], 0))
    sheet.paste(new_panel, (panel_size[0] * 2, 0))
    out = R14 / "qa-gloves-reference-vs-review13-vs-review14.jpg"
    sheet.save(out, quality=95)
    print(out)


if __name__ == "__main__":
    main()
