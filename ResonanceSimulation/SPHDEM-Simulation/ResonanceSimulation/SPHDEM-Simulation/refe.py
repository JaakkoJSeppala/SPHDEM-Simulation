import re
import bibtexparser
from PyPDF2 import PdfReader

# --- 1. Extract text from PDF ---
def extract_text_from_pdf(pdf_path):
    reader = PdfReader(pdf_path)
    text = ""
    for page in reader.pages:
        text += page.extract_text() + "\n"
    return text

# --- 2. Find all citation keys from LaTeX-like syntax ---
def find_citations(text):
    # \cite{key}, \parencite{key}, \textcite{key}
    pattern = r"\\(?:cite|parencite|textcite)\{([^}]+)\}"
    keys = re.findall(pattern, text)
    
    # Split cases like {key1,key2}
    result = []
    for k in keys:
        result.extend([x.strip() for x in k.split(",")])
    return set(result)

# --- 3. Load BibTeX keys ---
def load_bibtex_keys(bib_path):
    with open(bib_path, "r", encoding="utf-8") as f:
        bib = bibtexparser.load(f)
    return set(entry["ID"] for entry in bib.entries)

# --- 4. Detect placeholder references like "Author (2024)" ---
def find_placeholder_references(text):
    pattern = r"Author\s*\(\s*\d{4}\s*\)"
    return re.findall(pattern, text)

# --- 5. Main analysis ---
def analyze(pdf_path, bib_path):
    print("Extracting PDF...")
    text = extract_text_from_pdf(pdf_path)

    print("Loading BibTeX...")
    bib_keys = load_bibtex_keys(bib_path)

    print("Finding citations...")
    cited_keys = find_citations(text)

    missing = cited_keys - bib_keys
    unused  = bib_keys - cited_keys
    placeholders = find_placeholder_references(text)

    print("\n=== RESULTS ===")
    print("\nCitations found in text:", cited_keys)
    print("\nMissing BibTeX entries:", missing)
    print("\nUnused BibTeX entries:", unused)
    print("\nPlaceholder references detected:", placeholders)


# --- Run ---
if __name__ == "__main__":
    analyze("gradu.pdf", "references.bib")
