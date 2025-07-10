using Newtonsoft.Json;
using System;

public class DateOnlyJsonConverterNewtonsoft : JsonConverter
{
    private const string Format = "yyyy-MM-dd";

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        if (value is DateOnly date)
        {
            writer.WriteValue(date.ToString(Format));
        }
        else if (value is DateOnly?)
        {
            DateOnly? dateNullable = (DateOnly?)value;
            if (dateNullable.HasValue)
                writer.WriteValue(dateNullable.Value.ToString(Format));
            else
                writer.WriteNull();
        }
        else
        {
            throw new JsonSerializationException("Unexpected value type for DateOnly");
        }
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var str = reader.Value?.ToString();
        if (string.IsNullOrWhiteSpace(str))
            return null;

        return DateOnly.ParseExact(str, Format);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(DateOnly) || objectType == typeof(DateOnly?);
    }
}
