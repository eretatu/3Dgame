//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace ArborEditor
{
	using Arbor;
	using Arbor.Playables;

	[CustomNodeGraphEditor(typeof(ArborFSMInternal))]
	public sealed class StateMachineGraphEditor : NodeGraphEditor
	{
		private static class Types
		{
			public static readonly System.Type SetParameterBehaviourType;

			static Types()
			{
				SetParameterBehaviourType = AssemblyHelper.GetTypeByName("Arbor.ParameterBehaviours.SetParameterBehaviour");
			}
		}

		private bool _DragStateBranchEnable = false;
		private Bezier2D _DragStateBranchBezier;
		private int _DragStateBranchNodeID = 0;
		private int _DragStateBranchHoverStateID = 0;

		public ArborFSMInternal stateMachine
		{
			get
			{
				return nodeGraph as ArborFSMInternal;
			}
			set
			{
				nodeGraph = value;
			}
		}

		public void BeginDragStateBranch(int nodeID)
		{
			_DragStateBranchEnable = true;
			_DragStateBranchNodeID = nodeID;
			_DragStateBranchHoverStateID = 0;
		}

		public void EndDragStateBranch()
		{
			_DragStateBranchEnable = false;
			_DragStateBranchNodeID = 0;
			_DragStateBranchHoverStateID = 0;

			hostWindow.Repaint();
		}

		public void DragStateBranchBezie(Bezier2D bezier)
		{
			_DragStateBranchBezier = bezier;
		}

		public void DragStateBranchHoverStateID(int stateID)
		{
			if (_DragStateBranchHoverStateID != stateID)
			{
				_DragStateBranchHoverStateID = stateID;
			}
		}

		void DrawDragStateBehaviour()
		{
			if (_DragStateBranchEnable)
			{
				EditorGUITools.DrawBranch(_DragStateBranchBezier, dragBezierColor, bezierShadowColor, 5.0f, true, false);
			}
		}

		public override void OnDrawDragBranchies()
		{
			DrawDragStateBehaviour();
		}

		public bool IsDragBranchHover(Node node)
		{
			return _DragStateBranchEnable && _DragStateBranchHoverStateID == node.nodeID;
		}

		public override bool IsDraggingBranch(Node node)
		{
			return base.IsDraggingBranch(node) ||
				_DragStateBranchEnable && _DragStateBranchNodeID == node.nodeID;
		}

		public override bool IsDragBranch()
		{
			return base.IsDragBranch() || _DragStateBranchEnable;
		}

		private StateLinkGUI _HoverStateLink = null;
		private StateLinkGUI _NextHoverStateLink = null;

		protected override void OnBeginDrawBranch()
		{
			_NextHoverStateLink = null;
		}

		protected override bool OnHasHoverBranch()
		{
			return _NextHoverStateLink != null;
		}

		protected override void OnClearHoverBranch()
		{
			_NextHoverStateLink = null;
		}

		protected override bool OnEndDrawBranch()
		{
			if (_HoverStateLink != _NextHoverStateLink)
			{
				_HoverStateLink = _NextHoverStateLink;
				return true;
			}
			return false;
		}

		bool IsLinkedRerouteNode(State state, StateLinkRerouteNode rerouteNode)
		{
			StateEditor stateEditor = GetNodeEditor(state) as StateEditor;

			if (stateEditor == null)
			{
				return false;
			}

			int behaviourCount = state.behaviourCount;

			for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
			{
				StateBehaviourEditorGUI behaviourEditor = stateEditor.GetBehaviourEditor(behaviourIndex);

				if (behaviourEditor != null)
				{
					var stateLinkProperties = behaviourEditor.stateLinkProperties;
					for (int stateLinkIndex = 0; stateLinkIndex < stateLinkProperties.Count; stateLinkIndex++)
					{
						StateEditor.StateLinkProperty stateLinkProperty = stateLinkProperties[stateLinkIndex];
						StateLinkRerouteNode currentRerouteNode = stateMachine.GetNodeFromID(stateLinkProperty.stateLinkGUI.stateLink.stateID) as StateLinkRerouteNode;
						while (currentRerouteNode != null)
						{
							if (currentRerouteNode == rerouteNode)
							{
								return true;
							}

							currentRerouteNode = stateMachine.GetNodeFromID(currentRerouteNode.link.stateID) as StateLinkRerouteNode;
						}
					}
				}
			}

			return false;
		}

		internal List<State> GetParentStates(StateLinkRerouteNode rerouteNode)
		{
			List<State> states = new List<State>();

			int stateCount = stateMachine.stateCount;
			for (int i = 0; i < stateCount; i++)
			{
				State state = stateMachine.GetStateFromIndex(i);
				if (IsLinkedRerouteNode(state, rerouteNode))
				{
					states.Add(state);
				}
			}
			return states;
		}

		internal bool IsHoverStateLink(StateLinkGUI stateLinkGUI)
		{
			return _HoverStateLink != null && _HoverStateLink.stateLink == stateLinkGUI.stateLink;
		}

		internal void SetHoverStateLink(StateLinkGUI stateLinkGUI, float distance)
		{
			ClearHoverBranch();

			_NextHoverStateLink = stateLinkGUI;
			_NextHoverBranchDistance = distance;
		}

		public static readonly Color reservedColor = new Color(0.5f, 0.0f, 1.0f);

		void DrawBehaviourBranches(StateEditor stateEditor)
		{
			using (new ProfilerScope("DrawBehaviourBranches"))
			{
				State state = stateEditor.state;
				int behaviourCount = state.behaviourCount;

				for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
				{
					StateBehaviourEditorGUI behaviourEditor = stateEditor.GetBehaviourEditor(behaviourIndex);
					if (behaviourEditor == null)
					{
						continue;
					}

					StateBehaviour behaviour = behaviourEditor.behaviourObj as StateBehaviour;
					if (behaviour == null)
					{
						continue;
					}

					var stateLinkProperties = behaviourEditor.stateLinkProperties;
					for (int stateLinkIndex = 0; stateLinkIndex < stateLinkProperties.Count; stateLinkIndex++)
					{
						StateEditor.StateLinkProperty stateLinkProperty = stateLinkProperties[stateLinkIndex];
						StateLinkGUI stateLinkGUI = stateLinkProperty.stateLinkGUI;
						if (_HoverStateLink == null || _HoverStateLink.stateLink != stateLinkGUI.stateLink)
						{
							stateLinkGUI.DrawBranchStateLink();
						}
					}
				}
			}
		}

		void DrawBehaviourBranchesTransitionCount(StateEditor stateEditor)
		{
			using (new ProfilerScope("DrawBehaviourBranches"))
			{
				State state = stateEditor.state;
				int behaviourCount = state.behaviourCount;

				for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
				{
					StateBehaviourEditorGUI behaviourEditor = stateEditor.GetBehaviourEditor(behaviourIndex);
					if (behaviourEditor == null)
					{
						continue;
					}

					var stateLinkProperties = behaviourEditor.stateLinkProperties;
					for (int stateLinkIndex = 0; stateLinkIndex < stateLinkProperties.Count; stateLinkIndex++)
					{
						StateEditor.StateLinkProperty stateLinkProperty = stateLinkProperties[stateLinkIndex];
						StateLinkGUI stateLinkGUI = stateLinkProperty.stateLinkGUI;
						if (_HoverStateLink == null || _HoverStateLink.stateLink != stateLinkGUI.stateLink)
						{
							stateLinkGUI.DrawTransitionCount();
						}
					}
				}
			}
		}

		void DrawStateLinkBranchies()
		{
			using (new ProfilerScope("States"))
			{
				StateLinkRerouteNodeList stateLinkRerouteNodes = stateMachine.stateLinkRerouteNodes;
				int rerouteCount = stateLinkRerouteNodes.count;
				for (int i = 0; i < rerouteCount; i++)
				{
					StateLinkRerouteNode rerouteNode = stateLinkRerouteNodes[i];

					if (rerouteNode == null)
					{
						continue;
					}

					StateLinkRerouteNodeEditor nodeEditor = GetNodeEditor(rerouteNode) as StateLinkRerouteNodeEditor;
					if (nodeEditor == null)
					{
						continue;
					}

					StateLinkGUI stateLinkGUI = nodeEditor.stateLinkGUI;

					if (_HoverStateLink == null || _HoverStateLink.stateLink != stateLinkGUI.stateLink)
					{
						stateLinkGUI.DrawBranchStateLink();
					}
				}

				int stateCount = stateMachine.stateCount;
				for (int i = 0; i < stateCount; i++)
				{
					State state = stateMachine.GetStateFromIndex(i);
					StateEditor stateEditor = GetNodeEditor(state) as StateEditor;

					if (stateEditor != null)
					{
						stateEditor.UpdateBehaviour();

						DrawBehaviourBranches(stateEditor);
					}
				}

				if (Application.isPlaying)
				{
					for (int i = 0; i < stateCount; i++)
					{
						State state = stateMachine.GetStateFromIndex(i);
						StateEditor stateEditor = GetNodeEditor(state) as StateEditor;

						if (stateEditor != null)
						{
							DrawBehaviourBranchesTransitionCount(stateEditor);
						}
					}
				}
			}
		}

		public override void OnDrawBranchies()
		{
			DrawStateLinkBranchies();
		}

		public override void OnDrawHoverBranch()
		{
			if (_HoverStateLink != null)
			{
				_HoverStateLink.DrawBranchStateLink();
				if (Application.isPlaying)
				{
					if (_HoverStateLink.nodeEditor.node is State)
					{
						_HoverStateLink.DrawTransitionCount();
					}
				}
			}
		}

		public State CreateState(Vector2 position, bool resident)
		{
			Undo.IncrementCurrentGroup();

			State state = stateMachine.CreateState(resident);

			if (state != null)
			{
				Undo.RecordObject(stateMachine, "Created State");

				state.position = EditorGUITools.SnapPositionToGrid(new Rect(position.x, position.y, 300, 100));

				EditorUtility.SetDirty(stateMachine);

				CreateNodeEditor(state);
				UpdateNodeCommentControl(state);

				SetSelectNode(state);

				BeginRename(state.nodeID, state.name, true);
			}

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			Repaint();

			return state;
		}

		public StateLinkRerouteNode CreateStateLinkRerouteNode(Vector2 position, Color lineColor)
		{
			StateLinkRerouteNode stateLinkRerouteNode = stateMachine.CreateStateLinkRerouteNode(EditorGUITools.SnapToGrid(position), lineColor);

			if (stateLinkRerouteNode != null)
			{
				CreateNodeEditor(stateLinkRerouteNode);

				SetSelectNode(stateLinkRerouteNode);
			}

			Repaint();

			return stateLinkRerouteNode;
		}

		void CreateState(object obj)
		{
			Vector2 position = (Vector2)obj;

			CreateState(position, false);
		}

		void CreateResidentState(object obj)
		{
			Vector2 position = (Vector2)obj;

			CreateState(position, true);
		}

		protected override void SetCreateNodeContextMenu(GenericMenu menu, bool editable)
		{
			Event current = Event.current;

			if (editable)
			{
				menu.AddItem(EditorContents.createState, false, CreateState, current.mousePosition);
				menu.AddItem(EditorContents.createResidentState, false, CreateResidentState, current.mousePosition);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.createState);
				menu.AddDisabledItem(EditorContents.createResidentState);
			}
		}

		void ClearCount()
		{
			for (int stateIndex = 0, stateCount = stateMachine.stateCount; stateIndex < stateCount; stateIndex++)
			{
				State state = stateMachine.GetStateFromIndex(stateIndex);
				StateEditor stateEditor = GetNodeEditor(state) as StateEditor;

				state.transitionCount = 0;

				for (int behaviourIndex = 0, behaviourCount = state.behaviourCount; behaviourIndex < behaviourCount; behaviourIndex++)
				{
					StateBehaviourEditorGUI behaviourEditor = stateEditor.GetBehaviourEditor(behaviourIndex);

					if (behaviourEditor == null)
					{
						continue;
					}

					var stateLinkProperties = behaviourEditor.stateLinkProperties;
					for (int stateLinkIndex = 0; stateLinkIndex < stateLinkProperties.Count; stateLinkIndex++)
					{
						StateEditor.StateLinkProperty stateLinkProperty = stateLinkProperties[stateLinkIndex];
						stateLinkProperty.stateLinkGUI.stateLink.transitionCount = 0;
					}
				}
			}
		}

		void SetBreakPoints()
		{
			Undo.RecordObject(stateMachine, "BreakPoint On");

			for (int i = 0; i < selectionCount; i++)
			{
				Node node = GetSelectionNode(i);
				State state = node as State;
				if (state != null)
				{
					state.breakPoint = true;
				}
			}

			EditorUtility.SetDirty(stateMachine);
		}

		void ReleaseBreakPoints()
		{
			Undo.RecordObject(stateMachine, "BreakPoint Off");

			for (int i = 0; i < selectionCount; i++)
			{
				Node node = GetSelectionNode(i);
				State state = node as State;
				if (state != null)
				{
					state.breakPoint = false;
				}
			}

			EditorUtility.SetDirty(stateMachine);
		}

		void ReleaseAllBreakPoints()
		{
			Undo.RecordObject(stateMachine, "Delete All BreakPoint");

			for (int stateIndex = 0, stateCount = stateMachine.stateCount; stateIndex < stateCount; stateIndex++)
			{
				State state = stateMachine.GetStateFromIndex(stateIndex);

				state.breakPoint = false;
			}

			EditorUtility.SetDirty(stateMachine);
		}

		internal static int InternalNodeListSortComparison(NodeEditor a, NodeEditor b)
		{
			StateEditor stateEditorA = a as StateEditor;
			StateEditor stateEditorB = b as StateEditor;
			if (stateEditorA == null || stateEditorB == null)
			{
				return NodeListGUI.Defaults.SortComparison(a, b);
			}

			ArborFSMInternal stateMachine = stateEditorA.graphEditor.nodeGraph as ArborFSMInternal;

			if (stateMachine.startStateID == stateEditorA.state.nodeID)
			{
				return -1;
			}
			else if (stateMachine.startStateID == stateEditorB.state.nodeID)
			{
				return 1;
			}
			if (!stateEditorA.state.resident && stateEditorB.state.resident)
			{
				return -1;
			}
			else if (stateEditorA.state.resident && !stateEditorB.state.resident)
			{
				return 1;
			}
			return stateEditorA.state.name.CompareTo(stateEditorB.state.name);
		}

		protected override int NodeListSortComparison(NodeEditor a, NodeEditor b)
		{
			return InternalNodeListSortComparison(a, b);
		}

		protected override bool HasViewMenu()
		{
			return true;
		}

		protected override void OnSetViewMenu(GenericMenu menu)
		{
			menu.AddItem(EditorContents.stateLinkShowNodeTop, ArborSettings.stateLinkShowMode == StateLinkShowMode.NodeTop, () =>
			{
				ArborSettings.stateLinkShowMode = StateLinkShowMode.NodeTop;
				Repaint();
			});
			menu.AddItem(EditorContents.stateLinkShowBehaviourTop, ArborSettings.stateLinkShowMode == StateLinkShowMode.BehaviourTop, () =>
			{
				ArborSettings.stateLinkShowMode = StateLinkShowMode.BehaviourTop;
				Repaint();
			});
			menu.AddItem(EditorContents.stateLinkShowBehaviourBottom, ArborSettings.stateLinkShowMode == StateLinkShowMode.BehaviourBottom, () =>
			{
				ArborSettings.stateLinkShowMode = StateLinkShowMode.BehaviourBottom;
				Repaint();
			});
			menu.AddItem(EditorContents.stateLinkShowNodeBottom, ArborSettings.stateLinkShowMode == StateLinkShowMode.NodeBottom, () =>
			{
				ArborSettings.stateLinkShowMode = StateLinkShowMode.NodeBottom;
				Repaint();
			});
		}

		protected override bool HasDebugMenu()
		{
			return true;
		}

		protected override void OnSetDebugMenu(GenericMenu menu)
		{
			bool isSelectionState = false;
			for (int i = 0; i < selectionCount; i++)
			{
				Node node = GetSelectionNode(i);
				if (node is State)
				{
					isSelectionState = true;
					break;
				}
			}

			bool editable = this.editable;

			if (isSelectionState && editable)
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

			if (Application.isPlaying && editable)
			{
				menu.AddItem(EditorContents.clearCount, false, ClearCount);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.clearCount);
			}
		}

		public override GUIContent GetGraphLabel()
		{
			return EditorContents.stateMachine;
		}

		public override bool HasPlayState()
		{
			return true;
		}

		public override PlayState GetPlayState()
		{
			return stateMachine.playState;
		}

		bool CheckLoop(StateLinkRerouteNode current, StateLinkRerouteNode target)
		{
			if (current == null)
			{
				return false;
			}

			while (target != null)
			{
				StateLinkRerouteNode nextNode = stateMachine.GetNodeFromID(target.link.stateID) as StateLinkRerouteNode;
				if (nextNode == null)
				{
					break;
				}

				if (nextNode == current)
				{
					return true;
				}

				target = nextNode;
			}

			return false;
		}

		public Node GetTargetNodeFromPosition(Vector2 position, Node node)
		{
			for (int i = 0, count = stateMachine.stateCount; i < count; i++)
			{
				State state = stateMachine.GetStateFromIndex(i);
				if (!state.resident && state.position.Contains(position))
				{
					return state;
				}
			}

			StateLinkRerouteNode rerouteNode = node as StateLinkRerouteNode;

			StateLinkRerouteNodeList stateLinks = stateMachine.stateLinkRerouteNodes;
			for (int i = 0, count = stateLinks.count; i < count; i++)
			{
				StateLinkRerouteNode stateLinkNode = stateLinks[i];
				if (rerouteNode != stateLinkNode && stateLinkNode.position.Contains(position))
				{
					if (!CheckLoop(rerouteNode, stateLinkNode))
					{
						return stateLinkNode;
					}
				}
			}

			return null;
		}

		protected override Node GetActiveNode()
		{
			return stateMachine.currentState;
		}

		public StateBehaviour AddSetParameterBehaviour(State state, Parameter parameter)
		{
			Arbor.ParameterBehaviours.SetParameterBehaviourInternal setParameterBehaviour = state.AddBehaviour(Types.SetParameterBehaviourType) as Arbor.ParameterBehaviours.SetParameterBehaviourInternal;

			Undo.RecordObject(setParameterBehaviour, "Add Behaviour");

			setParameterBehaviour.SetParameter(parameter);

			EditorUtility.SetDirty(setParameterBehaviour);

			return setParameterBehaviour;
		}

		public StateBehaviour InsertSetParameterBehaviour(State state, int index, Parameter parameter)
		{
			Arbor.ParameterBehaviours.SetParameterBehaviourInternal setParameterBehaviour = state.InsertBehaviour(index, Types.SetParameterBehaviourType) as Arbor.ParameterBehaviours.SetParameterBehaviourInternal;

			Undo.RecordObject(setParameterBehaviour, "Insert Behaviour");

			setParameterBehaviour.SetParameter(parameter);

			EditorUtility.SetDirty(setParameterBehaviour);

			return setParameterBehaviour;
		}


		protected override void OnCreateSetParameter(Vector2 position, Parameter parameter)
		{
			Undo.IncrementCurrentGroup();
			int undoGroup = Undo.GetCurrentGroup();

			State state = CreateState(position, false);

			AddSetParameterBehaviour(state, parameter);

			Undo.CollapseUndoOperations(undoGroup);

			EditorUtility.SetDirty(nodeGraph);
		}

		void DrawMinimapBehaviourBranchies(MinimapViewport minimapViewport, StateEditor stateEditor)
		{
			State state = stateEditor.state;
			int behaviourCount = state.behaviourCount;

			for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
			{
				StateBehaviourEditorGUI behaviourEditor = stateEditor.GetBehaviourEditor(behaviourIndex);
				if (behaviourEditor == null)
				{
					continue;
				}

				StateBehaviour behaviour = behaviourEditor.behaviourObj as StateBehaviour;
				if (behaviour == null)
				{
					continue;
				}

				var stateLinkProperties = behaviourEditor.stateLinkProperties;
				for (int stateLinkIndex = 0; stateLinkIndex < stateLinkProperties.Count; stateLinkIndex++)
				{
					StateEditor.StateLinkProperty stateLinkProperty = stateLinkProperties[stateLinkIndex];
					stateLinkProperty.stateLinkGUI.DrawMinimapBranch(minimapViewport);
				}
			}
		}

		protected override void OnDrawMinimapBranchies(MinimapViewport minimapViewport)
		{
			StateLinkRerouteNodeList stateLinkRerouteNodes = stateMachine.stateLinkRerouteNodes;
			int rerouteCount = stateLinkRerouteNodes.count;
			for (int i = 0; i < rerouteCount; i++)
			{
				StateLinkRerouteNode rerouteNode = stateLinkRerouteNodes[i];

				if (rerouteNode == null)
				{
					continue;
				}

				StateLinkRerouteNodeEditor rerouteNodeEditor = GetNodeEditor(rerouteNode) as StateLinkRerouteNodeEditor;
				if (rerouteNodeEditor == null)
				{
					continue;
				}

				rerouteNodeEditor.stateLinkGUI.DrawMinimapBranch(minimapViewport);
			}

			int stateCount = stateMachine.stateCount;
			for (int i = 0; i < stateCount; i++)
			{
				State state = stateMachine.GetStateFromIndex(i);
				StateEditor stateEditor = GetNodeEditor(state) as StateEditor;

				if (stateEditor != null)
				{
					stateEditor.UpdateBehaviour();

					DrawMinimapBehaviourBranchies(minimapViewport, stateEditor);
				}
			}
		}
	}
}