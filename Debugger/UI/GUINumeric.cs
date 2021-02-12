using System;
using ModTools.Utils;
using UnityEngine;
using System.Collections.Generic;

namespace ModTools.UI
{
    internal static class GUINumeric
    {
        private static string lastFocusedFieldId;
        private static string lastValue;

        private static ModConfiguration Config => MainWindow.Instance.Config;

        private static bool EnterPressed()
        {
            var keycode = Event.current.keyCode;
            return keycode == KeyCode.KeypadEnter || keycode == KeyCode.Return;
        }

        /// <summary>
        /// value gets updated only after user presses enter.
        /// if the field looses focus any other way, the value is discarded.
        /// if the value is not valid, then (error) is disabled infront of the field.
        /// if user has disabled option submit on enter, then value updates only if it is valid.
        /// </summary>
        public static object NumberField(string id, string name, object value, float fieldSize)
        {
            var valueType = value.GetType();

            var focusedFieldId = GUI.GetNameOfFocusedControl();

            if (lastValue != null)
            {
                if (string.IsNullOrEmpty(focusedFieldId) && EnterPressed())
                {
                    if (id == lastFocusedFieldId)
                    {
                        if (ParseHelper.TryParse(lastValue, valueType, out object result))
                        {
                            // only apply numeric value if user presses enter.
                            value = result;
                        }

                        lastValue = null;
                    }
                }
                else if (lastFocusedFieldId != focusedFieldId || string.IsNullOrEmpty(focusedFieldId))
                {
                    // discard last value if user did not use enter to submit results
                    lastValue = null;
                }
            }

            if (id != focusedFieldId)
            {
                GUI.SetNextControlName(id);
                GUILayout.TextField(value.ToString(), GUILayout.Width(fieldSize), GUILayout.Height(22f));
            }
            else
            {
                lastValue ??= value.ToString();
                GUI.SetNextControlName(id);
                lastValue = GUILayout.TextField(lastValue, GUILayout.Width(fieldSize), GUILayout.Height(22f));
                lastValue = ParseHelper.RemoveInvalidChars(lastValue, valueType);

                if (ParseHelper.TryParse(lastValue, valueType, out object result))
                {
                    if (!Config.SubmitNumbersOnEnter)
                        value = result; // auto-submit
                }
                else
                {
                    GUI.contentColor = Config.ConsoleErrorColor;
                    GUILayout.Label("(invalid number)");
                }

                lastFocusedFieldId = focusedFieldId;
            }

            return value;
        }
   }
}