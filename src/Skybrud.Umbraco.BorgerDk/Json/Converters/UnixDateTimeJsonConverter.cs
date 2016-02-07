using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Skybrud.Umbraco.BorgerDk.Json.Converters {

    public class UnixDateTimeJsonConverter : DateTimeConverterBase {

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            long val;
            if (value is DateTime) {
                val = BorgerDkHelper.GetUnixTimeFromDateTime((DateTime)value);
            } else {
                throw new Exception("Expected date object value.");
            }
            writer.WriteValue(val);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType != JsonToken.Integer) throw new Exception("Wrong token type");
            return BorgerDkHelper.GetDateTimeFromUnixTime((long) reader.Value);
        }

    }

}