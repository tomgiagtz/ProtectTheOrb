﻿/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Linq;
using System.Text;
using InfinityCode.uContext.Tools;
using InfinityCode.uContext.UnityTypes;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.uContext
{
    public static class GameObjectUtils
    {
        public static Action<GenericMenuEx, GameObject[]> OnPrepareGameObjectMenu;
        private static string[] typeNames;

        private static void AddGroupsToContextMenu(GameObject[] targets, GenericMenuEx menu)
        {
            bool hasGroupTag = false;

            string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
            foreach (string t in tags)
            {
                if (t == "Group")
                {
                    hasGroupTag = true;
                    break;
                }
            }

            if (!hasGroupTag) return;

            GameObject[] groups = GameObject.FindGameObjectsWithTag("Group");
            if (groups.Length == 0) return;

            Transform firstParent = targets[0].transform.parent;
            string activeGroup = (firstParent != null && firstParent.tag == "Group")? firstParent.gameObject.name: null;

            menu.Add("Groups/None", string.IsNullOrEmpty(activeGroup), () => MoveToGroup(targets, null));

            foreach (GameObject group in groups.OrderBy(g => g.name))
            {
                GameObject g = group;
                string groupName = g.name;
                menu.Add("Groups/" + groupName, groupName == activeGroup, () => MoveToGroup(targets, g));
            }
        }

        private static void AddLayersToContextMenu(GameObject[] targets, GenericMenuEx menu)
        {
            bool isMultiple = targets.Length > 1;
            string[] layers = UnityEditorInternal.InternalEditorUtility.layers;
            GameObject firstTarget = targets[0];

            foreach (string layer in layers)
            {
                bool isSameLayer = false;
                int layerIndex = LayerMask.NameToLayer(layer);
                if (firstTarget.layer == layerIndex)
                {
                    isSameLayer = true;
                    if (isMultiple)
                    {
                        for (int i = 1; i < targets.Length; i++)
                        {
                            if (firstTarget.layer != targets[i].layer)
                            {
                                isSameLayer = false;
                                break;
                            }
                        }
                    }
                }

                menu.Add("Layers/" + layer, isSameLayer, () =>
                {
                    foreach (GameObject target in targets) target.layer = layerIndex;
                });
            }

            menu.AddSeparator("Layers/");
            menu.Add("Layers/Add Layer...", false, ShowLayerProperties);
        }

        private static void AddTagsToContextMenu(GameObject[] targets, GenericMenuEx menu)
        {
            bool isMultiple = targets.Length > 1;
            GameObject firstTarget = targets[0];
            string[] tags = UnityEditorInternal.InternalEditorUtility.tags;

            menu.Add("Tags/New", false, CreateNewTag, targets);
            menu.AddSeparator("Tags/");

            for (int i = 0; i < tags.Length; i++)
            {
                string tag = tags[i];
                bool isSameTag = false;
                if (firstTarget.tag == tag)
                {
                    isSameTag = true;
                    if (isMultiple)
                    {
                        for (int j = 1; j < targets.Length; j++)
                        {
                            if (firstTarget.tag != targets[j].tag)
                            {
                                isSameTag = false;
                                break;
                            }
                        }
                    }
                }

                menu.Add("Tags/" + tag, isSameTag, () =>
                {
                    foreach (GameObject target in targets) target.tag = tag;
                });
            }
        }

        public static void Align(GameObject[] targets, int side, float xMul, float yMul, float zMul)
        {
            Bounds bounds = new Bounds();
            int count = 0;

            for (int i = 0; i < targets.Length; i++)
            {
                GameObject go1 = targets[i];
                if (go1.scene.name == null) continue;

                Renderer r1 = go1.GetComponent<Renderer>();

                if (count == 0)
                {
                    if (r1 != null) bounds = r1.bounds;
                    else bounds = new Bounds(go1.transform.position, Vector3.zero);
                }
                else
                {
                    if (r1 != null) bounds.Encapsulate(r1.bounds);
                    else bounds.Encapsulate(go1.transform.position);
                }

                count++;
            }

            if (count <= 1) return;

            Vector3 v;
            if (side == 0) v = bounds.min;
            else if (side == 1) v = bounds.center;
            else v = bounds.max;

            Undo.SetCurrentGroupName("Align GameObjects");
            int group = Undo.GetCurrentGroup();

            foreach (GameObject go in targets)
            {
                if (go.scene.name == null) continue;
                Renderer r = go.GetComponent<Renderer>();
                Undo.RecordObject(go.transform, "Align GameObjects");
                if (r != null)
                {
                    Vector3 v2;
                    if (side == 0) v2 = r.bounds.min;
                    else if (side == 1) v2 = r.bounds.center;
                    else v2 = r.bounds.max;

                    Vector3 offset = v2 - v;
                    go.transform.position -= new Vector3(offset.x * xMul, offset.y * yMul, offset.z * zMul);
                }
                else
                {
                    Vector3 offset = go.transform.position - v;
                    go.transform.position -= new Vector3(offset.x * xMul, offset.y * yMul, offset.z * zMul);
                }
            }

            Undo.CollapseUndoOperations(group);
        }

        private static bool AnyOutermostPrefabRoots(GameObject[] targets)
        {
            foreach (GameObject go in targets)
            {
                if (go != null && PrefabUtility.IsPartOfNonAssetPrefabInstance(go) && PrefabUtility.IsOutermostPrefabInstanceRoot(go))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CanOpenPrefab(GameObject target)
        {
            if (!PrefabUtility.IsPartOfAnyPrefab(target)) return false;
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(target)) return false;
            if (PrefabUtility.GetPrefabInstanceStatus(target) != PrefabInstanceStatus.Connected) return false;

            GameObject asset = PrefabUtilityRef.GetOriginalSourceOrVariantRoot(target);
            if (asset == null || PrefabUtility.IsPartOfImmutablePrefab(asset)) return false;

            return true;
        }

        private static void CreateNewTag(object data)
        {
            InputDialog.Show("Enter a new tag", "", tag =>
            {
                if (string.IsNullOrEmpty(tag)) return;

                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty tagsProp = tagManager.FindProperty("tags");

                bool found = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                    if (t.stringValue.Equals(tag))
                    {
                        found = true; 
                        break;
                    }
                }

                if (!found)
                {
                    tagsProp.InsertArrayElementAtIndex(0);
                    SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
                    n.stringValue = tag;
                }

                tagManager.ApplyModifiedProperties();

                GameObject[] targets = data as GameObject[];
                foreach (GameObject target in targets)
                {
                    target.tag = tag;
                }
            });
        }

        public static void Distribute(GameObject[] targets, float xMul, float yMul, float zMul)
        {
            Bounds bounds = new Bounds();
            int count = 0;
            Vector3 size = new Vector3();

            if (xMul > 0) targets = targets.OrderBy(t => t.transform.position.x).ToArray();
            else if (yMul > 0) targets = targets.OrderBy(t => t.transform.position.y).ToArray();
            else if (zMul > 0) targets = targets.OrderBy(t => t.transform.position.z).ToArray();

            for (int i = 0; i < targets.Length; i++)
            {
                GameObject go = targets[i];
                if (go.scene.name == null) continue;

                Renderer r = go.GetComponent<Renderer>();

                if (count == 0)
                {
                    if (r != null)
                    {
                        bounds = r.bounds;
                        size = r.bounds.size;
                    }
                    else bounds = new Bounds(go.transform.position, Vector3.zero);
                }
                else
                {
                    if (r != null)
                    {
                        bounds.Encapsulate(r.bounds);
                        size += r.bounds.size;
                    }
                    else bounds.Encapsulate(go.transform.position);
                }

                count++;
            }

            if (count <= 2) return;

            Vector3 shift = (bounds.size - size) / (count - 1);
            Vector3 nextPoint = bounds.min;

            Undo.SetCurrentGroupName("Distribute GameObjects");
            int group = Undo.GetCurrentGroup();

            foreach (GameObject go in targets)
            {
                if (go.scene.name == null) continue;
                Renderer r = go.GetComponent<Renderer>();
                Undo.RecordObject(go.transform, "Distribute GameObjects");
                if (r != null)
                {
                    Vector3 targetPoint = nextPoint + r.bounds.size / 2;
                    Vector3 offset = go.transform.position - targetPoint;
                    go.transform.position -= new Vector3(offset.x * xMul, offset.y * yMul, offset.z * zMul);
                    nextPoint += r.bounds.size;
                }
                else
                {
                    Vector3 offset = go.transform.position - nextPoint;
                    go.transform.position -= new Vector3(offset.x * xMul, offset.y * yMul, offset.z * zMul);
                }

                nextPoint += shift;
            }

            Undo.CollapseUndoOperations(group);
        }

        public static void GetPsIconContent(GUIContent content, int maxLength = 4)
        {
            content.text = GetPsIconLabel(content.tooltip, maxLength);
            content.image = null;
        }

        public static string GetPsIconLabel(string label, int maxLength = 4)
        {
            StaticStringBuilder.Clear();
            int l = 0;
            string text = Prefs.HierarchyIconRemovePrefix(label);
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (!char.IsUpper(c) && !char.IsDigit(c)) continue;
                l++;
                if (l > 1) c = char.ToLowerInvariant(c);
                StaticStringBuilder.Append(c);
            }

            if (l > maxLength) StaticStringBuilder.Length = maxLength;

            return StaticStringBuilder.GetString(true);
        }

        public static StringBuilder GetGameObjectPath(GameObject go)
        {
            return GetTransformPath(go.transform);
        }

        public static string[] GetTypesDisplayNames()
        {
            if (typeNames == null)
            {
                typeNames = new[]
                {
                    "AnimationClip",
                    "AudioClip",
                    "AudioMixer",
                    "ComputeShader",
                    "Font",
                    "GameObject",
                    "GUISkin",
                    "Material",
                    "Mesh",
                    "Model",
                    "PhysicMaterial",
                    "Scene",
                    "Script",
                    "Shader",
                    "Sprite",
                    "Texture",
                    "VideoClip"
                };
            }
            
            return typeNames;
        }

        public static T GetRoot<T>(T t) where T: Transform
        {
            while (t.parent != null && t.parent is T) t = t.parent as T;
            return t;
        }

        public static StringBuilder GetTransformPath(Transform t)
        {
            StaticStringBuilder.Clear();

            StaticStringBuilder.Append(t.name);
            while ((t = t.parent) != null)
            {
                StaticStringBuilder.Insert(0, '/');
                StaticStringBuilder.Insert(0, t.name);
            }

            return StaticStringBuilder.GetBuilder();
        }

        private static void MoveToGroup(GameObject[] targets, GameObject container)
        {
            Undo.SetCurrentGroupName("Move To Group");
            int group = Undo.GetCurrentGroup();

            Transform parent = container != null ? container.transform : null;

            foreach (GameObject target in targets) Undo.SetTransformParent(target.transform, parent, target.name);
            Undo.CollapseUndoOperations(@group);
        }

        public static void OpenPrefab(string path, GameObject gameObject = null)
        {
            PrefabStageUtilityRef.OpenPrefab(path, gameObject);
        }

        private static void SetActive(GameObject[] targets, bool value)
        {
            foreach (GameObject go in targets) go.SetActive(value);
        }

        public static void ShowContextMenu(bool restoreContextMenu, params GameObject[] targets)
        {
            if (targets.Length == 0) return;

            GenericMenuEx menu = new GenericMenuEx();

            bool isActive = targets.All(t => t.activeInHierarchy);
            menu.Add("Active", isActive, () => SetActive(targets, !isActive));
            menu.AddSeparator();

            menu.Add("Copy %c", Unsupported.CopyGameObjectsToPasteboard);
            menu.Add("Paste %v", Unsupported.PasteGameObjectsFromPasteboard);
            menu.AddSeparator();
            menu.Add("Rename _F2", () => Rename.Show(targets));
            menu.Add("Duplicate %d", () =>
            {
                Unsupported.DuplicateGameObjectsUsingPasteboard();
                if (restoreContextMenu) SceneViewManager.OnNextGUI += uContextMenu.ShowInLastPosition;
            });
            menu.Add("Delete _Del", () =>
            {
                Unsupported.DeleteGameObjectSelection();
                uContextMenu.Close();
            });
            menu.AddSeparator();

            bool isMultiple = targets.Length > 1;

            AddLayersToContextMenu(targets, menu);
            AddTagsToContextMenu(targets, menu);
            AddGroupsToContextMenu(targets, menu);

            menu.AddSeparator();

            if (!isMultiple)
            {
                GameObject firstTarget = targets[0];

                Bookmarks.InsertBookmarkMenu(menu, firstTarget);

                GameObject target = firstTarget;
                string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target);

                if (!string.IsNullOrEmpty(assetPath))
                {
                    if (PrefabUtility.IsPartOfModelPrefab(target))
                    {
                        menu.Add("Open Model", () =>
                        {
                            GameObject asset = PrefabUtilityRef.GetOriginalSourceOrVariantRoot(target);
                            AssetDatabase.OpenAsset(asset);
                            uContextMenu.Close();
                        });
                    }
                    else
                    {
                        if (CanOpenPrefab(target))
                        {
                            menu.Add("Open Prefab Asset", () =>
                            {
                                PrefabStageUtilityRef.OpenPrefab(assetPath, target);
                                uContextMenu.Close();
                            });
                        }
                    }

                    menu.Add("Select Prefab Asset", () =>
                    {
                        Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset.GetInstanceID());
                        uContextMenu.Close();
                    });
                }
            }

            if (AnyOutermostPrefabRoots(targets))
            {
                menu.Add("Unpack Prefab", false, () =>
                {
                    UnpackPrefab(PrefabUnpackMode.OutermostRoot, targets);
                    uContextMenu.Close();
                });
                menu.Add("Unpack Prefab Completely", () =>
                {
                    UnpackPrefab(PrefabUnpackMode.Completely, targets);
                    uContextMenu.Close();
                });
            }

            if (isMultiple)
            {
                menu.Add("Group %g", () => Group.GroupTargets(targets));
            }

            if (targets.Any(t => t.transform.childCount > 0)) menu.Add("Ungroup", () =>
            {
                Ungroup.UngroupTargets(targets);
                uContextMenu.Close();
            });

            if (OnPrepareGameObjectMenu != null) OnPrepareGameObjectMenu(menu, targets);

            menu.ShowAsContext();
        }

        private static void ShowLayerProperties()
        {
            SettingsService.OpenProjectSettings("Project/Tags and Layers");
        }

        private static void UnpackPrefab(PrefabUnpackMode unpackMode, GameObject[] targets)
        {
            foreach (GameObject go in targets)
            {
                if (go != null && PrefabUtility.IsPartOfNonAssetPrefabInstance(go) && PrefabUtility.IsOutermostPrefabInstanceRoot(go))
                {
                    PrefabUtility.UnpackPrefabInstance(go, unpackMode, InteractionMode.UserAction);
                }
            }
        }
    }
}