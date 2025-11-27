// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System.Collections.Generic;
using System.Reflection;
using FlaxEditor.CustomEditors;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEvent;

/// <summary>PersistentParameterListEditor class.</summary>
public class PersistentCallListEditor : CustomEditor
{

    private int persistentCallsCount = -1;

    public override void Initialize(LayoutElementsContainer layout)
    {
        var list = (List<PersistentCall>)Values[0];
        persistentCallsCount = list.Count;

        MemberInfo memberInfo = typeof(PersistentCall);

        // PersistentCalls List elements
        var elementsPanel = layout.VerticalPanel();
        elementsPanel.Control.BackgroundColor = FlaxEngine.GUI.Style.Current.CollectionBackgroundColor;

        elementsPanel.Label("This is label");

        for (int i = 0; i < list.Count; i++)
        {
            var lvc = new ListValueContainer(new(memberInfo.GetType()), i, Values);

            if (lvc[0] == null || lvc.Count == 0)
                return;

            elementsPanel.Object(lvc, new PersistentCallEditor());
        }


        // Add and Remove Buttons
        var buttonPanel = layout.HorizontalPanel();
        buttonPanel.Panel.Size = new Float2(0, 18);

        // NOTE: Margin of 3 for top margin (3rd parameter) is taken from FlaxEditor.Utilities.Constants.UIMargin.
        // It's used in the CollectionEditor for the +/- buttons, too. Let's make the editor look consistent.
        // Due to accessibility level, here it needs to be set manualy. If that value changes it needs to be updated here, too.
        buttonPanel.Panel.Margin = new Margin(0, 0, 3, 0);


        var removeButton = buttonPanel.Button("-", "Remove last item");
        removeButton.Button.Size = new Float2(16, 16);
        removeButton.Button.Enabled = 0 < list.Count;
        removeButton.Button.AnchorPreset = AnchorPresets.TopRight;
        removeButton.Button.Clicked += () =>
        {
            if (IsSetBlocked)
                return;

            ResizePeristentCallList(list.Count - 1);
        };

        var addButton = buttonPanel.Button("+", "Add new element");
        addButton.Button.Size = new Float2(16, 16);
        addButton.Button.Enabled = true;
        addButton.Button.AnchorPreset = AnchorPresets.TopRight;
        addButton.Button.Clicked += () =>
        {
            if (IsSetBlocked)
                return;

            ResizePeristentCallList(list.Count + 1);
        };
    }


    private void ResizePeristentCallList(int newSize)
    {
        var oldList = (List<PersistentCall>)Values[0];
        List<PersistentCall> newList = new();

        for (int i = 0; i < newSize; i++)
        {
            PersistentCall element = i < oldList.Count ? oldList[i] : new();
            newList.Add(element);
        }

        SetValue(newList);
        RebuildLayoutOnRefresh();
    }

    // This prevents a warning, that gets thrown that a ListValueContainer could not
    // refresh values, because index was out of range. Index out of range means that the linked PersistentCallList element
    // of that container could not be found, because obviously the PersistentCallList element has been removed.
    // It's not a serious error, but it's annoying.
    // This just rebuilds the editor, when the persitent call list size changes.
    public override void Refresh()
    {
        base.Refresh();

        if (((List<PersistentCall>)Values[0]).Count != persistentCallsCount)
            RebuildLayout();
    }

}
