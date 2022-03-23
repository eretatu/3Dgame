//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	using Arbor;

	[CustomNodeEditor(typeof(DataBranchRerouteNode))]
	internal sealed class DataBranchRerouteNodeEditor : NodeEditor
	{
		public DataBranchRerouteNode dataBranchRerouteNode
		{
			get
			{
				return node as DataBranchRerouteNode;
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
			return Localization.GetWord("DataBranchRerouteNode");
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

		private static readonly int s_ReroutePinHash = "s_ReroutePinHash".GetHashCode();
		private static readonly int s_DirectionFieldHash = "s_DirectionFieldHash".GetHashCode();
		private Vector2 _BeginDirection;
		private bool _IsUpdateBezier = false;

		void DirectionField(Rect position, Color pinColor, bool isSelection)
		{
			int controlID = EditorGUIUtility.GetControlID(s_DirectionFieldHash, FocusType.Passive);

			Event currentEvent = Event.current;

			EventType eventType = currentEvent.GetTypeForControl(controlID);

			Vector2 center = position.center;

			Vector2 direction = dataBranchRerouteNode.direction;

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
						GUIUtility.hotControl = controlID;
						graphEditor.BeginDisableContextClick();
						_BeginDirection = dataBranchRerouteNode.direction;
						currentEvent.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
					{
						Undo.RecordObject(node.nodeGraph, "Change Reroute Direction");
						dataBranchRerouteNode.direction = (currentEvent.mousePosition - position.center).normalized;
						EditorUtility.SetDirty(node.nodeGraph);
						currentEvent.Use();

						_IsUpdateBezier = true;
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						if (currentEvent.button == 0)
						{
							graphEditor.EndDisableContextClick();
							GUIUtility.hotControl = 0;
						}
						currentEvent.Use();
					}
					break;
				case EventType.KeyDown:
					if (GUIUtility.hotControl == controlID && currentEvent.keyCode == KeyCode.Escape)
					{
						Undo.RecordObject(node.nodeGraph, "Change Reroute Direction");
						dataBranchRerouteNode.direction = _BeginDirection;
						EditorUtility.SetDirty(node.nodeGraph);

						GUIUtility.hotControl = 0;
						currentEvent.Use();

						_IsUpdateBezier = true;
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

		void UpdateBezier(Rect pinPos)
		{
			bool isRepaint = false;

			dataBranchRerouteNode.link.position = NodeToGraphRect(pinPos);

			Vector2 pinPosition = NodeToGraphPoint(pinPos.center);
			Vector2 startPinPosition = pinPosition + dataBranchRerouteNode.direction * 8f;
			Vector2 endPinPosition = pinPosition - dataBranchRerouteNode.direction * 8f;

			IInputSlot inputSlot = dataBranchRerouteNode.link.inputSlot;

			DataBranch inputBranch = inputSlot.GetBranch();
			if (inputBranch != null)
			{
				if (inputBranch.lineBezier.endPosition != endPinPosition)
				{
					inputBranch.lineBezier.endPosition = endPinPosition;
					isRepaint = true;
				}
				Vector2 endControl = inputBranch.lineBezier.endPosition - dataBranchRerouteNode.direction * EditorGUITools.kBezierTangent;
				if (inputBranch.lineBezier.endControl != endControl)
				{
					inputBranch.lineBezier.endControl = endControl;
					isRepaint = true;
				}
			}

			IOutputSlot outputSlot = dataBranchRerouteNode.link.outputSlot;
			int outBranchCount = outputSlot.branchCount;
			for (int i = 0; i < outBranchCount; i++)
			{
				DataBranch outputBranch = outputSlot.GetBranch(i);
				if (outputBranch != null)
				{
					if (outputBranch.lineBezier.startPosition != startPinPosition)
					{
						outputBranch.lineBezier.startPosition = startPinPosition;
						isRepaint = true;
					}
					Vector2 startControl = outputBranch.lineBezier.startPosition + dataBranchRerouteNode.direction * EditorGUITools.kBezierTangent;
					if (outputBranch.lineBezier.startControl != startControl)
					{
						outputBranch.lineBezier.startControl = startControl;
						isRepaint = true;
					}
				}
			}

			if (isRepaint)
			{
				Repaint();
			}
		}

		bool ReroutePinGUI(Rect position, out Rect pinPos)
		{
			RerouteSlot slot = dataBranchRerouteNode.link;
			System.Type dataType = slot.connectableType;

			GUIStyle pinStyle = Styles.GetDataReroutePin(dataType);
			Vector2 size = pinStyle.CalcSize(GUIContent.none);

			pinPos = new Rect();
			pinPos.min = position.center - size * 0.5f;
			pinPos.max = position.center + size * 0.5f;

			int controlID = EditorGUIUtility.GetControlID(s_ReroutePinHash, FocusType.Passive);

			Event currentEvent = Event.current;

			Bezier2D dragBezier = new Bezier2D();
			dragBezier.startPosition = NodeToGraphPoint(pinPos.center) + dataBranchRerouteNode.direction * 8f;
			dragBezier.startControl = dragBezier.startPosition + dataBranchRerouteNode.direction * EditorGUITools.kBezierTangent;

			dragBezier.endPosition = graphEditor.hostWindow.UnclipToGraph(currentEvent.mousePosition);
			dragBezier.endControl = dragBezier.endPosition - EditorGUITools.kBezierTangentOffset;
			DataSlot hoverSlot = DataSlotGUI._HoverSlot;
			Node hoverNode = DataSlotGUI._HoverNode;
			Object hoverObj = DataSlotGUI._HoverObj;
			if (hoverSlot != null)
			{
				DataBranchRerouteNode hoverRerouteNode = hoverNode as DataBranchRerouteNode;
				if (hoverRerouteNode != null)
				{
					dragBezier.endPosition = hoverSlot.position.center;
					dragBezier.endControl = dragBezier.endPosition - hoverRerouteNode.direction * EditorGUITools.kBezierTangent;
				}
				else
				{
					dragBezier.endPosition = new Vector2(hoverSlot.position.x + 8, hoverSlot.position.center.y);
					dragBezier.endControl = dragBezier.endPosition - EditorGUITools.kBezierTangentOffset;
				}
			}

			bool dragging = (GUIUtility.hotControl == controlID && currentEvent.button == 0);

			bool on = dragging || slot.outputBranchIDs.Count > 0;

			EventType eventType = currentEvent.GetTypeForControl(controlID);
			switch (eventType)
			{
				case EventType.MouseDown:
					if (pinPos.Contains(currentEvent.mousePosition) && currentEvent.button == 0)
					{
						GUIUtility.hotControl = controlID;

						DataSlotGUI.BeginDragSlot(node, slot, null);

						graphEditor.BeginDragDataBranch(node.nodeID);
						graphEditor.DragDataBranchBezier(dragBezier.startPosition, dragBezier.startControl, dragBezier.endPosition, dragBezier.endControl);

						currentEvent.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
					{
						DragAndDrop.PrepareStartDrag();

						DataSlotGUI.UpdateHoverSlot(node.nodeGraph, node, null, slot, graphEditor.hostWindow.UnclipToGraph(currentEvent.mousePosition));

						graphEditor.DragDataBranchBezier(dragBezier.startPosition, dragBezier.startControl, dragBezier.endPosition, dragBezier.endControl);

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

							DataSlotGUI.EndDragSlot();
							graphEditor.EndDragDataBranch();

							DataBranch branch = null;

							if (hoverSlot == null)
							{
								GenericMenu menu = new GenericMenu();

								NodeGraph nodeGraph = node.nodeGraph;

								Vector2 mousePosition = graphEditor.hostWindow.UnclipToGraph(currentEvent.mousePosition);

								DataBranch currentBranch = branch;

								Bezier2D lineBezier = new Bezier2D(dragBezier);

								menu.AddItem(EditorContents.reroute, false, () =>
									{
										if (currentBranch != null)
										{
											nodeGraph.DeleteDataBranch(currentBranch);
											currentBranch = null;
										}

										Undo.IncrementCurrentGroup();
										int undoGroup = Undo.GetCurrentGroup();

										mousePosition -= new Vector2(16f, 16f);

										DataBranchRerouteNode newRerouteNode = nodeGraph.CreateDataBranchRerouteNode(EditorGUITools.SnapToGrid(mousePosition), slot.dataType);

										Undo.RecordObject(nodeGraph, "Reroute");

										RerouteSlot rerouteSlot = newRerouteNode.link;

										currentBranch = nodeGraph.ConnectDataBranch(newRerouteNode.nodeID, null, rerouteSlot, node.nodeID, null, slot);
										if (currentBranch != null)
										{
											currentBranch.enabled = true;
											currentBranch.lineBezier = lineBezier;
										}

										Undo.CollapseUndoOperations(undoGroup);

										EditorUtility.SetDirty(nodeGraph);
									});

								menu.ShowAsContext();
							}
							else if (hoverSlot != null)
							{
								NodeGraph nodeGraph = node.nodeGraph;

								InputSlotBase inputSlot = hoverSlot as InputSlotBase;

								if (inputSlot != null)
								{
									branch = nodeGraph.GetDataBranchFromID(inputSlot.branchID);
								}
								else
								{
									RerouteSlot rerouteSlot = hoverSlot as RerouteSlot;
									if (rerouteSlot != null)
									{
										branch = nodeGraph.GetDataBranchFromID(rerouteSlot.inputBranchID);
									}
								}

								if (branch != null)
								{
									nodeGraph.DeleteDataBranch(branch);
									branch = null;
								}

								branch = nodeGraph.ConnectDataBranch(hoverNode.nodeID, hoverObj, hoverSlot, node.nodeID, null, slot);
								if (branch != null)
								{
									branch.lineBezier = dragBezier;
								}
							}

							if (branch != null)
							{
								branch.enabled = true;
							}

							DataSlotGUI.ClearHoverSlot();
						}

						currentEvent.Use();
					}
					break;
				case EventType.KeyDown:
					if (GUIUtility.hotControl == controlID && currentEvent.keyCode == KeyCode.Escape)
					{
						GUIUtility.hotControl = 0;

						DataSlotGUI.EndDragSlot();
						graphEditor.EndDragDataBranch();

						DataSlotGUI.ClearHoverSlot();
					}
					break;
				case EventType.Repaint:
					_IsUpdateBezier = true;

					bool isDragConnectable = DataSlotGUI.IsDragSlotConnectable(node, slot, null);
					bool isActive = GUIUtility.hotControl == controlID;
					bool isDragHover = slot == DataSlotGUI._HoverSlot;

					Color slotColor = EditorGUITools.GetTypeColor(dataType);
					Color slotPinBackgroundColor = (isActive || isDragHover || isDragConnectable) ? NodeGraphEditor.dragBezierColor : slotColor;

					GUIContent label = new GUIContent("");
					label.tooltip = slot.connectableTypeName;

					Styles.reroutePinFrame.Draw(pinPos, label, controlID, on || isActive || isDragHover);

					Color backgroundColor = GUI.backgroundColor;
					GUI.backgroundColor = slotPinBackgroundColor;

					pinStyle.Draw(pinPos, GUIContent.none, controlID, on || isActive || isDragHover);

					GUI.backgroundColor = backgroundColor;
					break;
			}

			return on;
		}

		void ReroutePinGUI()
		{
			Rect position = GUILayoutUtility.GetRect(32f, 32f);

			Rect pinPos;
			bool on = ReroutePinGUI(position, out pinPos);

			System.Type dataType = dataBranchRerouteNode.link.type;
			Color lineColor = EditorGUITools.GetTypeColor(dataType);

			DirectionField(position, lineColor, isSelection || !on);

			if (_IsUpdateBezier)
			{
				UpdateBezier(pinPos);
				_IsUpdateBezier = false;
			}
		}

		protected override void OnGUI()
		{
			using (new ProfilerScope("OnGUI"))
			{
				DataSlot link = dataBranchRerouteNode.link;
				link.SetVisible();
				link.enabledGUI = true;

				ReroutePinGUI();
			}
		}

		void DeleteKeepConnection()
		{
			Undo.IncrementCurrentGroup();
			int undoGroup = Undo.GetCurrentGroup();

			NodeGraph nodeGraph = node.nodeGraph;

			DataBranch inputBranch = dataBranchRerouteNode.link.inputSlot.GetBranch();

			int outNodeID = inputBranch.outNodeID;
			Object outBehaviour = inputBranch.outBehaviour;
			DataSlot outputSlot = inputBranch.outputSlot;

			nodeGraph.DeleteDataBranch(inputBranch);

			for (int count = dataBranchRerouteNode.link.outputSlot.branchCount, i = count - 1; i >= 0; i--)
			{
				DataBranch outputBranch = dataBranchRerouteNode.link.outputSlot.GetBranch(i);

				int inNodeID = outputBranch.inNodeID;
				Object inBehaviour = outputBranch.inBehaviour;
				DataSlot inputSlot = outputBranch.inputSlot;

				nodeGraph.DeleteDataBranch(outputBranch);

				Undo.RecordObject(nodeGraph, "Delete Keep Connection");

				DataBranch currentBranch = nodeGraph.ConnectDataBranch(inNodeID, inBehaviour, inputSlot, outNodeID, outBehaviour, outputSlot);
				if (currentBranch != null)
				{
					currentBranch.lineBezier = new Bezier2D(inputBranch.lineBezier.startPosition, inputBranch.lineBezier.startControl, outputBranch.lineBezier.endPosition, outputBranch.lineBezier.endControl);
					currentBranch.enabled = true;
				}
			}

			graphEditor.DeleteNodes(new Node[] { node });

			Undo.CollapseUndoOperations(undoGroup);

			EditorUtility.SetDirty(nodeGraph);

			Repaint();
		}

		bool IsConnected()
		{
			DataBranch inputBranch = dataBranchRerouteNode.link.inputSlot.GetBranch();

			if (inputBranch == null)
			{
				return false;
			}

			int count = dataBranchRerouteNode.link.outputSlot.branchCount;
			if (count == 0)
			{
				return false;
			}

			return true;
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

		public override MinimapLayer minimapLayer
		{
			get
			{
				return MinimapLayer.None;
			}
		}
	}
}