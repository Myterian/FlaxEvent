// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using FlaxEngine;
using Object = FlaxEngine.Object;

namespace FlaxEvents;

/// <summary>Stores infos about object and method/member, that will be dynamically invoked by a <see cref="FlaxEventBase"/></summary>
public record struct PersistentCall
{
    /// <summary>Editor-Configured invokation parameters</summary>
    public PersistentParameter[] Parameters = [];

    /// <summary>The parent actor of the <see cref="TargetObject"/>. This is used for editor purposes, ie. listing all available scripts and methods.</summary>
    public Actor Parent => parent;

    /// <summary>Editor-Configured invokation target</summary>
    public Object TargetObject => targetObject;

    /// <summary>Editor-Configured invokation method</summary>
    public string MethodName => methodName;

    /// <summary>MethodInfo of the target method</summary>
    public MethodInfo MethodInfo => methodInfo ??= CacheMethodInfo();

    public Delegate Delegate => cachedDelegate ??= GetDelegate();

    [Serialize] private string methodName = string.Empty;

    [Serialize] private Actor parent = null;

    [Serialize] private Object targetObject = null;

    

    private MethodInfo methodInfo;

    private Delegate cachedDelegate;

    /// <summary>Enables or disables the invokation of this call</summary>
    public bool IsEnabled = true;

    /// <summary>if true, the call tries to use the invokation parameters if true, otherwise only uses the editor configured parameters</summary>
    public bool UseRuntimeParameters = true;

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

    /// <summary>Caches the method info for runtime, because flax doesn't serialize methodinfo types</summary>
    private MethodInfo CacheMethodInfo()
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

        // This should never happen
        return null;
    }

    /// <summary>Creates a delegate, that can be invoked</summary>
    /// <returns>Delegate</returns>
    private Delegate GetDelegate()
    {
        if (MethodInfo == null)
            return null;

        Type[] parameterTypes = GetParameterTypes();

        Type delegateType = Expression.GetActionType(parameterTypes);

        if (MethodInfo.ReturnType == typeof(void))
            return MethodInfo.CreateDelegate(delegateType, TargetObject);

        ParameterExpression[] paramsExpr = parameterTypes.Select(Expression.Parameter).ToArray();
        MethodCallExpression call = TargetObject != null ? Expression.Call(Expression.Constant(TargetObject), MethodInfo, paramsExpr) : Expression.Call(MethodInfo, paramsExpr);
        LambdaExpression ignoreReturn = Expression.Lambda(delegateType, Expression.Block(call, Expression.Empty()), paramsExpr);

        return ignoreReturn.Compile();
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

    /// <summary>Invokes the stored persistent action, if <see cref="IsEnabled"/> is true</summary>
    /// <param name="eventParams">Invokation parameters of an event. Will be ignored, when method signatures don't match.</param>
    public void Invoke(object[] eventParams)
    {
        if (!IsEnabled || MethodInfo == null)
            return;


        // Parameter signature matching check
        bool canUseRuntimeParams = UseRuntimeParameters && eventParams != null ? eventParams.Length == Parameters.Length : false;

        if (UseRuntimeParameters)
        {
            // TODO: Instead of instantly returning false when parameter types dont match, the types could be checked
            // for assignability, like with ints and floats, where an int gets automatically converted to a float.
            // useRuntimeParams = Parameters[i].ParameterType.IsAssignableFrom(eventParams[i].GetType());
            // Q: Will there be a noticable performance difference, when checking for 100 invokes?

            for (int i = 0; canUseRuntimeParams && i < eventParams.Length; i++)
                if (Parameters[i].ParameterType == null || eventParams[i].GetType() != Parameters[i].ParameterType)
                    canUseRuntimeParams = false;
        }
        
        // TODO: Check that saved parameters match the parameter types of the target method

        // TODO: useRuntimeParams can probably be cached, since the event signature and this call signature won't
        // change at runtime. Also, for micro-optimization, the target method could be susbscribed to the action
        // delegate in the main event, but there isn't a proper pipeline for that right now.
        // Not sure if worth exploring.

        // TODO: Stress test witch a lot of persistent calls. Current testen with ~5 calls yields same result
        // both with MethodInfo.Invoke and Delegate.DynamicInvoke

        // Early exit, we don't need to convert the parameters if we use runtime params
        if (canUseRuntimeParams)
        {
            // Delegate?.DynamicInvoke(eventParams);
            MethodInfo?.Invoke(TargetObject, eventParams);
            return;
        }

        // Use the serialized parameters
        object[] runtimeParameter = new object[Parameters.Length];

        for (int i = 0; i < Parameters.Length; i++)
            runtimeParameter[i] = Parameters[i].ParameterValue;

        // Delegate?.DynamicInvoke(runtimeParameter);
        MethodInfo?.Invoke(TargetObject, runtimeParameter);
    }

    public PersistentCall()
    {
    }

}
