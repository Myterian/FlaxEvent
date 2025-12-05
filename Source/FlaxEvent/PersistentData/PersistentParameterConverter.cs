// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FlaxEngine;
using System.Reflection;

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

            // Deserialising the ParameterType
            string typeName = (string)obj["ParameterType"];

            if (!string.IsNullOrEmpty(typeName))
                paraType = Type.GetType(typeName);

            // This is for parameters, that use custom classes/structs/Actors/Scripts/etc.
            // Project-Level assemblies are not guaranteed to be loaded during deserialization.
            // Loading them manually avoids errors and empty parameters.
            if (paraType != null)
            {
                AssemblyName typeAssemblyName = new AssemblyName(paraType.Assembly.FullName);
                Assembly.Load(typeAssemblyName);
            }
            
            // Deserialising the ParameterValue
            JToken valueToken = obj["ParameterValue"];

            // ParameterValue might be a type, which has to be resolved manually
            if (paraType != null && valueToken != null && typeof(Type).IsAssignableFrom(paraType))
            {
                string valueName = valueToken.ToObject<string>();
                Type valueAsType = Type.GetType(valueName);

                AssemblyName valueAssemblyName = new AssemblyName(valueAsType.Assembly.FullName);
                Assembly.Load(valueAssemblyName);

                paraValue = valueAsType;
            }
            
            // If ParameterValue is not a type, but a regular object (most cases)
            if (paraValue == null && valueToken != null && paraType != null)
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
        writer.WriteValue(parameter.ParameterType?.AssemblyQualifiedName);
        writer.WritePropertyName("ParameterValue");
        serializer.Serialize(writer, parameter.ParameterValue);
        writer.WriteEndObject();
    }
    
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(PersistentParameter);
    }
}
