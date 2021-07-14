namespace ModTools.UI
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;

    public struct MultiSelectItem
    {
        public bool Selected;
        public string Name;
        public object Value;
    }

    public static class GUIMultiSelectComboBox
    {
        private const string ExpandDownButtonText = " ▼ ";
        private static MultiPopupWindow popupWindow;

        public static MultiSelectItem[] DropDown(MultiSelectItem[] items, string value, string callerId)
        {
            if (items == null || items.Length == 0)
            {
                return items;
            }

            if (popupWindow != null && callerId == popupWindow.OwnerId)
            {
                items = popupWindow.GetItemsAndClose();
                if (!popupWindow)
                    popupWindow = null;
            }

            var popupSize = GetPopupDimensions(items.Select(item => item.Name), value);

            GUILayout.Box(value, GUILayout.Width(popupSize.x));
            var popupPosition = GUIUtility.GUIToScreenPoint(GUILayoutUtility.GetLastRect().position / UIScaler.UIScale) ;
            if (GUILayout.Button(ExpandDownButtonText, GUILayout.Width(24f)) && EnsurePopupWindow())
            {
                popupWindow.Show(callerId, items, popupPosition, popupSize);
            }

            return items;
        }

        private static bool EnsurePopupWindow()
        {
            if (popupWindow != null)
            {
                return true;
            }

            var modTools = GameObject.FindObjectOfType<MainWindow>();
            if (modTools == null)
            {
                return false;
            }

            if (modTools.GetComponent<MultiPopupWindow>() == null)
            {
                popupWindow = modTools.gameObject.AddComponent<MultiPopupWindow>();
            }

            return popupWindow != null;
        }

        private static Vector2 GetPopupDimensions(IEnumerable<string> items, string value)
        {
            float width = GUI.skin.button.CalcSize(new GUIContent(value)).x;
            float height = 0;

            foreach(var item in items)
            {
                var itemSize = GUI.skin.button.CalcSize(new GUIContent(item));
                if (itemSize.x > width)
                {
                    width = itemSize.x;
                }

                height += itemSize.y;
            }

            return new Vector2(width + 60, height + 36);
        }

        public static void MultiSelectList(MultiSelectItem[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            GUILayout.BeginVertical();

            for (int i = 0; i < items.Length; ++i)
            {
                ref var item = ref items[i];
                item.Selected = GUILayout.Toggle(item.Selected, item.Name);
            }

            GUILayout.EndVertical();
        }

        private sealed class MultiPopupWindow : MonoBehaviour, IUIObject, IGameObject
        {
            private const float MaxPopupHeight = 400f;

            private static readonly GUIStyle WindowStyle = CreateWindowStyle();

            private readonly int popupWindowId = GUIUtility.GetControlID(FocusType.Passive);
            private readonly GUIStyle style;

            private Vector2 popupScrollPosition = Vector2.zero;

            private Rect popupRect;
            private bool readyToClose;

            private MultiSelectItem[] popupItems;

            public MultiPopupWindow()
            {
                style = CreateStyle();
            }

            public string OwnerId { get; private set; }

            public void Show(string ownerId, MultiSelectItem[] items, Vector2 position, Vector2 popupSize)
            {
                OwnerId = ownerId;
                popupItems = items;
                popupRect = new Rect(position, new Vector2(popupSize.x, Mathf.Min(MaxPopupHeight, popupSize.y)));
                popupScrollPosition = default;
            }

            public void OnGUI()
            {
                if (OwnerId != null) {
                    var matrix0 = GUI.matrix;
                    GUI.matrix = UIScaler.ScaleMatrix;
                    GUI.ModalWindow(popupWindowId, popupRect, WindowFunction, string.Empty, WindowStyle);
                    GUI.matrix = matrix0;
                }
            }

            public void Update()
            {
                if (OwnerId == null)
                {
                    return;
                }

                bool clicked = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
                if (clicked)
                {
                    var mousePos = UIScaler.MousePosition;
                    if (!popupRect.Contains(mousePos))
                        readyToClose = true;
                }
            }

            private static GUIStyle CreateStyle()
            {
                var result = new GUIStyle(GUI.skin.label);
                result.hover.textColor = Color.yellow;
                var t = new Texture2D(1, 1);
                t.SetPixel(0, 0, default);
                t.Apply();
                result.hover.background = t;
                result.font = GUI.skin.font;

                return result;
            }

            private static GUIStyle CreateWindowStyle()
            {
                var background = new Texture2D(16, 16, TextureFormat.RGBA32, mipmap: false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                };

                for (var x = 0; x < background.width; x++)
                {
                    for (var y = 0; y < background.height; y++)
                    {
                        if (x == 0 || x == background.width - 1 || y == 0 || y == background.height - 1)
                        {
                            background.SetPixel(x, y, new Color(0, 0, 0, 1));
                        }
                        else
                        {
                            background.SetPixel(x, y, new Color(0.05f, 0.05f, 0.05f, 0.95f));
                        }
                    }
                }

                background.Apply();

                var result = new GUIStyle(GUI.skin.window);
                result.normal.background = background;
                result.onNormal.background = background;
                result.border.top = result.border.bottom;
                result.padding.top = result.padding.bottom;

                return result;
            }

            private void WindowFunction(int windowId)
            {
                if (OwnerId == null)
                    return;

                popupScrollPosition = GUILayout.BeginScrollView(popupScrollPosition, false, false);

                MultiSelectList(popupItems);

                GUILayout.EndScrollView();
            }

            public MultiSelectItem[] GetItemsAndClose()
            {
                var ret = popupItems;
                if (readyToClose)
                    Close();
                return ret;
            }

            private void Close()
            {
                OwnerId = null;
                popupItems = null;
                Destroy(this);
            }
        }
    }
}
