using System.Threading.Tasks;
using GOT.SharedKernel;
using Got.TelegramBot;

namespace GOT.Notification
{
    public class TelegramNotification : INotification
    {
        private readonly ITelegramClient _telegramClient;

        public TelegramNotification(IConfiguration configuration)
        {
            _telegramClient = new TelegramClient();
            UpdateServiceInfo(configuration);
        }
        
        public void UpdateServiceInfo(IConfiguration configuration)
        {
            _telegramClient.UpdateClient(configuration.TelegramId);
        }

        public async Task SendMessageAsync(string message, string subject = "")
        {
            await _telegramClient.SendMessageToBotAsync(message);
        }

        public void SendMessage(string message, string subject = "")
        {
            var t = new Task(() => _telegramClient.SendMessageToBotAsync(message));
            t.RunSynchronously();
        }
    }
}