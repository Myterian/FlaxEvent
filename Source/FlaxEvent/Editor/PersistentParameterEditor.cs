// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

using System;
using System.Reflection;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.Scripting;
using FlaxEngine.GUI;

namespace FlaxEvents;

/// <summary>PersistentParameterEditor class.</summary>
public class PersistentParameterEditor : CustomEditor
{
    private Type type;
    public override void Initialize(LayoutElementsContainer layout)
    {
        PersistentParameter parameter = (PersistentParameter)Values[0];
        type = parameter.ParameterType;

        CustomEditor editor = type.GetTypeEditor();

        // if (editor is GenericEditor)
        //     layout.Label("Editor for type '" + parameter.ParameterType.ToString() + "' could not be found");

        MemberInfo memberInfo = typeof(PersistentParameter).GetMember("ParameterValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0];
        ScriptMemberInfo scriptMember = new(memberInfo);
        GenericEditor.ItemInfo itemInfo = new(scriptMember);

        var vc = itemInfo.GetValues(Values);

        // Type has to be set manually, because arrays and lists would default back to type of System.Object 
        // and this causes editor and conversion issues
        vc.SetType(new ScriptType(type));
        
        layout.Object(vc, editor);
    }

    // This prevents a warning, where a stored value of a parameter cannot be cast to the current parameter editor.
    // It usually happens, when the target method changes and the new method parameter has a different type 
    // (and therefore editor) then the last method parameter displayed. The warning would be "cannot cast bool to float".
    // This just rebuilds the editor in case of type difference of the parameter.
    public override void Refresh()
    {
        base.Refresh();

        if (type != ((PersistentParameter)Values[0]).ParameterType)
            RebuildLayout();
    }

}

#endif