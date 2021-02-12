using System;
using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using ICities;
using UnityEngine;
using UnityEngine.SceneManagement;
using ModTools.Utils;

namespace ModTools
{
    public sealed class ModToolsMod : IUserMod
   {
        private static ModConfiguration Config => MainWindow.Instance.Config;

        public const string ModToolsName = "ModTools";

        private GameObject mainWindowObject;

        private GameObject mainObject;

        public static string Version { get; } = typeof(ModToolsMod).Assembly.GetName().Version.ToString(2);

        public string Name => ModToolsName;

        public string Description => "Debugging toolkit for modders, version " + Version;

        public void OnEnabled()
        {
            try
            {
                if (mainWindowObject != null)
                {
                    return;
                }

                CODebugBase<LogChannel>.verbose = true;
                CODebugBase<LogChannel>.EnableChannels(LogChannel.All);

                mainObject = new GameObject(ModToolsName);
                UnityEngine.Object.DontDestroyOnLoad(mainObject);

                mainWindowObject = new GameObject(ModToolsName + nameof(MainWindow));
                UnityEngine.Object.DontDestroyOnLoad(mainWindowObject);

                var modTools = mainWindowObject.AddComponent<MainWindow>();
                modTools.Initialize();

#if DEBUG
                Test.Create();
#endif

                string scene = SceneManager.GetActiveScene().name;
                if(scene != "IntroScreen" && scene != "Startup")
                {
                    // hot reload:
                    LoadingExtension.Load();
                }
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, e.Message);
            }
        }

        public void OnDisabled()
        {
            if (mainWindowObject != null)
            {
                CODebugBase<LogChannel>.verbose = false;
                UnityEngine.Object.Destroy(mainWindowObject);
                mainWindowObject = null;
            }

            if (mainObject != null)
            {
                CODebugBase<LogChannel>.verbose = false;
                UnityEngine.Object.Destroy(mainObject);
                mainObject = null;
            }

#if DEBUG
            Test.Release();
#endif

            // hot unload
            UnityEngine.Object.Destroy(UnityEngine.Object.FindObjectOfType<SelectionToolControl>());
        }


        public void OnSettingsUI(UIHelper helper)
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

            UISlider scaleSlider = null;
            scaleSlider = helper.AddSlider2(
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
        }
    }
}