using ModTools.UI;
using ModTools.Utils;
using UnityEngine;

namespace ModTools.Explorer
{
    internal static class GUIMaterial
    {
        public static void OnSceneReflectUnityEngineMaterial(
            SceneExplorerState state, ReferenceChain refChain, Material material)
        {
            Debug.Log($"OnSceneReflectUnityEngineMaterial(): " + System.Environment.StackTrace);

            if (!SceneExplorerCommon.SceneTreeCheckDepth(refChain))
            {
                return;
            }

            if (material == null)
            {
                SceneExplorerCommon.OnSceneTreeMessage(refChain, "null");
                return;
            }

            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.white;
            GUILayout.Label("Special Properties:");
            GUILayout.EndHorizontal();

            foreach (var prop in ShaderUtil.GetTextureProperties())
            {
                if (!material.HasProperty(prop))
                {
                    continue;
                }

                var value = material.GetTexture(prop);
                if (value == null)
                {
                    continue;
                }

                var newRefChain = refChain.Add(prop);

                var type = value.GetType();

                GUILayout.BeginHorizontal(GUIWindow.HighlightStyle);
                SceneExplorerCommon.InsertIndent(newRefChain.Indentation + 1);

                GUIExpander.ExpanderControls(state, newRefChain, type);

                GUI.contentColor = MainWindow.Instance.Config.TypeColor;

                GUILayout.Label(type.ToString() + " ");

                GUI.contentColor = MainWindow.Instance.Config.NameColor;

                GUILayout.Label(prop);

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");

                GUI.contentColor = MainWindow.Instance.Config.ValueColor;
                GUILayout.Label(value.ToString());
                GUI.contentColor = Color.white;

                GUILayout.FlexibleSpace();
                GUIButtons.SetupCommonButtons(newRefChain, value, valueIndex: 0);
                var doPaste = GUIButtons.SetupPasteButon(type, value, out var paste);
                if (value != null)
                {
                    GUIButtons.SetupJumpButton(value, newRefChain);
                }

                GUILayout.EndHorizontal();

                if (!TypeUtil.IsSpecialType(type) && state.ExpandedObjects.Contains(newRefChain.UniqueId))
                {
                    GUIReflect.OnSceneTreeReflect(state, newRefChain, value, false);
                }

                if (doPaste)
                {
                    material.SetTexture(prop, (Texture)paste);
                }
            }

            foreach (var prop in ShaderUtil.GetColorProperties())
            {
                if (!material.HasProperty(prop))
                {
                    continue;
                }

                var value = material.GetColor(prop);
                var newRefChain = refChain.Add(prop);

                var type = value.GetType();

                GUILayout.BeginHorizontal();
                SceneExplorerCommon.InsertIndent(newRefChain.Indentation + 1);

                GUIExpander.ExpanderControls(state, newRefChain, type);

                GUI.contentColor = MainWindow.Instance.Config.TypeColor;

                GUILayout.Label(type.ToString() + " ");

                GUI.contentColor = MainWindow.Instance.Config.NameColor;

                GUILayout.Label(prop);

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");

                GUI.contentColor = MainWindow.Instance.Config.ValueColor;

                var newColor = GUIControls.CustomValueField(newRefChain.UniqueId, string.Empty, GUIControls.PresentColor, value);
                if (newColor != value)
                {
                    material.SetColor(prop, newColor);
                }

                GUI.contentColor = Color.white;
                GUILayout.FlexibleSpace();
                GUIButtons.SetupCommonButtons(newRefChain, value, valueIndex: 0);
                var doPaste = GUIButtons.SetupPasteButon(type, value, out var paste);

                GUIButtons.SetupJumpButton(value, newRefChain);

                GUILayout.EndHorizontal();

                if (!TypeUtil.IsSpecialType(type) && state.ExpandedObjects.Contains(newRefChain.UniqueId))
                {
                    GUIReflect.OnSceneTreeReflect(state, newRefChain, value, false);
                }

                if (doPaste)
                {
                    material.SetColor(prop, (Color)paste);
                }
            }

            foreach (var prop in ShaderUtil.GetFloatProperties())
            {
                if (!material.HasProperty(prop))
                {
                    continue;
                }

                var value = material.GetFloat(prop);
                var newRefChain = refChain.Add(prop);

                var type = value.GetType();

                GUILayout.BeginHorizontal();
                SceneExplorerCommon.InsertIndent(newRefChain.Indentation + 1);

                GUIExpander.ExpanderControls(state, newRefChain, type);

                GUI.contentColor = MainWindow.Instance.Config.TypeColor;

                GUILayout.Label(type.ToString() + " ");

                GUI.contentColor = MainWindow.Instance.Config.NameColor;

                GUILayout.Label(prop);

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");

                GUI.contentColor = MainWindow.Instance.Config.ValueColor;

                var newValue = GUIControls.NumericValueField(newRefChain.UniqueId, string.Empty, value);
                if (newValue != value)
                {
                    material.SetFloat(prop, newValue);
                }

                GUI.contentColor = Color.white;
                GUILayout.FlexibleSpace();
                GUIButtons.SetupCommonButtons(newRefChain, value, 0);
                var doPaste = GUIButtons.SetupPasteButon(type, value, out var paste);
                GUIButtons.SetupJumpButton(value, newRefChain);

                GUILayout.EndHorizontal();

                if (!TypeUtil.IsSpecialType(type) && state.ExpandedObjects.Contains(newRefChain.UniqueId))
                {
                    GUIReflect.OnSceneTreeReflect(state, newRefChain, value, false);
                }

                if (doPaste)
                {
                    material.SetColor(prop, (Color)paste);
                }
            }

            foreach (var prop in ShaderUtil.GetVectorProperties())
            {
                if (!material.HasProperty(prop))
                {
                    continue;
                }

                var value = material.GetVector(prop);
                var newRefChain = refChain.Add(prop);

                var type = value.GetType();

                GUILayout.BeginHorizontal();
                SceneExplorerCommon.InsertIndent(newRefChain.Indentation + 1);

                GUIExpander.ExpanderControls(state, newRefChain, type);

                GUI.contentColor = MainWindow.Instance.Config.TypeColor;

                GUILayout.Label(type.ToString() + " ");

                GUI.contentColor = MainWindow.Instance.Config.NameColor;

                GUILayout.Label(prop);

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");

                GUI.contentColor = MainWindow.Instance.Config.ValueColor;

                var newValue = GUIControls.PresentVector4(newRefChain.UniqueId, value);
                if (newValue != value)
                {
                    material.SetVector(prop, newValue);
                }

                GUI.contentColor = Color.white;
                GUILayout.FlexibleSpace();
                GUIButtons.SetupCommonButtons(newRefChain, value, valueIndex: 0);
                var doPaste = GUIButtons.SetupPasteButon(type, value, out var paste);

                GUIButtons.SetupJumpButton(value, newRefChain);

                GUILayout.EndHorizontal();

                if (!TypeUtil.IsSpecialType(type) && state.ExpandedObjects.Contains(newRefChain.UniqueId))
                {
                    GUIReflect.OnSceneTreeReflect(state, newRefChain, value, false);
                }

                if (doPaste)
                {
                    material.SetColor(prop, (Color)paste);
                }
            }

            var shaderKeywords = material.shaderKeywords;
            if (shaderKeywords != null && shaderKeywords.Length > 0)
            {
                var valueTyoe = shaderKeywords.GetType();

                GUILayout.BeginHorizontal();
                SceneExplorerCommon.InsertIndent(refChain.Indentation + 2);

                GUI.contentColor = MainWindow.Instance.Config.TypeColor;
                GUILayout.Label(valueTyoe.ToString() + " ");

                GUI.contentColor = MainWindow.Instance.Config.NameColor;
                GUILayout.Label("Shader keywords ");

                GUI.contentColor = Color.white;
                GUILayout.Label(" = ");

                GUI.contentColor = MainWindow.Instance.Config.ValueColor;
                GUILayout.Label(string.Join(", ", shaderKeywords));

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUIReflect.OnSceneTreeReflect(state, refChain, material, true);
        }
    }
}