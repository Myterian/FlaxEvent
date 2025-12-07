// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Reflection;
using FlaxEngine;
using Object = FlaxEngine.Object;

namespace FlaxEvents;

/// <summary>Stores infos about object and method/member, that will be dynamically invoked by a <see cref="FlaxEventBase"/></summary>
public class PersistentCall
{
    /// <summary>Editor-Configured method name</summary>
    [Serialize] private string methodName = string.Empty;

    /// <summary>Editor-Configured invokation parameters</summary>
    public PersistentParameter[] Parameters = [];

    /// <summary>Cached parameters used at runtime</summary>
    private object[] cachedParameterValues;

    /// <summary>Editor-Configured parent of target (is really only used in editor for ui purposes)</summary>
    [Serialize] private Actor parent = null;

    /// <summary>Editor-Configured target object</summary>
    [Serialize] private Object targetObject = null;
    
    /// <summary>Cached methodinfo</summary>
    private MethodInfo methodInfo;

    /// <summary>Enables or disables the invokation of this call</summary>
    [Serialize] private bool isEnabled = true;

    /// <summary>if true, the call tries to use the invokation parameters if true, otherwise only uses the editor configured parameters</summary>
    [Serialize] private bool tryUseRuntimeParameters = false;

    /// <summary>The parent actor of the <see cref="TargetObject"/>. This is used for editor purposes, ie. listing all available scripts and methods.</summary>
    public Actor Parent => parent;

    /// <summary>Editor-Configured invokation target</summary>
    public Object TargetObject => targetObject;

    /// <summary>MethodInfo of the target method</summary>
    public MethodInfo MethodInfo => methodInfo ??= GetMethodInfo();

    /// <summary>Editor-Configured invokation method</summary>
    public string MethodName => methodName;

    /// <summary>The enabled state of this <see cref="PersistentCall"/>. Disabled calls are not invoked.</summary>
    public bool IsEnabled => isEnabled;

    /// <summary>if true, the call tries to use the invokation parameters if true, otherwise only uses the editor configured parameters</summary>
    public bool TryUseRuntimeParameters => tryUseRuntimeParameters;

    /// <summary>Sets the parent of the call. For editor purposes, this also sets the target object, but that can be overriden.</summary>
    /// <param name="newParent">The new parent actor, where the target of this call is.</param>
    public void SetParent(Actor newParent)
    {
        parent = newParent;
        targetObject = parent;
        methodName = null;
        methodInfo = null;
    }

    /// <summary>Sets the target of this call</summary>
    /// <param name="newTarget">The target object (actor or script)</param>
    public void SetTarget(Object newTarget)
    {
        targetObject = newTarget;
        methodName = null;
        methodInfo = null;
    }

    /// <summary>Sets the name of the target method</summary>
    /// <param name="name">The method name</param>
    public void SetMethodName(string name)
    {
        methodName = name;
        methodInfo = null;
    }

    /// <summary>Sets the enabled state of this call. Disabled calls are not invoked</summary>
    /// <param name="enable">true for enabled, false for disabled</param>
    public void SetEnabled(bool enable)
    {
        isEnabled = enable;
    }

    /// <summary>Enables or disables the use of runtime parameters. If enabled, the invoke tries to apply the event arguments to the target method.</summary>
    /// <param name="enable">true for enabled, false for disabled</param>
    public void SetUseRuntimeParameters(bool enable)
    {
        tryUseRuntimeParameters = enable;
    }

    /// <summary>Invokes the stored persistent action, if <see cref="IsEnabled"/> is true</summary>
    /// <param name="runtimeParams">Invokation parameters of an event. Will be ignored, when method signatures don't match.</param>
    public void Invoke(object[] runtimeParams)
    {
        if (!IsEnabled || MethodInfo == null)
            return;

        // Parameter signature matching check
        bool canUseRuntimeParams = TryUseRuntimeParameters && runtimeParams != null ? runtimeParams.Length == Parameters.Length : false;

        if (canUseRuntimeParams)
        {
            // TODO: Instead of instantly returning false when parameter types dont match, the types could be checked
            // for assignability, like with floats that can be assigned from ints.
            // useRuntimeParams = Parameters[i].ParameterType.IsAssignableFrom(eventParams[i].GetType());
            // Q: Will there be a noticable performance difference, when checking for 100 invokes?

            for (int i = 0; canUseRuntimeParams && i < runtimeParams.Length; i++)
                if (Parameters[i].ParameterType == null || runtimeParams[i].GetType() != Parameters[i].ParameterType)
                    canUseRuntimeParams = false;
        }

        // TODO: useRuntimeParams can probably be cached, since the event signature and this call signature won't
        // change at runtime. Also, for micro-optimization, the target method could be susbscribed to the action
        // delegate in the main event, but there isn't a proper pipeline for that right now.
        // Not sure if worth exploring.

        // Early exit, we don't need to convert the parameters if we use runtime params
        if (canUseRuntimeParams)
        {
            MethodInfo?.Invoke(TargetObject, runtimeParams);
            return;
        }

        // Use the serialized parameters
        cachedParameterValues ??= GetParameterValues();
        MethodInfo?.Invoke(TargetObject, cachedParameterValues);
    }

    /// <summary>Gets an array containing the types of the stored <see cref="PersistentParameter"/></summary>
    /// <returns>Type[]</returns>
    private Type[] GetParameterTypes()
    {
        Type[] parameterTypes = new Type[Parameters.Length];

        for (int i = 0; i < parameterTypes.Length; i++)
            parameterTypes[i] = Parameters[i].ParameterType;

        return parameterTypes;
    }

    /// <summary>Gets an array containing the values of the stored <see cref="PersistentParameter"/></summary>
    /// <returns>object[]</returns>
    private object[] GetParameterValues()
    {
        object[] values = new object[Parameters.Length];

        for (int i = 0; i < values.Length; i++)
            values[i] = Parameters[i].ParameterValue;

        return values;
    }

    /// <summary>Caches the method info for runtime, because flax doesn't serialize methodinfo types</summary>
    /// <returns>MethodInfo of target method. null if failed.</returns>
    private MethodInfo GetMethodInfo()
    {
        if (TargetObject == null || string.IsNullOrEmpty(MethodName))
            return null;

        // This is the more compiclated version of TargetObject.GetType().GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);,
        // but this allows a persistent call to use a specific method, even when overloads of said method exists.
        // Basically, we iterate over every method in a type and select the method with the matching name and parameters
        var methodinfos = TargetObject.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);

        for (int i = 0; i < methodinfos.Length; i++)
        {
            if (methodinfos[i].Name != MethodName)
                continue;

            var types = methodinfos[i].GetParameterTypes();

            if (types.Length != Parameters.Length)
                continue;

            bool isFound = true;

            for (int x = 0; x < types.Length; x++)
            {
                if (types[x] != Parameters[x].ParameterType)
                {
                    isFound = false;
                    break;
                }
            }

            if (!isFound)
                continue;

            return methodinfos[i];
        }

        return null;
    }

    public PersistentCall()
    {
    }

}
