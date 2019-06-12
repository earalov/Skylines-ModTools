﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ModTools.Explorer;
using UnityEngine;

namespace ModTools
{
    public class SceneExplorer : GUIWindow
    {
        public Dictionary<GameObject, bool> sceneRoots = new Dictionary<GameObject, bool>();
        private string findGameObjectFilter = string.Empty;
        private string findObjectTypeFilter = string.Empty;
        private string searchDisplayString = string.Empty;

        private readonly GUIArea headerArea;
        private readonly GUIArea sceneTreeArea;
        private readonly GUIArea componentArea;

        private Vector2 sceneTreeScrollPosition = Vector2.zero;
        private Vector2 componentScrollPosition = Vector2.zero;
        private SceneExplorerState state;

        private readonly float windowTopMargin = 16.0f;
        private readonly float windowBottomMargin = 8.0f;

        private readonly float headerHeightCompact = 1.65f;
        private readonly float headerHeightExpanded = 17.0f;
        private bool headerExpanded;

        private readonly float sceneTreeWidth = 320.0f;

        public SceneExplorer()
            : base("Scene Explorer", new Rect(128, 440, 800, 500), skin)
        {
            onDraw = DrawWindow;
            onException = ExceptionHandler;
            onUnityGUI = GUIComboBox.DrawGUI;

            headerArea = new GUIArea(this);
            sceneTreeArea = new GUIArea(this);
            componentArea = new GUIArea(this);
            state = new SceneExplorerState();

            RecalculateAreas();
        }

        public void Awake() => Plopper.Reset();

        public void Update() => Plopper.Update();

        public void RecalculateAreas()
        {
            headerArea.absolutePosition.y = windowTopMargin;
            headerArea.relativeSize.x = 1.0f;

            if (rect.width < Screen.width / 4.0f && state.CurrentRefChain != null)
            {
                sceneTreeArea.relativeSize = Vector2.zero;
                sceneTreeArea.relativeSize = Vector2.zero;

                componentArea.absolutePosition.x = 0.0f;
                componentArea.relativeSize.x = 1.0f;
                componentArea.relativeSize.y = 1.0f;
                componentArea.absoluteSize.x = 0.0f;
            }
            else
            {
                sceneTreeArea.relativeSize.y = 1.0f;
                sceneTreeArea.absoluteSize.x = sceneTreeWidth;

                componentArea.absolutePosition.x = sceneTreeWidth;
                componentArea.relativeSize.x = 1.0f;
                componentArea.relativeSize.y = 1.0f;
                componentArea.absoluteSize.x = -sceneTreeWidth;
            }

            var headerHeight = headerExpanded ? headerHeightExpanded : headerHeightCompact;
            headerHeight *= ModTools.Instance.config.fontSize;
            headerHeight += 32.0f;

            headerArea.absoluteSize.y = headerHeight - windowTopMargin;
            sceneTreeArea.absolutePosition.y = headerHeight - windowTopMargin;
            sceneTreeArea.absoluteSize.y = -(headerHeight - windowTopMargin) - windowBottomMargin;
            componentArea.absolutePosition.y = headerHeight - windowTopMargin;
            componentArea.absoluteSize.y = -(headerHeight - windowTopMargin) - windowBottomMargin;
        }

        private void ExceptionHandler(Exception ex)
        {
            Debug.LogException(ex);
            state = new SceneExplorerState();
            sceneRoots = GameObjectUtil.FindSceneRoots();
            TypeUtil.ClearTypeCache();
        }

        public void Refresh()
        {
            sceneRoots = GameObjectUtil.FindSceneRoots();
            TypeUtil.ClearTypeCache();
        }

        public void ExpandFromRefChain(ReferenceChain refChain)
        {
            if (refChain == null)
            {
                Log.Error("SceneExplorer: ExpandFromRefChain(): Null refChain");
                return;
            }
            if (refChain.Length == 0)
            {
                Log.Error("SceneExplorer: ExpandFromRefChain(): Invalid refChain, expected Length >= 0");
                return;
            }

            if (refChain.FirstItemType != ReferenceChain.ReferenceType.GameObject)
            {
                Log.Error($"SceneExplorer: ExpandFromRefChain(): invalid chain type for element [0] - expected {ReferenceChain.ReferenceType.GameObject}, got {refChain.FirstItemType}");
                return;
            }

            sceneRoots.Clear();
            ClearExpanded();
            searchDisplayString = $"Showing results for \"{refChain}\"";

            var rootGameObject = (GameObject)refChain.GetChainItem(0);
            sceneRoots.Add(rootGameObject, true);

            var expandedRefChain = new ReferenceChain().Add(rootGameObject);
            state.ExpandedGameObjects.Add(expandedRefChain.UniqueId);

            for (var i = 1; i < refChain.Length; i++)
            {
                switch (refChain.GetChainItemType(i))
                {
                    case ReferenceChain.ReferenceType.GameObject:
                        var go = (GameObject)refChain.GetChainItem(i);
                        expandedRefChain = expandedRefChain.Add(go);
                        state.ExpandedGameObjects.Add(expandedRefChain.UniqueId);
                        break;

                    case ReferenceChain.ReferenceType.Component:
                        var component = (Component)refChain.GetChainItem(i);
                        expandedRefChain = expandedRefChain.Add(component);
                        state.ExpandedComponents.Add(expandedRefChain.UniqueId);
                        break;

                    case ReferenceChain.ReferenceType.Field:
                        var field = (FieldInfo)refChain.GetChainItem(i);
                        expandedRefChain = expandedRefChain.Add(field);
                        state.ExpandedObjects.Add(expandedRefChain.UniqueId);
                        break;

                    case ReferenceChain.ReferenceType.Property:
                        var property = (PropertyInfo)refChain.GetChainItem(i);
                        expandedRefChain = expandedRefChain.Add(property);
                        state.ExpandedObjects.Add(expandedRefChain.UniqueId);
                        break;

                    case ReferenceChain.ReferenceType.EnumerableItem:
                        var index = (int)refChain.GetChainItem(i);
                        state.SelectedArrayStartIndices[expandedRefChain.UniqueId] = index;
                        state.SelectedArrayEndIndices[expandedRefChain.UniqueId] = index;
                        expandedRefChain = expandedRefChain.Add(index);
                        state.ExpandedObjects.Add(expandedRefChain.UniqueId);
                        break;
                }
            }

            state.CurrentRefChain = refChain.Clone();
            state.CurrentRefChain.IdentOffset = -state.CurrentRefChain.Length;
        }

        public void DrawHeader()
        {
            headerArea.Begin();

            if (headerExpanded)
            {
                DrawExpandedHeader();
            }
            else
            {
                DrawCompactHeader();
            }

            headerArea.End();
        }

        public void DrawCompactHeader()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("▼", GUILayout.ExpandWidth(false)))
            {
                headerExpanded = true;
                RecalculateAreas();
            }

            if (GUILayout.Button("Refresh", GUILayout.ExpandWidth(false)))
            {
                Refresh();
            }

            if (GUILayout.Button("Fold all/ Clear", GUILayout.ExpandWidth(false)))
            {
                ClearExpanded();
                Refresh();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public void DrawExpandedHeader()
        {
            GUILayout.BeginHorizontal();

            GUI.contentColor = Color.green;
            GUILayout.Label("Show:", GUILayout.ExpandWidth(false));
            GUI.contentColor = Color.white;

            var showFields = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerShowFields, string.Empty);
            if (ModTools.Instance.config.sceneExplorerShowFields != showFields)
            {
                ModTools.Instance.config.sceneExplorerShowFields = showFields;
                ModTools.Instance.SaveConfig();
            }
            GUILayout.Label("Fields");

            GUILayout.Space(ModTools.Instance.config.sceneExplorerTreeIdentSpacing);
            var showConsts = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerShowConsts, string.Empty);
            if (ModTools.Instance.config.sceneExplorerShowConsts != showConsts)
            {
                ModTools.Instance.config.sceneExplorerShowConsts = showConsts;
                ModTools.Instance.SaveConfig();
            }
            GUILayout.Label("Constants");

            GUILayout.Space(ModTools.Instance.config.sceneExplorerTreeIdentSpacing);
            var showProperties = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerShowProperties, string.Empty);
            if (ModTools.Instance.config.sceneExplorerShowProperties != showProperties)
            {
                ModTools.Instance.config.sceneExplorerShowProperties = showProperties;
                ModTools.Instance.SaveConfig();
            }
            GUILayout.Label("Properties");

            GUILayout.Space(ModTools.Instance.config.sceneExplorerTreeIdentSpacing);
            var showMethods = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerShowMethods, string.Empty);
            if (ModTools.Instance.config.sceneExplorerShowMethods != showMethods)
            {
                ModTools.Instance.config.sceneExplorerShowMethods = showMethods;
                ModTools.Instance.SaveConfig();
            }
            GUILayout.Label("Methods");

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Configure font & colors", GUILayout.ExpandWidth(false)))
            {
                ModTools.Instance.sceneExplorerColorConfig.visible = true;
                ModTools.Instance.sceneExplorerColorConfig.rect.position = rect.position + new Vector2(32.0f, 32.0f);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.green;
            GUILayout.Label("Show field/ property modifiers:", GUILayout.ExpandWidth(false));
            var showModifiers = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerShowModifiers, string.Empty);
            if (showModifiers != ModTools.Instance.config.sceneExplorerShowModifiers)
            {
                ModTools.Instance.config.sceneExplorerShowModifiers = showModifiers;
                ModTools.Instance.SaveConfig();
            }

            GUI.contentColor = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.green;
            GUILayout.Label("Show inherited members:", GUILayout.ExpandWidth(false));
            var showInheritedMembers = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerShowInheritedMembers, string.Empty);
            if (showInheritedMembers != ModTools.Instance.config.sceneExplorerShowInheritedMembers)
            {
                ModTools.Instance.config.sceneExplorerShowInheritedMembers = showInheritedMembers;
                ModTools.Instance.SaveConfig();
                TypeUtil.ClearTypeCache();
            }

            GUI.contentColor = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.green;
            GUILayout.Label("Evaluate properties automatically:", GUILayout.ExpandWidth(false));
            var evaluatePropertiesAutomatically = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerEvaluatePropertiesAutomatically, string.Empty);
            if (evaluatePropertiesAutomatically != ModTools.Instance.config.sceneExplorerEvaluatePropertiesAutomatically)
            {
                ModTools.Instance.config.sceneExplorerEvaluatePropertiesAutomatically = evaluatePropertiesAutomatically;
                ModTools.Instance.SaveConfig();
            }

            GUI.contentColor = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.green;
            GUILayout.Label("Sort alphabetically:", GUILayout.ExpandWidth(false));
            GUI.contentColor = Color.white;
            var sortAlphabetically = GUILayout.Toggle(ModTools.Instance.config.sceneExplorerSortAlphabetically, string.Empty);
            if (sortAlphabetically != ModTools.Instance.config.sceneExplorerSortAlphabetically)
            {
                ModTools.Instance.config.sceneExplorerSortAlphabetically = sortAlphabetically;
                ModTools.Instance.SaveConfig();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            DrawFindGameObjectPanel();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("▲", GUILayout.ExpandWidth(false)))
            {
                headerExpanded = false;
                RecalculateAreas();
            }

            if (GUILayout.Button("Refresh", GUILayout.ExpandWidth(false)))
            {
                Refresh();
            }

            if (GUILayout.Button("Fold all/ Clear", GUILayout.ExpandWidth(false)))
            {
                ClearExpanded();
                Refresh();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawFindGameObjectPanel()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("GameObject.Find");
            findGameObjectFilter = GUILayout.TextField(findGameObjectFilter, GUILayout.Width(256));

            if (findGameObjectFilter.Trim().Length == 0)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Find"))
            {
                ClearExpanded();
                var go = GameObject.Find(findGameObjectFilter.Trim());
                if (go != null)
                {
                    sceneRoots.Clear();
                    state.ExpandedGameObjects.Add(new ReferenceChain().Add(go).UniqueId);
                    sceneRoots.Add(go, true);
                    sceneTreeScrollPosition = Vector2.zero;
                    searchDisplayString = $"Showing results for GameObject.Find(\"{findGameObjectFilter}\")";
                }
            }

            if (GUILayout.Button("Reset"))
            {
                ClearExpanded();
                sceneRoots = GameObjectUtil.FindSceneRoots();
                sceneTreeScrollPosition = Vector2.zero;
                searchDisplayString = string.Empty;
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("GameObject.FindObjectsOfType");
            findObjectTypeFilter = GUILayout.TextField(findObjectTypeFilter, GUILayout.Width(256));

            if (findObjectTypeFilter.Trim().Length == 0)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Find"))
            {
                var gameObjects = GameObjectUtil.FindComponentsOfType(findObjectTypeFilter.Trim());

                sceneRoots.Clear();
                foreach (var item in gameObjects)
                {
                    ClearExpanded();
                    state.ExpandedGameObjects.Add(new ReferenceChain().Add(item.Key).UniqueId);
                    if (gameObjects.Count == 1)
                    {
                        state.ExpandedComponents.Add(new ReferenceChain().Add(item.Key).Add(item.Value).UniqueId);
                    }
                    sceneRoots.Add(item.Key, true);
                    sceneTreeScrollPosition = Vector2.zero;
                    searchDisplayString = $"Showing results for GameObject.FindObjectsOfType({findObjectTypeFilter})";
                }
            }

            if (GUILayout.Button("Reset"))
            {
                ClearExpanded();
                sceneRoots = GameObjectUtil.FindSceneRoots();
                sceneTreeScrollPosition = Vector2.zero;
                searchDisplayString = string.Empty;
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public void DrawSceneTree()
        {
            sceneTreeArea.Begin();

            if (searchDisplayString != string.Empty)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label(searchDisplayString);
                GUI.contentColor = Color.white;
            }

            sceneTreeScrollPosition = GUILayout.BeginScrollView(sceneTreeScrollPosition);

            var gameObjects = sceneRoots.Keys.ToArray();

            if (ModTools.Instance.config.sceneExplorerSortAlphabetically)
            {
                try
                {
                    Array.Sort(gameObjects, (o, o1) => o?.name == null ? 1 : o1?.name == null ? -1 : o.name.CompareTo(o1.name));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            foreach (var obj in gameObjects)
            {
                GUIRecursiveTree.OnSceneTreeRecursive(gameObject, state, new ReferenceChain().Add(obj), obj);
            }

            GUILayout.EndScrollView();

            sceneTreeArea.End();
        }

        public void DrawComponent()
        {
            componentArea.Begin();

            componentScrollPosition = GUILayout.BeginScrollView(componentScrollPosition);

            if (state.CurrentRefChain != null)
            {
                try
                {
                    GUIReflect.OnSceneTreeReflect(state, state.CurrentRefChain, state.CurrentRefChain.Evaluate());
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    state.CurrentRefChain = null;
                    throw;
                }
            }

            GUILayout.EndScrollView();

            componentArea.End();
        }

        public void DrawWindow()
        {
            RecalculateAreas();

            var enterPressed = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);

            if (enterPressed)
            {
                GUI.FocusControl(null);
            }

            state.PreventCircularReferences.Clear();

            DrawHeader();
            DrawSceneTree();
            DrawComponent();
        }

        private void ClearExpanded()
        {
            state.ExpandedGameObjects.Clear();
            state.ExpandedComponents.Clear();
            state.ExpandedObjects.Clear();
            state.EvaluatedProperties.Clear();
            state.SelectedArrayStartIndices.Clear();
            state.SelectedArrayEndIndices.Clear();
            searchDisplayString = string.Empty;
            sceneTreeScrollPosition = Vector2.zero;
            state.CurrentRefChain = null;
            TypeUtil.ClearTypeCache();
        }
    }
}