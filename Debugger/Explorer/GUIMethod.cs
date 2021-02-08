using ModTools.UI;
using System.Reflection;
using UnityEngine;

namespace ModTools.Explorer
{
    internal static class GUIMethod
    {
        public static void OnSceneTreeReflectMethod(ReferenceChain refChain, object obj, MethodInfo method, int nameHighlightFrom = -1, int nameHighlightLength = 0)
        {
            if (!SceneExplorerCommon.SceneTreeCheckDepth(refChain))
            {
                return;
            }

            if (obj == null || method == null)
            {
                SceneExplorerCommon.OnSceneTreeMessage(refChain, "null");
                return;
            }

            GUILayout.BeginHorizontal(GUIWindow.HighlightStyle);
            SceneExplorerCommon.InsertIndent(refChain.Indentation);

            GUI.contentColor = MainWindow.Instance.Config.MemberTypeColor;
            GUILayout.Label("method ");
            GUI.contentColor = Color.white;
            GUILayout.Label(method.ReturnType + " ");
            GUIMemberName.MemberName(method, nameHighlightFrom, nameHighlightLength);
            GUI.contentColor = Color.white;
            GUILayout.Label(method.ReturnType + "(");
            GUI.contentColor = MainWindow.Instance.Config.NameColor;

            var first = true;
            foreach (var param in method.GetParameters())
            {
                if (!first)
                {
                    GUILayout.Label(", ");
                }
                else
                {
                    first = false;
                }

                GUILayout.Label(param.ParameterType.ToString() + " " + param.Name);
            }

            GUI.contentColor = Color.white;
            GUILayout.Label(")");

            GUILayout.FlexibleSpace();
            if (!method.IsGenericMethod)
            {
                if (method.GetParameters().Length == 0)
                {
                    if (GUILayout.Button("Invoke", GUILayout.ExpandWidth(false)))
                    {
                        method.Invoke(method.IsStatic ? null : obj, new object[] { });
                    }
                }
                else if (method.GetParameters().Length == 1
                         && method.GetParameters()[0].ParameterType.IsInstanceOfType(obj))
                {
                    if (GUILayout.Button("Invoke", GUILayout.ExpandWidth(false)))
                    {
                        method.Invoke(method.IsStatic ? null : obj, new[] { obj });
                    }
                }
            }

            GUILayout.EndHorizontal();
        }
    }
}