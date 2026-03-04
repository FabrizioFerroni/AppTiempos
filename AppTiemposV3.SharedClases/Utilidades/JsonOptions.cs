using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppTiemposV3.SharedClases.Utilidades;

public static class JsonOptions
{
    public static JsonSerializerOptions JsonOptionsSafe()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Converters estándar
        options.Converters.Add(new JsonStringEnumConverter());

        // Converter “seguro” para TimeOnly
        options.Converters.Add(new TimeOnlyJsonConverterSafe());

        return options;
    }

// Converter que intenta parsear y si falla pone 00:00
    public class TimeOnlyJsonConverterSafe : JsonConverter<TimeOnly>
    {
        private static readonly string[] Formats = { "HH:mm", "HH:mm:ss" };

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                var str = reader.GetString();
                if (string.IsNullOrEmpty(str))
                    return default;

                return TimeOnly.ParseExact(str!, Formats, CultureInfo.InvariantCulture);
            }
            catch
            {
                return default; // si falla, devuelve 00:00
            }
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            var format = value.Second == 0 ? "HH:mm" : "HH:mm:ss";
            writer.WriteStringValue(value.ToString(format));
        }
    }

    public class FlexibleDoubleConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetDouble();

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var document = JsonDocument.ParseValue(ref reader);

                if (document.RootElement.TryGetProperty("parsedValue", out var parsed))
                    return parsed.GetDouble();
            }

            return 0;
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}