// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.Scripting;
using FlaxEngine;

namespace FlaxEvent;

/// <summary>
/// PersistentParameterListEditor class.
/// </summary>
public class PersistentCallListEditor : CustomEditor
{
    private int callIndex = -1;

    public void SetIndex(int newIndex) => callIndex = newIndex;

    public override void Initialize(LayoutElementsContainer layout)
    {
        // For each element in PersistentCallsList -> layout add new editor of PersistentCallEditor
        // layout.Label("Persistent Parameter");
        var panel = layout.VerticalPanel();

        // var call = (PersistentCall)Values[0];

        MemberInfo memberInfo1 = typeof(PersistentCall).GetMember("Parameters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0];
        ScriptMemberInfo scriptMember1 = new(memberInfo1);
        GenericEditor.ItemInfo itemInfo1 = new(scriptMember1);



        // var vc1 = itemInfo1.GetValues(Values);
        // var group = panel.Group("Of you sex tape");

        // for (int i = 0; i < list.Count; i++)
        // {

        var lvc = new ListValueContainer(new(memberInfo1.GetType()), callIndex, Values);


        panel.Object(lvc, new PersistentCallEditor());

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

    }

}
