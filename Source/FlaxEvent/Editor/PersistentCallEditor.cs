// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
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
using System.Linq;

namespace FlaxEvent;

/// <summary>
/// PersistentParameterEditor class.
/// </summary>
public class PersistentCallEditor : CustomEditor
{
    public override void Initialize(LayoutElementsContainer layout)
    {
        // layout.Label("PersistentParameter");
        // if (Values[0] == null )
        // return;

        PersistentCall call = (PersistentCall)Values[0];

        // string groupTitle = call.TargetObject == null ? "<No Target>" : call.TargetObject.

        Actor castActor = null;
        Script castScript = null;
        string headerText = "<null>";

        // if (call.TargetObject != null)
        // {
        //     Guid guid = call.TargetObject.ID;
        //     Object uncastObject = Object.Find<Object>(ref guid);

        //     castActor = uncastObject as Actor;
        //     castScript = uncastObject as Script;

        //     headerText = castActor?.Name ?? castScript.GetType().Name ?? "<target not found>";
        // }




        var group = layout.Group(headerText);
        // group.Panel.MouseButtonRightClicked += // TODO: Right-Click context menu

        bool isCallEnabled = call.IsEnabled;

        // Panel.HeaderText = "<null>";
        group.Panel.HeaderTextMargin = new(44, 0, 0, 0);
        // group.Panel.BackgroundColor = FlaxEngine.GUI.Style.Current.CollectionBackgroundColor;
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
        // Drag button with
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



        var propertyList = group.AddPropertyItem("Target", "The target of this event call");

        // var targetObject = ((PersistentCall)Values[0]).TargetObject;

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

        // objectPicker.CustomControl.ValueChanged += SetCallParentObject;

        objectPicker.CustomControl.ValueChanged += () =>
        {
            PersistentCall call = (PersistentCall)Values[0];
            call.SetParent(objectPicker.CustomControl.Value as Actor ?? (objectPicker.CustomControl.Value as Script)?.Actor ?? null);
            // call.TargetObject = objectPicker.CustomControl.Value;

            SetValue(call);
            RebuildLayoutOnRefresh();
        };

        // if (!string.IsNullOrEmpty(((PersistentCall)Values[0]).MethodName))
        // {
        //     methodPicker.Button.Text = savedMethodName;
        //     Panel.HeaderText += "." + savedMethodName;
        // }

        string buttonText = "<null>";

        if (!string.IsNullOrEmpty(call.MethodName))
        {
            group.Panel.HeaderText += "." + call.MethodName;
            buttonText = call.MethodName;
        }

        // buttonText ??= "<null>";

        var methodPicker = propertyList.Button(buttonText);
        methodPicker.Button.Height = 18;
        methodPicker.Button.Margin = new(2, 0, 0, 0);
        methodPicker.Button.HorizontalAlignment = TextAlignment.Near;
        methodPicker.Button.ButtonClicked += CreateMethodSelectionMenu;

        MethodInfo methodInfo = call.MethodInfo;

        if (methodInfo == null)
            return;

        // var parameterList = group.AddPropertyItem("Parameters", "The invokation parameters that are being used");
        // parameterList.ContainerControl.ClipChildren = false;
        // parameterList.ContainerControl.CullChildren = false;
        // group.prop

        // if(3 <= parameterList.Children.Count)
        //     Debug.Log(parameterList.Children[2].GetType());
        // var spacer = group.Space(20f);

        // var brush = new SolidColorBrush
        // {
        //     Color = FlaxEngine.GUI.Style.Current.BorderNormal,

        // };
        // var image = spacer.Image(brush);
        // image.Image.KeepAspectRatio = false;
        // image.Image.AnchorPreset = AnchorPresets.HorizontalStretchMiddle;
        // image.Image.Width = 30;
        // image.Image.LocalLocation = new(50, image.Image.LocalLocation.Y);
        // image.Image.Margin = new(0, 0, 9f, 9f);
        // image.Image.Bounds = new Rectangle(image.Image.Bounds.Location, image.Image.Bounds.X, image.Image.Bounds.Y - 1);
        // image.Image.Scale = new(1, 0.05f);
        // image.Image.DrawSelf();
        // image.Control.Width = 20;
        // Debug.Log(parameterList.ContainerControl.Size);
        // var parameterList = layout.VerticalPanel();
        // var label = layout.Label("Parameter(s)");
        // label.Label.Offsets = new(7, 0, 0, 0);

        MemberInfo memberInfo1 = typeof(PersistentCall).GetMember("Parameters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0];
        ScriptMemberInfo scriptMember1 = new(memberInfo1);
        GenericEditor.ItemInfo itemInfo1 = new(scriptMember1);

        var vc = itemInfo1.GetValues(Values);

        // var editor = new PersistentParameterArrayEditor();
        // editor

        // parameterList.Object()

        // Type[] methodParameterTypes = call.MethodInfo.GetParameterTypes();
        for (int i = 0; i < call.Parameters.Length; i++)
        {
            //     // var lvc = new ListValueContainer(new(memberInfo1.GetType()), i, Values);
            //     // CustomEditor editor = call.Parameters[i].ParameterType.FindEditorFromType();
            var editor = new PersistentParameterArrayEditor();
            editor.SetIndex(i);

            group.Object(vc, editor);
        }
    }

    private void CreateMethodSelectionMenu(Button button)
    {
        // if (!IsSetupValid())
        //     return;

        // List<PersistentCall> persistentCalls = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList;
        Actor parentActor = ((PersistentCall)Values[0]).Parent;

        if (parentActor == null)
            return;

        ContextMenu contextMenu = new();
        // FlaxEditor.Utilities.Utils. ;


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
            // Action<ContextMenuButton> action = (button) =>
            // {
            //     PersistentCall call = (PersistentCall)Values[0];

            //     call.MethodName = button.ShortKeys;
            //     call.Parameters = [];

            //     // if (call.MethodInfo != null)
            //     // {
            //     //     Type[] methodParameterTypes = call.MethodInfo.GetParameterTypes();
            //     //     PersistentParameter[] newParameters = new PersistentParameter[methodParameterTypes.Length];

            //     //     for (int i = 0; i < newParameters.Length; i++)
            //     //     {
            //     //         newParameters[i].ParameterValue = methodParameterTypes[i].IsValueType ? Activator.CreateInstance(methodParameterTypes[i]) : null;
            //     //         newParameters[i].ParameterType = methodParameterTypes[i];
            //     //     }

            //     //     call.Parameters = newParameters;
            //     // }

            //     SetValue(call);

            //     // ClearDefaultValueAll();
            //     // ChildrenEditors.Clear();
            //     // Presenter.BuildLayout();
            //     // RebuildLayout();

            //     RebuildLayoutOnRefresh();
            //     // Refresh();
            // };

            // Creates the display name for a button, which shows the method name and the parameter signature
            StringBuilder methodNameBuilder = new(methods[x].Name);
            methodNameBuilder.Append('(');

            Type[] paraTypes = methods[x].GetParameterTypes();

            for (int q = 0; q < paraTypes.Length; q++)
                methodNameBuilder.Append(paraTypes[q]);

            methodNameBuilder.Append(')');

            // Create the button. The 'raw' method name, that is used to set the value for the persistent call
            // is stored in ShortKeys. A bit hacky, but I can't be bothered right now. This entire thing is alredy
            // pretty hacky
            // var button = menu.AddButton(methodNameBuilder.ToString(), SetCall);
            var button = menu.AddButton(methodNameBuilder.ToString(), methods[x].Name, paraTypes, SetCall);
            // button.ShortKeys = methods[x].Name;

            // menu.add
            // menu.SortButtons();
            // ContextMenuButton b = new(menu, "menu item", ""){ Parent = menu.ItemsContainer };
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
