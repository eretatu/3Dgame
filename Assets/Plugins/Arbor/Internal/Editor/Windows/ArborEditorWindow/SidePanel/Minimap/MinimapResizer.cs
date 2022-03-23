//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ArborEditor
{
	[System.Reflection.Obfuscation(Exclude = true)]
	[System.Serializable]
	internal class MinimapResizer
	{
		// The raw preview size while dragging (not snapped to allowed values) (shared)
		static float s_DraggedPreviewSize = 0;
		// The returned preview size while dragging (shared)
		static float s_CachedPreviewSizeWhileDragging = 0;
		static float s_MouseDownLocation, s_MouseDownValue;
		static bool s_MouseDragged;

		// The last saved preview size - only saved when not dragging
		// The saved value is the size when expanded - when collapsed the value is negative,
		// so it can be restored when expanded again.
		private float pref
		{
			get
			{
				return ArborSettings.minimapSize;
			}
			set
			{
				ArborSettings.minimapSize = value;
			}
		}

		private static int s_MinimapResizerHas = "s_MinimapResizerHas".GetHashCode();

		private int m_Id = 0;

		private int id
		{
			get
			{
				if (m_Id == 0)
					m_Id = EditorGUIUtility.GetControlID(s_MinimapResizerHas, FocusType.Passive, new Rect());
				return m_Id;
			}
		}

		public float ResizeHandle(Rect windowPosition, float minSize, float minRemainingSize, Rect resizerRect)
		{
			return ResizeHandle(windowPosition, minSize, minRemainingSize, resizerRect, new Rect());
		}

		public float ResizeHandle(Rect windowPosition, float minSize, float maxPreviewSize, Rect resizerRect, Rect dragRect)
		{
			// Sanity check the cached value. It can be positive or negative, but never smaller than the minSize
			if (Mathf.Abs(pref) < minSize)
				pref = minSize * Mathf.Sign(pref);

			bool dragging = (GUIUtility.hotControl == id);

			float previewSize = (dragging ? s_DraggedPreviewSize : Mathf.Max(0, pref));
			bool expanded = (pref > 0);
			float lastSize = Mathf.Abs(pref);

			if (dragRect.width != 0)
			{
				resizerRect.x = dragRect.x;
				resizerRect.width = dragRect.width;
			}

			bool expandedBefore = expanded;
			previewSize = -PixelPreciseCollapsibleSlider(id, resizerRect, -previewSize, -maxPreviewSize, -0, ref expanded);
			previewSize = Mathf.Min(previewSize, maxPreviewSize);
			dragging = (GUIUtility.hotControl == id);

			if (dragging)
				s_DraggedPreviewSize = previewSize;

			// First snap size between 0 and minimum size
			if (previewSize < minSize)
				previewSize = (previewSize < minSize * 0.5f ? 0 : minSize);

			// If user clicked area, adjust size
			if (expanded != expandedBefore)
			{
				previewSize = (expanded ? lastSize : 0);
				GUI.changed = true;
			}

			// Determine new expanded state
			expanded = (previewSize >= minSize / 2);

			// Keep track of last preview size while not dragging or collapsed
			// Note we don't want to save when dragging preview OR window size,
			// so just don't save while dragging anything at all
			if (GUIUtility.hotControl == 0)
			{
				if (previewSize > 0)
					lastSize = previewSize;
				float newPref = lastSize * (expanded ? 1 : -1);
				if (newPref != pref)
				{
					// Save the value to prefs
					pref = newPref;
				}
			}

			s_CachedPreviewSizeWhileDragging = previewSize;
			return previewSize;
		}

		// This value will change in realtime while dragging
		public bool GetExpanded()
		{
			if (GUIUtility.hotControl == id)
				return (s_CachedPreviewSizeWhileDragging > 0);
			else
				return (pref > 0);
		}

		public float GetPreviewSize()
		{
			if (GUIUtility.hotControl == id)
				return Mathf.Max(0, s_CachedPreviewSizeWhileDragging);
			else
				return Mathf.Max(0, pref);
		}

		// This value won't change until we have stopped dragging again
		public bool GetExpandedBeforeDragging()
		{
			return (pref > 0);
		}

		public void SetExpanded(bool expanded)
		{
			if (GetExpanded() == expanded)
				return;

			// Set the sign based on whether it's collapsed or not, then save to prefs
			pref = Mathf.Abs(pref) * (expanded ? 1 : -1);
		}

		public void ToggleExpanded()
		{
			// Reverse the sign, then save to prefs
			pref = -pref;
		}

		// This is the slider behavior for resizing the preview area
		public static float PixelPreciseCollapsibleSlider(int id, Rect position, float value, float min, float max, ref bool expanded)
		{
			Event evt = Event.current;

			if (evt.type == EventType.Layout)
			{
				return value;
			}

			var mousePosition = evt.mousePosition;

			switch (evt.GetTypeForControl(id))
			{
				case EventType.MouseDown:
					if (GUIUtility.hotControl == 0 && evt.button == 0 && position.Contains(mousePosition))
					{
						GUIUtility.hotControl = id;
						s_MouseDownLocation = mousePosition.y;
						s_MouseDownValue = value;
						s_MouseDragged = false;
						evt.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == id)
					{
						value = Mathf.Clamp(mousePosition.y - s_MouseDownLocation + s_MouseDownValue, min, max - 1);
						GUI.changed = true;
						s_MouseDragged = true;
						evt.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == id)
					{
						GUIUtility.hotControl = 0;
						if (!s_MouseDragged)
							expanded = !expanded;
						evt.Use();
					}
					break;
				case EventType.Repaint:
					if (GUIUtility.hotControl == 0)
					{
						EditorGUIUtility.AddCursorRect(position, MouseCursor.SplitResizeUpDown);
					}
					if (GUIUtility.hotControl == id)
					{
						const int yMove = 100;
						const int heightMove = yMove * 2;
						EditorGUIUtility.AddCursorRect(new Rect(position.x, position.y - yMove, position.width, position.height + heightMove), MouseCursor.SplitResizeUpDown);
					}
					break;
			}
			return value;
		}
	}
}