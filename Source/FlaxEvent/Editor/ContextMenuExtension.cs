// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

using System;
using FlaxEditor.GUI.ContextMenu;

namespace FlaxEvents;

/// <summary>Extends <see cref="ContextMenu"/> to include custom FlaxEvent buttons and child menus</summary>
public static class ContextMenuExtension
{
    public static ContextMenuButton AddButton(this ContextMenu menu, string buttonDisplayText, FlaxEngine.Object target, string methodName, int index,Type[] parameters, Action<ContextMenuButton> action, string shortKeys = "")
    {
        return new FlaxEventContextButton(menu, buttonDisplayText, target, methodName, index, parameters, action, shortKeys);
    }

    public static FlaxEventContextChildMenu AddChildMenu(this ContextMenu menu, string text, bool isActiveTarget, string shortKeys = "")
    {
        return new FlaxEventContextChildMenu(menu, text, isActiveTarget, shortKeys);
    }
}

#endif