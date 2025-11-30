// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FlaxEvents;

/// <summary>Data container for parameters of a <see cref="PersistentCall"/></summary>
[JsonConverter(typeof(PersistentParameterConverter))]
public record struct PersistentParameter
{
    /// <summary>Value of the parameter</summary>
    public object ParameterValue;

    /// <summary>Type of the parameter</summary>
    public Type ParameterType;

    /// <summary>Cached value of the parameterValue conversion</summary>
    private object cachedValue;

    /// <summary>Converts the stored object to the runtime type</summary>
    /// <returns>Type-Converted object. Returns null if failed.</returns>
    public object GetValue()
    {
        if (ParameterValue == null || ParameterType == null)
            return null;

        if (cachedValue != null)
            return cachedValue;

        // Convert ParameterValue to Arrays, because Convert.ChangeType can't
        if (ParameterType.IsArray)
        {
            IList storedArray = ParameterValue as IList;
            Type elementType = ParameterType.GetElementType();
            int count = 0;

            if (storedArray != null)
                count = storedArray.Count;

            Array newArray = Array.CreateInstance(elementType, count);

            for (int i = 0; storedArray != null && i < count; i++)
                newArray.SetValue(Convert.ChangeType(storedArray[i], elementType), i);

            cachedValue = newArray;
            return newArray;
        }

        // Convert ParameterValue to Lists, because Convert.ChangeType can't
        if (ParameterType.IsGenericType && ParameterType.GetGenericTypeDefinition() == typeof(List<>))
        {
            IList storedList = ParameterValue as IList;
            Type elementType = ParameterType.GetGenericArguments()[0];
            IList newList = (IList)Activator.CreateInstance(ParameterType);
            int count = 0;

            if (storedList != null)
                count = storedList.Count;

            for (int i = 0; storedList != null && i < count; i++)
                newList.Add(Convert.ChangeType(storedList[i], elementType));

            cachedValue = newList;
            return newList;
        }

        cachedValue = Convert.ChangeType(ParameterValue, ParameterType);

        // Default
        return Convert.ChangeType(ParameterValue, ParameterType);
    }
}
