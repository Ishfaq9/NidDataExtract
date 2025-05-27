import pytesseract
from PIL import Image
import easyocr
import re
import json
import sys
import cv2

sys.stdout.reconfigure(encoding='utf-8')
# Get image path from argument
if len(sys.argv) < 2:
    print(json.dumps({"error": "Image path is required"}))
    sys.exit(1)

img_path = sys.argv[1]

img=cv2.imread(img_path)

#cv2.bitwise_not(img,img)

# Tesseract path
pytesseract.pytesseract.tesseract_cmd = r'C:\Program Files\Tesseract-OCR\tesseract.exe'

fields = {
    'নাম': r'নাম[:：]?\s*([^\n:：]+)',
    'Name': r'Name[:：]?\s*([^\n:：]+)',
    'পিতা': r'পিতা[:：]?\s*([^\n:：]+)',
    'মাতা': r'মাতা[:：]?\s*([^\n:：]+)',
    'স্বামী': r'স্বামী[:：]?\s*([^\n:：]+)',
    'স্ত্রী': r'স্ত্রী[:：]?\s*([^\n:：]+)',
    'DateOfBirth': r'Date of Birth[:：]?\s*([^\n:：]+)',
    #'IDNO': r'(?:ID\s*NO|NID\s*No\.?|NIDNo|NID\s*NO|NID\s*No)\s*[:：]?\s*([\d ]{8,30})'
    'IDNO': r'(?:ID\s*NO|NID\s*No\.?|NIDNo|NID\s*NO|NID\s*No|ID\s*N0)\s*[:：]?\s*([\d ]{8,30})'
}

def infer_name_from_lines(text, extracted_fields):
    lines = [line.strip() for line in text.splitlines() if line.strip()]
    for i, line in enumerate(lines):
        if "Name" in line:
            if extracted_fields.get('নাম') in [None, "", "No data found"] and i > 0:
                extracted_fields['নাম'] = lines[i - 1]
            if extracted_fields.get('Name') in [None, "", "No data found"] and i + 1 < len(lines):
                extracted_fields['Name'] = lines[i + 1]
    return extracted_fields

def clean_header_text(text):
    keywords_to_remove = [
        "বাংলাদেশ সরকার", "জাতীয় পরিচয়", "জাতীয় পরিচয়",
        "National ID", "Government of the People's Republic"
    ]
    return "\n".join(
        line.strip() for line in text.splitlines()
        if not any(keyword in line.strip() for keyword in keywords_to_remove)
    )

def extract_fields(text, fields):
    extracted = {}
    for key, pattern in fields.items():
        match = re.search(pattern, text, re.IGNORECASE | re.MULTILINE)
        if match:
            value = match.group(1).strip()
            if key == 'IDNO':
                value = value.replace(" ", "")
            extracted[key] = value
        else:
            extracted[key] = "No data found"
    return extracted

# OCR with Tesseract
#img = Image.open(img_path)
tesseract_text = pytesseract.image_to_string(img, lang='ben+eng')
tesseract_text = clean_header_text(tesseract_text)
tesseract_results = extract_fields(tesseract_text, fields)
tesseract_results = infer_name_from_lines(tesseract_text, tesseract_results)

# OCR with EasyOCR
reader = easyocr.Reader(['en', 'bn'], gpu=False)
results = reader.readtext(img)
easyocr_text = "\n".join([text for (_, text, _) in results])
easyocr_text = clean_header_text(easyocr_text)
easyocr_results = extract_fields(easyocr_text, fields)
easyocr_results = infer_name_from_lines(easyocr_text, easyocr_results)

# Merge results
final_results = {}
for key in fields:
    t_val = tesseract_results.get(key, "No data found")
    e_val = easyocr_results.get(key, "No data found")

    if t_val == "No data found" and e_val == "No data found":
        final = "No data found"
    elif t_val == e_val:
        final = t_val
    elif t_val != "No data found" and e_val == "No data found":
        final = t_val
    elif e_val != "No data found" and t_val == "No data found":
        final = e_val
    else:
        final = t_val if len(t_val.split()) >= len(e_val.split()) else e_val

    final_results[key] = final

print(json.dumps(final_results, ensure_ascii=False))
