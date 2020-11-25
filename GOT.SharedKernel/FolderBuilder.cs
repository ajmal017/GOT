using System;
using System.IO;

namespace GOT.SharedKernel
{
    /// <summary>
    ///     Создает структуру каталогов
    /// </summary>
    public static class FolderBuilder
    {
        private const string CONFIG_FILE_NAME = "GOT_config.json";
        private const string LOGS_FOLDER_NAME = "Logs";
        private const string DELETED_STRATEGY_FOLDER_NAME = "Deleted strategies";
        private const string STRATEGIES_FOLDER_NAME = "Strategies";
        private const string GET_APP_NAME = "BFF";

        /// <summary>
        ///     Возвращает путь к папке стратегий
        /// </summary>
        private static string GetStrategiesFolderPath()
        {
            var appFolder = GetAppFolderPath();
            var connectorsFolder = Path.Combine(appFolder, STRATEGIES_FOLDER_NAME);
            CheckToExistFolder(connectorsFolder);
            return connectorsFolder;
        }

        public static string GetConnectorFolderPath(string connectorType)
        {
            var connectorsFolderPath = GetStrategiesFolderPath();
            var connectorFolder = Path.Combine(connectorsFolderPath, connectorType);
            return connectorFolder;
        }

        public static string GetConfigurationPath()
        {
            var configurationPath = Path.Combine(GetAppFolderPath(), CONFIG_FILE_NAME);
            var exists = File.Exists(configurationPath);
            if (!exists) {
                using var file = File.Create(configurationPath);
                file.Close();
            }

            return configurationPath;
        }

        public static void CreateConnectorFolder(string connectorType)
        {
            CheckToExistFolder(GetConnectorFolderPath(connectorType));
        }

        public static void CreateAllDirectories()
        {
            CreateAppFolder();
            CreateConnectorFolder();
            CreateLogFolder();
        }

        /// <summary>
        ///     Возвращает путь к корневой папке
        /// </summary>
        /// <returns></returns>
        private static string GetAppFolderPath()
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(local, GET_APP_NAME);
        }

        /// <summary>
        ///     Возвращает путь к папке логов
        /// </summary>
        /// <returns></returns>
        public static string GetLogFolderPath()
        {
            var appFolder = GetAppFolderPath();
            var logFolder = Path.Combine(appFolder, LOGS_FOLDER_NAME);
            CheckToExistFolder(logFolder);
            return logFolder;
        }

        /// <summary>
        ///     Возвращает путь к папке удаленных стратегий
        /// </summary>
        /// <returns></returns>
        public static string GetDeletedStrategyFolderPath()
        {
            var appFolder = GetAppFolderPath();
            var path = Path.Combine(appFolder, DELETED_STRATEGY_FOLDER_NAME);
            CheckToExistFolder(path);
            return path;
        }

        /// <summary>
        ///     Создает папку приложения
        /// </summary>
        private static void CreateAppFolder()
        {
            CheckToExistFolder(GetAppFolderPath());
        }

        /// <summary>
        ///     Создает папку для коннектора
        /// </summary>
        private static void CreateConnectorFolder()
        {
            CheckToExistFolder(GetStrategiesFolderPath());
        }

        /// <summary>
        ///     Создает папку для файла логов
        /// </summary>
        private static void CreateLogFolder()
        {
            CheckToExistFolder(GetLogFolderPath());
        }

        private static void CheckToExistFolder(string currentDateLogFolder)
        {
            if (!Directory.Exists(currentDateLogFolder)) {
                Directory.CreateDirectory(currentDateLogFolder);
            }
        }
    }
}