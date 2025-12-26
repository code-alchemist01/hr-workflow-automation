using Microsoft.AspNetCore.SignalR.Client;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("SignalR Test Uygulaması Başlatılıyor...");
        
        var connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7030/noteHub")
            .Build();

        connection.On<string>("ReceiveNoteUpdate", (message) =>
        {
            Console.WriteLine($"Yeni mesaj: {message}");
        });

        try
        {
            await connection.StartAsync();
            Console.WriteLine("SignalR Bağlantısı başarılı!");
            Console.WriteLine("Bildirimler bekleniyor... (Çıkmak için 'exit' yazın)");

            while (true)
            {
                var message = Console.ReadLine();
                if (message?.ToLower() == "exit")
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }
} 