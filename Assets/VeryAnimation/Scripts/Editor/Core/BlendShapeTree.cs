﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace VeryAnimation
{
    [Serializable]
    public class BlendShapeTree
    {
        private VeryAnimationWindow vaw { get { return VeryAnimationWindow.instance; } }
        private VeryAnimation va { get { return VeryAnimation.instance; } }
        private VeryAnimationEditorWindow vae { get { return VeryAnimationEditorWindow.instance; } }

        [System.Diagnostics.DebuggerDisplay("{blendShapeName}")]
        private class BlendShapeInfo
        {
            public string blendShapeName;
        }
        private class BlendShapeNode
        {
            public string name;
            public bool foldout;
            public BlendShapeInfo[] infoList;
        }
        private class BlendShapeRootNode : BlendShapeNode
        {
            public SkinnedMeshRenderer renderer;
            public Mesh mesh;
            public string[] blendShapeNames;
        }
        private List<BlendShapeRootNode> blendShapeNodes;
        private Dictionary<BlendShapeNode, int> blendShapeGroupTreeTable;

        [SerializeField]
        private float[] blendShapeGroupValues;

        public BlendShapeTree()
        {
            if (vaw == null || vaw.gameObject == null)
                return;

            #region BlendShapeNode
            {
                blendShapeNodes = new List<BlendShapeRootNode>();
                foreach (var renderer in vaw.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (renderer.sharedMesh == null) continue;
                    if (renderer.sharedMesh.blendShapeCount <= 0) continue;
                    BlendShapeRootNode root = new BlendShapeRootNode()
                    {
                        renderer = renderer,
                        mesh = renderer.sharedMesh,
                        name = renderer.gameObject.name,
                        infoList = new BlendShapeInfo[renderer.sharedMesh.blendShapeCount],
                    };
                    root.blendShapeNames = new string[renderer.sharedMesh.blendShapeCount + 1];
                    root.blendShapeNames[0] = "[none]";
                    for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
                    {
                        root.infoList[i] = new BlendShapeInfo()
                        {
                            blendShapeName = renderer.sharedMesh.GetBlendShapeName(i),
                        };
                        root.blendShapeNames[i + 1] = renderer.sharedMesh.GetBlendShapeName(i);
                    }
                    blendShapeNodes.Add(root);
                }

                {
                    blendShapeGroupTreeTable = new Dictionary<BlendShapeNode, int>();
                    int counter = 0;
                    Action<BlendShapeNode> AddTable = null;
                    AddTable = (mg) =>
                    {
                        blendShapeGroupTreeTable.Add(mg, counter++);
                    };
                    foreach (var node in blendShapeNodes)
                    {
                        AddTable(node);
                    }

                    blendShapeGroupValues = new float[blendShapeGroupTreeTable.Count];
                }
            }
            #endregion
        }
        
        public bool IsHaveBlendShapeNodes()
        {
            return blendShapeNodes != null  && blendShapeNodes.Count > 0;
        }

        public void BlendShapeTreeGUI()
        {
            var e = Event.current;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                const int IndentWidth = 15;

                #region GetBlendShapeLevel
                Func<BlendShapeNode, int, int> GetBlendShapeLevel = null;
                GetBlendShapeLevel = (mg, level) =>
                {
                    if (mg.foldout)
                    {
                        if (mg.infoList != null && mg.infoList.Length > 0)
                        {
                            level++;
                        }
                    }
                    return level;
                };
                #endregion
                #region SetBlendShapeFoldout
                Action<BlendShapeNode, bool> SetBlendShapeFoldout = null;
                SetBlendShapeFoldout = (mg, foldout) =>
                {
                    mg.foldout = foldout;
                };
                #endregion

                var mgRoot = blendShapeNodes;

                #region Reset All
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Select All", GUILayout.Width(100)))
                    {
                        if (va.IsKeyControl(e) || e.shift)
                        {
                            var combineGoList = new List<GameObject>(va.selectionGameObjects);
                            var combineBindings = new List<EditorCurveBinding>(va.uAw.GetCurveSelection());
                            foreach (var root in mgRoot)
                            {
                                if (root.renderer != null && root.renderer.gameObject != null)
                                {
                                    combineGoList.Add(root.renderer.gameObject);
                                }
                                if (root.infoList != null && root.infoList.Length > 0)
                                {
                                    foreach (var info in root.infoList)
                                        combineBindings.Add(va.AnimationCurveBindingBlendShape(root.renderer, info.blendShapeName));
                                }
                            }
                            va.SelectGameObjects(combineGoList.ToArray());
                            va.SetAnimationWindowSynchroSelection(combineBindings.ToArray());
                        }
                        else
                        {
                            var combineGoList = new List<GameObject>();
                            var combineBindings = new List<EditorCurveBinding>();
                            foreach (var root in mgRoot)
                            {
                                if (root.renderer != null && root.renderer.gameObject != null)
                                {
                                    combineGoList.Add(root.renderer.gameObject);
                                }
                                if (root.infoList != null && root.infoList.Length > 0)
                                {
                                    foreach (var info in root.infoList)
                                        combineBindings.Add(va.AnimationCurveBindingBlendShape(root.renderer, info.blendShapeName));
                                }
                            }
                            va.SelectGameObjects(combineGoList.ToArray());
                            va.SetAnimationWindowSynchroSelection(combineBindings.ToArray());
                        }
                    }
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Reset All", GUILayout.Width(100)))
                    {
                        Undo.RecordObject(vae, "Reset All BlendShape Group");
                        for (int i = 0; i < blendShapeGroupValues.Length; i++)
                            blendShapeGroupValues[i] = 0f;
                        foreach (var root in mgRoot)
                        {
                            if (root.infoList != null && root.infoList.Length > 0)
                            {
                                foreach (var info in root.infoList)
                                {
                                    va.SetAnimationValueBlendShapeIfNotOriginal(root.renderer, info.blendShapeName, va.blendShapeWeightSave.GetDefaultWeight(root.renderer, info.blendShapeName));
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                #endregion

                EditorGUILayout.Space();

                #region BlendShape
                BlendShapeRootNode rootNode = null;
                int RowCount = 0;
                Action<BlendShapeNode> BlendShapeTreeGUI = null;
                BlendShapeTreeGUI = (mg) =>
                {
                    const int FoldoutWidth = 22;
                    const int FoldoutSpace = 17;
                    EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                    {
                        var rect = EditorGUILayout.GetControlRect();
                        {
                            var r = rect;
                            r.width = FoldoutWidth;
                            EditorGUI.BeginChangeCheck();
                            mg.foldout = EditorGUI.Foldout(r, mg.foldout, "", true);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (e.alt)
                                    SetBlendShapeFoldout(mg, mg.foldout);
                            }
                        }
                        {
                            var r = rect;
                            r.x += FoldoutWidth;
                            r.y += 1;
                            r.width -= FoldoutWidth;
                            r.height += 1;
                            if (GUI.Button(r, new GUIContent(mg.name, blendShapeGroupValues[blendShapeGroupTreeTable[mg]].ToString())))
                            {
                                if (va.IsKeyControl(e) || e.shift)
                                {
                                    var combineGoList = new List<GameObject>(va.selectionGameObjects);
                                    var combineBindings = new List<EditorCurveBinding>(va.uAw.GetCurveSelection());
                                    if (rootNode.renderer != null && rootNode.renderer.gameObject != null)
                                    {
                                        combineGoList.Add(rootNode.renderer.gameObject);
                                    }
                                    if (rootNode.infoList != null && rootNode.infoList.Length > 0)
                                    {
                                        foreach (var info in rootNode.infoList)
                                            combineBindings.Add(va.AnimationCurveBindingBlendShape(rootNode.renderer, info.blendShapeName));
                                    }
                                    va.SelectGameObjects(combineGoList.ToArray());
                                    va.SetAnimationWindowSynchroSelection(combineBindings.ToArray());
                                }
                                else
                                {
                                    var combineGoList = new List<GameObject>();
                                    var combineBindings = new List<EditorCurveBinding>();
                                    if (rootNode.renderer != null && rootNode.renderer.gameObject != null)
                                    {
                                        combineGoList.Add(rootNode.renderer.gameObject);
                                    }
                                    if (rootNode.infoList != null && rootNode.infoList.Length > 0)
                                    {
                                        foreach (var info in rootNode.infoList)
                                            combineBindings.Add(va.AnimationCurveBindingBlendShape(rootNode.renderer, info.blendShapeName));
                                    }
                                    va.SelectGameObjects(combineGoList.ToArray());
                                    va.SetAnimationWindowSynchroSelection(combineBindings.ToArray());
                                }
                            }
                        }
                    }
                    GUILayout.Space(FoldoutSpace);
                    {
                        EditorGUI.BeginChangeCheck();
                        var value = GUILayout.HorizontalSlider(blendShapeGroupValues[blendShapeGroupTreeTable[mg]], 0f, 100f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(vae, "Change BlendShape Group");
                            blendShapeGroupValues[blendShapeGroupTreeTable[mg]] = value;
                            if (mg.infoList != null && mg.infoList.Length > 0)
                            {
                                foreach (var info in mg.infoList)
                                {
                                    va.SetAnimationValueBlendShape(rootNode.renderer, info.blendShapeName, value);
                                }
                            }
                        }
                    }
                    GUILayout.Space(IndentWidth * GetBlendShapeLevel(mg, 0));
                    if (GUILayout.Button("Reset", GUILayout.Width(44)))
                    {
                        Undo.RecordObject(vae, "Reset BlendShape Group");
                        blendShapeGroupValues[blendShapeGroupTreeTable[mg]] = 0f;
                        if (mg.infoList != null && mg.infoList.Length > 0)
                        {
                            foreach (var info in mg.infoList)
                            {
                                va.SetAnimationValueBlendShapeIfNotOriginal(rootNode.renderer, info.blendShapeName, va.blendShapeWeightSave.GetDefaultWeight(rootNode.renderer, info.blendShapeName));
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    if (mg.foldout)
                    {
                        EditorGUI.indentLevel++;
                        if (mg.infoList != null && mg.infoList.Length > 0)
                        {
                            #region BlendShape
                            foreach (var info in mg.infoList)
                            {
                                var blendShapeValue = va.GetAnimationValueBlendShape(rootNode.renderer, info.blendShapeName);
                                EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                EditorGUI.indentLevel++;
                                {
                                    var rect = EditorGUILayout.GetControlRect();
                                    {
                                        var offset = IndentWidth * EditorGUI.indentLevel + FoldoutWidth - IndentWidth;
                                        rect.x += offset;
                                        rect.width -= offset;
                                        rect.y += 1;
                                        rect.height += 1;
                                    }
                                    if (GUI.Button(rect, new GUIContent(info.blendShapeName)))
                                    {
                                        if (va.IsKeyControl(e) || e.shift)
                                        {
                                            var combineGoList = new List<GameObject>(va.selectionGameObjects);
                                            var combineBindings = new List<EditorCurveBinding>(va.uAw.GetCurveSelection());
                                            if (rootNode.renderer != null && rootNode.renderer.gameObject != null)
                                            {
                                                combineGoList.Add(rootNode.renderer.gameObject);
                                            }
                                            combineBindings.Add(va.AnimationCurveBindingBlendShape(rootNode.renderer, info.blendShapeName));
                                            va.SelectGameObjects(combineGoList.ToArray());
                                            va.SetAnimationWindowSynchroSelection(combineBindings.ToArray());
                                        }
                                        else
                                        {
                                            if (rootNode.renderer != null && rootNode.renderer.gameObject != null)
                                                va.SelectGameObject(rootNode.renderer.gameObject);
                                            va.SetAnimationWindowSynchroSelection(new EditorCurveBinding[] { va.AnimationCurveBindingBlendShape(rootNode.renderer, info.blendShapeName) });
                                        }
                                    }
                                }
                                {
                                    var mirrorName = va.GetMirrorBlendShape(rootNode.renderer, info.blendShapeName);
                                    if (vae.blendShapeMirrorName)
                                    {
                                        var rect = EditorGUILayout.GetControlRect();
                                        rect.y += 2;
                                        {
                                            var mirrorIndex = EditorCommon.ArrayIndexOf(rootNode.blendShapeNames, mirrorName);
                                            EditorGUI.BeginChangeCheck();
                                            mirrorIndex = EditorGUI.Popup(rect, mirrorIndex, rootNode.blendShapeNames);
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                string newMirrorName = mirrorIndex > 0 ? rootNode.blendShapeNames[mirrorIndex] : null;
                                                if (info.blendShapeName == newMirrorName)
                                                    newMirrorName = null;
                                                va.ChangeBlendShapeMirror(rootNode.renderer, info.blendShapeName, newMirrorName);
                                                if (!string.IsNullOrEmpty(newMirrorName))
                                                    va.ChangeBlendShapeMirror(rootNode.renderer, newMirrorName, info.blendShapeName);
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(mirrorName))
                                        {
                                            var mirrorTex = vaw.guiStyleMirrorButton.normal.background;
                                            rect.width = mirrorTex.width;
                                            rect.height = mirrorTex.height;
                                            rect.x += 15f;
                                            if (GUI.Button(rect, new GUIContent("", string.Format("Mirror: '{0}'", mirrorName)), vaw.guiStyleMirrorButton))
                                            {
                                                if (rootNode.renderer != null && rootNode.renderer.gameObject != null)
                                                    va.SelectGameObject(rootNode.renderer.gameObject);
                                                va.SetAnimationWindowSynchroSelection(new EditorCurveBinding[] { va.AnimationCurveBindingBlendShape(rootNode.renderer, mirrorName) });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(mirrorName))
                                        {
                                            var mirrorTex = vaw.guiStyleMirrorButton.normal.background;
                                            if (GUILayout.Button(new GUIContent("", string.Format("Mirror: '{0}'", mirrorName)), vaw.guiStyleMirrorButton, GUILayout.Width(mirrorTex.width), GUILayout.Height(mirrorTex.height)))
                                            {
                                                if (rootNode.renderer != null && rootNode.renderer.gameObject != null)
                                                    va.SelectGameObject(rootNode.renderer.gameObject);
                                                va.SetAnimationWindowSynchroSelection(new EditorCurveBinding[] { va.AnimationCurveBindingBlendShape(rootNode.renderer, mirrorName) });
                                            }
                                        }
                                        else
                                        {
                                            GUILayout.Space(FoldoutSpace);
                                        }
                                    }
                                }
                                {
                                    EditorGUI.BeginChangeCheck();
                                    var value2 = GUILayout.HorizontalSlider(blendShapeValue, 0f, 100f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        va.SetAnimationValueBlendShape(rootNode.renderer, info.blendShapeName, value2);
                                    }
                                }
                                if (GUILayout.Button("Reset", GUILayout.Width(44)))
                                {
                                    va.SetAnimationValueBlendShapeIfNotOriginal(rootNode.renderer, info.blendShapeName, va.blendShapeWeightSave.GetDefaultWeight(rootNode.renderer, info.blendShapeName));
                                }
                                EditorGUI.indentLevel--;
                                EditorGUILayout.EndHorizontal();
                            }
                            #endregion
                        }
                        EditorGUI.indentLevel--;
                    }
                };
                foreach (var root in mgRoot)
                {
                    if (root.renderer != null && root.mesh != null && root.renderer.sharedMesh == root.mesh)
                    {
                        rootNode = root;
                        BlendShapeTreeGUI(root);
                    }
                }
                #endregion
            }
            EditorGUILayout.EndVertical();
        }
    }
}
