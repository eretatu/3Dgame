//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

using Arbor;

namespace ArborEditor
{
	[CustomNodeEditor(typeof(CommentNode))]
	internal sealed class CommentEditor : NodeEditor
	{
		public CommentNode comment
		{
			get
			{
				return node as CommentNode;
			}
		}

		public override GUIContent GetTitleContent()
		{
			return EditorContents.commentNodeTitle;
		}

		public override Styles.Color GetStyleColor()
		{
			return Styles.Color.Yellow;
		}

		void CommentField()
		{
			GUIStyle style = EditorStyles.textArea;

			EditorGUI.BeginDisabledGroup(!graphEditor.editable);

			EditorGUI.BeginChangeCheck();
			string commentText = EditorGUILayout.TextArea(comment.comment, style);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(comment.nodeGraph, "Change Comment");

				comment.comment = commentText;

				EditorUtility.SetDirty(comment.nodeGraph);
			}

			EditorGUI.EndDisabledGroup();
		}

		protected override void OnGUI()
		{
			using (new ProfilerScope("OnCommentGUI"))
			{
				EditorGUITools.DrawSeparator(ArborEditorWindow.isDarkSkin);

				CommentField();
			}
		}

		public override string GetTitle()
		{
			return Localization.GetWord("Comment");
		}
	}
}
