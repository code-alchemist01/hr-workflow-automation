from openai import OpenAI
import os
import json
from datetime import datetime, timedelta
import requests

# LM Studio local server configuration
LM_STUDIO_BASE_URL = "http://127.0.0.1:1234"
MODEL_NAME = "qwen/qwen3-4b-2507"

TRANSCRIPT_INPUT_FILE = "Tanınan Metin.txt"
MEETING_SUMMARY_FILE = "toplanti_ozeti.txt"
CLASSIFIED_OUTPUT_FILE = "siniflandirilmis_cikti.json"

# Backend configurations
TODO_BACKEND_URL = "http://localhost:5142"
NOTES_BACKEND_URL = "http://localhost:5258"
DEFAULT_CATEGORY_ID = "01990c81-4b45-7b89-89d1-82b7d41059aa"

# LM Studio client'ını yapılandır
client = OpenAI(
    base_url=LM_STUDIO_BASE_URL + "/v1",
    api_key="lm-studio"
)

def get_advanced_prompt():
    """Gelişmiş sınıflandırma prompt'unu döndür"""
    return """
[ROL]
Sen bir "Toplantı Akıl Katmanı"sın. Girdi olarak toplantı başlığı, tarih-zaman (Europe/Istanbul) ve ham transcript alırsın; çıktı olarak SADECE, ÖNCEDEN TANIMLI JSON şemasına %100 uyan bir nesne üretirsin: { "todos": [...], "notes": [...] }.

[HEDEF]
- Ham konuşmadan iki temiz liste çıkar: 
  1) todos → geleceğe dönük eylem, sorumlusu, zamanı/netliği olan maddeler. 
  2) notes → bilgi/karar/bağlam; eylem içermeyen veya eylemden bağımsız saklanması gereken içerik. Toplanti sırasında yapılan konuşmalarda yapılan notlar. 
- Fazla laf yok; sadece yalın JSON. 

[DEMİR KURAL]
- Backend sözleşmelerini ASLA bozma. Şema dışı alan ekleme.
- Markdown, açıklama, yorum, metin parçaları YOK. Yalnızca JSON.

[ZAMAN VE DİL]
- Transcript TR/EN karışık olabilir. Her ikisini de anla.
- Tüm tarih çözümlemelerinde varsayılan saat dilimi Europe/Istanbul (UTC+3).
- Göreli zamanları toplantı zamanını referans alarak ISO `YYYY-MM-DD`'e çevir. Çeviremiyorsan `null`.

[TODOLAR İÇİN EYLEM TESTİ (A/B/C)]
A) Eylem fiili var mı? (yap/başlat/bitir/gönder/planla/test et/araştır/yaz/optimize et/entegrasyon yap/incele/kontrol et… | EN: do/finish/send/plan/test/research/write/optimize/integrate/examine/check…)
B) Sahip/rol var mı? (Ömer/Oğuz/tasarım/backend/growth/ops/PM/QA…)
C) Zaman/teslim netliği var mı? (YYYY-MM-DD, yarın, haftaya pazartesi, bu akşam, EOD, Cuma 14:00, Q3, sprint-35 vb.)

- A + (B veya C) → kesin todo.
- Sadece A varsa bile todo olarak kabul et (confidence düşük ama dahil et).
- "Araştır", "incele", "kontrol et", "dene", "test et", "bak", "öğren" gibi TÜM eylemler için B/C şartını tamamen kaldır.
- Herhangi bir eylem fiili varsa todo'dur, sahip/tarih belirsiz olsa bile.
- Hem bilgi hem eylem içeriyorsa TODO önceliklidir; bilgi kısmını todo `description`/`meta` içinde özetle.

[NOTLAR İÇİN KAPSAM - GENİŞLETİLMİŞ]
- Kararlar, metrik sonuçları, gözlemler, gerekçeler, riskler, tartışma özleri.
- Teknik bilgiler, sistem açıklamaları, süreç tanımları, metodoloji bilgileri.
- Deneyim paylaşımları, öğrenilen dersler, best practice'ler, ipuçları.
- Durum güncellemeleri, proje durumları, mevcut çalışmalar hakkında bilgiler.
- Araç/teknoloji tanıtımları, karşılaştırmalar, öneriler.
- Ekip üyelerinin uzmanlık alanları, yetenekleri, deneyimleri.
- Sorun tanımları, hata raporları, bug açıklamaları (çözüm eylemi ayrı todo).
- Müşteri geri bildirimleri, kullanıcı yorumları, pazar bilgileri.
- Toplantı sırasında paylaşılan linkler, kaynaklar, referanslar.
- "Şu anda kullanıyoruz", "elimizde var", "mevcut durum" gibi bilgiler.
- "Tamamlandı" bildirimi eğer yeni eylem içermiyorsa not olur.
- Şirket politikaları, kurallar, prosedürler hakkında bilgiler.
- Geçmiş deneyimler, önceki projelerden çıkarılan sonuçlar.
- Herhangi bir "bilmeye değer" içerik, gelecekte referans olabilecek bilgiler.
- Linkler/dosyalar/araçlar → not veya ilgili todo `meta.dependencies`.

[STATUS HARİTASI]
- "planlandı, yapılacak, başlayacağız, üzerinde çalışılacak" → `planned`
- "devam ediyor, üzerinde çalışıyorum, yapıyorum, ele alıyorum" → `in_progress`
- "bitti, tamamlandı, kapandı, deploy edildi" → `done` (eylem yoksa notta sonuç bilgisi)

[OWNER ÇIKARIMI]
- Açık kişi/rol geçiyorsa onu kullan. "ekip/takım" varsa uygun rol adı yaz (ör. "tasarım").
- Hiçbiri yoksa `"owner":"unspecified"`. Asla uydurma kişi üretme.

[TARİH KURALLARI]
- Göreli → mutlak: "yarın", "Cuma", "haftaya pazartesi", "bu akşam", "EOD" vb. Toplantı tarihinden hesapla.
- Ay/gün isimleri TR/EN algılanır.
- Belirsizse `due_date=null`. Uydurma yok.
- Saat verildiyse tarihi yine `YYYY-MM-DD` olarak yaz; saat bilgisini açıklamaya eklemek istersen `description`'da geç.

[ETİKET/TAG]
- Belirginse kısa etiketler: ["backend","design","infra","ops","growth","ads","content","copy","analytics","security","mobile","web"] vb.
- Zorunlu değil.

[DEDUP ve BİRLEŞTİRME]
- Aynı işi anlatan tekrarları birleştir:
  - En net tarih/sahip/statü kalsın.
  - Açıklamaları tek `description` içinde birleştir.
  - Kaynak alıntısında en temsilî cümleyi tut.
- Parça parça ilerleyen tek görevi ayrı ayrı ekleme; tek todo yap.

[CONFIDENCE KALİBRASYONU]
- 0.95–1.00: açık eylem + net sahip + net tarih/bağlam
- 0.80–0.94: eylem + (sahip veya tarih) net; bağlam orta
- 0.60–0.79: eylem var ama sahip/tarih belirsiz
- 0.40–0.59: belirsiz ama potansiyel değerli içerik (dahil et)
- 0.30–0.39: zayıf bağlam ama değerli olabilir (dahil et)
- <0.30: gerçekten çöp (reddet)

[KAPSAYICILIK İLKESİ]
- Belirsizlik durumunda MUTLAKA dahil etme yönünde karar ver.
- "Yapılabilir" her şeyi todo, "bilinmeye değer" her şeyi note olarak değerlendir.
- Minimum 10-15 öğe hedefle (çok az çıkarma yasak), bunun 5-8'i note olsun.
- "Belki todo, belki note" durumunda todo olarak sınıflandır.
- Şüpheli durumlarda confidence düşük ver ama mutlaka dahil et.
- "Araştır", "bak", "kontrol et", "dene", "test et" gibi tüm eylem ifadeleri todo'dur.
- NOT KAPSAYICILIĞI: Herhangi bir bilgi paylaşımı, açıklama, deneyim aktarımı not olabilir.
- "Bu şekilde yapıyoruz", "şu araç var", "böyle çalışıyor" gibi mevcut durum bilgileri kesinlikle not.
- Teknik detaylar, sistem bilgileri, süreç açıklamaları hep not olarak değerlendir.
- Kişisel deneyimler, önceki projeler, öğrenilen şeyler mutlaka not olarak kaydet.

[ÇÖP/SOHBET FİLTRESİ]
- Sadece gerçek teknik sorunları filtrele: "duyuyor musun, bekle, tekrar alayım, ses gitti" gibi cümleleri at.
- Değerli olabilecek belirsiz içerikleri filtreleme.

[JSON ŞEMASI — SADECE BUNU DÖNDÜR]
{
  "todos": [
    {
      "title": "kısa eylem başlığı",
      "description": "gerekirse 1-2 cümle bağlam (saat gibi detaylar buraya eklenebilir)",
      "owner": "Ömer | Oğuz | tasarım | backend | growth | ops | unspecified",
      "status": "planned | in_progress | done",
      "due_date": "YYYY-MM-DD or null",
      "meta": {
        "source_snippet": "ham metinden kısa alıntı",
        "dependencies": ["opsiyonel"],
        "tags": ["opsiyonel"]
      },
      "confidence": 0.0
    }
  ],
  "notes": [
    {
      "title": "kısa bilgi başlığı",
      "content": "öz/karar/gerekçe/bulgu; net, kısa",
      "meta": {
        "source_snippet": "ham metinden kısa alıntı",
        "tags": ["opsiyonel"]
      },
      "confidence": 0.0
    }
  ]
}

[FORMAT KİLİDİ]
- Çıktı tam olarak `{` ile başlar `}` ile biter.
- Virgül, tırnak ve köşeli parantezler JSON standardına uygun olmalı.
- Boş listeler serbest: `"todos":[]`, `"notes":[]`.
- Null için `null` (tırnaksız) kullan; `"null"` yazma.
- Trailing comma YOK.

[ÖNLEYİCİ KURALLAR]
- İsim uydurma, kurum uydurma, tarih atama yok.
- "Yapılmış, bitti" cümlesi geleceğe dönük eylem içermiyorsa note.
- Tekrarlayan "hatırlatma" cümlelerini tek todo altında topla.
- Markaya özgü jargonları aynen koru.

[ÇIKTI]
- SADECE yukarıdaki JSON şemasına uyan tek bir JSON nesnesi üret.
- Açıklama, markdown, metin YOK.

[HATIRLATMA]
- Backend ve DB bağlantılarını KESİNLİKLE bozmayacak şekilde, yalnızca şemaya uygun JSON döndür. 
- Şüphede kalırsan şunları uygula: kısa başlık, sağlam snippet, `owner` çıkar, tarih çözemiyorsan `null`, confidence'ı düşür, todo-note ayrımını A/B/C testiyle netleştir.
"""

def classify_content_with_lm_studio(transcript_text, meeting_title="Toplantı", meeting_datetime=None):
    """Gelişmiş prompt ile içeriği sınıflandır"""
    if not transcript_text.strip() or transcript_text == "Metin bulunamadı":
        return {"todos": [], "notes": []}

    if meeting_datetime is None:
        meeting_datetime = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    prompt = f"""
meeting_title: "{meeting_title}"
meeting_datetime_tz: "{meeting_datetime} Europe/Istanbul"
transcript:
{transcript_text}
"""

    system_prompt = get_advanced_prompt()

    print("\nLM Studio'ya gelişmiş sınıflandırma isteği gönderiliyor...")
    try:
        response = client.chat.completions.create(
            model=MODEL_NAME,
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": prompt}
            ],
            temperature=0.2,
            max_tokens=3000
        )
        
        response_text = response.choices[0].message.content.strip()
        
        # JSON parse et
        try:
            classified_data = json.loads(response_text)
            return classified_data
        except json.JSONDecodeError as e:
            print(f"JSON parse hatası: {e}")
            print(f"Ham yanıt: {response_text}")
            return {"todos": [], "notes": []}
            
    except Exception as e:
        print(f"LM Studio'dan yanıt alınırken hata oluştu: {e}")
        return {"todos": [], "notes": []}

def convert_todos_to_backend_format(todos):
    """Todo'ları backend API formatına çevir"""
    backend_tasks = []
    
    for todo in todos:
        # Status mapping
        status_map = {
            "planned": "OPEN",
            "in_progress": "IN_PROGRESS", 
            "done": "COMPLETED"
        }
        
        # Priority mapping based on confidence and other factors
        priority = "Medium"
        if todo.get("confidence", 0) >= 0.9:
            priority = "High"
        elif todo.get("due_date") and todo["due_date"] != "null":
            priority = "High"
        elif todo.get("owner") and todo["owner"] != "unspecified":
            priority = "High"
        
        # Due date processing
        due_date = None
        if todo.get("due_date") and todo["due_date"] != "null":
            due_date = todo["due_date"]
        else:
            # Default to 7 days from now
            due_date = (datetime.now() + timedelta(days=7)).strftime("%Y-%m-%d")
        
        task_data = {
            "Title": todo.get("title", ""),
            "Description": todo.get("description", ""),
            "Due_date": due_date,
            "Status": status_map.get(todo.get("status", "planned"), "OPEN"),
            "Priority": priority,
        }
        
        backend_tasks.append(task_data)
    
    return {
        "DefaultCategoryId": DEFAULT_CATEGORY_ID,
        "Tasks": backend_tasks
    }

def convert_notes_to_backend_format(notes):
    """Note'ları backend API formatına çevir"""
    backend_notes = []
    
    for note in notes:
        note_data = {
            "title": note.get("title", ""),
            "content": note.get("content", ""),
            "color": None,
            "folderId": None,
            "tagIds": []
        }
        
        backend_notes.append(note_data)
    
    return backend_notes

def send_todos_to_backend(todos_data):
    """Todo'ları backend'e gönder"""
    if not todos_data["Tasks"]:
        print("📋 Gönderilecek todo bulunamadı.")
        return True
        
    try:
        print(f"Todo Backend API'ye bağlanmaya çalışılıyor: {TODO_BACKEND_URL}")
        
        api_response = requests.post(
            f"{TODO_BACKEND_URL}/api/Task/bulk",
            json=todos_data,
            headers={"Content-Type": "application/json"},
            timeout=10,
            verify=False
        )
        
        if api_response.status_code in [200, 201]:
            print(f"✅ {len(todos_data['Tasks'])} todo başarıyla eklendi!")
            return True
        else:
            print(f"❌ Todo'lar eklenemedi (Status: {api_response.status_code})")
            print(f"Hata: {api_response.text}")
            return False
            
    except requests.exceptions.ConnectionError:
        print("⚠️ Todo Backend API server'ına bağlanılamadı")
        return False
    except Exception as e:
        print(f"❌ Todo gönderim hatası: {e}")
        return False

def send_notes_to_backend(notes_data):
    """Note'ları backend'e gönder"""
    if not notes_data:
        print("📝 Gönderilecek not bulunamadı.")
        return True
        
    try:
        print(f"Notes Backend API'ye bağlanmaya çalışılıyor: {NOTES_BACKEND_URL}")
        
        success_count = 0
        for note in notes_data:
            try:
                api_response = requests.post(
                    f"{NOTES_BACKEND_URL}/api/Note",
                    json=note,
                    headers={"Content-Type": "application/json"},
                    timeout=10
                )
                
                if api_response.status_code in [200, 201]:
                    success_count += 1
                    print(f"✅ Not başarıyla eklendi: {note['title']}")
                else:
                    print(f"❌ Not eklenemedi: {note['title']} (Status: {api_response.status_code})")
                    
            except Exception as note_error:
                print(f"❌ Not gönderim hatası ({note['title']}): {note_error}")
        
        print(f"📝 Notes Backend API'ye gönderim tamamlandı! {success_count}/{len(notes_data)} not başarıyla eklendi.")
        return success_count > 0
        
    except requests.exceptions.ConnectionError:
        print("⚠️ Notes Backend API sunucusuna bağlanılamadı.")
        return False
    except Exception as e:
        print(f"❌ Notes gönderim hatası: {e}")
        return False

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
        print(f"2. {MODEL_NAME} modelinin yüklendiğini")
        print(f"3. Local server'ın başlatıldığını ({LM_STUDIO_BASE_URL})")
        return False

def get_meeting_summary_with_lm_studio(transcript_text):
    """Toplantı özeti çıkar (eski fonksiyon, uyumluluk için)"""
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

def smart_integration_main():
    """Ana entegrasyon fonksiyonu"""
    print("🤖 Akıllı Toplantı Entegrasyon Sistemi Başlatılıyor...")
    print("=" * 60)
    
    # LM Studio bağlantısını test et
    print("LM Studio bağlantısı test ediliyor...")
    if not test_lm_studio_connection():
        exit(1)
    
    # Transcript dosyasını kontrol et
    if not os.path.exists(TRANSCRIPT_INPUT_FILE):
        print(f"Hata: '{TRANSCRIPT_INPUT_FILE}' dosyası bulunamadı.")
        print("Önce transcribe_audio.py dosyasını çalıştırın.")
        exit(1)

    # Transcript'i oku
    print(f"\n'{TRANSCRIPT_INPUT_FILE}' dosyasından metin okunuyor...")
    try:
        with open(TRANSCRIPT_INPUT_FILE, "r", encoding="utf-8") as f:
            transcript_text = f.read()

        # Toplantı özeti çıkar (eski uyumluluk için)
        print("\n📋 Toplantı özeti çıkarılıyor...")
        meeting_summary = get_meeting_summary_with_lm_studio(transcript_text)
        
        with open(MEETING_SUMMARY_FILE, "w", encoding="utf-8") as f_summary:
            f_summary.write(meeting_summary)
        print(f"Toplantı özeti '{MEETING_SUMMARY_FILE}' dosyasına kaydedildi.")
        
        # Gelişmiş sınıflandırma yap
        print("\n🧠 Gelişmiş AI sınıflandırması yapılıyor...")
        classified_data = classify_content_with_lm_studio(
            transcript_text,
            meeting_title="Toplantı",
            meeting_datetime=datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        )
        
        print("\n--- Sınıflandırma Sonuçları ---")
        print(f"📋 {len(classified_data.get('todos', []))} todo bulundu")
        print(f"📝 {len(classified_data.get('notes', []))} not bulundu")
        
        # Sınıflandırılmış veriyi kaydet
        with open(CLASSIFIED_OUTPUT_FILE, "w", encoding="utf-8") as f_classified:
            json.dump(classified_data, f_classified, ensure_ascii=False, indent=2)
        print(f"\nSınıflandırılmış veriler '{CLASSIFIED_OUTPUT_FILE}' dosyasına kaydedildi.")
        
        # Todo'ları backend formatına çevir ve gönder
        if classified_data.get('todos'):
            print("\n📋 Todo'lar backend formatına çevriliyor...")
            todos_backend_data = convert_todos_to_backend_format(classified_data['todos'])
            
            print("\n--- Todo Backend API'ye Gönderim ---")
            send_todos_to_backend(todos_backend_data)
        
        # Note'ları backend formatına çevir ve gönder
        if classified_data.get('notes'):
            print("\n📝 Notlar backend formatına çevriliyor...")
            notes_backend_data = convert_notes_to_backend_format(classified_data['notes'])
            
            print("\n--- Notes Backend API'ye Gönderim ---")
            send_notes_to_backend(notes_backend_data)
        
        print("\n🎉 Akıllı entegrasyon tamamlandı!")
        print("=" * 60)
        
    except Exception as e:
        print(f"Beklenmeyen hata oluştu: {e}")
        exit(1)

if __name__ == "__main__":
    smart_integration_main()