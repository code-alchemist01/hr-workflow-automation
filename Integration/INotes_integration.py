from openai import OpenAI
from datetime import datetime
import json

# LM Studio local server configuration
LM_STUDIO_BASE_URL = "http://127.0.0.1:1234"  # LM Studio'nun varsayılan portu
#MODEL_NAME = "openai/gpt-oss-20b"  # LM Studio'da yüklediğiniz model adı
MODEL_NAME = "qwen/qwen3-4b-2507"

TRANSCRIPT_INPUT_FILE = "Tanınan Metin.txt"
MEETING_SUMMARY_FILE = "toplanti_ozeti.txt"

# LM Studio client'ını yapılandır
client = OpenAI(
    base_url=LM_STUDIO_BASE_URL + "/v1",
    api_key="lm-studio"  # LM Studio için dummy key
)

def extract_notes_with_lm_studio(transcript_text):
    if not transcript_text.strip() or transcript_text == "Metin bulunamadı":
        return "Transkript metni boş veya bulunamadı, not çıkarılamadı."

    prompt = f"""
Aşağıdaki toplantı metnini analiz et ve toplantıda geçen tüm not alınabilecek cümleleri bul. 
- Notlar net ve açık olmalı.
- Hiçbir açıkça belirtilen notu atlama.
- Genel konuşmaları ve tekrarları dahil etme.

Format:
* **Başlık:** Not için başlık
    * **İçerik:** Notun içeriği ("İçerik" kısmı tek satır olsun.)
    
Transkript:
{transcript_text}

Çıkarılan Notlar:
"""

    print("\nLM Studio'ya not çıkarma isteği gönderiliyor...")
    try:
        response = client.chat.completions.create(
            model=MODEL_NAME,
            messages=[
                {"role": "system", "content": "Sen bir toplantı analiz uzmanısın. Toplantı metinlerinden notlar çıkarma konusunda uzmansın."},
                {"role": "user", "content": prompt}
            ],
            temperature=0.3,
            max_tokens=2000
        )
        return response.choices[0].message.content
    except Exception as e:
        print(f"LM Studio'dan yanıt alınırken hata oluştu: {e}")
        print("LM Studio'nun çalıştığından ve modelin yüklendiğinden emin olun.")
        return "Notlar çıkarılamadı."

def test_lm_studio_connection():
    """LM Studio bağlantısını test et"""
    try:
        response = client.chat.completions.create(
            model=MODEL_NAME,
            messages=[{"role": "user", "content": "Merhaba, çalışıyor musun?"}],
            max_tokens=50
        )
        print("✓ LM Studio bağlantısı başarılı")
        return True
    except Exception as e:
        print(f"✗ LM Studio bağlantı hatası: {e}")
        print("Lütfen şunları kontrol edin:")
        print("1. LM Studio uygulamasının açık olduğunu")
        print("2. openai/gpt-oss-20b modelinin yüklendiğini")
        print("3. Local server'ın başlatıldığını (http://127.0.0.1:1234)")
        return False

def parse_lm_studio_to_notes_backend_api(lm_studio_text, folder_id=None):
    """LM Studio çıktısını Notes backend API formatına dönüştür"""
    notes = []
    current_note = {}

    for line in lm_studio_text.splitlines():
        line = line.strip()
        # Not başlığı satırları esnek yakalama
        if line.lower().startswith("başlık:") or line.startswith("* **Başlık:**") or line.startswith("* Başlık:"):
            if current_note:
                notes.append(current_note)
            title = line.replace("* **Başlık:**", "").replace("* Başlık:", "").replace("Başlık:", "").strip(" -–—:")
            current_note = {
                "title": title,
                "content": None,
                "color": None,
                "folderId": folder_id,
                "tagIds": []
            }
        if line.lower().startswith("içerik:") or line.startswith("* **İçerik:**") or line.startswith("* İçerik:"):
            content = line.replace("* **İçerik:**", "").replace("* İçerik:", "").replace("İçerik:", "").strip(" -–—:")
            current_note["content"] = content

    if current_note:
        notes.append(current_note)

    return notes

def INotes_main():
    print("LM Studio bağlantısı test ediliyor...")
    if not test_lm_studio_connection():
        exit()
    
    print(f"'{TRANSCRIPT_INPUT_FILE}' dosyasından transkript okunuyor...")
    with open("Tanınan Metin.txt", 'r', encoding="UTF-8") as f:
        toplantı_metni = f.read()

    notes = extract_notes_with_lm_studio(toplantı_metni)
    
    with open("Notlar.txt", 'w', encoding="UTF-8") as f:
        f.write(notes)

    # Notes Backend API formatına dönüştür
    notes_backend_list = parse_lm_studio_to_notes_backend_api(notes)
        
    print("\n--- Notes Backend API Uyumlu JSON ---")
    print(json.dumps(notes_backend_list, indent=2, ensure_ascii=False))
        
    # Notes Backend API formatını ayrı dosyaya kaydet
    backend_json_file = "notlar_backend.json"
    with open(backend_json_file, "w", encoding="utf-8") as f_backend:
        json.dump(notes_backend_list, f_backend, ensure_ascii=False, indent=2)
    print(f"Backend API formatındaki notlar '{backend_json_file}' dosyasına kaydedildi.")
    
    # Notes Backend API'ye gönder
    print("\n--- Notes Backend API'ye Gönderim ---")
    try:
        import requests
            
        # Notes Backend API server'ının çalışıp çalışmadığını kontrol et
        try:
            backend_base_url = "http://localhost:5258"  # Notes Backend API URL'i
                
            print(f"Notes Backend API'ye bağlanmaya çalışılıyor: {backend_base_url}")
                
            # Her notu tek tek gönder
            success_count = 0
            for note in notes_backend_list:
                try:
                    api_response = requests.post(
                        f"{backend_base_url}/api/Note",
                        json=note,
                        headers={"Content-Type": "application/json"},
                        timeout=10
                    )
                        
                    if api_response.status_code in [200, 201]:
                        success_count += 1
                        print(f"✅ Not başarıyla eklendi: {note['title']}")
                    else:
                        print(f"❌ Not eklenemedi: {note['title']} (Status: {api_response.status_code})")
                        print(f"Hata: {api_response.text}")
                except Exception as note_error:
                    print(f"❌ Not gönderim hatası ({note['title']}): {note_error}")
                        
            print(f"📝 Notes Backend API'ye gönderim tamamlandı! {success_count}/{len(notes_backend_list)} not başarıyla eklendi.")
        
        except requests.exceptions.ConnectionError:
            print("⚠️ Notes Backend API sunucusuna bağlanılamadı.")
            print("Server'ın çalıştığından emin olun.")
        except requests.exceptions.Timeout:
            print("⚠️ Notes Backend API sunucusu yanıt vermiyor. (timeout)")
    except ImportError:
        print("⚠️ requests kütüphanesi bulunamadı")
        print("Yüklemek için: pip install requests")
    except Exception as api_error:
        print(f"⚠️ Notes Backend API gönderim hatası: {api_error}")

if __name__ == "__main__":
    INotes_main()