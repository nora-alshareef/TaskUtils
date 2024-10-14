using System;
using System.Text.Json;

namespace  TaskUtils.Models
{
    public static class Helpers
    {
        public static string SerializeToJson(object value)
        {
            if (value is string || value.GetType().IsPrimitive)
                return value.ToString();

            try
            {
                return JsonSerializer.Serialize(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Serialization failed: " + ex.Message);
                throw;
            }
        }

        public static T HandlePrimitiveOrDeserialize<T>(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Input cannot be null or empty.", nameof(value));

            if (typeof(T) == typeof(string))
                return (T)(object)value;

            if (typeof(T) == typeof(int))
                return (T)(object)Convert.ToInt32(value);

            if (typeof(T) == typeof(double))
                return (T)(object)Convert.ToDouble(value);

            if (typeof(T) == typeof(bool))
                return (T)(object)Convert.ToBoolean(value);

            if (typeof(T) == typeof(long))
                return (T)(object)Convert.ToInt64(value);

            if (typeof(T) == typeof(float))
                return (T)(object)Convert.ToSingle(value);

            if (typeof(T) == typeof(decimal))
                return (T)(object)Convert.ToDecimal(value);

            try
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to deserialize JSON.", ex);
            }
        }
    }
}