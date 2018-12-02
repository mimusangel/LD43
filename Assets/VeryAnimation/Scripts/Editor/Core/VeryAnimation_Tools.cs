using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
#if UNITY_2017_1_OR_NEWER
using UnityEngine.Timeline;
#endif

namespace VeryAnimation
{
    public partial class VeryAnimation
    {
        public enum ToolMode
        {
            Copy,
            Trim,
            CreateNewClip,
            RangeIK,
            HumanoidIK,
            ParameterRelatedCurves,
            RotationCurveInterpolation,
            KeyframeReduction,
            EnsureQuaternionContinuity,
            Cleanup,
            FixErrors,
            Export,
        }
        public ToolMode toolMode;
        public bool toolsHelp = true;
        public int toolCopy_FirstFrame;
        public int toolCopy_LastFrame;
        public int toolCopy_WriteFrame;
        public int toolTrim_FirstFrame;
        public int toolTrim_LastFrame;
        public int toolRangeIK_FirstFrame;
        public int toolRangeIK_LastFrame;
        private bool toolRangeIK_AnimatorIKFoldout = true;
        private bool toolRangeIK_OriginalIKFoldout = true;
        public bool toolHumanoidIK_Hand;
        public bool toolHumanoidIK_Foot = true;
        public int toolHumanoidIK_FirstFrame;
        public int toolHumanoidIK_LastFrame;
        public bool toolCleanup_RemoveRoot;
        public bool toolCleanup_RemoveIK;
        public bool toolCleanup_RemoveTDOF;
        public bool toolCleanup_RemoveMotion;
        public bool toolCleanup_RemoveFinger;
        public bool toolCleanup_RemoveEyes;
        public bool toolCleanup_RemoveJaw;
        public bool toolCleanup_RemoveToes;
        public bool toolCleanup_RemoveTransformPosition;
        public bool toolCleanup_RemoveTransformRotation;
        public bool toolCleanup_RemoveTransformScale;
        public bool toolCleanup_RemoveBlendShape;
        public bool toolCleanup_RemoveObjectReference;
        public bool toolCleanup_RemoveEvent;
        public bool toolCleanup_RemoveMissing = true;
        public bool toolCleanup_RemoveHumanoidConflict = true;
        public bool toolCleanup_RemoveRootMotionConflict = true;
        public bool toolCleanup_RemoveUnnecessary = true;
        public enum RotationCurveInterpolationMode
        {
            Quaternion,
            EulerAngles,
        };
        public RotationCurveInterpolationMode toolRotationInterpolation_Mode;
        public float toolKeyframeReduction_RotationError = 0.5f;
        public float toolKeyframeReduction_PositionError = 0.5f;
        public float toolKeyframeReduction_ScaleAndOthersError = 0.5f;
        public bool toolKeyframeReduction_EnableHumanoid = true;
        public bool toolKeyframeReduction_EnableGeneric = true;
        public bool toolKeyframeReduction_EnableOther = true;
        public bool toolExport_ActiveOnly = true;
        public bool toolExport_Mesh = true;
        public enum ExportAnimationMode
        {
            None,
            CurrentClip,
            AllClips,
        };
        public ExportAnimationMode toolExport_AnimationMode = ExportAnimationMode.CurrentClip;
        public bool toolExport_FootIK = true;

        [Serializable]
        public class ParameterRelatedData
        {
            public string propertyName;
            public int parameterIndex;
            public bool enableAnimationCurve;
            public bool enableAnimatorParameter;
        }
        public List<ParameterRelatedData> toolParameterRelatedCurve_DataList;
        private bool toolParameterRelatedCurve_Update;
        private ReorderableList toolParameterRelatedCurve_List;

        public void ToolsGUI()
        {
            if (currentClip == null) return;
            var clip = currentClip;
            var e = Event.current;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    var mode = (ToolMode)EditorGUILayout.EnumPopup(toolMode);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Tool Mode");
                        toolMode = mode;
                    }
                }
                {
                    if (GUILayout.Button(vaw.uEditorGUI.GetHelpIcon(), toolsHelp ? vaw.guiStyleIconActiveButton : vaw.guiStyleIconButton, GUILayout.Width(19)))
                    {
                        Undo.RecordObject(vaw, "Change Tool Help");
                        toolsHelp = !toolsHelp;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel++;
            if (toolsHelp)
            {
                EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpToolsCopy + (int)toolMode), MessageType.Info);
            }
            if (toolMode == ToolMode.Copy)
            {
                #region Copy
                if (uAnimationClipEditor != null)
                {
                    float firstFrame = toolCopy_FirstFrame;
                    float lastFrame = toolCopy_LastFrame;
                    float additivePoseframe = 0.0f;
                    bool changedStart = false;
                    bool changedStop = false;
                    bool changedAdditivePoseframe = false;
                    uAnimationClipEditor.ClipRangeGUI(ref firstFrame, ref lastFrame, out changedStart, out changedStop, false, ref additivePoseframe, out changedAdditivePoseframe);
                    if (changedStart)
                    {
                        Undo.RecordObject(vaw, "Change First Frame");
                        toolCopy_FirstFrame = Mathf.RoundToInt(firstFrame);
                    }
                    if (changedStop)
                    {
                        Undo.RecordObject(vaw, "Change Last Frame");
                        toolCopy_LastFrame = Mathf.RoundToInt(lastFrame);
                    }
                }
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Write", "Write frame of the clip."), GUILayout.Width(132));
                    {
                        EditorGUI.BeginChangeCheck();
                        var frame = EditorGUILayout.IntField(toolCopy_WriteFrame, GUILayout.Width(64));
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(vaw, "Change Write Frame");
                            toolCopy_WriteFrame = Math.Max(frame, 0);
                        }
                        if (GUILayout.Button(new GUIContent("Current", "Set current frame"), EditorStyles.miniButton, GUILayout.Width(50), GUILayout.Height(15)))
                        {
                            Undo.RecordObject(vaw, "Change Write Frame");
                            toolCopy_WriteFrame = uAw.GetCurrentFrame();
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("to", GUILayout.Width(32));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField((toolCopy_WriteFrame + (toolCopy_LastFrame - toolCopy_FirstFrame)).ToString(), GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                }
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Copy"))
                    {
                        ToolsCopy(clip);
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                }
                #endregion
            }
            else if (toolMode == ToolMode.Trim)
            {
                #region Trim
                if (uAnimationClipEditor != null)
                {
                    float firstFrame = toolTrim_FirstFrame;
                    float lastFrame = toolTrim_LastFrame;
                    float additivePoseframe = 0.0f;
                    bool changedStart = false;
                    bool changedStop = false;
                    bool changedAdditivePoseframe = false;
                    uAnimationClipEditor.ClipRangeGUI(ref firstFrame, ref lastFrame, out changedStart, out changedStop, false, ref additivePoseframe, out changedAdditivePoseframe);
                    if (changedStart)
                    {
                        Undo.RecordObject(vaw, "Change First Frame");
                        toolTrim_FirstFrame = Mathf.RoundToInt(firstFrame);
                    }
                    if (changedStop)
                    {
                        Undo.RecordObject(vaw, "Change Last Frame");
                        toolTrim_LastFrame = Mathf.RoundToInt(lastFrame);
                    }
                }
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Trim"))
                    {
                        ToolsTrim(clip);
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                }
                #endregion
            }
            else if (toolMode == ToolMode.CreateNewClip)
            {
                #region CreateNewClip
                {
                    Action<ToolsCreateNewClipMode> ActionToolsCreateNewClip = (mode) =>
                    {
                        var name = clip.name;
                        if (mode == ToolsCreateNewClipMode.Mirror)
                            name += " (mirror)";
                        var assetPath = string.Format("{0}/{1}.anim", Path.GetDirectoryName(AssetDatabase.GetAssetPath(clip)), name);
                        var uniquePath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                        string path = EditorUtility.SaveFilePanel("Create new animation clip", Path.GetDirectoryName(uniquePath), Path.GetFileName(uniquePath), "anim");
                        if (!string.IsNullOrEmpty(path))
                        {
                            if (!path.StartsWith(Application.dataPath))
                            {
                                EditorCommon.SaveInsideAssetsFolderDisplayDialog();
                            }
                            else
                            {
                                path = path.Replace(Application.dataPath, "Assets");
                                ToolsCreateNewClip(path, mode);
                            }
                        }
                    };

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Blank"))
                    {
                        ActionToolsCreateNewClip(ToolsCreateNewClipMode.Blank);
                    }
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Duplicate"))
                    {
                        ActionToolsCreateNewClip(ToolsCreateNewClipMode.Duplicate);
                    }
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Mirror"))
                    {
                        ActionToolsCreateNewClip(ToolsCreateNewClipMode.Mirror);
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                }
                #endregion
            }
            else if (toolMode == ToolMode.RangeIK)
            {
                #region RangeIK
                if (uAnimationClipEditor != null)
                {
                    float firstFrame = toolRangeIK_FirstFrame;
                    float lastFrame = toolRangeIK_LastFrame;
                    float additivePoseframe = 0.0f;
                    bool changedStart = false;
                    bool changedStop = false;
                    bool changedAdditivePoseframe = false;
                    uAnimationClipEditor.ClipRangeGUI(ref firstFrame, ref lastFrame, out changedStart, out changedStop, false, ref additivePoseframe, out changedAdditivePoseframe);
                    if (changedStart)
                    {
                        Undo.RecordObject(vaw, "Change First Frame");
                        toolRangeIK_FirstFrame = Mathf.RoundToInt(firstFrame);
                    }
                    if (changedStop)
                    {
                        Undo.RecordObject(vaw, "Change Last Frame");
                        toolRangeIK_LastFrame = Mathf.RoundToInt(lastFrame);
                    }
                }
                bool disable = false;
                {
                    var saveIndentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    int enableCount = 0;
                    #region AnimatorIK
                    if (isHuman && animatorIK.ikData != null)
                    {
                        toolRangeIK_AnimatorIKFoldout = EditorGUILayout.Foldout(toolRangeIK_AnimatorIKFoldout, "Animator IK", true);
                        if (toolRangeIK_AnimatorIKFoldout)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.BeginVertical(GUI.skin.box);
                            for (int index = 0; index < animatorIK.ikData.Length; index++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUI.BeginChangeCheck();
                                    EditorGUILayout.ToggleLeft(AnimatorIKCore.IKTargetStrings[index], animatorIK.ikData[index].enable);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        animatorIK.ChangeTargetIK((AnimatorIKCore.IKTarget)index);
                                    }
                                }
                                GUILayout.FlexibleSpace();
                                {
                                    EditorGUILayout.LabelField(animatorIK.IKSpaceTypeStrings[(int)animatorIK.ikData[index].spaceType], vaw.guiStyleMiddleRightGreyMiniLabel);
                                }
                                EditorGUILayout.Space();
                                EditorGUILayout.EndHorizontal();

                                if (animatorIK.ikData[index].enable)
                                {
                                    enableCount++;
                                }
                            }
                            EditorGUILayout.EndVertical();
                            EditorGUI.indentLevel--;
                        }
                    }
                    #endregion
                    #region OriginalIK
                    if (originalIK.ikData != null)
                    {
                        toolRangeIK_OriginalIKFoldout = EditorGUILayout.Foldout(toolRangeIK_OriginalIKFoldout, "Original IK", true);
                        if (toolRangeIK_OriginalIKFoldout)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.BeginVertical(GUI.skin.box);
                            if (originalIK.ikData.Count > 0)
                            {
                                for (int index = 0; index < originalIK.ikData.Count; index++)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        EditorGUILayout.ToggleLeft(originalIK.ikData[index].name, originalIK.ikData[index].enable);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            originalIK.ChangeTargetIK(index);
                                        }
                                    }
                                    GUILayout.FlexibleSpace();
                                    {
                                        EditorGUILayout.LabelField(string.Format("{0} : {1}", originalIK.SolverTypeStrings[(int)originalIK.ikData[index].solverType].text, originalIK.IKSpaceTypeStrings[(int)originalIK.ikData[index].spaceType].text), vaw.guiStyleMiddleRightGreyMiniLabel);
                                    }
                                    EditorGUILayout.Space();
                                    EditorGUILayout.EndHorizontal();

                                    if (originalIK.ikData[index].enable)
                                    {
                                        enableCount++;
                                    }
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField("Empty", EditorStyles.centeredGreyMiniLabel);
                            }
                            EditorGUILayout.EndVertical();
                            EditorGUI.indentLevel--;
                        }
                    }
                    #endregion
                    disable = enableCount <= 0;
                    EditorGUI.indentLevel = saveIndentLevel;
                }
                EditorGUI.BeginDisabledGroup(disable);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                if (GUILayout.Button("Apply"))
                {
                    ToolsGenarateRangeIK(clip);
                }
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
                #endregion
            }
            else if (toolMode == ToolMode.HumanoidIK)
            {
                #region HumanoidIK
                EditorGUI.BeginDisabledGroup(!isHuman || !clip.isHumanMotion);
                if (uAnimationClipEditor != null)
                {
                    float firstFrame = toolHumanoidIK_FirstFrame;
                    float lastFrame = toolHumanoidIK_LastFrame;
                    float additivePoseframe = 0.0f;
                    bool changedStart = false;
                    bool changedStop = false;
                    bool changedAdditivePoseframe = false;
                    uAnimationClipEditor.ClipRangeGUI(ref firstFrame, ref lastFrame, out changedStart, out changedStop, false, ref additivePoseframe, out changedAdditivePoseframe);
                    if (changedStart)
                    {
                        Undo.RecordObject(vaw, "Change First Frame");
                        toolHumanoidIK_FirstFrame = Mathf.RoundToInt(firstFrame);
                    }
                    if (changedStop)
                    {
                        Undo.RecordObject(vaw, "Change Last Frame");
                        toolHumanoidIK_LastFrame = Mathf.RoundToInt(lastFrame);
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Hand IK", toolHumanoidIK_Hand);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change IK Curve setting");
                        toolHumanoidIK_Hand = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Foot IK", toolHumanoidIK_Foot);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change IK Curve setting");
                        toolHumanoidIK_Foot = flag;
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(!isHuman || !clip.isHumanMotion || (!toolHumanoidIK_Hand && !toolHumanoidIK_Foot));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                if (GUILayout.Button("Clear"))
                {
                    ToolsClearHumanoidIK(clip);
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("Genarate"))
                {
                    ToolsGenarateHumanoidIK(clip);
                }
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
                if (!isHuman || !clip.isHumanMotion)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpToolsHumanoidWarning), MessageType.Warning);
                }
                #endregion
            }
            else if (toolMode == ToolMode.ParameterRelatedCurves)
            {
                #region ParameterRelatedCurves
                if (e.type == EventType.Layout)
                {
                    ParameterRelatedCurveUpdateList();
                }
                if (toolParameterRelatedCurve_List != null)
                {
                    toolParameterRelatedCurve_List.DoLayoutList();
                }
                if (clip.legacy || vaw.animator == null)
                {
                    EditorGUILayout.HelpBox("Only Humanoid and Generic Motion will be effective.", MessageType.Warning);
                }
                #endregion
            }
            else if (toolMode == ToolMode.RotationCurveInterpolation)
            {
                #region RotationCurveInterpolation
                {
                    EditorGUI.BeginChangeCheck();
                    var mode = (RotationCurveInterpolationMode)EditorGUILayout.EnumPopup("Interpolation", toolRotationInterpolation_Mode);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Rotation Curve Interpolation setting");
                        toolRotationInterpolation_Mode = mode;
                    }
                }
                if (toolRotationInterpolation_Mode == RotationCurveInterpolationMode.EulerAngles)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpToolsRotationCurveInterpolationEulerAnglesWarning), MessageType.Warning);
                }
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Convert"))
                    {
                        ToolsRotationCurveInterpolation(clip, toolRotationInterpolation_Mode);
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                }
                #endregion
            }
            else if (toolMode == ToolMode.KeyframeReduction)
            {
                #region KeyframeReduction
                {
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Rotation Error", GUILayout.Width(150f));
                            EditorGUI.BeginChangeCheck();
                            var param = EditorGUILayout.FloatField(toolKeyframeReduction_RotationError, GUILayout.Width(100f));
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(vaw, "Change Keyframe Reduction setting");
                                toolKeyframeReduction_RotationError = param;
                            }
                        }
                        EditorGUILayout.Space();
                        {
                            if (GUILayout.Button("Reset"))
                            {
                                Undo.RecordObject(vaw, "Change Keyframe Reduction setting");
                                toolKeyframeReduction_RotationError = 0.5f;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Position Error", GUILayout.Width(150f));
                            EditorGUI.BeginChangeCheck();
                            var param = EditorGUILayout.FloatField(toolKeyframeReduction_PositionError, GUILayout.Width(100f));
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(vaw, "Change Keyframe Reduction setting");
                                toolKeyframeReduction_PositionError = param;
                            }
                        }
                        EditorGUILayout.Space();
                        {
                            if (GUILayout.Button("Reset"))
                            {
                                Undo.RecordObject(vaw, "Change Keyframe Reduction setting");
                                toolKeyframeReduction_PositionError = 0.5f;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Scale and Others Error", GUILayout.Width(150f));
                            EditorGUI.BeginChangeCheck();
                            var param = EditorGUILayout.FloatField(toolKeyframeReduction_ScaleAndOthersError, GUILayout.Width(100f));
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(vaw, "Change Keyframe Reduction setting");
                                toolKeyframeReduction_ScaleAndOthersError = param;
                            }
                        }
                        EditorGUILayout.Space();
                        {
                            if (GUILayout.Button("Reset"))
                            {
                                Undo.RecordObject(vaw, "Change Keyframe Reduction setting");
                                toolKeyframeReduction_ScaleAndOthersError = 0.5f;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    {
                        EditorGUI.BeginChangeCheck();
                        var flag = EditorGUILayout.Toggle(new GUIContent("Humanoid Curves", "Animator Type"), toolKeyframeReduction_EnableHumanoid);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(vaw, "Change Keyframe Reduction setting");
                            toolKeyframeReduction_EnableHumanoid = flag;
                        }
                    }
                    {
                        EditorGUI.BeginChangeCheck();
                        var flag = EditorGUILayout.Toggle(new GUIContent("Generic Curves", "Transform Type"), toolKeyframeReduction_EnableGeneric);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(vaw, "Change Keyframe Reduction setting");
                            toolKeyframeReduction_EnableGeneric = flag;
                        }
                    }
                    {
                        EditorGUI.BeginChangeCheck();
                        var flag = EditorGUILayout.Toggle(new GUIContent("Other Curves", "Anything Type"), toolKeyframeReduction_EnableOther);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(vaw, "Change Keyframe Reduction setting");
                            toolKeyframeReduction_EnableOther = flag;
                        }
                    }
                    {
                        EditorGUI.BeginDisabledGroup(!toolKeyframeReduction_EnableHumanoid && !toolKeyframeReduction_EnableGeneric && !toolKeyframeReduction_EnableOther);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.Space();
                        if (GUILayout.Button("Reduction"))
                        {
                            ToolsKeyframeReduction(clip);
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                    }
                }
                #endregion
            }
            else if (toolMode == ToolMode.EnsureQuaternionContinuity)
            {
                #region EnsureQuaternionContinuity
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Execute"))
                    {
                        ToolsEnsureQuaternionContinuity(clip);
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                }
                #endregion
            }
            else if (toolMode == ToolMode.Cleanup)
            {
                #region Cleanup
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft(new GUIContent("Remove Animator Root Curves", "RootT, RootQ"), toolCleanup_RemoveRoot);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveRoot = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft(new GUIContent("Remove Animator IK Curves", "LeftHandT, LeftHandQ,\nRightHandT, RightHandQ,\nLeftFootT, LeftFootQ,\nRightFootT, RightFootQ"), toolCleanup_RemoveIK);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveIK = flag;
                    }
                }
                {
                    string tooltip = "LeftUpperLegTDOF, RightUpperLegTDOF,\nSpineTDOF, ChestTDOF, NeckTDOF,\nLeftShoulderTDOF, RightShoulderTDOF,\nUpperChestTDOF";

                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft(new GUIContent("Remove Animator TDOF Curves", tooltip), toolCleanup_RemoveTDOF);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveTDOF = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft(new GUIContent("Remove Animator Motion Curves", "MotionT, MotionQ"), toolCleanup_RemoveMotion);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveMotion = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Remove Animator Finger Curves", toolCleanup_RemoveFinger);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveFinger = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Remove Animator Eye Curves", toolCleanup_RemoveEyes);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveEyes = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Remove Animator Jaw Curve", toolCleanup_RemoveJaw);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveJaw = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Remove Animator Toe Curves", toolCleanup_RemoveToes);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveToes = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Remove Transform Position Curves", toolCleanup_RemoveTransformPosition);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveTransformPosition = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Remove Transform Rotation Curves", toolCleanup_RemoveTransformRotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveTransformRotation = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Remove Transform Scale Curves", toolCleanup_RemoveTransformScale);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveTransformScale = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Remove BlendShape Curves", toolCleanup_RemoveBlendShape);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveBlendShape = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Remove Object Reference Curves", toolCleanup_RemoveObjectReference);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveObjectReference = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Remove Animation Events", toolCleanup_RemoveEvent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveEvent = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Remove Missing Curves", toolCleanup_RemoveMissing);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveMissing = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Remove Humanoid and Generic conflict Curves", toolCleanup_RemoveHumanoidConflict);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveHumanoidConflict = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft("Remove Root motion conflict Curves", toolCleanup_RemoveRootMotionConflict);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveRootMotionConflict = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft(new GUIContent("Remove Unnecessary Curves", "Unnecessary curves are deleted in the current state.\nThis process works with reference to 'Write Defaults' in Animator State and 'Apply Root Motion' in Animator."), toolCleanup_RemoveUnnecessary);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Cleanup setting");
                        toolCleanup_RemoveUnnecessary = flag;
                    }
                }
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    EditorGUI.BeginDisabledGroup(!toolCleanup_RemoveRoot && !toolCleanup_RemoveIK && !toolCleanup_RemoveTDOF && !toolCleanup_RemoveMotion &&
                                                    !toolCleanup_RemoveFinger && !toolCleanup_RemoveEyes && !toolCleanup_RemoveJaw && !toolCleanup_RemoveToes &&
                                                    !toolCleanup_RemoveTransformPosition && !toolCleanup_RemoveTransformRotation && !toolCleanup_RemoveTransformScale && !toolCleanup_RemoveBlendShape &&
                                                    !toolCleanup_RemoveObjectReference && !toolCleanup_RemoveEvent && !toolCleanup_RemoveMissing && !toolCleanup_RemoveHumanoidConflict && !toolCleanup_RemoveRootMotionConflict && !toolCleanup_RemoveUnnecessary);
                    if (GUILayout.Button("Cleanup"))
                    {
                        ToolsCleanup(clip);
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                }
                #endregion
            }
            else if (toolMode == ToolMode.FixErrors)
            {
                #region FixErrors
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Fix"))
                    {
                        ToolsFixErrors(clip);
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                }
                #endregion
            }
            else if (toolMode == ToolMode.Export)
            {
                #region Export
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.Toggle("Active Only", toolExport_ActiveOnly);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Export setting");
                        toolExport_ActiveOnly = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.Toggle("Export Mesh", toolExport_Mesh);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Export setting");
                        toolExport_Mesh = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var mode = (ExportAnimationMode)EditorGUILayout.EnumPopup("Export Animation", toolExport_AnimationMode);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Export setting");
                        toolExport_AnimationMode = mode;
                    }
                }
                if (toolExport_AnimationMode != ExportAnimationMode.None)
                {
                    EditorGUI.indentLevel++;
                    if (isHuman)
                    {
                        EditorGUI.BeginChangeCheck();
                        var flag = EditorGUILayout.Toggle(new GUIContent("Foot IK", "Activates feet IK bake."), toolExport_FootIK);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(vaw, "Change Export setting");
                            toolExport_FootIK = flag;
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Export"))
                    {
                        ToolsExport();
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                }
                #endregion
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        public void DuplicateAndReplace()
        {
            var assetPath = AssetDatabase.GetAssetPath(currentClip);
            assetPath = string.Format("{0}/{1}.anim", Path.GetDirectoryName(assetPath), currentClip.name);
            var uniquePath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            string path = EditorUtility.SaveFilePanel("Duplicate", Path.GetDirectoryName(uniquePath), Path.GetFileName(uniquePath), "anim");
            if (string.IsNullOrEmpty(path))
                return;
            if (!path.StartsWith(Application.dataPath))
            {
                EditorCommon.SaveInsideAssetsFolderDisplayDialog();
                return;
            }
            else
            {
                path = path.Replace(Application.dataPath, "Assets");
                AssetDatabase.CreateAsset(AnimationClip.Instantiate(currentClip), path);
                var newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
#if UNITY_2017_1_OR_NEWER
                #region Timeline
                if (uAw_2017_1.GetLinkedWithTimeline())
                {
                    var uAw_2017_1 = uAw as UAnimationWindow_2017_1;
                    var timelineClip = uAw_2017_1.GetTimelineAnimationClip();
                    if (timelineClip == currentClip)
                    {
                        uAw_2017_1.SetTimelineAnimationClip(newClip, "Duplicate and Replace");
                    }
                }
                #endregion
                else
#endif
                {
                    #region Animator
                    if (vaw.animator != null && vaw.animator.runtimeAnimatorController != null)
                    {
                        UnityEditor.Animations.AnimatorController ac = null;
                        #region AnimatorOverrideController
                        if (vaw.animator.runtimeAnimatorController is AnimatorOverrideController)
                        {
                            var owc = vaw.animator.runtimeAnimatorController as AnimatorOverrideController;
                            ac = owc.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                            {
                                List<KeyValuePair<AnimationClip, AnimationClip>> srcList = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                                owc.GetOverrides(srcList);
                                List<KeyValuePair<AnimationClip, AnimationClip>> dstList = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                                bool changed = false;
                                foreach (var pair in srcList)
                                {
                                    if (pair.Key == currentClip || pair.Value == currentClip)
                                        changed = true;
                                    dstList.Add(new KeyValuePair<AnimationClip, AnimationClip>(pair.Key != currentClip ? pair.Key : newClip,
                                                                                                pair.Value != currentClip ? pair.Value : newClip));
                                }
                                if (changed)
                                    owc.ApplyOverrides(dstList);
                            }
                        }
                        else
                        {
                            ac = vaw.animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                        }
                        #endregion
                        #region AnimatorControllerLayer
                        foreach (UnityEditor.Animations.AnimatorControllerLayer layer in ac.layers)
                        {
                            Action<AnimatorStateMachine> ReplaceStateMachine = null;
                            ReplaceStateMachine = (stateMachine) =>
                            {
                                foreach (var state in stateMachine.states)
                                {
                                    if (state.state.motion is UnityEditor.Animations.BlendTree)
                                    {
                                        Undo.RecordObject(state.state, "Duplicate and Replace");
                                        Action<UnityEditor.Animations.BlendTree> ReplaceBlendTree = null;
                                        ReplaceBlendTree = (blendTree) =>
                                        {
                                            if (blendTree.children == null) return;
                                            Undo.RecordObject(blendTree, "Duplicate and Replace");
                                            var children = blendTree.children;
                                            for (int i = 0; i < children.Length; i++)
                                            {
                                                if (children[i].motion is UnityEditor.Animations.BlendTree)
                                                {
                                                    ReplaceBlendTree(children[i].motion as UnityEditor.Animations.BlendTree);
                                                }
                                                else
                                                {
                                                    if (children[i].motion == currentClip)
                                                    {
                                                        children[i].motion = newClip;
                                                    }
                                                }
                                            }
                                            blendTree.children = children;
                                        };
                                        ReplaceBlendTree(state.state.motion as UnityEditor.Animations.BlendTree);
                                    }
                                    else
                                    {
                                        if (state.state.motion == currentClip)
                                        {
                                            Undo.RecordObject(state.state, "Duplicate and Replace");
                                            state.state.motion = newClip;
                                        }
                                    }
                                }
                                foreach (var childStateMachine in stateMachine.stateMachines)
                                {
                                    ReplaceStateMachine(childStateMachine.stateMachine);
                                }
                            };
                            ReplaceStateMachine(layer.stateMachine);
                        }
                        #endregion
                    }
                    #endregion
                    #region Animation
                    if (vaw.animation != null)
                    {
                        Undo.RecordObject(vaw.animation, "Duplicate and Replace");
                        bool changed = false;
                        var animations = AnimationUtility.GetAnimationClips(vaw.gameObject);
                        for (int i = 0; i < animations.Length; i++)
                        {
                            if (animations[i] == currentClip)
                            {
                                animations[i] = newClip;
                                changed = true;
                            }
                        }
                        if (vaw.animation.clip == currentClip)
                        {
                            vaw.animation.clip = newClip;
                            changed = true;
                        }
                        if (changed)
                            AnimationUtility.SetAnimationClips(vaw.animation, animations);
                    }
                    #endregion
                }

                #region Extra
                {
                    ToolsReductionCurve(newClip);

                    ToolsRotationCurveInterpolation(newClip, RotationCurveInterpolationMode.Quaternion);
                }
                #endregion

                IKUpdateBones();

                uAw.SetSelectionAnimationClip(newClip, "Duplicate and Replace");

                ClearEditorCurveCache();
                SetUpdateResampleAnimation();
                uAw.ForceRefresh();
            }
        }

        private void ToolsReset()
        {
            var lastFrame = GetLastFrame();
            toolRangeIK_FirstFrame = 0;
            toolRangeIK_LastFrame = lastFrame;
            toolHumanoidIK_FirstFrame = 0;
            toolHumanoidIK_LastFrame = lastFrame;
            toolCopy_FirstFrame = 0;
            toolCopy_LastFrame = lastFrame;
            toolCopy_WriteFrame = lastFrame + 1;
            toolTrim_FirstFrame = 0;
            toolTrim_LastFrame = lastFrame;

            ToolsParameterRelatedCurveReset();
        }
        private void ToolsParameterRelatedCurveReset()
        {
            toolParameterRelatedCurve_DataList = null;
            toolParameterRelatedCurve_Update = true;
            toolParameterRelatedCurve_List = null;
        }

        private void ParameterRelatedCurveUpdateList()
        {
            if (!toolParameterRelatedCurve_Update)
                return;

            Action UpdateEnableFlagAll = () =>
            {
                if (toolParameterRelatedCurve_DataList == null) return;
                var ac = GetAnimatorController();
                var parameters = ac.parameters;
                foreach (var data in toolParameterRelatedCurve_DataList)
                {
                    var binding = AnimationCurveBindingAnimatorCustom(data.propertyName);
                    var curve = GetEditorCurveCache(binding);
                    data.enableAnimationCurve = curve != null;
                    data.parameterIndex = ArrayUtility.FindIndex(parameters, (x) => x.name == data.propertyName);
                    data.enableAnimatorParameter = data.parameterIndex >= 0 && parameters[data.parameterIndex].type == UnityEngine.AnimatorControllerParameterType.Float;
                }
                SetUpdateResampleAnimation();
                SetAnimationWindowRefresh(AnimationWindowStateRefreshType.Everything);
            };
            Action<int> UpdateEnableFlag = (index) =>
            {
                if (toolParameterRelatedCurve_DataList == null) return;
                var ac = GetAnimatorController();
                var parameters = ac.parameters;
                {
                    var data = toolParameterRelatedCurve_DataList[index];
                    var binding = AnimationCurveBindingAnimatorCustom(data.propertyName);
                    var curve = GetEditorCurveCache(binding);
                    data.enableAnimationCurve = curve != null;
                    data.parameterIndex = ArrayUtility.FindIndex(parameters, (x) => x.name == data.propertyName);
                    data.enableAnimatorParameter = data.parameterIndex >= 0 && parameters[data.parameterIndex].type == UnityEngine.AnimatorControllerParameterType.Float;
                }
                SetUpdateResampleAnimation();
                SetAnimationWindowRefresh(AnimationWindowStateRefreshType.Everything);
            };

            if (toolParameterRelatedCurve_DataList == null)
                toolParameterRelatedCurve_DataList = new List<ParameterRelatedData>();
            else
                toolParameterRelatedCurve_DataList.Clear();
            {
                var ac = GetAnimatorController();
                var parameters = ac.parameters;
                foreach (var binding in AnimationUtility.GetCurveBindings(currentClip))
                {
                    if (binding.type != typeof(Animator)) continue;
                    if (IsAnimatorReservedPropertyName(binding.propertyName)) continue;
                    bool ready = false;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].type != UnityEngine.AnimatorControllerParameterType.Float) continue;
                        if (binding.propertyName == parameters[i].name)
                        {
                            toolParameterRelatedCurve_DataList.Add(new ParameterRelatedData()
                            {
                                propertyName = binding.propertyName,
                                parameterIndex = i,
                                enableAnimationCurve = true,
                                enableAnimatorParameter = true
                            });
                            ready = true;
                            break;
                        }
                    }
                    if (!ready)
                    {
                        toolParameterRelatedCurve_DataList.Add(new ParameterRelatedData()
                        {
                            propertyName = binding.propertyName,
                            parameterIndex = -1,
                            enableAnimationCurve = true,
                            enableAnimatorParameter = false,
                        });
                    }
                }
            }
            toolParameterRelatedCurve_List = new ReorderableList(toolParameterRelatedCurve_DataList, typeof(ParameterRelatedData), false, true, true, true);
            toolParameterRelatedCurve_List.elementHeight = 20;
            toolParameterRelatedCurve_List.drawHeaderCallback = (Rect rect) =>
            {
                float x = rect.x;
                {
                    const float Rate = 0.4f;
                    var r = rect;
                    r.x = x;
                    r.width = rect.width * Rate;
                    x += r.width;
                    EditorGUI.LabelField(r, "Name", vaw.guiStyleCenterAlignLabel);
                }
                {
                    const float Rate = 0.2f;
                    var r = rect;
                    r.x = x;
                    r.width = rect.width * Rate;
                    x += r.width;
                    EditorGUI.LabelField(r, new GUIContent("Curve", "Animation Curve"), vaw.guiStyleCenterAlignLabel);
                }
                {
                    const float Rate = 0.2f;
                    var r = rect;
                    r.x = x;
                    r.width = rect.width * Rate;
                    x += r.width;
                    EditorGUI.LabelField(r, new GUIContent("Parameter", "Animator Controller Parameter"), vaw.guiStyleCenterAlignLabel);
                }
                {
                    const float Rate = 0.2f;
                    var r = rect;
                    r.x = x;
                    r.width = rect.width * Rate;
                    x += r.width;
                    EditorGUI.LabelField(r, "Value", vaw.guiStyleCenterAlignLabel);
                }
            };
            toolParameterRelatedCurve_List.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index >= toolParameterRelatedCurve_DataList.Count)
                    return;

                EditorGUI.BeginDisabledGroup((currentClip.hideFlags & HideFlags.NotEditable) != HideFlags.None);

                float x = rect.x;
                {
                    const float Rate = 0.4f;
                    var r = rect;
                    r.x = x;
                    r.y += 2;
                    r.height -= 4;
                    r.width = rect.width * Rate;
                    x += r.width;
                    EditorGUI.BeginChangeCheck();
                    var text = EditorGUI.TextField(r, toolParameterRelatedCurve_DataList[index].propertyName);
                    if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(text))
                    {
                        if (ToolsCommonBefore(currentClip, "Change Parameter Related Curve"))
                        {
                            var ac = GetAnimatorController();
                            {
                                var origText = text;
                                if (IsAnimatorReservedPropertyName(text))
                                    text += " 0";
                                text = ac.MakeUniqueParameterName(text);
                                if (origText != text)
                                {
                                    Debug.LogWarningFormat(Language.GetText(Language.Help.LogParameterRelatedCurveNameChanged), origText, text);
                                }
                            }
                            Undo.RecordObject(vaw, "Change Parameter Related Curve");
                            Undo.RecordObject(ac, "Change Parameter Related Curve");
                            {
                                var binding = AnimationCurveBindingAnimatorCustom(toolParameterRelatedCurve_DataList[index].propertyName);
                                var curve = AnimationUtility.GetEditorCurve(currentClip, binding);
                                if (curve != null)
                                {
                                    AnimationUtility.SetEditorCurve(currentClip, binding, null);
                                    binding = AnimationCurveBindingAnimatorCustom(text);
                                    AnimationUtility.SetEditorCurve(currentClip, binding, curve);
                                }
                            }
                            {
                                var parameters = ac.parameters;
                                int paramIndex = toolParameterRelatedCurve_DataList[index].parameterIndex;
                                if (paramIndex >= 0 && paramIndex < parameters.Length)
                                {
                                    parameters[paramIndex].name = text;
                                    ac.parameters = parameters;
                                }
                            }
                            toolParameterRelatedCurve_DataList[index].propertyName = text;
                            UpdateEnableFlag(index);
                        }
                    }
                }
                {
                    const float Rate = 0.2f;
                    var r = rect;
                    r.x = x;
                    r.y += 2;
                    r.height -= 4;
                    r.width = rect.width * Rate;
                    x += r.width;
                    if (toolParameterRelatedCurve_DataList[index].enableAnimationCurve)
                        EditorGUI.LabelField(r, "Ready", vaw.guiStyleCenterAlignLabel);
                    else
                        EditorGUI.LabelField(r, "Missing", vaw.guiStyleCenterAlignYellowLabel);
                }
                {
                    const float Rate = 0.2f;
                    var r = rect;
                    r.x = x;
                    r.y += 2;
                    r.height -= 4;
                    r.width = rect.width * Rate;
                    x += r.width;
                    if (toolParameterRelatedCurve_DataList[index].enableAnimatorParameter)
                        EditorGUI.LabelField(r, "Ready", vaw.guiStyleCenterAlignLabel);
                    else
                        EditorGUI.LabelField(r, "Missing", vaw.guiStyleCenterAlignYellowLabel);
                }
                {
                    const float Rate = 0.2f;
                    var r = rect;
                    r.x = x;
                    r.y += 2;
                    r.height -= 4;
                    r.width = rect.width * Rate;
                    x += r.width;
                    if (!toolParameterRelatedCurve_DataList[index].enableAnimationCurve || !toolParameterRelatedCurve_DataList[index].enableAnimatorParameter)
                    {
                        if (GUI.Button(r, "Fix"))
                        {
                            if (ToolsCommonBefore(currentClip, "Fix Parameter Related Curve"))
                            {
                                var ac = GetAnimatorController();
                                Undo.RecordObject(vaw, "Fix Parameter Related Curve");
                                Undo.RecordObject(ac, "Fix Parameter Related Curve");
                                if (!toolParameterRelatedCurve_DataList[index].enableAnimationCurve)
                                {
                                    var binding = AnimationCurveBindingAnimatorCustom(toolParameterRelatedCurve_DataList[index].propertyName);
                                    var curve = new AnimationCurve();
                                    SetKeyframeTangentModeLinear(curve, curve.AddKey(0f, 0f));
                                    SetKeyframeTangentModeLinear(curve, curve.AddKey(currentClip.length, 1f));
                                    AnimationUtility.SetEditorCurve(currentClip, binding, curve);
                                }
                                if (!toolParameterRelatedCurve_DataList[index].enableAnimatorParameter)
                                {
                                    {
                                        var parameters = ac.parameters;
                                        var paramIndex = ArrayUtility.FindIndex(parameters, (d) => d.name == toolParameterRelatedCurve_DataList[index].propertyName);
                                        if (paramIndex >= 0 && paramIndex < parameters.Length)
                                        {
                                            ac.RemoveParameter(paramIndex);
                                        }
                                    }
                                    ac.AddParameter(toolParameterRelatedCurve_DataList[index].propertyName, UnityEngine.AnimatorControllerParameterType.Float);
                                }
                                UpdateEnableFlag(index);
                            }
                        }
                    }
                    else if (uAvatarPreview != null)
                    {
                        var binding = AnimationCurveBindingAnimatorCustom(toolParameterRelatedCurve_DataList[index].propertyName);
                        var curve = GetEditorCurveCache(binding);
                        if (curve != null)
                        {
                            var value = curve.Evaluate(uAvatarPreview.GetTime());
                            EditorGUI.LabelField(r, value.ToString("F2"), vaw.guiStyleCenterAlignLabel);
                        }
                    }
                }

                EditorGUI.EndDisabledGroup();
            };
            toolParameterRelatedCurve_List.onSelectCallback = (ReorderableList list) =>
            {
                UpdateEnableFlagAll();
                SetAnimationWindowSynchroSelection(new EditorCurveBinding[] { AnimationCurveBindingAnimatorCustom(toolParameterRelatedCurve_DataList[list.index].propertyName) });
            };
            toolParameterRelatedCurve_List.onAddCallback = (ReorderableList list) =>
            {
                if (!ToolsCommonBefore(currentClip, "Add Parameter Related Curve")) return;

                Undo.RecordObject(vaw, "Add Parameter Related Curve");
                {
                    var ac = GetAnimatorController();
                    {
                        var name = ac.MakeUniqueParameterName("New Parameter");
                        var data = new ParameterRelatedData() { propertyName = name };
                        {
                            var binding = AnimationCurveBindingAnimatorCustom(name);
                            var curve = new AnimationCurve();
                            SetKeyframeTangentModeLinear(curve, curve.AddKey(0f, 0f));
                            SetKeyframeTangentModeLinear(curve, curve.AddKey(currentClip.length, 1f));
                            AnimationUtility.SetEditorCurve(currentClip, binding, curve);
                        }
                        {
                            ac.AddParameter(name, UnityEngine.AnimatorControllerParameterType.Float);
                        }
                        toolParameterRelatedCurve_DataList.Add(data);
                    }
                }
                toolParameterRelatedCurve_Update = true;
                EditorApplication.delayCall += () =>
                {
                    toolParameterRelatedCurve_List.index = toolParameterRelatedCurve_DataList.Count - 1;
                    SetAnimationWindowSynchroSelection(new EditorCurveBinding[] { AnimationCurveBindingAnimatorCustom(toolParameterRelatedCurve_DataList[toolParameterRelatedCurve_List.index].propertyName) });
                    vaw.Repaint();
                };

                ToolsCommonAfter();
                InternalEditorUtility.RepaintAllViews();
            };
            toolParameterRelatedCurve_List.onRemoveCallback = (ReorderableList list) =>
            {
                if (!ToolsCommonBefore(currentClip, "Add Parameter Related Curve")) return;

                Undo.RecordObject(vaw, "Add Parameter Related Curve");
                {
                    var ac = GetAnimatorController();
                    {
                        var data = toolParameterRelatedCurve_DataList[list.index];
                        {
                            var binding = AnimationCurveBindingAnimatorCustom(data.propertyName);
                            AnimationUtility.SetEditorCurve(currentClip, binding, null);
                        }
                        {
                            var parameters = ac.parameters;
                            data.parameterIndex = ArrayUtility.FindIndex(parameters, (x) => x.name == data.propertyName);
                            if (data.parameterIndex >= 0)
                                ac.RemoveParameter(data.parameterIndex);
                        }
                    }
                }
                toolParameterRelatedCurve_DataList.RemoveAt(list.index);
                toolParameterRelatedCurve_Update = true;

                ToolsCommonAfter();
                InternalEditorUtility.RepaintAllViews();
            };

            UpdateEnableFlagAll();
            toolParameterRelatedCurve_Update = false;
        }

        private bool ToolsCommonBefore(AnimationClip clip, string undoName)
        {
            if (!BeginChangeAnimationCurve(clip, undoName))
                return false;

            uAw.ClearKeySelections();
            ClearEditorCurveCache();
            SetOnCurveWasModifiedStop(true);

            return true;
        }
        private void ToolsCommonAfter()
        {
            SetOnCurveWasModifiedStop(false);
            curvesWasModified.Clear();
            ClearEditorCurveCache();
            SetUpdateResampleAnimation();
            #region GenericRootMotion
            if (!isHuman && rootMotionBoneIndex >= 0)
                updateGenericRootMotion = true;
            #endregion
            SetUpdateIKtargetAll(false);
            SetSynchroIKtargetAll(true);
            uAw.ForceRefresh();
        }

        private void ToolsReductionCurve(AnimationClip clip)
        {
            if (!ToolsCommonBefore(clip, "Reduction Curve")) return;

            try
            {
                bool allWriteDefaults = true;
#if UNITY_2017_1_OR_NEWER
                if (uAw_2017_1.GetLinkedWithTimeline())
                {
                    allWriteDefaults = false;
                }
                else
#endif
                {
                    ActionAllAnimatorState(clip, (animatorState) =>
                    {
                        if (!animatorState.writeDefaultValues)
                            allWriteDefaults = false;
                    });
                }

                #region It is not necessary if AnimatorState.writeDefaultValues is enabled
                if (allWriteDefaults)
                {
                    const float eps = 0.0001f;

                    var bindings = AnimationUtility.GetCurveBindings(clip);
                    int progressIndex = 0;
                    int progressTotal = bindings.Length;

                    bool[] doneFlags = new bool[bindings.Length];
                    for (int k = 0; k < bindings.Length; k++)
                    {
                        EditorUtility.DisplayProgressBar("Reduction Curve", string.IsNullOrEmpty(bindings[k].path) ? bindings[k].propertyName : bindings[k].path, progressIndex++ / (float)progressTotal);
                        if (doneFlags[k]) continue;
                        doneFlags[k] = true;
                        var curve = AnimationUtility.GetEditorCurve(clip, bindings[k]);
                        if (curve == null) continue;
                        var t = GetTransformFromPath(bindings[k].path);
                        if (t == null) continue;
                        if (bindings[k].type == typeof(Animator))
                        {
                            #region Animator
                            if (GetMuscleIndexFromCurveBinding(bindings[k]) >= 0)
                            {
                                bool remove = true;
                                for (float time = 0f; time <= clip.length; time += 1f / clip.frameRate)
                                {
                                    if (Mathf.Abs(curve.Evaluate(time)) >= eps)
                                    {
                                        remove = false;
                                        break;
                                    }
                                }
                                if (remove)
                                {
                                    AnimationUtility.SetEditorCurve(clip, bindings[k], null);
                                }
                            }
                            #endregion
                        }
                        else if (bindings[k].type == typeof(Transform))
                        {
                            #region Transform
                            string[] TypeNames =
                            {
                                "m_LocalPosition",
                                "m_LocalRotation",
                                "m_LocalScale",
                                "localEulerAnglesRaw",
                            };
                            int type = -1;
                            for (int i = 0; i < TypeNames.Length; i++)
                            {
                                if (bindings[k].propertyName.StartsWith(TypeNames[i]))
                                {
                                    type = i;
                                    break;
                                }
                            }
                            var boneIndex = GetBoneIndexFromCurveBinding(bindings[k]);
                            if (type >= 0 && boneIndex >= 0)
                            {
                                var save = transformPoseSave.GetOriginalTransform(bones[boneIndex].transform);
                                if (save != null)
                                {
                                    int dofCount = type == 1 ? 4 : 3;
                                    bool remove = true;
                                    int[] indexes = new int[dofCount];
                                    for (int dof = 0; dof < dofCount; dof++)
                                    {
                                        indexes[dof] = ArrayUtility.FindIndex(bindings, (x) => x.type == bindings[k].type && x.path == bindings[k].path &&
                                                                                                x.propertyName == TypeNames[type] + DofIndex2String[dof]);
                                        if (indexes[dof] >= 0)
                                            doneFlags[indexes[dof]] = true;
                                        if (remove && indexes[dof] >= 0)
                                        {
                                            curve = AnimationUtility.GetEditorCurve(clip, bindings[indexes[dof]]);
                                            if (curve != null)
                                            {
                                                float saveValue = 0f;
                                                switch (type)
                                                {
                                                case 0: saveValue = save.localPosition[dof]; break;
                                                case 1: saveValue = save.localRotation[dof]; break;
                                                case 2: saveValue = save.localScale[dof]; break;
                                                case 3: saveValue = save.localRotation.eulerAngles[dof]; break;
                                                }
                                                for (float time = 0f; time <= clip.length; time += 1f / clip.frameRate)
                                                {
                                                    if (Mathf.Abs(curve.Evaluate(time) - saveValue) >= eps)
                                                    {
                                                        remove = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (remove)
                                    {
                                        foreach (var index in indexes)
                                        {
                                            if (index >= 0)
                                                AnimationUtility.SetEditorCurve(clip, bindings[index], null);
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        else if (bindings[k].type == typeof(SkinnedMeshRenderer))
                        {
                            #region SkinnedMeshRenderer
                            var renderer = t.GetComponent<SkinnedMeshRenderer>();
                            if (renderer != null && renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount > 0)
                            {
                                if (IsBlendShapePropertyName(bindings[k].propertyName))
                                {
                                    var name = PropertyName2BlendShapeName(bindings[k].propertyName);
                                    if (blendShapeWeightSave.IsHaveOriginalWeight(renderer, name))
                                    {
                                        var weight = blendShapeWeightSave.GetOriginalWeight(renderer, name);
                                        bool remove = true;
                                        for (float time = 0f; time <= clip.length; time += 1f / clip.frameRate)
                                        {
                                            if (Mathf.Abs(curve.Evaluate(time) - weight) >= eps)
                                            {
                                                remove = false;
                                                break;
                                            }
                                        }
                                        if (remove)
                                        {
                                            AnimationUtility.SetEditorCurve(clip, bindings[k], null);
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }
                #endregion

                #region Optional bone
                {
                    Action<HumanBodyBones> RemoveMuscleCurve = (hi) =>
                    {
                        for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                        {
                            var mi = HumanTrait.MuscleFromBone((int)hi, dofIndex);
                            if (mi < 0) continue;
                            AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorMuscle(mi), null);
                        }
                    };
                    if (!isHuman || !humanoidHasLeftHand)
                    {
                        for (var hi = HumanBodyBones.LeftThumbProximal; hi <= HumanBodyBones.LeftLittleDistal; hi++)
                            RemoveMuscleCurve(hi);
                    }
                    if (!isHuman || !humanoidHasRightHand)
                    {
                        for (var hi = HumanBodyBones.RightThumbProximal; hi <= HumanBodyBones.RightLittleDistal; hi++)
                            RemoveMuscleCurve(hi);
                    }
                    if (!isHuman || !humanoidHasTDoF)
                    {
                        for (int tdofIndex = 0; tdofIndex < (int)AnimatorTDOFIndex.Total; tdofIndex++)
                        {
                            for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                                AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorTDOF((AnimatorTDOFIndex)tdofIndex, dofIndex), null);
                        }
                    }
                    Action<HumanBodyBones> RemoveNoneMuscleCurve = (hi) =>
                    {
                        if (!isHuman || editHumanoidBones[(int)hi] == null)
                            RemoveMuscleCurve(hi);
                    };
                    RemoveNoneMuscleCurve(HumanBodyBones.LeftEye);
                    RemoveNoneMuscleCurve(HumanBodyBones.RightEye);
                    RemoveNoneMuscleCurve(HumanBodyBones.Jaw);
                    RemoveNoneMuscleCurve(HumanBodyBones.LeftToes);
                    RemoveNoneMuscleCurve(HumanBodyBones.RightToes);
                }
                #endregion

                #region GenericRootMotion
                if (!isHuman)
                {
                    if (rootMotionBoneIndex >= 0)
                    {
                        if (IsHaveAnimationCurveTransformPosition(0))
                        {
                            for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                                AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingTransformPosition(0, dofIndex), null);
                        }
                        if (IsHaveAnimationCurveTransformRotation(0) != URotationCurveInterpolation.Mode.Undefined)
                        {
                            for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                                AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingTransformRotation(0, dofIndex, URotationCurveInterpolation.Mode.RawEuler), null);
                            for (int dofIndex = 0; dofIndex < 4; dofIndex++)
                                AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingTransformRotation(0, dofIndex, URotationCurveInterpolation.Mode.RawQuaternions), null);
                        }
                    }
                    else
                    {
                        if (IsHaveAnimationCurveAnimatorRootT())
                        {
                            for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                                AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorRootT[dofIndex], null);
                        }
                        if (IsHaveAnimationCurveAnimatorRootQ())
                        {
                            for (int dofIndex = 0; dofIndex < 4; dofIndex++)
                                AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorRootQ[dofIndex], null);
                        }
                    }
                }
                #endregion
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            ToolsCommonAfter();
        }
        private bool ToolsFixOverRotationCurve(AnimationClip clip)
        {
            if (!ToolsCommonBefore(clip, "Fix Over Rotation Curve")) return false;

            try
            {
                var bindings = AnimationUtility.GetCurveBindings(clip);
                int progressIndex = 0;
                int progressTotal = bindings.Length;
                #region CurveBindings
                foreach (var binding in bindings)
                {
                    EditorUtility.DisplayProgressBar("Fix Over Rotation Curve", string.IsNullOrEmpty(binding.path) ? binding.propertyName : binding.path, progressIndex++ / (float)progressTotal);
                    if (!IsTransformRotationCurveBinding(binding) || uRotationCurveInterpolation.GetModeFromCurveData(binding) != URotationCurveInterpolation.Mode.RawEuler) continue;
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    if (curve == null) continue;
                    bool update = false;
                    for (int i = 1; i < curve.length; i++)
                    {
                        var power = curve[i].value - curve[i - 1].value;
                        if (Mathf.Abs(power) < 180f) continue;
                        var time = uAw.SnapToFrame(Mathf.Lerp(curve[i].time, curve[i - 1].time, 0.5f), clip.frameRate);
                        if (Mathf.Approximately(time, curve[i].time) || Mathf.Approximately(time, curve[i - 1].time)) continue;
                        AddKeyframe(curve, time, curve.Evaluate(time));
                        update = true;
                        i = 0;
                    }
                    if (update)
                    {
                        AnimationUtility.SetEditorCurve(clip, binding, curve);
                        Debug.LogWarningFormat(Language.GetText(Language.Help.LogFixOverRotationCurve), binding.path, binding.propertyName);
                    }
                }
                #endregion
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            ToolsCommonAfter();

            return true;
        }
        
        private struct IKDataSave
        {
            public IKDataSave(AnimatorIKCore.AnimatorIKData ikData)
            {
                position = ikData.position;
                rotation = ikData.rotation;
                worldPosition = ikData.worldPosition;
                worldRotation = ikData.worldRotation;
                swivelRotation = ikData.swivelRotation;
            }
            public IKDataSave(OriginalIKCore.OriginalIKData ikData)
            {
                position = ikData.position;
                rotation = ikData.rotation;
                worldPosition = ikData.worldPosition;
                worldRotation = ikData.worldRotation;
                swivelRotation = ikData.swivel;
            }
            public void Set(AnimatorIKCore.AnimatorIKData ikData)
            {
                if (ikData.spaceType == AnimatorIKCore.AnimatorIKData.SpaceType.Parent)
                {
                    ikData.position = position;
                    ikData.rotation = rotation;
                }
                else
                {
                    ikData.worldPosition = worldPosition;
                    ikData.worldRotation = worldRotation;
                }
                ikData.swivelRotation = swivelRotation;
            }
            public void Set(OriginalIKCore.OriginalIKData ikData)
            {
                if (ikData.spaceType == OriginalIKCore.OriginalIKData.SpaceType.Parent)
                {
                    ikData.position = position;
                    ikData.rotation = rotation;
                }
                else
                {
                    ikData.worldPosition = worldPosition;
                    ikData.worldRotation = worldRotation;
                }
                ikData.swivel = swivelRotation;
            }

            public static IKDataSave Lerp(IKDataSave a, IKDataSave b, float t)
            {
                var ikDataSave = new IKDataSave()
                {
                    position = Vector3.Lerp(a.position, b.position, t),
                    rotation = Quaternion.Slerp(a.rotation, b.rotation, t),
                    worldPosition = Vector3.Lerp(a.worldPosition, b.worldPosition, t),
                    worldRotation = Quaternion.Slerp(a.worldRotation, b.worldRotation, t),
                };
                if (Mathf.Abs(a.swivelRotation - b.swivelRotation) > 180f)
                {
                    var aSwivel = a.swivelRotation;
                    if (aSwivel < 0f) aSwivel += 360f;
                    var bSwivel = b.swivelRotation;
                    if (bSwivel < 0f) bSwivel += 360f;
                    ikDataSave.swivelRotation = Mathf.Lerp(aSwivel, bSwivel, t);
                    while (ikDataSave.swivelRotation < -180f || ikDataSave.swivelRotation > 180f)
                    {
                        if (ikDataSave.swivelRotation > 180f)
                            ikDataSave.swivelRotation -= 360f;
                        else if (ikDataSave.swivelRotation < -180f)
                            ikDataSave.swivelRotation += 360f;
                    }
                }
                else
                {
                    ikDataSave.swivelRotation = Mathf.Lerp(a.swivelRotation, b.swivelRotation, t);
                }
                return ikDataSave;
            }

            public Vector3 position;
            public Quaternion rotation;
            public Vector3 worldPosition;
            public Quaternion worldRotation;
            public float swivelRotation;
        }
        private void ToolsGenarateRangeIK(AnimationClip clip)
        {
            if (!ToolsCommonBefore(clip, "Range IK")) return;

            if (clip != currentClip) return;

            var saveCurrentTime = uAw.GetCurrentTime();
            try
            {
                var beginTime = uAw.SnapToFrame(toolRangeIK_FirstFrame >= 0 ? toolRangeIK_FirstFrame / clip.frameRate : 0f, clip.frameRate);
                var endTime = uAw.SnapToFrame(toolRangeIK_LastFrame >= 0 ? toolRangeIK_LastFrame / clip.frameRate : clip.length, clip.frameRate);
                int progressIndex = 0;
                int progressTotal = 2;
                #region AnimatorIK
                EditorUtility.DisplayProgressBar("Range IK", "Animator IK", progressIndex++ / (float)progressTotal);
                if (isHuman && animatorIK.ikData.Where(data => data.enable).ToArray().Length > 0)
                {
                    IKDataSave[] ikDataBeginSave = new IKDataSave[animatorIK.ikData.Length];
                    IKDataSave[] ikDataEndSave = new IKDataSave[animatorIK.ikData.Length];
                    {
                        ResampleAnimation(beginTime);
                        for (var index = 0; index < animatorIK.ikData.Length; index++)
                        {
                            if (!animatorIK.ikData[index].enable) continue;
                            animatorIK.SynchroSet((AnimatorIKCore.IKTarget)index);
                            ikDataBeginSave[index] = new IKDataSave(animatorIK.ikData[index]);
                        }
                        ResampleAnimation(endTime);
                        for (var index = 0; index < animatorIK.ikData.Length; index++)
                        {
                            if (!animatorIK.ikData[index].enable) continue;
                            animatorIK.SynchroSet((AnimatorIKCore.IKTarget)index);
                            ikDataEndSave[index] = new IKDataSave(animatorIK.ikData[index]);
                        }
                    }

                    ResetAnimatorRootCorrection();
                    for (int frame = toolRangeIK_FirstFrame; frame <= toolRangeIK_LastFrame; frame++)
                    {
                        var time = uAw.GetFrameTime(frame, clip);
                        uAw.SetCurrentTime(time);

                        float rate = 0f;
                        if (toolRangeIK_LastFrame - toolRangeIK_FirstFrame > 0)
                            rate = (frame - toolRangeIK_FirstFrame) / (float)(toolRangeIK_LastFrame - toolRangeIK_FirstFrame);

#if UNITY_2017_1_OR_NEWER
                        if (uAw_2017_1.GetLinkedWithTimeline())
                        {
                            var director = uAw_2017_1.GetTimelineCurrentDirector();
                            var timellineTime = director.time;
                            var timelineWrap = director.extrapolationMode;
                            director.extrapolationMode = UnityEngine.Playables.DirectorWrapMode.None;
                            var befActive = vaw.gameObject.activeInHierarchy;
                            director.Evaluate();
                            director.time = timellineTime;
                            director.extrapolationMode = timelineWrap;
                        }
#endif

                        for (var index = 0; index < animatorIK.ikData.Length; index++)
                        {
                            if (!animatorIK.ikData[index].enable) continue;
                            var ikDataSave = IKDataSave.Lerp(ikDataBeginSave[index], ikDataEndSave[index], rate);
                            ikDataSave.Set(animatorIK.ikData[index]);
                            animatorIK.SetUpdateIKtargetAnimatorIK((AnimatorIKCore.IKTarget)index);
                        }

                        EnableAnimatorRootCorrection(time, time, time);
                        UpdateAnimatorRootCorrection();
                        ResetAnimatorRootCorrection();

                        animatorIK.UpdateIK(time, false);
                        animatorIK.SetUpdateIKtargetAll(false);
                    }
                    for (int frame = toolRangeIK_FirstFrame; frame <= toolRangeIK_LastFrame; frame++)
                    {
                        var time = uAw.GetFrameTime(frame, clip);
                        EnableAnimatorRootCorrection(time, time, time);
                    }
                    UpdateAnimatorRootCorrection();
                }
                #endregion
                #region OriginalIK
                EditorUtility.DisplayProgressBar("Range IK", "Original IK", progressIndex++ / (float)progressTotal);
                if (originalIK.ikData.Where(data => data.enable).ToArray().Length > 0)
                {
                    IKDataSave[] ikDataBeginSave = new IKDataSave[originalIK.ikData.Count];
                    IKDataSave[] ikDataEndSave = new IKDataSave[originalIK.ikData.Count];
                    {
                        ResampleAnimation(beginTime);
                        for (var index = 0; index < originalIK.ikData.Count; index++)
                        {
                            if (!originalIK.ikData[index].enable) continue;
                            originalIK.SynchroSet(index);
                            ikDataBeginSave[index] = new IKDataSave(originalIK.ikData[index]);
                        }
                        ResampleAnimation(endTime);
                        for (var index = 0; index < originalIK.ikData.Count; index++)
                        {
                            if (!originalIK.ikData[index].enable) continue;
                            originalIK.SynchroSet(index);
                            ikDataEndSave[index] = new IKDataSave(originalIK.ikData[index]);
                        }
                    }

                    for (int frame = toolRangeIK_FirstFrame; frame <= toolRangeIK_LastFrame; frame++)
                    {
                        var time = uAw.GetFrameTime(frame, clip);
                        uAw.SetCurrentTime(time);

                        float rate = 0f;
                        if (toolRangeIK_LastFrame - toolRangeIK_FirstFrame > 0)
                            rate = (frame - toolRangeIK_FirstFrame) / (float)(toolRangeIK_LastFrame - toolRangeIK_FirstFrame);

                        for (var index = 0; index < originalIK.ikData.Count; index++)
                        {
                            if (!originalIK.ikData[index].enable) continue;
                            var ikDataSave = IKDataSave.Lerp(ikDataBeginSave[index], ikDataEndSave[index], rate);
                            ikDataSave.Set(originalIK.ikData[index]);
                            originalIK.SetUpdateIKtargetOriginalIK(index);
                        }
                        
                        originalIK.UpdateIK(time);
                        originalIK.SetUpdateIKtargetAll(false);
                    }
                }
                #endregion
            }
            finally
            {
                uAw.SetCurrentTime(saveCurrentTime);
                EditorUtility.ClearProgressBar();
            }

            ToolsCommonAfter();
        }

        private void ToolsClearHumanoidIK(AnimationClip clip)
        {
            if (!ToolsCommonBefore(clip, "Clear IK Keyframe")) return;

            {
                var beginTime = uAw.SnapToFrame(toolHumanoidIK_FirstFrame >= 0 ? toolHumanoidIK_FirstFrame / clip.frameRate : 0f, clip.frameRate);
                var endTime = uAw.SnapToFrame(toolHumanoidIK_LastFrame >= 0 ? toolHumanoidIK_LastFrame / clip.frameRate : clip.length, clip.frameRate);
                float halfFrameTime = (0.5f / clip.frameRate) - 0.0001f;
                for (var ikIndex = (AnimatorIKIndex)0; ikIndex < AnimatorIKIndex.Total; ikIndex++)
                {
                    if (ikIndex == AnimatorIKIndex.LeftHand || ikIndex == AnimatorIKIndex.RightHand)
                    {
                        if (!toolHumanoidIK_Hand) continue;
                    }
                    else if (ikIndex == AnimatorIKIndex.LeftFoot || ikIndex == AnimatorIKIndex.RightFoot)
                    {
                        if (!toolHumanoidIK_Foot) continue;
                    }
                    else
                    {
                        continue;
                    }
                    Action<EditorCurveBinding> ClearHumanoidIK = (binding) =>
                    {
                        var curve = AnimationUtility.GetEditorCurve(clip, binding);
                        if (curve == null) return;
                        for (int i = curve.length - 1; i >= 0; i--)
                        {
                            if (curve[i].time >= beginTime - halfFrameTime && curve[i].time <= endTime + halfFrameTime)
                            {
                                curve.RemoveKey(i);
                            }
                        }
                        AnimationUtility.SetEditorCurve(clip, binding, curve.length > 0 ? curve : null);
                    };
                    for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                        ClearHumanoidIK(AnimationCurveBindingAnimatorIkT(ikIndex, dofIndex));
                    for (int dofIndex = 0; dofIndex < 4; dofIndex++)
                        ClearHumanoidIK(AnimationCurveBindingAnimatorIkQ(ikIndex, dofIndex));
                }
            }

            ToolsCommonAfter();
        }
        private void ToolsGenarateHumanoidIK(AnimationClip clip)
        {
            if (!isHuman || !clip.isHumanMotion) return;
            if (!ToolsCommonBefore(clip, "Genarate IK Keyframe")) return;

            calcObject.vaEdit.SetAnimationClip(clip);
            calcObject.vaEdit.SetIKPass(false);
            calcObject.SetOrigin();
            calcObject.AnimatorRebind();

            try
            {
                EditorUtility.DisplayProgressBar("Genarate IK Keyframe", "", 0f);
                var currentFrame = uAw.GetCurrentFrame();
                var beginTime = uAw.SnapToFrame(toolHumanoidIK_FirstFrame >= 0 ? toolHumanoidIK_FirstFrame / clip.frameRate : 0f, clip.frameRate);
                var endTime = uAw.SnapToFrame(toolHumanoidIK_LastFrame >= 0 ? toolHumanoidIK_LastFrame / clip.frameRate : clip.length, clip.frameRate);
                float halfFrameTime = (0.5f / clip.frameRate) - 0.0001f;
                for (var ikIndex = (AnimatorIKIndex)0; ikIndex < AnimatorIKIndex.Total; ikIndex++)
                {
                    EditorUtility.DisplayProgressBar("Genarate IK Keyframe", string.Format("{0} / {1}", (int)ikIndex, (int)AnimatorIKIndex.Total), (int)ikIndex / (float)AnimatorIKIndex.Total);
                    if (ikIndex == AnimatorIKIndex.LeftHand || ikIndex == AnimatorIKIndex.RightHand)
                    {
                        if (!toolHumanoidIK_Hand) continue;
                    }
                    else if (ikIndex == AnimatorIKIndex.LeftFoot || ikIndex == AnimatorIKIndex.RightFoot)
                    {
                        if (!toolHumanoidIK_Foot) continue;
                    }
                    else
                    {
                        continue;
                    }

                    AnimationCurve[] ikTCurves = new AnimationCurve[3];
                    AnimationCurve[] ikQCurves = new AnimationCurve[4];
                    {
                        for (int dofIndex = 0; dofIndex < ikTCurves.Length; dofIndex++)
                        {
                            ikTCurves[dofIndex] = AnimationUtility.GetEditorCurve(clip, AnimationCurveBindingAnimatorIkT((AnimatorIKIndex)ikIndex, dofIndex));
                            if (ikTCurves[dofIndex] == null)
                                ikTCurves[dofIndex] = new AnimationCurve();
                            else
                            {
                                for (int i = ikTCurves[dofIndex].length - 1; i >= 0; i--)
                                {
                                    if (ikTCurves[dofIndex][i].time >= beginTime - halfFrameTime && ikTCurves[dofIndex][i].time <= endTime + halfFrameTime)
                                    {
                                        ikTCurves[dofIndex].RemoveKey(i);
                                    }
                                }
                            }
                        }
                        for (int dofIndex = 0; dofIndex < ikQCurves.Length; dofIndex++)
                        {
                            ikQCurves[dofIndex] = AnimationUtility.GetEditorCurve(clip, AnimationCurveBindingAnimatorIkQ((AnimatorIKIndex)ikIndex, dofIndex));
                            if (ikQCurves[dofIndex] == null)
                                ikQCurves[dofIndex] = new AnimationCurve();
                            else
                            {
                                for (int i = ikQCurves[dofIndex].length - 1; i >= 0; i--)
                                {
                                    if (ikQCurves[dofIndex][i].time >= beginTime - halfFrameTime && ikQCurves[dofIndex][i].time <= endTime + halfFrameTime)
                                    {
                                        ikQCurves[dofIndex].RemoveKey(i);
                                    }
                                }
                            }
                        }
                    }
                    HashSet<float> keyTimes = new HashSet<float>();
                    #region KeyTimes
                    {
                        Action<HumanBodyBones> AddKeyTimes = null;
                        AddKeyTimes = (hi) =>
                        {
                            for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                            {
                                var mi = HumanTrait.MuscleFromBone((int)hi, dofIndex);
                                if (mi < 0) continue;
                                var curve = AnimationUtility.GetEditorCurve(clip, AnimationCurveBindingAnimatorMuscle(mi));
                                if (curve == null) continue;
                                for (int i = 0; i < curve.length; i++)
                                    keyTimes.Add(curve[i].time);
                            }
                            var phi = (HumanBodyBones)HumanTrait.GetParentBone((int)hi);
                            if (phi >= 0)
                            {
                                AddKeyTimes(phi);
                            }
                            else
                            {
                                for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                                {
                                    var curve = AnimationUtility.GetEditorCurve(clip, AnimationCurveBindingAnimatorRootT[dofIndex]);
                                    if (curve == null) continue;
                                    for (int i = 0; i < curve.length; i++)
                                        keyTimes.Add(curve[i].time);
                                }
                                for (int dofIndex = 0; dofIndex < 4; dofIndex++)
                                {
                                    var curve = AnimationUtility.GetEditorCurve(clip, AnimationCurveBindingAnimatorRootQ[dofIndex]);
                                    if (curve == null) continue;
                                    for (int i = 0; i < curve.length; i++)
                                        keyTimes.Add(curve[i].time);
                                }
                            }
                        };
                        keyTimes.Add(0f);
                        keyTimes.Add(clip.length);
                        AddKeyTimes(AnimatorIKIndex2HumanBodyBones[(int)ikIndex]);
                    }
                    #endregion
                    var humanoidIndex = AnimatorIKIndex2HumanBodyBones[(int)ikIndex];
                    var t = calcObject.humanoidBones[(int)humanoidIndex].transform;
                    var positionTable = new Dictionary<float, Vector3>();
                    var rotationTable = new Dictionary<float, Quaternion>();
                    #region KeyInfoTable
                    foreach (var time in keyTimes)
                    {
                        clip.SampleAnimation(calcObject.gameObject, time);
                        positionTable.Add(time, t.position);
                        rotationTable.Add(time, t.rotation);
                    }
                    #endregion
                    for (int frame = toolHumanoidIK_FirstFrame; frame <= toolHumanoidIK_LastFrame; frame++)
                    {
                        var time = uAw.GetFrameTime(frame, clip);
                        clip.SampleAnimation(calcObject.gameObject, time);
                        Vector3 position;
                        Quaternion rotation;
                        {
                            Vector3 positionL = Vector3.zero, positionR = Vector3.zero;
                            float nearL = float.MinValue, nearR = float.MaxValue;
                            foreach (var pair in positionTable)
                            {
                                if (pair.Key <= time && pair.Key > nearL)
                                {
                                    positionL = pair.Value;
                                    nearL = pair.Key;
                                }
                                if (pair.Key >= time && pair.Key < nearR)
                                {
                                    positionR = pair.Value;
                                    nearR = pair.Key;
                                }
                            }
                            var rate = nearR - nearL != 0f ? (time - nearL) / (nearR - nearL) : 0f;
                            position = Vector3.Lerp(positionL, positionR, rate);
                        }
                        {
                            Quaternion rotationL = Quaternion.identity, rotationR = Quaternion.identity;
                            float nearL = float.MinValue, nearR = float.MaxValue;
                            foreach (var pair in rotationTable)
                            {
                                if (pair.Key <= time && pair.Key > nearL)
                                {
                                    rotationL = pair.Value;
                                    nearL = pair.Key;
                                }
                                if (pair.Key >= time && pair.Key < nearR)
                                {
                                    rotationR = pair.Value;
                                    nearR = pair.Key;
                                }
                            }
                            var rate = nearR - nearL != 0f ? (time - nearL) / (nearR - nearL) : 0f;
                            rotation = Quaternion.Slerp(rotationL, rotationR, rate);
                        }
                        Vector3 ikT = position;
                        Quaternion ikQ = rotation;
                        {
                            var rootT = GetAnimationValueAnimatorRootT(time);
                            var rootQ = GetAnimationValueAnimatorRootQ(time);
                            var post = uAvatar.GetPostRotation(calcObject.animator.avatar, (int)humanoidIndex);
                            #region IkT
                            if (ikIndex == AnimatorIKIndex.LeftFoot || ikIndex == AnimatorIKIndex.RightFoot)
                            {
                                Vector3 add = Vector3.zero;
                                switch (ikIndex)
                                {
                                case AnimatorIKIndex.LeftFoot: add.x += calcObject.animator.leftFeetBottomHeight; break;
                                case AnimatorIKIndex.RightFoot: add.x += calcObject.animator.rightFeetBottomHeight; break;
                                }
                                ikT += (rotation * post) * add;
                            }
                            ikT = calcObject.gameObjectTransform.worldToLocalMatrix.MultiplyPoint3x4(ikT) - (rootT * calcObject.animator.humanScale);
                            ikT = Quaternion.Inverse(rootQ) * ikT;
                            ikT *= 1f / calcObject.animator.humanScale;
                            #endregion
                            #region IkQ
                            ikQ = Quaternion.Inverse(calcObject.gameObjectTransform.rotation * rootQ) * (rotation * post);
                            #endregion
                        }
                        for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                        {
                            AddKeyframe(ikTCurves[dofIndex], time, ikT[dofIndex]);
                        }
                        for (int dofIndex = 0; dofIndex < 4; dofIndex++)
                        {
                            AddKeyframe(ikQCurves[dofIndex], time, ikQ[dofIndex]);
                        }
                    }
                    for (int dofIndex = 0; dofIndex < ikTCurves.Length; dofIndex++)
                        AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorIkT(ikIndex, dofIndex), ikTCurves[dofIndex]);
                    for (int dofIndex = 0; dofIndex < ikQCurves.Length; dofIndex++)
                        AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorIkQ(ikIndex, dofIndex), ikQCurves[dofIndex]);
                }
                uAw.SetCurrentFrame(currentFrame);
            }
            finally
            {
                calcObject.SetOutside();
                EditorUtility.ClearProgressBar();
            }

            ToolsCommonAfter();
        }
        private void ToolsCopy(AnimationClip clip)
        {
            if (!ToolsCommonBefore(clip, "Copy Keyframe")) return;

            try
            {
                var bindings = AnimationUtility.GetCurveBindings(clip);
                var rbindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                var events = AnimationUtility.GetAnimationEvents(clip);
                var beginTime = uAw.SnapToFrame(toolCopy_FirstFrame / clip.frameRate, clip.frameRate);
                var endTime = uAw.SnapToFrame(toolCopy_LastFrame / clip.frameRate, clip.frameRate);
                var writeBeginTime = uAw.SnapToFrame(toolCopy_WriteFrame / clip.frameRate, clip.frameRate);
                var writeEndTime = writeBeginTime + (endTime - beginTime);
                float halfFrameTime = (0.5f / clip.frameRate) - 0.0001f;
                int progressIndex = 0;
                int progressTotal = 3;

                EditorUtility.DisplayProgressBar("Copy Keyframe", "Read", progressIndex++ / (float)progressTotal);
                if (writeBeginTime > clip.length)
                {
                    #region AddLastLinerKey
                    foreach (var binding in bindings)
                    {
                        var curve = AnimationUtility.GetEditorCurve(clip, binding);
                        var keyIndex = FindKeyframeAtTime(curve, clip.length);
                        if (keyIndex < 0)
                        {
                            keyIndex = AddKeyframe(curve, clip.length, curve.Evaluate(clip.length));
                            AnimationUtility.SetKeyLeftTangentMode(curve, keyIndex, AnimationUtility.TangentMode.Linear);
                            if (keyIndex > 0)
                                AnimationUtility.SetKeyRightTangentMode(curve, keyIndex - 1, AnimationUtility.TangentMode.Linear);
                            AnimationUtility.SetEditorCurve(clip, binding, curve);
                        }
                    }
                    #endregion
                }
                
                EditorUtility.DisplayProgressBar("Copy Keyframe", "Read", progressIndex++ / (float)progressTotal);
                #region CurveBindings
                List<Keyframe>[] curveCopyKeyframes = new List<Keyframe>[bindings.Length];
                List<int>[] markKeyIndexes = new List<int>[bindings.Length];
                for (int i = 0; i < bindings.Length; i++)
                {
                    curveCopyKeyframes[i] = new List<Keyframe>();
                    markKeyIndexes[i] = new List<int>();
                    bool update = false;
                    var curve = AnimationUtility.GetEditorCurve(clip, bindings[i]);
                    if (curveCopyKeyframes[i].FindIndex((x) => Mathf.Approximately(uAw.SnapToFrame(x.time, clip.frameRate), uAw.SnapToFrame(writeBeginTime, clip.frameRate))) < 0)
                    {
                        var key = new Keyframe(writeBeginTime, curve.Evaluate(beginTime));
                        markKeyIndexes[i].Add(curveCopyKeyframes[i].Count);
                        curveCopyKeyframes[i].Add(key);
                    }
                    if (curveCopyKeyframes[i].FindIndex((x) => Mathf.Approximately(uAw.SnapToFrame(x.time, clip.frameRate), uAw.SnapToFrame(writeEndTime, clip.frameRate))) < 0)
                    {
                        var key = new Keyframe(writeEndTime, curve.Evaluate(endTime));
                        markKeyIndexes[i].Add(curveCopyKeyframes[i].Count);
                        curveCopyKeyframes[i].Add(key);
                    }
                    for (int j = 0; j < curve.length; j++)
                    {
                        if (curve[j].time >= beginTime - halfFrameTime && curve[j].time <= endTime + halfFrameTime)
                        {
                            var key = curve[j];
                            key.time = uAw.SnapToFrame(writeBeginTime + (key.time - beginTime), clip.frameRate);
                            curveCopyKeyframes[i].Add(key);
                        }
                        if (curve[j].time > writeBeginTime + halfFrameTime && curve[j].time < writeEndTime - halfFrameTime)
                        {
                            curve.RemoveKey(j--);
                            update = true;
                        }
                    }
                    {
                        Action<int> ActionAddKeyframe = (frame) =>
                        {
                            var setTime = uAw.GetFrameTime(frame, clip);
                            var keyIndex = FindKeyframeAtTime(curve, setTime);
                            if (keyIndex < 0)
                            {
                                AddKeyframe(curve, setTime, curve.Evaluate(setTime));
                                update = true;
                            }
                        };
                        if (toolCopy_WriteFrame < toolCopy_LastFrame)
                        {
                            ActionAddKeyframe(toolCopy_WriteFrame);
                        }
                        if (toolCopy_WriteFrame + (toolCopy_LastFrame - toolCopy_FirstFrame) > toolCopy_FirstFrame)
                        {
                            ActionAddKeyframe(toolCopy_WriteFrame + (toolCopy_LastFrame - toolCopy_FirstFrame));
                        }
                    }
                    if (update)
                        AnimationUtility.SetEditorCurve(clip, bindings[i], curve);
                }
                #endregion
                #region ObjectReferenceCurveBindings
                List<ObjectReferenceKeyframe>[] rcurveCopyKeyframes = new List<ObjectReferenceKeyframe>[rbindings.Length];
                for (int i = 0; i < rbindings.Length; i++)
                {
                    rcurveCopyKeyframes[i] = new List<ObjectReferenceKeyframe>();
                    bool update = false;
                    var keys = new List<ObjectReferenceKeyframe>(AnimationUtility.GetObjectReferenceCurve(clip, rbindings[i]));
                    for (int j = 0; j < keys.Count; j++)
                    {
                        if (keys[j].time >= beginTime - halfFrameTime && keys[j].time <= endTime + halfFrameTime)
                        {
                            var key = keys[j];
                            key.time = uAw.SnapToFrame(writeBeginTime + (key.time - beginTime), clip.frameRate);
                            rcurveCopyKeyframes[i].Add(key);
                        }
                        if (keys[j].time > writeBeginTime + halfFrameTime && keys[j].time < writeEndTime - halfFrameTime)
                        {
                            keys.RemoveAt(j--);
                            update = true;
                        }
                    }
                    if (update)
                        AnimationUtility.SetObjectReferenceCurve(clip, rbindings[i], keys.ToArray());
                }
                #endregion
                #region AnimationEvents
                List<AnimationEvent> newEvents = new List<AnimationEvent>();
                for (int i = 0; i < events.Length; i++)
                {
                    if (events[i].time >= beginTime - halfFrameTime && events[i].time <= endTime + halfFrameTime)
                    {
                        var key = new AnimationEvent()
                        {
                            stringParameter = events[i].stringParameter,
                            floatParameter = events[i].floatParameter,
                            intParameter = events[i].intParameter,
                            objectReferenceParameter = events[i].objectReferenceParameter,
                            functionName = events[i].functionName,
                            time = writeBeginTime + (events[i].time - beginTime),
                            messageOptions = events[i].messageOptions,
                        };
                        newEvents.Add(key);
                    }
                    if (events[i].time < writeBeginTime - halfFrameTime || events[i].time > writeEndTime + halfFrameTime)
                    {
                        newEvents.Add(events[i]);
                    }
                }
                newEvents.Sort((x, y) =>
                {
                    if (x.time > y.time) return 1;
                    else if (x.time < y.time) return -1;
                    else return 0;
                });
                #endregion
                
                EditorUtility.DisplayProgressBar("Copy Keyframe", "Write", progressIndex++ / (float)progressTotal);
                #region CurveBindings
                for (int i = 0; i < bindings.Length; i++)
                {
                    if (curveCopyKeyframes[i] == null || curveCopyKeyframes[i].Count <= 0) continue;
                    var curve = AnimationUtility.GetEditorCurve(clip, bindings[i]);
                    for (int j = 0; j < curveCopyKeyframes[i].Count; j++)
                    {
                        if (markKeyIndexes[i].Contains(j))
                        {
                            SetKeyframe(curve, curveCopyKeyframes[i][j].time, curveCopyKeyframes[i][j].value);
                        }
                        else
                        {
                            var index = SetKeyframe(curve, curveCopyKeyframes[i][j]);
                            curve.MoveKey(index, curveCopyKeyframes[i][j]);
                        }
                    }
                    AnimationUtility.SetEditorCurve(clip, bindings[i], curve);
                }
                #endregion
                #region ObjectReferenceCurveBindings
                for (int i = 0; i < rbindings.Length; i++)
                {
                    if (rcurveCopyKeyframes[i] == null || rcurveCopyKeyframes[i].Count <= 0) continue;
                    var keys = new List<ObjectReferenceKeyframe>(AnimationUtility.GetObjectReferenceCurve(clip, rbindings[i]));
                    for (int j = 0; j < rcurveCopyKeyframes[i].Count; j++)
                    {
                        var keyIndex = FindKeyframeAtTime(keys, rcurveCopyKeyframes[i][j].time);
                        if (keyIndex >= 0)
                        {
                            keys[keyIndex] = rcurveCopyKeyframes[i][j];
                        }
                        else
                        {
                            keys.Add(rcurveCopyKeyframes[i][j]);
                        }
                    }
                    AnimationUtility.SetObjectReferenceCurve(clip, rbindings[i], keys.ToArray());
                }
                #endregion
                #region AnimationEvents
                AnimationUtility.SetAnimationEvents(clip, newEvents.ToArray());
                #endregion
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            ToolsCommonAfter();
        }
        private void ToolsTrim(AnimationClip clip)
        {
            if (!ToolsCommonBefore(clip, "Trim Keyframe")) return;

            try
            {
                var bindings = AnimationUtility.GetCurveBindings(clip);
                var rbindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                var events = AnimationUtility.GetAnimationEvents(clip);
                var beginTime = uAw.SnapToFrame(toolTrim_FirstFrame / clip.frameRate, clip.frameRate);
                var endTime = uAw.SnapToFrame(toolTrim_LastFrame / clip.frameRate, clip.frameRate);
                float halfFrameTime = (0.5f / clip.frameRate) - 0.0001f;
                int progressIndex = 0;
                int progressTotal = bindings.Length * 3 + rbindings.Length + events.Length;
                {
                    AnimationCurve[] curves = new AnimationCurve[bindings.Length];
                    for (int i = 0; i < bindings.Length; i++)
                    {
                        EditorUtility.DisplayProgressBar("Trim", string.IsNullOrEmpty(bindings[i].path) ? bindings[i].propertyName : bindings[i].path, progressIndex++ / (float)progressTotal);
                        var curve = AnimationUtility.GetEditorCurve(clip, bindings[i]);
                        {
                            var keys = new List<Keyframe>();
                            for (int j = 0; j < curve.length; j++)
                            {
                                if (curve[j].time < beginTime - halfFrameTime || curve[j].time > endTime + halfFrameTime) continue;
                                var tmp = curve[j];
                                tmp.time = uAw.SnapToFrame(tmp.time - beginTime, clip.frameRate);
                                keys.Add(tmp);
                            }
                            if (keys.FindIndex((x) => Mathf.Approximately(x.time, 0f)) < 0)
                            {
                                keys.Insert(0, new Keyframe(0, curve.Evaluate(beginTime)));
                            }
                            if (keys.FindIndex((x) => Mathf.Approximately(x.time, endTime - beginTime)) < 0)
                            {
                                keys.Add(new Keyframe(endTime - beginTime, curve.Evaluate(endTime)));
                            }
                            curve.keys = keys.ToArray();
                        }
                        curves[i] = curve;
                    }
                    for (int i = 0; i < bindings.Length; i++)
                    {
                        EditorUtility.DisplayProgressBar("Trim", string.IsNullOrEmpty(bindings[i].path) ? bindings[i].propertyName : bindings[i].path, progressIndex++ / (float)progressTotal);
                        AnimationUtility.SetEditorCurve(clip, bindings[i], null);
                    }
                    for (int i = 0; i < bindings.Length; i++)
                    {
                        EditorUtility.DisplayProgressBar("Trim", string.IsNullOrEmpty(bindings[i].path) ? bindings[i].propertyName : bindings[i].path, progressIndex++ / (float)progressTotal);
                        AnimationUtility.SetEditorCurve(clip, bindings[i], curves[i]);
                    }
                }
                for (int i = 0; i < rbindings.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Trim", string.IsNullOrEmpty(rbindings[i].path) ? rbindings[i].propertyName : rbindings[i].path, progressIndex++ / (float)progressTotal);
                    var rkeys = AnimationUtility.GetObjectReferenceCurve(clip, rbindings[i]);
                    var keys = new List<ObjectReferenceKeyframe>();
                    foreach (var key in rkeys)
                    {
                        if (key.time < beginTime - halfFrameTime || key.time > endTime + halfFrameTime) continue;
                        var tmp = key;
                        tmp.time = uAw.SnapToFrame(tmp.time - beginTime, clip.frameRate);
                        keys.Add(tmp);
                    }
                    if (keys.FindIndex((x) => Mathf.Approximately(x.time, 0f)) < 0)
                    {
                        var nearIndex = FindBeforeNearKeyframeAtTime(rkeys, beginTime);
                        keys.Insert(0, new ObjectReferenceKeyframe() { time = 0f, value = rkeys[nearIndex].value });
                    }
                    AnimationUtility.SetObjectReferenceCurve(clip, rbindings[i], keys.ToArray());
                }
                {
                    List<AnimationEvent> newEvents = new List<AnimationEvent>(events.Length);
                    for (int i = 0; i < events.Length; i++)
                    {
                        EditorUtility.DisplayProgressBar("Trim", events[i].functionName, progressIndex++ / (float)progressTotal);
                        if (events[i].time < beginTime - halfFrameTime || events[i].time > endTime + halfFrameTime) continue;
                        var tmp = events[i];
                        tmp.time = uAw.SnapToFrame(tmp.time - beginTime, clip.frameRate);
                        newEvents.Add(tmp);
                    }
                    AnimationUtility.SetAnimationEvents(clip, newEvents.ToArray());
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            ToolsCommonAfter();
        }
        private enum ToolsCreateNewClipMode
        {
            Blank,
            Duplicate,
            Mirror,
        }
        private void ToolsCreateNewClip(string clipPath, ToolsCreateNewClipMode mode)
        {
            VeryAnimationWindow.CustomAssetModificationProcessor.Pause();
            var clip = uAnimationWindowUtility.CreateNewClipAtPath(clipPath);
            VeryAnimationWindow.CustomAssetModificationProcessor.Resume();

            if (!ToolsCommonBefore(clip, "Create new clip")) return;

            try
            {
                int progressIndex = 0;
                int progressTotal = 1;
                EditorUtility.DisplayProgressBar("Create", clipPath, progressIndex++ / (float)progressTotal);

                AnimationUtility.SetAnimationClipSettings(clip, AnimationUtility.GetAnimationClipSettings(currentClip));
                {
                    clip.frameRate = currentClip.frameRate;
                    clip.wrapMode = currentClip.wrapMode;
                    clip.localBounds = currentClip.localBounds;
                    clip.legacy = currentClip.legacy;
                }
                if (mode == ToolsCreateNewClipMode.Duplicate || mode == ToolsCreateNewClipMode.Mirror)
                {
                    #region Duplicate
                    foreach (var binding in AnimationUtility.GetCurveBindings(currentClip))
                    {
                        AnimationUtility.SetEditorCurve(clip, binding, AnimationUtility.GetEditorCurve(currentClip, binding));
                    }
                    foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(currentClip))
                    {
                        AnimationUtility.SetObjectReferenceCurve(clip, binding, AnimationUtility.GetObjectReferenceCurve(currentClip, binding));
                    }
                    AnimationUtility.SetAnimationEvents(clip, AnimationUtility.GetAnimationEvents(currentClip));
                    #endregion
                }
                if (mode == ToolsCreateNewClipMode.Mirror)
                {
                    #region Mirror
                    {
                        #region SwapMirrorCurve
                        {
                            var bindings = AnimationUtility.GetCurveBindings(clip);
                            Func<EditorCurveBinding, int> GetMirrorBindingIndex = (binding) =>
                            {
                                var mbinding = GetMirrorAnimationCurveBinding(binding);
                                if (!mbinding.HasValue) return -1;
                                return EditorCommon.ArrayIndexOf(bindings, mbinding.Value);
                            };
                            #region CreateMirrorCurve
                            foreach (var binding in bindings)
                            {
                                var mbinding = GetMirrorAnimationCurveBinding(binding);
                                if (!mbinding.HasValue) continue;
                                if (!EditorCommon.ArrayContains(bindings, mbinding.Value))
                                {
                                    var mcurve = new AnimationCurve();
                                    Action<float> AddKey = (value) =>
                                    {
                                        AddKeyframe(mcurve, 0, value);
                                        AddKeyframe(mcurve, clip.length, value);
                                    };
                                    int dofIndex = GetDOFIndexFromCurveBinding(mbinding.Value);
                                    if (mbinding.Value.type == typeof(Animator))
                                    {
                                        AddKey(0f);
                                    }
                                    else if (mbinding.Value.type == typeof(Transform))
                                    {
                                        var boneIndex = GetBoneIndexFromCurveBinding(mbinding.Value);
                                        if (IsTransformPositionCurveBinding(mbinding.Value))
                                            AddKey(boneSaveTransforms[boneIndex].localPosition[dofIndex]);
                                        else if (IsTransformRotationCurveBinding(mbinding.Value))
                                        {
                                            if (uRotationCurveInterpolation.GetModeFromCurveData(mbinding.Value) == URotationCurveInterpolation.Mode.RawEuler)
                                                AddKey(boneSaveTransforms[boneIndex].localRotation.eulerAngles[dofIndex]);
                                            else if (uRotationCurveInterpolation.GetModeFromCurveData(mbinding.Value) == URotationCurveInterpolation.Mode.RawQuaternions)
                                                AddKey(boneSaveTransforms[boneIndex].localRotation[dofIndex]);
                                            else
                                                Assert.IsTrue(false);
                                        }
                                        else if (IsTransformScaleCurveBinding(mbinding.Value))
                                            AddKey(boneSaveTransforms[boneIndex].localScale[dofIndex]);
                                    }
                                    else if (IsSkinnedMeshRendererBlendShapeCurveBinding(mbinding.Value))
                                    {
                                        var boneIndex = GetBoneIndexFromCurveBinding(mbinding.Value);
                                        var renderer = bones[boneIndex].GetComponent<SkinnedMeshRenderer>();
                                        var name = PropertyName2BlendShapeName(mbinding.Value.propertyName);
                                        AddKey(blendShapeWeightSave.GetDefaultWeight(renderer, name));
                                    }
                                    else
                                    {
                                        Assert.IsTrue(false);
                                    }
                                    AnimationUtility.SetEditorCurve(clip, mbinding.Value, mcurve);
                                }
                            }
                            bindings = AnimationUtility.GetCurveBindings(clip);
                            #endregion

                            #region MirrorCurve
                            {
                                Action<int, int> SwapCurve = (indexA, indexB) =>
                                {
                                    var curveA = AnimationUtility.GetEditorCurve(clip, bindings[indexA]);
                                    var curveB = AnimationUtility.GetEditorCurve(clip, bindings[indexB]);
                                    AnimationUtility.SetEditorCurve(clip, bindings[indexB], curveA);
                                    AnimationUtility.SetEditorCurve(clip, bindings[indexA], curveB);
                                };
                                Action<int> MirrorCurve = (index) =>
                                {
                                    var curve = AnimationUtility.GetEditorCurve(clip, bindings[index]);
                                    for (int i = 0; i < curve.length; i++)
                                    {
                                        var key = curve[i];
                                        key.value = -key.value;
                                        key.inTangent = -key.inTangent;
                                        key.outTangent = -key.outTangent;
                                        curve.MoveKey(i, key);
                                    }
                                    AnimationUtility.SetEditorCurve(clip, bindings[index], curve);
                                };

                                bool[] doneFlag = new bool[bindings.Length];
                                for (int i = 0; i < bindings.Length; i++)
                                {
                                    if (doneFlag[i]) continue;
                                    doneFlag[i] = true;
                                    if (bindings[i].type == typeof(Animator))
                                    {
                                        #region Animator
                                        AnimatorIKIndex ikIndex = AnimatorIKIndex.None;
                                        AnimatorTDOFIndex tdofIndex = AnimatorTDOFIndex.None;
                                        var muscleIndex = GetMuscleIndexFromCurveBinding(bindings[i]);
                                        if (muscleIndex >= 0)
                                        {
                                            #region Muscle
                                            var mirrorMuscleIndex = GetMirrorMuscleIndex(muscleIndex);
                                            if (mirrorMuscleIndex >= 0)
                                            {
                                                var mirrorBindingIndex = GetMirrorBindingIndex(bindings[i]);
                                                doneFlag[mirrorBindingIndex] = true;
                                                SwapCurve(i, mirrorBindingIndex);
                                            }
                                            else if (muscleIndex == HumanTrait.MuscleFromBone(HumanTrait.BoneFromMuscle(muscleIndex), 0) ||
                                                    muscleIndex == HumanTrait.MuscleFromBone(HumanTrait.BoneFromMuscle(muscleIndex), 1))
                                            {
                                                MirrorCurve(i);
                                            }
                                            #endregion
                                        }
                                        else if (bindings[i].propertyName == AnimationCurveBindingAnimatorRootT[0].propertyName ||
                                                bindings[i].propertyName == AnimationCurveBindingAnimatorRootQ[1].propertyName ||
                                                bindings[i].propertyName == AnimationCurveBindingAnimatorRootQ[2].propertyName)
                                        {
                                            #region Root
                                            MirrorCurve(i);
                                            #endregion
                                        }
                                        else if ((ikIndex = GetIkTIndexFromCurveBinding(bindings[i])) != AnimatorIKIndex.None)
                                        {
                                            #region IKT
                                            var mirrorBindingIndex = GetMirrorBindingIndex(bindings[i]);
                                            if (mirrorBindingIndex >= 0)
                                            {
                                                doneFlag[mirrorBindingIndex] = true;
                                                var dofIndex = GetDOFIndexFromCurveBinding(bindings[i]);
                                                if (dofIndex == 0)
                                                {
                                                    MirrorCurve(i);
                                                }
                                                SwapCurve(i, mirrorBindingIndex);
                                                if (dofIndex == 0)
                                                {
                                                    MirrorCurve(i);
                                                }
                                            }
                                            #endregion
                                        }
                                        else if ((ikIndex = GetIkQIndexFromCurveBinding(bindings[i])) != AnimatorIKIndex.None)
                                        {
                                            #region IKQ
                                            EditorCurveBinding[] ikQBindings = new EditorCurveBinding[4];
                                            int[] bindingIndexes = new int[4];
                                            int[] mirrorBindingIndexes = new int[4];
                                            for (int dof = 0; dof < ikQBindings.Length; dof++)
                                            {
                                                ikQBindings[dof] = AnimationCurveBindingAnimatorIkQ(ikIndex, dof);
                                                bindingIndexes[dof] = ArrayUtility.FindIndex(bindings, x => x.propertyName == ikQBindings[dof].propertyName);
                                                string mirrorPropertyName = null;
                                                if (ikQBindings[dof].propertyName.IndexOf("Left") >= 0)
                                                    mirrorPropertyName = ikQBindings[dof].propertyName.Replace("Left", "Right");
                                                else if (ikQBindings[dof].propertyName.IndexOf("Right") >= 0)
                                                    mirrorPropertyName = ikQBindings[dof].propertyName.Replace("Right", "Left");
                                                Assert.IsNotNull(mirrorPropertyName);
                                                mirrorBindingIndexes[dof] = ArrayUtility.FindIndex(bindings, x => x.propertyName == mirrorPropertyName);
                                            }
                                            if (bindingIndexes[0] >= 0 && bindingIndexes[1] >= 0 && bindingIndexes[2] >= 0 && bindingIndexes[3] >= 0 &&
                                                mirrorBindingIndexes[0] >= 0 && mirrorBindingIndexes[1] >= 0 && mirrorBindingIndexes[2] >= 0 && mirrorBindingIndexes[3] >= 0)
                                            {
                                                for (int dof = 0; dof < ikQBindings.Length; dof++)
                                                {
                                                    SwapCurve(bindingIndexes[dof], mirrorBindingIndexes[QuaternionXMirrorSwapDof[dof]]);
                                                    doneFlag[bindingIndexes[dof]] = true;
                                                    doneFlag[mirrorBindingIndexes[QuaternionXMirrorSwapDof[dof]]] = true;
                                                }
                                            }
                                            #endregion
                                        }
                                        else if ((tdofIndex = GetTDOFIndexFromCurveBinding(bindings[i])) != AnimatorTDOFIndex.None)
                                        {
                                            #region TDOF
                                            var dofIndex = GetDOFIndexFromCurveBinding(bindings[i]);
                                            var mirrortdofIndex = AnimatorTDOFMirrorIndexes[(int)tdofIndex];
                                            if (mirrortdofIndex == AnimatorTDOFIndex.None)
                                            {
                                                if (dofIndex == 2)
                                                {
                                                    MirrorCurve(i);
                                                }
                                            }
                                            else
                                            {
                                                var mirrorBindingIndex = GetMirrorBindingIndex(bindings[i]);
                                                if (mirrorBindingIndex >= 0)
                                                {
                                                    doneFlag[mirrorBindingIndex] = true;
                                                    if (dofIndex == 2)
                                                    {
                                                        MirrorCurve(i);
                                                    }
                                                    SwapCurve(i, mirrorBindingIndex);
                                                    if (dofIndex == 2)
                                                    {
                                                        MirrorCurve(i);
                                                    }
                                                }
                                            }
                                            #endregion
                                        }
                                        #endregion
                                    }
                                    else if (IsSkinnedMeshRendererBlendShapeCurveBinding(bindings[i]))
                                    {
                                        #region BlendShape
                                        var boneIndex = GetBoneIndexFromCurveBinding(bindings[i]);
                                        if (boneIndex >= 0)
                                        {
                                            var mirrorBindingIndex = GetMirrorBindingIndex(bindings[i]);
                                            if (mirrorBindingIndex >= 0)
                                            {
                                                doneFlag[mirrorBindingIndex] = true;
                                                SwapCurve(i, mirrorBindingIndex);
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                            #endregion
                        }
                        #endregion

                        #region FullBakeKeyframe
                        {
                            var curves = new Dictionary<EditorCurveBinding, AnimationCurve>();
                            Func<EditorCurveBinding, AnimationCurve> GetCurve = (binding) =>
                            {
                                AnimationCurve curve;
                                if (!curves.TryGetValue(binding, out curve))
                                {
                                    curve = AnimationUtility.GetEditorCurve(clip, binding);
                                    if (curve == null)
                                        curve = new AnimationCurve();
                                    curves.Add(binding, curve);
                                }
                                return curve;
                            };

                            Quaternion[] boneWroteRotation = new Quaternion[editBones.Length];
                            Vector3[] boneWroteEuler = new Vector3[editBones.Length];
                            for (int boneIndex = 0; boneIndex < editBones.Length; boneIndex++)
                            {
                                boneWroteRotation[boneIndex] = boneSaveOriginalTransforms[boneIndex].localRotation;
                                boneWroteEuler[boneIndex] = boneWroteRotation[boneIndex].eulerAngles;
                            }
                            for (int frame = 0; frame <= GetLastFrame(); frame++)
                            {
                                var time = GetFrameTime(frame);
                                #region Generic
                                {
                                    for (int boneIndex = 0; boneIndex < editBones.Length; boneIndex++)
                                    {
                                        if (isHuman && humanoidConflict[boneIndex]) continue;
                                        if (boneIndex == rootMotionBoneIndex) continue;
                                        Vector3 position;
                                        Quaternion rotation;
                                        Vector3 scale;
                                        Vector3 positionMirrorOriginal;
                                        Quaternion rotationMirrorOriginal;
                                        Vector3 scaleMirrorOriginal;
                                        var mbi = mirrorBoneIndexes[boneIndex];
                                        if (mbi >= 0)
                                        {
                                            position = GetMirrorBoneLocalPosition(mbi, GetAnimationValueTransformPosition(mbi, time));
                                            rotation = GetMirrorBoneLocalRotation(mbi, GetAnimationValueTransformRotation(mbi, time));
                                            scale = GetMirrorBoneLocalScale(mbi, GetAnimationValueTransformScale(mbi, time));
                                            positionMirrorOriginal = GetMirrorBoneLocalPosition(mbi, boneSaveTransforms[mbi].localPosition);
                                            rotationMirrorOriginal = GetMirrorBoneLocalRotation(mbi, boneSaveTransforms[mbi].localRotation);
                                            scaleMirrorOriginal = GetMirrorBoneLocalScale(mbi, boneSaveTransforms[mbi].localScale);
                                        }
                                        else
                                        {
                                            position = GetMirrorBoneLocalPosition(boneIndex, GetAnimationValueTransformPosition(boneIndex, time));
                                            rotation = GetMirrorBoneLocalRotation(boneIndex, GetAnimationValueTransformRotation(boneIndex, time));
                                            scale = GetMirrorBoneLocalScale(boneIndex, GetAnimationValueTransformScale(boneIndex, time));
                                            positionMirrorOriginal = GetMirrorBoneLocalPosition(boneIndex, boneSaveTransforms[boneIndex].localPosition);
                                            rotationMirrorOriginal = GetMirrorBoneLocalRotation(boneIndex, boneSaveTransforms[boneIndex].localRotation);
                                            scaleMirrorOriginal = GetMirrorBoneLocalScale(boneIndex, boneSaveTransforms[boneIndex].localScale);
                                        }
                                        bool positionMirrorDifferent = false;
                                        bool rotationMirrorDifferent = false;
                                        bool scaleMirrorDifferent = false;
                                        {
                                            positionMirrorDifferent = Mathf.Abs(positionMirrorOriginal.x - boneSaveOriginalTransforms[boneIndex].localPosition.x) >= TransformPositionApproximatelyThreshold ||
                                                                        Mathf.Abs(positionMirrorOriginal.y - boneSaveOriginalTransforms[boneIndex].localPosition.y) >= TransformPositionApproximatelyThreshold ||
                                                                        Mathf.Abs(positionMirrorOriginal.z - boneSaveOriginalTransforms[boneIndex].localPosition.z) >= TransformPositionApproximatelyThreshold; ;
                                            {
                                                var eulerAngles = rotationMirrorOriginal.eulerAngles;
                                                var originalEulerAngles = boneSaveTransforms[boneIndex].localRotation.eulerAngles;
                                                rotationMirrorDifferent = Mathf.Abs(eulerAngles.x - originalEulerAngles.x) >= TransformRotationApproximatelyThreshold ||
                                                                            Mathf.Abs(eulerAngles.y - originalEulerAngles.y) >= TransformRotationApproximatelyThreshold ||
                                                                            Mathf.Abs(eulerAngles.z - originalEulerAngles.z) >= TransformRotationApproximatelyThreshold;
                                            }
                                            scaleMirrorDifferent = Mathf.Abs(scaleMirrorOriginal.x - boneSaveOriginalTransforms[boneIndex].localScale.x) >= TransformScaleApproximatelyThreshold ||
                                                                    Mathf.Abs(scaleMirrorOriginal.y - boneSaveOriginalTransforms[boneIndex].localScale.y) >= TransformScaleApproximatelyThreshold ||
                                                                    Mathf.Abs(scaleMirrorOriginal.z - boneSaveOriginalTransforms[boneIndex].localScale.z) >= TransformScaleApproximatelyThreshold;
                                        }
                                        if (IsHaveAnimationCurveTransformPosition(boneIndex) || IsHaveAnimationCurveTransformPosition(mbi) || positionMirrorDifferent)
                                        {
                                            for (int dof = 0; dof < 3; dof++)
                                            {
                                                var curve = GetCurve(AnimationCurveBindingTransformPosition(boneIndex, dof));
                                                SetKeyframe(curve, time, position[dof]);
                                            }
                                        }
                                        {
                                            var rotationMode = IsHaveAnimationCurveTransformRotation(boneIndex);
                                            var mrotationMode = IsHaveAnimationCurveTransformRotation(mbi);
                                            if (rotationMode != URotationCurveInterpolation.Mode.Undefined || mrotationMode != URotationCurveInterpolation.Mode.Undefined || rotationMirrorDifferent)
                                            {
                                                if (rotationMode == URotationCurveInterpolation.Mode.RawEuler)
                                                {
                                                    var eulerAngles = rotation.eulerAngles;
                                                    eulerAngles = FixReverseRotationEuler(eulerAngles, boneWroteEuler[boneIndex]);
                                                    boneWroteEuler[boneIndex] = eulerAngles;
                                                    for (int dof = 0; dof < 3; dof++)
                                                    {
                                                        var curve = GetCurve(AnimationCurveBindingTransformRotation(boneIndex, dof, URotationCurveInterpolation.Mode.RawEuler));
                                                        SetKeyframe(curve, time, eulerAngles[dof]);
                                                    }
                                                }
                                                else
                                                {
                                                    rotation = FixReverseRotationQuaternion(rotation, boneWroteRotation[boneIndex]);
                                                    boneWroteRotation[boneIndex] = rotation;
                                                    for (int dof = 0; dof < 4; dof++)
                                                    {
                                                        var curve = GetCurve(AnimationCurveBindingTransformRotation(boneIndex, dof, URotationCurveInterpolation.Mode.RawQuaternions));
                                                        SetKeyframe(curve, time, rotation[dof]);
                                                    }
                                                }
                                            }
                                        }
                                        if (IsHaveAnimationCurveTransformScale(boneIndex) || IsHaveAnimationCurveTransformScale(mbi) || scaleMirrorDifferent)
                                        {
                                            for (int dof = 0; dof < 3; dof++)
                                            {
                                                var curve = GetCurve(AnimationCurveBindingTransformScale(boneIndex, dof));
                                                SetKeyframe(curve, time, scale[dof]);
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                            
                            foreach (var pair in curves)
                            {
                                var curve = pair.Value;
                                if (curve == null || curve.length <= 0) continue;

                                for (int i = 0; i < curve.length; i++)
                                    curve.SmoothTangents(i, 0f);
                                AnimationUtility.SetEditorCurve(clip, pair.Key, curve);
                            }
                        }
                        #endregion
                    }
                    #endregion
                }

#if UNITY_2017_1_OR_NEWER
                if (uAw_2017_1.GetLinkedWithTimeline())
                {
                    Undo.RecordObject(uAw_2017_1.GetTimelineCurrentDirector(), "Create New Clip");
                    var animationTrack = uAw_2017_1.GetAnimationTrack();
                    Undo.RecordObject(animationTrack, "Create New Clip");
                    var timelineClip = animationTrack.CreateClip(clip);
                    timelineClip.displayName = Path.GetFileNameWithoutExtension(clipPath);
                    uAw.ForceRefresh();
                    uAw_2017_1.EditSequencerClip(timelineClip);
                }
                else
#endif
                {
                    #region Animator
                    if (vaw.animator != null && vaw.animator.runtimeAnimatorController != null)
                    {
                        UnityEditor.Animations.AnimatorController ac = null;
                        AnimationClip virtualClip = null;
                        #region AnimatorOverrideController
                        if (vaw.animator.runtimeAnimatorController is AnimatorOverrideController)
                        {
                            var owc = vaw.animator.runtimeAnimatorController as AnimatorOverrideController;
                            ac = owc.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                            {
                                List<KeyValuePair<AnimationClip, AnimationClip>> srcList = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                                owc.GetOverrides(srcList);
                                foreach (var pair in srcList)
                                {
                                    if (pair.Value == currentClip)
                                    {
                                        virtualClip = pair.Key;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ac = vaw.animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                        }
                        #endregion
                        #region AnimatorControllerLayer
                        AnimatorState animatorState = null;
                        foreach (UnityEditor.Animations.AnimatorControllerLayer layer in ac.layers)
                        {
                            Action<AnimatorStateMachine> FindStateMachine = null;
                            FindStateMachine = (stateMachine) =>
                            {
                                if (animatorState != null)
                                    return;
                                foreach (var state in stateMachine.states)
                                {
                                    Action AddState = () =>
                                    {
                                        if (animatorState != null)
                                            return;
                                        animatorState = new AnimatorState();
                                        {
                                            animatorState.name = Path.GetFileNameWithoutExtension(clipPath);
                                            animatorState.behaviours = state.state.behaviours;
                                            animatorState.transitions = state.state.transitions;
                                            animatorState.mirrorParameterActive = state.state.mirrorParameterActive;
                                            animatorState.cycleOffsetParameterActive = state.state.cycleOffsetParameterActive;
                                            animatorState.speedParameterActive = state.state.speedParameterActive;
                                            animatorState.mirrorParameter = state.state.mirrorParameter;
                                            animatorState.cycleOffsetParameter = state.state.cycleOffsetParameter;
                                            animatorState.speedParameter = state.state.speedParameter;
                                            animatorState.tag = state.state.tag;
                                            animatorState.writeDefaultValues = state.state.writeDefaultValues;
                                            animatorState.iKOnFeet = state.state.iKOnFeet;
                                            animatorState.mirror = state.state.mirror;
                                            animatorState.cycleOffset = state.state.cycleOffset;
                                            animatorState.speed = state.state.speed;
                                            animatorState.motion = clip;
#if UNITY_2017_2_OR_NEWER
                                            animatorState.timeParameter = state.state.timeParameter;
                                            animatorState.timeParameterActive = state.state.timeParameterActive;
#endif
                                        }
                                        stateMachine.AddState(animatorState, state.position + new Vector3(35f, 65f));
                                    };

                                    if (state.state.motion is UnityEditor.Animations.BlendTree)
                                    {
                                        Action<UnityEditor.Animations.BlendTree> FindBlendTree = null;
                                        FindBlendTree = (blendTree) =>
                                        {
                                            if (animatorState != null)
                                                return;
                                            if (blendTree.children == null) return;
                                            var children = blendTree.children;
                                            for (int i = 0; i < children.Length; i++)
                                            {
                                                if (children[i].motion is UnityEditor.Animations.BlendTree)
                                                {
                                                    FindBlendTree(children[i].motion as UnityEditor.Animations.BlendTree);
                                                }
                                                else
                                                {
                                                    if (children[i].motion == currentClip || (virtualClip != null && children[i].motion == virtualClip))
                                                    {
                                                        AddState();
                                                        return;
                                                    }
                                                }
                                            }
                                            blendTree.children = children;
                                        };
                                        FindBlendTree(state.state.motion as UnityEditor.Animations.BlendTree);
                                    }
                                    else
                                    {
                                        if (state.state.motion == currentClip || (virtualClip != null && state.state.motion == virtualClip))
                                        {
                                            AddState();
                                            return;
                                        }
                                    }
                                }
                                foreach (var childStateMachine in stateMachine.stateMachines)
                                {
                                    FindStateMachine(childStateMachine.stateMachine);
                                }
                            };
                            FindStateMachine(layer.stateMachine);
                        }
                        #endregion
                    }
                    #endregion
                    #region Animation
                    if (vaw.animation != null)
                    {
                        Undo.RecordObject(vaw.animation, "Create New Clip");
                        var animations = AnimationUtility.GetAnimationClips(vaw.gameObject);
                        ArrayUtility.Add(ref animations, clip);
                        AnimationUtility.SetAnimationClips(vaw.animation, animations);
                    }
                    #endregion

                    uAw.ForceRefresh();
                    uAw.SetSelectionAnimationClip(clip);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            ToolsCommonAfter();
        }
        private void ToolsRotationCurveInterpolation(AnimationClip clip, RotationCurveInterpolationMode rotationCurveInterpolationMode)
        {
            if (!ToolsFixOverRotationCurve(clip)) return;

            if (!ToolsCommonBefore(clip, "RotationCurveInterpolation")) return;

            try
            {
                var bindings = AnimationUtility.GetCurveBindings(clip);
                int progressIndex = 0;
                int progressTotal = bindings.Length + 1;

                {
                    List<EditorCurveBinding> convertBindings = new List<EditorCurveBinding>();
                    for (int i = 0; i < bindings.Length; i++)
                    {
                        EditorUtility.DisplayProgressBar("Read", string.IsNullOrEmpty(bindings[i].path) ? bindings[i].propertyName : bindings[i].path, progressIndex++ / (float)progressTotal);
                        if (!IsTransformRotationCurveBinding(bindings[i])) continue;
                        var mode = uRotationCurveInterpolation.GetModeFromCurveData(bindings[i]);
                        if (convertBindings.FindIndex((x) => x.path == bindings[i].path) < 0)
                        {
                            var boneIndex = GetBoneIndexFromCurveBinding(bindings[i]);
                            if (boneIndex >= 0)
                            {
                                switch (mode)
                                {
                                case URotationCurveInterpolation.Mode.RawQuaternions:
                                    if (rotationCurveInterpolationMode != RotationCurveInterpolationMode.Quaternion)
                                    {
                                        for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                                            convertBindings.Add(AnimationCurveBindingTransformRotation(boneIndex, dofIndex, URotationCurveInterpolation.Mode.Baked));
                                        for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                                            convertBindings.Add(AnimationCurveBindingTransformRotation(boneIndex, dofIndex, URotationCurveInterpolation.Mode.NonBaked));
                                    }
                                    else
                                    {
                                        for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                                            convertBindings.Add(AnimationCurveBindingTransformRotation(boneIndex, dofIndex, URotationCurveInterpolation.Mode.Baked));
                                    }
                                    break;
                                case URotationCurveInterpolation.Mode.RawEuler:
                                    if (rotationCurveInterpolationMode != RotationCurveInterpolationMode.EulerAngles)
                                    {
                                        for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                                            convertBindings.Add(AnimationCurveBindingTransformRotation(boneIndex, dofIndex, URotationCurveInterpolation.Mode.RawEuler));
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    {
                        URotationCurveInterpolation.Mode mode = (URotationCurveInterpolation.Mode)(-1);
                        switch (rotationCurveInterpolationMode)
                        {
                        case RotationCurveInterpolationMode.Quaternion: mode = URotationCurveInterpolation.Mode.NonBaked; break;
                        case RotationCurveInterpolationMode.EulerAngles: mode = URotationCurveInterpolation.Mode.RawEuler; break;
                        default: Assert.IsTrue(false); break;
                        }
                        EditorUtility.DisplayProgressBar("Convert", "", progressIndex++ / (float)progressTotal);
                        if (convertBindings.Count > 0)
                            uRotationCurveInterpolation.SetInterpolation(clip, convertBindings.ToArray(), mode);
                    }
                }
                #region FixReverseRotation
                if (rotationCurveInterpolationMode == RotationCurveInterpolationMode.EulerAngles)
                {
                    bindings = AnimationUtility.GetCurveBindings(clip);
                    foreach (var binding in bindings)
                    {
                        if (!IsTransformRotationCurveBinding(binding)) continue;
                        var curve = AnimationUtility.GetEditorCurve(clip, binding);
                        if (FixReverseRotationEuler(curve))
                            AnimationUtility.SetEditorCurve(clip, binding, curve);
                    }
                }
                #endregion
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            ToolsCommonAfter();
        }
        private void ToolsKeyframeReduction(AnimationClip clip)
        {
            if (!ToolsCommonBefore(clip, "KeyframeReduction")) return;

            ToolsRotationCurveInterpolation(clip, RotationCurveInterpolationMode.Quaternion);

            AnimationClip tmpClip = null;
            GameObject tmpObject = null;

            try
            {
                VeryAnimationWindow.CustomAssetModificationProcessor.Pause();

                var assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(clip)) + "/" + clip.name + "_tmp.dae";
                assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                var path = Application.dataPath + assetPath.Remove(0, "Assets".Length);

                transformPoseSave.ResetOriginalTransform();
                blendShapeWeightSave.ResetOriginalWeight();
                {
                    editBones[0].transform.localPosition = Vector3.zero;
                    editBones[0].transform.localRotation = Quaternion.identity;
                    editBones[0].transform.localScale = Vector3.one;
                }

                tmpClip = AnimationClip.Instantiate(clip);
                tmpClip.hideFlags |= HideFlags.HideAndDontSave;
                tmpObject = GameObject.Instantiate(editGameObject);
                tmpObject.hideFlags |= HideFlags.HideAndDontSave;

                #region AddOtherCurve
                var otherCurveDic = new Dictionary<EditorCurveBinding, EditorCurveBinding>();
                {
                    var bindings = AnimationUtility.GetCurveBindings(tmpClip);
                    foreach (var binding in bindings)
                    {
                        if (binding.type == typeof(Animator) || binding.type == typeof(Transform))
                            continue;
                        var valueType = AnimationUtility.GetEditorCurveValueType(vaw.gameObject, binding);
                        if (valueType != typeof(float))
                            continue;
                        var curve = AnimationUtility.GetEditorCurve(tmpClip, binding);
                        if (curve == null)
                            continue;
                        AnimationUtility.SetEditorCurve(tmpClip, binding, null);
                        var add = new GameObject(binding.GetHashCode().ToString());
                        add.hideFlags |= HideFlags.HideAndDontSave;
                        add.transform.SetParent(tmpObject.transform);
                        var addBinding = new EditorCurveBinding()
                        {
                            type = typeof(Transform),
                            path = AnimationUtility.CalculateTransformPath(add.transform, tmpObject.transform),
                            propertyName = EditorCurveBindingTransformScalePropertyNames[0],
                        };
                        AnimationUtility.SetEditorCurve(tmpClip, addBinding, curve);
                        otherCurveDic.Add(binding, addBinding);
                    }
                }
                #endregion

                var tmpBones = EditorCommon.GetHierarchyGameObject(tmpObject).ToArray();

                var transforms = new List<Transform>(tmpBones.Length);
                foreach (var b in tmpBones)
                    transforms.Add(b.transform);

                AnimationClip[] clips = new AnimationClip[] { tmpClip };

                DaeExporter exporter = new DaeExporter()
                {
                    settings_activeOnly = false,
                    settings_exportMesh = false,
                    settings_iKOnFeet = false,
                };
                var result = exporter.Export(path, transforms, clips);
                if (result)
                {
                    AnimationClip reductionClip = null;
                    foreach (var p in exporter.exportedFiles)
                    {
                        if (!Path.GetFileNameWithoutExtension(p).Contains("@"))
                            continue;
                        if (!p.StartsWith(Application.dataPath)) continue;
                        var pTmp = p.Replace(Application.dataPath, "Assets");
                        if (!File.Exists(pTmp)) continue;
                        AssetDatabase.ImportAsset(pTmp);
                        var importer = AssetImporter.GetAtPath(pTmp);
                        if (importer is ModelImporter)
                        {
                            #region ModelImporter
                            var modelImporter = importer as ModelImporter;
                            var setClips = modelImporter.defaultClipAnimations;
                            if (modelImporter != null)
                            {
                                if (editAnimator != null)
                                {
                                    modelImporter.animationType = isHuman ? ModelImporterAnimationType.Human : ModelImporterAnimationType.Generic;
                                    modelImporter.sourceAvatar = vaw.animator.avatar;
                                }
                                else
                                {
                                    modelImporter.animationType = ModelImporterAnimationType.Legacy;
                                }
                                modelImporter.importAnimation = true;
                                modelImporter.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
                                modelImporter.animationRotationError = toolKeyframeReduction_RotationError;
                                modelImporter.animationPositionError = toolKeyframeReduction_PositionError;
                                modelImporter.animationScaleError = toolKeyframeReduction_ScaleAndOthersError;
                            }
                            if (clips != null)
                            {
                                var index = 0;
                                {
                                    var name = pTmp.Substring(pTmp.LastIndexOf("@") + 1);
                                    name = name.Remove(name.LastIndexOf("."));
                                    for (int i = 0; i < clips.Length; i++)
                                    {
                                        if (clips[i].name.StartsWith(name))
                                        {
                                            index = i;
                                            break;
                                        }
                                    }
                                }
                                var settings = AnimationUtility.GetAnimationClipSettings(clips[index]);
                                var hasMotionCurves = uAnimationUtility.HasMotionCurves(clips[index]);
                                foreach (var setClip in setClips)
                                {
                                    setClip.keepOriginalPositionXZ = true;
                                    setClip.keepOriginalPositionY = true;
                                    setClip.keepOriginalOrientation = true;
                                    if (!hasMotionCurves)
                                    {
                                        setClip.lockRootPositionXZ = settings.loopBlendPositionXZ;
                                        setClip.lockRootHeightY = settings.loopBlendPositionY;
                                        setClip.lockRootRotation = settings.loopBlendOrientation;
                                    }
                                    setClip.name = clips[index].name;
                                    if (modelImporter.animationType == ModelImporterAnimationType.Legacy)
                                    {
                                        setClip.wrapMode = settings.loopTime ? WrapMode.Loop : WrapMode.Default;
                                    }
                                    else
                                    {
                                        setClip.loop = settings.loopTime;
                                        setClip.loopTime = settings.loopTime;
                                    }
                                }
                                modelImporter.clipAnimations = setClips;
                                #region Mask
                                {
                                    var avatarMask = new AvatarMask();
                                    {
                                        avatarMask.transformCount = modelImporter.transformPaths.Length;
                                        for (int i = 0; i < modelImporter.transformPaths.Length; i++)
                                        {
                                            avatarMask.SetTransformPath(i, modelImporter.transformPaths[i]);
                                            avatarMask.SetTransformActive(i, true);
                                        }
                                    }
                                    var updateTransformMask = importer.GetType().GetMethod("UpdateTransformMask", BindingFlags.NonPublic | BindingFlags.Static);
                                    SerializedObject so = new SerializedObject(modelImporter);
                                    SerializedProperty spClips = so.FindProperty("m_ClipAnimations");
                                    for (int i = 0; i < spClips.arraySize; i++)
                                    {
                                        var spTransformMask = spClips.GetArrayElementAtIndex(i).FindPropertyRelative("transformMask");
                                        updateTransformMask.Invoke(modelImporter, new System.Object[] { avatarMask, spTransformMask });
                                    }
                                    so.ApplyModifiedProperties();
                                }
                                #endregion
                                modelImporter.SaveAndReimport();
                            }
                            #endregion
                        }
                        reductionClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(importer.assetPath);
                    }
                    AssetDatabase.Refresh();
                    if (reductionClip != null)
                    {
                        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                        {
                            var srcCurve = AnimationUtility.GetEditorCurve(clip, binding);
                            var reductionCurve = AnimationUtility.GetEditorCurve(reductionClip, binding);
                            if (reductionCurve != null)
                            {
                                #region CopyCurve
                                if (!toolKeyframeReduction_EnableHumanoid && binding.type == typeof(Animator))
                                    continue;
                                if (!toolKeyframeReduction_EnableGeneric && binding.type == typeof(Transform))
                                    continue;
                                if (srcCurve.length > reductionCurve.length)
                                    AnimationUtility.SetEditorCurve(clip, binding, reductionCurve);
                                #endregion
                            }
                            else
                            {
                                #region CopyOtherCurve
                                if (!toolKeyframeReduction_EnableOther)
                                    continue;
                                EditorCurveBinding origBinding;
                                if (otherCurveDic.TryGetValue(binding, out origBinding))
                                {
                                    reductionCurve = AnimationUtility.GetEditorCurve(reductionClip, origBinding);
                                    if (reductionCurve != null)
                                    {
                                        if (srcCurve.length > reductionCurve.length)
                                            AnimationUtility.SetEditorCurve(clip, binding, reductionCurve);
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    foreach (var p in exporter.exportedFiles)
                    {
                        var pTmp = p.Replace(Application.dataPath, "Assets");
                        AssetDatabase.DeleteAsset(pTmp);
                    }
                    AssetDatabase.Refresh();
                }
                #region SimpleReductionKeyframe
                if (toolKeyframeReduction_EnableOther)
                {
                    foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                    {
                        if (binding.type == typeof(Animator) || binding.type == typeof(Transform))
                            continue;
                        var valueType = AnimationUtility.GetEditorCurveValueType(vaw.gameObject, binding);
                        if (valueType == null || valueType == typeof(float))
                            continue;
                        var curve = AnimationUtility.GetEditorCurve(clip, binding);
                        if (curve == null)
                            continue;
                        bool update = false;
                        for (int i = 1; i < curve.length - 1; i++)
                        {
                            if (Mathf.Approximately(curve[i - 1].value, curve[i].value) &&
                                Mathf.Approximately(curve[i + 1].value, curve[i].value))
                            {
                                curve.RemoveKey(i--);
                                update = true;
                            }
                        }
                        if (update)
                            AnimationUtility.SetEditorCurve(clip, binding, curve);
                    }
                    foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                    {
                        var curve = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                        if (curve == null)
                            continue;
                        bool update = false;
                        var keys = new List<ObjectReferenceKeyframe>(curve);
                        for (int i = 1; i < keys.Count - 1; i++)
                        {
                            if (keys[i - 1].value == keys[i].value)
                            {
                                keys.RemoveAt(i--);
                                update = true;
                            }
                        }
                        if (update)
                            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys.ToArray());
                    }
                }
                #endregion
            }
            finally
            {
                if (tmpClip != null)
                    AnimationClip.DestroyImmediate(tmpClip);
                if (tmpObject != null)
                    GameObject.DestroyImmediate(tmpObject);
                transformPoseSave.ResetOriginalTransform();
                ResampleAnimation();
                VeryAnimationWindow.CustomAssetModificationProcessor.Resume();
            }

            ToolsCommonAfter();
        }
        private void ToolsEnsureQuaternionContinuity(AnimationClip clip)
        {
            if (!ToolsCommonBefore(clip, "EnsureQuaternionContinuity")) return;

            {
                clip.EnsureQuaternionContinuity();
            }

            ToolsCommonAfter();
        }
        private void ToolsCleanup(AnimationClip clip)
        {
            if (!ToolsCommonBefore(clip, "Cleanup")) return;

            try
            {
                Action<HumanBodyBones> RemoveMuscleCurve = (hi) =>
                {
                    for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                    {
                        var mi = HumanTrait.MuscleFromBone((int)hi, dofIndex);
                        if (mi < 0) continue;
                        AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorMuscle(mi), null);
                    }
                };

                int progressIndex = 0;
                int progressTotal = 16;

                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveRoot)
                {
                    for (int i = 0; i < AnimationCurveBindingAnimatorRootT.Length; i++)
                        AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorRootT[i], null);
                    for (int i = 0; i < AnimationCurveBindingAnimatorRootQ.Length; i++)
                        AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorRootQ[i], null);
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveIK)
                {
                    for (int ikIndex = 0; ikIndex < (int)AnimatorIKIndex.Total; ikIndex++)
                    {
                        for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                            AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorIkT((AnimatorIKIndex)ikIndex, dofIndex), null);
                        for (int dofIndex = 0; dofIndex < 4; dofIndex++)
                            AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorIkQ((AnimatorIKIndex)ikIndex, dofIndex), null);
                    }
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveTDOF)
                {
                    for (int tdofIndex = 0; tdofIndex < (int)AnimatorTDOFIndex.Total; tdofIndex++)
                    {
                        for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                            AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorTDOF((AnimatorTDOFIndex)tdofIndex, dofIndex), null);
                    }
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveMotion)
                {
                    for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                        AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorMotionT[dofIndex], null);
                    for (int dofIndex = 0; dofIndex < 4; dofIndex++)
                        AnimationUtility.SetEditorCurve(clip, AnimationCurveBindingAnimatorMotionQ[dofIndex], null);
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveFinger)
                {
                    for (var hi = HumanBodyBones.LeftThumbProximal; hi <= HumanBodyBones.RightLittleDistal; hi++)
                        RemoveMuscleCurve(hi);
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveEyes)
                {
                    RemoveMuscleCurve(HumanBodyBones.LeftEye);
                    RemoveMuscleCurve(HumanBodyBones.RightEye);
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveJaw)
                {
                    RemoveMuscleCurve(HumanBodyBones.Jaw);
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveToes)
                {
                    RemoveMuscleCurve(HumanBodyBones.LeftToes);
                    RemoveMuscleCurve(HumanBodyBones.RightToes);
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveTransformPosition || toolCleanup_RemoveTransformRotation || toolCleanup_RemoveTransformScale)
                {
                    foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                    {
                        if (binding.type == typeof(Transform))
                        {
                            if ((toolCleanup_RemoveTransformPosition && binding.propertyName.StartsWith("m_LocalPosition.")) ||
                                (toolCleanup_RemoveTransformRotation && (binding.propertyName.StartsWith("m_LocalRotation.") || binding.propertyName.StartsWith("localEulerAngles"))) ||
                                (toolCleanup_RemoveTransformScale && binding.propertyName.StartsWith("m_LocalScale.")))
                            {
                                AnimationUtility.SetEditorCurve(clip, binding, null);
                            }
                        }
                    }
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveBlendShape)
                {
                    foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                    {
                        if (IsSkinnedMeshRendererBlendShapeCurveBinding(binding))
                        {
                            AnimationUtility.SetEditorCurve(clip, binding, null);
                        }
                    }
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveObjectReference)
                {
                    foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                    {
                        AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
                    }
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveEvent)
                {
                    AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveMissing)
                {
                    foreach (var binding in uAw.GetMissingCurveBindings())
                    {
                        if (!binding.isPPtrCurve)
                            AnimationUtility.SetEditorCurve(clip, binding, null);
                        else
                            AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
                    }
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveHumanoidConflict && isHuman)
                {
                    List<string> paths = new List<string>();
                    for (int i = 0; i < editBones.Length; i++)
                    {
                        if (humanoidConflict[i])
                        {
                            paths.Add(AnimationUtility.CalculateTransformPath(editBones[i].transform, editGameObject.transform));
                        }
                    }
                    foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                    {
                        if (binding.type != typeof(Transform)) continue;
                        if (!paths.Contains(binding.path)) continue;
                        AnimationUtility.SetEditorCurve(clip, binding, null);
                    }
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveRootMotionConflict && rootMotionBoneIndex >= 0)
                {
                    foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                    {
                        if (!IsTransformPositionCurveBinding(binding) && !IsTransformRotationCurveBinding(binding)) continue;
                        var boneIndex = GetBoneIndexFromCurveBinding(binding);
                        if (boneIndex == 0)
                            AnimationUtility.SetEditorCurve(clip, binding, null);
                    }
                }
                EditorUtility.DisplayProgressBar("Cleanup", "", progressIndex++ / (float)progressTotal);
                if (toolCleanup_RemoveUnnecessary)
                {
                    ToolsReductionCurve(clip);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            ToolsCommonAfter();
        }
        private void ToolsFixErrors(AnimationClip clip)
        {
            if (!ToolsFixOverRotationCurve(clip)) return;

            if (!ToolsCommonBefore(clip, "Fix Errors")) return;

            try
            {
                var bindings = AnimationUtility.GetCurveBindings(clip);

                int progressIndex = 0;
                int progressTotal = bindings.Length;

                foreach (var binding in bindings)
                {
                    EditorUtility.DisplayProgressBar("Fix Errors", "", progressIndex++ / (float)progressTotal);

                    #region There must be at least two keyframes. If not, an Assert will occur.[AnimationUtility.GetEditorCurve]
                    if (IsTransformRotationCurveBinding(binding) && uRotationCurveInterpolation.GetModeFromCurveData(binding) == URotationCurveInterpolation.Mode.RawQuaternions)
                    {
                        var curve = AnimationUtility.GetEditorCurve(clip, binding);
                        if (curve.length <= 1)
                        {
                            Action<float> ErrorAvoidance = (time) =>
                            {
                                while (curve.length < 2)
                                {
                                    var addTime = 0f;
                                    if (time != 0f) addTime = 0f;
                                    else if (clip.length != 0f) addTime = clip.length;
                                    else addTime = 1f;
                                    AddKeyframe(curve, addTime, curve.Evaluate(addTime));
                                }
                            };
                            ErrorAvoidance(0f);
                            AnimationUtility.SetEditorCurve(clip, binding, curve);
                            Debug.LogWarningFormat(Language.GetText(Language.Help.LogFixErrors), binding.path, binding.propertyName);
                        }
                    }
                    #endregion
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            ToolsCommonAfter();
        }
        private void ToolsExport()
        {
            string path = EditorUtility.SaveFilePanel("Export",
                                                        Path.GetDirectoryName(AssetDatabase.GetAssetPath(currentClip)),
                                                        editGameObject.name + ".dae", "dae");
            if (string.IsNullOrEmpty(path))
                return;

            transformPoseSave.ResetOriginalTransform();
            blendShapeWeightSave.ResetOriginalWeight();
            {
                editBones[0].transform.localPosition = Vector3.zero;
                editBones[0].transform.localRotation = Quaternion.identity;
                editBones[0].transform.localScale = Vector3.one;
            }

            var transforms = new List<Transform>(editBones.Length);
            foreach (var b in editBones)
                transforms.Add(b.transform);

            AnimationClip[] clips = null;
            switch (toolExport_AnimationMode)
            {
            case ExportAnimationMode.None:
                clips = null;
                break;
            case ExportAnimationMode.CurrentClip:
                clips = new AnimationClip[] { currentClip };
                break;
            case ExportAnimationMode.AllClips:
                clips = AnimationUtility.GetAnimationClips(vaw.gameObject);
                break;
            }

            try
            {
                VeryAnimationWindow.CustomAssetModificationProcessor.Pause();

                DaeExporter exporter = new DaeExporter()
                {
                    settings_activeOnly = toolExport_ActiveOnly,
                    settings_exportMesh = toolExport_Mesh,
                    settings_iKOnFeet = toolExport_FootIK,
                };
                var result = exporter.Export(path, transforms, clips);
                if (result)
                {
                    foreach (var p in exporter.exportedFiles)
                    {
                        if (!p.StartsWith(Application.dataPath)) continue;
                        var pTmp = p.Replace(Application.dataPath, "Assets");
                        if (!File.Exists(pTmp)) continue;
                        AssetDatabase.ImportAsset(pTmp);
                        var importer = AssetImporter.GetAtPath(pTmp);
                        if (importer is ModelImporter)
                        {
                            #region ModelImporter
                            var modelImporter = importer as ModelImporter;
                            if (clips != null)
                            {
                                var index = 0;
                                {
                                    var name = pTmp.Substring(pTmp.LastIndexOf("@") + 1);
                                    name = name.Remove(name.LastIndexOf("."));
                                    for (int i = 0; i < clips.Length; i++)
                                    {
                                        if (clips[i].name.StartsWith(name))
                                        {
                                            index = i;
                                            break;
                                        }
                                    }
                                }
                                var settings = AnimationUtility.GetAnimationClipSettings(clips[index]);
                                var hasMotionCurves = uAnimationUtility.HasMotionCurves(clips[index]);
                                var setClips = modelImporter.defaultClipAnimations;
                                foreach (var setClip in setClips)
                                {
                                    setClip.keepOriginalPositionXZ = true;
                                    setClip.keepOriginalPositionY = true;
                                    setClip.keepOriginalOrientation = true;
                                    if (!hasMotionCurves)
                                    {
                                        setClip.lockRootPositionXZ = settings.loopBlendPositionXZ;
                                        setClip.lockRootHeightY = settings.loopBlendPositionY;
                                        setClip.lockRootRotation = settings.loopBlendOrientation;
                                    }
                                    setClip.name = clips[index].name;
                                    if (modelImporter.animationType == ModelImporterAnimationType.Legacy)
                                    {
                                        setClip.wrapMode = settings.loopTime ? WrapMode.Loop : WrapMode.Default;
                                    }
                                    else
                                    {
                                        setClip.loop = settings.loopTime;
                                        setClip.loopTime = settings.loopTime;
                                    }
                                }
                                modelImporter.clipAnimations = setClips;
                                #region Fix mask
                                if (modelImporter.animationType == ModelImporterAnimationType.Legacy)
                                {
                                    var avatarMask = new AvatarMask();
                                    {
                                        avatarMask.transformCount = modelImporter.transformPaths.Length;
                                        for (int i = 0; i < modelImporter.transformPaths.Length; i++)
                                        {
                                            avatarMask.SetTransformPath(i, modelImporter.transformPaths[i]);
                                            avatarMask.SetTransformActive(i, true);
                                        }
                                    }
                                    {
                                        var updateTransformMask = importer.GetType().GetMethod("UpdateTransformMask", BindingFlags.NonPublic | BindingFlags.Static);
                                        SerializedObject so = new SerializedObject(modelImporter);
                                        SerializedProperty spClips = so.FindProperty("m_ClipAnimations");
                                        for (int i = 0; i < spClips.arraySize; i++)
                                        {
                                            var spTransformMask = spClips.GetArrayElementAtIndex(i).FindPropertyRelative("transformMask");
                                            updateTransformMask.Invoke(modelImporter, new System.Object[] { avatarMask, spTransformMask });
                                        }
                                        so.ApplyModifiedProperties();
                                    }
                                }
                                #endregion
                                modelImporter.SaveAndReimport();
                            }
                            #endregion
                        }
                    }
                    AssetDatabase.Refresh();
                }
            }
            finally
            {
                transformPoseSave.ResetOriginalTransform();
                ResampleAnimation();
                VeryAnimationWindow.CustomAssetModificationProcessor.Resume();
            }
            SetUpdateResampleAnimation();
            uAw.ForceRefresh();
        }
    }
}
