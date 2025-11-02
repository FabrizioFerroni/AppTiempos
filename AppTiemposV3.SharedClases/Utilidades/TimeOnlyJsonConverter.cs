using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace AppTiemposV3.SharedClases.Utilidades;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private static readonly string[] Formats = { "HH:mm", "HH:mm:ss" };

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return default;

        return TimeOnly.ParseExact(value, Formats, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        var format = value.Second == 0 ? "HH:mm" : "HH:mm:ss";
        writer.WriteStringValue(value.ToString(format));
    }
}


public class TimeOnlyJsonConverterSTJ : JsonConverter<TimeOnly>
{
    private static readonly string[] Formats = { "HH:mm", "HH:mm:ss" };

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return default;

        return TimeOnly.ParseExact(value, Formats, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        var format = value.Second == 0 ? "HH:mm" : "HH:mm:ss";
        writer.WriteStringValue(value.ToString(format));
    }
}

public class NullableTimeOnlyJsonConverterSTJ : JsonConverter<TimeOnly?>
{
    private static readonly string[] Formats = { "HH:mm", "HH:mm:ss" };

    public override TimeOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return null;

        return TimeOnly.ParseExact(value, Formats, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            var format = value.Value.Second == 0 ? "HH:mm" : "HH:mm:ss";
            writer.WriteStringValue(value.Value.ToString(format));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
