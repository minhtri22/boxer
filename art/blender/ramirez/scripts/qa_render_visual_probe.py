import argparse
from collections import Counter
from pathlib import Path

from PIL import Image


SYMBOLS = " .:-=+*#%@ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("image", type=Path)
    parser.add_argument("--width", type=int, default=64)
    parser.add_argument("--height", type=int, default=96)
    parser.add_argument("--colors", type=int, default=10)
    args = parser.parse_args()

    source = Image.open(args.image).convert("RGB")
    small = source.resize((args.width, args.height), Image.Resampling.LANCZOS)
    quant = small.quantize(colors=args.colors, method=Image.Quantize.MEDIANCUT)
    palette = quant.getpalette()
    counts = Counter(quant.getdata())
    ordered = [idx for idx, _ in counts.most_common()]
    symbol_for = {idx: SYMBOLS[i] for i, idx in enumerate(ordered)}

    print(f"IMAGE={args.image}")
    print(f"SOURCE_SIZE={source.size[0]}x{source.size[1]}")
    for idx in ordered:
        rgb = tuple(palette[idx * 3 : idx * 3 + 3])
        print(f"PALETTE {symbol_for[idx]!r} count={counts[idx]} rgb={rgb}")
    print("ASCII_BEGIN")
    pixels = list(quant.getdata())
    for y in range(args.height):
        row = pixels[y * args.width : (y + 1) * args.width]
        print("".join(symbol_for[v] for v in row))
    print("ASCII_END")


if __name__ == "__main__":
    main()
