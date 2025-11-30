// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlaxEvents;

/// <summary>Serializes a <see cref="FlaxEventBase"/> to json and deserializes the store PersistentCalls for further usage</summary>
public class FlaxEventConverter : JsonConverter
{
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);

        FlaxEventBase result = (FlaxEventBase)Activator.CreateInstance(objectType);
        List<PersistentCall> calls = new();

        JToken valueToken = obj["persistentCalls"];

        if (valueToken != null)
            calls = valueToken.ToObject<List<PersistentCall>>(serializer);

        result.SetPersistentCalls(calls);

        return result;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        FlaxEventBase obj = (FlaxEventBase)value;
        List<PersistentCall> calls = obj.PersistentCallList;

        writer.WriteStartObject();
        writer.WritePropertyName("persistentCalls");
        serializer.Serialize(writer, calls);

        writer.WriteEndObject();
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(FlaxEventBase).IsAssignableFrom(objectType);
    }

}
