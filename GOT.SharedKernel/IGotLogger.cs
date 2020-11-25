namespace GOT.SharedKernel
{
    public interface IGotLogger
    {
        /// <summary>
        /// Добавить лог
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <param name="type">тип сообщения. 1 - info, 2 - warning, 3 - error, 4 - fatal</param>
        void AddLog(string message, int type = 1);
    }
}