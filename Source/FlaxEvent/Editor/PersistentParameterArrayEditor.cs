// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

using System.Reflection;
using FlaxEditor.CustomEditors;
using FlaxEngine;

namespace FlaxEvents;

/// <summary>PersistentParameterArrayEditor class.</summary>
public class PersistentParameterArrayEditor : CustomEditor
{
    // private int index = -1;
    private int parameterCount = -1;

    // public void SetIndex(int newIndex) => index = newIndex;

    public override void Initialize(LayoutElementsContainer layout)
    {   
        MemberInfo memberInfo = typeof(PersistentParameter);
        // var lvc = new ListValueContainer(new(memberInfo.GetType()), index, Values);

        parameterCount = ((PersistentParameter[])Values[0]).Length;

        for (int i = 0; i < parameterCount; i++)
        {
            // var editor = new PersistentParameterArrayEditor();
            // editor.SetIndex(i);

            // group.Object(vc, editor);
            var lvc = new ListValueContainer(new(memberInfo.GetType()), i, Values);

            var parameterList = layout.AddPropertyItem(((PersistentParameter)lvc[0]).ParameterType.Name, "The invokation parameter that is being used");
            parameterList.Object(lvc, new PersistentParameterEditor());
        }

        // var parameterList = layout.AddPropertyItem(((PersistentParameter)lvc[0]).ParameterType.Name, "The invokation parameter that is being used");
        // parameterList.Object(lvc, new PersistentParameterEditor());

    }

    // This prevents a warning, that gets thrown that when the editor could not
    // refresh values, because index was out of range. Index out of range means that 
    // the linked PersistentParameter array element of that container could not be found,
    // because the parameter element has been removed. It's not a serious error, but it's annoying.
    // 
    // This just rebuilds the editor, when the persitent call list size changes.
    public override void Refresh()
    {
        base.Refresh();

        if (parameterCount != ((PersistentParameter[])Values[0]).Length)
            RebuildLayout();
    }

}

#endif