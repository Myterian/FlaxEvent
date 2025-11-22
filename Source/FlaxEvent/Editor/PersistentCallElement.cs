// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEditor.CustomEditors.GUI;
using FlaxEditor.GUI;
using FlaxEditor.GUI.ContextMenu;
using FlaxEditor.Scripting;
using FlaxEngine;
using FlaxEngine.GUI;
using FlaxEngine.Utilities;
using Object = FlaxEngine.Object;

namespace FlaxEvent;

/// <summary>Custom editor for <see cref="PersistentCall"/> elements, that appear in the <see cref="FlaxEventEditor"/></summary>
public class PersistentCallElement : GroupElement
{
    /// <summary>The parent editor that is currently in use. The link-back is needed to set values via <see cref="CustomEditor.SetValue"/>, the cleanest way to modify editor values.</summary>
    private FlaxEventEditor LinkedEditor;

    // private List<Per>

    /// <summary> 
    /// <see cref="FlaxObjectRefPickerControl"/> for this <see cref="PersistentCallElement"/>. 
    /// The control is not passed in the <see cref="FlaxObjectRefPickerControl.ValueChanged"/> delegate,
    /// so it has to be made available class wide.
    /// </summary>
    private CustomElement<FlaxObjectRefPickerControl> objectPicker;

    /// <summary>The index of the <see cref="PersistentCall"/> element in the <see cref="FlaxEventBase.PersistentCallList"/>, which this element refers to</summary>
    private int callIndex = -1;


    /// <summary>Initializes this <see cref="PersistentCallElement"/>. Is needed to set up values and ui elements for display in the inspector.</summary>
    /// <param name="editor">The editor we're currently working in</param>
    /// <param name="index">The index of the <see cref="PersistentCall"/> element in a <see cref="FlaxEventBase.PersistentCallList"/>, that is being displayed</param>
    public void Init(FlaxEventEditor editor, int index)
    {
        LinkedEditor = editor;
        callIndex = index;

        bool isCallEnabled = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList[callIndex].IsEnabled;

        Panel.HeaderText = "<null>";
        Panel.HeaderTextMargin = new(44, 0, 0, 0);
        Panel.BackgroundColor = Style.Current.CollectionBackgroundColor;
        Panel.HeaderTextColor = isCallEnabled ? Style.Current.Foreground : Style.Current.ForegroundDisabled;
        Panel.EnableContainmentLines = false;

        float headerHeight = Panel.HeaderHeight;

        // Checkbox with enable/disable logic
        var toggle = new CheckBox
        {
            TooltipText = "If checked, the target will be invoked",
            IsScrollable = false,
            Checked = isCallEnabled,
            Parent = Panel,
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
            BackgroundColor = Style.Current.ForegroundGrey,
            BackgroundColorHighlighted = Style.Current.ForegroundGrey.RGBMultiplied(1.5f),
            HasBorder = false,
            Parent = Panel,
            Bounds = new(toggle.Right, 1, headerHeight, headerHeight),
            Scale = new(0.9f)
        };

        // Property list with object picker and methodpicker and logic
        var propertyList = AddPropertyItem("Target", "The target of this event");
        // var parameterItemList = AddPropertyItem("Parameters", "The invokation parameters, when this method get called");
        // propertyList.Control.Parent = Panel;
        objectPicker = propertyList.Custom<FlaxObjectRefPickerControl>();

        // Method picker button
        var methodPicker = propertyList.Button("<null>");
        methodPicker.Button.Height = 18;
        methodPicker.Button.Margin = new(2, 0, 0, 0);
        methodPicker.Button.HorizontalAlignment = TextAlignment.Near;
        methodPicker.Button.ButtonClicked += CreateAndShowContextMenu;


        // Target Object Picker Logic
        var targetObject = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList[callIndex].TargetObject;

        if (targetObject != null)
        {
            objectPicker.CustomControl.Value = targetObject;

            Guid guid = targetObject.ID;
            Object uncastObject = FlaxEngine.Object.Find<Object>(ref guid);

            Actor castActor = uncastObject as Actor;
            Script castScript = uncastObject as Script;

            Panel.HeaderText = castActor?.Name ?? castScript.GetType().Name ?? "<type not found>";
        }

        objectPicker.CustomControl.ValueChanged += SetCallParentObject;

        string savedMethodName = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList[callIndex].MethodName;

        if (!string.IsNullOrEmpty(savedMethodName))
        {
            methodPicker.Button.Text = savedMethodName;
            Panel.HeaderText += "." + savedMethodName;
        }

        // Drag state changed

        if ((LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList[callIndex].MethodInfo == null)
            return;

        // var parameterItemList = AddPropertyItem("Parameters");
        // parameterItemList.Control.Parent = Panel;
        // Panel.Height = 100;
        // var parameterItemList = AddPropertyItem("Parameters", "The invokation parameters, when this method get called");
        // var parameterOverrideCheckbox = new CheckBoxElement();
        // parameterOverrideCheckbox.CheckBox.
        // AddElement(parameterOverrideCheckbox);

        // Checkbox();
        // var parameterItemList = AddPropertyItem("Parameters", "The invokation parameters, when this method get called");
        PropertiesListElement propertiesListElement = new();
        // propertiesListElement.OnAddProperty("Parameters", "The invokation parameters, when this method get called");
        var nameLabel = new PropertyNameLabel("Parameters");
        nameLabel.TooltipText = "The invokation parameters, when this method get called";
        nameLabel.Parent = propertiesListElement.Properties;
        // nameLabel.first
        // propertiesListElement.la
        // propertiesListElement.ContainerControl.
        // propertiesListElement.ContainerControl.TooltipText = "The invokation parameters, when this method get called";
        AddElement(propertiesListElement);

        // propertyList.Space(10);
        // propertyList.Label("Parameters", TextAlignment.Near);

        // propertyList.Properties.
        // OnAddElement(propertiesListElement);

        var parameterInfos = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList[callIndex].MethodInfo.GetParameters();
        var parameterTypes = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList[callIndex].MethodInfo.GetParameterTypes();

        for (int i = 0; i < parameterTypes.Length; i++)
        {
            propertiesListElement.AddTypeElement(parameterTypes[i]);
            /*
        var value = Values[0]; // Temporary fix to have values. Remove for proper impl
        List<ItemInfo> itemInfos = GetItemsForType(TypeUtils.GetObjectType(value)); // This needs to be the types in a method signature, instead of using value
        SpawnProperty(layout, itemInfos[0].GetValues(Values), itemInfos[0]); // This needs to be done for every PersistentCall Element on every element of itemInfos
        */
            // ScriptType scriptType = new ScriptType(parameterTypes[i]);
            // scriptType.
            // // var itemInfos = GenericEditor.GetItemsForType(scriptType, scriptType.IsClass, true);
            // MemberInfo member = new( typeof(List<object>));
            // // Debug.Log
            // ScriptMemberInfo memberInfo = new((LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList[callIndex].MethodInfo);
            // memberInfo.vla

            // // string tip = Editor.Instance.CodeDocs.GetTooltip(memberInfo);
            // // Debug.Log("Found ToolTip:" +!string.IsNullOrEmpty(tip));
            // // memberInfo.get
            // List<object> defaults = new();
            // ValueContainer fakeValues = new(memberInfo);

            // for (int x = 0; x < parameterTypes.Length; x++)
            // {
            //     defaults.Add(Activator.CreateInstance(parameterTypes[x]));
            //     // fakeValues.Add(Activator.CreateInstance(parameterTypes[x]));
            // }

            // ValueContainer fakeValues = new(memberInfo) { defaults };

            // // CustomValueContainer fakeValues = new(scriptType, (instance, index) => (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList[callIndex].Parameters[i].GetValue());
            // // LinkedEditor.ValuesTypes
            // // fakeValues.SetDefaultValue()
            // // propertiesListElement.Property(parameterTypes[i].Name, fakeValues);
            // // SpawnProperty
            // // Debug.Log(itemInfos.Count);
            // // parameterItemList.Control
            // // parameterTypes[i].getp
            // var itemInfos = GenericEditor.GetItemsForType(scriptType, scriptType.IsClass, true);
            // GenericEditor.ItemInfo itemInfo = new(memberInfo, []);
            // var y = itemInfo.GetValues(fakeValues);
            // // ValueContainer itemValues = new()

            // Property(parameterTypes[i].Name, y, itemInfo.OverrideEditor);
            // CustomEditor = (CustomEditorAttribute)attributes.FirstOrDefault((object x) => x is CustomEditorAttribute);
            // SpawnP
            // CustomEditorUtils

            // GenericEditor genericEditor = new();
            // Add
        }



    }

    #region PersistentCall values

    /// <summary>Verifies that this <see cref="PersistentCallElement"/> has been set up with valid values to access a <see cref="FlaxEventBase.PersistentCallList"/></summary>
    /// <returns>true if valid, false if not</returns>
    private bool IsSetupValid()
    {
        if (callIndex < 0 || LinkedEditor == null)
            return false;

        List<PersistentCall> persistentCalls = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList;

        if (Mathf.IsNotInRange(callIndex, 0, persistentCalls.Count - 1))
            return false;

        return true;
    }

    /// <summary>Sets the target object of a <see cref="PersistentCall"/></summary>
    private void SetCallParentObject()
    {
        if (!IsSetupValid())
            return;

        PersistentCall oldCall = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList[callIndex];

        // NOTE: Clears the call when the object picker value changes via inspector.
        // Q: Will this cause trouble?
        PersistentCall call = new();

        if (objectPicker.CustomControl.Value == null)
            call.Parent = null;
        else
            call.Parent = objectPicker.CustomControl.Value as Actor ?? (objectPicker.CustomControl.Value as Script).Actor ?? null;

        call.TargetObject = call.Parent;
        call.IsEnabled = oldCall.IsEnabled;


        List<PersistentCall> newPersistentCalls = [.. (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList];
        newPersistentCalls[callIndex] = call;

        LinkedEditor.SetValues(newPersistentCalls);
    }

    /// <summary>Sets the target object and method in a <see cref="PersistentCall"/></summary>
    /// <param name="target">The target object</param>
    /// <param name="methodName">The target method name</param>
    private void SetCallTarget(Object target, string methodName)
    {
        if (!IsSetupValid())
            return;

        List<PersistentCall> persistentCalls = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList;

        PersistentCall call = persistentCalls[callIndex];
        call.TargetObject = target;
        call.MethodName = methodName;

        List<PersistentCall> newPersistentCalls = [.. (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList];
        newPersistentCalls[callIndex] = call;

        LinkedEditor.SetValues(newPersistentCalls);
    }

    /// <summary>Sets the enabled state of the linked <see cref="PersistentCall"/></summary>
    /// <param name="box">The checkbox to use for the enabled/checked state. Checkbox gets passed via action delegate</param>
    private void SetCallEnabledState(CheckBox box)
    {
        if (!IsSetupValid())
            return;

        List<PersistentCall> persistentCalls = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList;

        PersistentCall call = persistentCalls[callIndex];
        call.IsEnabled = box.Checked;
        Panel.HeaderTextColor = box.Checked ? Style.Current.Foreground : Style.Current.ForegroundDisabled;

        List<PersistentCall> newPersistentCalls = [.. (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList];
        newPersistentCalls[callIndex] = call;

        LinkedEditor.SetValues(newPersistentCalls);
    }

    // private void ClearCall()
    // {
    //     if (callIndex < 0 || LinkedEditor == null)
    //         return;

    //     List<PersistentCall> persistentCalls = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList;

    //     if (Mathf.IsNotInRange(callIndex, 0, persistentCalls.Count - 1))
    //         return;

    //     PersistentCall call = new();

    //     List<PersistentCall> newPersistentCalls = [.. (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList];
    //     newPersistentCalls[callIndex] = call;

    //     LinkedEditor.SetValues(newPersistentCalls);
    // }
    #endregion

    #region Inspector/UI Behaviour

    private void CreateAndShowContextMenu(Button button)
    {
        if (!IsSetupValid())
            return;

        List<PersistentCall> persistentCalls = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList;
        Actor parentActor = persistentCalls[callIndex].Parent;

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

        if (!IsSetupValid())
            return;

        List<PersistentCall> persistentCalls = (LinkedEditor.Values[0] as FlaxEventBase).PersistentCallList;
        Actor parentActor = persistentCalls[callIndex].Parent;
        BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Instance;

        MethodInfo[] methods = target.GetType().GetMethods(flags);

        for (int x = 0; x < methods.Length; x++)
        {
            Action<ContextMenuButton> action = (button) => SetCallTarget(target, button.Text);
            menu.AddButton(methods[x].Name, action);
        }
    }
    
    #endregion
}