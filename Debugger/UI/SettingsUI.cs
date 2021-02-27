namespace ModTools.UI
{
    using System;
    using ColossalFramework.UI;
    using ICities;
    using ColossalFramework;
    using UnityEngine;

    public class HotKey : SavedInputKey
    {
        public InputKey Default { get; private set; }

        public HotKey(string id, KeyCode key, bool control, bool shift, bool alt)
            : base(id, SettingsUI.FILE_NAME, key, control, shift, alt, autoUpdate: true)
        {
            Default = Encode(key, control, shift, alt);
        }

        public void ResetToDefault() => value = Default;
    }

    public static class SettingsUI
    {
        static SettingsUI()
        {
            if (GameSettings.FindSettingsFileByName(FILE_NAME) == null)
            {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FILE_NAME } });
            }
        }

        public const string FILE_NAME = "ModTools";

        public static readonly HotKey MainWindowKey = new HotKey(
            "MainWindowKey", KeyCode.Q, control: true, shift: false, alt: false);

        public static readonly HotKey WatchesKey = new HotKey(
            "WatchesKey", KeyCode.W, control: true, shift: false, alt: false);

        public static readonly HotKey SceneExplorerKey = new HotKey(
            "SceneExplorerKey", KeyCode.E, control: true, shift: false, alt: false);

        public static readonly HotKey DebugRendererKey = new HotKey(
            "DebugRendererKey", KeyCode.R, control: true, shift: false, alt: false);

        public static readonly HotKey ScriptEditorKey = new HotKey(
            "ScriptEditorKey", KeyCode.S, control: true, shift: false, alt: false);

        public static readonly HotKey ShowComponentKey = new HotKey(
            "ShowComponentKey", KeyCode.F, control: true, shift: false, alt: false);

        public static readonly HotKey IterateComponentKey = new HotKey(
            "IterateComponentKey", KeyCode.G, control: true, shift: false, alt: false);

        public static readonly HotKey SelectionToolKey = new HotKey(
            "SelectionToolKey", KeyCode.M, control: true, shift: false, alt: false);

        public static readonly HotKey ConsoleKey = new HotKey(
            "ConsoleKey", KeyCode.F7, control: false, shift: false, alt: false);

        private static HotKey[] Hotkeys => new HotKey[]
        {
            MainWindowKey,
            WatchesKey,
            SceneExplorerKey,
            DebugRendererKey,
            ScriptEditorKey,
            ShowComponentKey,
            IterateComponentKey,
            SelectionToolKey,
            ConsoleKey,
        };

        private static ModConfiguration Config => MainWindow.Instance.Config;

        private static void SaveConfig() => MainWindow.Instance.SaveConfig();

        public static UISlider AddSlider2(
            this UIHelperBase helper,
            string text,
            float min, float max, float step, float defaultValue,
            Func<float, string> onValueChanged)
        {
            UISlider slider = null;
            slider = helper.AddSlider(
                text, min, max, step, defaultValue,
                Func) as UISlider;

            void Func(float value)
            {
                string result = onValueChanged?.Invoke(value);
                if (slider)
                {
                    slider.parent.Find<UILabel>("Label").text = $"{text}: {result}";
                }
            }

            Func(defaultValue);

            return slider;
        }

        public static UIPanel Panel(this UIHelperBase helper) => (helper as UIHelper).self as UIPanel;

        public static void OnSettingsUI(UIHelper helper)
        {
            helper.AddButton("Reset all settings", () =>
            {
                MainWindow.Instance.Config = new ModConfiguration();
                foreach (HotKey hotkey in Hotkeys)
                {
                    hotkey.ResetToDefault();
                }
                SaveConfig();
            });

            helper.AddCheckbox("Scale to resolution", Config.ScaleToResolution, val =>
            {
                Config.ScaleToResolution = val;
                SaveConfig();
            });

            var debugRendererIncludeAll = helper.AddCheckbox(
                "Debug renderer excludes UI components that do not respond to mouse",
                Config.DebugRendererExcludeUninteractive, val =>
                {
                    Config.DebugRendererExcludeUninteractive = val;
                    SaveConfig();
                }) as UIComponent;
            debugRendererIncludeAll.tooltip = "their children will be shown either way.";

            var debugRendererAutoTurnOff = helper.AddCheckbox(
                "Automatically turn off debug renderer",
                Config.DebugRendererAutoTurnOff, val =>
            {
                Config.DebugRendererAutoTurnOff = val;
                SaveConfig();
            }) as UIComponent;
            debugRendererAutoTurnOff.tooltip = "turns off debug render when user shows UI component in scene explorer";

            helper.AddSlider2(
                "UI Scale",
                25, 400, 10,
                Config.UIScale * 100,
                val =>
                {
                    if (Config.UIScale != val)
                    {
                        Config.UIScale = val * 0.01f;
                        SaveConfig();
                    }

                    return "%" + val;
                });

            var g = helper.AddGroup("Hot Keys");
            var keymappings = g.Panel().gameObject.AddComponent<UIKeymappingsPanel>();
            keymappings.AddKeymapping("Selection Tool", SelectionToolKey);
            keymappings.AddKeymapping("Debug Console", ConsoleKey);
            keymappings.AddKeymapping("Main window", MainWindowKey);
            keymappings.AddKeymapping("Watches", WatchesKey);
            keymappings.AddKeymapping("Script Editor", ScriptEditorKey);
            keymappings.AddKeymapping("Scene Explorer", SceneExplorerKey);
            keymappings.AddKeymapping("Debug Renderer", DebugRendererKey);
            keymappings.AddKeymapping("Debug Renderer\\show in SceneExplorer", ShowComponentKey);
            keymappings.AddKeymapping("Debug Renderer\\iterate", IterateComponentKey);
        }

    }
}
