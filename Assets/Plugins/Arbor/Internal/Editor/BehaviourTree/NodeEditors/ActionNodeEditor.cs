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

	[CustomNodeEditor(typeof(ActionNode))]
	internal sealed class ActionNodeEditor : TreeBehaviourNodeEditor
	{
		ActionNode actionNode
		{
			get
			{
				return node as ActionNode;
			}
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		protected override void OnEnable()
		{
			base.OnEnable();

			isRenamable = true;
			isShowableComment = true;
		}

		protected override void BeginRename()
		{
			if (graphEditor != null)
			{
				graphEditor.BeginRename(actionNode.nodeID, actionNode.name);
			}
		}

		public override void OnRename(string name)
		{
			if (name != actionNode.name)
			{
				NodeGraph nodeGraph = actionNode.nodeGraph;

				Undo.RecordObject(nodeGraph, "Rename State");

				actionNode.name = name;

				EditorUtility.SetDirty(nodeGraph);
			}
		}

		protected override void OnPreHeaderGUI()
		{
			using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
			{
				GUILayout.FlexibleSpace();
				ParentLinkSlot(actionNode.parentLink);
				GUILayout.FlexibleSpace();
			}
		}

		protected override Styles.Color GetNormalStyleColor()
		{
			return Styles.Color.Purple;
		}

		protected override Texture2D GetDefaultIcon()
		{
			return Icons.defaultActionIcon;
		}

		void OnReplaceAction(Vector2 position, System.Type classType)
		{
			string oldActionName = string.Empty;

			ActionBehaviour oldAction = actionNode.behaviour as ActionBehaviour;
			if ((object)oldAction != null)
			{
				BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(oldAction.GetType());
				oldActionName = behaviourInfo.titleContent.text;
			}

			Undo.IncrementCurrentGroup();

			actionNode.DestroyBehaviour();
			actionNode.CreateActionBehaviour(classType);

			if (!string.IsNullOrEmpty(oldActionName))
			{
				Undo.RecordObject(graphEditor.nodeGraph, "Replaced ActionBehaviour");

				BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(classType);
				actionNode.name = actionNode.name.Replace(oldActionName, behaviourInfo.titleContent.text);

				EditorUtility.SetDirty(graphEditor.nodeGraph);
			}

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			graphEditor.RaiseOnChangedNodes();
		}

		void OnReplaceContextMenu(object obj)
		{
			Rect buttonRect = (Rect)obj;

			ActionBehaviourMenuWindow.instance.Init(graphEditor, Vector2.zero, buttonRect, OnReplaceAction, null, null);
		}

		protected override void SetReplaceBehaviourMenu(GenericMenu menu, Rect headerPosition, bool editable)
		{
			if (!Application.isPlaying && editable)
			{
				Rect buttonRect = EditorGUITools.GUIToScreenRect(headerPosition);
				menu.AddItem(EditorContents.replaceAction, false, OnReplaceContextMenu, buttonRect);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.replaceAction);
			}
		}
	}
}