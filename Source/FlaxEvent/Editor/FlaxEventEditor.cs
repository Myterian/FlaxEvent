// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEvent;

/// <summary>Custom editor to make <see cref="FlaxEventBase"/> appear as a list in the inspector</summary>
[CustomEditor(typeof(FlaxEventBase))]
public class FlaxEventEditor : GenericEditor
{
    public override void Initialize(LayoutElementsContainer layout)
    {
        List<PersistentCall> activePersistentCalls = (Values[0] as FlaxEventBase).PersistentCallList;
        // throw new NotImplementedException();
        layout.Label("This could be your ad!");
        layout.ContainerControl.ClipChildren = false;
        layout.ContainerControl.CullChildren = false;
        // layout.ContainerControl.Offsets = new Margin(7, 7, 0, 0);
        layout.ContainerControl.BackgroundColor = Color.DarkGreen;
        // Debug.Log(layout.Control.Width);
        // (layout.Control as DropPanel).HeaderText = 
        // base.Initialize(layout);


        // var ListValueContainer = new ListValueContainer(new FlaxEditor.Scripting.ScriptType(typeof(PersistentCall)), )
        // var listContainer = new ListEditor();
        // listContainer.
        // listContainer.Initialize(layout);
        // Debug.Log(layout.Control is DropPanel);

        // TODO: Custom List layout

        // Show every persistent listener in list style

        // Add an remove listeners from list
        // var eventObject = Values[0] as FlaxEventBase;
        // var listValue = eventObject.PersistentCallList;

        // layout.CustomContainer<ListValueContainer>();

        /*
        var value = Values[0]; // Temporary fix to have values. Remove for proper impl
        List<ItemInfo> itemInfos = GetItemsForType(TypeUtils.GetObjectType(value)); // This needs to be the types in a method signature, instead of using value
        SpawnProperty(layout, itemInfos[0].GetValues(Values), itemInfos[0]); // This needs to be done for every PersistentCall Element on every element of itemInfos
        */

        // for (int i = 0; i < activePersistentCalls.Count; i++)
        // {
        //     layout.AddElement(new PersistentCallElement());
        // }

        var test = new PersistentCallElement();
        test.SetTitle("Adam Splasher");
        layout.AddElement(test);

        var propertyItem = layout.AddPropertyItem("Yes", "This is tooltip, yes");
        // propertyItem
        // test.Control.AnchorMin = new(0.1f, 0);
        // test.Control.Offsets = new Margin(7, 7, 0, 0);

        // var group = layout.Group("Yea", new PersistentCallEditor(), false);

        // group.Control.Offsets = new Margin(7, 7, 0, 0);
        // layout.Object(new PersistentCallEditor);
        // var callPanel = layout.VerticalPanel();
        // callPanel.Panel.Offsets = new Margin(7, 7, 0, 0);

        var buttonPanel = layout.HorizontalPanel();
        buttonPanel.Panel.Size = new Float2(0, 18);
        buttonPanel.Panel.BackgroundColor = Color.OrangeRed;
        // buttonPanel.Panel.AnchorPreset = AnchorPresets.BottomRight;
        // Debug.Log(buttonPanel.Panel.Size);

        // NOTE: Margin of 3 for top margin (3rd parameter) is taken from FlaxEditor.Utilities.Constants.UIMargin. 
        // Due to accessibility level, here it needs to be set manualy. If that value changes it needs to be updated here, too.
        buttonPanel.Panel.Margin = new Margin(0, 0, 3, 0); 


        var removeButton = buttonPanel.Button("-", "Remove last item");
        removeButton.Button.Size = new Float2(16, 16);
        removeButton.Button.Enabled = true; // !IsSetBlocked && 0 < persistentcalls.count
        removeButton.Button.AnchorPreset = AnchorPresets.TopRight;
        // removeButton.Button.Clicked += () =>

        var addButton = buttonPanel.Button("+", "Add new element");
        addButton.Button.Size = new Float2(16, 16);
        addButton.Button.Enabled = true; // !IsSetBlocked && persistencalls.count < int32.maxvalue
        addButton.Button.AnchorPreset = AnchorPresets.TopRight;
        // addButton.Button.Clicked += () =>
        // IsSetBlocked
    }
}
