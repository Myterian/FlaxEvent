// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System.Reflection;
using FlaxEditor.CustomEditors;
using FlaxEngine;

namespace FlaxEvents;

/// <summary>PersistentParameterArrayEditor class.</summary>
public class PersistentParameterArrayEditor : CustomEditor
{
    private int index = -1;

    public void SetIndex(int newIndex) => index = newIndex;

    public override void Initialize(LayoutElementsContainer layout)
    {
        MemberInfo memberInfo = typeof(PersistentParameter);
        var lvc = new ListValueContainer(new(memberInfo.GetType()), index, Values);

        var parameterList = layout.AddPropertyItem(((PersistentParameter)lvc[0]).ParameterType.Name, "The invokation parameter that is being used");
        parameterList.Object(lvc, new PersistentParameterEditor());

    }
}
