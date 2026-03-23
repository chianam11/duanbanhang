using System.Text.Json;

namespace ShopMVC.Extensions
{
    /// <summary>
    /// Extension methods cho JSON operations
    /// </summary>
    public static class JsonExtensions
    {
        public static string ToJson<T>(this T obj, bool indented = true)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = indented,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(obj, options);
        }

        public static T? FromJson<T>(this string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch
            {
                return default;
            }
        }
    }

    /// <summary>
    /// Extension methods cho DateTime operations
    /// </summary>
    public static class DateTimeExtensions
    {
        public static bool IsToday(this DateTime date)
        {
            return date.Date == DateTime.Today;
        }

        public static bool IsYesterday(this DateTime date)
        {
            return date.Date == DateTime.Today.AddDays(-1);
        }

        public static string ToVietnamTime(this DateTime date)
        {
            var vietnamZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTime(date, vietnamZone).ToString("dd/MM/yyyy HH:mm:ss");
        }

        public static int GetAge(this DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
                age--;

            return age;
        }
    }

    /// <summary>
    /// Extension methods cho String operations
    /// </summary>
    public static class StringExtensions
    {
        public static bool IsPhoneNumber(this string? str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"^0\d{9}$");
        }

        public static string Truncate(this string? str, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(str)) return str ?? string.Empty;
            if (str.Length <= maxLength) return str;

            return str.Substring(0, maxLength - suffix.Length) + suffix;
        }

        public static string ToSlug(this string? str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;

            string text = str.Trim().ToLower();
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[^\w\s-]", string.Empty);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"(\s|-)+", "-");

            return text;
        }

        public static string RemoveVietnameseMarks(this string? str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;

            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(str);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public static bool ContainsIgnoreCase(this string? str, string? value)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(value)) return false;
            return str.Contains(value, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Extension methods cho Collection operations
    /// </summary>
    public static class CollectionExtensions
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
        {
            return source == null || !source.Any();
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var list = source.ToList();
            var rng = new Random();
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }

            return list;
        }

        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
        {
            while (source.Any())
            {
                yield return source.Take(chunkSize);
                source = source.Skip(chunkSize);
            }
        }
    }
}