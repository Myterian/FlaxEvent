// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using Newtonsoft.Json;

namespace FlaxEvents;

/// <summary>Data container for parameters of a <see cref="PersistentCall"/></summary>
[JsonConverter(typeof(PersistentParameterConverter))]
public class PersistentParameter
{
    /// <summary>Value of the parameter</summary>
    public object ParameterValue { get; init; }

    /// <summary>Type of the parameter</summary>
    public Type ParameterType { get; init; }
}
