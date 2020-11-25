using System.Collections.Generic;
using GOT.Logic.Strategies;

namespace GOT.Logic.Utils
{
    public interface ILoader
    {
        void SaveStrategies(IEnumerable<MainStrategy> mainStrategies, string connectorType);
        IList<MainStrategy> LoadStrategies(string connectorType);
        void Save(object obj, string title);
        void SaveToDeleteFolder(string connectorType, string name);
        T LoadDialog<T>(string title) where T : class;
        T Load<T>(string title) where T : class;
        void ReplaceStrategy(string connectorType, string oldStrategyName, string newStrategyName);
    }
}