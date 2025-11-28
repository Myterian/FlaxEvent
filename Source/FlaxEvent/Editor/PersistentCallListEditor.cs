// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

using System.Collections.Generic;
using System.Reflection;
using FlaxEditor.CustomEditors;
using FlaxEngine;
using FlaxEngine.GUI;
using FlaxEngine.Json;
using FlaxEngine.Utilities;

namespace FlaxEvents;

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

        for (int i = 0; i < list.Count; i++)
        {
            var lvc = new ListValueContainer(new(memberInfo.GetType()), i, Values);

            if (lvc[0] == null || lvc.Count == 0)
                return;

            var newEditor = elementsPanel.Object(lvc, new PersistentCallEditor());
            (newEditor as PersistentCallEditor).Setup(this, i);
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

    #region This Editor Methods

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

    #endregion

    #region Child Editor Methods

    /// <summary>Duplicates a persistent call and adds it to the end of a persistent calls list</summary>
    /// <param name="index">The index of the element, that gets duplicated</param>
    public void DuplicatePersistentCall(int index)
    {
        var oldList = (List<PersistentCall>)Values[0];

        if (Mathf.IsNotInRange(index, 0, oldList.Count - 1))
            return;

        PersistentCall newCall = oldList[index].DeepClone();
        oldList.Add(newCall);

        SetValue(oldList);
        RebuildLayoutOnRefresh();
    }

    /// <summary>Removes an element from the persistent call list</summary>
    /// <param name="index">The index of the element to remove</param>
    public void RemovePersistentCall(int index)
    {
        var oldList = (List<PersistentCall>)Values[0];
        List<PersistentCall> newList = new();

        for (int i = 0; i < index; i++)
        {
            newList.Add(oldList[i]);
        }

        for (int i = index + 1; i < oldList.Count; i++)
        {
            newList.Add(oldList[i]);
        }

        SetValue(newList);
        RebuildLayoutOnRefresh();
    }

    /// <summary>Pastes an element to a persistent call list element</summary>
    /// <param name="index">The index to paste the persistent call to</param>
    public void PastePersistentCall(int index)
    {
        PersistentCall newCall = JsonSerializer.Deserialize<PersistentCall>(Clipboard.Text);

        if (newCall == null)
            return;

        var calls = (List<PersistentCall>)Values[0];
        calls[index] = newCall;

        SetValue(calls);
        RebuildLayoutOnRefresh();
    }

    /// <summary>Swaps a persistent call to a different index of the persistent call list</summary>
    /// <param name="fromIndex">The current index of the element</param>
    /// <param name="toIndex">The new index of the element after the swap</param>
    public void MovePersistentCall(int fromIndex, int toIndex)
    {
        var calls = (List<PersistentCall>)Values[0];

        if (Mathf.IsNotInRange(toIndex, 0, calls.Count - 1))
            return;

        PersistentCall tmp = calls[fromIndex];

        calls[fromIndex] = calls[toIndex];
        calls[toIndex] = tmp;

        SetValue(calls);
        RebuildLayoutOnRefresh();
    }

    #endregion

}

#endif