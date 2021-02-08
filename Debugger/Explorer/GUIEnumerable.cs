using System.Collections;
using ModTools.UI;
using ModTools.Utils;
using UnityEngine;

namespace ModTools.Explorer
{
    internal static class GUIEnumerable
    {
        public static void OnSceneTreeReflectIEnumerable(SceneExplorerState state, ReferenceChain refChain, object myProperty, TypeUtil.SmartType elementSmartType = TypeUtil.SmartType.Undefined)
        {
            if (!SceneExplorerCommon.SceneTreeCheckDepth(refChain))
            {
                return;
            }

            if (!(myProperty is IEnumerable enumerable))
            {
                return;
            }

            uint count = 0;
            var oldRefChain = refChain;

            foreach (var value in enumerable)
            {
                refChain = oldRefChain.Add(count);

                GUILayout.BeginHorizontal(GUIWindow.HighlightStyle);
                SceneExplorerCommon.InsertIndent(refChain.Indentation);

                var type = value?.GetType();
                if (type != null)
                {
                    GUIExpander.ExpanderControls(state, refChain, type);

                    GUI.contentColor = MainWindow.Instance.Config.TypeColor;

                    GUILayout.Label(type.ToString() + " ");
                }

                GUI.contentColor = MainWindow.Instance.Config.NameColor;

                GUILayout.Label($"{oldRefChain.LastItemName}.[{count}]");

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");

                GUI.contentColor = MainWindow.Instance.Config.ValueColor;

                GUILayout.Label(value == null ? "null" : value.ToString());

                GUI.contentColor = Color.white;

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (type != null && !TypeUtil.IsSpecialType(type) && state.ExpandedObjects.Contains(refChain.UniqueId))
                {
                    GUIReflect.OnSceneTreeReflect(state, refChain, value, false, elementSmartType, string.Empty);
                }

                count++;
                if (count >= 128)
                {
                    SceneExplorerCommon.OnSceneTreeMessage(refChain, "Enumerable too large to display");
                    break;
                }
            }
        }
    }
}