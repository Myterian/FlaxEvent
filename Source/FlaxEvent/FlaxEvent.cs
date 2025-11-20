// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using FlaxEngine;
using Object = FlaxEngine.Object;

namespace FlaxEvent;

/// <summary>Base Class of all FlaxEvent types</summary>
[Serializable]
public abstract class FlaxEventBase
{
    /// <summary>List of all editor-configured actions</summary>
    public List<PersistentCall> PersistentCallList = new();

    // [Serialize] private List<PersistentCall> persistentCallList = new();

    /// <summary>Add a new persistent call</summary>
    /// <param name="call">Call to add</param>
    internal void AddPersistentListener(PersistentCall call) => PersistentCallList.Add(call);

    /// <summary>Clears the persistent calls list</summary>
    internal void ClearPersistent() => PersistentCallList.Clear();

    /// <summary>Removes the first occurance of a persistent call</summary>
    /// <param name="call">Call to remove</param>
    internal void RemovePersistentListener(PersistentCall call) => PersistentCallList.Remove(call);

    /// <summary>Removes a persistent call at an index</summary>
    /// <param name="index">Index of the element to remove</param>
    internal void RemovePersistentListener(int index) => PersistentCallList.RemoveAt(index);

    /// <summary>Replaces the current peristent calls list</summary>
    /// <param name="newCalls">New List</param>
    internal void SetPersistentCalls(List<PersistentCall> newCalls) => PersistentCallList = newCalls;

    /// <summary>Invokes persistent listeners. Get invoked automatically, when calling <see cref="FlaxEvent.Invoke"/></summary>
    protected void InvokePersistent(object[] args)
    {
        try
        {
            for (int i = 0; i < PersistentCallList.Count; i++)
                PersistentCallList[i].Invoke(args);
        }
        catch (AggregateException ex)
        {
            Debug.Log(ex.InnerExceptions.Count);
            for (int i = 0; i < ex.InnerExceptions.Count; i++)
            {
                Exception exception = ex.InnerExceptions[i];
                Debug.LogError("FlaxEvent: One of the persistent listeners caused an error:\n" + exception);
            }
        }
    }

}

[Serializable]
public class FlaxEvent : FlaxEventBase
{
    private Action runtimeAction = delegate { };

    /// <summary>Invokes persistent and runtime listeners of this event</summary>
    public void Invoke()
    {
        runtimeAction?.Invoke();
        InvokePersistent(null);
    }

    public void AddListener(Action listener, bool saveAdd = true)
    {
        if (saveAdd)
            runtimeAction -= listener;

        runtimeAction += listener;
    }

    public void RemoveListener(Action listener)
    {
        runtimeAction -= listener;
    }

}

public class FlaxEvent<T0> : FlaxEventBase
{
    private Action<T0> runtimeAction = delegate { };

    public void Invoke(T0 parameter)
    {
        runtimeAction?.Invoke(parameter);
        InvokePersistent([parameter]);
    }

    public void AddListener(Action<T0> listener, bool saveAdd = true)
    {
        if (saveAdd)
            runtimeAction -= listener;

        runtimeAction += listener;
    }

    public void RemoveListener(Action<T0> listener)
    {
        runtimeAction -= listener;
    }
}

public class FlaxEvent<T0, T1> : FlaxEventBase
{
    private Action<T0, T1> runtimeAction = delegate { };

    public void Invoke(T0 parameter, T1 parameter1)
    {
        runtimeAction?.Invoke(parameter, parameter1);
        InvokePersistent([parameter, parameter1]);
    }

    public void AddListener(Action<T0, T1> listener, bool saveAdd = true)
    {
        if (saveAdd)
            runtimeAction -= listener;

        runtimeAction += listener;
    }

    public void RemoveListener(Action<T0, T1> listener)
    {
        runtimeAction -= listener;
    }
}

public class FlaxEvent<T0, T1, T2> : FlaxEventBase
{
    private Action<T0, T1, T2> runtimeAction = delegate { };

    public void Invoke(T0 parameter, T1 parameter1, T2 parameter2)
    {
        runtimeAction?.Invoke(parameter, parameter1, parameter2);
        InvokePersistent([parameter, parameter1, parameter2]);
    }

    public void AddListener(Action<T0, T1, T2> listener, bool saveAdd = true)
    {
        if (saveAdd)
            runtimeAction -= listener;

        runtimeAction += listener;
    }

    public void RemoveListener(Action<T0, T1, T2> listener)
    {
        runtimeAction -= listener;
    }
}

public class FlaxEvent<T0, T1, T2, T3> : FlaxEventBase
{
    private Action<T0, T1, T2, T3> runtimeAction = delegate { };

    public void Invoke(T0 parameter, T1 parameter1, T2 parameter2, T3 parameter3)
    {
        runtimeAction?.Invoke(parameter, parameter1, parameter2, parameter3);
        InvokePersistent([parameter, parameter1, parameter2, parameter3]);
    }

    public void AddListener(Action<T0, T1, T2, T3> listener, bool saveAdd = true)
    {
        if (saveAdd)
            runtimeAction -= listener;

        runtimeAction += listener;
    }

    public void RemoveListener(Action<T0, T1, T2, T3> listener)
    {
        runtimeAction -= listener;
    }
}
