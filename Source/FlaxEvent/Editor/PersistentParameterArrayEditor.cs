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
/// PersistentParameterArrayEditor class.
/// </summary>
public class PersistentParameterArrayEditor : CustomEditor
{
    private int index = -1;

    public void SetIndex(int newIndex) => index = newIndex;

    public override void Initialize(LayoutElementsContainer layout)
    {
        // layout.Label("Hier könnte ihr array stehen");

        // ((PersistentParameter[])Values[0])


        MemberInfo memberInfo1 = typeof(PersistentParameter).GetMember("ParameterValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0];
        ScriptMemberInfo scriptMember1 = new(memberInfo1);
        GenericEditor.ItemInfo itemInfo1 = new(scriptMember1);

        var lvc = new ListValueContainer(new(memberInfo1.GetType()), index, Values);



        var parameterList = layout.AddPropertyItem(((PersistentParameter)lvc[0]).ParameterType.Name, "The invokation parameter that is being used");
        // parameterList.ContainerControl.BackgroundColor = Color.BlueViolet;
        // var space = parameterList.Space(20f);
        // space.ContainerControl.Width *= 2;
        // space.ContainerControl.BackgroundColor = Color.Blue;
        // var l = layout.ClickableLabel("Some Text");


        parameterList.Object(lvc, new PersistentParameterEditor());
        // space.ContainerControl.PerformLayout();
    }
}
