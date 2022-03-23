//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

using Arbor;

namespace ArborEditor
{
	public abstract class NodeEditor : ScriptableObject
	{
		public static NodeEditor currentEditor = null;

		private struct CurrentEditorScope : System.IDisposable
		{
			private bool _Disposed;

			public CurrentEditorScope(NodeEditor editor)
			{
				_Disposed = false;
				NodeEditor.currentEditor = editor;
			}

			public void Dispose()
			{
				if (_Disposed)
				{
					return;
				}

				_Disposed = true;
				NodeEditor.currentEditor = null;
			}
		}

		private struct OutsideScore : System.IDisposable
		{
			private bool _Disposed;
			private NodeEditor _Editor;

			public OutsideScore(NodeEditor editor)
			{
				_Disposed = false;
				_Editor = editor;

				_Editor._InOutsideGUI = true;
			}

			public void Dispose()
			{
				if (_Disposed)
				{
					return;
				}

				_Disposed = true;
				_Editor._InOutsideGUI = false;
				_Editor = null;
			}
		}

		public static bool HasEditor(Node node)
		{
			if (node == null)
			{
				return false;
			}

			System.Type classType = node.GetType();
			System.Type editorType = CustomAttributes<CustomNodeEditor, NodeEditor>.FindEditorType(classType);

			return editorType != null;
		}

		public static NodeEditor CreateEditors(NodeGraphEditor graphEditor, Node node)
		{
			if (node == null)
			{
				return null;
			}

			System.Type classType = node.GetType();
			System.Type editorType = CustomAttributes<CustomNodeEditor, NodeEditor>.FindEditorType(classType);

			if (editorType == null)
			{
				return null;
			}

			NodeEditor nodeEditor = CreateInstance(editorType) as NodeEditor;
			nodeEditor.hideFlags = HideFlags.HideAndDontSave;
			nodeEditor.Initialize(graphEditor, node);

			return nodeEditor;
		}

		[SerializeField]
		private int _NodeID;

		[SerializeField]
		private NodeGraphEditor _GraphEditor;

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
		}

		public NodeGraphEditor graphEditor
		{
			get
			{
				return _GraphEditor;
			}
		}

		public bool isSelection
		{
			get
			{
				if (_GraphEditor != null)
				{
					return _GraphEditor.IsSelection(_Node);
				}
				return false;
			}
		}

		public bool isRenamable = false;
		public bool isShowableComment = false;
		public bool isNormalInvisibleStyle = false;
		public bool isShowContextMenuInHeader = true;
		public bool isShowContextMenuInWindow = false;
		public bool isUsedMouseDownOnMainGUI = true;

		[SerializeField]
		private bool _IsResizable = true;

		public bool isResizable
		{
			get
			{
				return _IsResizable;
			}
			set
			{
				if (_IsResizable != value)
				{
					_IsResizable = value;

					_IsDirtyWidth = true;
				}
			}
		}

		private bool _IsDirtyWidth = false;

		private bool _InOutsideGUI = false;

		public virtual void Validate(Node node)
		{
			_Node = node;
		}

		public bool IsValidNode(Node node)
		{
			if (node == null)
			{
				return false;
			}

			System.Type classType = node.GetType();
			System.Type editorType = CustomAttributes<CustomNodeEditor, NodeEditor>.FindEditorType(classType);

			if (editorType == null)
			{
				return false;
			}

			return editorType == GetType();
		}

		public void Initialize(NodeGraphEditor graphEditor, Node node)
		{
			_NodeID = node.nodeID;

			_GraphEditor = graphEditor;
			_Node = node;

			OnInitialize();
		}

		private GUIStyle _BaseStyle = null;
		private GUIStyle _FrameStyle = null;
		private Styles.BaseColor _StyleBaseColor;
		private bool _IsSelection;
		private bool _IsActive;
		private bool _IsHover = false;

		public virtual bool IsActive()
		{
			return false;
		}

		public void UpdateStyles()
		{
			Styles.BaseColor color = GetStyleBaseColor();
			bool isActive = IsActive();
			bool isHover = _IsHover;

			if (isNormalInvisibleStyle && Event.current.type == EventType.MouseMove)
			{
				isHover = _Node.position.Contains(Event.current.mousePosition);
			}

			if (_BaseStyle == null || _FrameStyle == null || color != _StyleBaseColor || isSelection != _IsSelection || isActive != _IsActive || isHover != _IsHover)
			{
				if (isNormalInvisibleStyle && !isSelection && !isActive && !isHover)
				{
					_BaseStyle = GUIStyle.none;
					_FrameStyle = GUIStyle.none;
				}
				else
				{
					_BaseStyle = Styles.GetNodeBaseStyle(color);
					_FrameStyle = Styles.GetNodeFrameStyle(isSelection, isActive);
				}

				if (_IsHover != isHover)
				{
					Repaint();
				}

				_StyleBaseColor = color;
				_IsSelection = isSelection;
				_IsActive = isActive;
				_IsHover = isHover;
			}
		}

		public GUIStyle GetFrameStyle()
		{
			return _FrameStyle;
		}

		public GUIStyle GetBaseStyle()
		{
			return _BaseStyle;
		}

		private GUIStyle _HeaderStyle = null;
		private Styles.Color _StyleHeaderColor;

		public GUIStyle GetHeaderStyle()
		{
			Styles.Color color = GetStyleColor();
			if (_HeaderStyle == null || color != _StyleHeaderColor)
			{
				_HeaderStyle = Styles.GetNodeHeaderStyle(color);
				_StyleHeaderColor = color;
			}
			return _HeaderStyle;
		}

		private GUIContent _TitleContent = new GUIContent();
		private GUIContent _ListElementContent = new GUIContent();

		public virtual GUIContent GetTitleContent()
		{
			_TitleContent.text = GetTitle();

			return _TitleContent;
		}

		public virtual GUIContent GetListElementContent()
		{
			_ListElementContent.text = GetTitle();
			_ListElementContent.image = GetIcon();

			return _ListElementContent;
		}

		public virtual Styles.BaseColor GetStyleBaseColor()
		{
			return Styles.BaseColor.Gray;
		}

		public virtual Styles.Color GetStyleColor()
		{
			return Styles.Color.Gray;
		}

		protected virtual float GetWidth()
		{
			return 300f;
		}

		private static class ResizeDefaults
		{
			public const int edgeSize = 8;
			public static readonly RectOffset offset = new RectOffset(edgeSize, edgeSize, 0, 0);
			public static readonly RectOffset offsetZero = new RectOffset();
		}

		private Color _GUIColor;

		internal void DoWindow()
		{
			Rect position = _Node.position;

			if (_IsDirtyWidth)
			{
				float width = GetWidth();

				if (isResizable)
				{
					width = Mathf.Max(width, position.width);
				}

				position.width = width;

				_Node.position = position;

				graphEditor.hostWindow.DirtyGraphExtents();

				_IsDirtyWidth = false;
			}

			position = GetOverflowOffset().Add(position);

			Color backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0f, 0.5f, 0.5f, 0.5f);

			_GUIColor = GUI.color;

			GUI.Window(_Node.nodeID, position, OnWindow, GUIContent.none, Styles.nodeWindow);

			GUI.backgroundColor = backgroundColor;
		}

		internal void DragNodes(int controlId)
		{
			if (_GraphEditor != null)
			{
				Rect selectRect = GetSelectableRect();
				selectRect.position -= node.position.position;
				//selectRect.position += new Vector2(ResizeStyle.offset.left, ResizeStyle.offset.top);

				_GraphEditor.DragNodes(_Node, selectRect, new RectOffset(), controlId);
			}
		}

		private void DragNodes()
		{
			if (_GraphEditor != null)
			{
				Rect selectRect = GetSelectableRect();
				selectRect.position -= node.position.position;

				RectOffset overflowOffset = GetOverflowOffset();
				selectRect.position += new Vector2(overflowOffset.left, overflowOffset.top);

				_GraphEditor.DragNodes(_Node, selectRect, overflowOffset);
			}
		}

		protected virtual bool HasHeaderGUI()
		{
			return true;
		}

		protected virtual void OnPreHeaderGUI()
		{
		}

		protected virtual void OnSubHeaderGUI()
		{
		}

		protected virtual void OnFooterGUI()
		{
		}

		protected virtual GUIStyle GetBackgroundStyle()
		{
			return Styles.hostview;
		}

		public virtual Rect GetSelectableRect()
		{
			return node.position;
		}

		public virtual void OnBeginDrag(Event evt)
		{
			graphEditor.RegisterDragNode(node);
		}

		public bool IsHover(Vector2 position)
		{
			RectOffset overflowOffset = GetOverflowOffset();
			Rect overflowRect = overflowOffset.Add(node.position);
			return overflowRect.Contains(position);
		}

		protected NodeGraphEditor.ResizeControlPointFlags GetResizeControlPoints()
		{
			return NodeGraphEditor.ResizeControlPointFlags.Left | NodeGraphEditor.ResizeControlPointFlags.Right;
		}

		public Vector2 NodeToGraphPoint(Vector2 point)
		{
			point += node.position.position;
			if (_InOutsideGUI)
			{
				RectOffset offset = GetOverflowOffset();
				point.x -= offset.left;
				point.y -= offset.top;
			}
			return point;
		}

		public Rect NodeToGraphRect(Rect rect)
		{
			rect.position = NodeToGraphPoint(rect.position);
			return rect;
		}

		public RectOffset GetOverflowOffset()
		{
			RectOffset offset = GetResizeOffset();

			int left = offset.left;
			int right = offset.right;
			int top = offset.top;
			int bottom = offset.bottom;

			GUIStyle frameStyle = GetFrameStyle();
			if (frameStyle != null)
			{
				left = Mathf.Max(left, frameStyle.overflow.left);
				right = Mathf.Max(right, frameStyle.overflow.right);
				top = Mathf.Max(top, frameStyle.overflow.top);
				bottom = Mathf.Max(bottom, frameStyle.overflow.bottom);
			}

			if (HasOutsideGUI())
			{
				RectOffset outsideOffset = GetOutsideOffset();

				left = Mathf.Max(left, outsideOffset.left);
				right = Mathf.Max(right, outsideOffset.right);
				top = Mathf.Max(top, outsideOffset.top);
				bottom = Mathf.Max(bottom, outsideOffset.bottom);
			}

			return new RectOffset(left, right, top, bottom);
		}

		protected RectOffset GetResizeOffset()
		{
			if (isResizable)
			{
				return ResizeDefaults.offset;
			}

			return ResizeDefaults.offsetZero;
		}

		public bool IsSelectPoint(Vector2 point)
		{
			Rect position = GetSelectableRect();
			return position.Contains(point);
		}

		public bool IsSelectRect(Rect rect)
		{
			Rect position = GetSelectableRect();
			return (position.xMax >= rect.x && position.x <= rect.xMax &&
				position.yMax >= rect.y && position.y <= rect.yMax);
		}

		public enum MinimapLayer
		{
			None,
			Underlay,
			Middle,
			Overlay,
		}

		public virtual MinimapLayer minimapLayer
		{
			get
			{
				return MinimapLayer.Middle;
			}
		}

		protected virtual void OnDrawMinimap(Rect nodeRect)
		{
			Color backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = GetListColor();
			GetListStyle().Draw(nodeRect, false, false, false, false);
			GUI.backgroundColor = backgroundColor;

			//EditorGUI.DrawRect(nodeRect, GetListColor());
		}

		internal void DrawMinimap(Rect nodeRect)
		{
			OnDrawMinimap(nodeRect);
		}

		void DoGUI()
		{
			if (HasHeaderGUI())
			{
				OnPreHeaderGUI();

				HeaderGUI();

				OnSubHeaderGUI();
			}

			Rect guiRect = EditorGUILayout.BeginVertical(GetBackgroundStyle());

			bool hierarchyMode = EditorGUIUtility.hierarchyMode;
			EditorGUIUtility.hierarchyMode = true;

			bool wideMode = EditorGUIUtility.wideMode;
			EditorGUIUtility.wideMode = node.position.width > EditorGUITools.kWideModeMinWidth;

			try
			{
				OnGUI();
			}
			finally
			{
				EditorGUIUtility.wideMode = wideMode;
				EditorGUIUtility.hierarchyMode = hierarchyMode;
			}

			EditorGUILayout.EndVertical();

			Event current = Event.current;
			if (isUsedMouseDownOnMainGUI && current.type == EventType.MouseDown && guiRect.Contains(current.mousePosition))
			{
#if ARBOR_DEBUG
				Debug.Log("NodeEditor : MouseDown to Used.");
#endif
				current.Use();
			}

			OnFooterGUI();
		}

		void ResizeGUI(int controlID)
		{
			if (!isResizable)
			{
				return;
			}

			NodeGraphEditor graphEditor = this.graphEditor;

			if (!graphEditor.editable)
			{
				return;
			}

			Vector2 minSize = new Vector2(GetWidth(), 100);

			Rect nodePosition = node.position;
			nodePosition.position = Vector2.zero;

			NodeGraphEditor.ResizeControlPointFlags points = GetResizeControlPoints();

			RectOffset overflowOffset = GetOverflowOffset();

			Rect outsideRect = overflowOffset.Add(nodePosition);

			RectOffset resizeOffset = GetResizeOffset();

			Rect resizeEdge = resizeOffset.Add(nodePosition);

			Vector2 resizePosition = resizeEdge.position - outsideRect.position;

			Rect resizeTopEdge = new Rect(resizePosition.x, resizePosition.y, nodePosition.width, resizeOffset.top);

			Rect resizeBottomEdge = new Rect(resizePosition.x, resizeTopEdge.yMax + nodePosition.height, nodePosition.width, resizeOffset.bottom);

			Rect resizeLeftEdge = new Rect(resizePosition.x, resizePosition.y, resizeOffset.left, nodePosition.height);

			Rect resizeRightEdge = new Rect(resizeLeftEdge.xMax + nodePosition.width, resizePosition.y, resizeOffset.right, nodePosition.height);

			Event current = Event.current;

			EventType typeForControl = current.GetTypeForControl(controlID);
			switch (typeForControl)
			{
				case EventType.MouseDown:
					if (current.button == 0)
					{
						bool isResize = false;
						NodeGraphEditor.ResizeControlPointFlags resizeControlPoint = 0;

						if ((points & NodeGraphEditor.ResizeControlPointFlags.Top) == NodeGraphEditor.ResizeControlPointFlags.Top && resizeTopEdge.Contains(current.mousePosition))
						{
							isResize = true;
							resizeControlPoint |= NodeGraphEditor.ResizeControlPointFlags.Top;
						}
						else if ((points & NodeGraphEditor.ResizeControlPointFlags.Bottom) == NodeGraphEditor.ResizeControlPointFlags.Bottom && resizeBottomEdge.Contains(current.mousePosition))
						{
							isResize = true;
							resizeControlPoint |= NodeGraphEditor.ResizeControlPointFlags.Bottom;
						}

						if ((points & NodeGraphEditor.ResizeControlPointFlags.Left) == NodeGraphEditor.ResizeControlPointFlags.Left && resizeLeftEdge.Contains(current.mousePosition))
						{
							isResize = true;
							resizeControlPoint |= NodeGraphEditor.ResizeControlPointFlags.Left;
						}
						else if ((points & NodeGraphEditor.ResizeControlPointFlags.Right) == NodeGraphEditor.ResizeControlPointFlags.Right && resizeRightEdge.Contains(current.mousePosition))
						{
							isResize = true;
							resizeControlPoint |= NodeGraphEditor.ResizeControlPointFlags.Right;
						}

						if (isResize)
						{
							Undo.IncrementCurrentGroup();

							graphEditor.OnBeginResizeNode(current, node, resizeControlPoint);
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
						graphEditor.OnResizeNode(current, node, minSize);

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

		private static readonly int s_ResizeHash = "s_ResizeHash".GetHashCode();

		void ResizeGUI()
		{
			int controlID = GUIUtility.GetControlID(s_ResizeHash, FocusType.Passive);
			ResizeGUI(controlID);
		}

		void OnWindow(int id)
		{
			using (new CurrentEditorScope(this))
			{
				Color guiColor = GUI.color;
				GUI.color = _GUIColor;

				if (graphEditor != null && graphEditor.hostWindow != null)
				{
					graphEditor.hostWindow.BeginNode();
				}

				Rect nodePosition = node.position;

				RectOffset overflowOffset = GetOverflowOffset();

				Rect windowRect = overflowOffset.Add(nodePosition);
				windowRect.position = Vector2.zero;

				Rect framePosition = new Rect(overflowOffset.left, overflowOffset.top, nodePosition.width, nodePosition.height);

				Rect position = framePosition;

				GUILayout.BeginArea(position);

				position.position = Vector2.zero;

				// When EventType.Layout, GUIClip.GetTopRect() is an invalid value, so overwrite it with BeginGroup.
				GUI.BeginGroup(position);

				EditorGUIUtility.labelWidth = 0f; // Use default labelWidth

				Rect nodeBaseRect = EditorGUILayout.BeginVertical();

				if (Event.current.type == EventType.Repaint)
				{
					GUIStyle baseStyle = GetBaseStyle();
					if (baseStyle != null)
					{
						baseStyle.Draw(nodeBaseRect, false, false, false, false);
					}
				}

				Vector2 iconSize = EditorGUIUtility.GetIconSize();
				EditorGUIUtility.SetIconSize(Vector2.zero);

				bool isExitGUI = false;

				try
				{
					DoGUI();
				}
				catch (System.Exception ex)
				{
					if (EditorGUITools.ShouldRethrowException(ex))
					{
#if ARBOR_DEBUG
						Debug.Log("catch ExitGUIException");
#endif
						isExitGUI = true;
						throw;
					}
					else
					{
						Debug.LogException(ex);
					}
				}
				finally
				{
					EditorGUIUtility.SetIconSize(iconSize);

					if (isExitGUI)
					{
						if (graphEditor != null && graphEditor.hostWindow != null)
						{
							graphEditor.hostWindow._GUIIsExiting = true;
						}
					}
					else
					{
						EditorGUILayout.EndVertical();

						GUI.EndGroup();

						GUILayout.EndArea();

						Event current = Event.current;

						switch (current.type)
						{
							case EventType.ContextClick:
								if (isShowContextMenuInWindow)
								{
									DoContextMenu(new Rect(current.mousePosition.x, current.mousePosition.y, 0, 0), nodeBaseRect);
								}
								Event.current.Use();
								break;
							case EventType.DragUpdated:
								if (position.Contains(current.mousePosition))
								{
									DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
									current.Use();
								}
								break;
						}

						if (current.type != EventType.Layout && current.type != EventType.Used)
						{
							float height = nodeBaseRect.yMax;

							if (height != node.position.height)
							{
								node.position.height = height;

								EditorUtility.SetDirty(node.nodeGraph);

								graphEditor.hostWindow.DirtyGraphExtents();

								Repaint();
							}
						}

						nodePosition = node.position;

						if (Event.current.type == EventType.Repaint)
						{
							GUIStyle frameStyle = GetFrameStyle();
							if (frameStyle != null)
							{
								frameStyle.Draw(framePosition, GUIContent.none, false, false, false, false);
							}
						}

						if (HasOutsideGUI())
						{
							using (new OutsideScore(this))
							{
								OnOutsideGUI();
							}
						}

						DragNodes();

						ResizeGUI();

						switch (current.type)
						{
							case EventType.MouseDown:
								if (windowRect.Contains(current.mousePosition))
								{
#if ARBOR_DEBUG
									Debug.Log("NodeEditor : MouseDown to Used.");
#endif
									current.Use();
								}
								break;
							case EventType.MouseMove:
								if (windowRect.Contains(current.mousePosition))
								{
									current.Use();
								}
								break;
							case EventType.Repaint:
								//EditorGUIUtility.AddCursorRect(windowRect, MouseCursor.Arrow);
								//The order is not considered when the GUI.Window overlaps, so the cursor is not changed.
								break;
						}
					}

					if (graphEditor != null && graphEditor.hostWindow != null)
					{
						graphEditor.hostWindow.EndNode();
					}

					GUI.color = guiColor;
				}
			}
		}

		protected virtual void OnInitialize()
		{
		}

		protected virtual void OnGUI()
		{
		}

		protected virtual bool HasOutsideGUI()
		{
			return true;
		}

		protected virtual RectOffset GetOutsideOffset()
		{
			return ResizeDefaults.offsetZero;
		}

		protected virtual void OnOutsideGUI()
		{
		}

		private static readonly int s_HeaderHash = "s_HeaderHash".GetHashCode();

		protected virtual bool HasHelpButton()
		{
			return false;
		}

		protected virtual void OnHelpButton(Rect position, GUIStyle style)
		{
		}

		protected virtual bool HasPresetButton()
		{
			return false;
		}

		protected virtual void OnPresetButton(Rect position, GUIStyle style)
		{
		}

		protected virtual void SetContextMenu(GenericMenu menu, Rect headerPosition, bool editable)
		{
		}

		protected virtual void SetDebugContextMenu(GenericMenu menu, Rect headerPosition, bool editable)
		{
		}

		void ChangeShowComment()
		{
			bool showComment = !NodeEditorUtility.GetShowComment(node);

			NodeEditorUtility.SetShowComment(node, showComment);

			if (graphEditor.editable)
			{
				NodeGraph nodeGraph = node.nodeGraph;

				Undo.RecordObject(nodeGraph, "Change Show Comment");

				node.showComment = showComment;

				EditorUtility.SetDirty(nodeGraph);
			}

			graphEditor.UpdateNodeCommentControl(node);
		}

		void DoContextMenu(Rect popupPosition, Rect headerPosition)
		{
			GUI.UnfocusWindow();

			GenericMenu menu = new GenericMenu();

			bool editable = graphEditor.editable;

			if (isRenamable)
			{
				if (editable)
				{
					menu.AddItem(EditorContents.rename, false, BeginRename);
				}
				else
				{
					menu.AddDisabledItem(EditorContents.rename);
				}
			}

			if (isShowableComment)
			{
				switch (ArborSettings.nodeCommentViewMode)
				{
					case NodeCommentViewMode.Normal:
						menu.AddItem(EditorContents.showComment, NodeEditorUtility.GetShowComment(node), ChangeShowComment);
						break;
					case NodeCommentViewMode.ShowAll:
						menu.AddDisabledItem(EditorContents.showCommentViewModeShowAll);
						break;
					case NodeCommentViewMode.ShowCommentedOnly:
						menu.AddDisabledItem(EditorContents.showCommentViewModeShowCommentedOnly);
						break;
					case NodeCommentViewMode.HideAll:
						menu.AddDisabledItem(EditorContents.showCommentViewModeHideAll);
						break;
				}
			}

			SetContextMenu(menu, headerPosition, editable);

			if (menu.GetItemCount() > 0)
			{
				menu.AddSeparator("");
			}

			SetNodeContextMenu(menu, editable);

			SetDebugContextMenu(menu, headerPosition, editable);

			menu.DropDown(popupPosition);
		}

		protected virtual void BeginRename()
		{
		}

		public void Settings(Rect popupPosition, Rect headerPosition, GUIStyle style)
		{
			if (isShowContextMenuInHeader)
			{
				if (EditorGUI.DropdownButton(popupPosition, EditorContents.popupIcon, FocusType.Passive, style))
				{
					DoContextMenu(popupPosition, headerPosition);
				}
			}
		}

		private Rect _HeaderRect;
		private bool _HasHeaderHelpButton;
		private bool _HasHeaderPresetButton;

		internal void SetHeaderRect(Rect headerRect)
		{
			_HeaderRect = headerRect;
		}

		protected virtual void DrawOverlayIcon(Rect iconRect)
		{
		}

		protected void DrawHeaderIcon(Rect iconRect, Texture icon)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}

			Vector2 iconSize = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));

			GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

			EditorGUIUtility.SetIconSize(iconSize);

			DrawOverlayIcon(iconRect);
		}

		protected virtual void HeaderIconGUI(Rect iconRect, Texture icon)
		{
			DrawHeaderIcon(iconRect, icon);
		}

		internal void HeaderGUI(Rect headerRect, int controlId)
		{
			Rect position = node.position;
			position.x = position.y = 0;

			bool editable = graphEditor.editable;

			GUIContent titleContent = GetTitleContent();
			GUIStyle headerStyle = GetHeaderStyle();

			if (Event.current.type != EventType.Layout && Event.current.type != EventType.Used)
			{
				SetHeaderRect(headerRect);
			}

			_HasHeaderHelpButton = HasHelpButton();
			_HasHeaderPresetButton = HasPresetButton();

			Event current = Event.current;

			EventType typeForControl = current.GetTypeForControl(controlId);

			Rect namePosition;
			Rect popupPosition;
			Rect helpPosition;
			Rect presetPosition;
			GetHeaderRects(position.position, out namePosition, out popupPosition, out helpPosition, out presetPosition);

			Rect headerTitleRect = headerStyle.padding.Remove(headerRect);
			Rect headerIconRect = new Rect(headerTitleRect);
			headerIconRect.width = 16f;
			headerIconRect.height = 16f;

			Texture icon = GetIcon();
			if (icon != null)
			{
				headerTitleRect.xMin += 16f;
			}

			switch (typeForControl)
			{
				case EventType.ContextClick:
					if (headerRect.Contains(current.mousePosition))
					{
						if (isShowContextMenuInHeader)
						{
							DoContextMenu(new Rect(current.mousePosition.x, current.mousePosition.y, 0, 0), headerRect);
						}
						current.Use();
					}
					break;
				case EventType.MouseDown:
					if (current.button == 0 && current.clickCount == 2 && namePosition.Contains(current.mousePosition))
					{
						if (isRenamable && graphEditor != null && !graphEditor.IsRenaming())
						{
							GUIUtility.hotControl = controlId;
							current.Use();
						}
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlId)
					{
						if (current.button == 0)
						{
							GUIUtility.hotControl = 0;

							if (isRenamable && editable)
							{
								BeginRename();
							}
						}

						current.Use();
					}
					break;
				case EventType.Repaint:
					using (new EditorGUI.DisabledScope(!graphEditor.editable))
					{
						headerStyle.Draw(headerRect, GUIContent.none, false, false, false, false);
						if (graphEditor == null || !graphEditor.IsRenaming(node.nodeID))
						{
							Styles.nodeHeaderTitle.Draw(headerTitleRect, titleContent, false, false, false, false);
						}
					}
					break;
			}

			if (icon != null)
			{
				HeaderIconGUI(headerIconRect, icon);
			}

			Color backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = Color.white;

			if (_HasHeaderHelpButton)
			{
				OnHelpButton(helpPosition, Styles.nodeHeaderButtonLeft);
			}

			if (_HasHeaderPresetButton)
			{
				OnPresetButton(presetPosition, _HasHeaderHelpButton ? Styles.nodeHeaderButtonMid : Styles.nodeHeaderButtonLeft);
			}

			GUIStyle popupStyle = GetHeaderPopupStyle();
			Settings(popupPosition, headerRect, popupStyle);

			GUI.backgroundColor = backgroundColor;
		}

		internal void HeaderGUI()
		{
			GUIContent titleContent = GetTitleContent();
			GUIStyle headerStyle = GetHeaderStyle();

			Rect headerRect = GUILayoutUtility.GetRect(titleContent, headerStyle);

			int controlId = GUIUtility.GetControlID(s_HeaderHash, FocusType.Passive, headerRect);

			HeaderGUI(headerRect, controlId);
		}

		public virtual bool IsWindow()
		{
			return true;
		}

		protected void Repaint()
		{
			if (_GraphEditor != null)
			{
				_GraphEditor.Repaint();
			}
		}

		GUIStyle GetHeaderPopupStyle()
		{
			return (_HasHeaderHelpButton || _HasHeaderPresetButton) ? Styles.nodeHeaderButtonRight : Styles.nodeHeaderButton;
		}

		void GetHeaderRects(Vector2 nodePosition, out Rect namePosition, out Rect popupPosition, out Rect helpPosition, out Rect presetPostion)
		{
			Rect headerRect = new Rect(_HeaderRect);
			headerRect.position += nodePosition;

			GUIStyle style = GetHeaderStyle();
			headerRect = style.padding.Remove(headerRect);

			GUIStyle popupStyle = GetHeaderPopupStyle();

			Vector2 popupIconSize = popupStyle.CalcSize(EditorContents.popupIcon);

			popupPosition = new Rect(headerRect.xMax - popupIconSize.x, headerRect.y, popupIconSize.x, popupIconSize.y);
			popupPosition.y = Mathf.Floor(popupPosition.y + (headerRect.height - popupPosition.height) / 2f);

			Rect headerItemRect = popupPosition;

			if (_HasHeaderPresetButton)
			{
				GUIStyle presetStyle = _HasHeaderHelpButton ? Styles.nodeHeaderButtonMid : Styles.nodeHeaderButtonLeft;

				Vector2 iconSize = presetStyle.CalcSize(EditorContents.popupIcon);

				presetPostion = new Rect(headerItemRect.xMin - iconSize.x, headerRect.y, iconSize.x, iconSize.y);
				presetPostion.y = Mathf.Floor(presetPostion.y + (headerRect.height - presetPostion.height) / 2f);

				headerItemRect = presetPostion;
			}
			else
			{
				presetPostion = new Rect();
			}

			if (_HasHeaderHelpButton)
			{
				GUIStyle helpStyle = Styles.nodeHeaderButtonLeft;

				Vector2 iconSize = helpStyle.CalcSize(EditorContents.helpIcon);

				helpPosition = new Rect(headerItemRect.xMin - iconSize.x, headerRect.y, iconSize.x, iconSize.y);
				helpPosition.y = Mathf.Floor(helpPosition.y + (headerRect.height - helpPosition.height) / 2f);

				headerItemRect = helpPosition;
			}
			else
			{
				helpPosition = new Rect();
			}

			namePosition = new Rect(headerRect);

			namePosition.yMin -= (EditorGUIUtility.singleLineHeight - namePosition.height) / 2;
			namePosition.height = EditorGUIUtility.singleLineHeight;
			namePosition.xMax = headerItemRect.x - 2f;

			Texture icon = GetIcon();
			if (icon != null)
			{
				float size = EditorGUIUtility.GetIconSize().x;
				if (size == 0)
				{
					size = icon.width;
				}

				namePosition.xMin += size;
			}
		}

		public Rect GetNameRect(Vector2 nodePosition)
		{
			Rect namePosition;
			Rect popupPosition;
			Rect helpPosition;
			Rect presetPosition;
			GetHeaderRects(nodePosition, out namePosition, out popupPosition, out helpPosition, out presetPosition);
			return namePosition;
		}

		public Rect GetHeaderRect()
		{
			return _HeaderRect;
		}

		void CopyNode()
		{
			Clipboard.CopyNodes(new Node[] { node });
		}

		void CutNode()
		{
			Clipboard.CopyNodes(new Node[] { node });
			DeleteNode();
		}

		void DeleteNode()
		{
			_GraphEditor.DeleteNodes(new Node[] { node });
		}

		void DuplicateNode(object obj)
		{
			Vector2 position = (Vector2)obj;

			_GraphEditor.DuplicateNodes(position, new Node[] { node });
		}

		protected virtual void SetDeleteContextMenu(GenericMenu menu, bool deletable, bool editable)
		{
		}

		public virtual bool IsCopyable()
		{
			return true;
		}

		protected void SetNodeContextMenu(GenericMenu menu, bool editable)
		{
			bool isCopyable = IsCopyable();
			bool isDeletable = node.IsDeletable();

			if (isCopyable && isDeletable && editable)
			{
				menu.AddItem(EditorContents.cut, false, CutNode);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.cut);
			}
			if (isCopyable)
			{
				menu.AddItem(EditorContents.copy, false, CopyNode);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.copy);
			}

			menu.AddSeparator("");

			if (isCopyable && editable)
			{
				Vector2 duplicatePosition = new Vector2(node.position.xMax, node.position.yMin);
				if (ArborSettings.showGrid && ArborSettings.snapGrid)
				{
					float gridSizeMinor = ArborSettings.gridSize / (float)ArborSettings.gridSplitNum;

					int num1 = Mathf.CeilToInt(duplicatePosition.x / gridSizeMinor) + 1;
					int num2 = Mathf.FloorToInt(duplicatePosition.y / gridSizeMinor);
					duplicatePosition.x = num1 * gridSizeMinor;
					duplicatePosition.y = num2 * gridSizeMinor;
				}
				else
				{
					duplicatePosition.x += 10f;
				}
				menu.AddItem(EditorContents.duplicate, false, DuplicateNode, duplicatePosition);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.duplicate);
			}

			if (isDeletable && editable)
			{
				menu.AddItem(EditorContents.delete, false, DeleteNode);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.delete);
			}
			SetDeleteContextMenu(menu, isDeletable, editable);
		}

		public virtual bool IsDraggingVisible()
		{
			return false;
		}

		public virtual void OnRename(string name)
		{
		}

		public virtual string GetTitle()
		{
			return node.GetName();
		}

		public virtual bool IsShowNodeList()
		{
			return false;
		}

		private GUIStyle _ListStyle = null;
		private bool _IsListSelection;

		public GUIStyle GetListStyle()
		{
			if (_ListStyle == null || _IsListSelection != isSelection)
			{
				string key = string.Format("node element{0}", (!isSelection) ? string.Empty : " on");
				_ListStyle = Styles.GetStyle(key);
				_IsListSelection = isSelection;
			}
			return _ListStyle;
		}

		public Color GetListColor()
		{
			return Styles.GetColor(GetStyleColor());
		}

		public virtual Texture2D GetIcon()
		{
			return null;
		}

		public virtual void OnListElement(Rect rect)
		{
		}

		public virtual void ExpandAll(bool expanded)
		{
		}
	}
}