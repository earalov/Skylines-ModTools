namespace ModTools.UI
{
    using System;
    using ColossalFramework.UI;
    using ICities;
    using System.Reflection;
    using ColossalFramework;
    using ColossalFramework.UI;
    using UnityEngine;

    public static class SettingsUI
    {
        private static ModConfiguration Config => MainWindow.Instance.Config;

        public static readonly SavedInputKey MainWindowKey = new SavedInputKey(
            "MainWindowKey", FILE_NAME, KeyCode.Q, control: true, shift: false, alt: false, autoUpdate: true);

        public static readonly SavedInputKey WatchesKey = new SavedInputKey(
            "WatchesKey", FILE_NAME, KeyCode.W, control: true, shift: false, alt: false, autoUpdate: true);

        public static readonly SavedInputKey SceneExplorerKey = new SavedInputKey(
            "SceneExplorerKey", FILE_NAME, KeyCode.E, control: true, shift: false, alt: false, autoUpdate: true);

        public static readonly SavedInputKey DebugRendererKey = new SavedInputKey(
            "DebugRendererKey", FILE_NAME, KeyCode.R, control: true, shift: false, alt: false, autoUpdate: true);

        public static readonly SavedInputKey ScriptEditorKey = new SavedInputKey(
            "ScriptEditorKey", FILE_NAME, KeyCode.S, control: true, shift: false, alt: false, autoUpdate: true);

        public static readonly SavedInputKey ShowComponentKey = new SavedInputKey(
            "ShowComponentKey", FILE_NAME, KeyCode.F, control: true, shift: false, alt: false, autoUpdate: true);

        public static readonly SavedInputKey IterateComponentKey = new SavedInputKey(
            "IterateComponentKey", FILE_NAME, KeyCode.G, control: true, shift: false, alt: false, autoUpdate: true);

        public static readonly SavedInputKey SelectionToolKey = new SavedInputKey(
            "SelectionToolKey", FILE_NAME, KeyCode.M, control: true, shift: false, alt: false, autoUpdate: true);

        public static readonly SavedInputKey ConsoleKey = new SavedInputKey(
            "ConsoleKey", FILE_NAME, KeyCode.F7, control: false, shift: false, alt: false, autoUpdate: true);

        private const string FILE_NAME = "ModTools";

        static SettingsUI()
        {
            if (GameSettings.FindSettingsFileByName(FILE_NAME) == null)
            {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FILE_NAME } });
            }
        }

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
                MainWindow.Instance.SaveConfig();
            });

            helper.AddCheckbox("Scale to resolution", MainWindow.Instance.Config.ScaleToResolution, val =>
            {
                MainWindow.Instance.Config.ScaleToResolution = val;
                MainWindow.Instance.SaveConfig();
            });

            helper.AddSlider2(
                "UI Scale",
                25, 400, 10,
                Config.UIScale * 100,
                val =>
                {
                    if (Config.UIScale != val)
                    {
                        Config.UIScale = val * 0.01f;
                        MainWindow.Instance.SaveConfig();
                    }

                    return "%" + val;
                });

            var g = helper.AddGroup("Hot Keys");
            var keymappings = g.Panel().gameObject.AddComponent<UIKeymappingsPanel>();
            keymappings.AddKeymapping("Selection Tool", SelectionToolKey);
            keymappings.AddKeymapping("Debug Console", ConsoleKey);
            keymappings.AddKeymapping("Main window", MainWindowKey);
            keymappings.AddKeymapping("Watches", WatchesKey);
            keymappings.AddKeymapping("Scene Explorer", SceneExplorerKey);
            keymappings.AddKeymapping("Debug Renderer", DebugRendererKey);
            keymappings.AddKeymapping("Debug Renderer\\show in SceneExplorer", ShowComponentKey);
            keymappings.AddKeymapping("Debug Renderer\\iterate", IterateComponentKey);
        }

    }
}
