//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace ArborEditor
{
	using Arbor;
	using Arbor.DynamicReflection;

	internal static class EditorWindowExtensions
	{
		private static readonly DynamicField s_ParentField;
		private static readonly DynamicMethod s_GetBorderSizeMethod;
		private static readonly DynamicMethod s_HasFocusMethod;

		static EditorWindowExtensions()
		{
			FieldInfo parentField = typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
			if (parentField != null)
			{
				s_ParentField = DynamicField.GetField(parentField);
			}

			Assembly assemblyUnityEditor = Assembly.Load("UnityEditor.dll");
			System.Type hostViewType = assemblyUnityEditor.GetType("UnityEditor.HostView");

			PropertyInfo borderSizeProperty = hostViewType.GetProperty("borderSize", BindingFlags.Instance | BindingFlags.NonPublic);
			if (borderSizeProperty != null)
			{
				s_GetBorderSizeMethod = DynamicMethod.GetMethod(borderSizeProperty.GetGetMethod(true));
			}

			PropertyInfo hasFocusProperty = hostViewType.GetProperty("hasFocus", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			if (hasFocusProperty != null)
			{
				s_HasFocusMethod = DynamicMethod.GetMethod(hasFocusProperty.GetGetMethod());
			}
		}

		public static bool HasBorderSize()
		{
			return s_ParentField != null && s_GetBorderSizeMethod != null;
		}

		public static RectOffset GetBorderSize(this EditorWindow window)
		{
			var hostView = s_ParentField.GetValue(window);
			return (RectOffset)s_GetBorderSizeMethod.Invoke(hostView, null);
		}

		public static bool HasFocus(this EditorWindow window)
		{
			var hostView = s_ParentField.GetValue(window);
			return (bool)s_HasFocusMethod.Invoke(hostView, null);
		}
	}
}