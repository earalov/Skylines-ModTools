﻿using UnityEngine;

namespace ModTools.Explorer
{
    public static class GUICollectionNavigation
    {
        public static void SetUpCollectionNavigation(string collectionLabel, SceneExplorerState state, ReferenceChain refChain, ReferenceChain oldRefChain, int collectionSize, out int arrayStart,
    out int arrayEnd)
        {
            GUILayout.BeginHorizontal();
            SceneExplorerCommon.InsertIndent(refChain.Ident);

            GUILayout.Label($"{collectionLabel} size: {collectionSize}");

            if (!state.SelectedArrayStartIndices.TryGetValue(refChain.UniqueId, out arrayStart))
            {
                state.SelectedArrayStartIndices.Add(refChain.UniqueId, 0);
            }

            if (!state.SelectedArrayEndIndices.TryGetValue(refChain.UniqueId, out arrayEnd))
            {
                state.SelectedArrayEndIndices.Add(refChain.UniqueId, 32);
                arrayEnd = 32;
            }

            GUIControls.IntField($"{oldRefChain}.arrayStart", "Start index", ref arrayStart, 0.0f, true, true);
            GUIControls.IntField($"{oldRefChain}.arrayEnd", "End index", ref arrayEnd, 0.0f, true, true);
            GUILayout.Label("(32 items max)");
            var pageSize = Mathf.Clamp(arrayEnd - arrayStart + 1, 1, Mathf.Min(32, collectionSize - arrayStart, arrayEnd + 1));
            if (GUILayout.Button("◄", GUILayout.ExpandWidth(false)))
            {
                arrayStart -= pageSize;
                arrayEnd -= pageSize;
            }
            if (GUILayout.Button("►", GUILayout.ExpandWidth(false)))
            {
                arrayStart += pageSize;
                arrayEnd += pageSize;
            }
            arrayStart = Mathf.Clamp(arrayStart, 0, collectionSize - pageSize);
            arrayEnd = Mathf.Clamp(arrayEnd, pageSize - 1, collectionSize - 1);
            if (arrayStart > arrayEnd)
            {
                arrayEnd = arrayStart;
            }

            if (arrayEnd - arrayStart > 32)
            {
                arrayEnd = arrayStart + 32;
                arrayEnd = Mathf.Clamp(arrayEnd, 32, collectionSize - 1);
            }
            state.SelectedArrayStartIndices[refChain.UniqueId] = arrayStart;
            state.SelectedArrayEndIndices[refChain.UniqueId] = arrayEnd;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}