// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEditor.GUI.ContextMenu;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEvent;

/// <summary>Custom editor for <see cref="PersistentCall"/> elements, that appear in the <see cref="FlaxEventEditor"/></summary>
[CustomEditor(typeof(PersistentCall)), DefaultEditor]
public class PersistentCallElement : LayoutElement
{
    private GroupElement parentGroup;

    public override Control Control => parentGroup.Control;

    // public override void Initialize(LayoutElementsContainer layout)
    // {
    //     var group = layout.Group("Yeahaw", false);

    //     // parentGroup.Panel.

    //     group.Label("This you be you property");
    //     group.Label("And this!");
    //     group.Label("And this!");
    //     group.Label("And this!");
    //     group.Label("And this!");

    //     var propertyItem = group.AddPropertyItem("Target");

    //     var actorPicker = propertyItem.Custom<FlaxObjectRefPickerControl>();
    //     actorPicker.Control.Parent = propertyItem.ContainerControl;

    //     var comboBox = propertyItem.ComboBox();
    //     // actorPicker.CustomControl.Type = TypeUtils.GetType( "FlaxEngine.Actor" );

    //     // group.Custom<>

    //     /*
    //     var value = Values[0]; // Temporary fix to have values. Remove for proper impl
    //     List<ItemInfo> itemInfos = GetItemsForType(TypeUtils.GetObjectType(value)); // This needs to be the types in a method signature, instead of using value
    //     SpawnProperty(layout, itemInfos[0].GetValues(Values), itemInfos[0]); // This needs to be done for every PersistentCall Element on every element of itemInfos
    //     */
    // }

    private void Init()
    {
        parentGroup.Panel.BackgroundColor = Style.Current.CollectionBackgroundColor;
        parentGroup.Panel.EnableContainmentLines = false;
        // parentGroup.Panel.
        var propertyItem = parentGroup.AddPropertyItem("Target");
        var actorPicker = propertyItem.Custom<FlaxObjectRefPickerControl>();
        actorPicker.Control.Parent = propertyItem.ContainerControl;

        var comboBox = propertyItem.ComboBox();
        parentGroup.Label("This you be you property");
        parentGroup.Label("And this!");
        parentGroup.Label("And this!");
        parentGroup.Label("And this!");
        parentGroup.Label("And this!");

        // SetupContextMenu +=

        parentGroup.Panel.MouseButtonRightClicked += SetupContextMenu;
        // parentGroup.Panel.mouse
    }

    public void SetTitle(string title) => parentGroup.Panel.HeaderText = title;

    private void SetupContextMenu(DropPanel dropPanel, Float2 mouseLocation)
    {
        ContextMenu contextMenu = new();
        contextMenu.ItemsContainer.RemoveChildren();


        contextMenu.AddButton("Remove", () => Debug.Log("Clicked Remove"));
        contextMenu.Show(dropPanel, mouseLocation);

        // menu.AddButton("Copy", linkedEditor.Copy);
        // var b = menu.AddButton("Duplicate", () => Editor.Duplicate(Index));
        // b.Enabled = !Editor._readOnly && Editor._canResize;
        // b = menu.AddButton("Paste", linkedEditor.Paste);
        // b.Enabled = linkedEditor.CanPaste && !Editor._readOnly;

        // menu.AddSeparator();
        // b = menu.AddButton("Move up", OnMoveUpClicked);
        // b.Enabled = Index > 0 && !Editor._readOnly;

        // b = menu.AddButton("Move down", OnMoveDownClicked);
        // b.Enabled = Index + 1 < Editor.Count && !Editor._readOnly;

        // b = menu.AddButton("Remove", OnRemoveClicked);
        // b.Enabled = !Editor._readOnly && Editor._canResize;
    }

    public PersistentCallElement()
    {
        parentGroup = new();
        // parentGroup.Control.BackgroundColor = Style.Current.CollectionBackgroundColor;
        // // parentGroup.Panel.HeaderColor = Style.Current.CollectionBackgroundColor;
        // parentGroup.Control.BackgroundColor = Style.Current.CollectionBackgroundColor;
        // // parentGroup.Panel.EnableContainmentLines = false;
        // parentGroup.ContainerControl.BackgroundColor = Color.Transparent;
        // // parentGroup.Panel.
        Init();
    }
}
