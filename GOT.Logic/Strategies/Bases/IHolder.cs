using GOT.Logic.Connectors;
using GOT.Logic.Models.Instruments;
using GOT.Notification;
using GOT.SharedKernel;

namespace GOT.Logic.Strategies.Bases
{
    public interface IHolder
    {
        /// <summary>
        ///     Позиция всех стратегий в контейнерах
        /// </summary>
        int PositionContainers { get; }

        /// <summary>
        ///     Profit and Loss всех стратегий в контейнерах
        /// </summary>
        decimal PnlContainers { get; }

        /// <summary>
        ///     Проверяет, есть ли стратегии в каком-нибудь из контейнеров.
        /// </summary>
        bool ExistsStrategies();

        /// <summary>
        ///     Остановка работы всех контейнеров
        /// </summary>
        void StopAllContainers();

        /// <summary>
        ///     Запуск всех контейнеров в работу
        /// </summary>
        void StartAllContainers();

        /// <summary>
        ///     Закрытие позиции во всех контейнерах
        /// </summary>
        void CloseAllPositions();

        /// <summary>
        ///     Обновление инструмента в контейнерах
        /// </summary>
        /// <param name="shiftStrikeStep">шаг, на который необходимо сдвинуть страйки инструментов</param>
        void UpdateInstruments(decimal shiftStrikeStep);

        /// <summary>
        ///     Задать имя родительской стратегии
        /// </summary>
        /// <param name="parentName">Имя родительской стратегии</param>
        void SetParentName(string parentName);

        /// <summary>
        ///     Задать родительский инструмент
        /// </summary>
        /// <param name="instrument">Родительский инструмент</param>
        void SetParentInstrument(Future instrument);

        void SetConnector(IConnector connector);
        void SetNotification(INotification notification);
        void SetLogger(IGotLogger logger);
        void SetAccount(string account);
    }
}