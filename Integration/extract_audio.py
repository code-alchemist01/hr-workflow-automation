import os
os.environ["PATH"] += os.pathsep + r"C:/path/to/ffmpeg/bin"
from moviepy.editor import VideoFileClip

video_path = r"C:\Users\excalibur\Desktop\Projeler\Intellium\a\Integration\input_video.mp4"  # Video dosya yolunu buraya girin
output_audio_path = r"C:\Users\excalibur\Desktop\Projeler\Intellium\a\Integration\extracted_audio.wav"

if not os.path.exists(video_path):
    print(f"Hata: '{video_path}' dosyası bulunamadı. Lütfen video dosya yolunu kontrol edin.")
    exit()

print(f"'{video_path}' dosyasından ses çıkarılıyor...")
video = VideoFileClip(video_path)
video.audio.write_audiofile(output_audio_path)
print(f"Ses dosyası '{output_audio_path}' olarak kaydedildi.")