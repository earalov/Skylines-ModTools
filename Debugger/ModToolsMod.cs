using System;
using ColossalFramework;
using ColossalFramework.Plugins;
using ICities;
using ModTools.Utils;
using UnityEngine;

namespace ModTools
{
    public sealed class ModToolsMod : IUserMod
    {
        public const string ModToolsName = "ModTools";

        private GameObject mainWindowObject;

        private GameObject mainObject;

        public static string Version { get; } = typeof(ModToolsMod).Assembly.GetName().Version.ToString();

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
        }
    }
}