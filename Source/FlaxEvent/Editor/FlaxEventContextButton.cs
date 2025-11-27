// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using FlaxEditor.GUI.ContextMenu;
using FlaxEngine;

namespace FlaxEvents;

/// <summary>
/// FlaxEventContextButton class.
/// </summary>
[HideInEditor]
public class FlaxEventContextButton : ContextMenuButton
{
    public string MethodName => methodName;

    public Type[] ParameterTypes => parameterTypes;

    public FlaxEngine.Object TargetObject => targetObject;

    private string methodName;

    private Type[] parameterTypes;

    private FlaxEngine.Object targetObject;

    [Obsolete("Don't you dare use this!")]
    public FlaxEventContextButton(ContextMenu parent, string text, string shortKeys = "") : base(parent, text, shortKeys)
    {
        Parent = parent.ItemsContainer;
        Text = text;
        ShortKeys = shortKeys;
    }

    public FlaxEventContextButton(ContextMenu parent, string text, FlaxEngine.Object target, string method, Type[] parameters, Action<ContextMenuButton> action, string shortKeys = "") : base(parent, text, shortKeys)
    {
        Parent = parent.ItemsContainer;
        Text = text;
        ShortKeys = shortKeys;
        parameterTypes = parameters;
        methodName = method;
        targetObject = target;
        ButtonClicked += action;
    }
}

public static class ContextMenuExtension
{
    public static ContextMenuButton AddButton(this ContextMenu menu, string buttonDisplayText, FlaxEngine.Object target, string methodName, Type[] parameters, Action<ContextMenuButton> action, string shortKeys = "")
    {
        return new FlaxEventContextButton(menu, buttonDisplayText, target, methodName, parameters, action, shortKeys);
    }
}
