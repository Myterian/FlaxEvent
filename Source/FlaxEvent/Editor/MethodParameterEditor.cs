// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEngine;

namespace FlaxEvent;

/// <summary>
/// MethodParameterEditor class.
/// </summary>
public class MethodParameterEditor : CustomEditor
{
    public override void Initialize(LayoutElementsContainer layout)
    {
        var y = layout.Label("My Space");
        var newEl = new PropertiesListElement();
        // var x = new Vector3Editor();
        // x.Initialize(newEl);
        newEl.Label("My Space 2");
    }

    public override void Refresh()
    {
        // base.Refresh();
        Debug.Log("On Refresh");
    }

    protected override bool OnDirty(CustomEditor editor, object value, object token = null)
    {
        // return base.OnDirty(editor, value, token);
        Debug.Log("On Dirty");
        return true;
    }


}
