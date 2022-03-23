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

	[System.Serializable]
	public class TreeNodeBehaviourEditorGUI : BehaviourEditorGUI
	{
		public TreeBehaviourNodeEditor treeBehaviourEditor
		{
			get
			{
				return nodeEditor as TreeBehaviourNodeEditor;
			}
		}

		public TreeBehaviourNode treeBehaviourNode
		{
			get
			{
				return (nodeEditor != null) ? nodeEditor.node as TreeBehaviourNode : null;
			}
		}

		public TreeNodeBehaviour treeNodeBehaviour
		{
			get
			{
				return behaviourObj as TreeNodeBehaviour;
			}
		}

		protected override bool HasTitlebar()
		{
			return true;
		}

		public override bool GetExpanded()
		{
			return (treeNodeBehaviour != null) ? BehaviourEditorUtility.GetExpanded(treeNodeBehaviour, treeNodeBehaviour.expanded) : this.expanded;
		}

		public override void SetExpanded(bool expanded)
		{
			if (treeNodeBehaviour != null)
			{
				if ((treeNodeBehaviour.hideFlags & HideFlags.NotEditable) != HideFlags.NotEditable)
				{
					treeNodeBehaviour.expanded = expanded;
					EditorUtility.SetDirty(treeNodeBehaviour);
				}
				BehaviourEditorUtility.SetExpanded(treeNodeBehaviour, expanded);
			}
			else
			{
				this.expanded = expanded;
			}
		}

		protected override void SetPopupMenu(GenericMenu menu)
		{
			bool editable = nodeEditor.graphEditor.editable;

			NodeBehaviour behaviour = behaviourObj as NodeBehaviour;
			if (behaviour != null)
			{
				menu.AddItem(EditorContents.copy, false, CopyBehaviourContextMenu);
				if (Clipboard.CompareBehaviourType(behaviourObj.GetType(), false) && editable)
				{
					menu.AddItem(EditorContents.pasteValues, false, PasteBehaviourContextMenu);
				}
				else
				{
					menu.AddDisabledItem(EditorContents.pasteValues);
				}
			}
		}

		void CopyBehaviourContextMenu()
		{
			NodeBehaviour behaviour = behaviourObj as NodeBehaviour;

			Clipboard.CopyBehaviour(behaviour);
		}

		void PasteBehaviourContextMenu()
		{
			NodeBehaviour behaviour = behaviourObj as NodeBehaviour;

			Undo.IncrementCurrentGroup();

			Undo.RecordObject(behaviour, "Paste Behaviour");

			Clipboard.PasteBehaviourValues(behaviour);

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			EditorUtility.SetDirty(behaviour);
		}
	}
}