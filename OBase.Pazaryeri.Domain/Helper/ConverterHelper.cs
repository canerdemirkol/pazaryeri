using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Domain.Helper
{
    public enum CurrencyEnum { Try };

    public struct CurrencyUnion
    {
        public CurrencyEnum? Enum;
        public long? Integer;

        public static implicit operator CurrencyUnion(CurrencyEnum Enum) => new CurrencyUnion { Enum = Enum };
        public static implicit operator CurrencyUnion(long Integer) => new CurrencyUnion { Integer = Integer };
    }
    public static class ConverterHelper
    {
        public static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
                    {
                        CurrencyUnionConverter.Singleton,
                        CurrencyEnumConverter.Singleton,
                        new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
                    },
            };
        }

        public class CurrencyUnionConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(CurrencyUnion) || t == typeof(CurrencyUnion?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.String:
                    case JsonToken.Date:
                        var stringValue = serializer.Deserialize<string>(reader);
                        if (stringValue == Currency.Try)
                        {
                            return new CurrencyUnion { Enum = CurrencyEnum.Try };
                        }
                        long l;
                        if (long.TryParse(stringValue, out l))
                        {
                            return new CurrencyUnion { Integer = l };
                        }
                        break;
                }
                throw new Exception("Cannot unmarshal type CurrencyUnion");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                var value = (CurrencyUnion)untypedValue;
                if (value.Enum != null)
                {
                    if (value.Enum == CurrencyEnum.Try)
                    {
                        serializer.Serialize(writer, Currency.Try);
                        return;
                    }
                }
                if (value.Integer != null)
                {
                    serializer.Serialize(writer, value.Integer.Value.ToString());
                    return;
                }
                throw new Exception("Cannot marshal type CurrencyUnion");
            }

            public static readonly CurrencyUnionConverter Singleton = new CurrencyUnionConverter();
        }

        public class CurrencyEnumConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(CurrencyEnum) || t == typeof(CurrencyEnum?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                if (value == Currency.Try)
                {
                    return CurrencyEnum.Try;
                }
                throw new Exception("Cannot unmarshal type CurrencyEnum");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (CurrencyEnum)untypedValue;
                if (value == CurrencyEnum.Try)
                {
                    serializer.Serialize(writer, Currency.Try);
                    return;
                }
                throw new Exception("Cannot marshal type CurrencyEnum");
            }

            public static readonly CurrencyEnumConverter Singleton = new CurrencyEnumConverter();
        }

        public class ParseStringConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                long l;
                if (long.TryParse(value, out l))
                {
                    return l;
                }
                throw new Exception("Cannot unmarshal type long");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (long)untypedValue;
                serializer.Serialize(writer, value.ToString());
                return;
            }

            public static readonly ParseStringConverter Singleton = new ParseStringConverter();
        }
    }
}