//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ArborEditor.BehaviourTree
{
	using Arbor;
	using Arbor.BehaviourTree;
	using Arbor.Playables;

	[CustomNodeGraphEditor(typeof(BehaviourTreeInternal))]
	public sealed class BehaviourTreeGraphEditor : NodeGraphEditor
	{
		private static class Types
		{
			public static readonly System.Type SetParameterActionType;

			static Types()
			{
				SetParameterActionType = AssemblyHelper.GetTypeByName("Arbor.ParameterBehaviours.SetParameterAction");
			}
		}

		BehaviourTreeInternal behaviourTree
		{
			get
			{
				return nodeGraph as BehaviourTreeInternal;
			}
		}

		internal bool _DragBranchEnable = false;
		private bool _DragBranchScroll = false;
		private Bezier2D _DragBranchBezier = null;
		private int _DragBranchNodeID = 0;
		private int _DragBranchHoverID = 0;
		private bool _IsDragParentSlot = false;

		protected override bool HasDebugMenu()
		{
			return true;
		}

		void SetBreakPoints()
		{
			Undo.RecordObject(nodeGraph, "BreakPoint On");

			for (int i = 0; i < selectionCount; i++)
			{
				Node node = GetSelectionNode(i);
				TreeBehaviourNode treeBehaviourNode = node as TreeBehaviourNode;
				if (treeBehaviourNode != null)
				{
					treeBehaviourNode.breakPoint = true;
				}
			}

			EditorUtility.SetDirty(nodeGraph);
		}

		void ReleaseBreakPoints()
		{
			Undo.RecordObject(nodeGraph, "BreakPoint Off");

			for (int i = 0; i < selectionCount; i++)
			{
				Node node = GetSelectionNode(i);
				TreeBehaviourNode treeBehaviourNode = node as TreeBehaviourNode;
				if (treeBehaviourNode != null)
				{
					treeBehaviourNode.breakPoint = false;
				}
			}

			EditorUtility.SetDirty(nodeGraph);
		}

		void ReleaseAllBreakPoints()
		{
			Undo.RecordObject(nodeGraph, "Delete All BreakPoint");

			for (int nodeIndex = 0, nodeCount = behaviourTree.nodeCount; nodeIndex < nodeCount; nodeIndex++)
			{
				TreeBehaviourNode treeBehaviourNode = behaviourTree.GetNodeFromIndex(nodeIndex) as TreeBehaviourNode;

				if (treeBehaviourNode != null)
				{
					treeBehaviourNode.breakPoint = false;
				}
			}

			EditorUtility.SetDirty(nodeGraph);
		}

		protected override void OnSetDebugMenu(GenericMenu menu)
		{
			bool isSelectionBehaviourNode = false;
			for (int i = 0; i < selectionCount; i++)
			{
				Node node = GetSelectionNode(i);
				if (node is TreeBehaviourNode)
				{
					isSelectionBehaviourNode = true;
					break;
				}
			}

			bool editable = this.editable;

			if (isSelectionBehaviourNode && editable)
			{
				menu.AddItem(EditorContents.setBreakPoints, false, SetBreakPoints);
				menu.AddItem(EditorContents.releaseBreakPoints, false, ReleaseBreakPoints);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.setBreakPoints);
				menu.AddDisabledItem(EditorContents.releaseBreakPoints);
			}

			if (editable)
			{
				menu.AddItem(EditorContents.releaseAllBreakPoints, false, ReleaseAllBreakPoints);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.releaseAllBreakPoints);
			}
		}

		public override GUIContent GetGraphLabel()
		{
			return EditorContents.behaviourTree;
		}

		public override bool HasPlayState()
		{
			return true;
		}

		public override PlayState GetPlayState()
		{
			return behaviourTree.playState;
		}

		void OnSelectComposite(Vector2 position, System.Type classType)
		{
			CreateComposite(position, classType);
			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
		}

		void CreateComposite(object obj)
		{
			MousePosition mousePosition = (MousePosition)obj;

			Rect buttonRect = new Rect(mousePosition.screenPoint, Vector2.zero);

			CompositeBehaviourMenuWindow.instance.Init(this, mousePosition.guiPoint, buttonRect, OnSelectComposite, null, null);
		}

		public CompositeNode CreateComposite(Vector2 position, System.Type classType)
		{
			CompositeNode compositeNode = behaviourTree.CreateComposite(EditorGUITools.SnapToGrid(position), classType);

			if (compositeNode != null)
			{
				Undo.RecordObject(behaviourTree, "Created CompositeNode");

				BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(classType);
				compositeNode.name = behaviourInfo.titleContent.text;

				EditorUtility.SetDirty(behaviourTree);

				CreateNodeEditor(compositeNode);
				UpdateNodeCommentControl(compositeNode);

				SetSelectNode(compositeNode);

				BeginRename(compositeNode.nodeID, compositeNode.name, true);
			}

			return compositeNode;
		}

		void OnSelectAction(Vector2 position, System.Type classType)
		{
			CreateAction(position, classType);
			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
		}

		void CreateAction(object obj)
		{
			MousePosition mousePosition = (MousePosition)obj;

			Rect buttonRect = new Rect(mousePosition.screenPoint, Vector2.zero);

			ActionBehaviourMenuWindow.instance.Init(this, mousePosition.guiPoint, buttonRect, OnSelectAction, null, null);
		}

		public ActionNode CreateAction(Vector2 position, System.Type classType)
		{
			ActionNode actionNode = behaviourTree.CreateAction(EditorGUITools.SnapToGrid(position), classType);

			if (actionNode != null)
			{
				Undo.RecordObject(behaviourTree, "Created ActionNode");

				BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(classType);
				actionNode.name = behaviourInfo.titleContent.text;

				EditorUtility.SetDirty(behaviourTree);

				CreateNodeEditor(actionNode);
				UpdateNodeCommentControl(actionNode);

				SetSelectNode(actionNode);

				BeginRename(actionNode.nodeID, actionNode.name, true);
			}

			return actionNode;
		}

		protected override void SetCreateNodeContextMenu(GenericMenu menu, bool editable)
		{
			Event current = Event.current;

			if (editable)
			{
				menu.AddItem(EditorContents.createComposite, false, CreateComposite, new MousePosition(current.mousePosition));
				menu.AddItem(EditorContents.createAction, false, CreateAction, new MousePosition(current.mousePosition));
			}
			else
			{
				menu.AddDisabledItem(EditorContents.createComposite);
				menu.AddDisabledItem(EditorContents.createAction);
			}
		}

		public HashSet<int> _DragHighlightControlIDs = new HashSet<int>();

		public void BeginDragBranch(int nodeID, bool isDragParentSlot)
		{
			_DragBranchEnable = true;
			_DragBranchScroll = true;
			_DragBranchNodeID = nodeID;
			_DragBranchHoverID = 0;
			_IsDragParentSlot = isDragParentSlot;
		}

		public bool IsDragBranchConnectable(TreeNodeBase node, bool isParentSlot)
		{
			if (!_DragBranchEnable || _IsDragParentSlot == isParentSlot || _DragBranchNodeID == node.nodeID)
			{
				return false;
			}

			TreeNodeBase draggingNode = behaviourTree.GetNodeFromID(_DragBranchNodeID) as TreeNodeBase;

			TreeNodeBase parentNode = isParentSlot ? draggingNode : node;
			TreeNodeBase childNode = isParentSlot ? node : draggingNode;

			return !behaviourTree.CheckLoop(parentNode, childNode);
		}

		public void ShowLinkSlotHightlight(Rect position, int controlID, GUIStyle style)
		{
			_DragHighlightControlIDs.Add(controlID);
			ShowHightlightControl(position, controlID, style);
		}

		public void EndDragBranch()
		{
			_DragBranchEnable = false;
			_DragBranchScroll = false;

			foreach (var controlID in _DragHighlightControlIDs)
			{
				CloseHighlightControl(controlID);
			}
			_DragHighlightControlIDs.Clear();

			hostWindow.Repaint();
		}

		public void DragBranchBezie(Bezier2D bezier)
		{
			_DragBranchBezier = bezier;
		}

		public int GetDragBranchHoverID()
		{
			return _DragBranchHoverID;
		}

		public void DragBranchHoverID(int nodeID)
		{
			if (_DragBranchHoverID != nodeID)
			{
				_DragBranchHoverID = nodeID;
			}
		}

		public bool IsDragBranchHover(Node node)
		{
			return _DragBranchEnable && _DragBranchHoverID == node.nodeID;
		}

		public bool IsDragParentSlot()
		{
			return _IsDragParentSlot;
		}

		public TreeNodeBase GetHoverChildNode(TreeNodeBase node, Vector2 position)
		{
			int nodeCount = nodeGraph.nodeCount;
			for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
			{
				TreeNodeBase n = nodeGraph.GetNodeFromIndex(nodeIndex) as TreeNodeBase;

				if (n != null && node != n && n.HasParentLinkSlot())
				{
					TreeNodeBaseEditor nodeEditor = GetNodeEditor(n) as TreeNodeBaseEditor;
					if (nodeEditor != null && nodeEditor.parentLinkSlotPosition.Contains(position) && !behaviourTree.CheckLoop(node, n))
					{
						return n;
					}
				}
			}

			return null;
		}

		public TreeNodeBase GetHoverParentNode(TreeNodeBase node, Vector2 position)
		{
			int nodeCount = nodeGraph.nodeCount;
			for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
			{
				TreeNodeBase n = nodeGraph.GetNodeFromIndex(nodeIndex) as TreeNodeBase;

				if (n != null && node != n && n.HasChildLinkSlot())
				{
					TreeNodeBaseEditor nodeEditor = GetNodeEditor(n) as TreeNodeBaseEditor;
					if (nodeEditor != null && nodeEditor.childLinkSlotPosition.Contains(position) && !behaviourTree.CheckLoop(n, node))
					{
						return n;
					}
				}
			}

			return null;
		}

		void DrawDragBranch()
		{
			if (_DragBranchEnable)
			{
				EditorGUITools.DrawBranch(_DragBranchBezier, dragBezierColor, bezierShadowColor, 5.0f, false, false);
			}
		}

		public override void OnDrawDragBranchies()
		{
			DrawDragBranch();
		}

		private static readonly int s_DrawBranchHash = "s_DrawBranchHash".GetHashCode();

		private static readonly Color s_BranchBezierColor = Color.white;
		private static readonly Color s_BranchBezierActiveColor = new Color(1.0f, 0.5f, 0.0f);
		private static readonly Color s_BranchBezierInactiveColor = new Color(0.5f, 0.5f, 0.5f);
		private static readonly Color s_BranchBezierShadowColor = new Color(0, 0, 0, 1.0f);

		private int _HoverBranchID = 0;
		private int _NextHoverBranchID = 0;

		protected override void OnBeginDrawBranch()
		{
			_NextHoverBranchID = 0;
		}

		protected override bool OnHasHoverBranch()
		{
			return _NextHoverBranchID != 0;
		}

		protected override void OnClearHoverBranch()
		{
			_NextHoverBranchID = 0;
		}

		protected override bool OnEndDrawBranch()
		{
			if (_HoverBranchID != _NextHoverBranchID)
			{
				_HoverBranchID = _NextHoverBranchID;
				return true;
			}
			return false;
		}

		void DrawNodeLinkBranch(NodeBranch branch)
		{
			if (branch == null)
			{
				return;
			}

			Bezier2D bezier = branch.bezier;

			int controlID = EditorGUIUtility.GetControlID(s_DrawBranchHash, FocusType.Passive);

			Event currentEvent = Event.current;

			EventType eventType = currentEvent.GetTypeForControl(controlID);

			bool isHover = _HoverBranchID == branch.branchID;

			switch (eventType)
			{
				case EventType.MouseDown:
					if (currentEvent.button == 1 || Application.platform == RuntimePlatform.OSXEditor && currentEvent.control)
					{
						if (isHover)
						{
							Node parentNode = nodeGraph.GetNodeFromID(branch.parentNodeID);
							Node childNode = nodeGraph.GetNodeFromID(branch.childNodeID);

							GenericMenu menu = new GenericMenu();
							menu.AddItem(EditorGUITools.GetTextContent(Localization.GetWord("Go to Parent Node") + " : " + GetNodeTitle(parentNode)), false, () =>
							   {
								   BeginFrameSelected(parentNode);
							   });
							menu.AddItem(EditorGUITools.GetTextContent(Localization.GetWord("Go to Child Node") + " : " + GetNodeTitle(childNode)), false, () =>
								{
									BeginFrameSelected(childNode);
								});
							if (!branch.isActive && editable)
							{
								menu.AddItem(EditorContents.disconnect, false, () =>
								{
									Undo.IncrementCurrentGroup();
									int undoGroup = Undo.GetCurrentGroup();

									behaviourTree.DisconnectBranch(branch);
									behaviourTree.CalculatePriority();

									Undo.CollapseUndoOperations(undoGroup);
								});
							}
							else
							{
								menu.AddDisabledItem(EditorContents.disconnect);
							}

							menu.ShowAsContext();
							currentEvent.Use();
						}
					}
					break;
				case EventType.MouseMove:
					{
						float distance = 0f;
						if (IsHoverBezier(currentEvent.mousePosition, bezier, ref distance))
						{
							ClearHoverBranch();
							_NextHoverBranchID = branch.branchID;
							_NextHoverBranchDistance = distance;
						}
					}
					break;
				case EventType.Repaint:
					{
						Color color = s_BranchBezierColor;
						if (Application.isPlaying)
						{
							if (branch.isActive)
							{
								color = s_BranchBezierActiveColor;
							}
							else
							{
								color = s_BranchBezierInactiveColor;
							}
						}
						EditorGUITools.DrawBranch(bezier, color, s_BranchBezierShadowColor, 5.0f, false, isHover);
					}
					break;
			}
		}

		void DrawNodeLinkBranchies()
		{
			int branchCount = behaviourTree.nodeBranchies.count;
			for (int branchIndex = 0; branchIndex < branchCount; branchIndex++)
			{
				NodeBranch branch = behaviourTree.nodeBranchies[branchIndex];

				if (branch == null)
				{
					continue;
				}

				if (_HoverBranchID != branch.branchID)
				{
					DrawNodeLinkBranch(branch);
				}
			}
		}

		public override void OnDrawBranchies()
		{
			DrawNodeLinkBranchies();
		}

		public override void OnDrawHoverBranch()
		{
			NodeBranch branch = behaviourTree.nodeBranchies.GetFromID(_HoverBranchID);
			if (branch != null)
			{
				DrawNodeLinkBranch(branch);
			}
		}

		public override bool IsDraggingBranch(Node node)
		{
			return base.IsDraggingBranch(node) ||
				_DragBranchEnable && _DragBranchNodeID == node.nodeID;
		}

		public override bool IsDragBranch()
		{
			return base.IsDragBranch() || _DragBranchEnable;
		}

		public override bool IsDragBranchScroll()
		{
			return IsDragBranch() && (!_DragBranchEnable || _DragBranchScroll);
		}

		protected override void OnDragNodes()
		{
			behaviourTree.CalculatePriority();

			RaiseOnChangedNodes();
		}

		internal static int InternalNodeListSortComparison(NodeEditor a, NodeEditor b)
		{
			TreeNodeBaseEditor treeNodeA = a as TreeNodeBaseEditor;
			TreeNodeBaseEditor treeNodeB = b as TreeNodeBaseEditor;
			if (treeNodeA == null || treeNodeB == null)
			{
				return NodeListGUI.Defaults.SortComparison(a, b);
			}

			bool enablePriorityA = treeNodeA.treeNode.enablePriority;
			bool enablePriorityB = treeNodeB.treeNode.enablePriority;
			if (enablePriorityA != enablePriorityB)
			{
				return enablePriorityB.CompareTo(enablePriorityA);
			}
			if (!enablePriorityA && !enablePriorityB)
			{
				bool isActionA = treeNodeA.treeNode is ActionNode;
				bool isActionB = treeNodeB.treeNode is ActionNode;
				if (isActionA != isActionB)
				{
					return isActionA.CompareTo(isActionB);
				}
				return NodeListGUI.Defaults.SortComparison(a, b);
			}

			return treeNodeA.treeNode.priority.CompareTo(treeNodeB.treeNode.priority);
		}

		protected override int NodeListSortComparison(NodeEditor a, NodeEditor b)
		{
			return InternalNodeListSortComparison(a, b);
		}

		protected override Node GetActiveNode()
		{
			return behaviourTree.currentNode;
		}

		protected override void OnCreateSetParameter(Vector2 position, Parameter parameter)
		{
			Undo.IncrementCurrentGroup();
			int undoGroup = Undo.GetCurrentGroup();

			ActionNode actionNode = CreateAction(position, Types.SetParameterActionType);

			Arbor.ParameterBehaviours.SetParameterActionInternal setParameterAction = actionNode.GetBehaviourObject() as Arbor.ParameterBehaviours.SetParameterActionInternal;

			Undo.RecordObject(setParameterAction, "Created ActionNode");

			setParameterAction.SetParameter(parameter);

			Undo.CollapseUndoOperations(undoGroup);

			EditorUtility.SetDirty(setParameterAction);
			EditorUtility.SetDirty(nodeGraph);
		}

		void DrawMinimapNodeLinkBranch(MinimapViewport minimapViewport, NodeBranch branch)
		{
			Color color = s_BranchBezierColor;
			if (Application.isPlaying)
			{
				if (branch.isActive)
				{
					color = s_BranchBezierActiveColor;
				}
				else
				{
					color = s_BranchBezierInactiveColor;
				}
			}

			EditorGUITools.DrawBranch(minimapViewport.GraphToMinimapBezier(branch.bezier), color, false, Color.white, 4f, false, 0f, false);
		}

		protected override void OnDrawMinimapBranchies(MinimapViewport minimapViewport)
		{
			int branchCount = behaviourTree.nodeBranchies.count;
			for (int branchIndex = 0; branchIndex < branchCount; branchIndex++)
			{
				NodeBranch branch = behaviourTree.nodeBranchies[branchIndex];

				if (branch == null)
				{
					continue;
				}

				DrawMinimapNodeLinkBranch(minimapViewport, branch);
			}
		}
	}
}