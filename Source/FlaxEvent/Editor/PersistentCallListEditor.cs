// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.Scripting;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEvent;

/// <summary>
/// PersistentParameterListEditor class.
/// </summary>
public class PersistentCallListEditor : CustomEditor
{
    // private CustomEditor persistentCallEditor = null;
    // private int callIndex = -1;

    // public void SetIndex(int newIndex) => callIndex = newIndex;

    public override void Initialize(LayoutElementsContainer layout)
    {

        // For each element in PersistentCallsList -> layout add new editor of PersistentCallEditor
        // layout.Label("Persistent Parameter");
        // var panel = layout.VerticalPanel();
        // panel.Control.Offsets = new(0);
        // panel.ContainerControl.Offsets = new();
        // layout.Control.BackgroundColor = FlaxEngine.GUI.Style.Current.CollectionBackgroundColor;
        // if (Mathf.IsNotInRange(callIndex, 0, ((List<PersistentCall>)Values[0]).Count))
        // {
        //     layout.Label("Persistent Call Not found");
        //     return;
        // }

        var list = (List<PersistentCall>)Values[0];
        // var call = (PersistentCall)Values[0];

        MemberInfo memberInfo1 = typeof(PersistentCall).GetMember("Parameters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0];
        // ScriptMemberInfo scriptMember1 = new(memberInfo1);
        // GenericEditor.ItemInfo itemInfo1 = new(scriptMember1);

        // layout.Space(20);

        // var vc1 = itemInfo1.GetValues(Values);
        // var group = layout.Group("Of you sex tape");
        var elementsPanel = layout.VerticalPanel();
        elementsPanel.Control.BackgroundColor = FlaxEngine.GUI.Style.Current.CollectionBackgroundColor;

        for (int i = 0; i < list.Count; i++)
        {

            var lvc = new ListValueContainer(new(memberInfo1.GetType()), i, Values);

            if (lvc.Count == 0 || lvc[0] == null)
                return;

            elementsPanel.Object(lvc, new PersistentCallEditor());
        }


        // var vc = itemInfo1.GetValues(Values);

        // for (int i = 0; i < call.Parameters.Length; i++)
        // {
        // var callElement = new PersistentCallElement();
        // callElement.Init(this, i);
        // layout.AddElement(callElement);


        // var editor = new PersistentCallListEditor();
        // editor.SetIndex(i);

        // layout.Object(vc, editor);

        // }

        // Add and Remove Buttons
        var buttonPanel = layout.HorizontalPanel();
        buttonPanel.Panel.Size = new Float2(0, 18);

        // NOTE: Margin of 3 for top margin (3rd parameter) is taken from FlaxEditor.Utilities.Constants.UIMargin. 
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
            PersistentCall element = new();

            if (i < oldList.Count)
                element = oldList[i];

            newList.Add(element);
        }

        // FlaxEventBase newEvent = (FlaxEventBase)Activator.CreateInstance(Values[0].GetType());
        // newEvent.SetPersistentCalls(newList);

        SetValue(newList);
        RebuildLayoutOnRefresh();
    }
}
