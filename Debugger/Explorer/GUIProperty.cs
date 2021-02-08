using System;
using System.Diagnostics;
using System.Reflection;
using ModTools.UI;
using ModTools.Utils;
using UnityEngine;

namespace ModTools.Explorer
{
    internal static class GUIProperty
    {
        public static void OnSceneTreeReflectProperty(SceneExplorerState state, ReferenceChain refChain, object obj, PropertyInfo property, TypeUtil.SmartType smartType = TypeUtil.SmartType.Undefined, int nameHighlightFrom = -1, int nameHighlightLength = 0)
        {
            if (!SceneExplorerCommon.SceneTreeCheckDepth(refChain))
            {
                return;
            }

            if (obj == null || property == null)
            {
                SceneExplorerCommon.OnSceneTreeMessage(refChain, "null");
                return;
            }

            GUILayout.BeginHorizontal(GUIWindow.HighlightStyle);
            SceneExplorerCommon.InsertIndent(refChain.Indentation);

            object value = null;

            Exception exceptionOnGetting = null;

            if (property.CanRead && MainWindow.Instance.Config.EvaluateProperties || state.EvaluatedProperties.Contains(refChain.UniqueId))
            {
                try
                {
                    value = property.GetValue(obj, null);
                }
                catch (Exception e)
                {
                    exceptionOnGetting = e;
                }

                if (value != null && exceptionOnGetting == null)
                {
                    GUIExpander.ExpanderControls(state, refChain, property.PropertyType, obj);
                }
            }

            GUI.contentColor = Color.white;

            if (!property.CanWrite)
            {
                GUI.enabled = false;
            }

            if (MainWindow.Instance.Config.ShowModifiers)
            {
                GUI.contentColor = MainWindow.Instance.Config.MemberTypeColor;
                GUILayout.Label("property ");

                if (!property.CanWrite)
                {
                    GUI.contentColor = MainWindow.Instance.Config.KeywordColor;
                    GUILayout.Label("const ");
                }
            }

            GUI.contentColor = MainWindow.Instance.Config.TypeColor;

            GUILayout.Label(property.PropertyType.ToString() + " ");

            GUI.contentColor = MainWindow.Instance.Config.NameColor;

            GUIMemberName.MemberName(property, nameHighlightFrom, nameHighlightLength);

            GUI.contentColor = Color.white;
            GUILayout.Label(" = ");
            GUI.contentColor = MainWindow.Instance.Config.ValueColor;
            if (exceptionOnGetting != null)
            {
                GUI.contentColor = Color.red;
                GUILayout.Label("Exception happened when getting property value");
                GUI.contentColor = Color.white;
                GUI.enabled = true;
                if (exceptionOnGetting.InnerException != null)
                {
                    GUIStackTrace.StackTraceButton(new StackTrace(exceptionOnGetting.InnerException, true), exceptionOnGetting.InnerException.Message);
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                return;
            }

            if (!MainWindow.Instance.Config.EvaluateProperties && !state.EvaluatedProperties.Contains(refChain.UniqueId))
            {
                GUI.enabled = true;

                if (GUILayout.Button("Evaluate"))
                {
                    state.EvaluatedProperties.Add(refChain.UniqueId);
                }
            }
            else if (value == null || !TypeUtil.IsSpecialType(property.PropertyType))
            {
                if (property.CanRead)
                {
                    GUILayout.Label(value == null ? "null" : value.ToString());
                }
                else
                {
                    GUILayout.Label("(no get method)");
                }

                GUI.contentColor = Color.white;
            }
            else
            {
                try
                {
                    var newValue = GUIControls.EditorValueField(refChain.UniqueId, property.PropertyType, value);
                    if (newValue != value)
                    {
                        property.SetValue(obj, newValue, null);
                    }
                }
                catch (Exception)
                {
                    if (property.CanRead)
                    {
                        GUILayout.Label(value == null ? "null" : value.ToString());
                    }
                    else
                    {
                        GUILayout.Label("(no get method)");
                    }

                    GUI.contentColor = Color.white;
                }
            }

            GUI.enabled = true;
            GUI.contentColor = Color.white;

            GUILayout.FlexibleSpace();

            GUIButtons.SetupCommonButtons(refChain, value, valueIndex: 0, smartType);
            object paste = null;
            var doPaste = property.CanWrite;
            if (doPaste)
            {
                doPaste = GUIButtons.SetupPasteButon(property.PropertyType, value, out paste);
            }

            if (value != null)
            {
                GUIButtons.SetupJumpButton(value, refChain);
            }

            GUILayout.EndHorizontal();

            if (value != null && state.ExpandedObjects.Contains(refChain.UniqueId))
            {
                GUIReflect.OnSceneTreeReflect(state, refChain, value, false);
            }

            if (doPaste)
            {
                try
                {
                    property.SetValue(obj, paste, null);
                }
                catch (Exception e)
                {
                    Logger.Warning(e.Message);
                }
            }
        }
    }
}