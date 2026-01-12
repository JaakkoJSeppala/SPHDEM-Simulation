#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Download PDF and extract all images (figures) as PNGs.
-----------------------------------------------------
Works for the open-access article:
  https://upcommons.upc.edu/server/api/core/bitstreams/ae46a928-f395-44b8-addc-fccfea044513/content

Steps:
  1. Downloads the PDF
  2. Extracts all embedded images
  3. Saves them as PNG/JPG in extracted_figures/

Requires: pip install PyMuPDF requests
"""

import fitz  # PyMuPDF
import os, requests

# === 1. Lähde-URL ===
url = "https://upcommons.upc.edu/server/api/core/bitstreams/ae46a928-f395-44b8-addc-fccfea044513/content"
pdf_file = "paper.pdf"

# === 2. Lataa PDF tiedostoksi, jos sitä ei ole ===
if not os.path.exists(pdf_file):
    print(f"⬇️ Downloading PDF from {url} ...")
    r = requests.get(url)
    if r.status_code == 200:
        with open(pdf_file, "wb") as f:
            f.write(r.content)
        print(f"✅ Saved as {pdf_file}")
    else:
        raise RuntimeError(f"Failed to download PDF (HTTP {r.status_code})")

# === 3. Luo kansio kuville ===
out_dir = "extracted_figures"
os.makedirs(out_dir, exist_ok=True)

# === 4. Avaa PDF ja käy läpi sivut ===
doc = fitz.open(pdf_file)
img_count = 0

for page_index, page in enumerate(doc):
    image_list = page.get_images(full=True)
    if not image_list:
        continue

    print(f"🧩 Page {page_index+1}: {len(image_list)} image(s) found.")
    for img_index, img in enumerate(image_list):
        xref = img[0]
        base_image = doc.extract_image(xref)
        image_bytes = base_image["image"]
        image_ext = base_image["ext"]
        img_count += 1
        img_filename = os.path.join(out_dir, f"figure_{page_index+1}_{img_index+1}.{image_ext}")
        with open(img_filename, "wb") as f:
            f.write(image_bytes)
        print(f"  → saved {img_filename}")

doc.close()
print(f"\n✅ Extraction complete — {img_count} images saved to: {out_dir}/")
