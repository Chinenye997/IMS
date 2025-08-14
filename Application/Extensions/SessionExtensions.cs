using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Application.Extensions
{
    public static class SessionExtensions
    {
        // Save any object as a JSON string under the given key in session.
        public static void SetObjectAsJson(this ISession session, string key, object value)
            => session.SetString(key, JsonConvert.SerializeObject(value));

        // Retrieve and deserialize JSON from session
        // Retrieve an object of type T from a JSON string stored under the given key.        
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            return string.IsNullOrEmpty(json)
                ? default!
                : JsonConvert.DeserializeObject<T>(json)!;
        }
    }
}