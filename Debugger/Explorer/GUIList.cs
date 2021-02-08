using System;
using System.Collections;
using ModTools.UI;
using ModTools.Utils;
using UnityEngine;

namespace ModTools.Explorer
{
    internal static class GUIList
    {
        public static void OnSceneTreeReflectIList(SceneExplorerState state, ReferenceChain refChain, object myProperty, TypeUtil.SmartType elementSmartType = TypeUtil.SmartType.Undefined)
        {
            if (!SceneExplorerCommon.SceneTreeCheckDepth(refChain))
            {
                return;
            }

            if (!(myProperty is IList list))
            {
                return;
            }

            var oldRefChain = refChain;
            var collectionSize = list.Count;
            if (collectionSize == 0)
            {
                GUILayout.BeginHorizontal();
                GUI.contentColor = Color.yellow;
                GUILayout.Label("List is empty!");
                GUI.contentColor = Color.white;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                return;
            }

            var listItemType = list.GetType().GetElementType();
            var flagsField = listItemType?.GetField("m_flags");
            var flagIsEnum = flagsField?.FieldType.IsEnum == true && Type.GetTypeCode(flagsField.FieldType) == TypeCode.Int32;

            GUICollectionNavigation.SetUpCollectionNavigation("List", state, refChain, oldRefChain, collectionSize, out var arrayStart, out var arrayEnd);
            for (var i = arrayStart; i <= arrayEnd; i++)
            {
                refChain = oldRefChain.Add(i);

                GUILayout.BeginHorizontal(GUIWindow.HighlightStyle);
                SceneExplorerCommon.InsertIndent(refChain.Indentation);

                GUI.contentColor = Color.white;

                var value = list[(int)i];
                var type = value?.GetType() ?? listItemType;
                var isNullOrEmpty = value == null || flagIsEnum && Convert.ToInt32(flagsField.GetValue(value)) == 0;
                if (type != null)
                {
                    if (!isNullOrEmpty)
                    {
                        GUIExpander.ExpanderControls(state, refChain, type);
                    }

                    GUI.contentColor = MainWindow.Instance.Config.TypeColor;

                    GUILayout.Label($"{type} ");
                }

                GUI.contentColor = MainWindow.Instance.Config.NameColor;

                GUILayout.Label($"{oldRefChain.LastItemName}.[{i}]");

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");

                GUI.contentColor = MainWindow.Instance.Config.ValueColor;

                GUILayout.Label(value == null ? "null" : isNullOrEmpty ? "empty" : value.ToString());

                GUI.contentColor = Color.white;

                GUILayout.FlexibleSpace();

                if (!isNullOrEmpty)
                {
                    GUIButtons.SetupCommonButtons(refChain, value, i, elementSmartType);
                }

                if (value != null)
                {
                    GUIButtons.SetupJumpButton(value, refChain);
                }

                GUILayout.EndHorizontal();

                if (!isNullOrEmpty && !TypeUtil.IsSpecialType(type) && state.ExpandedObjects.Contains(refChain.UniqueId))
                {
                    GUIReflect.OnSceneTreeReflect(state, refChain, value, false);
                }
            }
        }
    }
}