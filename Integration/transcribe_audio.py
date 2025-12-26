import requests
import re
import os

API_KEY = "your_elevenlabs_api_key_here"  # ElevenLabs API anahtarınızı buraya girin
AUDIO_FILE = "extracted_audio.wav"  # extract_audio.py'den gelen ses dosyası

if not os.path.exists(AUDIO_FILE):
    print(f"Hata: '{AUDIO_FILE}' dosyası bulunamadı. Önce extract_audio.py dosyasını çalıştırın.")
    exit()

if not API_KEY or API_KEY == "your_elevenlabs_api_key_here":
    print("Lütfen geçerli bir ElevenLabs API anahtarı girin.")
    exit()

url = "https://api.elevenlabs.io/v1/speech-to-text"

headers = {
    "xi-api-key": API_KEY,
}

data = {
    "model_id": "scribe_v1"  # veya "scribe_v1_experimental"
}

print(f"'{AUDIO_FILE}' dosyası ElevenLabs API'ye gönderiliyor...")

with open(AUDIO_FILE, "rb") as f:
    files = {
        "file": (AUDIO_FILE, f, "audio/wav")
    }
    response = requests.post(url, headers=headers, data=data, files=files)

if response.status_code == 200:
    data = response.json()
    text = data.get("text", "Metin bulunamadı")

    # Nokta, soru, ünlemden sonra satır başı yap
    sentences = re.split(r'(?<=[.!?])\s+', text)
    formatted_text = "\n".join(sentences)

    print("Düzenlenmiş Transkript:\n")
    print(formatted_text)

    # Dosyaya da kaydet
    with open("Tanınan Metin.txt", "w", encoding="utf-8") as f_out:
        f_out.write(formatted_text)
    print(f"\nTranskript 'Tanınan Metin.txt' dosyasına kaydedildi.")

else:
    print("Hata oluştu:", response.status_code, response.text)