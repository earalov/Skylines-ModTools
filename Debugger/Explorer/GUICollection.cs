﻿using System;
using System.Collections;
using UnityEngine;

namespace ModTools.Explorer
{
    internal static class GUICollection
    {
        public static void OnSceneTreeReflectICollection(SceneExplorerState state, ReferenceChain refChain, object myProperty)
        {
            if (!SceneExplorerCommon.SceneTreeCheckDepth(refChain))
            {
                return;
            }

            if (!(myProperty is ICollection collection))
            {
                return;
            }

            var oldRefChain = refChain;
            var collectionSize = collection.Count;
            if (collectionSize == 0)
            {
                GUILayout.BeginHorizontal();
                GUI.contentColor = Color.yellow;
                GUILayout.Label("Collection is empty!");
                GUI.contentColor = Color.white;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                return;
            }

            var collectionItemType = collection.GetType().GetElementType();
            var flagsField = collectionItemType?.GetField("m_flags");
            var flagIsEnum = flagsField?.FieldType.IsEnum == true && Type.GetTypeCode(flagsField.FieldType) == TypeCode.Int32;

            GUICollectionNavigation.SetUpCollectionNavigation("Collection", state, refChain, oldRefChain, collectionSize, out var arrayStart, out var arrayEnd);
            var count = 0;
            foreach (var value in collection)
            {
                if (count < arrayStart)
                {
                    count++;
                    continue;
                }

                refChain = oldRefChain.Add(count);

                GUILayout.BeginHorizontal();
                SceneExplorerCommon.InsertIndent(refChain.Ident);

                var isNullOrEmpty = value == null || flagIsEnum && Convert.ToInt32(flagsField.GetValue(value)) == 0;

                var type = value?.GetType() ?? collectionItemType;
                if (type != null)
                {
                    if (!isNullOrEmpty)
                    {
                        GUIExpander.ExpanderControls(state, refChain, type);
                    }

                    GUI.contentColor = ModTools.Instance.Config.TypeColor;

                    GUILayout.Label(type.ToString() + " ");
                }

                GUI.contentColor = ModTools.Instance.Config.NameColor;

                GUILayout.Label($"{oldRefChain.LastItemName}.[{count}]");

                GUI.contentColor = Color.white;

                GUILayout.Label(" = ");

                GUI.contentColor = ModTools.Instance.Config.ValueColor;
                GUILayout.Label(value == null ? "null" : isNullOrEmpty ? "empty" : value.ToString());

                GUI.contentColor = Color.white;

                GUILayout.FlexibleSpace();

                if (!isNullOrEmpty)
                {
                    GUIButtons.SetupButtons(refChain, type, value, count);
                }

                GUILayout.EndHorizontal();

                if (!isNullOrEmpty && !TypeUtil.IsSpecialType(type) && state.ExpandedObjects.Contains(refChain.UniqueId))
                {
                    if (value is GameObject go)
                    {
                        foreach (var component in go.GetComponents<Component>())
                        {
                            GUIComponent.OnSceneTreeComponent(state, refChain, component);
                        }
                    }
                    else if (value is Transform transforms)
                    {
                        GUITransform.OnSceneTreeReflectUnityEngineTransform(refChain, transforms);
                    }
                    else
                    {
                        GUIReflect.OnSceneTreeReflect(state, refChain, value);
                    }
                }

                count++;
                if (count > arrayEnd)
                {
                    break;
                }
            }
        }
    }
}