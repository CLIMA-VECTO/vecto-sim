using System.Globalization;
using Newtonsoft.Json;

namespace TUGraz.VectoCore.Utils
{
    public static class Memento
    {
        public static string Serialize<T>(T memento)
        {
            var mementoObject = (memento as IMemento);
            if (mementoObject != null)
                return mementoObject.Serialize();
            
            return JsonConvert.SerializeObject(memento, Formatting.Indented, new JsonSerializerSettings { Culture = CultureInfo.InvariantCulture });
        }

        public static T Deserialize<T>(string data, T mem)
        {
            return JsonConvert.DeserializeAnonymousType(data, mem,
                new JsonSerializerSettings { Culture = CultureInfo.InvariantCulture });
        }

        public static T Deserialize<T>(string data) where T : IMemento, new()
        {
            var x = new T();
            x.Deserialize(data);
            return x;
        }
    }
}