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
	using ArborEditor.IMGUI.Controls;

	[System.Reflection.Obfuscation(Exclude = true)]
	[System.Serializable]
	internal class GraphTabPanel : Panel
	{
		private GraphTreeViewGUI _TreeViewGUI = null;

		public override void Setup(ArborEditorWindow hostWindow)
		{
			base.Setup(hostWindow);

			_TreeViewGUI = new GraphTreeViewGUI(hostWindow, hostWindow.treeView, hostWindow.treeViewState);
			_TreeViewGUI.onSubmit += OnSubmit;
			_TreeViewGUI.contextClickItemCallback += OnContextClickItem;
			_TreeViewGUI.onRenameEnded += OnRenameEnded;
			_TreeViewGUI.selectSubmit = true;
			_TreeViewGUI.renamable = true;
		}

		public void SelectNopdeGraph(GraphTreeViewItem graphItem)
		{
			if (graphItem == null)
			{
				return;
			}

			if (graphItem != null && !_TreeViewGUI.IsSelected(graphItem))
			{
				_TreeViewGUI.SetSelectedItem(graphItem, true);

				var parent = graphItem.parent;
				while (parent != null)
				{
					_TreeViewGUI.SetExpanded(parent, true);
					parent = parent.parent;
				}
			}
		}

		public void DirtyTree()
		{
			_TreeViewGUI.DirtyTree();
		}

		void OnSubmit(TreeViewItem item)
		{
			var graphItem = item as GraphTreeViewItem;
			if (graphItem == null)
			{
				return;
			}

			hostWindow.ChangeCurrentNodeGraph(graphItem);
		}

		void OnContextClickItem(TreeViewItem item)
		{
			Event evt = Event.current;

			evt.Use();

			var graphItem = item as GraphTreeViewItem;
			if (graphItem == null)
			{
				return;
			}

			GenericMenu menu = new GenericMenu();

			if (graphItem.disable || graphItem.isExternal)
			{
				menu.AddDisabledItem(EditorContents.rename);
			}
			else
			{
				menu.AddItem(EditorContents.rename, false, () => {
					_TreeViewGUI.BeginNameEditing(0f);
				});
			}

			menu.ShowAsContext();
		}

		void OnRenameEnded(RenameEndedArgs args)
		{
			string name = string.IsNullOrEmpty(args.newName) ? args.originalName : args.newName;
			int instanceID = args.itemID;
			bool userAccepted = args.acceptedRename;

			if (userAccepted)
			{
				GraphTreeViewItem valueItem = hostWindow.treeView.FindItem(instanceID) as GraphTreeViewItem;
				NodeGraph nodeGraph = valueItem != null ? valueItem.nodeGraph : null;
				if (nodeGraph != null)
				{
					SetGraphName(nodeGraph, name);
				}
			}
		}

		void SetGraphName(NodeGraph nodeGraph, string graphName)
		{
			Undo.RecordObject(nodeGraph, "Change Graph Name");

			nodeGraph.graphName = graphName;

			EditorUtility.SetDirty(nodeGraph);
		}

		void ExternalHeaderGUI()
		{
			Rect rect = GUILayoutUtility.GetRect(10f, 25f);

			NodeGraph externalGraph = hostWindow.rootGraph;
			GUI.Box(rect, GUIContent.none, Styles.header);

			GUIStyle leftArrowStyle = Styles.leftArrow;
			Vector2 leftArrowSize = new Vector2(leftArrowStyle.fixedWidth, leftArrowStyle.fixedHeight);

			Rect interactionRect = new Rect(
				rect.x, rect.y + (rect.height - leftArrowSize.y)*0.5f,
				leftArrowSize.x + leftArrowStyle.margin.horizontal,
				leftArrowSize.y);

			if (Event.current.type == EventType.Repaint)
			{
				float oldW = leftArrowStyle.fixedWidth, oldH = leftArrowStyle.fixedHeight;

				leftArrowStyle.fixedWidth = 0;
				leftArrowStyle.fixedHeight = 0;
				leftArrowStyle.Draw(interactionRect, GUIContent.none, interactionRect.Contains(Event.current.mousePosition), false, false, false);
				leftArrowStyle.fixedWidth = oldW;
				leftArrowStyle.fixedHeight = oldH;
			}

			if (GUI.Button(interactionRect, GUIContent.none, GUIStyle.none))
			{
				hostWindow.SelectRootGraph(hostWindow.rootGraphPrev);
			}

			GUIContent content = EditorGUITools.GetTextContent(externalGraph.graphName);

			EditorGUIUtility.SetIconSize(new Vector2(16, 16));
			Vector2 labelSize = EditorStyles.boldLabel.CalcSize(content);
			float xStart = leftArrowStyle.margin.left + leftArrowStyle.fixedWidth;
			float space = rect.width;
			float offsetFromStart = Mathf.Max(xStart, (space - labelSize.x) / 2);
			Rect labelRect = new Rect(offsetFromStart, rect.y + (rect.height - labelSize.y)*0.5f, rect.width - xStart, labelSize.y);
			if (GUI.Button(labelRect, content, EditorStyles.boldLabel))
			{
				EditorGUIUtility.PingObject(externalGraph.gameObject);
			}
			EditorGUIUtility.SetIconSize(Vector2.zero);
		}

		public override void OnFocus(bool focused)
		{
			_TreeViewGUI.OnFocus(focused);
		}

		public override void OnGUI(Rect position)
		{
			NodeGraphEditor graphEditor = hostWindow.graphEditor;
			if (graphEditor == null || graphEditor.nodeGraph == null)
			{
				return;
			}

			EditorGUIUtility.labelWidth = 100;
			
			bool editable = graphEditor.editable;

			using (new ProfilerScope("GraphPanel"))
			{
				if (hostWindow.rootGraphPrev != null)
				{
					ExternalHeaderGUI();
				}
				if (_TreeViewGUI != null)
				{
					_TreeViewGUI.renamable = editable;

					_TreeViewGUI.DoTreeViewGUI();
				}
			}

			EditorGUIUtility.labelWidth = 0f;
		}
	}
}
