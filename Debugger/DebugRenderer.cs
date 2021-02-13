namespace ModTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ColossalFramework.UI;
    using ModTools.Explorer;
    using ModTools.Utils;
    using UnityEngine;
    using ModTools.UI;

    internal sealed class DebugRenderer : MonoBehaviour, IGameObject, IUIObject
    {
        private readonly List<UIComponent> hoveredComponents = new List<UIComponent>();

        private GUIStyle normalRectStyle;
        private GUIStyle hoveredRectStyle;
        private GUIStyle infoWindowStyle;

        private UIComponent hoveredComponent;

        private long previousHash = 0;

        private List<UIComponent> visibleComponents = new List<UIComponent>();
        private List<UIComponent> topLevelComponents = new List<UIComponent>();

        public bool DrawDebugInfo { get; set; }

        private static bool IncludeAll => !MainWindow.Instance.Config.DebugRendererExcludeUninteractive;

        private List<UIComponent> GetVisibleComponents()
        {
            topLevelComponents.Clear();
            for (int i = 0; i < this.transform.childCount; ++i)
            {
                UIComponent c = transform.GetChild(i).GetComponent<UIComponent>();
                if (c && c.isVisibleSelf)
                    topLevelComponents.Add(c);
            }

            topLevelComponents.Sort(RenderSortFunc);

            visibleComponents.Clear();
            foreach (UIComponent c in topLevelComponents)
            {
                if (IncludeAll || (c.isActiveAndEnabled && c.isInteractive))
                    visibleComponents.Add(c);
                TraverseRecursive(c);
            }

            return visibleComponents;
        }

        private void TraverseRecursive(UIComponent parent)
        {
            foreach (var c in parent.components)
            {
                if (c && c.isVisibleSelf)
                {
                    if (IncludeAll || (c.isActiveAndEnabled && c.isInteractive))
                        visibleComponents.Add(c);
                    TraverseRecursive(c);
                }
            }
        }

        public void Update()
        {
            var hoveredLocal = hoveredComponent;
            if (!DrawDebugInfo || hoveredLocal == null)
            {
                return;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
            {
                var refChain = ReferenceChainBuilder.ForUIComponent(hoveredLocal);

                var sceneExplorer = FindObjectOfType<SceneExplorer>();
                sceneExplorer.Show(refChain);

                if (MainWindow.Instance.Config.DebugRendererAutoTurnOff && sceneExplorer.Visible)
                {
                    DrawDebugInfo = false;
                }
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G) && hoveredComponents.Count > 1 && hoveredComponent != null)
            {
                var index = hoveredComponents.IndexOf(hoveredComponent);
                var newIndex = (index + hoveredComponents.Count + 1) % hoveredComponents.Count;
                hoveredComponent = hoveredComponents[newIndex];
            }
        }

        public void OnGUI()
        {
            if (!DrawDebugInfo)
            {
                return;
            }

            if (normalRectStyle == null)
            {
                normalRectStyle = new GUIStyle(GUI.skin.box);
                var bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, new Color(1.0f, 0.0f, 1.0f, 0.1f));
                bgTexture.Apply();
                normalRectStyle.normal.background = bgTexture;
                normalRectStyle.hover.background = bgTexture;
                normalRectStyle.active.background = bgTexture;
                normalRectStyle.focused.background = bgTexture;

                hoveredRectStyle = new GUIStyle(GUI.skin.box);
                bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, new Color(0.0f, 1.0f, 0.0f, 0.3f));
                bgTexture.Apply();
                hoveredRectStyle.normal.background = bgTexture;
                hoveredRectStyle.hover.background = bgTexture;
                hoveredRectStyle.active.background = bgTexture;
                hoveredRectStyle.focused.background = bgTexture;

                infoWindowStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = null },
                    hover = { background = null },
                    active = { background = null },
                    focused = { background = null },
                };
            }

            var components = GetVisibleComponents();

            var mouse = Input.mousePosition;
            mouse.y = Screen.height - mouse.y;

            hoveredComponents.Clear();
            long hash = 0;
            for (var i = components.Count - 1; i > 0; i--)
            {
                var component = components[i];
                var position = component.absolutePosition;
                var size = component.size;
                var rect = CalculateRealComponentRect(position, size);
                if (rect.Contains(mouse))
                {
                    hash += CalculateHash(component);
                    hoveredComponents.Add(component);
                }
            }

            if (hoveredComponent != null && hash != previousHash)
            {
                hoveredComponent = null;
                previousHash = hash;
            }

            if (hoveredComponent == null && hoveredComponents.Count > 0)
            {
                hoveredComponent = hoveredComponents[0];
            }

            foreach (var component in components)
            {
                var position = component.absolutePosition;
                var size = component.size;
                var rect = CalculateRealComponentRect(position, size);

                GUI.Box(rect, string.Empty, hoveredComponent == component ? hoveredRectStyle : normalRectStyle);
            }

            if (hoveredComponent != null)
            {
                var coords = mouse;

                var size = new Vector2(300.0f, 300.0f);

                if (coords.x + size.x >= Screen.width)
                {
                    coords.x = Screen.width - size.x;
                }

                if (coords.y + size.y >= Screen.height)
                {
                    coords.y = Screen.height - size.y;
                }

                GUI.Window(81871, new Rect(coords.x, coords.y, size.x, size.y), DoInfoWindow, string.Empty, infoWindowStyle);
            }
        }

        private static Rect CalculateRealComponentRect(Vector3 absolutePosition, Vector2 size)
        {
            var dx = Screen.width / 1920.0f;
            var dy = Screen.height / 1080.0f;
            absolutePosition.x *= dx;
            absolutePosition.y *= dy;
            size.x *= dx;
            size.y *= dy;
            return new Rect(absolutePosition.x, absolutePosition.y, size.x, size.y);
        }

        private static long CalculateHash(UIComponent c)
            => HashUtil.HashRect(new Rect(c.relativePosition.x, c.relativePosition.y, c.size.x, c.size.y));

        private void DoInfoWindow(int i)
        {
            if (hoveredComponent == null)
            {
                return;
            }

            GUI.color = Color.cyan;
            GUILayout.Label($"[Press {SettingsUI.ShowComponentKey} to show in SceneExplorer]");
            GUILayout.Label($"[Press {SettingsUI.IterateComponentKey} to iterate]");

            GUILayout.Label($"name: {hoveredComponent.name}");
            GUILayout.Label($"type: {hoveredComponent.GetType().Name}");

            if (hoveredComponent.parent != null)
            {
                GUILayout.Label($"parent: {hoveredComponent.parent?.name}");
            }

            GUILayout.Label($"anchor: {hoveredComponent.anchor}");
            GUILayout.Label($"size: {hoveredComponent.size}");
            GUILayout.Label($"position: {hoveredComponent.position}");
            GUILayout.Label($"relativePosition: {hoveredComponent.relativePosition}");
            var interactiveComponent = hoveredComponent as UIInteractiveComponent;
            if (interactiveComponent != null)
            {
                GUILayout.Label($"atlas.name: {interactiveComponent.atlas.name}");
            }

            var sprite = hoveredComponent as UISprite;
            if (sprite != null)
            {
                GUILayout.Label($"atlas.name: {sprite.atlas?.name}");
                GUILayout.Label($"spriteName: {sprite.spriteName}");
            }

            var textureSprite = hoveredComponent as UITextureSprite;
            if (textureSprite != null)
            {
                GUILayout.Label($"texture.name: {textureSprite.texture?.name}");
            }

            GUILayout.Label($"zOrder: {hoveredComponent.zOrder}");
            var hash = CalculateHash(hoveredComponent);
            GUILayout.Label($"hash: {HashUtil.HashToString(hash)}");
        }

        private int RenderSortFunc(UIComponent lhs, UIComponent rhs) => lhs.renderOrder.CompareTo(rhs.renderOrder);
    }
}