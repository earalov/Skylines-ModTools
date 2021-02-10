using ModTools.UI;
using ModTools.Utils;
using UnityEngine;

namespace ModTools.Explorer
{
    internal static class GUIMaterial
    {
        public static void OnSceneReflectUnityEngineMaterial(SceneExplorerState state, ReferenceChain refChain, Material material)
        {
            if (!SceneExplorerCommon.SceneTreeCheckDepth(refChain))
            {
                return;
            }

            if (material == null)
            {
                SceneExplorerCommon.OnSceneTreeMessage(refChain, "null");
                return;
            }

            var oldRefChain = refChain;

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

                refChain = oldRefChain.Add(prop);

                var type = value.GetType();

                GUILayout.BeginHorizontal(GUIWindow.HighlightStyle);
                SceneExplorerCommon.InsertIndent(refChain.Indentation + 1);

                GUIExpander.ExpanderControls(state, refChain, type);

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
                GUIButtons.SetupCommonButtons(refChain, value, valueIndex: 0);
                var doPaste = GUIButtons.SetupPasteButon(type, value, out var paste);
                if (value != null)
                {
                    GUIButtons.SetupJumpButton(value, refChain);
                }

                GUILayout.EndHorizontal();

                if (!TypeUtil.IsSpecialType(type) && state.ExpandedObjects.Contains(refChain.UniqueId))
                {
                    GUIReflect.OnSceneTreeReflect(state, refChain, value, false);
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
                refChain = oldRefChain.Add(prop);

                var type = value.GetType();

                GUILayout.BeginHorizontal();
                SceneExplorerCommon.InsertIndent(refChain.Indentation + 1);

                GUIExpander.ExpanderControls(state, refChain, type);

                GUI.contentColor = MainWindow.Instance.Config.TypeColor;

                GUILayout.Label(type.ToString() + " ");

                GUI.contentColor = MainWindow.Instance.Config.NameColor;

                GUILayout.Label(prop);

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");

                GUI.contentColor = MainWindow.Instance.Config.ValueColor;

                var newColor = GUIControls.CustomValueField(refChain.UniqueId, string.Empty, GUIControls.PresentColor, value);
                if (newColor != value)
                {
                    material.SetColor(prop, newColor);
                }

                GUI.contentColor = Color.white;
                GUILayout.FlexibleSpace();
                GUIButtons.SetupCommonButtons(refChain, value, valueIndex: 0);
                var doPaste = GUIButtons.SetupPasteButon(type, value, out var paste);

                GUIButtons.SetupJumpButton(value, refChain);

                GUILayout.EndHorizontal();

                if (!TypeUtil.IsSpecialType(type) && state.ExpandedObjects.Contains(refChain.UniqueId))
                {
                    GUIReflect.OnSceneTreeReflect(state, refChain, value, false);
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
                refChain = oldRefChain.Add(prop);

                var type = value.GetType();

                GUILayout.BeginHorizontal();
                SceneExplorerCommon.InsertIndent(refChain.Indentation + 1);

                GUIExpander.ExpanderControls(state, refChain, type);

                GUI.contentColor = MainWindow.Instance.Config.TypeColor;

                GUILayout.Label(type.ToString() + " ");

                GUI.contentColor = MainWindow.Instance.Config.NameColor;

                GUILayout.Label(prop);

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");

                GUI.contentColor = MainWindow.Instance.Config.ValueColor;

                var newValue = GUIControls.NumericValueField(refChain.UniqueId, string.Empty, value);
                if (newValue != value)
                {
                    material.SetFloat(prop, newValue);
                }

                GUI.contentColor = Color.white;
                GUILayout.FlexibleSpace();
                GUIButtons.SetupCommonButtons(refChain, value, 0);
                var doPaste = GUIButtons.SetupPasteButon(type, value, out var paste);
                GUIButtons.SetupJumpButton(value, refChain);

                GUILayout.EndHorizontal();

                if (!TypeUtil.IsSpecialType(type) && state.ExpandedObjects.Contains(refChain.UniqueId))
                {
                    GUIReflect.OnSceneTreeReflect(state, refChain, value, false);
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
                refChain = oldRefChain.Add(prop);

                var type = value.GetType();

                GUILayout.BeginHorizontal();
                SceneExplorerCommon.InsertIndent(refChain.Indentation + 1);

                GUIExpander.ExpanderControls(state, refChain, type);

                GUI.contentColor = MainWindow.Instance.Config.TypeColor;

                GUILayout.Label(type.ToString() + " ");

                GUI.contentColor = MainWindow.Instance.Config.NameColor;

                GUILayout.Label(prop);

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");

                GUI.contentColor = MainWindow.Instance.Config.ValueColor;

                var newValue = GUIControls.PresentVector4(refChain.UniqueId, value);
                if (newValue != value)
                {
                    material.SetVector(prop, newValue);
                }

                GUI.contentColor = Color.white;
                GUILayout.FlexibleSpace();
                GUIButtons.SetupCommonButtons(refChain, value, valueIndex: 0);
                var doPaste = GUIButtons.SetupPasteButon(type, value, out var paste);

                GUIButtons.SetupJumpButton(value, refChain);

                GUILayout.EndHorizontal();

                if (!TypeUtil.IsSpecialType(type) && state.ExpandedObjects.Contains(refChain.UniqueId))
                {
                    GUIReflect.OnSceneTreeReflect(state, refChain, value, false);
                }

                if (doPaste)
                {
                    material.SetColor(prop, (Color)paste);
                }
            }

            GUIReflect.OnSceneTreeReflect(state, refChain, material, true);
        }
    }
}