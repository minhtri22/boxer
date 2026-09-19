from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[4]
R14 = ROOT / "art" / "blender" / "ramirez" / "renders" / "review14-glove-reference-correction"
R15 = ROOT / "art" / "blender" / "ramirez" / "renders" / "review15-gloves-only"


def fit(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    target_w, target_h = size
    scale = max(target_w / image.width, target_h / image.height)
    resized = image.resize((round(image.width * scale), round(image.height * scale)), Image.Resampling.LANCZOS)
    left = max(0, (resized.width - target_w) // 2)
    top = max(0, (resized.height - target_h) // 2)
    return resized.crop((left, top, left + target_w, top + target_h))


def label(panel: Image.Image, text: str) -> None:
    draw = ImageDraw.Draw(panel)
    font = ImageFont.load_default(size=20)
    box = draw.textbbox((0, 0), text, font=font)
    pad = 8
    draw.rectangle((8, 8, box[2] + pad * 2 + 8, box[3] + pad * 2 + 8), fill=(0, 0, 0))
    draw.text((8 + pad, 8 + pad), text, fill="white", font=font)


def reference_panel() -> Image.Image:
    src = Image.open(R14 / "qa-gloves-reference-vs-review13-vs-review14.jpg").convert("RGB")
    return src.crop((0, 0, src.width // 3, src.height))


def build_reference_comparison() -> None:
    size = (520, 720)
    ref = fit(reference_panel(), size)
    old = fit(Image.open(R14 / "combat" / "01_neutral_guard.png").convert("RGB"), size)
    new = fit(Image.open(R15 / "combat-closeup" / "01_neutral_guard_closeup.png").convert("RGB"), size)
    label(ref, "APPROVED REFERENCE")
    label(old, "REVIEW14 - REJECTED")
    label(new, "REVIEW15 - GLOVES ONLY")
    sheet = Image.new("RGB", (size[0] * 3, size[1]), "white")
    sheet.paste(ref, (0, 0))
    sheet.paste(old, (size[0], 0))
    sheet.paste(new, (size[0] * 2, 0))
    sheet.save(R15 / "qa-reference-vs-review14-vs-review15.jpg", quality=92)


def build_evidence_grid() -> None:
    items = [
        ("STATIC FRONT", R15 / "static-closeup" / "01_front_closeup.png"),
        ("STATIC 3/4", R15 / "static-closeup" / "02_three_quarter_front_closeup.png"),
        ("STATIC SIDE", R15 / "static-closeup" / "03_side_closeup.png"),
        ("NEUTRAL GUARD", R15 / "combat-closeup" / "01_neutral_guard_closeup.png"),
        ("JAB", R15 / "combat-closeup" / "02_jab_closeup.png"),
        ("CROSS", R15 / "combat-closeup" / "03_cross_closeup.png"),
    ]
    size = (360, 360)
    sheet = Image.new("RGB", (size[0] * 3, size[1] * 2), "black")
    for index, (name, path) in enumerate(items):
        panel = fit(Image.open(path).convert("RGB"), size)
        label(panel, name)
        sheet.paste(panel, ((index % 3) * size[0], (index // 3) * size[1]))
    sheet.save(R15 / "qa-review15-required-evidence.jpg", quality=88)


if __name__ == "__main__":
    build_reference_comparison()
    build_evidence_grid()
    print(R15 / "qa-reference-vs-review14-vs-review15.jpg")
    print(R15 / "qa-review15-required-evidence.jpg")
