//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace ArborEditor
{
	internal sealed class HighlightControl : Control
	{
		public NodeGraphEditor graphEditor;
		public Rect position;
		public GUIStyle style;

		public override Rect GetPosition()
		{
			return graphEditor.hostWindow.GraphToWindowRect(position);
		}

		public override void OnGUI()
		{
			Rect position = GetPosition();

			Event current = Event.current;

			EventType typeForControl = current.GetTypeForControl(controlID);
			switch (typeForControl)
			{
				case EventType.Repaint:
					style.Draw(position, GUIContent.none, controlID);
					break;
			}
		}
	}
}