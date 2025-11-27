// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEditor.Modules;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEvents;

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

        // Early exit, when the class / struct that is being inspected comes with an custom editor attribute
        object[] attrbutes = type.GetCustomAttributes(false);
        CustomEditorAttribute customEditor = (CustomEditorAttribute)attrbutes.FirstOrDefault(x => x is CustomEditorAliasAttribute);

        if (customEditor != null)
            return (CustomEditor)Activator.CreateInstance(customEditor.Type);

        // For actors and scripst, we don't want their actual editors. We just want to set a object reference
        if (typeof(FlaxEngine.Object).IsAssignableFrom(type))
            return new FlaxObjectRefEditor();

        // if (typeof(Asset).IsAssignableFrom(type))
        //     return new AssetRefEditor();

        // Enums are being displayed as integers by default
        if (type.IsEnum)
            return new EnumEditor();
        
        // Get all custom editors, built in and on project level
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

        // Fallback, when no editor was found
        return new GenericEditor();
    }
}
