// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEditor.GUI.ContextMenu;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEvent;

/// <summary>Custom editor for <see cref="PersistentCall"/> elements, that appear in the <see cref="FlaxEventEditor"/></summary>
// [CustomEditor(typeof(PersistentCall)), DefaultEditor]
public class PersistentCallElement : GroupElement
{
    public FlaxEventEditor LinkedEditor;

    private int callIndex = -1;

    public void Init(FlaxEventEditor editor, int index)
    {
        LinkedEditor = editor;
        callIndex = index;

        // Pivot = Float2.Zero;
        // HeaderHeight = 18;
        // EnableDropDownIcon = true;
        // var icons = FlaxEditor.Editor.Instance.Icons;
        // ArrowImageClosed = new SpriteBrush(icons.ArrowRight12);
        // ArrowImageOpened = new SpriteBrush(icons.ArrowDown12);
        // HeaderText = "<null>";

        // Offsets = new Margin(7, 7, 0, 0);

        // HeaderTextMargin = new Margin(36, 0, 0, 0);
        // _arrangeButtonRect = new Rectangle(16, 3, 12, 12);

        // MouseButtonRightClicked += SetupContextMenu;
        // IsClosedChanged += OnIsClosedChanged;

        // var propertyList = new PropertiesListElement();
        // this.

        Panel.HeaderText = "<null>";
        Panel.HeaderTextMargin = new(44, 0, 0, 0);

        float height = Panel.HeaderHeight;

        var toggle = new CheckBox
        {
            TooltipText = "If checked, the target will be invoked",
            IsScrollable = false,
            Checked = true, // Change to persistentcall.IsEnabled
            Parent = Panel,
            Size = new(height),
            Bounds = new(height, 0, height, height),
            BoxSize = height - 4
        };

        var dragButton = new Button
        {
            BackgroundBrush = new SpriteBrush(Editor.Instance.Icons.DragBar12),
            AutoFocus = true,
            IsScrollable = false,
            BackgroundColor = FlaxEngine.GUI.Style.Current.ForegroundGrey,
            BackgroundColorHighlighted = FlaxEngine.GUI.Style.Current.ForegroundGrey.RGBMultiplied(1.5f),
            HasBorder = false,
            Parent = Panel,
            Bounds = new(toggle.Right, 1, height, height),
            Scale = new(0.9f)
        };

        var propertyList = AddPropertyItem("Target", "The target of this event");
        propertyList.Custom<FlaxObjectRefPickerControl>();
        propertyList.ComboBox();

        toggle.StateChanged += SetCallEnabledState;
    }

    /// <summary>Sets the enabled state of the linked <see cref="PersistentCall"/></summary>
    /// <param name="box">The checkbox to use for the enabled/checked state. Checkbox gets passed via action delegate</param>
    private void SetCallEnabledState(CheckBox box)
    {
        if (callIndex < 0 || LinkedEditor == null)
            return;

        PersistentCall oldCall = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList[callIndex];

        PersistentCall newCall = new()
        {
            TargetObject = oldCall.TargetObject,
            MethodName = oldCall.MethodName,
            Parameters = oldCall.Parameters,
            IsEnabled = box.Checked
        };

        (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList[callIndex] = newCall;
    }


    // private void SetupContextMenu(DropPanel dropPanel, Float2 mouseLocation)
    // {
    //     ContextMenu contextMenu = new();
    //     contextMenu.ItemsContainer.RemoveChildren();


    //     contextMenu.AddButton("Remove", () => Debug.Log("Clicked Remove"));
    //     contextMenu.Show(dropPanel, mouseLocation);

    //     // menu.AddButton("Copy", linkedEditor.Copy);
    //     // var b = menu.AddButton("Duplicate", () => Editor.Duplicate(Index));
    //     // b.Enabled = !Editor._readOnly && Editor._canResize;
    //     // b = menu.AddButton("Paste", linkedEditor.Paste);
    //     // b.Enabled = linkedEditor.CanPaste && !Editor._readOnly;

    //     // menu.AddSeparator();
    //     // b = menu.AddButton("Move up", OnMoveUpClicked);
    //     // b.Enabled = Index > 0 && !Editor._readOnly;

    //     // b = menu.AddButton("Move down", OnMoveDownClicked);
    //     // b.Enabled = Index + 1 < Editor.Count && !Editor._readOnly;

    //     // b = menu.AddButton("Remove", OnRemoveClicked);
    //     // b.Enabled = !Editor._readOnly && Editor._canResize;
    // }

    // public PersistentCallElement()
    // {
    // }
}
/*
private class CollectionDropPanel : DropPanel
{
    /// <summary>
    /// The collection editor.
    /// </summary>
    public CollectionEditor Editor;

    /// <summary>
    /// The index of the item (zero-based).
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// The linked editor.
    /// </summary>
    public CustomEditor LinkedEditor;

    private bool _canReorder = true;

    private Rectangle _arrangeButtonRect;
    private bool _arrangeButtonInUse;

    public void Setup(CollectionEditor editor, int index, bool canReorder = true)
    {
        Pivot = Float2.Zero;
        HeaderHeight = 18;
        _canReorder = canReorder;
        EnableDropDownIcon = true;
        var icons = FlaxEditor.Editor.Instance.Icons;
        ArrowImageClosed = new SpriteBrush(icons.ArrowRight12);
        ArrowImageOpened = new SpriteBrush(icons.ArrowDown12);
        HeaderText = $"Element {index}";
        
        string saveName = string.Empty;
        if (editor.Presenter?.Owner is PropertiesWindow propertiesWindow)
        {
            var selection = FlaxEditor.Editor.Instance.SceneEditing.Selection[0];
            if (selection != null)
            {
                saveName += $"{selection.ID},";
            }
        }
        else if (editor.Presenter?.Owner is PrefabWindow prefabWindow)
        {
            var selection = prefabWindow.Selection[0];
            if (selection != null)
            {
                saveName += $"{selection.ID},";
            }
        }
        if (editor.ParentEditor?.Layout.ContainerControl is DropPanel pdp)
        {
            saveName += $"{pdp.HeaderText},";
        }
        if (editor.Layout.ContainerControl is DropPanel mainGroup)
        {
            saveName += $"{mainGroup.HeaderText}";
            IsClosed = FlaxEditor.Editor.Instance.ProjectCache.IsGroupToggled($"{saveName}:{index}");
        }
        else
        {
            IsClosed = false;
        }
        
        Editor = editor;
        Index = index;
        Offsets = new Margin(7, 7, 0, 0);

        MouseButtonRightClicked += OnMouseButtonRightClicked;
        if (_canReorder)
        {
            HeaderTextMargin = new Margin(18, 0, 0, 0);
            _arrangeButtonRect = new Rectangle(16, 3, 12, 12);
        }
        IsClosedChanged += OnIsClosedChanged;
    }

    private void OnIsClosedChanged(DropPanel panel)
    {
        string saveName = string.Empty;
        if (Editor.Presenter?.Owner is PropertiesWindow pw)
        {
            var selection = FlaxEditor.Editor.Instance.SceneEditing.Selection[0];
            if (selection != null)
            {
                saveName += $"{selection.ID},";
            }
        }
        else if (Editor.Presenter?.Owner is PrefabWindow prefabWindow)
        {
            var selection = prefabWindow.Selection[0];
            if (selection != null)
            {
                saveName += $"{selection.ID},";
            }
        }
        if (Editor.ParentEditor?.Layout.ContainerControl is DropPanel pdp)
        {
            saveName += $"{pdp.HeaderText},";
        }
        if (Editor.Layout.ContainerControl is DropPanel mainGroup)
        {
            saveName += $"{mainGroup.HeaderText}";
            FlaxEditor.Editor.Instance.ProjectCache.SetGroupToggle($"{saveName}:{Index}", panel.IsClosed);
        }
    }

    private bool ArrangeAreaCheck(out int index, out Rectangle rect)
    {
        var container = Parent;
        var mousePosition = container.PointFromScreen(Input.MouseScreenPosition);
        var barSidesExtend = 20.0f;
        var barHeight = 5.0f;
        var barCheckAreaHeight = 40.0f;
        var pos = mousePosition.Y + barCheckAreaHeight * 0.5f;

        for (int i = 0; i < (container.Children.Count + 1) / 2; i++) // Add 1 to pretend there is a spacer at the end.
        {
            var containerChild = container.Children[i * 2]; // times 2 to skip the value editor
            if (Mathf.IsInRange(pos, containerChild.Top, containerChild.Top + barCheckAreaHeight) || (i == 0 && pos < containerChild.Top))
            {
                index = i;
                var p1 = containerChild.UpperLeft;
                rect = new Rectangle(PointFromParent(p1) - new Float2(barSidesExtend * 0.5f, barHeight * 0.5f), Width + barSidesExtend, barHeight);
                return true;
            }
        }

        var p2 = container.Children[container.Children.Count - 1].BottomLeft;
        if (pos > p2.Y)
        {
            index = ((container.Children.Count + 1) / 2) - 1;
            rect = new Rectangle(PointFromParent(p2) - new Float2(barSidesExtend * 0.5f, barHeight * 0.5f), Width + barSidesExtend, barHeight);
            return true;
        }

        index = -1;
        rect = Rectangle.Empty;
        return false;
    }

    public override void Draw()
    {
        base.Draw();

        if (_canReorder)
        {
            var style = FlaxEngine.GUI.Style.Current;
            var mousePosition = PointFromScreen(Input.MouseScreenPosition);
            var dragBarColor = _arrangeButtonRect.Contains(mousePosition) ? style.Foreground : style.ForegroundGrey;
            Render2D.DrawSprite(FlaxEditor.Editor.Instance.Icons.DragBar12, _arrangeButtonRect, _arrangeButtonInUse ? Color.Orange : dragBarColor);
            if (_arrangeButtonInUse && ArrangeAreaCheck(out _, out var arrangeTargetRect))
            {
                Render2D.FillRectangle(arrangeTargetRect, style.Selection);
            }
        }
    }

    /// <inheritdoc />
    public override bool OnMouseDown(Float2 location, MouseButton button)
    {
        if (button == MouseButton.Left && _arrangeButtonRect.Contains(ref location))
        {
            _arrangeButtonInUse = true;
            Focus();
            StartMouseCapture();
            return true;
        }

        return base.OnMouseDown(location, button);
    }

    /// <inheritdoc />
    public override bool OnMouseUp(Float2 location, MouseButton button)
    {
        if (button == MouseButton.Left && _arrangeButtonInUse)
        {
            _arrangeButtonInUse = false;
            EndMouseCapture();
            if (ArrangeAreaCheck(out var index, out _))
            {
                Editor.Shift(Index, index);
            }
        }

        return base.OnMouseUp(location, button);
    }

    private void OnMouseButtonRightClicked(DropPanel panel, Float2 location)
    {
        if (LinkedEditor == null)
            return;
        var linkedEditor = LinkedEditor;
        var menu = new ContextMenu();

        menu.AddButton("Copy", linkedEditor.Copy);
        var b = menu.AddButton("Duplicate", () => Editor.Duplicate(Index));
        b.Enabled = !Editor._readOnly && Editor._canResize;
        var paste = menu.AddButton("Paste", linkedEditor.Paste);
        paste.Enabled = linkedEditor.CanPaste && !Editor._readOnly;

        if (_canReorder)
        {
            menu.AddSeparator();

            var moveUpButton = menu.AddButton("Move up", OnMoveUpClicked);
            moveUpButton.Enabled = Index > 0;

            var moveDownButton = menu.AddButton("Move down", OnMoveDownClicked);
            moveDownButton.Enabled = Index + 1 < Editor.Count;
        }

        b = menu.AddButton("Remove", OnRemoveClicked);
        b.Enabled = !Editor._readOnly && Editor._canResize;

        menu.Show(panel, location);
    }

    private void OnMoveUpClicked()
    {
        Editor.Move(Index, Index - 1);
    }

    private void OnMoveDownClicked()
    {
        Editor.Move(Index, Index + 1);
    }

    private void OnRemoveClicked()
    {
        Editor.Remove(Index);
    }
}
*/