﻿using FaunaDB.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FaunaDB.Types
{
    class ValueJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ((Expr) value).WriteJson(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
            // For some reason, reader starts with one token already read.
            ValueReader.HandleValue(reader);

        public override bool CanConvert(Type objectType) =>
            typeof(Expr).IsAssignableFrom(objectType);
    }

    class ValueReader
    {
        readonly JsonReader reader;

        public static Expr HandleValue(JsonReader reader) =>
            new ValueReader(reader).HandleValue();

        ValueReader(JsonReader reader)
        {
            this.reader = reader;
        }

        Value HandleValue()
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return ReadObject();
                case JsonToken.StartArray:
                    return ReadArray();
                case JsonToken.Integer:
                    return new LongV((long) reader.Value);
                case JsonToken.Float:
                    return new DoubleV((double) reader.Value);
                case JsonToken.String:
                    return new StringV((string) reader.Value);
                case JsonToken.Boolean:
                    return BooleanV.Of((bool) reader.Value);
                case JsonToken.Null:
                    return NullV.Instance;
                default:
                    return Unexpected();
            }
        }

        JsonToken Next()
        {
            reader.Read();
            return reader.TokenType;
        }

        Value ReadValue()
        {
            reader.Read();
            return HandleValue();
        }

        ArrayV ReadArray() =>
            new ArrayV(Add =>
            {
                while (Next() != JsonToken.EndArray)
                    Add(HandleValue());
            });

        Value ReadObject()
        {
            switch (Next())
            {
                case JsonToken.PropertyName:
                    var name = (string) reader.Value;
                    switch (name)
                    {
                        case "@ref":
                            return new Ref(ReadStringAndEndObject());
                        case "@obj":
                            NextAndExpect(JsonToken.StartObject);
                            var obj = ReadObjectBody(ReadPropertyName());
                            NextAndExpect(JsonToken.EndObject);
                            return obj;
                        case "@set":
                            var v = ReadValue();
                            NextAndExpect(JsonToken.EndObject);
                            return new SetRef(((ObjectV)v).Value);
                        case "@ts":
                            return new TsV(ReadStringAndEndObject());
                        case "@date":
                            return new DateV(ReadStringAndEndObject());
                        default:
                            return ReadObjectBody(name);
                    }
                case JsonToken.EndObject:
                    return ObjectV.Empty;
                default:
                    return Unexpected();
            }
        }

        ObjectV ReadObjectBody(string firstPropertyName) =>
            new ObjectV(Add =>
            {
                Add(firstPropertyName, ReadValue());
                while (Next() != JsonToken.EndObject)
                    Add(ExpectPropertyName(), ReadValue());
            });

        string ReadPropertyName()
        {
            Next();
            return ExpectPropertyName();
        }

        string ExpectPropertyName()
        {
            Expect(JsonToken.PropertyName);
            return (string) reader.Value;
        }

        string ReadString()
        {
            NextAndExpect(JsonToken.String);
            return (string) reader.Value;
        }

        string ReadStringAndEndObject()
        {
            var s = ReadString();
            NextAndExpect(JsonToken.EndObject);
            return s;
        }

        void NextAndExpect(JsonToken tokenType)
        {
            Next();
            Expect(tokenType);
        }

        void Expect(JsonToken tokenType)
        {
            if (reader.TokenType != tokenType)
                Unexpected();
        }

        Value Unexpected()
        {
            //todo: FaunaException for invalid json
            throw new NotSupportedException(reader.TokenType.ToString());
        }
    }

    static class JsonWriterExtensions
    {
        public static void WriteArray(this JsonWriter writer, IEnumerable<Expr> vals)
        {
            writer.WriteStartArray();
            foreach (var _ in vals)
                _.WriteJson(writer);
            writer.WriteEndArray();
        }

        public static void WriteObject(this JsonWriter writer, string name, Expr value)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(name);
            value.WriteJson(writer);
            writer.WriteEndObject();
        }

        public static void WriteObject<TValue>(this JsonWriter writer, IEnumerable<KeyValuePair<string, TValue>> props) where TValue : Expr
        {
            writer.WriteStartObject();
            foreach (var kv in props)
            {
                writer.WritePropertyName(kv.Key);
                kv.Value.WriteJson(writer);
            }
            writer.WriteEndObject();
        }
    }

}
