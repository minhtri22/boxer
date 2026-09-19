import argparse
import colorsys
from pathlib import Path

from PIL import Image


def classify(rgb):
    r, g, b = rgb
    h, sat, val = colorsys.rgb_to_hsv(r / 255.0, g / 255.0, b / 255.0)
    if max(rgb) < 28:
        return " "
    if sat < 0.28 and val > 0.38:
        return "W"
    if (h > 0.94 or h < 0.018) and sat > 0.42 and val > 0.12:
        return "R"
    if h < 0.10 and sat > 0.25:
        return "S"
    if 0.10 <= h < 0.18 and sat > 0.35:
        return "Y"
    return "."


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("image")
    args = parser.parse_args()
    image = Image.open(Path(args.image)).convert("RGB")

    points = []
    stride = 2
    for y in range(0, image.height, stride):
        for x in range(0, image.width, stride):
            if classify(image.getpixel((x, y))) in {"R", "W"}:
                points.append((x, y))
    if not points:
        raise SystemExit("No glove/cuff pixels detected")

    xs = [p[0] for p in points]
    ys = [p[1] for p in points]
    margin = 45
    box = (
        max(0, min(xs) - margin),
        max(0, min(ys) - margin),
        min(image.width, max(xs) + margin),
        min(image.height, max(ys) + margin),
    )
    print("GLOVE_MASK_BOX=" + ",".join(str(v) for v in box))
    crop = image.crop(box).resize((70, 50), Image.Resampling.BILINEAR)
    print("LEGEND R=glove W=cuff S=skin Y=gold .=other")
    for y in range(crop.height):
        print("".join(classify(crop.getpixel((x, y))) for x in range(crop.width)))


if __name__ == "__main__":
    main()
