using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ModTools.UI;
using ModTools.Utils;
using UnityEngine;

namespace ModTools.Explorer
{
    internal sealed class SceneExplorer : GUIWindow, IGameObject, IAwakingObject
    {
        private const float WindowTopMargin = 16.0f;
        private const float WindowBottomMargin = 8.0f;

        private const float HeaderHeightCompact = 1.65f;
        private const float HeaderHeightExpanded = 13.75f;

        private const float SceneTreeWidth = 320.0f;

        private const string TitleBase = "Scene Explorer";
        private const string ExpandDownButtonText = " ▼ ▼ ▼ ";
        private const string CollapseUpButtonText = " ▲ ▲ ▲ ";
        private const string ExpandRightButtonText = " ▶▶▶ ";
        private const string CollapseLeftButtonText = " ◀◀◀ ";

        private static readonly Queue<ShowRequest> ShowRequests = new Queue<ShowRequest>();
        private readonly GUIArea headerArea;
        private readonly GUIArea sceneTreeArea;
        private readonly GUIArea componentArea;

        private Dictionary<GameObject, bool> sceneRoots = new Dictionary<GameObject, bool>();

        private string findGameObjectFilter = string.Empty;
        private string findObjectTypeFilter = string.Empty;
        private string searchDisplayString = string.Empty;

        private Vector2 sceneTreeScrollPosition = Vector2.zero;
        private Vector2 componentScrollPosition = Vector2.zero;
        private SceneExplorerState state;

        private bool headerExpanded;
        private bool treeExpanded = true;

        private string quickFilter = string.Empty;

        public SceneExplorer()
            : base(TitleBase, new Rect(128, 440, 800, 500))
        {
            headerArea = new GUIArea(this)
                .ChangeSizeRelative(height: 0)
                .OffsetBy(vertical: WindowTopMargin);

            sceneTreeArea = new GUIArea(this)
                .ChangeSizeRelative(width: 0)
                .ChangeSizeBy(width: SceneTreeWidth);

            componentArea = new GUIArea(this)
                .OffsetBy(horizontal: SceneTreeWidth)
                .ChangeSizeBy(width: -SceneTreeWidth);

            state = new SceneExplorerState();

            RecalculateAreas();
        }

        public void Awake() => Plopper.Reset();

        public void Update()
        {
            try
            {
                var refChain = state.CurrentRefChain;
                if (refChain?.Length > 0)
                {
                    var typeName = refChain.Evaluate().GetType().Name;
                    Title = $"{TitleBase} - {typeName} \"{refChain}\"";
                }
                else
                {
                    Title = TitleBase;
                }
            }
            catch
            {
                ClearExpanded();
                RefreshSceneRoots();
                ClearHistory();
            }

            Plopper.Update();
        }

        public new void OnGUI()
        {
            base.OnGUI();
            MainWindow.Instance?.SceneExplorer?.RecalculateAreas();
        }

        public void RecalculateAreas()
        {
            if (!treeExpanded || WindowRect.width < Screen.width / 4)
            {
                sceneTreeArea.ChangeSizeBy(width: 0);

                componentArea
                    .OffsetBy(horizontal: 0)
                    .ChangeSizeBy(width: 0f);
            }
            else
            {
                sceneTreeArea.ChangeSizeBy(width: SceneTreeWidth);

                componentArea
                    .OffsetBy(horizontal: SceneTreeWidth)
                    .ChangeSizeBy(width: -SceneTreeWidth);
            }

            var headerHeight = headerExpanded ? HeaderHeightExpanded : HeaderHeightCompact;
            headerHeight *= MainWindow.Instance.Config.FontSize;
            headerHeight += 32.0f;

            var verticalOffset = headerHeight - WindowTopMargin;
            var verticalSizeOffset = -(verticalOffset + WindowBottomMargin);

            headerArea.ChangeSizeBy(height: verticalOffset);

            sceneTreeArea
                .OffsetBy(vertical: verticalOffset)
                .ChangeSizeBy(height: verticalSizeOffset);

            componentArea
                .OffsetBy(vertical: verticalOffset)
                .ChangeSizeBy(height: verticalSizeOffset);
        }

        public void RefreshSceneRoots() => sceneRoots = GameObjectUtil.FindSceneRoots();


        private void BackToPreviousRefChain()
        {
            var chain = state.RefChainHistory.Last();
            state.RefChainHistory.RemoveAt(state.RefChainHistory.Count - 1);
            Show(chain, false, addChainToHistory: false);
        }

        public void Show(ReferenceChain refChain, bool hideUnrelatedSceneRoots = true, string updatedSearchString = "", bool addChainToHistory = true)
        {
            if (refChain == null)
            {
                Logger.Error("SceneExplorer: Show(): Null refChain");
                return;
            }

            if (refChain.Length == 0)
            {
                Logger.Error("SceneExplorer: Show(): Invalid refChain, expected Length >= 0");
                return;
            }

            if (refChain.FirstItemType != ReferenceChain.ReferenceType.GameObject)
            {
                Logger.Error($"SceneExplorer: Show(): invalid chain type for element [0] - expected {ReferenceChain.ReferenceType.GameObject}, got {refChain.FirstItemType}");
                return;
            }

            EnqueueShowRefChainRequest(new List<ReferenceChain> { refChain }, hideUnrelatedSceneRoots, updatedSearchString, addChainToHistory);

            Visible = true;
        }

        public void DrawHeader()
        {
            headerArea.Begin();

            if (headerExpanded)
            {
                DrawExpandedHeader();
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(treeExpanded ? CollapseLeftButtonText : ExpandRightButtonText, GUILayout.ExpandWidth(false)))
            {
                treeExpanded = !treeExpanded;
                RecalculateAreas();
            }

            GUILayout.Space(MainWindow.Instance.Config.TreeIdentSpacing);

            if (GUILayout.Button(headerExpanded ? CollapseUpButtonText : ExpandDownButtonText))
            {
                headerExpanded = !headerExpanded;
                RecalculateAreas();
            }

            GUILayout.EndHorizontal();

            headerArea.End();
        }

        public void DrawExpandedHeader()
        {
            GUILayout.BeginHorizontal();

            GUI.contentColor = Color.green;
            GUILayout.Label("Show:", GUILayout.ExpandWidth(false));

            var configChanged = false;

            var showFields = GUILayout.Toggle(MainWindow.Instance.Config.ShowFields, " Fields");
            if (MainWindow.Instance.Config.ShowFields != showFields)
            {
                MainWindow.Instance.Config.ShowFields = showFields;
                configChanged = true;
            }

            GUILayout.Space(MainWindow.Instance.Config.TreeIdentSpacing);
            var showConsts = GUILayout.Toggle(MainWindow.Instance.Config.ShowConsts, " Constants");
            if (MainWindow.Instance.Config.ShowConsts != showConsts)
            {
                MainWindow.Instance.Config.ShowConsts = showConsts;
                configChanged = true;
            }

            GUILayout.Space(MainWindow.Instance.Config.TreeIdentSpacing);
            var showProperties = GUILayout.Toggle(MainWindow.Instance.Config.ShowProperties, " Properties");
            if (MainWindow.Instance.Config.ShowProperties != showProperties)
            {
                MainWindow.Instance.Config.ShowProperties = showProperties;
                configChanged = true;
            }

            GUILayout.Space(MainWindow.Instance.Config.TreeIdentSpacing);
            var showMethods = GUILayout.Toggle(MainWindow.Instance.Config.ShowMethods, " Methods");
            if (MainWindow.Instance.Config.ShowMethods != showMethods)
            {
                MainWindow.Instance.Config.ShowMethods = showMethods;
                configChanged = true;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            var showModifiers = GUILayout.Toggle(MainWindow.Instance.Config.ShowModifiers, " Show field / property modifiers");
            if (showModifiers != MainWindow.Instance.Config.ShowModifiers)
            {
                MainWindow.Instance.Config.ShowModifiers = showModifiers;
                configChanged = true;
            }

            var showInheritedMembers = GUILayout.Toggle(MainWindow.Instance.Config.ShowInheritedMembers, " Show inherited members");
            if (showInheritedMembers != MainWindow.Instance.Config.ShowInheritedMembers)
            {
                MainWindow.Instance.Config.ShowInheritedMembers = showInheritedMembers;
                configChanged = true;
                TypeUtil.ClearTypeCache();
            }

            var evaluatePropertiesAutomatically = GUILayout.Toggle(MainWindow.Instance.Config.EvaluateProperties, " Evaluate properties automatically");
            if (evaluatePropertiesAutomatically != MainWindow.Instance.Config.EvaluateProperties)
            {
                MainWindow.Instance.Config.EvaluateProperties = evaluatePropertiesAutomatically;
                configChanged = true;
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            var sortAlphabetically = GUILayout.Toggle(MainWindow.Instance.Config.SortItemsAlphabetically, " Sort alphabetically");
            if (sortAlphabetically != MainWindow.Instance.Config.SortItemsAlphabetically)
            {
                MainWindow.Instance.Config.SortItemsAlphabetically = sortAlphabetically;
                configChanged = true;
            }

            var highlightHoveredMember = GUILayout.Toggle(MainWindow.Instance.Config.HighlightHoveredMember, " Highlight hovered member");
            if (highlightHoveredMember != MainWindow.Instance.Config.HighlightHoveredMember)
            {
                MainWindow.Instance.Config.HighlightHoveredMember = highlightHoveredMember;
                configChanged = true;
            }

            var submitNumbersOnEnter = GUILayout.Toggle(MainWindow.Instance.Config.SubmitNumbersOnEnter, " Submit numbers on enter");
            if (submitNumbersOnEnter != MainWindow.Instance.Config.SubmitNumbersOnEnter)
            {
                MainWindow.Instance.Config.SubmitNumbersOnEnter = submitNumbersOnEnter;
                configChanged = true;
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();


            if (configChanged)
            {
                MainWindow.Instance.SaveConfig();
            }

            GUI.contentColor = Color.white;
            DrawFindGameObjectPanel();
        }

        public void DrawSceneTree()
        {
            var gameObjects = sceneRoots.Keys.ToArray();

            if (MainWindow.Instance.Config.SortItemsAlphabetically)
            {
                Array.Sort(gameObjects, (x, y) => string.CompareOrdinal(x?.name, y?.name));
            }

            if (!sceneTreeArea.Begin())
            {
                return;
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Refresh", GUILayout.ExpandWidth(false)))
            {
                RefreshSceneRoots();
            }

            if (GUILayout.Button("Fold all / Clear", GUILayout.ExpandWidth(false)))
            {
                ClearExpanded();
                RefreshSceneRoots();
                ClearHistory();
            }

            GUILayout.EndHorizontal();

            if (ToolManager.exists && ToolManager.instance?.m_properties?.CurrentTool != null)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Show current tool", GUILayout.ExpandWidth(false)))
                {
                    Show(ReferenceChainBuilder.ForCurrentTool());
                }

                GUILayout.EndHorizontal();
            }

            if (ToolManager.exists && ToolManager.instance?.m_properties?.m_editPrefabInfo != null)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Show edited prefab", GUILayout.ExpandWidth(false)))
                {
                    Show(ReferenceChainBuilder.ForEditPrefabInfo());
                }

                GUILayout.EndHorizontal();
            }

            if (!string.IsNullOrEmpty(searchDisplayString))
            {
                GUI.contentColor = Color.green;
                GUILayout.Label(searchDisplayString);
                GUI.contentColor = Color.white;
            }

            sceneTreeScrollPosition = GUILayout.BeginScrollView(sceneTreeScrollPosition);

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

            object value;
            try
            {
                value = state.CurrentRefChain?.Evaluate();
                if (state.CurrentRefChain?.Length > 1)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Actions", GUILayout.MinWidth(110f));
                    if (GUILayout.Button("Back") && state.RefChainHistory.Count > 0)
                    {
                        BackToPreviousRefChain();
                    }

                    if (GUILayout.Button("Up", GUILayout.ExpandWidth(false)))
                    {
                        Show(state.CurrentRefChain.SubChain(state.CurrentRefChain.Length - 1), false);
                    }

                    GUIButtons.SetupCommonButtons(state.CurrentRefChain, value, state.CurrentRefChain.LastItem is uint u ? u : 0);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                else if (state.CurrentRefChain?.LastItem is GameObject gameObject)
                {
                    if (gameObject.transform?.parent?.gameObject != null)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Actions", GUILayout.MinWidth(110f));
                        if (GUILayout.Button("Back") && state.RefChainHistory.Count > 0)
                        {
                            BackToPreviousRefChain();
                        }

                        if (GUILayout.Button("Up", GUILayout.ExpandWidth(false)))
                        {
                            Show(ReferenceChainBuilder.ForGameObject(gameObject.transform?.parent?.gameObject));
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                value = null;
            }

            DrawQuickFilter();

            componentScrollPosition = GUILayout.BeginScrollView(componentScrollPosition);

            if (value != null)
            {
                try
                {
                    GUIReflect.OnSceneTreeReflect(state, state.CurrentRefChain, value, false, TypeUtil.SmartType.Undefined, quickFilter);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    state.CurrentRefChain = null;
                }
            }

            GUILayout.EndScrollView();

            componentArea.End();
        }

        public void ClearHistory()
        {
            state.RefChainHistory = new List<ReferenceChain>();
        }

        public void ClearExpanded()
        {
            state.ExpandedGameObjects.Clear();
            state.ExpandedObjects.Clear();
            state.EvaluatedProperties.Clear();
            state.SelectedArrayStartIndices.Clear();
            state.SelectedArrayEndIndices.Clear();
            searchDisplayString = string.Empty;
            quickFilter = string.Empty;
            sceneTreeScrollPosition = Vector2.zero;
            state.CurrentRefChain = null;
        }

        protected override void HandleException(Exception ex)
        {
            Debug.LogException(ex);
            state = new SceneExplorerState();
            RefreshSceneRoots();
        }

        protected override void DrawWindow()
        {
            RecalculateAreas();

            var enterPressed = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);

            if (enterPressed && !GUIControls.EditingMultiLineField)
            {
                GUI.FocusControl(null);
            }

            GUIControls.EditingMultiLineField = false; // consume

            state.PreventCircularReferences.Clear();

            lock (ShowRequests)
            {
                while (ShowRequests.Count > 0)
                {
                    ProcessShowRequest(ShowRequests.Dequeue());
                }
            }

            DrawHeader();
            DrawSceneTree();
            DrawComponent();
        }

        private static void EnqueueShowRefChainRequest(List<ReferenceChain> refChains, bool hideUnrelatedSceneRoots, string updatedSearchString, bool addChainToHistory)
        {
            ShowRequests.Enqueue(new ShowRequest()
            {
                ReferenceChains = refChains,
                HideUnrelatedSceneRoots = hideUnrelatedSceneRoots,
                UpdatedSearchString = updatedSearchString,
                AddChainToHistory = addChainToHistory,
            });
        }

        private void ProcessShowRequest(ShowRequest request)
        {
            if (state.CurrentRefChain != null && request.AddChainToHistory)
            {
                state.RefChainHistory.Add(state.CurrentRefChain);
            }

            if (request.HideUnrelatedSceneRoots)
            {
                sceneTreeScrollPosition = Vector2.zero;
                sceneRoots.Clear();
                ClearExpanded();
            }

            searchDisplayString = request.UpdatedSearchString;
            ReferenceChain currentRefChain = null;
            var singleRefChain = request.ReferenceChains.Count == 1;
            foreach (var refChain in request.ReferenceChains)
            {
                var rootGameObject = (GameObject)refChain.GetChainItem(0);
                if (!sceneRoots.ContainsKey(rootGameObject))
                {
                    sceneRoots.Add(rootGameObject, true);
                }

                var expandedRefChain = ReferenceChainBuilder.ForGameObject(rootGameObject);
                if (!state.ExpandedGameObjects.Contains(expandedRefChain.UniqueId))
                {
                    state.ExpandedGameObjects.Add(expandedRefChain.UniqueId);
                }

                if (!singleRefChain)
                {
                    continue;
                }

                for (var i = 1; i < refChain.Length; i++)
                {
                    var breakLoop = false;
                    switch (refChain.GetChainItemType(i))
                    {
                        case ReferenceChain.ReferenceType.GameObject:
                            var go = (GameObject)refChain.GetChainItem(i);
                            expandedRefChain = expandedRefChain.Add(go);
                            if (!state.ExpandedGameObjects.Contains(expandedRefChain.UniqueId))
                            {
                                state.ExpandedGameObjects.Add(expandedRefChain.UniqueId);
                            }

                            break;

                        case ReferenceChain.ReferenceType.Component:
                            var component = (Component)refChain.GetChainItem(i);
                            expandedRefChain = expandedRefChain.Add(component);
                            if (currentRefChain != null)
                            {
                                breakLoop = true;
                            }

                            break;

                        case ReferenceChain.ReferenceType.Field:
                            var field = (FieldInfo)refChain.GetChainItem(i);
                            expandedRefChain = expandedRefChain.Add(field);
                            if (!state.ExpandedObjects.Contains(expandedRefChain.UniqueId))
                            {
                                state.ExpandedObjects.Add(expandedRefChain.UniqueId);
                            }

                            break;

                        case ReferenceChain.ReferenceType.Property:
                            var property = (PropertyInfo)refChain.GetChainItem(i);
                            expandedRefChain = expandedRefChain.Add(property);
                            if (!state.ExpandedObjects.Contains(expandedRefChain.UniqueId))
                            {
                                state.ExpandedObjects.Add(expandedRefChain.UniqueId);
                            }

                            break;

                        case ReferenceChain.ReferenceType.EnumerableItem:
                            var index = (uint)refChain.GetChainItem(i);
                            uint startIndex;
                            if (state.SelectedArrayStartIndices.TryGetValue(expandedRefChain.UniqueId,out startIndex)) {
                                startIndex = Math.Min(index, startIndex);
                            } else if (index > 16) {
                                    startIndex = index - 16;
                            } else {
                                startIndex = 0;
                            }

                            uint endIndex;
                            if (state.SelectedArrayEndIndices.TryGetValue(expandedRefChain.UniqueId, out endIndex)) {
                                endIndex = Math.Max(index, endIndex);
                            } else {
                                endIndex = startIndex + 31;
                            }

                            state.SelectedArrayStartIndices[expandedRefChain.UniqueId] = startIndex;
                            state.SelectedArrayEndIndices[expandedRefChain.UniqueId] = endIndex;
                            expandedRefChain = expandedRefChain.Add(index);
                            if (!state.ExpandedObjects.Contains(expandedRefChain.UniqueId))
                            {
                                state.ExpandedObjects.Add(expandedRefChain.UniqueId);
                            }

                            break;
                    }

                    if (breakLoop)
                    {
                        break;
                    }
                }

                if (currentRefChain == null)
                {
                    currentRefChain = refChain.Clone();
                    currentRefChain.IndentationOffset = refChain.Length;
                }
            }
            state.CurrentRefChain = currentRefChain;
        }

        private void DrawQuickFilter()
        {
            if (!(state.CurrentRefChain?.Length > 1))
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Quick filter", GUILayout.MinWidth(110f));
            var updatedFilter =
                GUILayout.TextField(quickFilter, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(float.MaxValue));
            if (!updatedFilter.Equals(quickFilter))
            {
                quickFilter = updatedFilter.Trim();
            }

            if (GUILayout.Button("Clear"))
            {
                quickFilter = string.Empty;
            }

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
                ClearHistory();
                var go = GameObject.Find(findGameObjectFilter.Trim());
                if (go != null)
                {
                    EnqueueShowRefChainRequest(new List<ReferenceChain> { new ReferenceChain().Add(go) }, true, $"Showing results for GameObject.Find(\"{findGameObjectFilter}\")", true);
                }
            }

            if (GUILayout.Button("Reset"))
            {
                ClearExpanded();
                ClearHistory();
                RefreshSceneRoots();
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
                EnqueueShowRefChainRequest(gameObjects.Select(item => new ReferenceChain().Add(item.Key)).ToList(), true, $"Showing results for GameObject.FindObjectsOfType({findObjectTypeFilter})", false);
            }

            if (GUILayout.Button("Reset"))
            {
                ClearExpanded();
                RefreshSceneRoots();
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private sealed class ShowRequest
        {
            public List<ReferenceChain> ReferenceChains { get; set; }

            public bool HideUnrelatedSceneRoots { get; set; }

            public string UpdatedSearchString { get; set; }
            
            public bool AddChainToHistory { get; set; }
        }
    }
}