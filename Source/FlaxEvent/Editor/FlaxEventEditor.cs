// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEditor.GUI.ContextMenu;
using FlaxEditor.Scripting;
using FlaxEngine;
using FlaxEngine.GUI;

//////////////////////////////////////////////////////////////////////////////////////
//                How does this entire editor thing even work?
//
// There is next to no documentation about how the editor works and how it builds for
// presenting stuff in the inspector or in general. So, here is what I've figured out
// based on pure observation and weeks of trial and error.
// 
// A custom editor (the default editor "GenericEditor" is also a custom editor) is
// always responsible for one specific value. Even if it has an empty Initialize method
// and doesn't display or do anything: When an editor get spawned, it get handed a
// Value, that the editor is responsible for. That can be an actor, a script, a field,
// a property or an entire class or struct, that is a field or property of an actor
// or a script. But the editor never cares the parent or child(ren) value.
// 
// Meaning, an editor responisble for an actor will only set the value 'actor'.
// Yes, an actor has a string for a name, a bool for enabled and an array for tags.
// But the editor doesn't care about that, it only sets the value 'actor'. 
// Everything is bundeled as one data value.
// 
// When you call SetValue to modify anything, it's hardcoded that an editor only sets 
// the values it is responsible for. So, an editor is specifically responisble for 
// one property of a class/struct/whatever, but not for the properties child
// or parent element.
//
// That means that this FlaxEventEditor is only responsible and can only replace the
// entire flax event. If I wanted to modify only the PersistentCallList field in a
// FlaxEventBase, then I have to create a new editor and pass only the 
// PersistentCallList value to that editor (which it does in this class).
//
// This persistent call list editor then can only modify the entire list, but not an
// individual element. If we wanted to modify a specifiy element only, guess what?
// Another editor, where the value of the individual element is extracted from the
// list and handed over to the new element editor. If we wanted to affect a 
// specific element within again, new editor, and then new editor and so on.
// 
// This is how the entire editor chain for a FlaxEventBase - derived class looks:
// FlaxEventEditor ---for the persistencall List---> PersistentCallListEditor
//     PersistentCallListEditor ---for every entry---> PersistentCallEditor
//         PersistentCallEditor --for the parameter array--> PersistentParameterArrayEditor
//             PersistentParameterArrayEditor --for every entry--> PersistentParameterEditor
//
// As far as I can tell, SetValues are done from bottom to root editor, but Refresh
// is done from root to bottom. So a child editor will communicate upwards that
// a value has changed and the root will do a refresh based on that. The refresh then
// tries to apply the values to the existing editors. This caused trouble for the
// parameter editing, because the parameter types may have changed and now the root
// editor tries to apply a bool to a float editor. The solution here is to force a
// rebuild on a failed type / count comparison.
//////////////////////////////////////////////////////////////////////////////////////

namespace FlaxEvents;

/// <summary>Custom editor to make <see cref="FlaxEventBase"/> appear as a list in the inspector</summary>
[CustomEditor(typeof(FlaxEventBase)), DefaultEditor]
public class FlaxEventEditor : CustomEditor
{
    private bool isClassNameSet = false;

    public override void Initialize(LayoutElementsContainer layout)
    {

        var x = Editor.Instance.Undo.UndoOperationsStack.PeekReverse();

        List<PersistentCall> activePersistentCalls = (Values[0] as FlaxEventBase).PersistentCallList;

        // Show what kind of argument types are being passed by the event in the header name
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


        // Base panel contains a large panel for all persistent call editor elements and another panel for the buttons
        // Yes, it is required, otherwise the buttons always jump around, when some ui changes
        var basePanel = layout.VerticalPanel();

        MemberInfo memberInfo = typeof(FlaxEventBase).GetMember("PersistentCallList", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0];
        ScriptMemberInfo scriptMember = new(memberInfo);
        GenericEditor.ItemInfo itemInfo = new(scriptMember);

        var vc = itemInfo.GetValues(Values);
        var editor = new PersistentCallListEditor();
        basePanel.Object(vc, editor);

        // (layout as GroupElement).SetupContextMenu += (ContextMenu menu, DropPanel panel) =>
        // {
        //     menu.AddSeparator();
        //     menu.AddButton("Open All");
        //     menu.AddButton("Close All");
        // };

    }

}

#endif