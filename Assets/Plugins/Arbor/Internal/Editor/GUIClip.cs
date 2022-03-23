//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using System.Reflection;

namespace ArborEditor
{
	internal sealed class GUIClip
	{
		private delegate Rect DelegateGetTopmostRect();
		private static readonly DelegateGetTopmostRect s_GetTopmostRect;

		private delegate Rect DelegateGetVisibleRect();
		private static readonly DelegateGetVisibleRect s_GetVisibleRect;

		private delegate Vector2 DelegateClipVector2(Vector2 absolutePos);
		private static readonly DelegateClipVector2 s_ClipVector2;

		private delegate Rect DelegateClipRect(Rect absoluteRect);
		private static readonly DelegateClipRect s_ClipRect;

		private delegate Vector2 DelegateUnclipVector2(Vector2 pos);
		private static readonly DelegateUnclipVector2 s_UnclipVector2;

		private delegate Rect DelegateUnclipRect(Rect rect);
		private static readonly DelegateUnclipRect s_UnclipRect;

		public static Rect topmostRect
		{
			get
			{
				return s_GetTopmostRect();
			}
		}

		public static Rect visibleRect
		{
			get
			{
				return s_GetVisibleRect();
			}
		}

		static GUIClip()
		{
			Assembly assembly = Assembly.Load("UnityEngine.dll");
			System.Type guiClipType = assembly.GetType("UnityEngine.GUIClip");

			PropertyInfo topmostRectProperty = guiClipType.GetProperty("topmostRect", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			s_GetTopmostRect = (DelegateGetTopmostRect)System.Delegate.CreateDelegate(typeof(DelegateGetTopmostRect), topmostRectProperty.GetGetMethod(true));

			PropertyInfo visibleRectProperty = guiClipType.GetProperty("visibleRect", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			s_GetVisibleRect = (DelegateGetVisibleRect)System.Delegate.CreateDelegate(typeof(DelegateGetVisibleRect), visibleRectProperty.GetGetMethod(true));

			s_ClipVector2 = EditorGUITools.GetDelegate<DelegateClipVector2>(guiClipType, "Clip", BindingFlags.Static | BindingFlags.Public);
			s_ClipRect = EditorGUITools.GetDelegate<DelegateClipRect>(guiClipType, "Clip", BindingFlags.Static | BindingFlags.Public);
			s_UnclipVector2 = EditorGUITools.GetDelegate<DelegateUnclipVector2>(guiClipType, "Unclip", BindingFlags.Static | BindingFlags.Public);
			s_UnclipRect = EditorGUITools.GetDelegate<DelegateUnclipRect>(guiClipType, "Unclip", BindingFlags.Static | BindingFlags.Public);
		}

		public static Vector2 Clip(Vector2 absolutePos)
		{
			return s_ClipVector2(absolutePos);
		}

		public static Rect Clip(Rect absoluteRect)
		{
			return s_ClipRect(absoluteRect);
		}

		public static Vector2 Unclip(Vector2 pos)
		{
			return s_UnclipVector2(pos);
		}

		public static Rect Unclip(Rect rect)
		{
			return s_UnclipRect(rect);
		}
	}
}
