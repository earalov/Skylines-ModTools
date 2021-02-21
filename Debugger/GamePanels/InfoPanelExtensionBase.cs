using System;
using System.Linq;
using System.Reflection;
using ColossalFramework.UI;
using ModTools.Utils;
using UnityEngine;

namespace ModTools.GamePanels
{
    /// <summary>A base class for the world info panel extension.</summary>
    /// <typeparam name="T">The type of the game world info panel to extend.</typeparam>
    internal abstract class InfoPanelExtensionBase<T> : IInfoPanelExtension
        where T : WorldInfoPanel
    {
        private readonly Func<InstanceID, string> getAssetName;
        private readonly Action<InstanceID> showInExplorer;
        private readonly string infoPanelName;
        private readonly FieldInfo instanceField;

        private T infoPanel;
        private UIPanel infoPanelContainer;
        private UILabel assetLabel;
        private UIButton showInExplorerButton;

        /// <summary>Initializes a new instance of the <see cref="InfoPanelExtensionBase{T}"/> class.</summary>
        /// <param name="infoPanelName">Name of the game's panel object.</param>
        /// <param name="assetNameGetter">A delegate representing a method that can return the asset name of the panel's displayed object.</param>
        /// <param name="displayAction">A delegate representing a method that can display the panel's object in the scene explorer.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="infoPanelName"/> is null or an empty string.
        /// </exception>
        protected InfoPanelExtensionBase(
            string infoPanelName,
            Func<InstanceID, string> assetNameGetter,
            Action<InstanceID> displayAction)
        {
            if (string.IsNullOrEmpty(infoPanelName))
            {
                throw new ArgumentException("The panel name cannot be null or an empty string.", nameof(infoPanelName));
            }

            getAssetName = assetNameGetter ?? throw new ArgumentNullException(nameof(assetNameGetter));
            showInExplorer = displayAction ?? throw new ArgumentNullException(nameof(displayAction));

            this.infoPanelName = infoPanelName;
            instanceField = TypeUtil.FindField(typeof(T), "m_InstanceID");
        }

        /// <summary>
        /// Gets the current value of the game panel's <see cref="InstanceID"/> that is being displayed.
        /// </summary>
        protected InstanceID CurrentInstanceId { get; private set; }

        /// <summary>Gets the UI container of the info panel where to custom controls can be placed.</summary>
        protected UIPanel ItemsPanel { get; private set; }

        /// <summary>Disables the info panel extension, if it is enabled.</summary>
        public void Disable()
        {
            DisableCore();

            if (infoPanelContainer != null)
            {
                infoPanelContainer.eventPositionChanged -= UpdatePanelContent;
                infoPanelContainer.eventSizeChanged -= UpdatePanelSize;
                infoPanelContainer = null;
            }

            if (ItemsPanel == null)
            {
                return;
            }

            showInExplorerButton.eventClick -= ShowInExplorerButtonClick;

            ItemsPanel.RemoveUIComponent(showInExplorerButton);
            UnityEngine.Object.Destroy(showInExplorerButton);

            ItemsPanel.RemoveUIComponent(assetLabel);
            UnityEngine.Object.Destroy(assetLabel);

            infoPanel.component.RemoveUIComponent(ItemsPanel);
            UnityEngine.Object.Destroy(ItemsPanel);
            ItemsPanel = null;
            infoPanel = null;
        }

        /// <summary>
        /// Adds a simple label into this info panel extension.
        /// </summary>
        /// <returns>The newly created label.</returns>
        protected UILabel AddLabel()
        {
            if (ItemsPanel == null)
            {
                return null;
            }

            var label = ItemsPanel.AddUIComponent<UILabel>();
            label.name = "ModTools Label";
            label.autoSize = true;
            return label;
        }

        /// <summary>
        /// Adds a button with specified <paramref name="text"/> as caption into this info panel extension.
        /// </summary>
        /// <param name="text">The button's caption.</param>
        /// <returns>The newly created button.</returns>
        protected UIButton AddButton(string text)
        {
            if (ItemsPanel == null)
            {
                return null;
            }

            var button = ItemsPanel.AddUIComponent<UIButton>();
            button.name = "ModTools Button";
            button.text = text;
            button.textScale = 0.8f;
            button.autoSize = true;
            button.textPadding = new RectOffset(10, 10, 5, 5);
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenu";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.textColor = new Color32(255, 255, 255, 255);
            button.disabledTextColor = new Color32(7, 7, 7, 255);
            button.hoveredTextColor = new Color32(255, 255, 255, 255);
            button.focusedTextColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = new Color32(30, 30, 44, 255);
            return button;
        }

        /// <summary>Initializes this instance and builds up the custom UI objects.</summary>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        protected bool Initialize()
        {
            ConnectToInfoPanel();
            if (infoPanelContainer != null)
            {
                infoPanelContainer.eventPositionChanged += UpdatePanelContent;
                infoPanelContainer.eventSizeChanged += UpdatePanelSize;
            }

            SetupItemsPanel();
            if (ItemsPanel == null)
            {
                return false;
            }

            assetLabel = AddLabel();
            assetLabel.padding = new RectOffset(4, 4, 4, 8);
            showInExplorerButton = AddButton("Show in Scene Explorer");
            showInExplorerButton.eventClick += ShowInExplorerButtonClick;

            var ret =  InitializeCore();
            UpdatePanelContent(default, default);
            UpdatePanelSize(default, default);
            return ret;
        }

        /// <summary>When overridden in derive classes, builds up the custom UI objects for the info panel.</summary>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        protected virtual bool InitializeCore() => true;

        /// <summary>When overridden in derived classes, destroys the custom UI objects for the info panel.</summary>
        protected virtual void DisableCore()
        {
        }

        private void ShowInExplorerButtonClick(UIComponent component, UIMouseEventParameter eventParam) => showInExplorer(CurrentInstanceId);

        private void UpdatePanelContent(UIComponent component, Vector2 value)
        {
            CurrentInstanceId = (InstanceID)instanceField.GetValue(infoPanel);
            assetLabel.text = getAssetName(CurrentInstanceId);
        }

        private void UpdatePanelSize(UIComponent component, Vector2 value)
        {
            var parentWidth = infoPanelContainer.width;
            if (ItemsPanel.width != parentWidth)
            {
                ItemsPanel.width = parentWidth;
            }
        }

        private void ConnectToInfoPanel()
        {
            var panelGameObject = GameObject.Find(infoPanelName);
            if (panelGameObject == null)
            {
                Debug.LogWarning($"Failed to extend the info panel '{infoPanelName}'. No game object '{infoPanelName}' found.");
                return;
            }

            infoPanel = panelGameObject.GetComponent<T>();
            if (infoPanel == null)
            {
                Debug.LogWarning($"Failed to extend the info panel '{infoPanelName}'. No game object's component of type '{typeof(T).Name}' found.");
                return;
            }

            infoPanelContainer = infoPanel.component as UIPanel;
            if (infoPanelContainer == null)
            {
                Debug.LogWarning($"Failed to extend the info panel '{infoPanelName}'. No main UI panel found.");
            }
        }

        private void SetupItemsPanel()
        {
            if (infoPanelContainer == null)
            {
                return;
            }

            var parentPanel = infoPanelContainer;

            if (typeof(T) == typeof(CityServiceWorldInfoPanel))
            {
                // This magic is required because the 'city service info panel' has a different layout,
                // so the standard AlignTo won't do - we must align not to the panel itself, but to a child panel.
                var panel2 = infoPanelContainer.Find<UIPanel>("MainBottom");
                panel2 = panel2?.Find<UIPanel>("Panel");
                if (panel2 != null)
                {
                    parentPanel = panel2;
                }
            }
            else if (typeof(T) == typeof(FestivalPanel) || typeof(T) == typeof(DistrictWorldInfoPanel))
            {
                // This magic is required because the 'festival panel' and 'district world info panel' are buggy,
                // they grow unlimitedly on performing auto layout - so restrict their size.
                infoPanelContainer.maximumSize = infoPanelContainer.size;
                infoPanelContainer.clipChildren = false;
            }

            var itemsPanel = parentPanel.AddUIComponent<UIPanel>();
            itemsPanel.name = "ModTools extension panel";
            itemsPanel.width = parentPanel.width;
            itemsPanel.backgroundSprite = "MenuPanel";

            itemsPanel.padding = new RectOffset(5, 5, 10, 10);
            itemsPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 4);
            itemsPanel.autoLayout = true;
            itemsPanel.autoFitChildrenVertically = true;
            itemsPanel.autoLayoutDirection = LayoutDirection.Vertical;

            itemsPanel.AlignTo(parentPanel, UIAlignAnchor.BottomLeft);

            ItemsPanel = itemsPanel;
        }
    }
}