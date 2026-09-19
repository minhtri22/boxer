from pathlib import Path

from PIL import Image, ImageDraw


REPO = Path(__file__).resolve().parents[4]
ROOT = REPO / "art" / "blender" / "ramirez"
REFS = ROOT / "references"
REVIEW = ROOT / "renders" / "review5"


def fit(image, box):
    image = image.copy()
    image.thumbnail(box, Image.Resampling.LANCZOS)
    return image


def build_trunks_comparisons():
    out = REVIEW / "trunks-comparison"
    out.mkdir(parents=True, exist_ok=True)
    views = [
        ("front", "front_reference.png", "01_front.png"),
        ("three_quarter_front", "three_quarter_front_reference.png", "02_three_quarter_front.png"),
        ("side", "side_reference.png", "03_side.png"),
        ("back", "back_reference.png", "04_back.png"),
    ]
    for name, ref_name, render_name in views:
        ref = Image.open(REFS / ref_name).convert("RGB")
        render = Image.open(REVIEW / "static" / render_name).convert("RGB")
        canvas = Image.new("RGB", (1440, 1080), (12, 12, 14))
        draw = ImageDraw.Draw(canvas)
        draw.text((30, 20), f"RAMIREZ TRUNKS - {name.replace('_', ' ').upper()}", fill="white")
        draw.text((170, 55), "APPROVED REFERENCE", fill=(220, 190, 120))
        draw.text((930, 55), "BLENDER REVIEW5", fill=(220, 190, 120))
        left = fit(ref, (620, 950))
        right = fit(render, (620, 950))
        canvas.paste(left, (40 + (620 - left.width) // 2, 90 + (950 - left.height) // 2))
        canvas.paste(right, (780 + (620 - right.width) // 2, 90 + (950 - right.height) // 2))
        draw.line((720, 80, 720, 1060), fill=(80, 80, 84), width=2)
        canvas.save(out / f"{name}_reference_vs_blender.jpg", quality=90)


def build_pose_contact():
    files = sorted((REVIEW / "combat").glob("*.png"))
    width, height, cols = 260, 390, 4
    rows = (len(files) + cols - 1) // cols
    canvas = Image.new("RGB", (cols * width, rows * (height + 26)), (8, 8, 10))
    draw = ImageDraw.Draw(canvas)
    for index, path in enumerate(files):
        image = Image.open(path).convert("RGB")
        image.thumbnail((width - 8, height - 8), Image.Resampling.LANCZOS)
        x = (index % cols) * width
        y = (index // cols) * (height + 26)
        canvas.paste(image, (x + (width - image.width) // 2, y + (height - image.height) // 2))
        draw.text((x + 5, y + height + 4), path.stem, fill="white")
    canvas.save(REVIEW / "pose-set-contact.jpg", quality=90)


def main():
    build_trunks_comparisons()
    build_pose_contact()
    print("review5 trunks + pose evidence ready")


if __name__ == "__main__":
    main()
