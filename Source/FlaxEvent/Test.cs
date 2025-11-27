// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FlaxEngine;
using Debug = FlaxEngine.Debug;

namespace FlaxEvents;

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
    // public List<PersistentCall> persistentCalls = [new PersistentCall()];

    public FlaxEvent OnSomething = new();

    public FlaxEvent<string, decimal, float> OnEvent = new();


    /// <inheritdoc/>
    public override void OnEnable()
    {
        // OnSomething?.Invoke();
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyboardKeys.Spacebar))
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            OnEvent?.Invoke("123", 1, 2);
            stopwatch.Stop();
            
            Debug.Log($"{stopwatch.ElapsedTicks} ticks, which are {stopwatch.Elapsed.TotalMilliseconds} ms");
        }
    }


    public void MyMethod(Vector3 vector3)
    {
        Debug.Log("FlaxEvent Invoked! " + vector3);
    }

    // [Button("Subscribe")]
    public void Subscribe()
    {
        // OnSomething?.AddListener(MyMethod);
        // OnSomething.PersistentCallList = new();
        
    }
}
