using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cryptotrader
{
    public static class Json
    {
        private static readonly JsonSerializerOptions options = new()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            WriteIndented = true,
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            Converters =
            {
                new JsonStringEnumConverter(new SnakeCaseNamingPolicy())
            }
        };

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }
        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, options);
        }

        public static ValueTask<T> DeserializeAsync<T>(Stream stream)
        {
            return JsonSerializer.DeserializeAsync<T>(stream, options);
        }
        public static Task SerializeAsync<T>(T obj, Stream stream)
        {
            return JsonSerializer.SerializeAsync<T>(stream, obj, options);
        }
    }

    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            char[] chars = name.ToCharArray();
            return new string(SnakeCase(chars));
        }

        private static char[] SnakeCase(char[] chars)
        {
            List<char> charList = new();
            int offset = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if ((i > 0 && char.IsUpper(c)) || char.IsWhiteSpace(c)) charList.Insert(i + offset++, '_');
                if (!char.IsWhiteSpace(c)) charList.Insert(i + offset, char.ToLowerInvariant(c));
            }

            return charList.ToArray();
        }
    }
}
