// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlaxEvents;

/// <summary>Serializes a <see cref="PersistentParameter"/> to json and deserializes the store ParameterValue for further usage</summary>
public class PersistentParameterConverter : JsonConverter
{
    // Returns the Persistent Parameter, but with the Parameter value converted to its original type
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        PersistentParameter result = new();

        string typeName = (string)obj["ParameterType"];

        if (!string.IsNullOrEmpty(typeName))
            result.ParameterType = Type.GetType(typeName);

        JToken valueToken = obj["ParameterValue"];

        if (valueToken != null && result.ParameterType != null)
            result.ParameterValue = valueToken.ToObject(result.ParameterType, serializer);

        return result;
    }

    public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
    {
        PersistentParameter parameter = (PersistentParameter)value;
        writer.WriteStartObject();
        writer.WritePropertyName("ParameterType");
        writer.WriteValue(parameter.ParameterType.AssemblyQualifiedName);
        writer.WritePropertyName("ParameterValue");
        serializer.Serialize(writer, parameter.ParameterValue);
        writer.WriteEndObject();
    }
    
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(PersistentParameter);
    }
}
