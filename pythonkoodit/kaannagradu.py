import tkinter as tk
from tkinter import messagebox
import subprocess
import os

def run_batch_file():
    batch_file_path = "C:\\Users\\jaakk\\Desktop\\Gradu\\compile.bat"  # Korvaa tämä polku ja tiedostonimi omalla
    if os.path.exists(batch_file_path):
        try:
            subprocess.Popen(batch_file_path, shell=True)
        except Exception as e:
            messagebox.showerror("Virhe", f"Tiedoston suorittamisessa tapahtui virhe: {e}")
    else:
        messagebox.showerror("Virhe", "Batch-tiedostoa ei löydy.")

# Luodaan pääikkuna
root = tk.Tk()
root.title("Batch-tiedoston suoritin")

# Luodaan painonappi
button = tk.Button(
    root,
    text="Käännä gradu",
    command=run_batch_file,
    padx=20,
    pady=10
)
button.pack(pady=20)

# Käynnistää pääikkunan
root.mainloop()
