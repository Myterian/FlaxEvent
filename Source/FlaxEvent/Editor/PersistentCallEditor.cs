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

namespace FlaxEvents;

/// <summary>PersistentParameterEditor class.</summary>
public class PersistentCallEditor : CustomEditor
{
    private PersistentCallListEditor parentEditor;
    private GroupElement group;
    public int Index { get; private set; } = -1;
    private bool isDragging = false;

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
        // var dragIndicator = layout.VerticalPanel();
        // dragIndicator.Control.BackgroundColor = FlaxEngine.GUI.Style.Current.Selection;
        // dragIndicator.ContainerControl.Offsets = new(0);

        PersistentCall call = (PersistentCall)Values[0];

        Actor castActor = null;
        Script castScript = null;
        string headerText = "<null>";

        // PropertyNameLabel x = new PropertyNameLabel("Some label");
        // PropertiesListElement y = layout.AddPropertyItem(x);
        // var group = (LayoutElementsContainer)y;
        // var basePanel = layout.VerticalPanel();

        group = layout.Group(headerText);
        group.Panel.MouseButtonRightClicked += RightClickContextMenu;
        bool isCallEnabled = call.IsEnabled;

        group.Panel.HeaderTextMargin = new(44, 0, 0, 0);
        group.Panel.HeaderTextColor = isCallEnabled ? FlaxEngine.GUI.Style.Current.Foreground : FlaxEngine.GUI.Style.Current.ForegroundDisabled;
        group.Panel.EnableContainmentLines = false;



        if (isDragging)
        {
            // group.Panel.HeaderColor = FlaxEngine.GUI.Style.Current.Selection;
            // group.Panel.BackgroundColor = FlaxEngine.GUI.Style.Current.Selection;

        }

        float headerHeight = group.Panel.HeaderHeight;

        // Checkbox with enable/disable logic
        var toggle = new CheckBox
        {
            TooltipText = "If checked, the target will be invoked",
            IsScrollable = false,
            Checked = isCallEnabled,
            Parent = group.Panel,
            Size = new(headerHeight),
            Bounds = new(headerHeight, 0, headerHeight, headerHeight),
            BoxSize = headerHeight - 4
        };

        toggle.StateChanged += SetCallEnabledState;

        // Drag and Drop button. TODO: Drag Reorder
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

        
        // dragButton.ButtonClicked += (Button buttno) => parentEditor.StartDrag(Index);
        // RootControl.GameRoot.StartMouseCapture()

        // Object picker
        var propertyList = group.AddPropertyItem("Target", "The target of this event call");
        var objectPicker = propertyList.Custom<FlaxObjectRefPickerControl>();

        // var runtimeParameterCheckbox = layout.Checkbox("Use Event Inputs",
        //                 "If checked, the event will try to pass the runtime parameters instead of the editor-configured parameters to the target method. Will only work, if the event signature and method signature match.");
        // runtimeParameterCheckbox.CheckBox.Checked = call.UseRuntimeParameters;
        // runtimeParameterCheckbox.CheckBox.StateChanged += (CheckBox box) =>
        // {
        //     PersistentCall call = (PersistentCall)Values[0];
        //     call.UseRuntimeParameters = box.Checked;

        //     SetValue(call);
        //     RebuildLayoutOnRefresh();
        // };


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

        objectPicker.CustomControl.ValueChanged += () =>
        {
            PersistentCall call = (PersistentCall)Values[0];
            call.SetParent(objectPicker.CustomControl.Value as Actor ?? (objectPicker.CustomControl.Value as Script)?.Actor ?? null);

            SetValue(call);
            RebuildLayoutOnRefresh();
        };

        // Method picker button
        string buttonText = "<null>";

        if (!string.IsNullOrEmpty(call.MethodName))
            buttonText = call.MethodName;

        var methodPicker = propertyList.Button(buttonText);
        methodPicker.Button.Height = 18;
        methodPicker.Button.Margin = new(2, 0, 0, 0);
        methodPicker.Button.HorizontalAlignment = TextAlignment.Near;
        methodPicker.Button.ButtonClicked += CreateMethodSelectionMenu;

        // Parameter controls
        MethodInfo methodInfo = call.MethodInfo;

        if (methodInfo == null)
            return;

        MemberInfo memberInfo = typeof(PersistentCall).GetMember("Parameters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0];
        ScriptMemberInfo scriptMember = new(memberInfo);
        GenericEditor.ItemInfo itemInfo = new(scriptMember);

        var vc = itemInfo.GetValues(Values);

        for (int i = 0; i < call.Parameters.Length; i++)
        {
            var editor = new PersistentParameterArrayEditor();
            editor.SetIndex(i);

            layout.Object(vc, editor);
        }
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
            // Creates the display name for a button, which shows the method name, the parameter signature and selection indicator
            StringBuilder methodNameBuilder = new(methods[x].Name);
            methodNameBuilder.Append('(');

            Type[] paraTypes = methods[x].GetParameterTypes();
            string shortKeys = null;
            bool sameOverloadMethod = true;

            for (int q = 0; q < paraTypes.Length; q++)
            {
                methodNameBuilder.Append(paraTypes[q]);

                if (q != paraTypes.Length - 1)
                    methodNameBuilder.Append(", ");

                if (call.Parameters == null || call.Parameters.Length != paraTypes.Length || call.Parameters[q].ParameterType != paraTypes[q])
                    sameOverloadMethod = false;
            }

            methodNameBuilder.Append(')');
            
            ContextMenuButton button = menu.AddButton(methodNameBuilder.ToString(), target, methods[x].Name, paraTypes, SetCall);
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


    private void StartAwaitingDrag()
    {
        Editor.Instance.EditorUpdate -= AwaitDrag;
        Editor.Instance.EditorUpdate += AwaitDrag;
    }

    private void StopAwaitingDrag()
    {
        Editor.Instance.EditorUpdate -= AwaitDrag;
    }

    private void AwaitDrag()
    {
        if (!Input.GetMouseButtonDown(MouseButton.Left))
            return;

        group.ContainerControl.Enabled = false;
        parentEditor.StartDrag(Index);
    }

    public bool IsMouseInBounds(Float2 mousePosition)
    {
        Float2 mouse = group.ContainerControl.PointFromScreen(mousePosition);
        Float2 offeset = group.ContainerControl.Bounds.Size - mouse;

        bool isMouseOver = 0 <= offeset.X && offeset.X <= group.ContainerControl.Width && 0 <= offeset.Y && offeset.Y <= group.ContainerControl.Height;

        if (isMouseOver == true)
        {
            group.ContainerControl.BackgroundColor = FlaxEngine.GUI.Style.Current.Selection;
            group.Panel.HeaderColor = FlaxEngine.GUI.Style.Current.Selection;
        }
        else
        {
            group.ContainerControl.BackgroundColor = Color.Transparent;
            group.Panel.HeaderColor = FlaxEngine.GUI.Style.Current.BackgroundNormal;
        }

        return isMouseOver;
    }
    #endregion

    #region Peristent Call Values Setter

    /// <summary>Sets the enabled state of the linked <see cref="PersistentCall"/></summary>
    /// <param name="box">The checkbox to use for the enabled/checked state. Checkbox gets passed via action delegate</param>
    private void SetCallEnabledState(CheckBox box)
    {
        PersistentCall call = (PersistentCall)Values[0];
        call.IsEnabled = box.Checked;

        SetValue(call);
        RebuildLayoutOnRefresh();
    }

    /// <summary>Sets the call of this editor to a new call, with infos based in the button that was clicked</summary>
    /// <param name="button">The button that was clicked</param>
    private void SetCall(ContextMenuButton button)
    {
        var flaxEventButton = button as FlaxEventContextButton;

        PersistentCall call = (PersistentCall)Values[0];

        call.SetTarget(flaxEventButton.TargetObject);
        call.SetMethodName(flaxEventButton.MethodName);
        call.Parameters = [];

        if (flaxEventButton.ParameterTypes != null && 0 < flaxEventButton.ParameterTypes.Length)
        {
            PersistentParameter[] newParameters = new PersistentParameter[flaxEventButton.ParameterTypes.Length];

            for (int i = 0; i < newParameters.Length; i++)
            {
                newParameters[i].ParameterValue = flaxEventButton.ParameterTypes[i].GetDefault();
                newParameters[i].ParameterType = flaxEventButton.ParameterTypes[i];
            }

            call.Parameters = newParameters;
        }

        SetValue(call);
        RebuildLayoutOnRefresh();
    }

    #endregion
}

#endif