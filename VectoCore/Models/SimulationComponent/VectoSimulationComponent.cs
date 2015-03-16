using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Common.Logging.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
    public abstract class VectoSimulationComponent
    {
        [NonSerialized]
        protected ILog Log;

        protected VectoSimulationComponent()
        {
            Log = LogManager.GetLogger(this.GetType());
        }

        public abstract void CommitSimulationStep(IModalDataWriter writer);

    }

    public class Memento
    {
        public class MyContractResolver : DefaultContractResolver
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
            var settings = new JsonSerializerSettings { ContractResolver = new MyContractResolver() };
            return JsonConvert.SerializeObject(data, Formatting.Indented, settings);
        }

        public static T Deserialize<T>(string data)
        {
            try
            {
                var settings = new JsonSerializerSettings { ContractResolver = new MyContractResolver() };
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
