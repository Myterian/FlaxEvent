// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using FlaxEditor.CustomEditors;
using FlaxEngine;

namespace FlaxEvent;

/// <summary>Custom editor to make <see cref="FlaxEventBase"/> appear as a list in the inspector</summary>
[CustomEditor(typeof(FlaxEventBase))]
public class FlaxEventEditor : CustomEditor
{
    public override void Initialize(LayoutElementsContainer layout)
    {
        // throw new NotImplementedException();
        layout.Label("This could be your ad!");

        // TODO: Custom List layout
    }
}
