//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

using Arbor;

namespace ArborEditor
{
	[CustomNodeEditor(typeof(StateLinkRerouteNode))]
	internal sealed class StateLinkRerouteNodeEditor : NodeEditor
	{
		private StateLinkGUI _StateLinkGUI = null;

		public StateLinkRerouteNode stateLinkRerouteNode
		{
			get
			{
				return node as StateLinkRerouteNode;
			}
		}

		public StateLinkGUI stateLinkGUI
		{
			get
			{
				if (_StateLinkGUI == null)
				{
					_StateLinkGUI = new StateLinkGUI(this, null);
				}
				_StateLinkGUI.stateLink = stateLinkRerouteNode.link;
				return _StateLinkGUI;
			}
		}

		protected override bool HasHeaderGUI()
		{
			return false;
		}

		public override GUIContent GetTitleContent()
		{
			return GUIContent.none;
		}

		public override string GetTitle()
		{
			return Localization.GetWord("StateLinkRerouteNode");
		}

		protected override float GetWidth()
		{
			return 32f;
		}

		protected override GUIStyle GetBackgroundStyle()
		{
			return GUIStyle.none;
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnEnable()
		{
			isNormalInvisibleStyle = true;
			isShowContextMenuInWindow = true;
			isUsedMouseDownOnMainGUI = false;
			isResizable = false;
		}

		private static readonly int s_DirectionFieldHash = "s_DirectionFieldHash".GetHashCode();

		public bool isDragDirection
		{
			get;
			private set;
		}

		private Vector2 _BeginDirection;

		void DirectionField(Rect position, Color pinColor, bool isSelection)
		{
			int controlID = EditorGUIUtility.GetControlID(s_DirectionFieldHash, FocusType.Passive);

			Event currentEvent = Event.current;

			EventType eventType = currentEvent.GetTypeForControl(controlID);

			Vector2 center = position.center;

			Vector2 direction = stateLinkRerouteNode.direction;

			Vector2 arrowPosition = center + direction * 16f;

			float arrowWidth = 8f;
			float arrowWidthHalf = arrowWidth * 0.5f;
			Vector2 arrowCenter = arrowPosition - direction * arrowWidthHalf;
			Rect arrowRect = new Rect(arrowCenter.x - arrowWidthHalf, arrowCenter.y - arrowWidthHalf, arrowWidth, arrowWidth);

			switch (eventType)
			{
				case EventType.MouseDown:
					if (isSelection && arrowRect.Contains(currentEvent.mousePosition) && currentEvent.button == 0)
					{
						isDragDirection = true;
						graphEditor.BeginDisableContextClick();
						_BeginDirection = stateLinkRerouteNode.direction;
						GUIUtility.hotControl = controlID;
						currentEvent.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
					{
						Undo.RecordObject(node.nodeGraph, "Change Reroute Direction");
						stateLinkRerouteNode.direction = (currentEvent.mousePosition - position.center).normalized;
						EditorUtility.SetDirty(node.nodeGraph);
						currentEvent.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						if (currentEvent.button == 0)
						{
							graphEditor.EndDisableContextClick();
							GUIUtility.hotControl = 0;
							isDragDirection = false;
						}
						currentEvent.Use();
					}
					break;
				case EventType.KeyDown:
					if (GUIUtility.hotControl == controlID && currentEvent.keyCode == KeyCode.Escape)
					{
						Undo.RecordObject(node.nodeGraph, "Change Reroute Direction");
						stateLinkRerouteNode.direction = _BeginDirection;
						EditorUtility.SetDirty(node.nodeGraph);

						GUIUtility.hotControl = 0;
						isDragDirection = false;
						currentEvent.Use();

						Repaint();
					}
					break;
				case EventType.Repaint:
					if (isSelection)
					{
						bool isHover = GUIUtility.hotControl == controlID || arrowRect.Contains(currentEvent.mousePosition);
						Color color = isHover ? Color.cyan : pinColor;
						EditorGUITools.DrawArrow(arrowPosition, direction, color, arrowWidth);
					}
					break;
			}
		}

		void StateLinkField(StateLinkGUI stateLinkGUI)
		{
			Rect position = GUILayoutUtility.GetRect(32f, 32f);

			bool on = stateLinkGUI.RerouteSlotField(position, stateLinkRerouteNode.direction);

			StateLink stateLink = stateLinkGUI.stateLink;
			Color lineColor = stateLink.lineColorChanged ? stateLink.lineColor : Color.white;

			DirectionField(position, lineColor, isSelection || !on);
		}

		void DeleteKeepConnection()
		{
			NodeGraph nodeGraph = this.node.nodeGraph;
			ArborFSMInternal stateMachine = nodeGraph as ArborFSMInternal;
			StateLinkRerouteNode rerouteNode = stateLinkRerouteNode;

			Undo.IncrementCurrentGroup();
			int undoGroup = Undo.GetCurrentGroup();

			StateLinkGUI stateLinkGUI = this.stateLinkGUI;

			int nextStateID = stateLinkGUI.stateLink.stateID;
			Vector2 nextEndPosition = stateLinkGUI.bezier.endPosition;
			Vector2 nextEndControl = stateLinkGUI.bezier.endControl;

			StateLinkRerouteNodeList stateLinkRerouteNodes = stateMachine.stateLinkRerouteNodes;
			int rerouteCount = stateLinkRerouteNodes.count;
			for (int i = 0; i < rerouteCount; i++)
			{
				StateLinkRerouteNode node = stateLinkRerouteNodes[i];

				if (node == null || node.link.stateID != rerouteNode.nodeID)
				{
					continue;
				}

				StateLinkRerouteNodeEditor nodeEditor = graphEditor.GetNodeEditor(node) as StateLinkRerouteNodeEditor;
				if (nodeEditor == null)
				{
					continue;
				}

				Undo.RecordObject(stateMachine, "Delete Keep Connection");

				node.link.stateID = nextStateID;

				Bezier2D bezier = nodeEditor.stateLinkGUI.bezier;
				bezier.endPosition = nextEndPosition;
				bezier.endControl = nextEndControl;

				graphEditor.VisibleNode(node);
			}

			for (int stateIndex = 0, count = stateMachine.stateCount; stateIndex < count; stateIndex++)
			{
				State state = stateMachine.GetStateFromIndex(stateIndex);
				StateEditor stateEditor = graphEditor.GetNodeEditor(state) as StateEditor;

				bool visible = false;

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
						StateLinkGUI stateLinkGUI_ = stateLinkProperty.stateLinkGUI;
						StateLink stateLink = stateLinkGUI_.stateLink;
						if (stateLink.stateID == rerouteNode.nodeID)
						{
							Undo.RecordObject(behaviourEditor.behaviourObj, "Delete Keep Connection");

							stateLink.stateID = nextStateID;
							stateLinkGUI_.bezier.endPosition = nextEndPosition;
							stateLinkGUI_.bezier.endControl = nextEndControl;

							EditorUtility.SetDirty(behaviourEditor.behaviourObj);

							visible = true;
						}
					}
				}

				if (visible)
				{
					graphEditor.VisibleNode(state);
				}
			}

			graphEditor.DeleteNodes(new Node[] { rerouteNode });

			Undo.CollapseUndoOperations(undoGroup);

			EditorUtility.SetDirty(nodeGraph);

			Repaint();
		}

		bool IsConnected()
		{
			NodeGraph nodeGraph = this.node.nodeGraph;
			ArborFSMInternal stateMachine = nodeGraph as ArborFSMInternal;
			StateLinkRerouteNode rerouteNode = stateLinkRerouteNode;

			int nextStateID = rerouteNode.link.stateID;

			if (nextStateID == 0)
			{
				return false;
			}

			StateLinkRerouteNodeList stateLinkRerouteNodes = stateMachine.stateLinkRerouteNodes;
			int rerouteCount = stateLinkRerouteNodes.count;
			for (int i = 0; i < rerouteCount; i++)
			{
				StateLinkRerouteNode node = stateLinkRerouteNodes[i];

				if (node != null && node.link.stateID == rerouteNode.nodeID)
				{
					return true;
				}
			}

			for (int stateIndex = 0, count = stateMachine.stateCount; stateIndex < count; stateIndex++)
			{
				State state = stateMachine.GetStateFromIndex(stateIndex);
				StateEditor stateEditor = graphEditor.GetNodeEditor(state) as StateEditor;

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
						if (stateLinkProperty.stateLinkGUI.stateLink.stateID == rerouteNode.nodeID)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		protected override void SetDeleteContextMenu(GenericMenu menu, bool deletable, bool editable)
		{
			if (deletable && IsConnected() && editable)
			{
				menu.AddItem(EditorContents.deleteKeepConnection, false, DeleteKeepConnection);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.deleteKeepConnection);
			}
		}

		protected override void SetContextMenu(GenericMenu menu, Rect headerPosition, bool editable)
		{
			Rect mousePosition = new Rect(0, 0, 0, 0);
			mousePosition.position = Event.current.mousePosition;
			Rect position = EditorGUITools.GUIToScreenRect(mousePosition);

			menu.AddItem(EditorContents.settings, false, () =>
			{
				stateLinkGUI.OpenSettingsWindow(GUIUtility.ScreenToGUIRect(position), false);
			});
		}

		protected override void OnGUI()
		{
			using (new ProfilerScope("OnGUI"))
			{
				StateLinkField(stateLinkGUI);
			}
		}

		public override bool IsDraggingVisible()
		{
			Node targetNode = graphEditor.nodeGraph.GetNodeFromID(stateLinkRerouteNode.link.stateID);
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
			return false;
		}

		public override MinimapLayer minimapLayer
		{
			get
			{
				return MinimapLayer.None;
			}
		}
	}
}
