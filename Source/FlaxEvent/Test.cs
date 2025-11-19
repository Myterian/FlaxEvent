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
    // public List<Vector3> Vectors = new();
    public List<PersistentCall> persistentCalls = [new PersistentCall()];

    public FlaxEvent OnSomething = new();


    /// <inheritdoc/>
    public override void OnEnable()
    {
        // OnSomething?.Invoke();
    }

    public void MyMethod()
    {
        Debug.Log("FlaxEvent Invoked!");
    }

    // [Button("Subscribe")]
    public void Subscribe()
    {
        // OnSomething?.AddListener(MyMethod);
        
    }
}
