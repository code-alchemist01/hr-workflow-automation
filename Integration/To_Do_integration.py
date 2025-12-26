from openai import OpenAI
import os
import json
from datetime import datetime, timedelta

# LM Studio local server configuration
LM_STUDIO_BASE_URL = "http://127.0.0.1:1234"  # LM Studio'nun varsayılan portu
#MODEL_NAME = "openai/gpt-oss-20b"  # LM Studio'da yüklediğiniz model adı
MODEL_NAME = "qwen/qwen3-4b-2507"

TRANSCRIPT_INPUT_FILE = "Tanınan Metin.txt"
ACTION_ITEMS_OUTPUT_FILE = "eylem_maddeleri.txt"
ACTION_ITEMS_JSON_FILE = "eylem_maddeleri.json"
MEETING_SUMMARY_FILE = "toplanti_ozeti.txt"

# LM Studio client'ını yapılandır
client = OpenAI(
    base_url=LM_STUDIO_BASE_URL + "/v1",
    api_key="lm-studio"  # LM Studio için dummy key
)

def get_action_items_with_lm_studio(transcript_text):
    if not transcript_text.strip() or transcript_text == "Metin bulunamadı":
        return "Transkript metni boş veya bulunamadı, aksiyon maddeleri çıkarılamadı."

    prompt = f"""
Aşağıdaki toplantı transkriptini analiz et ve toplantıda geçen tüm uygulanabilir görev/aksiyon maddelerini çıkar. 
- Görevler net, açık ve uygulanabilir olmalı.
- Tüm mantıklı görevleri dahil et, hiçbir açıkça belirtilen görevi atlama.
- Genel konuşmaları ve tekrarları dahil etme.

Format:
* **Görevi:** Görev açıklaması
    * **Kişi:** İlgili kişi adı (varsa, belirtilmemiş ise "belirtilmemiş")
    * **Son Tarih:** gg.aa.yyyy veya belirtilmemiş

Transkript:
{transcript_text}

Görev ve Aksiyon Maddeleri:
"""

    print("\nLM Studio'ya aksiyon maddeleri çıkarma isteği gönderiliyor...")
    try:
        response = client.chat.completions.create(
            model=MODEL_NAME,
            messages=[
                {"role": "system", "content": "Sen bir toplantı analiz uzmanısın. Toplantı transkriptlerinden aksiyon maddelerini çıkarma konusunda uzmansın."},
                {"role": "user", "content": prompt}
            ],
            temperature=0.3,
            max_tokens=2000
        )
        return response.choices[0].message.content
    except Exception as e:
        print(f"LM Studio'dan yanıt alınırken hata oluştu: {e}")
        print("LM Studio'nun çalıştığından ve modelin yüklendiğinden emin olun.")
        return "Görev maddeleri çıkarılamadı."

def get_meeting_summary_with_lm_studio(transcript_text):
    if not transcript_text.strip() or transcript_text == "Metin bulunamadı":
        return "Transkript metni boş veya bulunamadı, toplantı özeti çıkarılamadı."

    prompt = f"""
Aşağıdaki toplantı transkriptini analiz et ve kapsamlı bir toplantı özeti çıkar.

Özet şu bölümleri içermeli:
1. **Toplantı Konusu:** Ana konu ve amaç
2. **Katılımcılar:** Toplantıya katılan kişiler
3. **Ana Konular:** Tartışılan başlıca konular
4. **Kararlar:** Alınan kararlar ve sonuçlar
5. **Önemli Noktalar:** Vurgulanan önemli bilgiler
6. **Sonraki Adımlar:** Planlanan gelecek faaliyetler

Transkript:
{transcript_text}

Toplantı Özeti:
"""

    print("\nLM Studio'ya toplantı özeti çıkarma isteği gönderiliyor...")
    try:
        response = client.chat.completions.create(
            model=MODEL_NAME,
            messages=[
                {"role": "system", "content": "Sen bir toplantı analiz uzmanısın. Toplantı transkriptlerinden kapsamlı özetler çıkarma konusunda uzmansın."},
                {"role": "user", "content": prompt}
            ],
            temperature=0.3,
            max_tokens=2000
        )
        return response.choices[0].message.content
    except Exception as e:
        return f"LM Studio bağlantı hatası: {str(e)}"

    print("\nLM Studio'ya aksiyon maddeleri çıkarma isteği gönderiliyor...")
    try:
        response = client.chat.completions.create(
            model=MODEL_NAME,
            messages=[
                {"role": "system", "content": "Sen bir toplantı analiz uzmanısın. Toplantı transkriptlerinden aksiyon maddelerini çıkarma konusunda uzmansın."},
                {"role": "user", "content": prompt}
            ],
            temperature=0.3,
            max_tokens=2000
        )
        return response.choices[0].message.content
    except Exception as e:
        print(f"LM Studio'dan yanıt alınırken hata oluştu: {e}")
        print("LM Studio'nun çalıştığından ve modelin yüklendiğinden emin olun.")
        return "Görev maddeleri çıkarılamadı."

def parse_lm_studio_to_todojson(lm_studio_text):
    tasks = []
    current_task = {}

    for line in lm_studio_text.splitlines():
        line = line.strip()
        # Görevi satırları esnek yakalama
        if line.lower().startswith("görevi:") or line.startswith("* **Görevi:**") or line.startswith("* Görevi:"):
            if current_task:
                tasks.append(current_task)
            description = line.replace("* **Görevi:**", "").replace("* Görevi:", "").replace("Görevi:", "").strip(" -–—:")
            current_task = {
                "title": description if len(description) <= 100 else description[:97] + "...",
                "description": description,
                "assignees": [],
                "created_date": "",
                "due_date": "",
                "status": "OPEN",
                "priority": "",
                "category": "",
                "tags": [],
                "subtasks": []
            }
        elif line.lower().startswith("kişi:") or line.startswith("* **Kişi:**") or line.startswith("* Kişi:"):
            if current_task:
                assignee = line.split(":")[1].strip()
                if assignee and assignee.lower() != "belirtilmemiş":
                    current_task["assignees"].append(assignee)
        elif line.lower().startswith("son tarih:") or line.startswith("* **Son Tarih:**") or line.startswith("* Son Tarih:"):
            if current_task:
                due_raw = line.split(":")[1].strip()
                if due_raw and due_raw.lower() != "belirtilmemiş":
                    due_iso = None
                    for fmt in ("%Y-%m-%d", "%d.%m.%Y", "%d/%m/%Y", "%d-%m-%Y", "%B %d, %Y", "%d.%m.%y"):
                        try:
                            dt = datetime.strptime(due_raw, fmt)
                            due_iso = dt.isoformat()
                            break
                        except ValueError:
                            continue
                    current_task["due_date"] = due_iso if due_iso else ""

    if current_task:
        tasks.append(current_task)

    return {"tasks": tasks}

def parse_lm_studio_to_todo_backend_api(lm_studio_text, category_id="00000000-0000-0000-0000-000000000000", user_id="00000000-0000-0000-0000-000000000000"):
    """LM Studio çıktısını todo backend API formatına dönüştür"""
    tasks = []
    current_task = {}

    for line in lm_studio_text.splitlines():
        line = line.strip()
        # Görevi satırları esnek yakalama
        if line.lower().startswith("görevi:") or line.startswith("* **Görevi:**") or line.startswith("* Görevi:"):
            if current_task:
                tasks.append(current_task)
            description = line.replace("* **Görevi:**", "").replace("* Görevi:", "").replace("Görevi:", "").strip(" -–—:")
            current_task = {
                "categoryId": category_id,
                "title": description if len(description) <= 100 else description[:97] + "...",
                "description": description,
                "isCompleted": False,
                "createdAt": datetime.now().isoformat(),
                "completedAt": None,
                "priority": "Medium",
                "repeatDays": [],
                "repeatType": "None"
            }
        elif line.lower().startswith("kişi:") or line.startswith("* **Kişi:**") or line.startswith("* Kişi:"):
            # Kişi bilgisi varsa priority'yi High yap
            if current_task:
                assignee = line.split(":")[1].strip()
                if assignee and assignee.lower() != "belirtilmemiş":
                    current_task["priority"] = "High"
        elif line.lower().startswith("son tarih:") or line.startswith("* **Son Tarih:**") or line.startswith("* Son Tarih:"):
            if current_task:
                due_raw = line.split(":")[1].strip()
                if due_raw and due_raw.lower() != "belirtilmemiş":
                    # Son tarih varsa priority'yi High yap
                    current_task["priority"] = "High"
                    # CompletedAt alanını due date olarak kullan (geçici çözüm)
                    for fmt in ("%Y-%m-%d", "%d.%m.%Y", "%d/%m/%Y", "%d-%m-%Y", "%B %d, %Y", "%d.%m.%y"):
                        try:
                            dt = datetime.strptime(due_raw, fmt)
                            # Due date'i description'a ekle
                            current_task["description"] += f" (Son Tarih: {dt.strftime('%d.%m.%Y')})"
                            break
                        except ValueError:
                            continue

    if current_task:
        tasks.append(current_task)

    return {"tasks": tasks}

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

def To_Do_main():
    print("LM Studio bağlantısı test ediliyor...")
    if not test_lm_studio_connection():
        exit()
    
    if not os.path.exists(TRANSCRIPT_INPUT_FILE):
        print(f"Hata: '{TRANSCRIPT_INPUT_FILE}' dosyası bulunamadı. Önce transcribe_audio.py dosyasını çalıştırın.")
        exit()

    print(f"'{TRANSCRIPT_INPUT_FILE}' dosyasından metin okunuyor...")
    try:
        with open(TRANSCRIPT_INPUT_FILE, "r", encoding="utf-8") as f:
            read_transcript_text = f.read()

        # Toplantı özeti çıkar
        meeting_summary = get_meeting_summary_with_lm_studio(read_transcript_text)
        
        print("\n--- Toplantı Özeti ---")
        print(meeting_summary)
        
        # Toplantı özetini dosyaya kaydet
        with open(MEETING_SUMMARY_FILE, "w", encoding="utf-8") as f_summary:
            f_summary.write(meeting_summary)
        print(f"Toplantı özeti '{MEETING_SUMMARY_FILE}' dosyasına kaydedildi.")
        
        # Aksiyon maddeleri çıkar
        action_items = get_action_items_with_lm_studio(read_transcript_text)

        print("\n--- Çıkarılan Aksiyon Maddeleri (Ham Metin) ---")
        print(action_items)

        # Ham aksiyon maddelerini dosyaya kaydet
        with open(ACTION_ITEMS_OUTPUT_FILE, "w", encoding="utf-8") as f_out:
            f_out.write(action_items)
        print(f"Ham aksiyon maddeleri '{ACTION_ITEMS_OUTPUT_FILE}' dosyasına kaydedildi.")

        # JSON formatına çevir (eski format)
        todo_json = parse_lm_studio_to_todojson(action_items)

        print("\n--- ToDo App Uyumlu JSON (Eski Format) ---")
        print(json.dumps(todo_json, indent=2, ensure_ascii=False))

        with open(ACTION_ITEMS_JSON_FILE, "w", encoding="utf-8") as f_json:
            json.dump(todo_json, f_json, ensure_ascii=False, indent=2)
        print(f"JSON formatındaki aksiyon maddeleri '{ACTION_ITEMS_JSON_FILE}' dosyasına kaydedildi.")

        # Todo Backend API formatına dönüştür
        todo_backend_json = parse_lm_studio_to_todo_backend_api(action_items)
        
        print("\n--- Todo Backend API Uyumlu JSON ---")
        print(json.dumps(todo_backend_json, indent=2, ensure_ascii=False))
        
        # Todo Backend API formatını ayrı dosyaya kaydet
        backend_json_file = "aksiyon_maddeleri_backend.json"
        with open(backend_json_file, "w", encoding="utf-8") as f_backend:
            json.dump(todo_backend_json, f_backend, ensure_ascii=False, indent=2)
        print(f"Backend API formatındaki aksiyon maddeleri '{backend_json_file}' dosyasına kaydedildi.")

        # Todo Backend API'ye gönder
        print("\n--- Todo Backend API'ye Gönderim ---")
        try:
            import requests
            
            # Todo Backend API server'ının çalışıp çalışmadığını kontrol et
            try:
                # Önce health check yapalım (eğer varsa)
                backend_base_url = "http://localhost:5142"  # Todo Backend API URL'i
                
                print(f"Todo Backend API'ye bağlanmaya çalışılıyor: {backend_base_url}")
                
                # Görevleri toplu olarak gönder
                try:
                    # Bulk endpoint için format (BulkTaskCreateDto)
                    bulk_data = {
                        "DefaultCategoryId": "01990c81-4b45-7b89-89d1-82b7d41059aa",
                        "Tasks": []
                    }
                    
                    # Her task'ı bulk format'a çevir
                    for task in todo_backend_json["tasks"]:
                        # Tarih formatını düzelt
                        created_date = task["createdAt"][:10] if task["createdAt"] else datetime.now().strftime("%Y-%m-%d")
                        due_date = task["completedAt"][:10] if task["completedAt"] else (datetime.now() + timedelta(days=7)).strftime("%Y-%m-%d")
                        
                        task_data = {
                            "Title": task["title"],
                            "Description": task["description"],
                            "Due_date": due_date,
                            "Status": "OPEN",
                            "Priority": task["priority"]
                        }
                        bulk_data["Tasks"].append(task_data)
                    
                    api_response = requests.post(
                        f"{backend_base_url}/api/Task/bulk",
                        json=bulk_data,
                        headers={"Content-Type": "application/json"},
                        timeout=10,
                        verify=False
                    )
                    
                    if api_response.status_code in [200, 201]:
                        print(f"✅ {len(bulk_data['Tasks'])} görev başarıyla eklendi!")
                        print(f"API Response: {api_response.text}")
                    else:
                        print(f"❌ Görevler eklenemedi (Status: {api_response.status_code})")
                        print(f"Hata: {api_response.text}")
                        
                except Exception as bulk_error:
                    print(f"❌ Bulk görev gönderim hatası: {bulk_error}")
                        
                print("📋 Todo Backend API'ye gönderim tamamlandı!")
                    
            except requests.exceptions.ConnectionError:
                print("⚠️ Todo Backend API server'ına bağlanılamadı")
                print("Server'ın çalıştığından emin olun.")
            except requests.exceptions.Timeout:
                print("⚠️ Todo Backend API server'ı yanıt vermiyor (timeout)")
                
        except ImportError:
            print("⚠️ requests kütüphanesi bulunamadı")
            print("Yüklemek için: pip install requests")
        except Exception as api_error:
            print(f"⚠️ Todo Backend API gönderim hatası: {api_error}")
    
    except Exception as e:
        print(f"Beklenmeyen hata oluştu: {e}")

if __name__ == "__main__":
    To_Do_main()