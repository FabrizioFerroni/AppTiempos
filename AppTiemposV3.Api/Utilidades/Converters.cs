using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using System.Globalization;

namespace AppTiemposV3.Api.Utilidades;


public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(Format));
    }

    public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string? str = (string?)reader.Value;
        return DateOnly.ParseExact(str!, Format, CultureInfo.InvariantCulture);
    }
}

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private static readonly string[] Formats = { "HH:mm", "HH:mm:ss", "HH:mm:ss.fffffff" };

    public override void WriteJson(JsonWriter writer, TimeOnly value, JsonSerializer serializer)
    {
        string format;

        if (value.Second == 0 && value.Millisecond == 0)
        {
            format = "HH:mm";
        }
        else if (value.Millisecond == 0)
        {
            format = "HH:mm:ss";
        }
        else
        {
            format = "HH:mm:ss.fffffff";
        }

        writer.WriteValue(value.ToString(format, CultureInfo.InvariantCulture));
    }

    public override TimeOnly ReadJson(JsonReader reader, Type objectType, TimeOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string? str = (string?)reader.Value;

        return TimeOnly.ParseExact(str!, Formats, CultureInfo.InvariantCulture);
    }
}

public class TimeOnlyNullableJsonConverter : JsonConverter<TimeOnly?>
{
    private static readonly string[] Formats = { "HH:mm", "HH:mm:ss", "HH:mm:ss.fffffff" };

    public override void WriteJson(JsonWriter writer, TimeOnly? value, JsonSerializer serializer)
    {
        if (value.HasValue)
        {            
            string format;

            if (value.Value.Second == 0 && value.Value.Millisecond == 0)
            {
                format = "HH:mm";
            }
            else if (value.Value.Millisecond == 0)
            {
                format = "HH:mm:ss";
            }
            else
            {
                format = "HH:mm:ss.fffffff";
            }

            writer.WriteValue(value.Value.ToString(format, CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteNull();
        }
    }

    public override TimeOnly? ReadJson(JsonReader reader, Type objectType, TimeOnly? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        string? str = (string?)reader.Value;
        return TimeOnly.ParseExact(str!, Formats, CultureInfo.InvariantCulture);
    }
}

public class DateOnlyNullableJsonConverter : JsonConverter<DateOnly?>
{
    private const string Format = "yyyy-MM-dd";

    public override void WriteJson(JsonWriter writer, DateOnly? value, JsonSerializer serializer)
    {
        if (value.HasValue)
            writer.WriteValue(value.Value.ToString(Format));
        else
            writer.WriteNull();
    }

    public override DateOnly? ReadJson(JsonReader reader, Type objectType, DateOnly? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        string? str = (string?)reader.Value;
        return DateOnly.ParseExact(str!, Format, CultureInfo.InvariantCulture);
    }
}

