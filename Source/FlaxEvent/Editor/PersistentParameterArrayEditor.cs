// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System.Reflection;
using FlaxEditor.CustomEditors;
using FlaxEngine;

namespace FlaxEvent;

/// <summary>PersistentParameterArrayEditor class.</summary>
public class PersistentParameterArrayEditor : CustomEditor
{
    private int index = -1;

    public void SetIndex(int newIndex) => index = newIndex;

    public override void Initialize(LayoutElementsContainer layout)
    {
        if (Mathf.IsNotInRange(index, 0, ((PersistentParameter[])Values[0]).Length))
        {
            layout.Label("PersistentParameter Not found");
            return;
        }

        MemberInfo memberInfo = typeof(PersistentParameter);//.GetMember("ParameterValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0];
        var lvc = new ListValueContainer(new(memberInfo.GetType()), index, Values);

        var parameterList = layout.AddPropertyItem(((PersistentParameter)lvc[0]).ParameterType.Name, "The invokation parameter that is being used");
        parameterList.Object(lvc, new PersistentParameterEditor());

    }
}
