﻿using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.IMGUI.Controls;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace VeryAnimation
{
    public class UAnimationWindow //560
    {
        protected VeryAnimationWindow vaw { get { return VeryAnimationWindow.instance; } }
        protected VeryAnimation va { get { return VeryAnimation.instance; } }
        
        protected Func<object, IList> dg_get_s_AnimationWindows;
        protected Func<object, object> dg_get_m_AnimEditor;
        protected Func<object, bool> dg_get_m_Locked;
        protected Action<object, bool> dg_set_m_Locked;

        protected class UAnimEditor
        {
            protected PropertyInfo pi_selectedItem;
            private PropertyInfo pi_triggerFraming;
            private MethodInfo mi_SwitchBetweenCurvesAndDopesheet;
            private MethodInfo mi_UpdateSelectedKeysToCurveEditor;
            private Func<object, object> dg_get_m_State;
            private Func<object> dg_get_selectedItem;
            private Func<object> dg_get_curveEditor;

            public UAnimEditor(Assembly asmUnityEditor)
            {
                var animEditorType = asmUnityEditor.GetType("UnityEditor.AnimEditor");
                pi_selectedItem = animEditorType.GetProperty("selectedItem");
                Assert.IsNotNull(pi_triggerFraming = animEditorType.GetProperty("triggerFraming", BindingFlags.NonPublic | BindingFlags.Instance));
                Assert.IsNotNull(mi_SwitchBetweenCurvesAndDopesheet = animEditorType.GetMethod("SwitchBetweenCurvesAndDopesheet", BindingFlags.NonPublic | BindingFlags.Instance));
                Assert.IsNotNull(mi_UpdateSelectedKeysToCurveEditor = animEditorType.GetMethod("UpdateSelectedKeysToCurveEditor", BindingFlags.NonPublic | BindingFlags.Instance));
                Assert.IsNotNull(dg_get_m_State = EditorCommon.CreateGetFieldDelegate<object>(animEditorType.GetField("m_State", BindingFlags.NonPublic | BindingFlags.Instance)));
            }

            public object GetAnimationWindowState(object instance)
            {
                if (instance == null) return null;
                return dg_get_m_State(instance);
            }

            public object GetSelectedItem(object instance)
            {
                if (instance == null) return null;
                if (dg_get_selectedItem == null || dg_get_selectedItem.Target != instance)
                    dg_get_selectedItem = (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), instance, pi_selectedItem.GetGetMethod());
                return dg_get_selectedItem();
            }
            public void SetTriggerFraming(object instance)
            {
                if (instance == null) return;
                pi_triggerFraming.SetValue(instance, true, null);
            }

            public void SwitchBetweenCurvesAndDopesheet(object instance)
            {
                if (instance == null) return;
                mi_SwitchBetweenCurvesAndDopesheet.Invoke(instance, null);
            }

            public void UpdateSelectedKeysToCurveEditor(object instance)
            {
                if (instance == null) return;
                mi_UpdateSelectedKeysToCurveEditor.Invoke(instance, null);
            }

            public object GetCurveEditor(object instance)
            {
                if (instance == null) return null;
                if (dg_get_curveEditor == null || dg_get_curveEditor.Target != instance)
                    dg_get_curveEditor = (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), instance, instance.GetType().GetProperty("curveEditor", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true));
                return dg_get_curveEditor();
            }
        }
        protected class UCurveEditor
        {
            private Func<bool> dg_get_hasSelection;
            private Action dg_ClearSelection;

            public UCurveEditor(Assembly asmUnityEditor)
            {
            }

            public bool HasSelection(object instance)
            {
                if (instance == null) return false;
                if (dg_get_hasSelection == null || dg_get_hasSelection.Target != instance)
                    dg_get_hasSelection = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), instance, instance.GetType().GetProperty("hasSelection", BindingFlags.Public | BindingFlags.Instance).GetGetMethod());
                return dg_get_hasSelection();
            }
            public void ClearSelection(object instance)
            {
                if (instance == null) return;
                if (dg_ClearSelection == null || dg_ClearSelection.Target != instance)
                    dg_ClearSelection = (Action)Delegate.CreateDelegate(typeof(Action), instance, instance.GetType().GetMethod("ClearSelection", BindingFlags.NonPublic | BindingFlags.Instance));
                dg_ClearSelection();
            }
        }
        protected class UAnimationWindowState
        {
            protected Type animationWindowStateType;
            protected PropertyInfo pi_refresh;
            protected MethodInfo mi_ForceRefresh;
            protected MethodInfo mi_CurveWasModified;
            protected MethodInfo mi_SelectKey;
            protected MethodInfo mi_ClearKeySelections;
            protected MethodInfo mi_ClearHierarchySelection;
            protected MethodInfo mi_SelectHierarchyItem;
            protected MethodInfo mi_UnSelectHierarchyItem;
            protected MethodInfo mi_SnapToFrame;
            protected MethodInfo mi_TimeToFrameRound;
            protected MethodInfo mi_StartRecording;
            protected MethodInfo mi_StopRecording;
            protected MethodInfo mi_StartPlayback;
            protected MethodInfo mi_StopPlayback;
            protected Func<object, bool>  dg_get_showCurveEditor;
            protected Func<object, TreeViewState> dg_get_hierarchyState;
            protected Func<object, object> dg_get_hierarchyData;
            protected Func<object, IList> dg_get_m_ActiveCurvesCache;
            protected Action<object, IList> dg_set_m_ActiveCurvesCache;
            protected Func<object, IList> dg_get_m_dopelinesCache;
            protected Action<object, IList> dg_set_m_dopelinesCache;
            protected Func<object> dg_get_controlInterface;
            protected Func<GameObject> dg_get_activeRootGameObject;
            protected Func<Component> dg_get_activeAnimationPlayer;
            protected Func<bool> dg_get_playing;
            protected Action<bool> dg_set_playing;
            protected Func<bool> dg_get_recording;
            protected Action<bool> dg_set_recording;
            protected Func<int> dg_get_currentFrame;
            protected Action<int> dg_set_currentFrame;
            protected Func<float> dg_get_currentTime;
            protected Action<float> dg_set_currentTime;
            protected Func<IList> dg_get_allCurves;
            protected Func<IList> dg_get_activeCurves;
            protected Func<IList> dg_get_dopelines;
            protected Func<IEnumerable> dg_get_selectedKeyHashes;
            protected Action<AnimationClip, EditorCurveBinding, AnimationUtility.CurveModifiedType> dg_CurveWasModified;
            protected Func<float, float, float> dg_SnapToFrame;
            protected Func<float, int> dg_TimeToFrameRound;

            public UAnimationWindowState(Assembly asmUnityEditor)
            {
                Assert.IsNotNull(animationWindowStateType = asmUnityEditor.GetType("UnityEditorInternal.AnimationWindowState"));                
                Assert.IsNotNull(pi_refresh = animationWindowStateType.GetProperty("refresh"));
                Assert.IsNotNull(mi_ForceRefresh = animationWindowStateType.GetMethod("ForceRefresh"));
                Assert.IsNotNull(mi_CurveWasModified = animationWindowStateType.GetMethod("CurveWasModified", BindingFlags.Instance | BindingFlags.NonPublic));
                Assert.IsNotNull(mi_SelectKey = animationWindowStateType.GetMethod("SelectKey"));
                Assert.IsNotNull(mi_ClearKeySelections = animationWindowStateType.GetMethod("ClearKeySelections"));
                Assert.IsNotNull(mi_ClearHierarchySelection = animationWindowStateType.GetMethod("ClearHierarchySelection"));
                Assert.IsNotNull(mi_SelectHierarchyItem = animationWindowStateType.GetMethod("SelectHierarchyItem", new Type[] { typeof(int), typeof(bool), typeof(bool) }));
                Assert.IsNotNull(mi_UnSelectHierarchyItem = animationWindowStateType.GetMethod("UnSelectHierarchyItem", new Type[] { typeof(int) }));
                Assert.IsNotNull(mi_SnapToFrame = animationWindowStateType.GetMethod("SnapToFrame", new Type[] { typeof(float), typeof(float) }));
                Assert.IsNotNull(mi_TimeToFrameRound = animationWindowStateType.GetMethod("TimeToFrameRound"));
                Assert.IsNotNull(mi_StartRecording = animationWindowStateType.GetMethod("StartRecording"));
                Assert.IsNotNull(mi_StopRecording = animationWindowStateType.GetMethod("StopRecording"));
                Assert.IsNotNull(mi_StartPlayback = animationWindowStateType.GetMethod("StartPlayback"));
                Assert.IsNotNull(mi_StopPlayback = animationWindowStateType.GetMethod("StopPlayback"));
                Assert.IsNotNull(dg_get_showCurveEditor = EditorCommon.CreateGetFieldDelegate<bool>(animationWindowStateType.GetField("showCurveEditor")));
                Assert.IsNotNull(dg_get_hierarchyState = EditorCommon.CreateGetFieldDelegate<TreeViewState>(animationWindowStateType.GetField("hierarchyState")));
                Assert.IsNotNull(dg_get_hierarchyData = EditorCommon.CreateGetFieldDelegate<object>(animationWindowStateType.GetField("hierarchyData")));
                Assert.IsNotNull(dg_get_m_ActiveCurvesCache = EditorCommon.CreateGetFieldDelegate<IList>(animationWindowStateType.GetField("m_ActiveCurvesCache", BindingFlags.NonPublic | BindingFlags.Instance)));
                Assert.IsNotNull(dg_set_m_ActiveCurvesCache = EditorCommon.CreateSetFieldDelegate<IList>(animationWindowStateType.GetField("m_ActiveCurvesCache", BindingFlags.NonPublic | BindingFlags.Instance)));
                Assert.IsNotNull(dg_get_m_dopelinesCache = EditorCommon.CreateGetFieldDelegate<IList>(animationWindowStateType.GetField("m_dopelinesCache", BindingFlags.NonPublic | BindingFlags.Instance)));
                Assert.IsNotNull(dg_set_m_dopelinesCache = EditorCommon.CreateSetFieldDelegate<IList>(animationWindowStateType.GetField("m_dopelinesCache", BindingFlags.NonPublic | BindingFlags.Instance)));
            }

            public enum RefreshType
            {
                None,
                CurvesOnly,
                Everything,
            }

            public TreeViewState GetHierarchyState(object instance)
            {
                if (instance == null) return null;
                return dg_get_hierarchyState(instance); 
            }
            public bool GetShowCurveEditor(object instance)
            {
                if (instance == null) return false;
                return dg_get_showCurveEditor(instance);
            }
            public object GetHierarchyData(object instance)
            {
                if (instance == null) return null;
                return dg_get_hierarchyData(instance);
            }
            public object GetControlInterface(object instance)
            {
                if (instance == null) return null;
                if (dg_get_controlInterface == null || dg_get_controlInterface.Target != instance)
                    dg_get_controlInterface = (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), instance, instance.GetType().GetProperty("controlInterface").GetGetMethod());
                return dg_get_controlInterface();
            }
            public GameObject GetActiveRootGameObject(object instance)
            {
                if (instance == null) return null;
                if (dg_get_activeRootGameObject == null || dg_get_activeRootGameObject.Target != instance)
                    dg_get_activeRootGameObject = (Func<GameObject>)Delegate.CreateDelegate(typeof(Func<GameObject>), instance, instance.GetType().GetProperty("activeRootGameObject").GetGetMethod());
                return dg_get_activeRootGameObject();
            }
            public Component GetActiveAnimationPlayer(object instance)
            {
                if (instance == null) return null;
                if (dg_get_activeAnimationPlayer == null || dg_get_activeAnimationPlayer.Target != instance)
                    dg_get_activeAnimationPlayer = (Func<Component>)Delegate.CreateDelegate(typeof(Func<Component>), instance, instance.GetType().GetProperty("activeAnimationPlayer").GetGetMethod());
                return dg_get_activeAnimationPlayer();
            }
            public RefreshType GetRefresh(object instance)
            {
                if (instance == null) return RefreshType.None;
                return (RefreshType)pi_refresh.GetValue(instance, null);
            }
            public bool GetPlaying(object instance)
            {
                if (instance == null) return false;
                if (dg_get_playing == null || dg_get_playing.Target != instance)
                    dg_get_playing = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), instance, instance.GetType().GetProperty("playing").GetGetMethod());
                return dg_get_playing();
            }
            public void SetPlaying(object instance, bool value)
            {
                if (instance == null) return;
                if (dg_set_playing == null || dg_set_playing.Target != instance)
                    dg_set_playing = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>), instance, instance.GetType().GetProperty("playing").GetSetMethod());
                dg_set_playing(value);
            }
            public bool GetRecording(object instance)
            {
                if (instance == null) return false;
                if (dg_get_recording == null || dg_get_recording.Target != instance)
                    dg_get_recording = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), instance, instance.GetType().GetProperty("recording").GetGetMethod());
                return dg_get_recording();
            }
            public void SetRecording(object instance, bool value)
            {
                if (instance == null) return;
                if (dg_set_recording == null || dg_set_recording.Target != instance)
                    dg_set_recording = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>), instance, instance.GetType().GetProperty("recording").GetSetMethod());
                dg_set_recording(value);
            }
            public int GetCurrentFrame(object instance)
            {
                if (instance == null) return 0;
                if (dg_get_currentFrame == null || dg_get_currentFrame.Target != instance)
                    dg_get_currentFrame = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), instance, instance.GetType().GetProperty("currentFrame").GetGetMethod());
                return dg_get_currentFrame();
            }
            public void SetCurrentFrame(object instance, int value)
            {
                if (instance == null) return;
                if (dg_set_currentFrame == null || dg_set_currentFrame.Target != instance)
                    dg_set_currentFrame = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), instance, instance.GetType().GetProperty("currentFrame").GetSetMethod());
                dg_set_currentFrame(value);
            }
            public float GetCurrentTime(object instance)
            {
                if (instance == null) return 0f;
                if (dg_get_currentTime == null || dg_get_currentTime.Target != instance)
                    dg_get_currentTime = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), instance, instance.GetType().GetProperty("currentTime").GetGetMethod());
                return dg_get_currentTime();
            }
            public void SetCurrentTime(object instance, float value)
            {
                if (instance == null) return;
                if (dg_set_currentTime == null || dg_set_currentTime.Target != instance)
                    dg_set_currentTime = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), instance, instance.GetType().GetProperty("currentTime").GetSetMethod());
                dg_set_currentTime(value);
            }
            public IList GetAllCurves(object instance)
            {
                if (instance == null) return null;
                if (dg_get_allCurves == null || dg_get_allCurves.Target != instance)
                    dg_get_allCurves = (Func<IList>)Delegate.CreateDelegate(typeof(Func<IList>), instance, instance.GetType().GetProperty("allCurves").GetGetMethod());
                return dg_get_allCurves();
            }
            public IList GetActiveCurves(object instance)
            {
                if (instance == null) return null;
                //Cache Hit
                var list = dg_get_m_ActiveCurvesCache.Invoke(instance);
                if (list != null)
                    return list;
                //Cache Miss
                if (dg_get_activeCurves == null || dg_get_activeCurves.Target != instance)
                    dg_get_activeCurves = (Func<IList>)Delegate.CreateDelegate(typeof(Func<IList>), instance, instance.GetType().GetProperty("activeCurves").GetGetMethod());
                list = dg_get_activeCurves();
                dg_set_m_ActiveCurvesCache.Invoke(instance, null);  //Cache Clear
                return list;
            }
            public IList GetDopelines(object instance)
            {
                if (instance == null) return null;
                //Cache Hit
                var list = dg_get_m_dopelinesCache.Invoke(instance);
                if (list != null)
                    return list;
                //Cache Miss
                if (dg_get_dopelines == null || dg_get_dopelines.Target != instance)
                    dg_get_dopelines = (Func<IList>)Delegate.CreateDelegate(typeof(Func<IList>), instance, instance.GetType().GetProperty("dopelines").GetGetMethod());
                list = dg_get_dopelines();
                dg_set_m_dopelinesCache.Invoke(instance, null);  //Cache Clear
                return list;
            }
            public IEnumerable GetSelectedKeyHashes(object instance)
            {
                if (instance == null) return null;
                if (dg_get_selectedKeyHashes == null || dg_get_selectedKeyHashes.Target != instance)
                    dg_get_selectedKeyHashes = (Func<IEnumerable>)Delegate.CreateDelegate(typeof(Func<IEnumerable>), instance, instance.GetType().GetProperty("selectedKeyHashes", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true));
                return dg_get_selectedKeyHashes();
            }

            public void ForceRefresh(object instance)
            {
                if (instance == null) return;
                mi_ForceRefresh.Invoke(instance, null);
            }
            public void CurveWasModified(object instance, AnimationClip clip, EditorCurveBinding binding, AnimationUtility.CurveModifiedType type)
            {
                if (instance == null) return;
                if (dg_CurveWasModified == null || dg_CurveWasModified.Target != instance)
                    dg_CurveWasModified = (Action<AnimationClip, EditorCurveBinding, AnimationUtility.CurveModifiedType>)Delegate.CreateDelegate(typeof(Action<AnimationClip, EditorCurveBinding, AnimationUtility.CurveModifiedType>), instance, mi_CurveWasModified);
                dg_CurveWasModified(clip, binding, type);
            }
            public void SelectKey(object instance, object keyframe)
            {
                if (instance == null) return;
                mi_SelectKey.Invoke(instance, new object[] { keyframe });
            }
            public void ClearKeySelections(object instance)
            {
                if (instance == null) return;
                mi_ClearKeySelections.Invoke(instance, null);
            }
            public void ClearHierarchySelection(object instance)
            {
                if (instance == null) return;
                mi_ClearHierarchySelection.Invoke(instance, null);
            }
            public void SelectHierarchyItem(object instance, int hierarchyNodeID, bool additive, bool triggerSceneSelectionSync)
            {
                if (instance == null) return;
                mi_SelectHierarchyItem.Invoke(instance, new object[] { hierarchyNodeID, additive, triggerSceneSelectionSync });
            }
            public void UnSelectHierarchyItem(object instance, int hierarchyNodeID)
            {
                if (instance == null) return;
                mi_UnSelectHierarchyItem.Invoke(instance, new object[] { hierarchyNodeID });
            }
            public float SnapToFrame(object instance, float time, float fps)
            {
                if (instance == null) return 0f;
                if (dg_SnapToFrame == null || dg_SnapToFrame.Target != instance)
                    dg_SnapToFrame = (Func<float, float, float>)Delegate.CreateDelegate(typeof(Func<float, float, float>), instance, mi_SnapToFrame);
                return dg_SnapToFrame(time, fps);
            }
            public int TimeToFrameRound(object instance, float time)
            {
                if (instance == null) return 0;
                if (dg_TimeToFrameRound == null || dg_TimeToFrameRound.Target != instance)
                    dg_TimeToFrameRound = (Func<float, int>)Delegate.CreateDelegate(typeof(Func<float, int>), instance, mi_TimeToFrameRound);
                return dg_TimeToFrameRound(time);
            }

            public void StartRecording(object instance)
            {
                if (instance == null) return;
                mi_StartRecording.Invoke(instance, null);
            }
            public virtual void StopRecording(object instance)
            {
                if (instance == null) return;
                mi_StopRecording.Invoke(instance, null);
            }
            public void StartPlayback(object instance)
            {
                if (instance == null) return;
                mi_StartPlayback.Invoke(instance, null);
            }
            public void StopPlayback(object instance)
            {
                if (instance == null) return;
                mi_StopPlayback.Invoke(instance, null);
            }
        }
        protected class UAnimationWindowControl
        {
            protected MethodInfo mi_GoToNextKeyframe;
            protected MethodInfo mi_GoToPreviousKeyframe;
            protected MethodInfo mi_GoToFirstKeyframe;
            protected MethodInfo mi_GoToLastKeyframe;
            protected Func<bool> dg_get_canRecord;
            protected Action dg_ResampleAnimation;

            public UAnimationWindowControl(Assembly asmUnityEditor)
            {
                var animationWindowControlType = asmUnityEditor.GetType("UnityEditorInternal.IAnimationWindowControl");
                Assert.IsNotNull(mi_GoToNextKeyframe = animationWindowControlType.GetMethod("GoToNextKeyframe", new Type[] { }));
                Assert.IsNotNull(mi_GoToPreviousKeyframe = animationWindowControlType.GetMethod("GoToPreviousKeyframe", new Type[] { }));
                Assert.IsNotNull(mi_GoToFirstKeyframe = animationWindowControlType.GetMethod("GoToFirstKeyframe", new Type[] { }));
                Assert.IsNotNull(mi_GoToLastKeyframe = animationWindowControlType.GetMethod("GoToLastKeyframe", new Type[] { }));
            }

            public virtual bool GetCanRecord(object instance)
            {
                if (instance == null) return false;
                if (dg_get_canRecord == null || dg_get_canRecord.Target != instance)
                    dg_get_canRecord = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), instance, instance.GetType().GetProperty("canRecord").GetGetMethod());
                return dg_get_canRecord();
            }
            public void ResampleAnimation(object instance)
            {
                if (instance == null) return;
                if (dg_ResampleAnimation == null || dg_ResampleAnimation.Target != instance)
                    dg_ResampleAnimation = (Action)Delegate.CreateDelegate(typeof(Action), instance, instance.GetType().GetMethod("ResampleAnimation"));
                dg_ResampleAnimation();
            }
            public void GoToNextKeyframe(object instance)
            {
                if (instance == null) return;
                mi_GoToNextKeyframe.Invoke(instance, null);
            }
            public void GoToPreviousKeyframe(object instance)
            {
                if (instance == null) return;
                mi_GoToPreviousKeyframe.Invoke(instance, null);
            }
            public void GoToFirstKeyframe(object instance)
            {
                if (instance == null) return;
                mi_GoToFirstKeyframe.Invoke(instance, null);
            }
            public void GoToLastKeyframe(object instance)
            {
                if (instance == null) return;
                mi_GoToLastKeyframe.Invoke(instance, null);
            }
        }
        protected class UAnimationKeyTime
        {
            protected MethodInfo mi_Time;

            public UAnimationKeyTime(Assembly asmUnityEditor)
            {
                var animationKeyTimeType  = asmUnityEditor.GetType("UnityEditorInternal.AnimationKeyTime");
                mi_Time = animationKeyTimeType.GetMethod("Time", BindingFlags.Public | BindingFlags.Static);
            }
            public object Time(float time, float frameRate)
            {
                return mi_Time.Invoke(null, new object[] { time, frameRate });
            }
        }
        protected class UAnimationWindowCurve
        {
            private PropertyInfo pi_binding;
            private PropertyInfo pi_propertyName;
            private PropertyInfo pi_path;
            private MethodInfo mi_GetHashCode;
            private MethodInfo mi_FindKeyAtTime;

            public UAnimationWindowCurve(Assembly asmUnityEditor)
            {
                var animationWindowCurveType = asmUnityEditor.GetType("UnityEditorInternal.AnimationWindowCurve");
                Assert.IsNotNull(pi_binding = animationWindowCurveType.GetProperty("binding"));
                Assert.IsNotNull(pi_propertyName = animationWindowCurveType.GetProperty("propertyName"));
                Assert.IsNotNull(pi_path = animationWindowCurveType.GetProperty("path"));
                Assert.IsNotNull(mi_GetHashCode = animationWindowCurveType.GetMethod("GetHashCode"));
                Assert.IsNotNull(mi_FindKeyAtTime = animationWindowCurveType.GetMethod("FindKeyAtTime"));
            }

            public EditorCurveBinding GetBinding(object instance)
            {
                if (instance == null) return new EditorCurveBinding();
                return (EditorCurveBinding)pi_binding.GetValue(instance, null);
            }
            public string GetPropertyName(object instance)
            {
                if (instance == null) return null;
                return (string)pi_propertyName.GetValue(instance, null);
            }
            public string GetPath(object instance)
            {
                if (instance == null) return null;
                return (string)pi_path.GetValue(instance, null);
            }
            public int GetHashCode(object instance)
            {
                if (instance == null) return -1;
                return (int)mi_GetHashCode.Invoke(instance, null);
            }
            public object FindKeyAtTime(object instance, object keyTime)
            {
                if (instance == null) return null;
                return mi_FindKeyAtTime.Invoke(instance, new object[] { keyTime });
            }
        }
        protected class UAnimationWindowSelectionItem
        {
            private Func<GameObject> dg_get_gameObject;
            private Action<GameObject> dg_set_gameObject;
            private Func<AnimationClip> dg_get_animationClip;
            private Action<AnimationClip> dg_set_animationClip;

            public UAnimationWindowSelectionItem(Assembly asmUnityEditor)
            {
            }

            public GameObject GetGameObject(object instance)
            {
                if (instance == null) return null;
                if (dg_get_gameObject == null || dg_get_gameObject.Target != instance)
                    dg_get_gameObject = (Func<GameObject>)Delegate.CreateDelegate(typeof(Func<GameObject>), instance, instance.GetType().GetProperty("gameObject").GetGetMethod());
                return dg_get_gameObject();
            }
            public void SetGameObject(object instance, GameObject gameObject)
            {
                if (instance == null) return;
                if (dg_set_gameObject == null || dg_set_gameObject.Target != instance)
                    dg_set_gameObject = (Action<GameObject>)Delegate.CreateDelegate(typeof(Action<GameObject>), instance, instance.GetType().GetProperty("gameObject").GetSetMethod());
                dg_set_gameObject(gameObject);
            }

            public AnimationClip GetAnimationClip(object instance)
            {
                if (instance == null) return null;
                if (dg_get_animationClip == null || dg_get_animationClip.Target != instance)
                    dg_get_animationClip = (Func<AnimationClip>)Delegate.CreateDelegate(typeof(Func<AnimationClip>), instance, instance.GetType().GetProperty("animationClip").GetGetMethod());
                return dg_get_animationClip();
            }
            public void SetAnimationClip(object instance, AnimationClip clip)
            {
                if (instance == null) return;
                if (dg_set_animationClip == null || dg_set_animationClip.Target != instance)
                    dg_set_animationClip = (Action<AnimationClip>)Delegate.CreateDelegate(typeof(Action<AnimationClip>), instance, instance.GetType().GetProperty("animationClip").GetSetMethod());
                dg_set_animationClip(clip);
            }
        }
        protected class UAnimationWindowHierarchyDataSource
        {
            private MethodInfo mi_FindItem;

            public UAnimationWindowHierarchyDataSource(Assembly asmUnityEditor)
            {
                var animationWindowHierarchyDataSourceType = asmUnityEditor.GetType("UnityEditorInternal.AnimationWindowHierarchyDataSource");
                Assert.IsNotNull(mi_FindItem = animationWindowHierarchyDataSourceType.GetMethod("FindItem", BindingFlags.Public | BindingFlags.Instance));
            }
            public object FindItem(object instance, int id)
            {
                if (instance == null) return null;
                return mi_FindItem.Invoke(instance, new object[] { id });
            }
        }
        protected class UAnimationWindowHierarchyNode
        {
            private Func<object, IList> dg_get_curves;

            public UAnimationWindowHierarchyNode(Assembly asmUnityEditor)
            {
                var animationWindowHierarchyNodeType = asmUnityEditor.GetType("UnityEditorInternal.AnimationWindowHierarchyNode");
                Assert.IsNotNull(dg_get_curves = EditorCommon.CreateGetFieldDelegate<IList>(animationWindowHierarchyNodeType.GetField("curves", BindingFlags.Public | BindingFlags.Instance)));
            }

            public IList GetCurves(object instance)
            {
                if (instance == null) return null;
                return dg_get_curves(instance);
            }
        }
        protected class UDopeLine
        {
            private PropertyInfo pi_curves;
            private PropertyInfo pi_hierarchyNodeID;
            private Func<object, Type> dg_get_objectType;

            public UDopeLine(Assembly asmUnityEditor)
            {
                var dopeLineType = asmUnityEditor.GetType("UnityEditorInternal.DopeLine");
                Assert.IsNotNull(pi_curves = dopeLineType.GetProperty("curves"));
                Assert.IsNotNull(pi_hierarchyNodeID = dopeLineType.GetProperty("hierarchyNodeID"));
                Assert.IsNotNull(dg_get_objectType = EditorCommon.CreateGetFieldDelegate<Type>(dopeLineType.GetField("objectType")));
            }

            public Type GetObjectType(object instance)
            {
                if (instance == null) return null;
                return dg_get_objectType(instance);
            }
            public IList GetCurves(object instance)
            {
                if (instance == null) return null;
                return (IList)pi_curves.GetValue(instance, null);
            }
            public int GetHierarchyNodeID(object instance)
            {
                if (instance == null) return -1;
                return (int)pi_hierarchyNodeID.GetValue(instance, null);
            }
        }

        protected UEditorWindow uEditorWindow;
        protected UAnimationWindowUtility uAnimationWindowUtility;
        protected UAnimEditor uAnimEditor;
        protected UCurveEditor uCurveEditor;
        protected UAnimationWindowState uAnimationWindowState;
        protected UAnimationWindowControl uAnimationWindowControl;
        protected UAnimationKeyTime uAnimationKeyTime;
        protected UAnimationWindowCurve uAnimationWindowCurve;
        protected UAnimationWindowSelectionItem uAnimationWindowSelectionItem;
        protected UAnimationWindowHierarchyDataSource uAnimationWindowHierarchyDataSource;
        protected UAnimationWindowHierarchyNode uAnimationWindowHierarchyNode;
        protected UDopeLine uDopeLine;

        protected object animEditorInstance
        {
            get
            {
                var aw = instance;
                if (aw == null) return null;
                return dg_get_m_AnimEditor(aw);
            }
        }
        protected object animationWindowStateInstance
        {
            get
            {
                return uAnimEditor.GetAnimationWindowState(animEditorInstance);
            }
        }
        protected object animationWindowControlInstance
        {
            get
            {
                return uAnimationWindowState.GetControlInterface(animationWindowStateInstance);
            }
        }

        protected object selectedItem
        {
            get
            {
                var ae = animEditorInstance;
                var si = uAnimEditor.GetSelectedItem(ae);
                if (si == null)
                {
                    if (!HasFocus())
                        instance.Focus();
                    si = uAnimEditor.GetSelectedItem(ae);
                    if (si == null)
                        return null;
                }
                return si;
            }
        }

        public UAnimationWindow()
        {
            var asmUnityEditor = Assembly.LoadFrom(InternalEditorUtility.GetEditorAssemblyPath());
            var animationWindowType = asmUnityEditor.GetType("UnityEditor.AnimationWindow");

            Assert.IsNotNull(dg_get_s_AnimationWindows = EditorCommon.CreateGetFieldDelegate<IList>(animationWindowType.GetField("s_AnimationWindows", BindingFlags.NonPublic | BindingFlags.Static)));
            Assert.IsNotNull(dg_get_m_AnimEditor = EditorCommon.CreateGetFieldDelegate<object>(animationWindowType.GetField("m_AnimEditor", BindingFlags.NonPublic | BindingFlags.Instance)));
            {
                var fi_m_Locked = animationWindowType.GetField("m_Locked", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi_m_Locked != null)
                {
                    Assert.IsNotNull(dg_get_m_Locked = EditorCommon.CreateGetFieldDelegate<bool>(fi_m_Locked));
                    Assert.IsNotNull(dg_set_m_Locked = EditorCommon.CreateSetFieldDelegate<bool>(fi_m_Locked));
                }
            }

            uEditorWindow = new UEditorWindow();
            uAnimationWindowUtility = new UAnimationWindowUtility();
            uAnimEditor = new UAnimEditor(asmUnityEditor);
            uCurveEditor = new UCurveEditor(asmUnityEditor);
            uAnimationWindowState = new UAnimationWindowState(asmUnityEditor);
            uAnimationWindowControl = new UAnimationWindowControl(asmUnityEditor);
            uAnimationKeyTime = new UAnimationKeyTime(asmUnityEditor);
            uAnimationWindowCurve = new UAnimationWindowCurve(asmUnityEditor);
            uAnimationWindowSelectionItem = new UAnimationWindowSelectionItem(asmUnityEditor);
            uAnimationWindowHierarchyDataSource = new UAnimationWindowHierarchyDataSource(asmUnityEditor);
            uAnimationWindowHierarchyNode = new UAnimationWindowHierarchyNode(asmUnityEditor);
            uDopeLine = new UDopeLine(asmUnityEditor);
        }

        public EditorWindow instance
        {
            get
            {
                EditorWindow result = null;
                {
                    var list = dg_get_s_AnimationWindows(null);
                    if (list.Count > 0)
                        result = list[0] as EditorWindow;
                }
                return result;
            }
        }

        public virtual GameObject GetActiveRootGameObject()
        {
            return uAnimationWindowState.GetActiveRootGameObject(animationWindowStateInstance);
        }
        public virtual Component GetActiveAnimationPlayer()
        {
            return uAnimationWindowState.GetActiveAnimationPlayer(animationWindowStateInstance);
        }

        public AnimationClip GetSelectionAnimationClip()
        {
            if (instance == null) return null;
            return uAnimationWindowSelectionItem.GetAnimationClip(selectedItem);
        }
        public void SetSelectionAnimationClip(AnimationClip animationClip, string undoName = null)
        {
            if (instance == null) return;
            if (GetSelectionAnimationClip() == animationClip) return;
            var si = selectedItem;
            if (si == null) return;
            if (undoName != null)
                Undo.RecordObject(selectedItem as UnityEngine.Object, undoName);
            uAnimationWindowSelectionItem.SetAnimationClip(selectedItem, animationClip);
            ForceRefresh();
        }

        public virtual void RecordingDisable()
        {
            var aws = animationWindowStateInstance;
            if (!uAnimationWindowState.GetRecording(aws))
                return;
            uAnimationWindowState.StopRecording(aws);
        }
        public void RecordingChange()
        {
            var aws = animationWindowStateInstance;
            var recording = uAnimationWindowState.GetRecording(aws);
            recording = !recording;
            if (recording)
                uAnimationWindowState.StartRecording(aws);
            else
                uAnimationWindowState.StopRecording(aws);
        }
        public bool GetCanRecord()
        {
            return uAnimationWindowControl.GetCanRecord(animationWindowControlInstance);
        }
        public bool GetRecording()
        {
            return uAnimationWindowState.GetRecording(animationWindowStateInstance);
        }
        
        public void PlayingChange()
        {
            var aws = animationWindowStateInstance;
            if (!HasFocus())
                instance.Focus();
            var playing = uAnimationWindowState.GetPlaying(aws);
            playing = !playing;
            if (playing)
                uAnimationWindowState.StartPlayback(aws);
            else
                uAnimationWindowState.StopPlayback(aws);
        }
        public bool GetPlaying()
        {
            return uAnimationWindowState.GetPlaying(animationWindowStateInstance);
        }

        public int GetCurrentFrame()
        {
            return uAnimationWindowState.GetCurrentFrame(animationWindowStateInstance);
        }
        public void SetCurrentFrame(int frame)
        {
            uAnimationWindowState.SetCurrentFrame(animationWindowStateInstance, frame);
            Repaint();
        }
        public void MoveFrame(int add)
        {
            var aws = animationWindowStateInstance;
            var frame = uAnimationWindowState.GetCurrentFrame(aws);
            if (frame + add < 0)
            {
                if (frame == 0)
                    return;
                uAnimationWindowState.SetCurrentFrame(aws, 0);
            }
            else
            {
                uAnimationWindowState.SetCurrentFrame(aws, frame + add);
            }
            Repaint();
        }
        public int GetLastFrame(AnimationClip clip)
        {
            return Mathf.RoundToInt(clip.length * clip.frameRate);
        }
        public float GetFrameTime(int frame, AnimationClip clip)
        {
            return SnapToFrame(frame * (1f / clip.frameRate), clip.frameRate);
        }

        public float GetCurrentTime()
        {
            return uAnimationWindowState.GetCurrentTime(animationWindowStateInstance);
        }
        public void SetCurrentTime(float time)
        {
            uAnimationWindowState.SetCurrentTime(animationWindowStateInstance, time);
            Repaint();
        }

        public float SnapToFrame(float time, float fps)
        {
            return uAnimationWindowState.SnapToFrame(animationWindowStateInstance, time, fps);
        }
        public int TimeToFrameRound(float time)
        {
            return uAnimationWindowState.TimeToFrameRound(animationWindowStateInstance, time);
        }

        public virtual void MoveToNextFrame()
        {
            MoveFrame(1);
            Repaint();
        }
        public virtual void MoveToPrevFrame()
        {
            MoveFrame(-1);
            Repaint();
        }
        public void MoveToNextKeyframe()
        {
            uAnimationWindowControl.GoToNextKeyframe(animationWindowControlInstance);
            Repaint();
        }
        public void MoveToPreviousKeyframe()
        {
            uAnimationWindowControl.GoToPreviousKeyframe(animationWindowControlInstance);
            Repaint();
        }
        public void MoveToFirstKeyframe()
        {
            uAnimationWindowControl.GoToFirstKeyframe(animationWindowControlInstance);
            Repaint();
        }
        public void MoveToLastKeyframe()
        {
            uAnimationWindowControl.GoToLastKeyframe(animationWindowControlInstance);
            Repaint();
        }

        public void SwitchBetweenCurvesAndDopesheet()
        {
            uAnimEditor.SwitchBetweenCurvesAndDopesheet(animEditorInstance);
            Repaint();
        }
        public bool IsShowCurveEditor()
        {
            return uAnimationWindowState.GetShowCurveEditor(animationWindowStateInstance);
        }

        public void ClearKeySelections()
        {
            var ae = animEditorInstance;
            var aws = animationWindowStateInstance;
            if (ae == null || aws == null)
                return;
            if (IsShowCurveEditor())
            {
                var curveEditor = uAnimEditor.GetCurveEditor(ae);
                if (curveEditor != null)
                {
                    if (uCurveEditor.HasSelection(curveEditor))
                    {
                        uCurveEditor.ClearSelection(curveEditor);
                        Repaint();
                    }
                }
            }
            else
            {
                var list = uAnimationWindowState.GetSelectedKeyHashes(aws);
                if (list != null)
                {
                    var e = list.GetEnumerator();
                    if (e.MoveNext())
                    {
                        uAnimationWindowState.ClearKeySelections(aws);
                        Repaint();
                    }
                }
            }
        }

        public TreeViewState GetHierarchyState()
        {
            return uAnimationWindowState.GetHierarchyState(animationWindowStateInstance);
        }
        public void SynchroCurveSelection(List<EditorCurveBinding> bindings)
        {
            if (vaw == null || va == null)
                return;
            var ae = animEditorInstance;
            var aws = animationWindowStateInstance;
            if (ae == null || aws == null)
                return;

            uAnimationWindowState.ClearKeySelections(aws);
            uAnimationWindowState.ClearHierarchySelection(aws);

            if (bindings.Count > 0)
            {
                var animationKeyTime = uAnimationKeyTime.Time(GetCurrentTime(), GetSelectionAnimationClip().frameRate);
                foreach (object dopeline in uAnimationWindowState.GetDopelines(aws))
                {
                    foreach (var curve in uDopeLine.GetCurves(dopeline))
                    {
                        var cbinding = uAnimationWindowCurve.GetBinding(curve);
                        foreach (var binding in bindings)
                        {
                            if (binding == cbinding)
                            {
                                uAnimationWindowState.SelectHierarchyItem(aws, uDopeLine.GetHierarchyNodeID(dopeline), true, false);
                                var keyframe = uAnimationWindowCurve.FindKeyAtTime(curve, animationKeyTime);
                                if (keyframe != null)
                                    uAnimationWindowState.SelectKey(aws, keyframe);
                                break;
                            }
                        }
                    }
                }
                if (IsShowCurveEditor())
                {
                    uAnimEditor.UpdateSelectedKeysToCurveEditor(ae);
                }
            }

            uAnimEditor.SetTriggerFraming(ae);

            Repaint();
        }
        public List<EditorCurveBinding> GetCurveSelection()
        {
            var list = new List<EditorCurveBinding>();
            var activeCurves = uAnimationWindowState.GetActiveCurves(animationWindowStateInstance);
            if (activeCurves == null)
                return list;
            foreach (var curve in activeCurves)
            {
                var cbinding = uAnimationWindowCurve.GetBinding(curve);
                list.Add(cbinding);
            }
            return list;
        }

        public List<EditorCurveBinding> GetMissingCurveBindings()
        {
            var aws = animationWindowStateInstance;
            List<EditorCurveBinding> list = new List<EditorCurveBinding>();
            var hierarchyData = uAnimationWindowState.GetHierarchyData(aws);
            foreach (object dopeline in uAnimationWindowState.GetDopelines(aws))
            {
                var hierarchyNodeID = uDopeLine.GetHierarchyNodeID(dopeline);
                var windowHierarchyNode = uAnimationWindowHierarchyDataSource.FindItem(hierarchyData, hierarchyNodeID);
                if (windowHierarchyNode == null) continue;
                if (uAnimationWindowUtility.IsNodeLeftOverCurve(windowHierarchyNode))
                {
                    var curves = uAnimationWindowHierarchyNode.GetCurves(windowHierarchyNode);
                    if (curves == null) continue;
                    foreach (var curve in curves)
                    {
                        if (curve == null) continue;
                        var binding = uAnimationWindowCurve.GetBinding(curve);
                        list.Add(binding);
                    }
                }
            }
            return list;
        }

        public bool IsDoneRefresh()
        {
            var refresh = uAnimationWindowState.GetRefresh(animationWindowStateInstance);
            return refresh == UAnimationWindowState.RefreshType.None;
        }
        public void ForceRefresh()
        {
            uAnimationWindowState.ForceRefresh(animationWindowStateInstance);
            Repaint();
        }
        
        public void CurveWasModified(AnimationClip clip, EditorCurveBinding binding, AnimationUtility.CurveModifiedType type)
        {
            var aws = animationWindowStateInstance;
            uAnimationWindowState.CurveWasModified(aws, clip, binding, type);
            if (type == AnimationUtility.CurveModifiedType.CurveDeleted)
            {
                Repaint();
            }
        }

        public void ResampleAnimation()
        {
            uAnimationWindowControl.ResampleAnimation(animationWindowControlInstance);
        }

        public void Repaint()
        {
            if (!HasFocus()) return;

            var list = dg_get_s_AnimationWindows(null);
            if (list.Count > 0)
            {
                (list[0] as EditorWindow).Repaint();
                #region OtherAnimationWindows
                if (list.Count > 1)
                {
                    var clip = GetSelectionAnimationClip();
                    for (int i = 1; i < list.Count; i++)
                    {
                        var ew = list[i] as EditorWindow;
                        if (uEditorWindow.HasFocus(ew))
                        {
                            var ae = dg_get_m_AnimEditor(ew);
                            var si = uAnimEditor.GetSelectedItem(ae);
                            if (si != null)
                            {
                                var sclip = uAnimationWindowSelectionItem.GetAnimationClip(si);
                                if (clip == sclip)
                                {
                                    var aws = uAnimEditor.GetAnimationWindowState(ae);
                                    uAnimationWindowState.ForceRefresh(aws);
                                    ew.Repaint();
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }

        public bool HasFocus()
        {
            return uEditorWindow.HasFocus(instance);
        }

        public virtual bool GetLock(EditorWindow aw)
        {
            return dg_get_m_Locked(aw);
        }
        public virtual void SetLock(EditorWindow aw, bool flag)
        {
            dg_set_m_Locked(aw, flag);
        }
    }
}
