// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEditor.Modules;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEvent;

/// <summary>
/// The Flax-interal helper class <see cref="CustomEditorsUtil"/> for finding custom editors is not available due to the accesability level,
/// this is the FlaxEvent version of that class.
/// </summary>
public static class TypeToElementExtension
{
    private static List<Type> editorTypes = null;

    public static void Invalidate() => editorTypes = null;

    public static CustomEditor FindEditorFromType(this Type type)
    {
        ScriptsBuilder.ScriptsReloadEnd -= Invalidate;
        ScriptsBuilder.ScriptsReloadEnd += Invalidate;



        Type typeToProcess = type;

        if (type.IsAssignableFrom(typeof(FlaxEngine.Object)))
        {
            return new FlaxObjectRefEditor();
        }

        editorTypes ??= AppDomain.CurrentDomain.GetAssemblies()
                                                .SelectMany(x =>
                                                {
                                                    Type[] types = null;
                                                    try { types = x.GetTypes(); } catch { }
                                                    return types ?? Enumerable.Empty<Type>();
                                                })
                                                .Where(t => typeof(CustomEditor).IsAssignableFrom(t) && !t.IsAbstract)
                                                .ToList();


        for (int i = 0; i < editorTypes.Count; i++)
        {
            Type editorType = editorTypes[i];

            List<object> attributes = editorType.GetCustomAttributes(false).Where(x => x.GetType() == typeof(CustomEditorAttribute)).ToList();

            for (int x = 0; x < attributes.Count; x++)
            {
                if (((CustomEditorAttribute)attributes[x]).Type != type)
                    continue;

                return (CustomEditor)Activator.CreateInstance(editorType);
            }
        }

        return new GenericEditor();
    }
}
