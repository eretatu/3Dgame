//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.BehaviourTree
{
	using Arbor;
	using Arbor.BehaviourTree;

	[System.Reflection.Obfuscation(Exclude = true)]
	[System.Serializable]
	internal sealed class ServiceEditorList : BehaviourEditorList<ServiceEditorGUI, Service>
	{
		public TreeBehaviourNodeEditor treeNodeEditor
		{
			get
			{
				return nodeEditor as TreeBehaviourNodeEditor;
			}
		}

		public TreeBehaviourNode treeNode
		{
			get
			{
				return node as TreeBehaviourNode;
			}
		}

		private static readonly Color kBackGroundColor = new Color(0.9f, 1.0f, 0.9f);

		public override Color backgroundColor
		{
			get
			{
				return kBackGroundColor;
			}
		}
		public override GUIStyle backgroundStyle
		{
			get
			{
				return Styles.treeBehaviourBackground;
			}
		}

		public override System.Type targetType
		{
			get
			{
				return typeof(Service);
			}
		}

		public override GUIContent GetAddBehaviourContent()
		{
			return EditorContents.addService;
		}

		public override GUIContent GetInsertButtonContent()
		{
			return EditorContents.insertService;
		}

		public override GUIContent GetPasteBehaviourContent()
		{
			return EditorContents.pasteService;
		}

		public override Object GetObject(int behaviourIndex)
		{
			return treeNode.serviceList[behaviourIndex];
		}

		public override int GetCount()
		{
			return treeNode.serviceList.count;
		}

		public override void InsertBehaviour(int index, System.Type classType)
		{
			treeNodeEditor.InsertService(index, classType);
		}

		public override void MoveBehaviour(Node fromNode, int fromIndex, Node toNode, int toIndex, bool isCopy)
		{
			TreeBehaviourNode fromTreeNode = fromNode as TreeBehaviourNode;
			TreeBehaviourNode toTreeNode = toNode as TreeBehaviourNode;

			NodeGraph nodeGraph = fromTreeNode.nodeGraph;

			Undo.IncrementCurrentGroup();

			Undo.RecordObject(nodeGraph, isCopy ? "Paste Behaviour" : "Move Behaviour");

			if (isCopy)
			{
				Service destService = fromTreeNode.serviceList[fromIndex];
				TreeBehaviourNodeEditor.PasteServiceAsNew(toTreeNode, toIndex, destService);
			}
			else
			{
				fromTreeNode.MoveService(fromIndex, toTreeNode, toIndex);
			}

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			EditorUtility.SetDirty(nodeGraph);

			graphEditor.RaiseOnChangedNodes();
		}

		public override void PasteBehaviour(int index)
		{
			treeNodeEditor.PasteService(index);
		}

		public override void OpenBehaviourMenu(Rect buttonRect, int index)
		{
			ServiceMenuWindow.instance.Init(treeNodeEditor, buttonRect, index);
		}
	}
}