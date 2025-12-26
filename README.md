# Intellium - Kapsamlı İş Zekası ve Yönetim Platformu

## 📋 İçindekiler

- [Genel Bakış](#genel-bakış)
- [Proje Yapısı](#proje-yapısı)
- [Sistem Gereksinimleri](#sistem-gereksinimleri)
- [Kurulum](#kurulum)
  - [IIAS (Mülakat Analiz Sistemi)](#iias-mülakat-analiz-sistemi)
  - [ToDo Backend](#todo-backend)
  - [ToDo Frontend](#todo-frontend)
  - [Notes Backend](#notes-backend)
  - [Integration Scripts](#integration-scripts)
- [Kullanım](#kullanım)
- [API Dokümantasyonu](#api-dokümantasyonu)
- [Yapılandırma](#yapılandırma)
- [Sorun Giderme](#sorun-giderme)
- [Katkıda Bulunma](#katkıda-bulunma)

---

## 🎯 Genel Bakış

**Intellium**, iş süreçlerini otomatikleştiren ve analiz eden kapsamlı bir platformdur. Sistem şu ana bileşenlerden oluşur:

1. **IIAS (Intelligent Interview Analysis System)**: Video mülakatları analiz eden, transkript çıkaran, duygu analizi yapan ve detaylı raporlar oluşturan AI destekli sistem
2. **ToDo Management System**: Görev yönetimi için tam özellikli bir uygulama (Backend + Frontend)
3. **Notes Management System**: Not yönetimi için RESTful API tabanlı backend
4. **Integration Layer**: Toplantı kayıtlarını otomatik olarak todo ve note'lara dönüştüren entegrasyon katmanı

### 🚀 Temel Özellikler

- **AI Destekli Mülakat Analizi**: Video mülakatlarından otomatik transkript, duygu analizi ve kapsamlı raporlar
- **Akıllı İçerik Sınıflandırması**: Toplantı transkriptlerinden otomatik todo ve note çıkarma
- **Görev Yönetimi**: Kategori, öncelik, tarih ve durum yönetimi ile tam özellikli todo sistemi
- **Not Yönetimi**: Klasör, etiket, hatırlatıcı ve paylaşım özellikleri ile not sistemi
- **Gerçek Zamanlı Entegrasyon**: Toplantı kayıtlarının otomatik olarak todo ve note sistemlerine aktarılması

---

## 📁 Proje Yapısı

```
Intellium Full/
│
├── IIAS-main/                          # Mülakat Analiz Sistemi
│   ├── IIAS.py                         # Ana analiz scripti
│   ├── requirements.txt                # Python bağımlılıkları
│   ├── video1723838072.mp4             # Örnek video dosyası
│   ├── mulakat_transkripti.docx        # Çıktı: Transkript
│   └── analiz_sonucu.docx              # Çıktı: Analiz raporu
│
├── to do/                              # ToDo Uygulaması
│   ├── todo-backend/                   # .NET Core Backend
│   │   ├── ToDoList.Api/               # API katmanı
│   │   ├── ToDoList.AppLogic/          # İş mantığı
│   │   ├── ToDoList.Domain/            # Domain modelleri
│   │   └── ToDoList.Infrastructure/    # Veritabanı ve repository
│   │
│   └── todo-frontend/                  # React Frontend
│       ├── src/
│       │   ├── components/             # UI bileşenleri
│       │   ├── pages/                   # Sayfa bileşenleri
│       │   └── App.tsx                  # Ana uygulama
│       └── package.json
│
├── portal-notes-backend-clone-main/    # Notes Backend
│   └── portal-notes-backend/
│       └── NoteApp/
│           ├── NoteApp.WebApi/         # API katmanı
│           ├── NoteApp.Business/       # İş mantığı
│           ├── NoteApp.DataAccess/     # Veritabanı erişimi
│           └── NoteApp.Test/           # Test projesi
│
└── Integration/                        # Entegrasyon Scriptleri
    ├── extract_audio.py                 # Video'dan ses çıkarma
    ├── transcribe_audio.py             # Ses'ten metin çıkarma
    ├── smart_integration.py            # Akıllı sınıflandırma ve entegrasyon
    ├── INotes_integration.py           # Notes entegrasyonu
    ├── To_Do_integration.py            # Todo entegrasyonu
    └── INotes_ve_ToDo.py               # Kombine entegrasyon
```

---

## 💻 Sistem Gereksinimleri

### Genel Gereksinimler

- **İşletim Sistemi**: Windows 10/11, Linux, macOS
- **RAM**: Minimum 8GB (16GB önerilir)
- **Disk Alanı**: Minimum 10GB boş alan
- **İnternet Bağlantısı**: API çağrıları için gerekli

### IIAS için

- **Python**: 3.8 veya üzeri
- **FFmpeg**: Video işleme için
- **LM Studio**: Yerel LLM sunucusu (opsiyonel, önerilir)
- **API Anahtarları**:
  - ElevenLabs API Key (Speech-to-Text için)
  - Google Gemini API Key (Görsel analiz için)

### Backend için

- **.NET SDK**: 8.0 veya üzeri
- **PostgreSQL**: 14 veya üzeri
- **Node.js**: 18 veya üzeri (Frontend için)

### Frontend için

- **Node.js**: 18 veya üzeri
- **npm** veya **yarn**

---

## 🛠️ Kurulum

### 1. IIAS (Mülakat Analiz Sistemi)

#### Adım 1: Python Ortamını Hazırlama

```bash
# Python sanal ortamı oluştur
python -m venv venv

# Windows
venv\Scripts\activate

# Linux/macOS
source venv/bin/activate
```

#### Adım 2: Bağımlılıkları Yükleme

```bash
cd IIAS-main
pip install -r requirements.txt

# spaCy modelini yükle
python -m spacy download xx_ent_wiki_sm
```

#### Adım 3: FFmpeg Kurulumu

**Windows:**
1. [FFmpeg](https://ffmpeg.org/download.html) indirin
2. ZIP dosyasını çıkarın
3. `extract_audio.py` dosyasındaki path'i güncelleyin:
   ```python
   os.environ["PATH"] += os.pathsep + r"C:/path/to/ffmpeg/bin"
   ```

**Linux:**
```bash
sudo apt-get update
sudo apt-get install ffmpeg
```

**macOS:**
```bash
brew install ffmpeg
```

#### Adım 4: LM Studio Kurulumu (Opsiyonel ama Önerilir)

1. [LM Studio](https://lmstudio.ai/) indirin ve kurun
2. LM Studio'yu açın
3. Model indirin (ör: `qwen/qwen3-4b-2507`)
4. Local Server'ı başlatın (varsayılan port: 1234)

#### Adım 5: API Anahtarlarını Yapılandırma

`IIAS.py` dosyasını açın ve API anahtarlarını güncelleyin:

```python
ELEVENLABS_API_KEY = "your_elevenlabs_api_key_here"
GEMINI_API_KEY = "your_gemini_api_key_here"
```

#### Adım 6: Video Dosyasını Yerleştirme

Analiz edilecek video dosyasını `IIAS-main` klasörüne koyun ve `IIAS.py` içinde dosya adını güncelleyin:

```python
INPUT_VIDEO_FILE = "your_video.mp4"
```

### 2. ToDo Backend

#### Adım 1: PostgreSQL Veritabanı Kurulumu

```bash
# PostgreSQL'i başlat
# Windows: Services'ten başlatın
# Linux:
sudo systemctl start postgresql
sudo systemctl enable postgresql

# Veritabanı oluştur
psql -U postgres
CREATE DATABASE ToDoListDb;
\q
```

#### Adım 2: Connection String Yapılandırması

`to do/todo-backend/ToDoList.Api/appsettings.json` dosyasını açın:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ToDoListDb;Username=postgres;Password=your_password;"
  }
}
```

#### Adım 3: Projeyi Çalıştırma

```bash
cd "to do/todo-backend/ToDoList.Api"
dotnet restore
dotnet ef database update  # Migration'ları uygula
dotnet run
```

Backend `http://localhost:5142` adresinde çalışacaktır.

**Swagger UI**: `http://localhost:5142/swagger`

### 3. ToDo Frontend

#### Adım 1: Bağımlılıkları Yükleme

```bash
cd "to do/todo-frontend"
npm install
```

#### Adım 2: API Endpoint Yapılandırması

Backend URL'ini yapılandırın (gerekirse `src` klasöründeki API dosyalarını kontrol edin).

#### Adım 3: Geliştirme Sunucusunu Başlatma

```bash
npm run dev
```

Frontend `http://localhost:5173` (veya Vite'ın belirlediği port) adresinde çalışacaktır.

### 4. Notes Backend

#### Adım 1: PostgreSQL Veritabanı Kurulumu

```bash
psql -U postgres
CREATE DATABASE noteapp;
\q
```

#### Adım 2: Connection String ve JWT Yapılandırması

`portal-notes-backend-clone-main/portal-notes-backend/NoteApp/NoteApp.WebApi/appsettings.json` dosyasını açın:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=noteapp;Username=postgres;Password=your_password"
  },
  "JwtSettings": {
    "SecretKey": "your_super_secret_key_that_is_at_least_32_characters_long_1234567890!@#$%^&*()",
    "Issuer": "NoteApp",
    "Audience": "NoteAppUsers",
    "ExpiresInMinutes": 60
  }
}
```

#### Adım 3: Projeyi Çalıştırma

```bash
cd "portal-notes-backend-clone-main/portal-notes-backend/NoteApp/NoteApp.WebApi"
dotnet restore
dotnet ef database update
dotnet run
```

Backend `http://localhost:5258` adresinde çalışacaktır.

**Swagger UI**: `http://localhost:5258` (kök URL)

### 5. Integration Scripts

#### Adım 1: Python Bağımlılıklarını Yükleme

```bash
cd Integration
pip install openai requests moviepy python-docx
```

#### Adım 2: LM Studio Yapılandırması

Integration scriptlerinde LM Studio kullanılıyorsa, LM Studio'nun çalıştığından emin olun:

```bash
# LM Studio'yu açın ve local server'ı başlatın
# Varsayılan: http://127.0.0.1:1234
```

#### Adım 3: Backend URL'lerini Yapılandırma

`smart_integration.py` dosyasını açın ve backend URL'lerini kontrol edin:

```python
TODO_BACKEND_URL = "http://localhost:5142"
NOTES_BACKEND_URL = "http://localhost:5258"
```

---

## 📖 Kullanım

### IIAS ile Mülakat Analizi

1. **Video Dosyasını Hazırlama**
   - Video dosyasını `IIAS-main` klasörüne koyun
   - `IIAS.py` içinde `INPUT_VIDEO_FILE` değişkenini güncelleyin

2. **Analizi Başlatma**

```bash
cd IIAS-main
python IIAS.py
```

3. **Çıktılar**
   - `mulakat_transkripti.docx`: Konuşmacı ayrımlı transkript
   - `analiz_sonucu.docx`: Kapsamlı analiz raporu
   - `temp_face.jpg`: Çıkarılan yüz görüntüsü

### ToDo Uygulaması

1. **Backend'i Başlatma**
   ```bash
   cd "to do/todo-backend/ToDoList.Api"
   dotnet run
   ```

2. **Frontend'i Başlatma**
   ```bash
   cd "to do/todo-frontend"
   npm run dev
   ```

3. **Kullanım**
   - Tarayıcıda `http://localhost:5173` adresine gidin
   - Kayıt olun veya giriş yapın
   - Görevler oluşturun, düzenleyin ve yönetin

### Notes Backend

1. **Backend'i Başlatma**
   ```bash
   cd "portal-notes-backend-clone-main/portal-notes-backend/NoteApp/NoteApp.WebApi"
   dotnet run
   ```

2. **API Kullanımı**
   - Swagger UI: `http://localhost:5258`
   - JWT token ile kimlik doğrulama yapın
   - Notlar oluşturun, düzenleyin ve yönetin

### Integration Workflow

1. **Video'dan Ses Çıkarma**
   ```bash
   cd Integration
   python extract_audio.py
   ```

2. **Ses'ten Metin Çıkarma**
   ```bash
   python transcribe_audio.py
   # Çıktı: Tanınan Metin.txt
   ```

3. **Akıllı Sınıflandırma ve Entegrasyon**
   ```bash
   python smart_integration.py
   # Otomatik olarak todo ve note'ları backend'lere gönderir
   ```

4. **Ayrı Entegrasyonlar**
   ```bash
   # Sadece Notes
   python INotes_integration.py
   
   # Sadece ToDo
   python To_Do_integration.py
   
   # Her ikisi birden
   python INotes_ve_ToDo.py
   ```

---

## 🔌 API Dokümantasyonu

### ToDo Backend API

**Base URL**: `http://localhost:5142`

#### Authentication
- `POST /api/Auth/Register` - Kullanıcı kaydı
- `POST /api/Auth/Login` - Kullanıcı girişi

#### Tasks
- `GET /api/Task` - Tüm görevleri getir
- `GET /api/Task/{id}` - Görev detayı
- `POST /api/Task` - Yeni görev oluştur
- `POST /api/Task/bulk` - Toplu görev oluştur
- `PUT /api/Task/{id}` - Görev güncelle
- `DELETE /api/Task/{id}` - Görev sil

#### Categories
- `GET /api/Category` - Tüm kategorileri getir
- `POST /api/Category` - Yeni kategori oluştur

**Swagger**: `http://localhost:5142/swagger`

### Notes Backend API

**Base URL**: `http://localhost:5258`

#### Authentication
- `POST /api/Auth/Register` - Kullanıcı kaydı
- `POST /api/Auth/Login` - Kullanıcı girişi

#### Notes
- `GET /api/Note` - Tüm notları getir (Auth gerekli)
- `GET /api/Note/{id}` - Not detayı
- `POST /api/Note` - Yeni not oluştur (AllowAnonymous)
- `PUT /api/Note/{id}` - Not güncelle
- `DELETE /api/Note/{id}` - Not sil
- `GET /api/Note/me` - Kullanıcının notları

#### Folders
- `GET /api/Folder` - Tüm klasörleri getir
- `POST /api/Folder` - Yeni klasör oluştur

**Swagger**: `http://localhost:5258`

---

## ⚙️ Yapılandırma

### IIAS Yapılandırması

`IIAS.py` dosyasındaki önemli değişkenler:

```python
# API Anahtarları
ELEVENLABS_API_KEY = "your_key"
GEMINI_API_KEY = "your_key"

# Dosya Yolları
INPUT_VIDEO_FILE = "video.mp4"
TRANSCRIPT_DOCX_FILE = "mulakat_transkripti.docx"
FINAL_ANALYSIS_TXT_FILE = "analiz_sonucu.docx"

# Analiz Ayarları
SANIYEDE_ANALIZ_SAYISI = 2  # Duygu analizi sıklığı
DURATION_THRESHOLD = 20     # Chunk analizi için eşik (dakika)

# LM Studio
LM_STUDIO_API_URL = "http://localhost:1234/v1/chat/completions"
MODEL_NAME = "qwen/qwen3-4b-2507"
```

### Integration Yapılandırması

`smart_integration.py` dosyasındaki ayarlar:

```python
# LM Studio
LM_STUDIO_BASE_URL = "http://127.0.0.1:1234"
MODEL_NAME = "qwen/qwen3-4b-2507"

# Backend URL'leri
TODO_BACKEND_URL = "http://localhost:5142"
NOTES_BACKEND_URL = "http://localhost:5258"
DEFAULT_CATEGORY_ID = "01990c81-4b45-7b89-89d1-82b7d41059aa"

# Dosya Yolları
TRANSCRIPT_INPUT_FILE = "Tanınan Metin.txt"
```

### Veritabanı Yapılandırması

**ToDo Backend** (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ToDoListDb;Username=postgres;Password=123456;"
  }
}
```

**Notes Backend** (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=noteapp;Username=postgres;Password=123456"
  },
  "JwtSettings": {
    "SecretKey": "your_secret_key_min_32_chars",
    "Issuer": "NoteApp",
    "Audience": "NoteAppUsers",
    "ExpiresInMinutes": 60
  }
}
```

---

## 🔧 Sorun Giderme

### IIAS Sorunları

**Problem**: Video işlenemiyor
- **Çözüm**: FFmpeg'in kurulu ve PATH'te olduğundan emin olun

**Problem**: ElevenLabs API hatası
- **Çözüm**: API anahtarının geçerli olduğunu ve kredilerin yeterli olduğunu kontrol edin

**Problem**: LM Studio bağlantı hatası
- **Çözüm**: 
  1. LM Studio'nun açık olduğundan emin olun
  2. Local server'ın başlatıldığını kontrol edin
  3. Model'in yüklü olduğunu doğrulayın

**Problem**: DeepFace yükleme hatası
- **Çözüm**: TensorFlow ve gerekli bağımlılıkların yüklü olduğundan emin olun

### Backend Sorunları

**Problem**: Veritabanı bağlantı hatası
- **Çözüm**: 
  1. PostgreSQL'in çalıştığını kontrol edin
  2. Connection string'i doğrulayın
  3. Veritabanının oluşturulduğundan emin olun

**Problem**: Migration hatası
- **Çözüm**: 
  ```bash
  dotnet ef migrations add InitialCreate
  dotnet ef database update
  ```

**Problem**: Port zaten kullanımda
- **Çözüm**: `launchSettings.json` dosyasında port numarasını değiştirin

### Frontend Sorunları

**Problem**: API bağlantı hatası
- **Çözüm**: Backend'in çalıştığını ve CORS ayarlarının doğru olduğunu kontrol edin

**Problem**: Bağımlılık hataları
- **Çözüm**: 
  ```bash
  rm -rf node_modules package-lock.json
  npm install
  ```

### Integration Sorunları

**Problem**: Backend'e veri gönderilemiyor
- **Çözüm**: 
  1. Backend'lerin çalıştığını kontrol edin
  2. URL'lerin doğru olduğunu doğrulayın
  3. Network bağlantısını kontrol edin

**Problem**: LM Studio yanıt vermiyor
- **Çözüm**: LM Studio'nun çalıştığını ve model'in yüklü olduğunu kontrol edin

---

## 🎓 Özellik Detayları

### IIAS Analiz Bileşenleri

1. **Video İşleme**
   - Video kalitesi değerlendirmesi
   - Frame bazlı duygu analizi
   - Yüz tespiti ve çıkarma

2. **Ses İşleme**
   - Video'dan ses çıkarma
   - ElevenLabs ile konuşmacı ayrımlı transkript
   - Duygu-zaman çizelgesi oluşturma

3. **Metin Analizi**
   - LLM ile puanlama tablosu
   - Recruiter notu
   - Soru-cevap analizi
   - Teknik yetkinlik değerlendirmesi
   - Soft skill analizi

4. **Görsel Analiz**
   - Gemini AI ile profesyonel görünüm analizi
   - Beden dili değerlendirmesi
   - Yüz ifadesi analizi

### Integration Akıllı Sınıflandırma

1. **İçerik Analizi**
   - Eylem fiilleri tespiti
   - Sahip/rol çıkarımı
   - Tarih çözümlemesi
   - Confidence skorlama

2. **Sınıflandırma**
   - Todo/Note ayrımı
   - Status belirleme (planned/in_progress/done)
   - Priority hesaplama
   - Tag önerileri

3. **Backend Entegrasyonu**
   - Otomatik format dönüşümü
   - Bulk gönderim
   - Hata yönetimi

---

## 📝 Notlar

- **API Anahtarları**: Tüm API anahtarlarını güvenli bir şekilde saklayın ve asla commit etmeyin
- **Veritabanı Şifreleri**: Production ortamında güçlü şifreler kullanın
- **LM Studio**: Yerel LLM kullanımı için yeterli RAM gerekir (en az 8GB önerilir)
- **Video Formatları**: IIAS, MP4 formatını destekler
- **Dil Desteği**: Sistem Türkçe ve İngilizce transkriptleri destekler

---

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'Add amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

---

## 📄 Lisans

Bu proje özel bir projedir. Tüm hakları saklıdır.

---

## 📞 İletişim

Sorularınız için issue açabilir veya proje sahibi ile iletişime geçebilirsiniz.

---

## 🎉 Teşekkürler

- **ElevenLabs** - Speech-to-Text API
- **Google Gemini** - Görsel analiz
- **LM Studio** - Yerel LLM desteği
- **DeepFace** - Duygu analizi
- **spaCy** - Doğal dil işleme

---

**Son Güncelleme**: 2025

