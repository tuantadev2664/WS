using System.Text.Json;

namespace MiniBattleship.Utils
{
    public static class SessionJson
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }


        public static T? Get<T>(this ISession session, string key)
        {
            var s = session.GetString(key);
            return s is null ? default : JsonSerializer.Deserialize<T>(s);
        }
    }
}
