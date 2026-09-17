from pathlib import Path
from PIL import Image, ImageDraw


REPO = Path(__file__).resolve().parents[4]
SOURCE = REPO / "docs/handoff/reference-ui/06-opponent-ramirez-turnaround.jpg"
OUT = REPO / "art/blender/ramirez/references"

# Pixel crops preserve the approved source pixels exactly inside each crop.
# The original approved image is never rewritten.
CROPS = {
    "front": (22, 72, 190, 486),
    "three_quarter_front": (190, 72, 354, 486),
    "side": (346, 72, 510, 486),
    "back": (500, 72, 672, 486),
    "three_quarter_back": (662, 72, 840, 486),
}


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    image = Image.open(SOURCE).convert("RGB")
    for name, box in CROPS.items():
        crop = image.crop(box)
        crop.save(OUT / f"{name}_reference.png")

    # Diagnostic grid used only to make the manually selected anatomical
    # landmarks reproducible. It is not an art reference replacement.
    front = Image.open(OUT / "front_reference.png").convert("RGB")
    draw = ImageDraw.Draw(front)
    w, h = front.size
    for x in range(0, w, 20):
        draw.line((x, 0, x, h), fill=(80, 80, 80), width=1)
        draw.text((x + 1, 1), str(x), fill=(255, 255, 0))
    for y in range(0, h, 20):
        draw.line((0, y, w, y), fill=(80, 80, 80), width=1)
        draw.text((1, y + 1), str(y), fill=(255, 255, 0))
    front.save(OUT / "front_reference_grid.png")


if __name__ == "__main__":
    main()
