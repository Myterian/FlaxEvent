// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

using System;
using System.Reflection;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.GUI.ContextMenu;
using FlaxEditor.Scripting;
using FlaxEditor;
using FlaxEngine;
using FlaxEngine.GUI;
using Object = FlaxEngine.Object;
using System.Text;
using FlaxEditor.CustomEditors.Elements;
using FlaxEngine.Utilities;

namespace FlaxEvents;

/// <summary>PersistentParameterEditor class.</summary>
public class PersistentCallEditor : CustomEditor
{
    private PersistentCallListEditor parentEditor;
    private GroupElement group;
    private SpaceElement dragShiftIndicatorTop;
    private DragOperation dragOperation;

    public int Index { get; private set; } = -1;

    public void Setup(PersistentCallListEditor editor, int elementIndex)
    {
        parentEditor = editor;
        Index = elementIndex;
    }

    /// <summary>Sets the open state of the group element</summary>
    /// <param name="open">open if true, close if false</param>
    public void SetOpen(bool open = true)
    {
        if (open)
            group.Panel.Open(false);
        else
            group.Panel.Close(false);
    }

    public override void Initialize(LayoutElementsContainer layout)
    {
        PersistentCall call = (PersistentCall)Values[0];

        Actor castActor = null;
        Script castScript = null;
        string headerText = "<null>";

        // Drag and Drop indicator on the Top of this editor
        dragShiftIndicatorTop = layout.Space(0.1f);
        // Scaling will make the spacer overlap the group, while not creating empty spaces between the call elements
        dragShiftIndicatorTop.ContainerControl.Scale = new(1, 50);

        // Properties group
        group = layout.Group(headerText);
        group.Panel.MouseButtonRightClicked += RightClickContextMenu;
        bool isCallEnabled = call.IsEnabled;

        group.Panel.HeaderTextMargin = new(44, 0, 0, 0);
        group.Panel.HeaderTextColor = isCallEnabled ? FlaxEngine.GUI.Style.Current.Foreground : FlaxEngine.GUI.Style.Current.ForegroundDisabled;
        group.Panel.EnableContainmentLines = false;
        
        float headerHeight = group.Panel.HeaderHeight;

        // Checkbox with enable/disable logic
        var toggle = new CheckBox
        {
            Checked = isCallEnabled,
            TooltipText = "If checked, the target will be invoked",
            IsScrollable = false,
            Parent = group.Panel,
            Size = new(headerHeight),
            Bounds = new(headerHeight, 0, headerHeight, headerHeight),
            BoxSize = headerHeight - 4
        };

        toggle.StateChanged += SetCallEnabledState;

        // Drag and Drop Reorder
        var dragButton = new Button
        {
            BackgroundBrush = new SpriteBrush(Editor.Instance.Icons.DragBar12),
            AutoFocus = true,
            IsScrollable = false,
            BackgroundColor = FlaxEngine.GUI.Style.Current.ForegroundGrey,
            BackgroundColorHighlighted = FlaxEngine.GUI.Style.Current.ForegroundGrey.RGBMultiplied(1.5f),
            HasBorder = false,
            Parent = group.Panel,
            Bounds = new(toggle.Right, 1, headerHeight, headerHeight),
            Scale = new(0.9f)
        };

        dragButton.HoverBegin -= StartAwaitingDrag;
        dragButton.HoverBegin += StartAwaitingDrag;

        dragButton.HoverEnd -= StopAwaitingDrag;
        dragButton.HoverEnd += StopAwaitingDrag;

        // Runtime Parameter enable/disable
        var runtimeCheckBox = new Button
        {

            BackgroundBrush = call.TryUseRuntimeParameters ? new SpriteBrush(Editor.Instance.Icons.Link32) : new SpriteBrush(Editor.Instance.Icons.BrokenLink32),
            BackgroundColor = call.TryUseRuntimeParameters ? FlaxEngine.GUI.Style.Current.BorderSelected.RGBMultiplied(1.25f) : FlaxEngine.GUI.Style.Current.ForegroundGrey,
            BackgroundColorHighlighted = call.TryUseRuntimeParameters ? FlaxEngine.GUI.Style.Current.BorderSelected.RGBMultiplied(1.5f) : FlaxEngine.GUI.Style.Current.ForegroundGrey.RGBMultiplied(1.5f),
            AnchorPreset = AnchorPresets.TopRight,
            Parent = group.Panel,
            Bounds = new(40, 0, headerHeight, headerHeight),
            HasBorder = false,
            TooltipText = "If linked, this listeners can use the event parameters.\n Requirement: The listener method must match the events signature."
        };

        runtimeCheckBox.ButtonClicked += SetUseRuntimeParameter;


        // Target Object Picker
        var targetPanel = group.VerticalPanel();
        var propertyList = targetPanel.AddPropertyItem("Target", "The target of this event call");
        var objectPicker = propertyList.Custom<FlaxObjectRefPickerControl>();


        if (call.TargetObject != null)
        {
            objectPicker.CustomControl.Value = call.TargetObject;

            Guid guid = call.TargetObject.ID;
            Object uncastObject = Object.Find<Object>(ref guid);

            castActor = uncastObject as Actor;
            castScript = uncastObject as Script;

            StringBuilder headerBuilder = new("<null>");

            if (call.Parent != null)
            {
                headerBuilder.Clear();
                headerBuilder.Append(call.Parent.Name);
            }

            if (uncastObject != null && call.Parent != call.TargetObject)
            {
                headerBuilder.Append('.');
                headerBuilder.Append(castActor?.Name ?? castScript?.GetType().Name ?? "<target name not found>");
            }

            if (!string.IsNullOrEmpty(call.MethodName))
            {
                headerBuilder.Append('.');
                headerBuilder.Append(call.MethodName);
            }

            group.Panel.HeaderText = headerBuilder.ToString();
        }

        objectPicker.CustomControl.ValueChanged += () => SetCallTarget(objectPicker);

        // Method picker button
        string buttonText = "<null>";

        if (!string.IsNullOrEmpty(call.MethodName))
            buttonText = call.MethodName;

        var methodPicker = propertyList.Button(buttonText);
        methodPicker.Button.Height = 18;
        methodPicker.Button.Margin = new(3, 0, 0, 0);
        methodPicker.Button.HorizontalAlignment = TextAlignment.Near;
        methodPicker.Button.ButtonClicked += CreateMethodSelectionMenu;

        // Parameter controls
        if (call.MethodInfo == null)
            return;

        // Parameter editors
        MemberInfo memberInfo = typeof(PersistentCall).GetMember("Parameters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0];
        ScriptMemberInfo scriptMember = new(memberInfo);
        GenericEditor.ItemInfo itemInfo = new(scriptMember);

        var editor = new PersistentParameterArrayEditor();

        var vc = itemInfo.GetValues(Values);
        var parameterPanel = group.VerticalPanel();
        var customEditor = parameterPanel.Object(vc, editor);
        
    }

    #region Editor-UI Setup

    /// <summary>Creates and show the context menu with buttons for the actor and attached scripts, which contain child menus of the available methods</summary>
    /// <param name="button">The button that was clicked. Should be a button for method selection. If not, dafuq is going on then.</param>
    private void CreateMethodSelectionMenu(Button button)
    {
        // Parent actor is used to figure out what scripts and methods are available in total, because the call target
        // might be a script, which doens't have access to the entire actors script hierarchy
        // NOTE: Yes, it does via Script.Actor. This works completly fine, but could be removed
        PersistentCall call = (PersistentCall)Values[0];

        if (call.Parent == null)
            return;

        ContextMenu contextMenu = new();
        for (int i = -1; call.Parent != null && i < call.Parent.Scripts.Length; i++)
        {
            Object target;
            FlaxEventContextChildMenu childMenu;
            bool isCallTarget = false;

            // FlaxEditor.Utilities.Utils.CreateSearchPopup(out TextBox searchBox, out FlaxEditor.GUI.Tree.Tree tree);

            if (i == -1)
            {
                target = call.Parent;
                childMenu = contextMenu.AddChildMenu(call.Parent.Name, isCallTarget);

                if (call.TargetObject == call.Parent)
                    isCallTarget = true;
            }
            else
            {
                target = call.Parent.Scripts[i];
                childMenu = contextMenu.AddChildMenu(call.Parent.Scripts[i].GetType().Name, isCallTarget);

                if (call.TargetObject == call.Parent.Scripts[i])
                    isCallTarget = true;
            }

            if (string.IsNullOrEmpty(call.MethodName))
                isCallTarget = false;

            if (isCallTarget)
            {
                childMenu.Icon = Editor.Instance.Icons.ArrowRight12;
                childMenu.ExtraAdjustmentAmount = -20;
                childMenu.IsActiveTarget = true;
            }


            SetMenuItems(childMenu.ContextMenu, target);

            if (i != call.Parent.Scripts.Length - 1)
                contextMenu.AddSeparator();
        }

        contextMenu.Show(button, button.PointFromScreen(Input.MouseScreenPosition));
    }

    /// <summary>Populates a <see cref="ContextMenu"/> with buttons of available methods of a target</summary>
    /// <param name="menu">The menu to modify</param>
    /// <param name="target">The target to get the method from</param>
    private void SetMenuItems(ContextMenu menu, Object target)
    {
        menu.DisposeAllItems();

        PersistentCall call = (PersistentCall)Values[0];
        Actor parentActor = ((PersistentCall)Values[0]).Parent;
        string callMethodName = ((PersistentCall)Values[0]).MethodName;

        BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Instance;
        MethodInfo[] methods = target.GetType().GetMethods(flags);

        for (int x = 0; x < methods.Length; x++)
        {
            // Can't invoke generic methods
            if (methods[x].IsGenericMethod)
                continue;

            // Creates the display name for a button, which shows the method name, the parameter signature and selection indicator
            StringBuilder methodNameBuilder = new(methods[x].Name);
            methodNameBuilder.Append('(');

            Type[] paraTypes = methods[x].GetParameterTypes();
            string shortKeys = null;
            bool sameOverloadMethod = false;

            for (int q = 0; q < paraTypes.Length; q++)
            {
                methodNameBuilder.Append(paraTypes[q]);

                if (q != paraTypes.Length - 1)
                    methodNameBuilder.Append(", ");

                if (call.Parameters != null && call.Parameters.Length == paraTypes.Length && call.Parameters[q].ParameterType == paraTypes[q])
                    sameOverloadMethod = true;
            }

            methodNameBuilder.Append(')');

            ContextMenuButton button = menu.AddButton(methodNameBuilder.ToString(), target, methods[x].Name, menu.ItemsContainer.ChildrenCount, paraTypes, SetCall);
            button.ShortKeys = shortKeys;

            // Selection indicator. Highlight existing selection.
            if (!string.IsNullOrEmpty(callMethodName) && callMethodName.Equals(methods[x].Name) && sameOverloadMethod)
            {
                button.Icon = Editor.Instance.Icons.ArrowRight12;
                button.ShortKeys = "(active)";
                (button as FlaxEventContextButton).IsActiveTarget = true;
            }
        }
    }

    /// <summary>Creates and show the right click context menu for a peristent call element</summary>
    /// <param name="dropPanel">The parent drop panel</param>
    /// <param name="location">The mouse location</param>
    private void RightClickContextMenu(DropPanel dropPanel, Float2 location)
    {
        ContextMenu menu = new();

        menu.AddButton("Copy", Copy);
        menu.AddButton("Duplicate", () => parentEditor.DuplicatePersistentCall(Index));
        menu.AddButton("Paste", () => parentEditor.PastePersistentCall(Index));

        menu.AddSeparator();

        menu.AddButton("Move up", () => parentEditor.MovePersistentCall(Index, Index - 1));
        menu.AddButton("Move down", () => parentEditor.MovePersistentCall(Index, Index + 1));
        menu.AddButton("Remove", () => parentEditor.RemovePersistentCall(Index));

        menu.Show(dropPanel, location);
    }

    #endregion

    #region Drag and Drop

    /// <summary>Readies the drag and drop funcionality</summary>
    private void StartAwaitingDrag()
    {
        Editor.Instance.EditorUpdate -= AwaitDrag;
        Editor.Instance.EditorUpdate += AwaitDrag;
    }

    /// <summary>Disengages the drag and drop funcionality</summary>
    private void StopAwaitingDrag()
    {
        Editor.Instance.EditorUpdate -= AwaitDrag;
    }

    /// <summary>Creates a drag operation, to move or shift this editors value to another index</summary>
    private void AwaitDrag()
    {
        if (!Input.GetMouseButtonDown(MouseButton.Left))
            return;

        dragOperation?.Dispose();
        dragOperation = new(parentEditor, Index);
    }

    /// <summary>Gets a value indicating if the mouse is over this editors group element</summary>
    /// <returns>true if mouse is in bounds</returns>
    internal bool IsMouseInBounds()
    {
        Float2 mousePos = group.ContainerControl.PointFromScreen(Input.MouseScreenPosition);
        return 0 <= mousePos.Y && mousePos.Y <= group.ContainerControl.Height;
    }

    /// <summary>Sets the editors elements color to their selection color</summary>
    /// <param name="selectionType">Type of the active selection</param>
    internal void SetDragSelectionColor(DragType selectionType)
    {
        group.ContainerControl.BackgroundColor = Color.Transparent;
        group.Panel.HeaderColor = FlaxEngine.GUI.Style.Current.BackgroundNormal;
        group.Panel.HeaderColorMouseOver = FlaxEngine.GUI.Style.Current.BackgroundHighlighted;
        dragShiftIndicatorTop.ContainerControl.BackgroundColor = Color.Transparent;

        if (selectionType == DragType.Shift)
            dragShiftIndicatorTop.ContainerControl.BackgroundColor = FlaxEngine.GUI.Style.Current.Selection;

        if (selectionType == DragType.Move)
        {
            group.ContainerControl.BackgroundColor = FlaxEngine.GUI.Style.Current.DragWindow;
            group.Panel.HeaderColor = FlaxEngine.GUI.Style.Current.DragWindow;
            group.Panel.HeaderColorMouseOver = FlaxEngine.GUI.Style.Current.DragWindow;
        }
    }
    #endregion

    #region Persistent Call Values Setter

    /// <summary>Sets the enabled state of the linked <see cref="PersistentCall"/></summary>
    /// <param name="box">The checkbox to use for the enabled/checked state. Checkbox gets passed via action delegate</param>
    private void SetCallEnabledState(CheckBox box)
    {
        PersistentCall oldCall = (PersistentCall)Values[0];
        PersistentCall call = oldCall.DeepClone();

        call.SetEnabled(box.Checked);

        SetValue(call);
        RebuildLayoutOnRefresh();
    }

    /// <summary>Sets the target of the linked <see cref="PersistentCall"/> to the <see cref="FlaxObjectRefPickerControl"/> value</summary>
    /// <param name="objectPicker">The object picker that is passed via action</param>
    private void SetCallTarget(CustomElement<FlaxObjectRefPickerControl> objectPicker)
    {
        PersistentCall oldCall = (PersistentCall)Values[0];
        PersistentCall call = oldCall.DeepClone();

        call.SetParent(objectPicker.CustomControl.Value as Actor ?? (objectPicker.CustomControl.Value as Script)?.Actor ?? null);

        SetValue(call);
        RebuildLayoutOnRefresh();
    }

    /// <summary>Sets the state for runtime parameter use of the linked <see cref="PersistentCall"/></summary>
    /// <param name="button">The Button that is passed via action</param>
    private void SetUseRuntimeParameter(Button button)
    {
        PersistentCall oldCall = (PersistentCall)Values[0];
        PersistentCall call = oldCall.DeepClone();

        call.SetUseRuntimeParameters(!call.TryUseRuntimeParameters);

        SetValue(call);
        RebuildLayoutOnRefresh();
    }

    /// <summary>Sets the call of this editor to a new call, with infos based in the button that was clicked</summary>
    /// <param name="button">The button that was clicked</param>
    private void SetCall(ContextMenuButton button)
    {
        var flaxEventButton = button as FlaxEventContextButton;

        PersistentCall oldCall = (PersistentCall)Values[0];
        PersistentCall call = oldCall.DeepClone();

        call.SetTarget(flaxEventButton.TargetObject);
        call.SetMethodName(flaxEventButton.MethodName);
        call.Parameters = [];

        if (flaxEventButton.ParameterTypes != null && 0 < flaxEventButton.ParameterTypes.Length)
        {
            PersistentParameter[] newParameters = new PersistentParameter[flaxEventButton.ParameterTypes.Length];

            for (int i = 0; i < newParameters.Length; i++)
            {
                PersistentParameter newParameter = new()
                {
                    ParameterValue = flaxEventButton.ParameterTypes[i].GetDefault(),
                    ParameterType = flaxEventButton.ParameterTypes[i]
                };

                newParameters[i] = newParameter;
            }

            call.Parameters = newParameters;
        }

        SetValue(call);
        RebuildLayoutOnRefresh();
    }

    #endregion
}

#endif