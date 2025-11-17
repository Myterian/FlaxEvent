// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;

namespace FlaxEvent;

/// <summary>Data container for parameters of a <see cref="PersistentCall"/></summary>
public record struct PersistentParameter
{
    /// <summary>Value of the parameter</summary>
    public object ParameterValue;

    /// <summary>Type of the parameter</summary>
    public Type ParameterType;

    /// <summary>Converts the stored object to the runtime type</summary>
    /// <returns>Type-Converted object. Returns null if failed.</returns>
    public object GetValue()
    {
        if (ParameterValue == null || ParameterType == null)
            return null;

        return Convert.ChangeType(ParameterValue, ParameterType);
    }
}
