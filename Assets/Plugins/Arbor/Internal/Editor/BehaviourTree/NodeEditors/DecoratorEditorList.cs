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
	internal sealed class DecoratorEditorList : BehaviourEditorList<DecoratorEditorGUI, Decorator>
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

		private static readonly Color kBackGroundColor = new Color(0.9f, 0.9f, 1.0f);

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
				return typeof(Decorator);
			}
		}

		public override GUIContent GetAddBehaviourContent()
		{
			return EditorContents.addDecorator;
		}

		public override GUIContent GetInsertButtonContent()
		{
			return EditorContents.insertDecorator;
		}

		public override GUIContent GetPasteBehaviourContent()
		{
			return EditorContents.pasteDecorator;
		}

		public override Object GetObject(int behaviourIndex)
		{
			return treeNode.decoratorList[behaviourIndex];
		}

		public override int GetCount()
		{
			return treeNode.decoratorList.count;
		}

		public override void InsertBehaviour(int index, System.Type classType)
		{
			treeNodeEditor.InsertDecorator(index, classType);
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
				Decorator destDecorator = fromTreeNode.decoratorList[fromIndex];
				TreeBehaviourNodeEditor.PasteDecoratorAsNew(toTreeNode, toIndex, destDecorator);
			}
			else
			{
				fromTreeNode.MoveDecorator(fromIndex, toTreeNode, toIndex);
			}

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			EditorUtility.SetDirty(nodeGraph);

			graphEditor.RaiseOnChangedNodes();
		}

		public override void PasteBehaviour(int index)
		{
			treeNodeEditor.PasteDecorator(index);
		}

		public override void OpenBehaviourMenu(Rect buttonRect, int index)
		{
			DecoratorMenuWindow.instance.Init(treeNodeEditor, buttonRect, index);
		}
	}
}