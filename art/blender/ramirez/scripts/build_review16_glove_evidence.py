from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[4]
R14 = ROOT / "art" / "blender" / "ramirez" / "renders" / "review14-glove-reference-correction"
R15 = ROOT / "art" / "blender" / "ramirez" / "renders" / "review15-gloves-only"
R16 = ROOT / "art" / "blender" / "ramirez" / "renders" / "review16-glove-fix"


def fit(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    target_w, target_h = size
    scale = max(target_w / image.width, target_h / image.height)
    resized = image.resize(
        (round(image.width * scale), round(image.height * scale)),
        Image.Resampling.LANCZOS,
    )
    left = max(0, (resized.width - target_w) // 2)
    top = max(0, (resized.height - target_h) // 2)
    return resized.crop((left, top, left + target_w, top + target_h))


def label(panel: Image.Image, text: str) -> None:
    draw = ImageDraw.Draw(panel)
    font = ImageFont.load_default(size=18)
    box = draw.textbbox((0, 0), text, font=font)
    pad = 7
    draw.rectangle(
        (6, 6, box[2] + pad * 2 + 6, box[3] + pad * 2 + 6),
        fill=(0, 0, 0),
    )
    draw.text((6 + pad, 6 + pad), text, fill="white", font=font)


def approved_reference() -> Image.Image:
    sheet = Image.open(R14 / "qa-gloves-reference-vs-review13-vs-review14.jpg").convert("RGB")
    return sheet.crop((0, 0, sheet.width // 3, sheet.height))


def evidence_grid() -> None:
    names = [
        "glove_front_closeup.png",
        "glove_three_quarter_closeup.png",
        "glove_side_closeup.png",
        "neutral_guard_closeup.png",
        "jab_closeup.png",
        "cross_closeup.png",
    ]
    labels = ["FRONT", "3/4 FRONT", "SIDE", "GUARD", "JAB", "CROSS"]
    cell = (360, 360)
    sheet = Image.new("RGB", (cell[0] * 3, cell[1] * 2), "black")
    for index, (name, title) in enumerate(zip(names, labels)):
        panel = fit(Image.open(R16 / name).convert("RGB"), cell)
        label(panel, title)
        sheet.paste(panel, ((index % 3) * cell[0], (index // 3) * cell[1]))
    sheet.save(R16 / "qa-review16-required-evidence.jpg", quality=90)


def round_comparison() -> None:
    cell = (420, 560)
    ref = fit(approved_reference(), cell)
    old = fit(Image.open(R15 / "combat-closeup" / "01_neutral_guard_closeup.png").convert("RGB"), cell)
    new = fit(Image.open(R16 / "neutral_guard_closeup.png").convert("RGB"), cell)
    label(ref, "APPROVED REFERENCE")
    label(old, "REVIEW15 - REJECTED")
    label(new, "REVIEW16 - GLOVE FIX")
    sheet = Image.new("RGB", (cell[0] * 3, cell[1]), "black")
    sheet.paste(ref, (0, 0))
    sheet.paste(old, (cell[0], 0))
    sheet.paste(new, (cell[0] * 2, 0))
    sheet.save(R16 / "qa-reference-vs-review15-vs-review16.jpg", quality=92)


def small_previews() -> None:
    for source, output in [
        ("glove_side_closeup.png", "qa-side-latest-small.jpg"),
        ("qa-review16-required-evidence.jpg", "qa-evidence-small.jpg"),
        ("qa-reference-vs-review15-vs-review16.jpg", "qa-comparison-small.jpg"),
    ]:
        image = Image.open(R16 / source).convert("RGB")
        image.thumbnail((300, 300))
        image.save(R16 / output, quality=25, optimize=True)


if __name__ == "__main__":
    evidence_grid()
    round_comparison()
    small_previews()
    for name in [
        "qa-review16-required-evidence.jpg",
        "qa-reference-vs-review15-vs-review16.jpg",
        "qa-side-latest-small.jpg",
        "qa-evidence-small.jpg",
        "qa-comparison-small.jpg",
    ]:
        path = R16 / name
        print(path, path.stat().st_size)
