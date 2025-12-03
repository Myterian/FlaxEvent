// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FlaxEngine;

namespace FlaxEvents;

/// <summary>Serializes a <see cref="PersistentParameter"/> to json and deserializes the store ParameterValue for further usage</summary>
public class PersistentParameterConverter : JsonConverter
{
    // Returns the Persistent Parameter, but with the Parameter value converted to its original type
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        try
        {
            JObject obj = JObject.Load(reader);

            object paraValue = null;
            Type paraType = null;

            string typeName = (string)obj["ParameterType"];

            if (!string.IsNullOrEmpty(typeName))
                paraType = Type.GetType(typeName);

            JToken valueToken = obj["ParameterValue"];

            if (valueToken != null && paraType != null)
                paraValue = valueToken.ToObject(paraType, serializer);

            PersistentParameter result = new()
            {
                ParameterType = paraType,
                ParameterValue = paraValue
            };

            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError("Persisent Parameter could not deserialized");
            Debug.LogException(ex);
            
            return new PersistentParameter { };
        }
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
