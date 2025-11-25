// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.Scripting;
using FlaxEngine;
using FlaxEngine.GUI;

//////////////////////////////////////////////////////////////////////////////////////
//                How does this entire editor thing even work?
//
// There is next to no documentation about how the editor works and how get build for
// presenting stuff in the inspector or in general. So, here is what I've figured out
// based on pure observation and weeks of trial and error.
//
// A custom editor (the default editor "GenericEditor" is also a custom editor) is
// always responsible for a set of values. Even if it has an empty Initialize method
// or doesn't do anything in general: If an editor gets spawned, a subset of values
// gets linked to that editor. When you call SetValue to modify anything, it's
// hardcoded that an editor only sets the values it is responsible for. So, an editor
// is specifically responisble for one property of a class/struct/whatever, but not
// for the properties child or parent element.
//
// That means that this FlaxEventEditor is only responsible and can only replace the
// entire flax event. If I wanted to modify only the PersistentCallList field in a
// FlaxEventBase, then I have to create a new editor and pass only the 
// PersistentCallList value to that editor (which it does in this class).
//
// This persistent call list editor then can only modify the entire list, but not an
// individual element. If we wanted to modify a specifiy element only, guess what?
// Another editor, where the values of that specific element are extracted from that
// list and handed over to the new specific element editor. If we wanted to only
// affect a specific element on there, new editor, and then new editor and so on
// 
// This is how the entire editor chain for a FlaxEventBase - derived class looks:
// FlaxEventEditor ---for the persistencall List---> PersistentCallListEditor
//     PersistentCallListEditor ---for every entry---> PersistentCallEditor
//         PersistentCallEditor --for the parameter array--> PersistentParameterArrayEditor
//             PersistentParameterArrayEditor --for every entry--> PersistentParameterEditor
//////////////////////////////////////////////////////////////////////////////////////

namespace FlaxEvent;

/// <summary>Custom editor to make <see cref="FlaxEventBase"/> appear as a list in the inspector</summary>
[CustomEditor(typeof(FlaxEventBase)), DefaultEditor]
public class FlaxEventEditor : CustomEditor
{
    public int CallsCount { get; private set; } = -1;
    private bool isClassNameSet = false;

    public override void Initialize(LayoutElementsContainer layout)
    {
        List<PersistentCall> activePersistentCalls = (Values[0] as FlaxEventBase).PersistentCallList;
        CallsCount = activePersistentCalls.Count;

        // if (activePersistentCalls.Count == 0)
        //     return;

        // ChildrenEditors[i].

        // RevertValueWithChildren = true;

        // Show what kind of argument types are being passed by the event. This helps to select methods with the same signature.
        DropPanel dropPanel = layout.Control as DropPanel;
        Type[] argTypes = Values[0].GetType().GetGenericArguments();


        if (!isClassNameSet)
        {
            isClassNameSet = true;
            StringBuilder headerBuilder = new();
            headerBuilder.Append(dropPanel.HeaderText);

            if (0 < argTypes.Length)
            {
                headerBuilder.Append(" <");

                for (int i = 0; i < argTypes.Length; i++)
                {
                    headerBuilder.Append(argTypes[i].ToString().Split('.').Last());

                    if (i < argTypes.Length - 1)
                        headerBuilder.Append(", ");
                }

                headerBuilder.Append(">");
            }

            headerBuilder.Append(" (");
            headerBuilder.Append(activePersistentCalls.Count);
            headerBuilder.Append(')');
            dropPanel.HeaderText = headerBuilder.ToString();
        }


        FlaxEventBase eventBase = Values[0] as FlaxEventBase;
        // if (eventBase.PersistentCallList.Count == 0)
        //     return;


        // Base panel contains a large panel for all persisten call editor elements and another panel for the buttons
        // Yes, it is required, otherwise the buttons always jump around, when some ui changes
        var basePanel = layout.VerticalPanel();

        MemberInfo memberInfo = typeof(FlaxEventBase).GetMember("PersistentCallList", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0];
        ScriptMemberInfo scriptMember = new(memberInfo);
        GenericEditor.ItemInfo itemInfo = new(scriptMember);

        var vc = itemInfo.GetValues(Values);

        // var elementsPanel = basePanel.VerticalPanel();
        // basePanel.Control.BackgroundColor = FlaxEngine.GUI.Style.Current.CollectionBackgroundColor;

        // for (int i = 0; i < eventBase.PersistentCallList.Count; i++)
        // {
        var editor = new PersistentCallListEditor();
        // editor.SetIndex(i);

        basePanel.Object(vc, editor);
        // }

        return;

        // Add and Remove Buttons
        var buttonPanel = basePanel.HorizontalPanel();
        buttonPanel.Panel.Size = new Float2(0, 18);

        // NOTE: Margin of 3 for top margin (3rd parameter) is taken from FlaxEditor.Utilities.Constants.UIMargin. 
        // Due to accessibility level, here it needs to be set manualy. If that value changes it needs to be updated here, too.
        buttonPanel.Panel.Margin = new Margin(0, 0, 3, 0);


        var removeButton = buttonPanel.Button("-", "Remove last item");
        removeButton.Button.Size = new Float2(16, 16);
        removeButton.Button.Enabled = 0 < activePersistentCalls.Count;
        removeButton.Button.AnchorPreset = AnchorPresets.TopRight;
        removeButton.Button.Clicked += () =>
        {
            if (IsSetBlocked)
                return;

            ResizePeristentCallList(activePersistentCalls.Count - 1);
        };

        var addButton = buttonPanel.Button("+", "Add new element");
        addButton.Button.Size = new Float2(16, 16);
        addButton.Button.Enabled = true;
        addButton.Button.AnchorPreset = AnchorPresets.TopRight;
        addButton.Button.Clicked += () =>
        {
            if (IsSetBlocked)
                return;

            ResizePeristentCallList(activePersistentCalls.Count + 1);
        };
    }


    private void ResizePeristentCallList(int newSize)
    {
        var oldList = (Values[0] as FlaxEventBase).PersistentCallList;
        List<PersistentCall> newList = new();

        for (int i = 0; i < newSize; i++)
        {
            PersistentCall element = new();

            if (i < oldList.Count)
                element = oldList[i];

            newList.Add(element);
        }

        FlaxEventBase newEvent = (FlaxEventBase)Activator.CreateInstance(Values[0].GetType());
        newEvent.SetPersistentCalls(newList);

        SetValue(newEvent);
        RebuildLayoutOnRefresh();
    }

    public override void Refresh()
    {
        // base.Refresh();
        if(CallsCount != (Values[0] as FlaxEventBase).PersistentCallList.Count)
            RebuildLayout();
    }


}
