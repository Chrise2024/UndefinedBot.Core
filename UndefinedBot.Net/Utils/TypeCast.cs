using System.Reflection;
using Newtonsoft.Json.Linq;

namespace UndefinedBot.Net.Utils;

public static class JObjectExtensions
{
    public static T ToStruct<T>(this JObject jObject) where T : struct
    {
        T result = Activator.CreateInstance<T>();

        foreach (PropertyInfo property in typeof(T).GetProperties())
        {
            if (jObject.TryGetValue(property.Name, out var token))
            {
                property.SetValue(result, token.ToObject(property.PropertyType));
            }
        }

        return result;
    }
}
