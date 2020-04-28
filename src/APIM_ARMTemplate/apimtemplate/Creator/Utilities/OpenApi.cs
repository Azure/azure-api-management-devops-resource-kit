using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace apimtemplate.Creator.Utilities
{
    internal class OpenApi
    {

        private readonly string format_ = "openapi";
        private readonly IDictionary<string, object> definition_;

        public OpenApi(string definition, string format = null)
        {
            format_ = format ?? "openapi";

            if (format_ == "openapi")
            {
                // TODO: find a way to have inner dictionaries
                // TODO: be of type Dictionary<string, object> instead
                // TODO: of Dictionary<object, object> as currently

                definition_ = new Deserializer().Deserialize<Dictionary<string, object>>(definition);
            }
            else
            {
                definition_ = JsonConvert.DeserializeObject<Dictionary<string, object>>(definition, new JsonDictionaryConverter());
            }
        }
        public OpenApi SetTitle(string title)
        {
            if (format_ == "openapi")
            {
                var dict = definition_["info"] as Dictionary<object, object>;
                dict["title"] = title;
            }
            else
            {
                var dict = definition_["info"] as Dictionary<string, object>;
                dict["title"] = title;
            }
            return this;
        }

        public string GetDefinition()
        {
            if (format_ == "openapi")
            {
                return new Serializer().Serialize(definition_);
            }
            else
            {
                return JsonConvert.SerializeObject(definition_);
            }
        }

        public class JsonDictionaryConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { this.WriteValue(writer, value); }

            private void WriteValue(JsonWriter writer, object value)
            {
                var t = JToken.FromObject(value);
                switch (t.Type)
                {
                    case JTokenType.Object:
                        this.WriteObject(writer, value);
                        break;
                    case JTokenType.Array:
                        this.WriteArray(writer, value);
                        break;
                    default:
                        writer.WriteValue(value);
                        break;
                }
            }

            private void WriteObject(JsonWriter writer, object value)
            {
                writer.WriteStartObject();
                var obj = value as IDictionary<string, object>;
                foreach (var kvp in obj)
                {
                    writer.WritePropertyName(kvp.Key);
                    this.WriteValue(writer, kvp.Value);
                }
                writer.WriteEndObject();
            }

            private void WriteArray(JsonWriter writer, object value)
            {
                writer.WriteStartArray();
                var array = value as IEnumerable<object>;
                foreach (var o in array)
                {
                    this.WriteValue(writer, o);
                }
                writer.WriteEndArray();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return ReadValue(reader);
            }

            private object ReadValue(JsonReader reader)
            {
                while (reader.TokenType == JsonToken.Comment)
                {
                    if (!reader.Read()) throw new JsonSerializationException("Unexpected Token when converting IDictionary<string, object>");
                }

                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:
                        return ReadObject(reader);
                    case JsonToken.StartArray:
                        return this.ReadArray(reader);
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.String:
                    case JsonToken.Boolean:
                    case JsonToken.Undefined:
                    case JsonToken.Null:
                    case JsonToken.Date:
                    case JsonToken.Bytes:
                        return reader.Value;
                    default:
                        throw new JsonSerializationException
                            (string.Format("Unexpected token when converting IDictionary<string, object>: {0}", reader.TokenType));
                }
            }

            private object ReadArray(JsonReader reader)
            {
                IList<object> list = new List<object>();

                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.Comment:
                            break;
                        default:
                            var v = ReadValue(reader);

                            list.Add(v);
                            break;
                        case JsonToken.EndArray:
                            return list;
                    }
                }

                throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
            }

            private object ReadObject(JsonReader reader)
            {
                var obj = new Dictionary<string, object>();

                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.PropertyName:
                            var propertyName = reader.Value.ToString();

                            if (!reader.Read())
                            {
                                throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
                            }

                            var v = ReadValue(reader);

                            obj[propertyName] = v;
                            break;
                        case JsonToken.Comment:
                            break;
                        case JsonToken.EndObject:
                            return obj;
                    }
                }

                throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
            }

            public override bool CanConvert(Type objectType) { return typeof(IDictionary<string, object>).IsAssignableFrom(objectType); }
        }
    }
}