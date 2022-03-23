//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

using Arbor;

namespace ArborEditor
{
	internal sealed class NodeCommentControl : Control
	{
		[SerializeField]
		private int _NodeID;

		[System.NonSerialized]
		private Node _Node;

		public int nodeID
		{
			get
			{
				return _NodeID;
			}
		}

		public Node node
		{
			get
			{
				return _Node;
			}
			set
			{
				_Node = value;
				if (_Node != null)
				{
					_NodeID = _Node.nodeID;
				}
				else
				{
					_NodeID = 0;
				}
			}
		}

		public NodeGraphEditor graphEditor
		{
			get;
			set;
		}

		public override Rect GetPosition()
		{
			Node node = this.node;

			GUIStyle style = Styles.nodeCommentField;
			GUIContent content = new GUIContent(node.nodeComment);
			float width = 300f;
			Vector2 size = new Vector3(width, style.CalcHeight(content, width));

			Rect commentRect = new Rect(0, -4f, size.x, size.y);

			commentRect.y -= commentRect.height;

			if (ArborEditorWindow.nodeCommentAffectsZoom)
			{
				commentRect.position += node.position.position;
			}
			else
			{
				commentRect.position += graphEditor.hostWindow.GraphToWindowPoint(node.position.position);
			}

			return commentRect;
		}

		public override void OnGUI()
		{
			Node node = this.node;

			GUIStyle style = Styles.nodeCommentField;
			Rect commentRect = GetPosition();

			EditorGUI.BeginDisabledGroup(!graphEditor.editable);

			EditorGUI.BeginChangeCheck();
			string nodeComment = EditorGUITools.TextArea(commentRect, node.nodeComment, controlID, style);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(node.nodeGraph, "Edit Node Comment");

				node.nodeComment = nodeComment;

				EditorUtility.SetDirty(node.nodeGraph);
			}

			Event current = Event.current;
			EventType typeForControl = current.GetTypeForControl(controlID);
			if (typeForControl == EventType.MouseMove && commentRect.Contains(current.mousePosition))
			{
				current.Use();
			}

			EditorGUI.EndDisabledGroup();
		}
	}
}