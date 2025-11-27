// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

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

namespace FlaxEvent;

/// <summary>PersistentParameterEditor class.</summary>
public class PersistentCallEditor : CustomEditor
{
    public override void Initialize(LayoutElementsContainer layout)
    {
        // layout.Label("PersistentParameter");
        if (Values[0] == null || Values.Count == 0)
            return;

        PersistentCall call = (PersistentCall)Values[0];

        Actor castActor = null;
        Script castScript = null;
        string headerText = "<null>";

        var group = layout.Group(headerText);
        // group.Panel.MouseButtonRightClicked += // TODO: Right-Click context menu
        bool isCallEnabled = call.IsEnabled;


        group.Panel.HeaderTextMargin = new(44, 0, 0, 0);
        group.Panel.HeaderTextColor = isCallEnabled ? FlaxEngine.GUI.Style.Current.Foreground : FlaxEngine.GUI.Style.Current.ForegroundDisabled;
        group.Panel.EnableContainmentLines = false;

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

        // Drag button. TODO: Drag Reorder
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

        // Object picker
        var propertyList = group.AddPropertyItem("Target", "The target of this event call");
        var objectPicker = propertyList.Custom<FlaxObjectRefPickerControl>();

        if (call.TargetObject != null)
        {
            objectPicker.CustomControl.Value = call.TargetObject;

            Guid guid = call.TargetObject.ID;
            Object uncastObject = Object.Find<Object>(ref guid);

            castActor = uncastObject as Actor;
            castScript = uncastObject as Script;

            group.Panel.HeaderText = castActor?.Name ?? castScript.GetType().Name ?? "<target not found>";
        }

        objectPicker.CustomControl.ValueChanged += () =>
        {
            PersistentCall call = (PersistentCall)Values[0];
            call.SetParent(objectPicker.CustomControl.Value as Actor ?? (objectPicker.CustomControl.Value as Script)?.Actor ?? null);

            SetValue(call);
            RebuildLayoutOnRefresh();
        };

        // Method picker button & context menu
        string buttonText = "<null>";

        if (!string.IsNullOrEmpty(call.MethodName))
        {
            group.Panel.HeaderText += "." + call.MethodName;
            buttonText = call.MethodName;
        }

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

            group.Object(vc, editor);
        }
    }

    
    /// <summary>Creates and show the context menu with buttons for the actor and attached scripts, which contain child menus of the available methods</summary>
    /// <param name="button">The button that was clicked. Should be a button for method selection. If not, dafuq is going on then.</param>
    private void CreateMethodSelectionMenu(Button button)
    {
        // Parent actor is used to figure out what scripts and methods are available in total, because the call target
        // might be a script, which doens't have access to the entire actors script hierarchy
        // NOTE: Yes, it does via Script.Actor. This works completly fine, but could be removed
        Actor parentActor = ((PersistentCall)Values[0]).Parent;

        if (parentActor == null)
            return;

        ContextMenu contextMenu = new();
        for (int i = -1; parentActor != null && i < parentActor.Scripts.Length; i++)
        {
            Object target;
            ContextMenuChildMenu childMenu;

            if (i == -1)
            {
                target = parentActor;
                childMenu = contextMenu.AddChildMenu(parentActor.Name);
            }
            else
            {
                target = parentActor.Scripts[i];
                childMenu = contextMenu.AddChildMenu(parentActor.Scripts[i].GetType().Name);
            }

            SetMenuItems(childMenu.ContextMenu, target);

            if (i != parentActor.Scripts.Length - 1)
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

        Actor parentActor = ((PersistentCall)Values[0]).Parent;
        BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Instance;

        MethodInfo[] methods = target.GetType().GetMethods(flags);

        for (int x = 0; x < methods.Length; x++)
        {
            // Creates the display name for a button, which shows the method name and the parameter signature
            StringBuilder methodNameBuilder = new(methods[x].Name);
            methodNameBuilder.Append('(');

            Type[] paraTypes = methods[x].GetParameterTypes();

            for (int q = 0; q < paraTypes.Length; q++)
                methodNameBuilder.Append(paraTypes[q]);

            methodNameBuilder.Append(')');
            var button = menu.AddButton(methodNameBuilder.ToString(), methods[x].Name, paraTypes, SetCall);
        }
    }

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

        call.SetMethodName(flaxEventButton.MethodName);
        call.Parameters = [];

        if (flaxEventButton.ParameterTypes != null && 0 < flaxEventButton.ParameterTypes.Length)
        {
            // Type[] methodParameterTypes = call.MethodInfo.GetParameterTypes();
            PersistentParameter[] newParameters = new PersistentParameter[flaxEventButton.ParameterTypes.Length];

            for (int i = 0; i < newParameters.Length; i++)
            {
                newParameters[i].ParameterValue = flaxEventButton.ParameterTypes[i].IsValueType ? Activator.CreateInstance(flaxEventButton.ParameterTypes[i]) : null;
                newParameters[i].ParameterType = flaxEventButton.ParameterTypes[i];
            }

            call.Parameters = newParameters;
        }

        SetValue(call);
        RebuildLayoutOnRefresh();
    }
}
