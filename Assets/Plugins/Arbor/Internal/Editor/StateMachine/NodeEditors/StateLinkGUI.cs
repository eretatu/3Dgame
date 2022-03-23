//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	using Arbor;
	using Arbor.Playables;

	public sealed class StateLinkGUI
	{
		private static readonly int s_StateLinkHash = "s_StateLinkHash".GetHashCode();
		private static readonly int s_DrawStateLinkBranchHash = "s_DrawStateLinkBranchHash".GetHashCode();

		private static StateLinkSettingWindow s_StateLinkSettingWindow = new StateLinkSettingWindow();

		public readonly StateMachineGraphEditor _GraphEditor;

		public NodeEditor nodeEditor
		{
			get;
			private set;
		}

		private readonly StateBehaviour _StateBehaviour;

		public StateLink stateLink;
		public System.Reflection.FieldInfo fieldInfo;
		public Bezier2D bezier = new Bezier2D();

		public StateLinkGUI(NodeEditor nodeEditor, StateBehaviour stateBehaviour)
		{
			_GraphEditor = nodeEditor.graphEditor as StateMachineGraphEditor;
			this.nodeEditor = nodeEditor;
			_StateBehaviour = stateBehaviour;
		}

		static TransitionTiming GetTransitionTiming(StateLink stateLink, System.Reflection.FieldInfo stateLinkFieldInfo)
		{
			FixedTransitionTiming fixedTransitionTiming = AttributeHelper.GetAttribute<FixedTransitionTiming>(stateLinkFieldInfo);
			FixedImmediateTransition fixedImmediateTransition = AttributeHelper.GetAttribute<FixedImmediateTransition>(stateLinkFieldInfo);

			TransitionTiming transitionTiming = TransitionTiming.LateUpdateDontOverwrite;

			if (fixedTransitionTiming != null)
			{
				transitionTiming = fixedTransitionTiming.transitionTiming;
			}
			else if (fixedImmediateTransition != null)
			{
				transitionTiming = fixedImmediateTransition.immediate ? TransitionTiming.Immediate : TransitionTiming.LateUpdateOverwrite;
			}
			else
			{
				transitionTiming = stateLink.transitionTiming;
			}

			return transitionTiming;
		}

		public static Texture GetTransitionTimingIcon(TransitionTiming transitionTiming)
		{
			switch (transitionTiming)
			{
				case TransitionTiming.LateUpdateOverwrite:
					return Icons.transitionTimingLateUpdateOverwrite;
				case TransitionTiming.Immediate:
					return Icons.transitionTimingImmediate;
				case TransitionTiming.LateUpdateDontOverwrite:
					return Icons.transitionTimingLateUpdateDontOverwrite;
				case TransitionTiming.NextUpdateOverwrite:
					return Icons.transitionTimingNextUpdateOverwrite;
				case TransitionTiming.NextUpdateDontOverwrite:
					return Icons.transitionTimingNextUpdateDontOverwrite;
			}

			return null;
		}

		private sealed class Pivot
		{
			public Vector2 position;
			public Vector2 pivotPosition;
			public Vector2 normal;

			public Pivot(Vector2 position, Vector2 normal)
			{
				this.position = position;
				this.pivotPosition = position;
				this.normal = normal;
			}

			public Pivot(Vector2 position, Vector2 pivotPosition, Vector2 normal)
			{
				this.position = position;
				this.pivotPosition = pivotPosition;
				this.normal = normal;
			}
		}

		public static Bezier2D GetTargetBezier(Node currentNode, Node targetNode, Vector2 leftPos, Vector2 rightPos, ref bool right)
		{
			Vector2 startPos = Vector2.zero;
			Vector2 startTangent = Vector2.zero;
			Vector2 endPos = Vector2.zero;
			Vector2 endTangent = Vector2.zero;

			right = true;

			if (targetNode != null)
			{
				Rect targetRect = targetNode.position;
				targetRect.x -= currentNode.position.x;
				targetRect.y -= currentNode.position.y;

				Pivot findPivot = null;

				List<Pivot> pivots = new List<Pivot>();

				StateLinkRerouteNode targetRerouteNode = targetNode as StateLinkRerouteNode;
				if (targetRerouteNode != null)
				{
					Pivot leftPivot = new Pivot(targetRect.center - targetRerouteNode.direction * 6f, targetRect.center, -targetRerouteNode.direction);
					pivots.Add(leftPivot);
					pivots.Add(leftPivot);
				}
				else
				{
					pivots.Add(new Pivot(new Vector2(targetRect.xMin, targetRect.yMin + EditorGUITools.kStateBezierTargetOffsetY), -Vector2.right));
					pivots.Add(new Pivot(new Vector2(targetRect.xMax, targetRect.yMin + EditorGUITools.kStateBezierTargetOffsetY), Vector2.right));
				}

				if (targetRect.x == 0.0f)
				{
					if (targetRect.y > 0.0f)
					{
						findPivot = pivots[0];
						right = false;
					}
					else
					{
						findPivot = pivots[1];
						right = true;
					}
				}
				else
				{
					float findDistance = 0.0f;

					int pivotCount = pivots.Count;
					for (int pivotIndex = 0; pivotIndex < pivotCount; pivotIndex++)
					{
						Pivot pivot = pivots[pivotIndex];

						Vector2 vl = leftPos - pivot.pivotPosition;
						Vector2 vr = rightPos - pivot.pivotPosition;

						float leftDistance = vl.magnitude;
						float rightDistance = vr.magnitude;

						float distance = 0.0f;
						bool checkRight = false;

						if (leftDistance > rightDistance)
						{
							distance = rightDistance;
							checkRight = true;
						}
						else
						{
							distance = leftDistance;
							checkRight = false;
						}

						if (findPivot == null || distance < findDistance)
						{
							findPivot = pivot;
							findDistance = distance;
							right = checkRight;
						}
					}
				}

				StateLinkRerouteNode currentRerouteNode = currentNode as StateLinkRerouteNode;
				if (currentRerouteNode != null)
				{
					startPos = rightPos;
					startTangent = startPos + currentRerouteNode.direction * EditorGUITools.kBezierTangent;
				}
				else if (right)
				{
					startPos = rightPos;
					startTangent = rightPos + EditorGUITools.kBezierTangentOffset;
				}
				else
				{
					startPos = leftPos;
					startTangent = leftPos - EditorGUITools.kBezierTangentOffset;
				}

				endPos = findPivot.position;
				endTangent = endPos + findPivot.normal * EditorGUITools.kBezierTangent;
			}

			return new Bezier2D(startPos, startTangent, endPos, endTangent);
		}

		static Bezier2D GetTargetBezier(Vector2 targetPos, Vector2 leftPos, Vector2 rightPos, ref bool isRight)
		{
			bool right = (targetPos - leftPos).magnitude > (targetPos - rightPos).magnitude;

			Vector2 startPos;
			Vector2 startTangent;

			if (right)
			{
				isRight = true;
				startPos = rightPos;
				startTangent = rightPos + EditorGUITools.kBezierTangentOffset;
			}
			else
			{
				isRight = false;
				startPos = leftPos;
				startTangent = leftPos - EditorGUITools.kBezierTangentOffset;
			}

			return new Bezier2D(startPos, startTangent, targetPos, startTangent);
		}

		private static Node _DragTargetNode = null;

		public void OpenSettingsWindow(Rect settingRect, bool onGUI)
		{
			s_StateLinkSettingWindow.Init(_GraphEditor.hostWindow, (Object)_StateBehaviour ?? (Object)_GraphEditor.nodeGraph, stateLink, fieldInfo, nodeEditor.node is StateLinkRerouteNode);
			PopupWindowUtility.Show(settingRect, s_StateLinkSettingWindow, onGUI);
		}

		public void StateLinkField(GUIContent label)
		{
			int controlID = EditorGUIUtility.GetControlID(s_StateLinkHash, FocusType.Passive);

			bool isActive = GUIUtility.hotControl == controlID;

			GUIStyle style = isActive ? Styles.nodeLinkSlotActive : Styles.nodeLinkSlot;

			StateLinkGUI stateLinkGUI = this;
			StateLink stateLink = stateLinkGUI.stateLink;

			TransitionTiming transitionTiming = GetTransitionTiming(stateLink, stateLinkGUI.fieldInfo);

			label.image = GetTransitionTimingIcon(transitionTiming);

			Rect position = GUILayoutUtility.GetRect(label, style, GUILayout.Height(18f));

			Rect nodePosition = nodeEditor.node.position;
			nodePosition.position = Vector2.zero;

			StateBehaviour behaviour = _StateBehaviour;

			ArborFSMInternal stateMachine = behaviour.stateMachine;
			State state = stateMachine.GetStateFromID(behaviour.nodeID);

			Event currentEvent = Event.current;

			Node targetNode = stateMachine.GetNodeFromID(stateLink.stateID);

			Vector2 nowPos = currentEvent.mousePosition;

			Vector2 leftPos = new Vector2(nodePosition.xMin, position.center.y);
			Vector2 rightPos = new Vector2(nodePosition.xMax, position.center.y);

			Bezier2D bezier = new Bezier2D();
			bool isRight = true;
			if (targetNode != null)
			{
				bezier = GetTargetBezier(state, targetNode, leftPos, rightPos, ref isRight);
			}
			else
			{
				bezier.startPosition = rightPos;
			}

			Bezier2D draggingBezier = new Bezier2D();
			bool isDraggingRight = true;
			if (isActive)
			{
				if (_DragTargetNode != null)
				{
					draggingBezier = GetTargetBezier(state, _DragTargetNode, leftPos, rightPos, ref isDraggingRight);
				}
				else
				{
					draggingBezier = GetTargetBezier(nowPos, leftPos, rightPos, ref isDraggingRight);
				}
			}

			GUIStyle stateLinkPinStyle = isRight ? Styles.stateLinkRightPin : Styles.stateLinkLeftPin;
			Vector2 pinSize = stateLinkPinStyle.CalcSize(GUIContent.none);

			Rect boxRect = new Rect(isRight ? position.xMax - pinSize.x : position.xMin, position.y, pinSize.x, pinSize.y);
			boxRect.y += Mathf.Floor((position.height - boxRect.height) / 2f);

			GUIStyle draggingStateLinkPinStyle = isDraggingRight ? Styles.stateLinkRightPin : Styles.stateLinkLeftPin;
			Vector2 draggingPinSize = draggingStateLinkPinStyle.CalcSize(GUIContent.none);

			Rect draggingBoxRect = new Rect(isDraggingRight ? position.xMax - draggingPinSize.x : position.xMin, position.y, draggingPinSize.x, draggingPinSize.y);
			draggingBoxRect.y += Mathf.Floor((position.height - draggingBoxRect.height) / 2f);

			Rect settingRect = position;

			GUIStyle settingStyle = Styles.popupIconButton;
			GUIContent settingContent = EditorContents.stateLinkPopupIcon;
			Vector2 settingButtonSize = settingStyle.CalcSize(settingContent);

			settingRect.x += position.width - settingButtonSize.x - pinSize.x - 4f;
			settingRect.y += Mathf.Floor((position.height - settingButtonSize.y) / 2);
			settingRect.height = settingButtonSize.x;
			settingRect.width = settingButtonSize.y;

			Vector2 bezierStartPosition = bezier.startPosition;

			Vector2 statePosition = new Vector2(state.position.x, state.position.y);

			if (nodeEditor != null)
			{
				bezier.startPosition = nodeEditor.NodeToGraphPoint(bezier.startPosition);
				bezier.startControl = nodeEditor.NodeToGraphPoint(bezier.startControl);
			}
			else
			{
				bezier.startPosition += statePosition;
				bezier.startControl += statePosition;
			}
			bezier.endPosition += statePosition;
			bezier.endControl += statePosition;

			Vector2 draggingBezierStartPosition = draggingBezier.startPosition;

			if (nodeEditor != null)
			{
				draggingBezier.startPosition = nodeEditor.NodeToGraphPoint(draggingBezier.startPosition);
				draggingBezier.startControl = nodeEditor.NodeToGraphPoint(draggingBezier.startControl);
			}
			else
			{
				draggingBezier.startPosition = statePosition;
				draggingBezier.startControl = statePosition;
			}
			draggingBezier.endPosition += statePosition;
			draggingBezier.endControl += statePosition;

			Color lineColor = stateLink.lineColorChanged ? stateLink.lineColor : Color.white;

			EventType eventType = currentEvent.GetTypeForControl(controlID);
			switch (eventType)
			{
				case EventType.ContextClick:
					if (position.Contains(nowPos))
					{
						OpenSettingsWindow(settingRect, true);
						currentEvent.Use();
					}
					break;
				case EventType.MouseDown:
					if (position.Contains(nowPos) && !settingRect.Contains(nowPos))
					{
						if (currentEvent.button == 0)
						{
							GUIUtility.hotControl = GUIUtility.keyboardControl = controlID;

							_DragTargetNode = null;

							if (_GraphEditor != null)
							{
								_GraphEditor.BeginDragStateBranch(state.nodeID);
								_GraphEditor.DragStateBranchBezie(bezier);
							}

							currentEvent.Use();
						}
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID && currentEvent.button == 0)
					{
						DragAndDrop.PrepareStartDrag();

						Node hoverNode = !position.Contains(nowPos) ? _GraphEditor.GetTargetNodeFromPosition(nowPos + statePosition, state) : null;

						if (hoverNode != null)
						{
							if (_GraphEditor != null)
							{
								_GraphEditor.DragStateBranchHoverStateID(hoverNode.nodeID);
							}

							_DragTargetNode = hoverNode;
						}
						else
						{
							if (_GraphEditor != null)
							{
								_GraphEditor.DragStateBranchHoverStateID(0);
							}
							_DragTargetNode = null;
						}

						currentEvent.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						if (currentEvent.button == 0)
						{
							GUIUtility.hotControl = 0;

							if (_DragTargetNode == null)
							{
								GenericMenu menu = new GenericMenu();

								Vector2 currentMousePosition = currentEvent.mousePosition;
								Vector2 mousePosition = _GraphEditor.hostWindow.UnclipToGraph(currentMousePosition);
								Vector2 screenMousePosition = EditorGUIUtility.GUIToScreenPoint(currentMousePosition);

								menu.AddItem(EditorContents.createState, false, () =>
								{
									Undo.IncrementCurrentGroup();
									int undoGroup = Undo.GetCurrentGroup();

									mousePosition -= new Vector2(8f, 12f);

									State newState = _GraphEditor.CreateState(mousePosition, false);

									Undo.RecordObject(behaviour, "Link State");

									stateLink.stateID = newState.nodeID;
									this.bezier = bezier;

									Undo.CollapseUndoOperations(undoGroup);

									EditorUtility.SetDirty(behaviour);
								});

								menu.AddItem(EditorContents.reroute, false, () =>
								{
									Undo.IncrementCurrentGroup();
									int undoGroup = Undo.GetCurrentGroup();

									mousePosition -= new Vector2(16f, 16f);

									StateLinkRerouteNode newStateLinkNode = _GraphEditor.CreateStateLinkRerouteNode(mousePosition, lineColor);

									Undo.RecordObject(behaviour, "Link State");

									stateLink.stateID = newStateLinkNode.nodeID;
									this.bezier = bezier;

									Undo.CollapseUndoOperations(undoGroup);

									EditorUtility.SetDirty(behaviour);
								});

								menu.AddSeparator("");

								menu.AddItem(EditorContents.nodeListSelection, false, () =>
								{
									StateLinkGUI currentStateLinkGUI = this;
									StateBehaviour currentBehaviour = behaviour;

									StateLinkSelectorWindow.instance.Open(_GraphEditor, new Rect(screenMousePosition, Vector2.zero), currentStateLinkGUI.stateLink.stateID,
										(targetNodeEditor) =>
										{
											Undo.RecordObject(currentBehaviour, "Link State");

											currentStateLinkGUI.stateLink.stateID = targetNodeEditor.nodeID;
											currentStateLinkGUI.bezier = draggingBezier;

											EditorUtility.SetDirty(currentBehaviour);

													//graphEditor.BeginFrameSelected(targetNodeEditor.node);
												}
									);
								});

								if (stateLink.stateID != 0)
								{
									menu.AddSeparator("");
									menu.AddItem(EditorContents.disconnect, false, () =>
									{
										Undo.RecordObject(behaviour, "Disconect StateLink");

										stateLink.stateID = 0;

										EditorUtility.SetDirty(behaviour);
									});
								}
								menu.ShowAsContext();
							}
							else if (_DragTargetNode != targetNode)
							{
								Undo.RecordObject(behaviour, "Link State");

								stateLink.stateID = _DragTargetNode.nodeID;
								this.bezier = draggingBezier;

								EditorUtility.SetDirty(behaviour);
							}

							if (_GraphEditor != null)
							{
								_GraphEditor.EndDragStateBranch();
							}

							_DragTargetNode = null;
						}

						currentEvent.Use();
					}
					break;
				case EventType.KeyDown:
					if (GUIUtility.hotControl == controlID && currentEvent.keyCode == KeyCode.Escape)
					{
						GUIUtility.hotControl = 0;
						if (_GraphEditor != null)
						{
							_GraphEditor.EndDragStateBranch();
						}

						_DragTargetNode = null;

						currentEvent.Use();
					}
					break;
				case EventType.Repaint:
					if (isActive)
					{
						if (_GraphEditor != null)
						{
							_GraphEditor.DragStateBranchBezie(draggingBezier);
						}
					}
					else if (targetNode != null)
					{
						if (this.bezier != bezier)
						{
							this.bezier = bezier;
							if (_GraphEditor != null)
							{
								_GraphEditor.Repaint();
							}
						}
					}

					bool isConnected = targetNode != null;

					bool on = isActive || isConnected;

					Color slotColor = isActive ? Color.white : new Color(lineColor.r, lineColor.g, lineColor.b);

					Color slotBackgroundColor = EditorGUITools.GetSlotBackgroundColor(slotColor, isActive, on);

					Color backgroundColor = GUI.backgroundColor;
					GUI.backgroundColor = slotBackgroundColor;
					style.Draw(position, label, controlID, on);
					GUI.backgroundColor = backgroundColor;

					if (isConnected)
					{
						EditorGUITools.DrawLines(Styles.outlineConnectionTexture, lineColor, 8.0f, boxRect.center, bezierStartPosition);
					}

					if (isConnected || !isActive)
					{
						GUI.backgroundColor = new Color(lineColor.r, lineColor.g, lineColor.b);
						stateLinkPinStyle.Draw(boxRect, GUIContent.none, controlID, on);
						GUI.backgroundColor = backgroundColor;
					}

					if (isActive)
					{
						EditorGUITools.DrawLines(Styles.outlineConnectionTexture, NodeGraphEditor.dragBezierColor, 8.0f, draggingBoxRect.center, draggingBezierStartPosition);

						GUI.backgroundColor = NodeGraphEditor.dragBezierColor;
						draggingStateLinkPinStyle.Draw(draggingBoxRect, GUIContent.none, controlID, on);
						GUI.backgroundColor = backgroundColor;
					}
					break;
			}

			if (EditorGUI.DropdownButton(settingRect, settingContent, FocusType.Passive, settingStyle))
			{
				OpenSettingsWindow(settingRect, true);
			}
		}

		public bool RerouteSlotField(Rect position, Vector2 direction)
		{
			StateLink stateLink = this.stateLink;
			StateMachineGraphEditor graphEditor = _GraphEditor;
			Node node = nodeEditor.node;
			StateLinkRerouteNode stateLinkRerouteNode = node as StateLinkRerouteNode;

			GUIStyle style = Styles.reroutePin;
			Vector2 size = style.CalcSize(GUIContent.none);

			Rect pinPos = new Rect();
			pinPos.min = position.center - size * 0.5f;
			pinPos.max = position.center + size * 0.5f;

			int controlID = EditorGUIUtility.GetControlID(s_StateLinkHash, FocusType.Passive);

			Event currentEvent = Event.current;

			EventType eventType = currentEvent.GetTypeForControl(controlID);

			Node targetNode = graphEditor.nodeGraph.GetNodeFromID(stateLink.stateID);

			bool isActive = GUIUtility.hotControl == controlID;

			Vector2 nowPos = currentEvent.mousePosition;

			Bezier2D bezier = new Bezier2D();
			if (targetNode != null)
			{
				bool isRight = false;
				bezier = GetTargetBezier(node, targetNode, pinPos.center, pinPos.center, ref isRight);
			}
			else
			{
				bezier.startPosition = pinPos.center;
			}

			Bezier2D draggingBezier = new Bezier2D();
			if (isActive)
			{
				if (_DragTargetNode != null)
				{
					bool isRight = false;
					draggingBezier = StateLinkGUI.GetTargetBezier(node, _DragTargetNode, pinPos.center, pinPos.center, ref isRight);
				}
				else
				{
					draggingBezier.startPosition = pinPos.center;
					draggingBezier.startControl = draggingBezier.startPosition + stateLinkRerouteNode.direction * EditorGUITools.kBezierTangent;
					draggingBezier.endPosition = nowPos;
					draggingBezier.endControl = draggingBezier.startControl;
				}
			}

			Vector2 statePosition = new Vector2(node.position.x, node.position.y);

			bezier.startPosition += statePosition;
			bezier.startControl += statePosition;
			bezier.endPosition += statePosition;
			bezier.endControl += statePosition;

			draggingBezier.startPosition += statePosition;
			draggingBezier.startControl += statePosition;
			draggingBezier.endPosition += statePosition;
			draggingBezier.endControl += statePosition;

			Color lineColor = stateLink.lineColorChanged ? stateLink.lineColor : Color.white;

			bool on = isActive || targetNode != null;

			switch (eventType)
			{
				case EventType.MouseDown:
					if (pinPos.Contains(nowPos))
					{
						if (currentEvent.button == 0)
						{
							GUIUtility.hotControl = GUIUtility.keyboardControl = controlID;

							_DragTargetNode = null;

							if (graphEditor != null)
							{
								graphEditor.BeginDragStateBranch(node.nodeID);
								graphEditor.DragStateBranchBezie(bezier);
							}

							currentEvent.Use();
						}
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID && currentEvent.button == 0)
					{
						DragAndDrop.PrepareStartDrag();

						Node hoverNode = graphEditor.GetTargetNodeFromPosition(nowPos + statePosition, node);

						if (hoverNode != null)
						{
							if (graphEditor != null)
							{
								graphEditor.DragStateBranchHoverStateID(hoverNode.nodeID);
							}

							_DragTargetNode = hoverNode;
						}
						else
						{
							if (graphEditor != null)
							{
								graphEditor.DragStateBranchHoverStateID(0);
							}
							_DragTargetNode = null;
						}

						currentEvent.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						if (currentEvent.button == 0)
						{
							GUI.UnfocusWindow();

							GUIUtility.hotControl = 0;

							if (_DragTargetNode == null)
							{
								GenericMenu menu = new GenericMenu();

								Vector2 currentMousePosition = currentEvent.mousePosition;
								Vector2 mousePosition = graphEditor.hostWindow.UnclipToGraph(currentMousePosition);
								Vector2 screenMousePosition = EditorGUIUtility.GUIToScreenPoint(currentMousePosition);

								menu.AddItem(EditorContents.createState, false, () =>
								{
									Undo.IncrementCurrentGroup();
									int undoGroup = Undo.GetCurrentGroup();

									mousePosition -= new Vector2(8f, 12f);

									State newState = graphEditor.CreateState(mousePosition, false);

									Undo.RecordObject(graphEditor.nodeGraph, "Link State");

									stateLink.stateID = newState.nodeID;
									this.bezier = bezier;

									Undo.CollapseUndoOperations(undoGroup);

									EditorUtility.SetDirty(graphEditor.nodeGraph);
								});

								menu.AddItem(EditorContents.reroute, false, () =>
								{
									Undo.IncrementCurrentGroup();
									int undoGroup = Undo.GetCurrentGroup();

									mousePosition -= new Vector2(16f, 16f);

									StateLinkRerouteNode newStateLinkNode = graphEditor.CreateStateLinkRerouteNode(mousePosition, lineColor);

									Undo.RecordObject(graphEditor.nodeGraph, "Link State");

									stateLink.stateID = newStateLinkNode.nodeID;
									this.bezier = bezier;

									Undo.CollapseUndoOperations(undoGroup);

									EditorUtility.SetDirty(graphEditor.nodeGraph);
								});

								menu.AddSeparator("");

								menu.AddItem(EditorContents.nodeListSelection, false, () =>
								{
									StateLink currentStateLink = stateLink;
									NodeGraph currentGraph = graphEditor.nodeGraph;

									StateLinkSelectorWindow.instance.Open(graphEditor, new Rect(screenMousePosition, Vector2.zero), currentStateLink.stateID,
										(targetNodeEditor) =>
										{
											Undo.RecordObject(currentGraph, "Link State");

											currentStateLink.stateID = targetNodeEditor.nodeID;
											this.bezier = draggingBezier;

											EditorUtility.SetDirty(currentGraph);

											//graphEditor.BeginFrameSelected(targetNodeEditor.node);
										}
									);
								});

								if (stateLink.stateID != 0)
								{
									menu.AddSeparator("");
									menu.AddItem(EditorContents.disconnect, false, () =>
									{
										Undo.RecordObject(graphEditor.nodeGraph, "Disconect StateLink");

										stateLink.stateID = 0;

										EditorUtility.SetDirty(graphEditor.nodeGraph);
									});
								}
								menu.ShowAsContext();
							}
							else if (_DragTargetNode != targetNode)
							{
								Undo.RecordObject(graphEditor.nodeGraph, "Link State");

								stateLink.stateID = _DragTargetNode.nodeID;
								this.bezier = draggingBezier;

								EditorUtility.SetDirty(graphEditor.nodeGraph);
							}

							if (graphEditor != null)
							{
								graphEditor.EndDragStateBranch();
							}

							_DragTargetNode = null;
						}

						currentEvent.Use();
					}
					break;
				case EventType.KeyDown:
					if (GUIUtility.hotControl == controlID && currentEvent.keyCode == KeyCode.Escape)
					{
						GUIUtility.hotControl = 0;
						if (graphEditor != null)
						{
							graphEditor.EndDragStateBranch();
						}

						_DragTargetNode = null;

						currentEvent.Use();
					}
					break;
				case EventType.Repaint:
					Vector2 iconSize = EditorGUIUtility.GetIconSize();
					EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));

					if (isActive)
					{
						if (graphEditor != null)
						{
							graphEditor.DragStateBranchBezie(draggingBezier);
						}
					}
					else if (targetNode != null)
					{
						if (this.bezier != bezier)
						{
							this.bezier = bezier;
							graphEditor.Repaint();
						}
					}

					Styles.reroutePinFrame.Draw(pinPos, GUIContent.none, controlID, on);

					bool isDragHover = graphEditor.IsDragBranchHover(stateLinkRerouteNode);

					Color slotColor = new Color(lineColor.r, lineColor.g, lineColor.b);
					Color slotPinBackgroundColor = (isActive || isDragHover) ? NodeGraphEditor.dragBezierColor : slotColor;

					Color backgroundColor = GUI.color;
					GUI.color = slotPinBackgroundColor;

					if (on)
					{
						Styles.reroutePin.Draw(pinPos, GUIContent.none, controlID, on);
					}
					else
					{
						Matrix4x4 matrix = GUI.matrix;

						float angle = Vector2.SignedAngle(Vector2.right, direction);

						GUIUtility.RotateAroundPivot(angle, pinPos.center);

						GUI.DrawTexture(pinPos, on? Styles.reroutePin.onNormal.background : Icons.stateLinkRerouteLargetPinNormal, ScaleMode.ScaleToFit);
					
						GUI.matrix = matrix;
					}

					GUI.color = backgroundColor;

					EditorGUIUtility.SetIconSize(iconSize);
					break;
			}

			return on;
		}

		public void DrawBranchStateLink()
		{
			using (new ProfilerScope("DrawBranchStateLink"))
			{
				StateLink stateLink = this.stateLink;
				
				ArborFSMInternal stateMachine = _GraphEditor.stateMachine;

				Node node = nodeEditor.node;

				if (stateLink.stateID != 0)
				{
					bool editable = _GraphEditor.editable;

					Bezier2D bezier = this.bezier;

					Node targetNode = stateMachine.GetNodeFromID(stateLink.stateID);

					int controlID = EditorGUIUtility.GetControlID(s_DrawStateLinkBranchHash, FocusType.Passive);

					Event currentEvent = Event.current;

					EventType eventType = currentEvent.GetTypeForControl(controlID);

					bool isHover = _GraphEditor.IsHoverStateLink(this);

					switch (eventType)
					{
						case EventType.MouseDown:
							if (currentEvent.button == 1 || Application.platform == RuntimePlatform.OSXEditor && currentEvent.control)
							{
								if (isHover)
								{
									GenericMenu menu = new GenericMenu();

									State prevState = node as State;
									State nextState = stateMachine.GetState(stateLink);

									if (prevState != null)
									{
										menu.AddItem(EditorGUITools.GetTextContent(Localization.GetWord("Go to Previous State") + " : " + prevState.name), false, () =>
										{
											_GraphEditor.BeginFrameSelected(prevState);
										});
									}
									else
									{
										StateLinkRerouteNode rerouteNode = node as StateLinkRerouteNode;
										if (rerouteNode != null)
										{
											List<State> parentStates = _GraphEditor.GetParentStates(rerouteNode);
											parentStates.Sort((a, b) =>
											{
												return a.position.y.CompareTo(b.position.y);
											});
											HashSet<string> names = new HashSet<string>();
											for (int parentStateIndex = 0; parentStateIndex < parentStates.Count; parentStateIndex++)
											{
												State state = parentStates[parentStateIndex];
												State s = state;

												string stateName = s.name;
												while (names.Contains(stateName))
												{
													// dummy code 001f(US)
													stateName += '\u001f';
												}
												names.Add(stateName);

												menu.AddItem(EditorGUITools.GetTextContent(Localization.GetWord("Go to Previous State") + " : " + stateName), false, () =>
												{
													_GraphEditor.BeginFrameSelected(s);
												});
											}

											if (parentStates.Count > 1)
											{
												menu.AddSeparator("");
											}
										}
									}

									if (nextState != null)
									{
										menu.AddItem(EditorGUITools.GetTextContent(Localization.GetWord("Go to Next State") + " : " + nextState.name), false, () =>
										{
											_GraphEditor.BeginFrameSelected(nextState);
										});
									}

									menu.AddSeparator("");

									bool flag1 = false;

									if (prevState == null)
									{
										menu.AddItem(EditorContents.goToPreviousNode, false, () =>
										{
											_GraphEditor.BeginFrameSelected(node);
										});
										flag1 = true;
									}

									if (targetNode != null && targetNode != nextState)
									{
										menu.AddItem(EditorContents.goToNextNode, false, () =>
										{
											_GraphEditor.BeginFrameSelected(targetNode);
										});
										flag1 = true;
									}

									if (flag1)
									{
										menu.AddSeparator("");
									}

									Vector2 mousePosition = currentEvent.mousePosition;

									if (editable)
									{
										menu.AddItem(EditorContents.reroute, false, () =>
										{
											int stateID = stateLink.stateID;

											Undo.IncrementCurrentGroup();
											int undoGroup = Undo.GetCurrentGroup();

											StateLinkRerouteNode newStateLinkNode = _GraphEditor.CreateStateLinkRerouteNode(EditorGUITools.SnapToGrid(mousePosition - new Vector2(16f, 16f)), stateLink.lineColorChanged ? stateLink.lineColor : Color.white);

											Undo.RecordObject(stateMachine, "Reroute");

											float t = bezier.GetClosestParam(mousePosition);
											newStateLinkNode.direction = bezier.GetTangent(t);
											newStateLinkNode.link.stateID = stateID;

											if (_StateBehaviour != null)
											{
												Undo.RecordObject(_StateBehaviour, "Reroute");
											}

											stateLink.stateID = newStateLinkNode.nodeID;

											Undo.CollapseUndoOperations(undoGroup);

											_GraphEditor.VisibleNode(node);

											EditorUtility.SetDirty(stateMachine);
											if (_StateBehaviour != null)
											{
												EditorUtility.SetDirty(_StateBehaviour);
											}
										});

										menu.AddItem(EditorContents.disconnect, false, () =>
										{
											if (_StateBehaviour != null)
											{
												Undo.RecordObject(_StateBehaviour, "Disconnect StateLink");
											}
											else
											{
												Undo.RecordObject(stateMachine, "Disconnect StateLink");
											}

											stateLink.stateID = 0;

											if (_StateBehaviour != null)
											{
												EditorUtility.SetDirty(_StateBehaviour);
											}
											else
											{
												EditorUtility.SetDirty(stateMachine);
											}
										});
									}
									else
									{
										menu.AddDisabledItem(EditorContents.reroute);
										menu.AddDisabledItem(EditorContents.disconnect);
									}

									menu.AddSeparator("");

									if (editable)
									{
										Rect settingRect = new Rect();
										settingRect.position = mousePosition;
										settingRect = EditorGUITools.GUIToScreenRect(settingRect);

										menu.AddItem(EditorContents.settings, false, () =>
										{
											OpenSettingsWindow(GUIUtility.ScreenToGUIRect(settingRect), false);
										});
									}
									else
									{
										menu.AddDisabledItem(EditorContents.settings);
									}

									menu.ShowAsContext();
									currentEvent.Use();
								}
							}
							break;
						case EventType.MouseMove:
							{
								float distance = 0f;
								if (_GraphEditor.IsHoverBezier(currentEvent.mousePosition, bezier, true, EditorGUITools.kBranchArrowWidth, ref distance))
								{
									_GraphEditor.SetHoverStateLink(this, distance);
								}
							}
							break;
						case EventType.Repaint:
							{
								Color lineColor = Color.white;

								Color shadowColor = NodeGraphEditor.bezierShadowColor;
								float width = 5;
								if (Application.isPlaying)
								{
									int index = stateMachine.IndexOfStateLinkHistory(stateLink);
									if (index != -1)
									{
										float t = (float)index / 4.0f;

										shadowColor = Color.Lerp(new Color(0.0f, 0.5f, 0.5f, 1.0f), Color.black, t);
										lineColor *= Color.Lerp(Color.white, Color.gray, t);
										width = Mathf.Lerp(15, 5, t);
									}
									else
									{
										if (stateMachine.playState == PlayState.InactivePausing && stateMachine.reservedStateLink == stateLink)
										{
											lineColor *= StateMachineGraphEditor.reservedColor;
										}
										else
										{
											lineColor *= Color.gray;
										}
									}
								}

								if (stateLink.lineColorChanged)
								{
									lineColor *= stateLink.lineColor;
								}

								using (new ProfilerScope("DrawBezierArrow"))
								{
									EditorGUITools.DrawBranch(bezier, lineColor, shadowColor, width, targetNode is State, isHover);
								}
							}
							break;
					}
				}
			}
		}

		public void DrawMinimapBranch(MinimapViewport minimapViewport)
		{
			StateLink stateLink = this.stateLink;
			ArborFSMInternal stateMachine = _GraphEditor.stateMachine;
			Node targetNode = stateMachine.GetNodeFromID(stateLink.stateID);
			if (targetNode == null)
			{
				return;
			}

			Color lineColor = Color.white;

			Bezier2D bezier = minimapViewport.GraphToMinimapBezier(this.bezier);

			if (stateLink.lineColorChanged)
			{
				lineColor = stateLink.lineColor;
			}

			EditorGUITools.DrawBranch(bezier, lineColor, false, NodeGraphEditor.bezierShadowColor, 4f, targetNode is State, 5f, false);
		}

		public void DrawTransitionCount()
		{
			StateLink stateLink = this.stateLink;
			if (stateLink.stateID != 0)
			{
				Bezier2D bezier = this.bezier;

				Vector2 startPosition = bezier.startPosition;
				Vector2 startControl = bezier.startControl;
				Vector2 endPosition = bezier.endPosition;
				Vector2 endControl = bezier.endControl;

				Vector2 v = (endPosition - endControl).normalized * EditorGUITools.kBranchArrowWidth;

				Vector2 pos = Bezier2D.GetPoint(startPosition, startControl, endPosition - v, endControl - v, 0.5f);

				GUIStyle style = Styles.countBadge;
				GUIContent content = new GUIContent(stateLink.transitionCount.ToString());
				Vector2 contentSize = style.CalcSize(content);

				Rect rect = new Rect(pos.x - contentSize.x / 2.0f, pos.y - contentSize.y / 2.0f, contentSize.x, contentSize.y);

				Color lineColor = Color.white;

				int index = _GraphEditor.stateMachine.IndexOfStateLinkHistory(stateLink);
				if (index != -1)
				{
					float t = (float)index / 4.0f;

					lineColor *= Color.Lerp(Color.white, Color.gray, t);
				}
				else
				{
					lineColor *= Color.gray;
				}

				lineColor = EditorGUITools.GetColorOnGUI(lineColor);

				Color savedColor = GUI.color;
				GUI.color = lineColor;

				EditorGUI.LabelField(rect, content, style);

				GUI.color = savedColor;
			}
		}
	}
}