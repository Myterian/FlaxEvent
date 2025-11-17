// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using FlaxEngine;

namespace FlaxEvent;

/// <summary>
/// Test Script.
/// </summary>
public class Test : Script
{
    // public bool IsSomething
    // {
    //     get => false;
    //     set => type = typeof(List<Actor>);
    // }
    // // [Serialize] public MethodInfo methodInfo;
    // [Serialize] public Type type;

    public FlaxEvent flaxEvent = new();

    /// <inheritdoc/>
    public override void OnStart()
    {
        // Here you can add code that needs to be called when script is created, just before the first game update
    }

    /// <inheritdoc/>
    public override void OnEnable()
    {
        // Here you can add code that needs to be called when script is enabled (eg. register for events)
    }

    /// <inheritdoc/>
    public override void OnDisable()
    {
        // Here you can add code that needs to be called when script is disabled (eg. unregister from events)
    }

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        // Here you can add code that needs to be called every frame
    }

    public void MyMethod(string someting)
    {
        // methodInfo.Invoke(this, ["dsf"]);
    }
}
