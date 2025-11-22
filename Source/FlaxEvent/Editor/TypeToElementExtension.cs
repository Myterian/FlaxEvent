// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Collections.Generic;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEvent;

/// <summary>
/// TypeToElementExtension class.
/// </summary>
public static class TypeToElementExtension
{
    public static void AddTypeElement(this LayoutElementsContainer element, Type type)
    {
        // if(type == typeof(bool))
        //     return
        // 
        //      FlaxEditor.CustomEditors.Elements.CheckBoxElement
        // FlaxEditor.CustomEditors.Elements.DoubleValueElement
        //      FlaxEditor.CustomEditors.Elements.FloatValueElement
        // FlaxEditor.CustomEditors.Elements.EnumElement
        // FlaxEditor.CustomEditors.Elements.ImageElement
        //      FlaxEditor.CustomEditors.Elements.IntegerValueElement
        // FlaxEditor.CustomEditors.Elements.SignedIntegerValueElement
        // switch(type)
        // {
        //     case bool Bool: new CheckBoxElement(); break;
        //     _ => LabelElement();
        // };
        // object value = type.IsValueType ? Activator.CreateInstance(type) : null;

        // if (type == typeof(bool))
        // {
        //     element.Checkbox();
        // }

        // return value switch
        // {
        //     bool b => new CheckBoxElement().Control,
        //     int i => new SignedIntegerValueElement().Control,
        //     float f => new FloatValueElement().Control,
        //     string s => new TextBoxElement().Control,
        //     FlaxEngine.Object o => new FlaxObjectRefPickerControl(),
        //     _ => new LabelElement().Control
        // };
        // Debug.Log(Type.GetTypeCode(typeof(Actor)));

        // TypeCode typeCode = Type.GetTypeCode(type);

        object x = type switch
        {
            // bool => element.Checkbox(),
            // byte or short or int or long => element.IntegerValue(),
            // float or double => element.FloatValue(),
            // string => element.TextBox(),
            // FlaxEngine.Object => new FlaxObjectRefPickerControl(),
            // _ => element.Label($"Editor for type {type} not foudn")



            // Type t when t == typeof(bool) => element.Checkbox(),
            
            Type t when Type.GetTypeCode(t) is TypeCode.Boolean => element.Checkbox(),

            Type t when Type.GetTypeCode(t) is TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 => element.SignedIntegerValue(),

            Type t when Type.GetTypeCode(t) is TypeCode.Byte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 => element.IntegerValue(),

            Type t when Type.GetTypeCode(t) is TypeCode.Single or TypeCode.Double => element.FloatValue(),

            Type t when Type.GetTypeCode(t) is TypeCode.String => element.TextBox(),

            Type t when typeof(FlaxEngine.Object).IsAssignableFrom(t) => new FlaxObjectRefPickerControl(),

            
            // typeof(FlaxEngine.Object).IsAssignableFrom(t) => new FlaxObjectRefPickerControl(),
            _ => element.Label($"Editor for type {type} not foudn")
        };

        // return null;
            
        // switch (value)
        // {
        //     case bool b: element.Checkbox(); break;
            
        //     case short:
        //     case int: 
        //     case long: element.SignedIntegerValue(); break;

        //     default: element.Label($"Editor for type {type} not foudn"); break;
        // }

    }
}
