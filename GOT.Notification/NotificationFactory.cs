using GOT.SharedKernel;

namespace GOT.Notification
{
    public class NotificationFactory : INotificationFactory
    {
        private readonly IConfiguration _configuration;
        public NotificationFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public INotification GetTelegramNotification()
        {
            return new TelegramNotification(_configuration);
        }
        
        public INotification GetEmailNotification()
        {
            return new EmailNotification(_configuration);
        }
    }
}