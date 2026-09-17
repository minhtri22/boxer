from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


REPO = Path(__file__).resolve().parents[4]
REF = REPO / "art/blender/ramirez/references"
CLAY = REPO / "art/blender/ramirez/renders/clay"
OUT = REPO / "art/blender/ramirez/renders/comparisons"

VIEWS = [
    ("front", "front_reference.png", "01_front_neutral.png"),
    ("three_quarter_front", "three_quarter_front_reference.png", "02_three_quarter_front_neutral.png"),
    ("side", "side_reference.png", "03_side_neutral.png"),
    ("back", "back_reference.png", "04_back_neutral.png"),
    ("three_quarter_back", "three_quarter_back_reference.png", "05_three_quarter_back_neutral.png"),
]


def contain(im: Image.Image, box):
    w, h = box
    scale = min(w / im.width, h / im.height)
    size = (max(1, round(im.width * scale)), max(1, round(im.height * scale)))
    return im.resize(size, Image.Resampling.LANCZOS)


def make_sheet(name, ref_name, clay_name):
    ref = Image.open(REF / ref_name).convert("RGB")
    clay = Image.open(CLAY / clay_name).convert("RGB")
    canvas = Image.new("RGB", (1400, 980), (18, 18, 20))
    draw = ImageDraw.Draw(canvas)
    font = ImageFont.load_default(size=28)
    small = ImageFont.load_default(size=20)
    draw.text((40, 24), f"RAMIREZ STATIC GATE — {name.replace('_', ' ').upper()}", fill=(238, 238, 238), font=font)
    draw.text((130, 78), "APPROVED REFERENCE", fill=(214, 184, 112), font=small)
    draw.text((865, 78), "BLENDER CLAY", fill=(214, 184, 112), font=small)

    left = contain(ref, (520, 820))
    right = contain(clay, (520, 820))
    canvas.paste(left, (100 + (520 - left.width) // 2, 125 + (820 - left.height) // 2))
    canvas.paste(right, (780 + (520 - right.width) // 2, 125 + (820 - right.height) // 2))
    draw.line((700, 110, 700, 945), fill=(90, 90, 94), width=2)
    canvas.save(OUT / f"{name}_reference_vs_blender.png")


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    for args in VIEWS:
        make_sheet(*args)


if __name__ == "__main__":
    main()
