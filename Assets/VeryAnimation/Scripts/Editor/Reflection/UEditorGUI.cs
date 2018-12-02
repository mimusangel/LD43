using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Reflection;

namespace VeryAnimation
{
    public class UEditorGUI
    {
        private Func<GUIContent> dg_titleSettingsIcon;
        private Func<GUIContent> dg_helpIcon;

        public UEditorGUI()
        {
            var asmUnityEditor = Assembly.LoadFrom(InternalEditorUtility.GetEditorAssemblyPath());
            var gUIContentsType = asmUnityEditor.GetType("UnityEditor.EditorGUI+GUIContents");

            Assert.IsNotNull(dg_titleSettingsIcon = (Func<GUIContent>)Delegate.CreateDelegate(typeof(Func<GUIContent>), null, gUIContentsType.GetProperty("titleSettingsIcon", BindingFlags.NonPublic | BindingFlags.Static).GetGetMethod(true)));
            Assert.IsNotNull(dg_helpIcon = (Func<GUIContent>)Delegate.CreateDelegate(typeof(Func<GUIContent>), null, gUIContentsType.GetProperty("helpIcon", BindingFlags.NonPublic | BindingFlags.Static).GetGetMethod(true)));
        }

        public GUIContent GetTitleSettingsIcon()
        {
            return dg_titleSettingsIcon();
        }
        public GUIContent GetHelpIcon()
        {
            return dg_helpIcon();
        }
    }
}
