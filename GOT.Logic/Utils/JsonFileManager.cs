using System.Collections.Generic;
using System.IO;
using GOT.Logic.Strategies;
using GOT.SharedKernel;
using GOT.SharedKernel.Utils.Json;
using Microsoft.Win32;

namespace GOT.Logic.Utils
{
    public class JsonFileManager : ILoader
    {
        private const string BASE_FORMAT = ".json";

        public void SaveStrategies(IEnumerable<MainStrategy> mainStrategies, string connectorType)
        {
            var folderPath = FolderBuilder.GetConnectorFolderPath(connectorType);
            foreach (var strategy in mainStrategies) {
                JsonHelper.SerializeToJsonFile(strategy, folderPath + "\\" + strategy.Name + ".json");
            }
        }

        public IList<MainStrategy> LoadStrategies(string connectorType)
        {
            var loadedStrategies = new List<MainStrategy>();
            var pathToFolder = FolderBuilder.GetConnectorFolderPath(connectorType);
            var strategiesPathsList = Directory.GetFiles(pathToFolder, "*.json");

            foreach (var path in strategiesPathsList) {
                var mainStrategy = JsonHelper.DeserializeFromJson<MainStrategy>(path);
                loadedStrategies.Add(mainStrategy);
            }

            return loadedStrategies;
        }

        public T LoadDialog<T>(string title) where T : class
        {
            T loadObject = null;
            var openDialog = new OpenFileDialog
            {
                AddExtension = true,
                Multiselect = false,
                DefaultExt = "json",
                Filter = "json files (*.json)|*.json",
                RestoreDirectory = true,
                Title = title
            };

            if (openDialog.ShowDialog() == true) {
                loadObject = Load<T>(openDialog.FileName);
            }

            return loadObject;
        }

        public T Load<T>(string title) where T : class
        {
            return JsonHelper.DeserializeFromJson<T>(title);
        }

        public void ReplaceStrategy(string connectorType, string oldStrategyName, string newStrategyName)
        {
            var strategiesFolder = FolderBuilder.GetConnectorFolderPath(connectorType);
            var oldFile = Path.Combine(strategiesFolder, oldStrategyName + BASE_FORMAT);
            var newFile = Path.Combine(strategiesFolder, newStrategyName + BASE_FORMAT);
            if (File.Exists(newFile)) {
                File.Delete(newFile);
            }

            File.Move(oldFile, newFile);
        }

        public void Save(object obj, string title)
        {
            var saveDialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "json",
                Filter = "json files (*.json)|*.json",
                OverwritePrompt = true,
                RestoreDirectory = true,
                Title = title
            };

            if (saveDialog.ShowDialog() == true) {
                JsonHelper.SerializeToJsonFile(obj, saveDialog.FileName);
            }
        }

        public void SaveToDeleteFolder(string connectorType, string name)
        {
            var strategiesFolder = FolderBuilder.GetConnectorFolderPath(connectorType);
            var oldFile = Path.Combine(strategiesFolder, name + BASE_FORMAT);
            var deletedStrategyFolderPath = FolderBuilder.GetDeletedStrategyFolderPath();

            var oldPrefix = "old_";
            var inProgressFilePath = Path.Combine(deletedStrategyFolderPath, oldPrefix + name + BASE_FORMAT);

            IsFileExist(ref inProgressFilePath);
            File.Move(oldFile, inProgressFilePath);

            void IsFileExist(ref string newFilePath)
            {
                if (File.Exists(newFilePath)) {
                    newFilePath = newFilePath.Insert(newFilePath.Length - BASE_FORMAT.Length, "_last");
                    IsFileExist(ref newFilePath);
                }
            }
        }
    }
}