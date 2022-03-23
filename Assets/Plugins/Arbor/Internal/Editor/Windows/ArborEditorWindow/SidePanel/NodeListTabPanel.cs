//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ArborEditor
{
	using Arbor;

	internal class NodeListTabPanel : Panel
	{
		public override void OnGUI(Rect position)
		{
			NodeGraphEditor graphEditor = hostWindow.graphEditor;
			if (graphEditor == null || graphEditor.nodeGraph == null)
			{
				return;
			}

			EditorGUIUtility.labelWidth = 100;

			using (new ProfilerScope("NodeListPanel"))
			{
				graphEditor.NodeListPanelGUI();
			}

			EditorGUIUtility.labelWidth = 0f;
		}
	}
}