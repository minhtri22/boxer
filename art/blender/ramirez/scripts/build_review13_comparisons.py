from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[4]
R12 = ROOT / "art" / "blender" / "ramirez" / "renders" / "review12-reference-rebuild"
R13 = ROOT / "art" / "blender" / "ramirez" / "renders" / "review13-guard-leg-correction"


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
    font = ImageFont.load_default(size=24)
    pad = 12
    box = draw.textbbox((0, 0), text, font=font)
    w = box[2] - box[0] + pad * 2
    h = box[3] - box[1] + pad * 2
    draw.rectangle((10, 10, 10 + w, 10 + h), fill=(0, 0, 0, 210))
    draw.text((10 + pad, 10 + pad), text, fill="white", font=font)


def reference_half(path: Path) -> Image.Image:
    image = Image.open(path).convert("RGB")
    return image.crop((0, 0, image.width // 2, image.height))


def build_reference_vs_review13() -> None:
    panel_size = (600, 760)

    body_ref = fit_cover(reference_half(R12 / "qa-body-reference-vs-review12.jpg"), panel_size)
    body_new = fit_cover(Image.open(R13 / "static" / "01_front.png").convert("RGB"), panel_size)
    label(body_ref, "APPROVED REFERENCE")
    label(body_new, "REVIEW13 FRONT")
    sheet = Image.new("RGB", (1200, 760), "white")
    sheet.paste(body_ref, (0, 0))
    sheet.paste(body_new, (600, 0))
    sheet.save(R13 / "qa-body-reference-vs-review13.jpg", quality=94)

    glove_ref = fit_cover(reference_half(R12 / "qa-gloves-reference-vs-review12.jpg"), panel_size)
    guard = Image.open(R13 / "combat" / "01_neutral_guard.png").convert("RGB")
    # Crop the upper half so glove position, face opening, cuff fit and forearm transition
    # occupy the same visual weight as the approved reference panel.
    glove_new = fit_cover(guard, panel_size, (0, 0, guard.width, round(guard.height * 0.64)))
    label(glove_ref, "APPROVED REFERENCE")
    label(glove_new, "REVIEW13 GUARD")
    sheet = Image.new("RGB", (1200, 760), "white")
    sheet.paste(glove_ref, (0, 0))
    sheet.paste(glove_new, (600, 0))
    sheet.save(R13 / "qa-gloves-reference-vs-review13.jpg", quality=94)


def build_review12_vs_review13_guard() -> None:
    panel_size = (600, 900)
    old = fit_cover(Image.open(R12 / "combat" / "01_neutral_guard.png").convert("RGB"), panel_size)
    new = fit_cover(Image.open(R13 / "combat" / "01_neutral_guard.png").convert("RGB"), panel_size)
    label(old, "REVIEW12 - REJECTED")
    label(new, "REVIEW13 - CORRECTED")
    sheet = Image.new("RGB", (1200, 900), "white")
    sheet.paste(old, (0, 0))
    sheet.paste(new, (600, 0))
    sheet.save(R13 / "qa-guard-review12-vs-review13.jpg", quality=94)


if __name__ == "__main__":
    R13.mkdir(parents=True, exist_ok=True)
    build_reference_vs_review13()
    build_review12_vs_review13_guard()
    print(R13 / "qa-body-reference-vs-review13.jpg")
    print(R13 / "qa-gloves-reference-vs-review13.jpg")
    print(R13 / "qa-guard-review12-vs-review13.jpg")
