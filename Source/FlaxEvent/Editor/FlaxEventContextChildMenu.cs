// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

using System;
using System.Collections.Generic;
using FlaxEditor.GUI.ContextMenu;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEvents;

/// <summary>FlaxEventContextChildMenu class.</summary>
[HideInEditor]
public class FlaxEventContextChildMenu : FlaxEventContextButton
{
    public readonly ContextMenu ContextMenu = new ContextMenu();

    private void ShowChild(ContextMenu parentContextMenu)
    {
        float top = parentContextMenu.ItemsAreaMargin.Top;
        Float2 location = new Float2(base.Width, 0f - top);
        location = PointToParent(parentContextMenu, location);
        parentContextMenu.ShowChild(ContextMenu, location);
    }

    public override void Draw()
    {
        Style current = Style.Current;
        Rectangle rect = new Rectangle(0f - X + 3f, 0f, Parent.Width - 6f, Height);

        bool isOpened = ContextMenu.IsOpened;

        if (isOpened)
            Render2D.FillRectangle(rect, current.LightBackground);


        base.Draw();

        if (ContextMenu.HasChildren)
            Render2D.DrawSprite(current.ArrowRight, new Rectangle(Width - 15f, (Height - 12f) / 2f, 12f, 12f), (!Enabled) ? current.ForegroundDisabled : (isOpened ? current.BackgroundSelected : current.Foreground));

    }

    public override void OnMouseEnter(Float2 location)
    {
        if (ContextMenu.HasChildren)
        {
            ContextMenu parentContextMenu = base.ParentContextMenu;
            if (parentContextMenu != ContextMenu && !ContextMenu.IsOpened)
            {
                base.OnMouseEnter(location);
                ShowChild(parentContextMenu);
            }
        }
    }

    public override bool OnMouseUp(Float2 location, MouseButton button)
    {
        ContextMenu parentContextMenu = ParentContextMenu;
        if (parentContextMenu == ContextMenu)
            return true;

        if (ContextMenu.IsOpened)
            return true;

        ShowChild(parentContextMenu);
        return base.OnMouseUp(location, button);
    }


    [Obsolete("Don't use!")]
    public FlaxEventContextChildMenu(ContextMenu parent, string text, string shortKeys = "") : base(parent, text, shortKeys)
    {
        Text = text;
        CloseMenuOnClick = false;
    }

    public FlaxEventContextChildMenu(ContextMenu parent, string text, bool isActiveTarget, string shortKeys = "") : base(parent, text, null, null, null, null, shortKeys)
    {
        Text = text;
        CloseMenuOnClick = false;
        IsActiveTarget = isActiveTarget;
    }
}

#endif