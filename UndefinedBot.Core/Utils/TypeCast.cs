using System.Reflection;
using Newtonsoft.Json.Linq;

namespace UndefinedBot.Core.Utils
{
    public static class JObjectExtensions
    {
        public static T ToStruct<T>(this JObject jObject) where T : struct
        {
            T result = Activator.CreateInstance<T>();

            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                if (jObject.TryGetValue(property.Name, out var token))
                {
                    //Console.WriteLine(property.Name);
                    property.SetValue(result, token.ToObject(property.PropertyType));
                }
            }
            return result;
        }
    }
}
