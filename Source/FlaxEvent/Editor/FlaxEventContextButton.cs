// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using FlaxEditor.GUI.ContextMenu;
using FlaxEngine;

namespace FlaxEvent;

/// <summary>
/// FlaxEventContextButton class.
/// </summary>
public class FlaxEventContextButton : ContextMenuButton
{
    public string MethodName => methodName;

    public Type[] ParameterTypes => parameterTypes;

    private string methodName;

    private Type[] parameterTypes;

    [Obsolete("Don't you dare use this!")]
    public FlaxEventContextButton(ContextMenu parent, string text, string shortKeys = "") : base(parent, text, shortKeys)
    {
        Parent = parent.ItemsContainer;
        Text = text;
        ShortKeys = shortKeys;
    }

    public FlaxEventContextButton(ContextMenu parent, string text, string method, Type[] parameters, Action<ContextMenuButton> action, string shortKeys = "") : base(parent, text, shortKeys)
    {
        Parent = parent.ItemsContainer;
        Text = text;
        ShortKeys = shortKeys;
        parameterTypes = parameters;
        methodName = method;
        ButtonClicked += action;
    }
}

public static class ContextMenuExtension
{
    public static ContextMenuButton AddButton(this ContextMenu menu, string buttonDisplayText, string methodName, Type[] parameters, Action<ContextMenuButton> action, string shortKeys = "")
    {
        return new FlaxEventContextButton(menu, buttonDisplayText, methodName, parameters, action, shortKeys);
    }
}
