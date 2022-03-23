//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	using Arbor;

	internal sealed class StateLinkSelectorWindow : EditorWindow
	{
		private static StateLinkSelectorWindow _Instance;

		public static StateLinkSelectorWindow instance
		{
			get
			{
				if (_Instance == null)
				{
					StateLinkSelectorWindow[] objects = Resources.FindObjectsOfTypeAll<StateLinkSelectorWindow>();
					if (objects.Length > 0)
					{
						_Instance = objects[0];
					}
				}
				if (_Instance == null)
				{
					_Instance = ScriptableObject.CreateInstance<StateLinkSelectorWindow>();
				}
				return _Instance;
			}
		}

		private NodeGraphEditor _NodeGraphEditor;

		public NodeListGUI _NodeListGUI = new NodeListGUI();

		public delegate void OnSelectCallback(NodeEditor nodeEditor);

		public OnSelectCallback onSelect;

		private int _SelectedStateID;

		private StateLinkSelectorWindow()
		{
			_NodeListGUI = new NodeListGUI();
			_NodeListGUI.isShowCallback = IsShowNode;
			_NodeListGUI.sortComparisonCallback = StateMachineGraphEditor.InternalNodeListSortComparison;
			_NodeListGUI.onSelectCallback = OnSelect;
			_NodeListGUI.isSelectionCallback = IsSelectionNode;
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnEnable()
		{
			_NodeListGUI.Initialize(_NodeGraphEditor);
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnDisable()
		{
			_NodeListGUI.Dispose();
		}

		bool IsShowNode(NodeEditor nodeEditor)
		{
			State state = nodeEditor.node as State;
			return (state != null && !state.resident);
		}

		bool IsSelectionNode(NodeEditor nodeEditor)
		{
			return nodeEditor != null && nodeEditor.nodeID == _SelectedStateID;
		}

		public void Open(NodeGraphEditor graphEditor, Rect buttonRect, int selectedStateID, OnSelectCallback onSelect)
		{
			_SelectedStateID = selectedStateID;

			_NodeGraphEditor = graphEditor;

			_NodeListGUI.Initialize(_NodeGraphEditor);

			this.onSelect = onSelect;

			ShowAsDropDown(buttonRect, new Vector2(300f, 320f));

			Focus();
		}

		void OnSelect(NodeEditor nodeEditor)
		{
			if (onSelect != null)
			{
				onSelect(nodeEditor);
			}

			nodeEditor.graphEditor.Repaint();

			Close();
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnGUI()
		{
			_NodeListGUI.OnGUI();
		}
	}
}