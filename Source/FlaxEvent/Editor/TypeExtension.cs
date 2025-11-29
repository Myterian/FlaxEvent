// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

#if FLAX_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEngine;

namespace FlaxEvents;

/// <summary>
/// The Flax-interal helper class <see cref="CustomEditorsUtil"/> for finding custom editors is not available due to the accesability level,
/// this is the FlaxEvent version of that class.
/// </summary>
public static class TypeExtension
{
    private static List<Type> editorTypes = null;

    public static void Invalidate() => editorTypes = null;

    /// <summary>Gets a built-in or project-level custom editor for a specific type</summary>
    /// <param name="type">The value type to get an editor for</param>
    /// <returns>Custom Editor. Return <see cref="GenericEditor"/> if no custom editor was found.</returns>
    public static CustomEditor GetTypeEditor(this Type type)
    {
        // Invalidates the cached editor types, when scripts are reloaded/recompile
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

        // Arrays
        if (type.IsArray)
            return new ArrayEditor();

        // Lists
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return new ListEditor();

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

    /// <summary>Gets a usable instance or value of a type</summary>
    /// <param name="type">The type to get a default for</param>
    /// <returns>A value, a instance or null</returns>
    public static object GetDefault(this Type type)
    {
        if (type == null)
            return null;
            
        if (type.IsArray)
        {
            Type elementType = type.GetElementType();
            int count = 0;

            Array newArray = Array.CreateInstance(elementType, 1);

            for (int i = 0; i < count; i++)
                newArray.SetValue(elementType.GetDefault(), i);

            return newArray;
        }

        // Convert ParameterValue to Lists, because Convert.ChangeType can't
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            Type elementType = type.GetGenericArguments()[0];
            IList newList = (IList)Activator.CreateInstance(type);
            int count = 1;

            for (int i = 0; i < count; i++)
                newList.Add(elementType.GetDefault());

            return newList;
        }

        if (type.IsValueType)
            return Activator.CreateInstance(type);

        return null;
    }
}

#endif