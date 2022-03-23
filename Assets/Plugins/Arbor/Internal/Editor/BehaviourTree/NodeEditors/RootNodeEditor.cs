//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.BehaviourTree
{
	using Arbor.BehaviourTree;

	[CustomNodeEditor(typeof(RootNode))]
	internal sealed class RootNodeEditor : TreeNodeBaseEditor
	{
		public RootNode rootNode
		{
			get
			{
				return node as RootNode;
			}
		}

		public override string GetTitle()
		{
			return Localization.GetWord("Root");
		}

		protected override float GetWidth()
		{
			return 180f;
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnEnable()
		{
			isShowContextMenuInHeader = false;
			isShowableComment = true;
			isResizable = false;
		}

		protected override void OnFooterGUI()
		{
			using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
			{
				GUILayout.FlexibleSpace();
				ChildLinkSlot(rootNode.childNodeLink);
				GUILayout.FlexibleSpace();
			}
		}

		protected override Styles.Color GetNormalStyleColor()
		{
			return Styles.Color.Aqua;
		}

		public override Texture2D GetIcon()
		{
			return Icons.rootIcon;
		}

		public override bool IsCopyable()
		{
			return false;
		}

		public override void RegisterDragChildren()
		{
			RegisterDragChild(rootNode.childNodeLink);
		}
	}
}