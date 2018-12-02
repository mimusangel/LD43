using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Reflection;

namespace VeryAnimation
{
    public class UEditorGUIUtility
    {
        private Func<object, int> dg_get_s_LastControlID;

        public UEditorGUIUtility()
        {
            var asmUnityEditor = Assembly.LoadFrom(InternalEditorUtility.GetEditorAssemblyPath());
            var editorGUIUtilityType = asmUnityEditor.GetType("UnityEditor.EditorGUIUtility");
            Assert.IsNotNull(dg_get_s_LastControlID = EditorCommon.CreateGetFieldDelegate<int>(editorGUIUtilityType.GetField("s_LastControlID", BindingFlags.NonPublic | BindingFlags.Static)));
        }

        public int GetLastControlID()
        {
            return dg_get_s_LastControlID(null);
        }
    }
}
