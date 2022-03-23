//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;


namespace ArborEditor
{
	using Arbor;
	using Arbor.Playables;

	[CustomNodeEditor(typeof(State))]
	public sealed class StateEditor : NodeEditor
	{
		public State state
		{
			get
			{
				return node as State;
			}
		}

		public sealed class StateLinkProperty
		{
			public GUIContent label;
			public SerializedProperty property;
			public StateLinkGUI stateLinkGUI;

			public StateLinkProperty(NodeEditor nodeEditor, StateBehaviour behaviour)
			{
				stateLinkGUI = new StateLinkGUI(nodeEditor, behaviour);
			}
		}

		private StateBehaviourEditorList _BehaviourList = new StateBehaviourEditorList();

		protected override void OnInitialize()
		{
			_BehaviourList.nodeEditor = this;
			CreateEditors();
		}

		void CreateEditors()
		{
			State state = this.state;
			if (state != null)
			{
				_BehaviourList.RebuildBehaviourEditors();
			}
		}

		public StateBehaviourEditorGUI GetBehaviourEditor(int behaviourIndex)
		{
			return _BehaviourList.GetBehaviourEditor(behaviourIndex);
		}

		public void UpdateBehaviour()
		{
			using (new ProfilerScope("UpdateBehaviour"))
			{
				int behaviourCount = state.behaviourCount;
				for (int i = 0; i < behaviourCount; i++)
				{
					StateBehaviourEditorGUI behaviourEditor = GetBehaviourEditor(i);

					if (behaviourEditor != null)
					{
						behaviourEditor.UpdateStateLink();
					}
				}
			}
		}

		public void MoveBehaviour(int fromIndex, int toIndex)
		{
			ArborFSMInternal stateMachine = state.stateMachine;

			Undo.IncrementCurrentGroup();

			string undoName = (fromIndex > toIndex) ? "MoveUp Behaviour" : "MoveDown Behaviour";

			Undo.RecordObject(stateMachine, undoName);

			state.SwapBehaviour(fromIndex, toIndex);

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			EditorUtility.SetDirty(stateMachine);

			_BehaviourList.MoveBehaviourEditor(fromIndex, toIndex);
		}

		public void RemoveBehaviour(int behaviourIndex)
		{
			_BehaviourList.RemoveBehaviourEditor(behaviourIndex);

			Undo.IncrementCurrentGroup();
			int undoGruop = Undo.GetCurrentGroup();

			state.DestroyBehaviourAt(behaviourIndex);

			Undo.CollapseUndoOperations(undoGruop);

			graphEditor.RaiseOnChangedNodes();
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnDestroy()
		{
			_BehaviourList.DestroyEditors();
		}

		public override void Validate(Node node)
		{
			base.Validate(node);

			_BehaviourList.nodeEditor = this;
			_BehaviourList.Validate();
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnEnable()
		{
			_BehaviourList.nodeEditor = this;

			isRenamable = true;
			isShowableComment = true;
		}

		protected override void BeginRename()
		{
			if (graphEditor != null)
			{
				graphEditor.BeginRename(state.nodeID, state.name);
			}
		}

		public override void ExpandAll(bool expanded)
		{
			int behaviourCount = state.behaviourCount;
			if (behaviourCount > 0)
			{
				for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
				{
					StateBehaviourEditorGUI behaviourEditor = GetBehaviourEditor(behaviourIndex);
					if (behaviourEditor != null)
					{
						behaviourEditor.SetExpanded(expanded);
					}
				}
			}
		}

		void ExpandAll()
		{
			ExpandAll(true);
		}

		void FoldAll()
		{
			ExpandAll(false);
		}

		protected override void SetContextMenu(GenericMenu menu, Rect headerPosition, bool editable)
		{
			State state = this.state;
			SerializedObject serializedObject = new SerializedObject(state.stateMachine);

			SerializedProperty startStateIDPropery = serializedObject.FindProperty("_StartStateID");

			if (!state.resident)
			{
				if (startStateIDPropery.intValue != state.nodeID && editable)
				{
					menu.AddItem(EditorContents.setStartState, false, SetStartStateContextMenu);
				}
				else
				{
					menu.AddDisabledItem(EditorContents.setStartState);
				}

				if (editable)
				{
					menu.AddItem(EditorContents.breakPoint, state.breakPoint, FlipStateBreakPoint);
				}
				else
				{
					menu.AddDisabledItem(EditorContents.breakPoint);
				}
			}

			if (menu.GetItemCount() > 0)
			{
				menu.AddSeparator("");
			}

			if (editable)
			{
				menu.AddItem(EditorContents.addBehaviour, false, AddBehaviourToStateContextMenu, EditorGUITools.GUIToScreenRect(headerPosition));
			}
			else
			{
				menu.AddDisabledItem(EditorContents.addBehaviour);
			}

			if (Clipboard.CompareBehaviourType(typeof(StateBehaviour), true) && editable)
			{
				menu.AddItem(EditorContents.pasteBehaviour, false, PasteBehaviourToStateContextMenu);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.pasteBehaviour);
			}

			menu.AddSeparator("");

			menu.AddItem(EditorContents.expandAll, false, ExpandAll);

			menu.AddItem(EditorContents.collapseAll, false, FoldAll);
		}

		protected override void SetDebugContextMenu(GenericMenu menu, Rect headerPosition, bool editable)
		{
			State state = this.state;

			if (!state.resident)
			{
				if (menu.GetItemCount() > 0)
				{
					menu.AddSeparator("");
				}
				if (Application.isPlaying && editable)
				{
					menu.AddItem(EditorContents.transition, false, TransitionState);
				}
				else
				{
					menu.AddDisabledItem(EditorContents.transition);
				}
			}
		}

		void SetStartStateContextMenu()
		{
			SerializedObject serializedObject = new SerializedObject(state.stateMachine);

			serializedObject.Update();

			SerializedProperty startStateIDPropery = serializedObject.FindProperty("_StartStateID");

			startStateIDPropery.intValue = state.nodeID;

			serializedObject.ApplyModifiedProperties();

			serializedObject.Dispose();
		}

		void FlipStateBreakPoint()
		{
			ArborFSMInternal stateMachine = state.stateMachine;

			if (state.breakPoint)
			{
				Undo.RecordObject(stateMachine, "State BreakPoint Off");
			}
			else
			{
				Undo.RecordObject(stateMachine, "State BreakPoint On");
			}

			state.breakPoint = !state.breakPoint;

			EditorUtility.SetDirty(stateMachine);
		}

		void TransitionState()
		{
			ArborFSMInternal stateMachine = state.stateMachine;

			stateMachine.Transition(state);
		}

		internal static void PasteStateBehaviourAsNew(State state, int index)
		{
			PasteStateBehaviourAsNew(state, index, Clipboard.copyBehaviour);
		}

		internal static void PasteStateBehaviourAsNew(State state, int index, NodeBehaviour destBehaviour)
		{
			using (new Presets.DisableApplyDefaultPresetScope(true))
			{
				StateBehaviour behaviour = NodeBehaviour.CreateNodeBehaviour(state, destBehaviour.GetType(), true) as StateBehaviour;

				if (index != -1)
				{
					state.InsertBehaviour(index, behaviour);
				}
				else
				{
					state.AddBehaviour(behaviour);
				}

				Clipboard.CopyNodeBehaviour(destBehaviour, behaviour, true);
			}
		}

		public void PasteBehaviour(int index)
		{
			Undo.IncrementCurrentGroup();

			ArborFSMInternal stateMachine = state.stateMachine;

			Undo.RecordObject(stateMachine, "Paste Behaviour");

			PasteStateBehaviourAsNew(state, index);

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			EditorUtility.SetDirty(stateMachine);

			graphEditor.RaiseOnChangedNodes();
		}

		void PasteBehaviourToStateContextMenu()
		{
			PasteBehaviour(-1);
		}

		public void InsertBehaviour(int index, System.Type classType)
		{
			Object behaviourObj = state.InsertBehaviour(index, classType);

			_BehaviourList.InsertBehaviourEditor(index, behaviourObj);

			graphEditor.RaiseOnChangedNodes();
		}

		public void AddBehaviour(System.Type classType)
		{
			State state = this.state;

			Object behaviourObj = state.AddBehaviour(classType);

			_BehaviourList.InsertBehaviourEditor(state.behaviourCount - 1, behaviourObj);

			graphEditor.RaiseOnChangedNodes();
		}

		public void InsertSetParameterBehaviour(int index, Parameter parameter)
		{
			StateMachineGraphEditor stateMachineGraphEditor = graphEditor as StateMachineGraphEditor;
			Object behaviourObj = stateMachineGraphEditor.InsertSetParameterBehaviour(state, index, parameter);

			_BehaviourList.InsertBehaviourEditor(index, behaviourObj);

			graphEditor.RaiseOnChangedNodes();
		}

		public void AddSetParameterBehaviour(Parameter parameter)
		{
			State state = this.state;

			StateMachineGraphEditor stateMachineGraphEditor = graphEditor as StateMachineGraphEditor;
			Object behaviourObj = stateMachineGraphEditor.AddSetParameterBehaviour(state, parameter);

			_BehaviourList.InsertBehaviourEditor(state.behaviourCount - 1, behaviourObj);

			graphEditor.RaiseOnChangedNodes();
		}

		void AddBehaviourToStateContextMenu(object obj)
		{
			Rect position = (Rect)obj;

			BehaviourMenuWindow.instance.Init(this, position);
		}

		void StateLinkListGUI()
		{
			int count = _BehaviourList.GetCount();

			GUIStyle headerStyle = Styles.stateLinkHeader;

			for (int i = 0; i < count; i++)
			{
				StateBehaviourEditorGUI editorGUI = _BehaviourList.GetBehaviourEditor(i);
				editorGUI.StateLinkGUI(headerStyle);
			}
		}

		protected override void OnGUI()
		{
			using (new ProfilerScope("OnStateGUI"))
			{
				if (ArborSettings.stateLinkShowMode == StateLinkShowMode.NodeTop)
				{
					StateLinkListGUI();
				}
				using (new ProfilerScope("Behaviours"))
				{
					_BehaviourList.OnGUI();
				}
				if (ArborSettings.stateLinkShowMode == StateLinkShowMode.NodeBottom)
				{
					StateLinkListGUI();
				}

				bool editable = graphEditor.editable;

				Event current = Event.current;
				switch (current.type)
				{
					case EventType.DragUpdated:
					case EventType.DragPerform:
						if (GetHeaderRect().Contains(current.mousePosition))
						{
							bool isPerform = current.type == EventType.DragPerform;
							bool isAccepted = false;
							var objectReferences = DragAndDrop.objectReferences;
							for (int objIndex = 0; objIndex < objectReferences.Length; objIndex++)
							{
								Object draggedObject = objectReferences[objIndex];
								MonoScript script = draggedObject as MonoScript;
								if (script != null)
								{
									System.Type classType = script.GetClass();

									if (classType != null && classType.IsSubclassOf(typeof(StateBehaviour)))
									{
										if (editable)
										{
											DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

											if (isPerform)
											{
												AddBehaviour(classType);
												isAccepted = true;
											}
										}
										else
										{
											DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
										}

										current.Use();
									}
								}
								else
								{
									ParameterDraggingObject parameterDraggingObject = draggedObject as ParameterDraggingObject;
									if (parameterDraggingObject != null)
									{
										if (editable)
										{
											DragAndDrop.visualMode = DragAndDropVisualMode.Link;

											if (isPerform)
											{
												AddSetParameterBehaviour(parameterDraggingObject.parameter);

												isAccepted = true;
											}
										}
										else
										{
											DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
										}

										current.Use();
									}
								}
							}

							if (isAccepted)
							{
								DragAndDrop.AcceptDrag();
								DragAndDrop.activeControlID = 0;
							}
						}
						break;
				}
			}
		}

		public override bool IsDraggingVisible()
		{
			int behaviourCount = state.behaviourCount;

			for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
			{
				StateBehaviourEditorGUI behaviourEditor = GetBehaviourEditor(behaviourIndex);

				if (behaviourEditor == null)
				{
					continue;
				}

				var stateLinkProperties = behaviourEditor.stateLinkProperties;
				for (int stateLinkIndex = 0; stateLinkIndex < stateLinkProperties.Count; stateLinkIndex++)
				{
					StateLinkProperty stateLinkProperty = stateLinkProperties[stateLinkIndex];
					Node targetNode = state.stateMachine.GetNodeFromID(stateLinkProperty.stateLinkGUI.stateLink.stateID);
					if (targetNode != null)
					{
						if (graphEditor.IsDraggingNode(targetNode))
						{
							return true;
						}
						StateLinkRerouteNodeEditor rerouteNodeEditor = graphEditor.GetNodeEditor(targetNode) as StateLinkRerouteNodeEditor;
						if (rerouteNodeEditor != null && rerouteNodeEditor.isDragDirection)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public override void OnRename(string name)
		{
			if (name != state.name)
			{
				NodeGraph nodeGraph = state.nodeGraph;

				Undo.RecordObject(nodeGraph, "Rename State");

				state.name = name;

				EditorUtility.SetDirty(nodeGraph);
			}
		}

		private static readonly Styles.Color s_DragColor = Styles.Color.Red;
		private static readonly Styles.Color s_CurrentColor = Styles.Color.Orange;
		private static readonly Styles.Color s_ReservedColor = Styles.Color.Purple;
		private static readonly Styles.Color s_StartColor = Styles.Color.Aqua;
		private static readonly Styles.Color s_ResidentColor = Styles.Color.Green;
		private static readonly Styles.Color s_NormalColor = Styles.Color.Gray;

		public override bool IsActive()
		{
			ArborFSMInternal stateMachine = state.stateMachine;
			return stateMachine.currentState == state;
		}

		public override Styles.Color GetStyleColor()
		{
			ArborFSMInternal stateMachine = state.stateMachine;

			StateMachineGraphEditor stateMachineGraphEditor = graphEditor as StateMachineGraphEditor;
			if (stateMachineGraphEditor != null && stateMachineGraphEditor.IsDragBranchHover(state))
			{
				return s_DragColor;
			}
			else if (stateMachine.currentState == state)
			{
				return s_CurrentColor;
			}
			else if (stateMachine.playState == PlayState.InactivePausing && stateMachine.reservedState == state)
			{
				return s_ReservedColor;
			}
			else if (stateMachine.startStateID == state.nodeID)
			{
				return s_StartColor;
			}
			else if (state.resident)
			{
				return s_ResidentColor;
			}

			return s_NormalColor;
		}

		static Texture2D GetStateIcon(State state)
		{
			ArborFSMInternal stateMachine = state.stateMachine;

			bool isCurrentState = Application.isPlaying && stateMachine.currentState == state;
			if (stateMachine.startStateID == state.nodeID)
			{
				return isCurrentState ? Icons.currentStartStateIcon : Icons.startStateIcon;
			}
			else if (state.resident)
			{
				return Icons.residentStateIcon;
			}

			return isCurrentState ? Icons.currentNormalStateIcon : Icons.normalStateIcon;
		}

		public override Texture2D GetIcon()
		{
			return GetStateIcon(state);
		}

		public override bool IsShowNodeList()
		{
			return true;
		}

		public override void OnListElement(Rect rect)
		{
			if (state.breakPoint)
			{
				GUIStyle style = (Application.isPlaying && EditorApplication.isPaused && state.stateMachine.currentState == state) ? Styles.breakpointOn : Styles.breakpoint;
				GUIContent content = EditorContents.breakPointTooltip;
				Vector2 size = style.CalcSize(content);
				Rect breakRect = new Rect(rect.x, rect.center.y - size.y * 0.5f, size.x, size.y);
				EditorGUI.LabelField(breakRect, content, style);
			}

			if (Application.isPlaying)
			{
				GUIStyle style = Styles.countBadge;
				GUIContent content = new GUIContent(state.transitionCount.ToString());
				Vector2 size = style.CalcSize(content);
				Rect countRect = new Rect(rect.x + rect.width - size.x - 2, rect.center.y - size.y * 0.5f, size.x, size.y);

				EditorGUI.LabelField(countRect, content, style);
			}
		}

		protected override bool HasOutsideGUI()
		{
			return true;
		}

		protected override RectOffset GetOutsideOffset()
		{
			RectOffset offset = _BehaviourList.IsVisibleDataLinkGUI() ? new RectOffset(DataSlotGUIUtility.kOutsideOffset, 0, 0, 0) : new RectOffset();

			State state = this.state;

			if (state.breakPoint)
			{
				GUIStyle style = (Application.isPlaying && EditorApplication.isPaused && state.stateMachine.currentState == state) ? Styles.breakpointOn : Styles.breakpoint;
				GUIContent content = EditorContents.breakPointTooltip;
				Vector2 size = style.CalcSize(content);

				offset.left = Mathf.Max(offset.left, (int)(size.x * 0.5f));
				offset.top = Mathf.Max(offset.top, (int)(size.y * 0.5f));
			}

			if (Application.isPlaying)
			{
				GUIStyle style = Styles.countBadge;
				GUIContent content = new GUIContent(state.transitionCount.ToString());
				Vector2 size = style.CalcSize(content);

				offset.right = Mathf.Max(offset.right, (int)(size.x * 0.5f));
				offset.top = Mathf.Max(offset.top, (int)(size.y * 0.5f));
			}

			return offset;
		}

		protected override void OnOutsideGUI()
		{
			RectOffset overflowOffset = GetOverflowOffset();

			_BehaviourList.OnDataLinkGUI(overflowOffset);

			State state = this.state;

			if (state.breakPoint)
			{
				GUIStyle style = (Application.isPlaying && EditorApplication.isPaused && state.stateMachine.currentState == state) ? Styles.breakpointOn : Styles.breakpoint;
				GUIContent content = EditorContents.breakPointTooltip;
				Vector2 size = style.CalcSize(content);
				Rect breakRect = new Rect(-size.x * 0.5f, -size.y * 0.5f, size.x, size.y);
				breakRect.position += new Vector2(overflowOffset.left, overflowOffset.top);

				EditorGUI.LabelField(breakRect, content, style);
			}

			if (Application.isPlaying)
			{
				GUIStyle style = Styles.countBadge;
				GUIContent content = new GUIContent(state.transitionCount.ToString());
				Vector2 size = style.CalcSize(content);
				Rect countRect = new Rect(state.position.width - size.x * 0.5f, -size.y * 0.5f, size.x, size.y);
				countRect.position += new Vector2(overflowOffset.left, overflowOffset.top);

				EditorGUI.LabelField(countRect, content, style);
			}
		}
	}
}
