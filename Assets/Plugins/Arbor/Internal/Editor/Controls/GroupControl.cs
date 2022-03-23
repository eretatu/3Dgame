//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

using Arbor;

namespace ArborEditor
{
	internal sealed class GroupControl : Control
	{
		[SerializeField]
		private GroupNodeEditor _GroupNodeEditor;

		public GroupNodeEditor groupNodeEditor
		{
			get
			{
				return _GroupNodeEditor;
			}
		}

		public int groupNodeID
		{
			get
			{
				return _GroupNodeEditor.nodeID;
			}
		}

		public GroupNode groupNode
		{
			get
			{
				return _GroupNodeEditor.node as GroupNode;
			}
		}

		public NodeGraphEditor graphEditor
		{
			get
			{
				return _GroupNodeEditor.graphEditor;
			}
		}

		public void Initialize(GroupNodeEditor groupNodeEditor)
		{
			_GroupNodeEditor = groupNodeEditor;
		}

		public override Rect GetPosition()
		{
			return groupNode.position;
		}

		private static readonly int s_HeaderHash = "s_HeaderHash".GetHashCode();
		private static readonly int s_ResizeHash = "s_ResizeHash".GetHashCode();
		private static readonly int s_DragNodesHash = "s_DragNodesHash".GetHashCode();

		private const int s_ResizeEdgeSize = 8;

		private int _HeaderControlID;
		private int _ResizeControlID;
		private int _DragNodesControlID;

		protected override void OnUpdateControlID()
		{
			_HeaderControlID = GUIUtility.GetControlID(s_HeaderHash, FocusType.Passive);
			_ResizeControlID = GUIUtility.GetControlID(s_ResizeHash, FocusType.Passive);
			_DragNodesControlID = GUIUtility.GetControlID(s_DragNodesHash, FocusType.Passive);
		}

		void ResizeGUI()
		{
			NodeGraphEditor graphEditor = this.graphEditor;

			if (!graphEditor.editable)
			{
				return;
			}

			GroupNode group = groupNode;

			Vector2 minSize = new Vector2(300, 100);

			Rect groupPosition = group.position;
			//groupPosition.position = Vector2.zero;

			Rect resizeEdge = new RectOffset(s_ResizeEdgeSize, s_ResizeEdgeSize, s_ResizeEdgeSize, s_ResizeEdgeSize).Add(groupPosition);

			Rect resizeTopEdge = resizeEdge;
			resizeTopEdge.yMax = groupPosition.yMin;

			Rect resizeBottomEdge = resizeEdge;
			resizeBottomEdge.yMin = groupPosition.yMax;

			Rect resizeLeftEdge = resizeEdge;
			resizeLeftEdge.xMax = groupPosition.xMin;

			Rect resizeRightEdge = resizeEdge;
			resizeRightEdge.xMin = groupPosition.xMax;

			Event current = Event.current;

			int controlID = _ResizeControlID;

			EventType typeForControl = current.GetTypeForControl(controlID);
			switch (typeForControl)
			{
				case EventType.MouseDown:
					if (current.button == 0)
					{
						bool isResize = false;
						NodeGraphEditor.ResizeControlPointFlags resizeControlPoint = 0;

						if (resizeTopEdge.Contains(current.mousePosition))
						{
							isResize = true;
							resizeControlPoint |= NodeGraphEditor.ResizeControlPointFlags.Top;
						}
						else if (resizeBottomEdge.Contains(current.mousePosition))
						{
							isResize = true;
							resizeControlPoint |= NodeGraphEditor.ResizeControlPointFlags.Bottom;
						}

						if (resizeLeftEdge.Contains(current.mousePosition))
						{
							isResize = true;
							resizeControlPoint |= NodeGraphEditor.ResizeControlPointFlags.Left;
						}
						else if (resizeRightEdge.Contains(current.mousePosition))
						{
							isResize = true;
							resizeControlPoint |= NodeGraphEditor.ResizeControlPointFlags.Right;
						}

						if (isResize)
						{
							Undo.IncrementCurrentGroup();

							graphEditor.OnBeginResizeNode(current, group, resizeControlPoint);
							graphEditor.BeginDisableContextClick();

							GUIUtility.hotControl = controlID;
							GUIUtility.keyboardControl = 0;
							current.Use();
						}
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						if (current.button == 0)
						{
							graphEditor.EndDisableContextClick();
							GUIUtility.hotControl = 0;
						}

						current.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
					{
						graphEditor.OnResizeNode(current, group, minSize);

						current.Use();
						break;
					}
					break;
				case EventType.KeyDown:
					if (GUIUtility.hotControl == controlID && current.keyCode == KeyCode.Escape)
					{
						GUIUtility.hotControl = 0;
						current.Use();

						Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
					}
					break;
				case EventType.Repaint:
					if (GUIUtility.hotControl == controlID)
					{
						Rect currentCursorRect = new Rect();
						currentCursorRect.position = current.mousePosition;
						currentCursorRect.min -= Vector2.one * 1000;
						currentCursorRect.max += Vector2.one * 1000;

						NodeGraphEditor.ResizeControlPointFlags controlPoint = graphEditor.GetResizeControlPoint();

						if ((controlPoint & NodeGraphEditor.ResizeControlPointFlags.TopLeft) == NodeGraphEditor.ResizeControlPointFlags.TopLeft ||
							(controlPoint & NodeGraphEditor.ResizeControlPointFlags.BottomRight) == NodeGraphEditor.ResizeControlPointFlags.BottomRight)
						{
							EditorGUIUtility.AddCursorRect(currentCursorRect, MouseCursor.ResizeUpLeft, controlID);
						}
						else if ((controlPoint & NodeGraphEditor.ResizeControlPointFlags.TopRight) == NodeGraphEditor.ResizeControlPointFlags.TopRight ||
						   (controlPoint & NodeGraphEditor.ResizeControlPointFlags.BottomLeft) == NodeGraphEditor.ResizeControlPointFlags.BottomLeft)
						{
							EditorGUIUtility.AddCursorRect(currentCursorRect, MouseCursor.ResizeUpRight, controlID);
						}
						else if ((controlPoint & (NodeGraphEditor.ResizeControlPointFlags.Top | NodeGraphEditor.ResizeControlPointFlags.Bottom)) != 0)
						{
							EditorGUIUtility.AddCursorRect(currentCursorRect, MouseCursor.ResizeVertical, controlID);
						}
						else if ((controlPoint & (NodeGraphEditor.ResizeControlPointFlags.Left | NodeGraphEditor.ResizeControlPointFlags.Right)) != 0)
						{
							EditorGUIUtility.AddCursorRect(currentCursorRect, MouseCursor.ResizeHorizontal, controlID);
						}
					}
					else
					{
						Rect rectTopLeft = resizeLeftEdge;
						rectTopLeft.yMax = resizeTopEdge.yMax;

						EditorGUIUtility.AddCursorRect(rectTopLeft, MouseCursor.ResizeUpLeft);

						Rect rectTopRight = resizeRightEdge;
						rectTopRight.yMax = resizeTopEdge.yMax;

						EditorGUIUtility.AddCursorRect(rectTopRight, MouseCursor.ResizeUpRight);

						Rect rectBottomLeft = resizeLeftEdge;
						rectBottomLeft.yMin = resizeBottomEdge.yMin;

						EditorGUIUtility.AddCursorRect(rectBottomLeft, MouseCursor.ResizeUpRight);

						Rect rectBottomRight = resizeRightEdge;
						rectBottomRight.yMin = resizeBottomEdge.yMin;

						EditorGUIUtility.AddCursorRect(rectBottomRight, MouseCursor.ResizeUpLeft);

						EditorGUIUtility.AddCursorRect(resizeTopEdge, MouseCursor.ResizeVertical);
						EditorGUIUtility.AddCursorRect(resizeBottomEdge, MouseCursor.ResizeVertical);
						EditorGUIUtility.AddCursorRect(resizeLeftEdge, MouseCursor.ResizeHorizontal);
						EditorGUIUtility.AddCursorRect(resizeRightEdge, MouseCursor.ResizeHorizontal);
					}
					break;
			}
		}

		void OnGroupGUI()
		{
			using (new ProfilerScope("OnGroupGUI"))
			{
				GroupNode group = groupNode;

				Rect nodePosition = group.position;
				nodePosition.x = nodePosition.y = 0;

				GUI.Box(nodePosition, GUIContent.none, groupNodeEditor.GetBaseStyle());

				GUIStyle headerStyle = groupNodeEditor.GetHeaderStyle();
				
				Rect headerPosition = new Rect(nodePosition.x, nodePosition.y, nodePosition.width, headerStyle.CalcHeight(GUIContent.none, nodePosition.width));

				groupNodeEditor.HeaderGUI(headerPosition, _HeaderControlID);

				groupNodeEditor.DragNodes(_DragNodesControlID);
			}
		}

		public override void OnGUI()
		{
			ResizeGUI();

			GUIStyle nodeStyle = groupNodeEditor.GetFrameStyle();
			GUI.BeginGroup(groupNode.position, GUIContent.none, nodeStyle);

			Color backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = groupNode.color;

			OnGroupGUI();

			GUI.backgroundColor = backgroundColor;

			GUI.EndGroup();
		}
	}
}