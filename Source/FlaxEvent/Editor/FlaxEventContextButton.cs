// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

using System;
using FlaxEditor.GUI.ContextMenu;
using FlaxEngine;
using FlaxEngine.GUI;

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

    public bool IsActiveTarget = false;

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

    public override void Draw()
    {
        Style current = Style.Current;
        Rectangle rect = new Rectangle(0f - X + 3f, 0f, Parent.Width - 6f, Height);
        Rectangle layoutRect = new Rectangle(0f, 0f, Width - 8f, Height);

        Color color = Color.OrangeRed;

        if(IsActiveTarget)
            color = Enabled ? current.BorderSelected : current.BorderSelected.RGBMultiplied(0.8f);
        else
            color = Enabled ? current.Foreground : current.ForegroundDisabled;


        if (IsMouseOver && Enabled)
            Render2D.FillRectangle(rect, current.LightBackground);

        else if (IsFocused)
            Render2D.FillRectangle(rect, current.LightBackground);
        

        base.Draw();

        Render2D.DrawText(current.FontMedium, Text, layoutRect, color, TextAlignment.Near, TextAlignment.Center);

        if (!string.IsNullOrEmpty(ShortKeys))
            Render2D.DrawText(current.FontMedium, ShortKeys, new Rectangle(layoutRect.X + ExtraAdjustmentAmount, layoutRect.Y, layoutRect.Width, layoutRect.Height), color, TextAlignment.Far, TextAlignment.Center);
        

        SpriteHandle spriteHandle = Checked ? current.CheckBoxTick : Icon;
        if (spriteHandle.IsValid)
            Render2D.DrawSprite(spriteHandle, new Rectangle(-15f, (Height - 14f) / 2f, 14f, 14f), color);
        
    }
}

#endif