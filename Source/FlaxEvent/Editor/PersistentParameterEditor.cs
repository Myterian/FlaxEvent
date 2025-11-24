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
/// PersistentParameterEditor class.
/// </summary>
public class PersistentParameterEditor : CustomEditor
{
    // private int index = -1;

    // public void SetIndex(int newIndex) => index = newIndex;

    public override void Initialize(LayoutElementsContainer layout)
    {
        // TypeToElementExtension.FindEditorFromType(((PersistentParameter)Values[0]).ParameterType);
        // layout.Object()
        // layout.Label("Some Parameter");

        PersistentParameter parameter = (PersistentParameter)Values[0];
        CustomEditor editor = parameter.ParameterType.FindEditorFromType();

        MemberInfo memberInfo1 = typeof(PersistentParameter).GetMember("ParameterValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0];
        ScriptMemberInfo scriptMember1 = new(memberInfo1);
        GenericEditor.ItemInfo itemInfo1 = new(scriptMember1);

        var vc = itemInfo1.GetValues(Values);
        // var lvc = new ListValueContainer(new(memberInfo1.GetType()), index, Values);

        layout.Object(vc, editor);
    }
}
