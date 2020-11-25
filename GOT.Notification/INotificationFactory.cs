namespace GOT.Notification
{
    public interface INotificationFactory
    {
        INotification GetTelegramNotification();
        INotification GetEmailNotification();
    }
}