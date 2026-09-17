import argparse
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo", required=True)
    return parser.parse_args()


def font(size, bold=False):
    candidates = [
        Path(r"C:\Windows\Fonts\arialbd.ttf" if bold else r"C:\Windows\Fonts\arial.ttf"),
        Path(r"C:\Windows\Fonts\segoeuib.ttf" if bold else r"C:\Windows\Fonts\segoeui.ttf"),
    ]
    for path in candidates:
        if path.exists():
            return ImageFont.truetype(str(path), size=size)
    return ImageFont.load_default()


def fit_image(image, box_w, box_h):
    image = image.convert("RGB")
    scale = min(box_w / image.width, box_h / image.height)
    size = (max(1, int(image.width * scale)), max(1, int(image.height * scale)))
    return image.resize(size, Image.Resampling.LANCZOS)


def paste_center(canvas, image, box):
    x0, y0, x1, y1 = box
    fitted = fit_image(image, x1 - x0, y1 - y0)
    x = x0 + ((x1 - x0) - fitted.width) // 2
    y = y0 + ((y1 - y0) - fitted.height) // 2
    canvas.paste(fitted, (x, y))


def crop_reference(reference, box_900):
    sx = reference.width / 900.0
    sy = reference.height / 675.0
    x0, y0, x1, y1 = box_900
    return reference.crop((int(x0 * sx), int(y0 * sy), int(x1 * sx), int(y1 * sy)))


def main():
    args = parse_args()
    repo = Path(args.repo)
    evidence = repo / "evidence" / "p1-ramirez-rebuild" / "blender-visual-gate-mpfb"
    reference_path = repo / "docs" / "handoff" / "reference-ui" / "06-opponent-ramirez-turnaround.jpg"
    reference = Image.open(reference_path).convert("RGB")

    # Crops target the five approved top-row turnaround figures in the 900x675 source.
    ref_crops = {
        "FRONT": (0, 50, 185, 475),
        "3/4 FRONT": (155, 45, 350, 475),
        "SIDE": (330, 45, 530, 475),
        "BACK": (505, 45, 710, 475),
        "GUARD": (0, 50, 185, 475),
    }
    new_files = {
        "FRONT": "01_front.png",
        "3/4 FRONT": "02_three_quarter_front.png",
        "SIDE": "03_side.png",
        "BACK": "04_back.png",
        "GUARD": "06_guard.png",
    }

    width = 1600
    header_h = 120
    row_h = 360
    margin = 36
    label_w = 180
    gap = 28
    col_w = (width - margin * 2 - label_w - gap) // 2
    height = header_h + row_h * len(new_files) + margin
    canvas = Image.new("RGB", (width, height), (16, 17, 20))
    draw = ImageDraw.Draw(canvas)
    title_font = font(34, bold=True)
    heading_font = font(25, bold=True)
    row_font = font(23, bold=True)
    note_font = font(18)

    draw.text((margin, 28), "RAMIREZ BLENDER VISUAL GATE — REFERENCE | NEW MODEL", fill=(235, 235, 238), font=title_font)
    ref_x0 = margin + label_w
    new_x0 = ref_x0 + col_w + gap
    draw.text((ref_x0 + 20, 80), "APPROVED REFERENCE", fill=(205, 185, 130), font=heading_font)
    draw.text((new_x0 + 20, 80), "NEW BLENDER MODEL", fill=(205, 185, 130), font=heading_font)

    for row, label in enumerate(new_files):
        y0 = header_h + row * row_h
        y1 = y0 + row_h - 12
        if row % 2:
            draw.rectangle((margin, y0, width - margin, y1), fill=(22, 23, 27))
        draw.text((margin + 8, y0 + 28), label, fill=(240, 240, 242), font=row_font)
        draw.text((margin + 8, y0 + 64), "silhouette / anatomy / gear", fill=(145, 147, 154), font=note_font)

        ref_img = crop_reference(reference, ref_crops[label])
        new_img = Image.open(evidence / new_files[label]).convert("RGB")
        paste_center(canvas, ref_img, (ref_x0 + 12, y0 + 16, ref_x0 + col_w - 12, y1 - 12))
        paste_center(canvas, new_img, (new_x0 + 12, y0 + 16, new_x0 + col_w - 12, y1 - 12))

        draw.line((ref_x0 + col_w + gap // 2, y0 + 12, ref_x0 + col_w + gap // 2, y1 - 12), fill=(78, 79, 85), width=2)

    output = evidence / "comparison-reference-vs-new.png"
    canvas.save(output, quality=95)
    print(output)


if __name__ == "__main__":
    main()
