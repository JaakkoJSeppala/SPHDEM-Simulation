import os
import matplotlib.pyplot as plt
import matplotlib.image as mpimg
import math

# Kansion nimi, jonne PDF-kuvat purettiin
img_dir = "extracted_figures"

# Kerää kaikki kuvatiedostot
files = sorted(
    [f for f in os.listdir(img_dir) if f.lower().endswith((".png", ".jpg", ".jpeg"))]
)

if not files:
    print("❌ Ei löytynyt yhtään kuvaa hakemistosta:", img_dir)
else:
    print(f"📸 Löytyi {len(files)} kuvaa hakemistosta {img_dir}")

    # Näytetään kuvat 3xN ruudukossa
    n = len(files)
    cols = 3
    rows = math.ceil(n / cols)
    fig, axes = plt.subplots(rows, cols, figsize=(12, 4 * rows))

    for i, ax in enumerate(axes.flatten()):
        if i < n:
            fname = os.path.join(img_dir, files[i])
            img = mpimg.imread(fname)
            ax.imshow(img)
            ax.set_title(files[i], fontsize=8)
            ax.axis("off")
        else:
            ax.axis("off")

    plt.tight_layout()
    plt.show()
