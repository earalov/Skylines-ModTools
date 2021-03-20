using System;
using ModTools.Utils;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ModTools.UI
{
    internal static class GUIControls
    {
        private const float NumberFieldSize = 100f;
        private const float StringFieldSize = 200f;
        private const float ByteFieldSize = 40f;
        private const float CharFieldSize = 25f;
        public static bool EditingMultiLineField;


        public delegate T ValuePresenterDelegate<T>(string id, T value)
            where T : struct;

        private static ModConfiguration Config => MainWindow.Instance.Config;

        public static object EditorValueField(string id, Type type, object value)
        {
            if (type == typeof(string))
            {
                return StringValueField(id, string.Empty, value as string);
            }

            if (type.IsNumeric())
            {
                return NumericValueField(id, string.Empty, value);
            }

            if (type == typeof(char))
            {
                return CharValueField(id, string.Empty, (char)value);
            }

            if (type == typeof(bool))
            {
                return BoolField(string.Empty, (bool)value);
            }

            if (type.IsEnum)
            {
                if (TypeUtil.IsBitmaskEnum(type))
                {
                    return FlagsField(id, string.Empty, value as Enum);
                }
                else
                {
                    return EnumField(id, string.Empty, value);
                }
            }

            if (type == typeof(Shader))
            {
                return ShaderField(id, (Shader)value);
            }

            if (type == typeof(Vector2))
            {
                return CustomValueField(id, string.Empty, PresentVector2, (Vector2)value);
            }

            if (type == typeof(Vector3))
            {
                return CustomValueField(id, string.Empty, PresentVector3, (Vector3)value);
            }

            if (type == typeof(Vector4))
            {
                return CustomValueField(id, string.Empty, PresentVector4, (Vector4)value);
            }

            if (type == typeof(Quaternion))
            {
                return CustomValueField(id, string.Empty, PresentQuaternion, (Quaternion)value);
            }

            if (type == typeof(Color))
            {
                return CustomValueField(id, string.Empty, PresentColor, (Color)value);
            }

            if (type == typeof(Color32))
            {
                return (Color32)CustomValueField(id, string.Empty, PresentColor, (Color)(Color32)value);
            }

            return value;
        }

        public static Vector3 PresentVector3(string id, Vector3 value)
        {
            value.x = NumericValueField(id + ".x", "x", value.x);
            value.y = NumericValueField(id + ".y", "y", value.y);
            value.z = NumericValueField(id + ".z", "z", value.z);
            return value;
        }

        public static Vector4 PresentVector4(string id, Vector4 value)
        {
            value.x = NumericValueField(id + ".x", "x", value.x);
            value.y = NumericValueField(id + ".y", "y", value.y);
            value.z = NumericValueField(id + ".z", "z", value.z);
            value.w = NumericValueField(id + ".w", "w", value.w);
            return value;
        }

        public static Quaternion PresentQuaternion(string id, Quaternion value)
        {
            var euler = value.eulerAngles;
            euler.x = NumericValueField(id + ".x", "x", euler.x);
            euler.y = NumericValueField(id + ".y", "y", euler.y);
            euler.z = NumericValueField(id + ".z", "z", euler.z);
            if (euler != value.eulerAngles)
            {
                value = Quaternion.Euler(euler);
            }

            return value;
        }

        public static Color PresentColor(string id, Color value)
        {
            var r = (byte)Mathf.Clamp(value.r * 255.0f, byte.MinValue, byte.MaxValue);
            var g = (byte)Mathf.Clamp(value.g * 255.0f, byte.MinValue, byte.MaxValue);
            var b = (byte)Mathf.Clamp(value.b * 255.0f, byte.MinValue, byte.MaxValue);
            var a = (byte)Mathf.Clamp(value.a * 255.0f, byte.MinValue, byte.MaxValue);

            r = NumericValueField(id + ".r", "r", r);
            g = NumericValueField(id + ".g", "g", g);
            b = NumericValueField(id + ".b", "b", b);
            a = NumericValueField(id + ".a", "a", a);

            value.r = Mathf.Clamp01(r / 255.0f);
            value.g = Mathf.Clamp01(g / 255.0f);
            value.b = Mathf.Clamp01(b / 255.0f);
            value.a = Mathf.Clamp01(a / 255.0f);

            if (GUILayout.Button(string.Empty, GUILayout.Width(72)))
            {
                var picker = MainWindow.Instance.ColorPicker;
                picker?.Show(id, value);
            }
            else
            {
                var picker = MainWindow.Instance.ColorPicker;
                if (picker?.Visible == true && picker.CurrentValueId == id)
                {
                    value = picker.SelectedColor;
                }
            }

            var lastRect = GUILayoutUtility.GetLastRect();
            lastRect.x += 4.0f;
            lastRect.y += 4.0f;
            lastRect.width -= 8.0f;
            lastRect.height -= 8.0f;
            GUI.DrawTexture(lastRect, ColorPicker.GetColorTexture(id, value), ScaleMode.StretchToFill);

            return value;
        }

        public static T CustomValueField<T>(string id, string name, ValuePresenterDelegate<T> presenter, T value, float ident = float.NaN)
            where T : struct
        {
            GUILayout.BeginHorizontal();

            if (!float.IsNaN(ident))
            {
                GUILayout.Space(ident);
                GUI.contentColor = Config.TypeColor;
                GUILayout.Label(value.GetType().Name);
            }

            GUI.contentColor = Config.NameColor;
            GUILayout.Label(name);

            GUI.contentColor = Config.ValueColor;

            if (!float.IsNaN(ident))
            {
                GUILayout.FlexibleSpace();
            }

            var result = presenter(id, value);

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();

            return result;
        }

        public static T NumericValueField<T>(string id, string name, T value)
            where T : struct, IConvertible =>
            (T)NumericValueField(id, name, (object)value);

        private static object NumericValueField(string id, string name, object value)
        {
            GUILayout.BeginHorizontal();

            if (!string.IsNullOrEmpty(name))
            {
                GUI.contentColor = Config.NameColor;
                GUILayout.Label(name);
            }

            GUI.contentColor = Config.ValueColor;

            var fieldSize = GetTextFieldSize(value.GetType());

            value = GUINumeric.NumberField(id, name, value, fieldSize);

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();

            return value;
        }

        /// <summary>
        /// value gets updated on the go.
        /// </summary>
        private static string StringValueField(string id, string name, string value)
        {
            GUILayout.BeginHorizontal();

            if (!string.IsNullOrEmpty(name))
            {
                GUI.contentColor = Config.NameColor;
                GUILayout.Label(name);
            }

            GUI.contentColor = Config.ValueColor;

            int nLines = Math.Min(4, value.Count(c => c == '\n') + 1);
            GUI.SetNextControlName(id);
            value = GUILayout.TextArea(value, GUILayout.Width(StringFieldSize), GUILayout.Height(22 * nLines));
            EditingMultiLineField = !string.IsNullOrEmpty(id) && GUI.GetNameOfFocusedControl() == id;

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();

            return value;
        }

        /// <summary>
        /// user cannot add or remove charcters from the text field
        /// to update value the user simply has to select the field and then 
        /// enter the new value.
        /// </summary>
        private static char CharValueField(string id, string name, char value)
        {
            GUILayout.BeginHorizontal();

            if (!string.IsNullOrEmpty(name))
            {
                GUI.contentColor = Config.NameColor;
                GUILayout.Label(name);
            }

            GUI.contentColor = Config.ValueColor;

            GUI.SetNextControlName(id);
            string newValue = GUILayout.TextField(value.ToString(), GUILayout.Width(CharFieldSize), GUILayout.Height(22f));
            if (newValue.Length >= 2)
            {
                if (newValue[0] != value)
                    value = newValue[0];
                else
                    value = newValue[1];
            }

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();

            return value;
        }

        private static bool BoolField(string name, bool value)
        {
            GUILayout.BeginHorizontal();

            if (!string.IsNullOrEmpty(name))
            {
                GUI.contentColor = Config.NameColor;
                GUILayout.Label(name);
            }

            GUI.contentColor = Config.ValueColor;

            value = GUILayout.Toggle(value, string.Empty);

            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();

            return value;
        }

        private static object EnumField(string id, string name, object value)
        {
            GUILayout.BeginHorizontal();

            try
            {
                var enumType = value.GetType();

                if (!string.IsNullOrEmpty(name))
                {
                    GUI.contentColor = Config.NameColor;
                    GUILayout.Label(name);
                }

                GUI.contentColor = Config.ValueColor;

                if (TypeUtil.IsBitmaskEnum(enumType))
                {
                    GUILayout.Label(value.ToString());
                }
                else
                {
                    var names = Enum.GetNames(enumType);
                    var values = Enum.GetValues(enumType);
                    var valueIndex = Array.IndexOf(values, value);
                    var newValueIndex = GUIComboBox.Box(valueIndex, names, id);
                    if (newValueIndex != valueIndex)
                    {
                        value = values.GetValue(newValueIndex);
                    }
                }

                GUI.contentColor = Color.white;
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            return value;
        }

        private static Enum FlagsField(string id, string name, Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            var enumType = value.GetType();
            if (!TypeUtil.IsBitmaskEnum(enumType))
                throw new ArgumentException(enumType + " Does not have [Flags] attribute.");

            GUILayout.BeginHorizontal();
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    GUI.contentColor = Config.NameColor;
                    GUILayout.Label(name);
                }

                GUI.contentColor = Config.ValueColor;

                var items = Enum2FlagArray(value);
                items = GUIMultiSelectComboBox.DropDown(items, value.ToString(), id);
                var newVal = FlagArray2Enum(items, value.GetType());
                if (!Equals(value, newVal))
                {
                    value = newVal;
                }

                GUI.contentColor = Color.white;
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            return value;
        }

        public static MultiSelectItem[] Enum2FlagArray(Enum value)
        {
            var enumType = value.GetType();
            bool signed = Enum.GetUnderlyingType(enumType).IsSignedInteger();
            var items = new List<MultiSelectItem>();

            ulong value2 = value.ToUInt64();

            foreach (Enum val in Enum.GetValues(enumType))
            {
                ulong val2 = val.ToUInt64();
                if (EnumUtil.IsPow2(val2))
                {
                    items.Add(new MultiSelectItem
                    {
                        Name = val.ToString(),
                        Selected = (value2 & val2) != 0,
                        Value = val,
                    });
                }
            }

            return items.ToArray();
        }

        public static Enum FlagArray2Enum(MultiSelectItem[] items, Type enumType)
        {
            ulong sum = 0;
            foreach (var item in items)
            {
                if (item.Selected)
                {
                    ulong val = (item.Value as Enum).ToUInt64();
                    sum |= val;
                }
            }

            return Enum.ToObject(enumType, sum) as Enum;
        }

        private static object ShaderField(string id, Shader value)
        {
            GUILayout.BeginHorizontal();

            try
            {
                GUI.contentColor = Config.ValueColor;
                var values = ShaderUtil.GetShaders();
                var valueIndex = Array.IndexOf(values, value.name);
                var newValueIndex = GUIComboBox.Box(valueIndex, values, id);
                if (newValueIndex != valueIndex)
                {
                    value = Shader.Find(values[newValueIndex]);
                }

                GUI.contentColor = Color.white;
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            return value;
        }

        private static Vector2 PresentVector2(string id, Vector2 value)
        {
            value.x = NumericValueField(id + ".x", "x", value.x);
            value.y = NumericValueField(id + ".y", "y", value.y);
            return value;
        }

        private static float GetTextFieldSize(Type valueType)
        {
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Char:
                    return CharFieldSize;

                case TypeCode.Byte:
                case TypeCode.SByte:
                    return ByteFieldSize;

                case TypeCode.String:
                    return StringFieldSize;

                default:
                    return NumberFieldSize;
            }
        }
    }
}