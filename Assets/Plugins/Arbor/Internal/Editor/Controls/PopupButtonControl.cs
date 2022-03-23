//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	internal sealed class PopupButtonControl : Control
	{
		public NodeGraphEditor graphEditor;
		public Rect position;
		public GUIContent content;
		public GUIStyle style;
		public System.Action<Rect> onClick;
		public int activeControlID;

		public override Rect GetPosition()
		{
			if (style != null)
			{
				Rect rect = new Rect();
				rect.size = style.CalcSize(content);
				Vector2 pos = graphEditor.hostWindow.GraphToWindowPoint(new Vector2(position.x, position.center.y));
				rect.x = pos.x;
				rect.y = pos.y - rect.height / 2f;
				return rect;
			}
			return graphEditor.hostWindow.GraphToWindowRect(position);
		}

		public override void OnGUI()
		{
			Rect position = GetPosition();

			Event current = Event.current;

			EventType typeForControl = current.GetTypeForControl(controlID);
			switch (typeForControl)
			{
				case EventType.MouseDown:
					if (position.Contains(current.mousePosition))
					{
						if (onClick != null)
						{
							onClick(position);
						}

						current.Use();

						graphEditor.ClosePopupButtonControl(this);
					}
					break;
				case EventType.MouseMove:
					if (position.Contains(current.mousePosition))
					{
						current.Use();
					}
					else
					{
						graphEditor.ClosePopupButtonControl(this);
					}
					break;
				case EventType.Repaint:
					EditorGUI.DropShadowLabel(position, content, style);
					break;
			}
		}
	}
}