using System.Threading.Tasks;
using GOT.SharedKernel;

namespace GOT.Notification
{
    public interface INotification
    {
        /// <summary>
        ///     Обновляет почту и телеграм, на который отправляются уведомления.
        /// </summary>
        void UpdateServiceInfo(IConfiguration configuration);

        /// <summary>
        ///     Отправить сообщение.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="subject">Заголовок</param>
        Task SendMessageAsync(string message, string subject = "");

        /// <summary>
        ///     Отправить письмо.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="subject">Заголовок</param>
        void SendMessage(string message, string subject = "");
    }
}