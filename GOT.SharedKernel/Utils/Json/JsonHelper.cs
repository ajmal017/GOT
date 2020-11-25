using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace GOT.SharedKernel.Utils.Json
{
    public static class JsonHelper
    {
        public static void SerializeToJsonFile<T>(T value, string filePath)
        {
            try {
                using var file = File.CreateText(filePath);
                var serializer = new JsonSerializer
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    Converters = {new DecimalJsonConverter()}
                };
                serializer.Error += SerializerOnError;
                serializer.Serialize(file, value);
            }
            catch (Exception e) {
                Debug.WriteLine(e);
            }
        }

        public static T DeserializeFromJson<T>(string filePath) where T : class
        {
            try {
                using var file = File.OpenText(filePath);
                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                };
                serializer.Error += SerializerOnError;
                return (T) serializer.Deserialize(file, typeof(T));
            }
            catch (Exception e) {
                Debug.WriteLine(e);
            }

            return null;
        }

        private static void SerializerOnError(object sender, ErrorEventArgs e)
        {
            if (e.ErrorContext.Member != null) {
                MessageBox.Show(e.ErrorContext.Member.ToString(), "Error...", MessageBoxButton.OK);
            }
        }
    }
}