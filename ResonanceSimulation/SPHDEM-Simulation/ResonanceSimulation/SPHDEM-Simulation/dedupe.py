
import re
import sys
from pathlib import Path

def load_text(path):
    return Path(path).read_text(encoding='utf-8', errors='ignore')

def split_paragraphs(text):
    # Split on blank lines
    return [p.strip() for p in re.split(r'\n\s*\n', text) if p.strip()]

def remove_duplicates(paragraphs):
    seen = set()
    unique = []
    for p in paragraphs:
        key = re.sub(r'\s+', ' ', p).strip().lower()
        if key not in seen:
            seen.add(key)
            unique.append(p)
    return unique

def save_text(path, paragraphs):
    Path(path).write_text("\n\n".join(paragraphs), encoding='utf-8')

def main():
    if len(sys.argv) < 3:
        print("Usage: python dedupe.py input.txt output.txt")
        sys.exit(1)
    inp, out = sys.argv[1], sys.argv[2]
    text = load_text(inp)
    paras = split_paragraphs(text)
    cleaned = remove_duplicates(paras)
    save_text(out, cleaned)
    print(f"Done. Reduced from {len(paras)} to {len(cleaned)} paragraphs.")

if __name__ == "__main__":
    main()
