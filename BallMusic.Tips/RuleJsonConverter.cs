using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BallMusic.Tips;

[Experimental("BMT001")]
public sealed class RuleJsonConverter : JsonConverter<Rule>
{
    public override Rule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var type = root.GetProperty("type").GetString();

        return type switch
        {
            nameof(CountOfDance) => new CountOfDance(
                requiredDances: JsonSerializer.Deserialize<ImmutableArray<(string, float, float)>>(root.GetProperty("requiredDances").GetRawText(), options)!
            ),
            nameof(EndWithSong) => new EndWithSong(
                song: JsonSerializer.Deserialize<FakeSong>(root.GetProperty("song").GetRawText(), options)!,
                severity: Enum.Parse<Rule.Severity>(root.GetProperty("severity").GetString()!)
            ),
            nameof(DurationBetween) => new DurationBetween(
                min: TimeSpan.Parse(root.GetProperty("min").GetString()!),
                max: TimeSpan.Parse(root.GetProperty("max").GetString()!)
            ),
            _ => throw new JsonException($"Unknown rule type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, Rule value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        switch (value)
        {
            case CountOfDance countOfDance:
                writer.WriteString("type", nameof(CountOfDance));
                writer.WritePropertyName("requiredDances");
                JsonSerializer.Serialize(writer, countOfDance.requiredDances, options);
                break;

            case EndWithSong endWithSong:
                writer.WriteString("type", nameof(EndWithSong));
                writer.WritePropertyName("song");
                JsonSerializer.Serialize(writer, endWithSong.song, options);
                writer.WriteString("severity", endWithSong.severity.ToString());
                break;

            case DurationBetween durationBetween:
                writer.WriteString("type", nameof(DurationBetween));
                writer.WriteString("min", durationBetween.min.ToString());
                writer.WriteString("max", durationBetween.max.ToString());
                break;

            default:
                throw new JsonException($"Unknown rule type: {value.GetType().Name}");
        }
        writer.WriteEndObject();
    }
}
