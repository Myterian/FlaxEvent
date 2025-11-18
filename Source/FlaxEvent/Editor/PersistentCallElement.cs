// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEvent;

/// <summary>Custom editor for <see cref="PersistentCall"/> elements, that appear in the <see cref="FlaxEventEditor"/></summary>
public class PersistentCallElement : LayoutElement
{
    private GroupElement parentGroup;

    public override Control Control => parentGroup.Control;

    // public override void Initialize(LayoutElementsContainer layout)
    // {
    //     layout.Group("Yeahaw", false);
    //     layout.Label("This you be you property");
    //     layout.Label("And this!");
    //     layout.Label("And this!");
    //     layout.Label("And this!");
    //     layout.Label("And this!");

    //     /*
    //     var value = Values[0]; // Temporary fix to have values. Remove for proper impl
    //     List<ItemInfo> itemInfos = GetItemsForType(TypeUtils.GetObjectType(value)); // This needs to be the types in a method signature, instead of using value
    //     SpawnProperty(layout, itemInfos[0].GetValues(Values), itemInfos[0]); // This needs to be done for every PersistentCall Element on every element of itemInfos
    //     */
    // }

    private void Init()
    {
        parentGroup.Label("This you be you property");
        parentGroup.Label("And this!");
        parentGroup.Label("And this!");
        parentGroup.Label("And this!");
        parentGroup.Label("And this!");
    }

    public void SetTitle(string title) => parentGroup.Panel.HeaderText = title;

    public PersistentCallElement()
    {
        parentGroup = new();
        // parentGroup.Panel.HeaderColor = Style.Current.CollectionBackgroundColor;
        parentGroup.Control.BackgroundColor = Style.Current.CollectionBackgroundColor;
        parentGroup.Panel.EnableContainmentLines = false;
        // parentGroup.ContainerControl.BackgroundColor = Color.Transparent;
        Init();
    }
}
