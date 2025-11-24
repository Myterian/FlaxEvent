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

namespace FlaxEvent;

/// <summary>
/// PersistentParameterEditor class.
/// </summary>
public class PersistentCallEditor : CustomEditor
{
    public override void Initialize(LayoutElementsContainer layout)
    {
        // layout.Label("PersistentParameter");
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

        bool isCallEnabled = call.IsEnabled;

        // Panel.HeaderText = "<null>";
        group.Panel.HeaderTextMargin = new(44, 0, 0, 0);
        group.Panel.BackgroundColor = FlaxEngine.GUI.Style.Current.CollectionBackgroundColor;
        group.Panel.HeaderTextColor = isCallEnabled ? FlaxEngine.GUI.Style.Current.Foreground : FlaxEngine.GUI.Style.Current.ForegroundDisabled;
        group.Panel.EnableContainmentLines = false;



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
            call.Parent = objectPicker.CustomControl.Value as Actor ?? (objectPicker.CustomControl.Value as Script).Actor ?? null;
            call.TargetObject = objectPicker.CustomControl.Value;

            SetValue(call);
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
        methodPicker.Button.ButtonClicked += CreateAndShowContextMenu;


        if (call.MethodInfo == null)
            return;

        var parameterList = group.AddPropertyItem("Parameters", "The invokation parameters that are being used");
        parameterList.Space(20f);
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

    private void CreateAndShowContextMenu(Button button)
    {
        // if (!IsSetupValid())
        //     return;

        // List<PersistentCall> persistentCalls = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList;
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

    /// <summary>Populates a <see cref="ContextMenu"/> with available methods of a target</summary>
    /// <param name="menu">The menu to modify</param>
    /// <param name="target">The target to get the method from</param>
    private void SetMenuItems(ContextMenu menu, Object target)
    {
        menu.DisposeAllItems();

        // if (!IsSetupValid())
        //     return;

        // List<PersistentCall> persistentCalls = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList;
        Actor parentActor = ((PersistentCall)Values[0]).Parent;
        BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Instance;

        MethodInfo[] methods = target.GetType().GetMethods(flags);

        for (int x = 0; x < methods.Length; x++)
        {
            // Action<ContextMenuButton> action = (button) => SetCallTarget(target, button.Text);
            Action<ContextMenuButton> action = (button) =>
            { 
                PersistentCall call = (PersistentCall)Values[0];
            
                call.MethodName = button.Text;

                if (call.MethodInfo != null)
                {
                    Type[] methodParameterTypes = call.MethodInfo.GetParameterTypes();
                    PersistentParameter[] newParameters = new PersistentParameter[methodParameterTypes.Length];

                    for (int i = 0; i < newParameters.Length; i++)
                    {
                        newParameters[i].ParameterValue = methodParameterTypes[i].IsValueType ? Activator.CreateInstance(methodParameterTypes[i]) : null;
                        newParameters[i].ParameterType = methodParameterTypes[i];
                    }

                    call.Parameters = newParameters;
                }

                SetValue(call);
            };
            menu.AddButton(methods[x].Name, action);
        }
    }
}
