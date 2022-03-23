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

	[CustomNodeEditor(typeof(CompositeNode))]
	internal sealed class CompositeNodeEditor : TreeBehaviourNodeEditor
	{
		CompositeNode compositeNode
		{
			get
			{
				return node as CompositeNode;
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
				graphEditor.BeginRename(compositeNode.nodeID, compositeNode.name);
			}
		}

		public override void OnRename(string name)
		{
			if (name != compositeNode.name)
			{
				NodeGraph nodeGraph = compositeNode.nodeGraph;

				Undo.RecordObject(nodeGraph, "Rename State");

				compositeNode.name = name;

				EditorUtility.SetDirty(nodeGraph);
			}
		}

		protected override void OnPreHeaderGUI()
		{
			using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
			{
				GUILayout.FlexibleSpace();
				ParentLinkSlot(compositeNode.parentLink);
				GUILayout.FlexibleSpace();
			}
		}

		protected override void OnFooterGUI()
		{
			using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
			{
				GUILayout.FlexibleSpace();
				ChildLinkSlot(compositeNode.childrenLink);
				GUILayout.FlexibleSpace();
			}
		}

		protected override Texture2D GetDefaultIcon()
		{
			return Icons.defaultCompositeIcon;
		}

		void OnReplaceComposite(Vector2 position, System.Type classType)
		{
			string oldCompositeName = string.Empty;

			CompositeBehaviour oldComposite = compositeNode.behaviour as CompositeBehaviour;
			if ((object)oldComposite != null)
			{
				BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(oldComposite.GetType());
				oldCompositeName = behaviourInfo.titleContent.text;
			}

			Undo.IncrementCurrentGroup();

			compositeNode.DestroyBehaviour();
			compositeNode.CreateCompositeBehaviour(classType);

			if (!string.IsNullOrEmpty(oldCompositeName))
			{
				Undo.RecordObject(graphEditor.nodeGraph, "Replaced CompositeBehaviour");

				BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(classType);
				compositeNode.name = compositeNode.name.Replace(oldCompositeName, behaviourInfo.titleContent.text);

				EditorUtility.SetDirty(graphEditor.nodeGraph);
			}

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			graphEditor.RaiseOnChangedNodes();
		}

		void OnReplaceContextMenu(object obj)
		{
			Rect buttonRect = (Rect)obj;

			CompositeBehaviourMenuWindow.instance.Init(graphEditor, Vector2.zero, buttonRect, OnReplaceComposite, null, null);
		}

		protected override void SetReplaceBehaviourMenu(GenericMenu menu, Rect headerPosition, bool editable)
		{
			if (!Application.isPlaying && editable)
			{
				Rect buttonRect = EditorGUITools.GUIToScreenRect(headerPosition);
				menu.AddItem(EditorContents.replaceComposite, false, OnReplaceContextMenu, buttonRect);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.replaceComposite);
			}
		}

		public override void RegisterDragChildren()
		{
			for (int i = 0; i < compositeNode.childrenLink.Count; i++)
			{
				NodeLinkSlot childNodeLink = compositeNode.childrenLink[i];
				RegisterDragChild(childNodeLink);
			}
		}
	}
}