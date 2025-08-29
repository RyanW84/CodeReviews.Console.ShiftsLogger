using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShiftsLoggerV2.RyanW84.Common;

/// <summary>
/// Custom JSON converter for DateTimeOffset that supports multiple date formats
/// including dd/MM/yyyy HH:mm, dd-MM-yyyy HH:mm, and standard ISO formats
/// </summary>
public class DdMmYyyyHHmmDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
	private static readonly string[] SupportedFormats = 
	{
		"dd/MM/yyyy HH:mm",
		"dd/MM/yyyy H:mm", 
		"dd-MM-yyyy HH:mm",
		"dd-MM-yyyy H:mm",
		"yyyy-MM-ddTHH:mm:ss.fffZ",
		"yyyy-MM-ddTHH:mm:ssZ",
		"yyyy-MM-ddTHH:mm:ss.fff",
		"yyyy-MM-ddTHH:mm:ss"
	};

	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		if (string.IsNullOrEmpty(value))
		{
			throw new JsonException("DateTime value cannot be null or empty");
		}

		// Try parsing with specific formats first
		if (DateTimeOffset.TryParseExact(value, SupportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
		{
			return result;
		}

		// Fall back to standard parsing (handles ISO formats and other standard formats)
		if (DateTimeOffset.TryParse(value, out result))
		{
			return result;
		}

		throw new JsonException($"Unable to parse '{value}' as DateTimeOffset. Supported formats: {string.Join(", ", SupportedFormats)}");
	}

	public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
	{
		// Write in ISO format for consistency
		writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
	}
}
