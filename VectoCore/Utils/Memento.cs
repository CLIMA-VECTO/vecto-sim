using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
    public class Memento
    {
        public class PrivateFieldsContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                var props = type.GetFields(flags).Select(f => CreateProperty(f, memberSerialization)).ToList();
                props.ForEach(p => { p.Writable = true; p.Readable = true; });
                return props;
            }
        }

        public static string Serialize<T>(T data)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new PrivateFieldsContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return JsonConvert.SerializeObject(data, Formatting.Indented, settings);
        }

        public static T Deserialize<T>(string data)
        {
            try
            {
                var settings = new JsonSerializerSettings { ContractResolver = new PrivateFieldsContractResolver() };
                return JsonConvert.DeserializeObject<T>(data, settings);
            }
            catch (Exception e)
            {
                var ex = new VectoException(string.Format("Object could not be deserialized: {0}", e.Message), e);
                ex.Data["data"] = data;
                throw ex;
            }
        }
    }
}