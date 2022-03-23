//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System;

namespace ArborEditor
{
	[System.Serializable]
	public sealed class GraphGUI
	{
		private static readonly int s_DragScrollHash = "DragScroll".GetHashCode();

		public EditorWindow hostWindow;

		private Rect _Position;
		private Rect _ViewportPosition;
		private Rect _HorizontalScrollbarPosition;
		private Rect _VerticalScrollbarPosition;

		public Rect position
		{
			get
			{
				return _Position;
			}
			set
			{
				_Position = value;
				_ViewportPosition = _Position;

				float scrollBarWidth = GUI.skin.verticalScrollbar.fixedWidth;
				float scrollBarHeight = GUI.skin.horizontalScrollbar.fixedHeight;

				_ViewportPosition.xMax -= scrollBarWidth;
				_ViewportPosition.yMax -= scrollBarHeight;

				_HorizontalScrollbarPosition = _Position;
				_HorizontalScrollbarPosition.xMax = _ViewportPosition.xMax;
				_HorizontalScrollbarPosition.yMin = _ViewportPosition.yMax;

				_VerticalScrollbarPosition = _Position;
				_VerticalScrollbarPosition.xMin = _ViewportPosition.xMax;
				_VerticalScrollbarPosition.yMax = _ViewportPosition.yMax;
			}
		}

		public Rect viewportPosition
		{
			get
			{
				return _ViewportPosition;
			}
		}

		public Vector2 scrollPos
		{
			get
			{
				return _ScrollPos;
			}
			set
			{
				_ScrollPos = value;
			}
		}

		public Rect extents;
		public Rect viewArea;

		[NonSerialized]
		public GUIContent label = null;

		[SerializeField]
		private Vector2 _ScrollPos;

		private int _CachedHotControl;

		public void HandleScrollbar()
		{
			_ScrollPos.x = GUI.HorizontalScrollbar(_HorizontalScrollbarPosition, _ScrollPos.x, _ViewportPosition.width, extents.xMin, extents.xMax);
			_ScrollPos.y = GUI.VerticalScrollbar(_VerticalScrollbarPosition, _ScrollPos.y, _ViewportPosition.height, extents.yMin, extents.yMax);
		}

		public void DrawBackground(Rect rect)
		{
			EditorGUITools.DrawGridBackground(rect);

			if (label != null)
			{
				Vector2 size = Styles.graphLabel.CalcSize(label);
				Rect labelPosition = rect;
				labelPosition.xMin = labelPosition.xMax - size.x;
				labelPosition.yMin = labelPosition.yMax - size.y;
				GUI.Label(labelPosition, label, Styles.graphLabel);
			}
		}

		public void BeginGraphGUI()
		{
			UpdateGraphViewArea();

			Rect clippedArea = _ViewportPosition;

			GUI.BeginClip(clippedArea, -_ScrollPos, Vector2.zero, false);
		}

		void UpdateGraphViewArea()
		{
			viewArea = new Rect(_ScrollPos.x, _ScrollPos.y, _ViewportPosition.width, _ViewportPosition.height);
		}

		public bool DragGrid()
		{
			bool scrolled = false;

			int controlID = GUIUtility.GetControlID(s_DragScrollHash, FocusType.Passive);

			Event current = Event.current;

			bool dragButton = current.button == 2 || current.alt;

			switch (current.GetTypeForControl(controlID))
			{
				case EventType.MouseDown:
					if (viewArea.Contains(current.mousePosition) && dragButton)
					{
						_CachedHotControl = GUIUtility.hotControl;
						GUIUtility.hotControl = controlID;
						current.Use();
						EditorGUIUtility.SetWantsMouseJumping(1);
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						if (dragButton)
						{
							GUIUtility.hotControl = _CachedHotControl;
							EditorGUIUtility.SetWantsMouseJumping(0);
						}
						current.Use();
					}
					break;
				case EventType.MouseMove:
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
					{
						_ScrollPos -= current.delta;
						_ScrollPos.x = (int)_ScrollPos.x;
						_ScrollPos.y = (int)_ScrollPos.y;

						scrolled = true;

						current.Use();
					}
					break;
				case EventType.Repaint:
					if (GUIUtility.hotControl == controlID)
					{
						EditorGUIUtility.AddCursorRect(viewArea, MouseCursor.Pan, controlID);
					}
					break;
			}

			return scrolled;
		}

		public void EndGraphGUI()
		{
			GUI.EndClip();

			if (Event.current.type == EventType.ScrollWheel && _ViewportPosition.Contains(Event.current.mousePosition))
			{
				_ScrollPos.x = (int)Mathf.Clamp(_ScrollPos.x + Event.current.delta.x * 20f, extents.xMin, extents.xMax - _ViewportPosition.width);
				_ScrollPos.y = (int)Mathf.Clamp(_ScrollPos.y + Event.current.delta.y * 20f, extents.yMin, extents.yMax - _ViewportPosition.height);

				UpdateGraphViewArea();

				Event.current.Use();
			}
		}
	}
}
