// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using FlaxEngine;
using Object = FlaxEngine.Object;

namespace FlaxEvent;

/// <summary>Stores infos about object and method/member, that will be dynamically invoked by a <see cref="FlaxEventBase"/></summary>
public record struct PersistentCall
{
    /// <summary>Editor-Configured invokation parameters</summary>
    public PersistentParameter[] Parameters = [];

    /// <summary>The parent actor of the <see cref="TargetObject"/>. If the <see cref="TargetObject"/> is an actor, this will be null.</summary>
    public Actor Parent = null;

    /// <summary>Editor-Configured invokation target</summary>
    public Object TargetObject = null;

    /// <summary>Editor-Configured invokation method</summary>
    public string MethodName = string.Empty;

    /// <summary>MethodInfo of the target method</summary>
    public MethodInfo MethodInfo => methodInfo ??= CacheMethodInfo();

    public Delegate Delegate => cachedDelegate ??= GetDelegate();

    private MethodInfo methodInfo;

    private Delegate cachedDelegate;

    /// <summary>Enables or disables the invokation of this call</summary>
    public bool IsEnabled = true;
    
    /// <summary>Caches the method info for runtime, because flax doesn't serialize methodinfo types</summary>
    private MethodInfo CacheMethodInfo()
    {
        if (TargetObject == null || string.IsNullOrEmpty(MethodName))
            return null;

        return TargetObject.GetType().GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
    }

    private Delegate GetDelegate()
    {
        if (MethodInfo == null)
            return null;

        Type[] parameterTypes = new Type[Parameters.Length];

        for (int i = 0; i < parameterTypes.Length; i++)
            parameterTypes[i] = Parameters[i].ParameterType;

        Type delegateType = Expression.GetActionType(parameterTypes);

        if (MethodInfo.ReturnType == typeof(void))
            return MethodInfo.CreateDelegate(delegateType, TargetObject);

        ParameterExpression[] paramsExpr = parameterTypes.Select(Expression.Parameter).ToArray();

        MethodCallExpression call = TargetObject != null ? Expression.Call(Expression.Constant(TargetObject), MethodInfo, paramsExpr) : Expression.Call(MethodInfo, paramsExpr);

        LambdaExpression ignoreReturn = Expression.Lambda(delegateType, Expression.Block(call, Expression.Empty()), paramsExpr);

        return ignoreReturn.Compile();
    }

    /// <summary>Invokes the stored persistent action, if <see cref="IsEnabled"/> is true</summary>
    /// <param name="eventParams">Invokation parameters of an event. Will be ignored, when method signatures don't match.</param>
    public void Invoke(object[] eventParams)
    {
        if (!IsEnabled || MethodInfo == null)
            return;

        // Parameter signature matching check
        bool useRuntimeParams = eventParams != null ? eventParams.Length == Parameters.Length : false;


        // TODO: Instead of instantly returning false when parameter types dont match, the types could be checked
        // for assignability, like with ints and floats, where an int gets automatically converted to a float.
        // useRuntimeParams = Parameters[i].ParameterType.IsAssignableFrom(eventParams[i].GetType());
        // Q: Will there be a noticable performance difference, when checking for 100 invokes?

        for (int i = 0; useRuntimeParams && i < eventParams.Length; i++)
            if (Parameters[i].ParameterType == null || eventParams[i].GetType() != Parameters[i].ParameterType)
                useRuntimeParams = false;

        // TODO: Make sure the saved parameters match the parameter types of the target method

        // TODO: useRuntimeParams can probably be cached, since the event signature and this call signature won't
        // change at runtime. Also, for micro-optimization, the target method could be susbscribed to the action
        // delegate in the main event, but there isn't a proper pipeline for that right now.
        // Not sure if worth exploring.

        // Early exit, we don't need to convert the parameters if we use runtime params
        if (useRuntimeParams)
        {
            Delegate?.DynamicInvoke(eventParams);
            // MethodInfo?.Invoke(TargetObject, eventParams);
            return;
        }

        // Use the serialized parameters
        object[] runtimeParameter = new object[Parameters.Length];

        for (int i = 0; i < Parameters.Length; i++)
            runtimeParameter[i] = Parameters[i].GetValue();

        Delegate?.DynamicInvoke(runtimeParameter);

        // MethodInfo?.Invoke(TargetObject, runtimeParameter);
    }

    public PersistentCall()
    {
    }

}
