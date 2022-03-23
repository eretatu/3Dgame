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

	public abstract class TreeNodeBaseEditor : NodeEditor
	{
		private const Styles.Color kDragColor = Styles.Color.Red;
		private const Styles.Color kCurrentColor = Styles.Color.Orange;
		private const Styles.Color kNormalColor = Styles.Color.Gray;

		public BehaviourTreeGraphEditor behaviourTreeGraphEditor
		{
			get
			{
				return graphEditor as BehaviourTreeGraphEditor;
			}
		}

		public TreeNodeBase treeNode
		{
			get
			{
				return node as TreeNodeBase;
			}
		}

		private static readonly int s_LinkSlotHash = "s_LinkSlotHash".GetHashCode();

		public Rect parentLinkSlotPosition
		{
			get;
			private set;
		}

		public Rect childLinkSlotPosition
		{
			get;
			private set;
		}

		public override bool IsActive()
		{
			return treeNode.isActive;
		}

		protected virtual Styles.Color GetNormalStyleColor()
		{
			return kNormalColor;
		}

		public override Styles.Color GetStyleColor()
		{
			BehaviourTreeGraphEditor behaviourTreeGraphEditor = graphEditor as BehaviourTreeGraphEditor;
			if (behaviourTreeGraphEditor != null && behaviourTreeGraphEditor.IsDragBranchHover(node))
			{
				return kDragColor;
			}
			else if (IsActive())
			{
				return kCurrentColor;
			}

			return GetNormalStyleColor();
		}

		void OnCreateComposite(MousePosition mousePosition, CompositeBehaviourMenuWindow.OnSelectCallback onSelect)
		{
			Vector2 guiPoint = NodeToGraphPoint(mousePosition.guiPoint);

			BehaviourTreeGraphEditor graphEditor = this.graphEditor as BehaviourTreeGraphEditor;

			Rect buttonRect = new Rect(mousePosition.screenPoint, Vector2.zero);

			graphEditor._DragBranchEnable = true;

			CompositeBehaviourMenuWindow.instance.Init(graphEditor, guiPoint, buttonRect, onSelect, null, () =>
				   {
					   graphEditor.EndDragBranch();
				   }
			);
		}

		void OnCreateAction(MousePosition mousePosition, ActionBehaviourMenuWindow.OnSelectCallback onSelect)
		{
			Vector2 guiPoint = NodeToGraphPoint(mousePosition.guiPoint);

			BehaviourTreeGraphEditor graphEditor = this.graphEditor as BehaviourTreeGraphEditor;

			Rect buttonRect = new Rect(mousePosition.screenPoint, Vector2.zero);

			graphEditor._DragBranchEnable = true;

			ActionBehaviourMenuWindow.instance.Init(graphEditor, guiPoint, buttonRect, onSelect, null, () =>
				   {
					   graphEditor.EndDragBranch();
				   }
			);
		}

		//public static readonly Vector2 kBezierTangentOffset = EditorGUITools.kBezierTangentOffset;
		public static readonly Vector2 kBezierTangentOffset = new Vector2(0f, EditorGUITools.kBezierTangent);

		static Vector2 GetNodeLinkPinPos(Rect position)
		{
			return position.center;
		}

		private const float kDefaultNodeWidth = 300f;
		private const float kDefaultNodeHeight = 93f; // Approximate height

		void DrawLinkSlot(Rect position, int controlID, bool on, bool isParentSlot)
		{
			GUIStyle pinStyle = Styles.nodeLinkPin;
			Vector2 pinSize = pinStyle.CalcSize(GUIContent.none);
			Rect pinRect = new Rect(position.center - pinSize * 0.5f, pinSize);

			BehaviourTreeGraphEditor graphEditor = this.graphEditor as BehaviourTreeGraphEditor;

			bool isDragConnectable = graphEditor.IsDragBranchConnectable(treeNode, isParentSlot);
			bool isDragHover = graphEditor.IsDragBranchHover(node) && (graphEditor.IsDragParentSlot() != isParentSlot);

			Event currentEvent = Event.current;
			bool isActive = GUIUtility.hotControl == controlID;
			bool isHover = position.Contains(currentEvent.mousePosition);

			bool hasKeyboardFocus = false;

			GUIStyle buttonStyle = isActive ? Styles.nodeLinkSlotActive : Styles.nodeLinkSlot;

			Color slotColor = isActive ? NodeGraphEditor.dragBezierColor : Color.white;
			Color slotBackgoundColor = EditorGUITools.GetSlotBackgroundColor(slotColor, isActive, on);

			Color backgroundColor = GUI.backgroundColor;

			GUI.backgroundColor = slotBackgoundColor;
			buttonStyle.Draw(position, GUIContent.none, isHover, isActive, on, hasKeyboardFocus);

			GUI.backgroundColor = slotColor;

			pinStyle.Draw(pinRect, GUIContent.none, isHover, isActive, on || isActive || isDragHover, hasKeyboardFocus);
			GUI.backgroundColor = backgroundColor;

			if (isDragConnectable)
			{
				graphEditor.ShowLinkSlotHightlight(NodeEditor.currentEditor.NodeToGraphRect(position), controlID, Styles.highlight);
			}
		}

		protected void ParentLinkSlot(NodeLinkSlot linkSlot)
		{
			BehaviourTreeGraphEditor graphEditor = this.graphEditor as BehaviourTreeGraphEditor;
			TreeNodeBase treeNode = node as TreeNodeBase;

			EditorGUI.BeginDisabledGroup(!graphEditor.editable);

			Rect position = GUILayoutUtility.GetRect(100, 16);

			BehaviourTreeInternal behaviourTree = graphEditor.nodeGraph as BehaviourTreeInternal;

			NodeBranch branch = behaviourTree.nodeBranchies.GetFromID(linkSlot.branchID);
			bool hasBranch = branch != null;

			int controlID = GUIUtility.GetControlID(s_LinkSlotHash, FocusType.Passive, position);

			Event currentEvent = Event.current;
			EventType eventType = currentEvent.GetTypeForControl(controlID);

			bool isDraggable = !(hasBranch && branch.isActive);
			bool dragging = (GUIUtility.hotControl == controlID && currentEvent.button == 0);

			Vector2 nowPos = currentEvent.mousePosition;

			Vector2 pinPos = GetNodeLinkPinPos(position);

			Bezier2D bezier = new Bezier2D();
			if (dragging)
			{
				if (isDraggable)
				{
					Vector2 startPos = NodeToGraphPoint(nowPos);
					Vector2 endPos = NodeToGraphPoint(pinPos);

					TreeNodeBase hoverNode = behaviourTree.GetNodeFromID(graphEditor.GetDragBranchHoverID()) as TreeNodeBase;
					if (hoverNode != null)
					{
						TreeNodeBaseEditor nodeEditor = graphEditor.GetNodeEditor(hoverNode) as TreeNodeBaseEditor;
						startPos = GetNodeLinkPinPos(nodeEditor.childLinkSlotPosition);
					}

					Vector2 startControl = startPos + kBezierTangentOffset;
					Vector2 endControl = endPos - kBezierTangentOffset;

					bezier = new Bezier2D(startPos, startControl, endPos, endControl);
				}
				else
				{
					graphEditor.EndDragBranch();
					GUIUtility.hotControl = 0;
					dragging = false;
				}
			}
			else
			{
				bezier.startPosition = NodeToGraphPoint(pinPos);
			}

			if (eventType != EventType.Layout && eventType != EventType.Used)
			{
				Rect slotPosition = NodeToGraphRect(position);

				parentLinkSlotPosition = slotPosition;

				if (branch != null)
				{
					Vector2 endPosition = NodeToGraphPoint(pinPos);
					Vector2 endControl = endPosition - kBezierTangentOffset;

					if (branch.bezier.SetEndPoint(endPosition, endControl))
					{
						Repaint();
					}
				}
			}

			switch (eventType)
			{
				case EventType.MouseDown:
					if (currentEvent.button == 0 && position.Contains(currentEvent.mousePosition))
					{
						if (isDraggable)
						{
							GUIUtility.hotControl = GUIUtility.keyboardControl = controlID;

							if (graphEditor != null)
							{
								graphEditor.BeginDragBranch(treeNode.nodeID, true);
								graphEditor.DragBranchBezie(bezier);
							}
						}

						currentEvent.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID && currentEvent.button == 0)
					{
						DragAndDrop.PrepareStartDrag();

						Node parentNode = graphEditor.GetHoverParentNode(treeNode, NodeToGraphPoint(nowPos));

						if (parentNode != null)
						{
							if (graphEditor != null)
							{
								graphEditor.DragBranchHoverID(parentNode.nodeID);
							}
						}
						else
						{
							if (graphEditor != null)
							{
								graphEditor.DragBranchHoverID(0);
							}
						}

						currentEvent.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						if (currentEvent.button == 0)
						{
							if (graphEditor != null)
							{
								int hoverID = graphEditor.GetDragBranchHoverID();
								TreeNodeBase parentNode = behaviourTree.GetNodeFromID(hoverID) as TreeNodeBase;

								if (behaviourTree.CheckLoop(parentNode, treeNode))
								{
									Debug.LogError("Node has become an infinite loop.");
								}
								else
								{
									if (parentNode == null)
									{
										GenericMenu menu = new GenericMenu();

										MousePosition mousePosition = new MousePosition(currentEvent.mousePosition);

										menu.AddItem(EditorContents.createComposite, false, () =>
											{
												OnCreateComposite(mousePosition, (pos, classType) =>
												{
													Undo.IncrementCurrentGroup();
													int undoGroup = Undo.GetCurrentGroup();

													pos -= new Vector2(kDefaultNodeWidth * 0.5f, kDefaultNodeHeight);

													CompositeNode compositeNode = graphEditor.CreateComposite(pos, classType);

													if (branch != null)
													{
														behaviourTree.DisconnectBranch(branch);
														branch = null;
													}

													branch = behaviourTree.ConnectBranch(compositeNode, treeNode);

													if (branch != null)
													{
														branch.bezier = bezier;
														Repaint();
													}

													behaviourTree.CalculatePriority();

													Undo.CollapseUndoOperations(undoGroup);
												});
											});

										if (branch != null)
										{
											menu.AddSeparator("");
											menu.AddItem(EditorContents.disconnect, false, () =>
												{
													Undo.IncrementCurrentGroup();
													int undoGroup = Undo.GetCurrentGroup();

													behaviourTree.DisconnectBranch(branch);
													behaviourTree.CalculatePriority();

													Undo.CollapseUndoOperations(undoGroup);
												});
										}

										menu.ShowAsContext();
									}
									else
									{
										if (branch != null && branch.parentNodeID != hoverID)
										{
											behaviourTree.DisconnectBranch(branch);
											branch = null;
										}

										if (branch == null && parentNode != null)
										{
											branch = behaviourTree.ConnectBranch(parentNode, treeNode);

											if (branch != null)
											{
												branch.bezier = bezier;
												Repaint();
											}
										}

										behaviourTree.CalculatePriority();
									}
								}

								graphEditor.EndDragBranch();
							}

							GUIUtility.hotControl = 0;
							GUIUtility.keyboardControl = 0;
						}

						currentEvent.Use();
					}
					break;
				case EventType.KeyDown:
					if (GUIUtility.hotControl == controlID && currentEvent.keyCode == KeyCode.Escape)
					{
						if (graphEditor != null)
						{
							graphEditor.EndDragBranch();
						}

						GUIUtility.hotControl = 0;
						GUIUtility.keyboardControl = 0;
						currentEvent.Use();
					}
					break;
				case EventType.ContextClick:
					if (position.Contains(currentEvent.mousePosition))
					{
						GenericMenu menu = new GenericMenu();

						if (branch != null)
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
					break;
				case EventType.Repaint:
					if (dragging)
					{
						if (graphEditor != null)
						{
							graphEditor.DragBranchBezie(bezier);
						}
					}

					DrawLinkSlot(position, controlID, hasBranch, true);
					break;
			}

			EditorGUI.EndDisabledGroup();
		}

		protected void ChildLinkSlot(NodeLinkSlot linkSlot)
		{
			BehaviourTreeGraphEditor graphEditor = this.graphEditor as BehaviourTreeGraphEditor;
			TreeNodeBase treeNode = node as TreeNodeBase;

			EditorGUI.BeginDisabledGroup(!graphEditor.editable);

			Rect position = GUILayoutUtility.GetRect(100, 16);

			BehaviourTreeInternal behaviourTree = graphEditor.nodeGraph as BehaviourTreeInternal;

			NodeBranch branch = behaviourTree.nodeBranchies.GetFromID(linkSlot.branchID);
			bool hasBranch = branch != null;

			int controlID = GUIUtility.GetControlID(s_LinkSlotHash, FocusType.Passive, position);

			Event currentEvent = Event.current;
			EventType eventType = currentEvent.GetTypeForControl(controlID);

			Vector2 nowPos = currentEvent.mousePosition;

			Vector2 pinPos = GetNodeLinkPinPos(position);

			bool isDraggable = !(hasBranch && branch.isActive);
			bool dragging = (GUIUtility.hotControl == controlID && currentEvent.button == 0);

			Bezier2D bezier = new Bezier2D();
			if (dragging)
			{
				if (isDraggable)
				{
					Vector2 startPos = NodeToGraphPoint(pinPos);
					Vector2 endPos = NodeToGraphPoint(nowPos);

					TreeNodeBase hoverNode = behaviourTree.GetNodeFromID(graphEditor.GetDragBranchHoverID()) as TreeNodeBase;
					if (hoverNode != null)
					{
						TreeNodeBaseEditor nodeEditor = graphEditor.GetNodeEditor(hoverNode) as TreeNodeBaseEditor;
						endPos = GetNodeLinkPinPos(nodeEditor.parentLinkSlotPosition);
					}

					Vector2 startControl = startPos + kBezierTangentOffset;
					Vector2 endControl = endPos - kBezierTangentOffset;

					bezier = new Bezier2D(startPos, startControl, endPos, endControl);
				}
				else
				{
					graphEditor.EndDragBranch();
					dragging = false;
				}
			}

			if (eventType != EventType.Layout && eventType != EventType.Used)
			{
				Rect slotPosition = NodeToGraphRect(position);

				childLinkSlotPosition = slotPosition;

				if (branch != null)
				{
					Vector2 startPosition = NodeToGraphPoint(pinPos);
					Vector2 startControl = startPosition + kBezierTangentOffset;

					if (branch.bezier.SetStartPoint(startPosition, startControl))
					{
						Repaint();
					}
				}
			}

			switch (eventType)
			{
				case EventType.MouseDown:
					if (currentEvent.button == 0 && position.Contains(currentEvent.mousePosition))
					{
						if (isDraggable)
						{
							GUIUtility.hotControl = GUIUtility.keyboardControl = controlID;

							if (graphEditor != null)
							{
								graphEditor.BeginDragBranch(node.nodeID, false);
								graphEditor.DragBranchBezie(bezier);
							}
						}
						currentEvent.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
					{
						Node childNode = graphEditor.GetHoverChildNode(treeNode, NodeToGraphPoint(nowPos));

						if (childNode != null)
						{
							if (graphEditor != null)
							{
								graphEditor.DragBranchHoverID(childNode.nodeID);
							}
						}
						else
						{
							if (graphEditor != null)
							{
								graphEditor.DragBranchHoverID(0);
							}
						}

						currentEvent.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						if (currentEvent.button == 0)
						{
							if (graphEditor != null)
							{
								int hoverID = graphEditor.GetDragBranchHoverID();
								TreeNodeBase childNode = behaviourTree.GetNodeFromID(hoverID) as TreeNodeBase;

								if (behaviourTree.CheckLoop(treeNode, childNode))
								{
									Debug.LogError("Node has become an infinite loop.");
								}
								else
								{
									if (childNode == null)
									{
										GenericMenu menu = new GenericMenu();

										MousePosition mousePosition = new MousePosition(currentEvent.mousePosition);

										menu.AddItem(EditorContents.createComposite, false, () =>
											{
												OnCreateComposite(mousePosition, (pos, classType) =>
												{
													Undo.IncrementCurrentGroup();
													int undoGroup = Undo.GetCurrentGroup();

													pos -= new Vector2(kDefaultNodeWidth * 0.5f, 0f);

													CompositeNode compositeNode = graphEditor.CreateComposite(pos, classType);

													if (branch != null)
													{
														behaviourTree.DisconnectBranch(branch);
														branch = null;
													}

													branch = behaviourTree.ConnectBranch(treeNode, compositeNode);
													if (branch != null)
													{
														branch.bezier = bezier;
														Repaint();
													}
													behaviourTree.CalculatePriority();

													Undo.CollapseUndoOperations(undoGroup);
												});
											});

										menu.AddItem(EditorContents.createAction, false, () =>
											{
												OnCreateAction(mousePosition, (pos, classType) =>
												{
													Undo.IncrementCurrentGroup();
													int undoGroup = Undo.GetCurrentGroup();

													pos -= new Vector2(kDefaultNodeWidth * 0.5f, 0f);

													ActionNode actionNode = graphEditor.CreateAction(pos, classType);

													if (branch != null)
													{
														behaviourTree.DisconnectBranch(branch);
														branch = null;
													}

													branch = behaviourTree.ConnectBranch(treeNode, actionNode);
													if (branch != null)
													{
														branch.bezier = bezier;
														Repaint();
													}

													behaviourTree.CalculatePriority();

													Undo.CollapseUndoOperations(undoGroup);
												});
											});

										if (branch != null)
										{
											menu.AddSeparator("");
											menu.AddItem(EditorContents.disconnect, false, () =>
												{
													Undo.IncrementCurrentGroup();
													int undoGroup = Undo.GetCurrentGroup();

													behaviourTree.DisconnectBranch(branch);
													behaviourTree.CalculatePriority();

													Undo.CollapseUndoOperations(undoGroup);
												});
										}

										menu.ShowAsContext();
									}
									else
									{
										if (branch != null && branch.childNodeID != hoverID)
										{
											behaviourTree.DisconnectBranch(branch);
											branch = null;
										}

										if (branch == null && childNode != null)
										{
											branch = behaviourTree.ConnectBranch(treeNode, childNode);

											if (branch != null)
											{
												branch.bezier = bezier;
												Repaint();
											}
										}

										behaviourTree.CalculatePriority();
									}
								}

								graphEditor.EndDragBranch();
							}

							GUIUtility.hotControl = 0;
							GUIUtility.keyboardControl = 0;
						}

						currentEvent.Use();
					}
					break;
				case EventType.KeyDown:
					if (GUIUtility.hotControl == controlID && currentEvent.keyCode == KeyCode.Escape)
					{
						if (graphEditor != null)
						{
							graphEditor.EndDragBranch();
						}

						GUIUtility.hotControl = 0;
						GUIUtility.keyboardControl = 0;
						currentEvent.Use();
					}
					break;
				case EventType.ContextClick:
					if (position.Contains(currentEvent.mousePosition))
					{
						GenericMenu menu = new GenericMenu();
						if (branch != null)
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
					break;
				case EventType.Repaint:
					if (dragging)
					{
						if (!(hasBranch && branch.isActive))
						{
							if (graphEditor != null)
							{
								graphEditor.DragBranchBezie(bezier);
							}
						}
						else
						{

						}
					}

					DrawLinkSlot(position, controlID, hasBranch, false);
					break;
			}

			EditorGUI.EndDisabledGroup();
		}

		public void ChildLinkSlot(List<NodeLinkSlot> linkSlots)
		{
			BehaviourTreeGraphEditor graphEditor = this.graphEditor as BehaviourTreeGraphEditor;
			TreeNodeBase treeNode = node as TreeNodeBase;

			EditorGUI.BeginDisabledGroup(!graphEditor.editable);

			Rect position = GUILayoutUtility.GetRect(100, 16);

			BehaviourTreeInternal behaviourTree = graphEditor.nodeGraph as BehaviourTreeInternal;

			bool hasBranch = false;
			int slotCount = linkSlots.Count;
			for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
			{
				NodeLinkSlot linkSlot = linkSlots[slotIndex];
				NodeBranch branch = behaviourTree.nodeBranchies.GetFromID(linkSlot.branchID);
				if (branch != null)
				{
					hasBranch = true;
					break;
				}
			}

			Vector2 pinPos = GetNodeLinkPinPos(position);

			int controlID = GUIUtility.GetControlID(s_LinkSlotHash, FocusType.Passive, position);

			Event currentEvent = Event.current;
			EventType eventType = currentEvent.GetTypeForControl(controlID);

			Vector2 nowPos = currentEvent.mousePosition;

			bool dragging = (GUIUtility.hotControl == controlID && currentEvent.button == 0);

			Bezier2D bezier = new Bezier2D();
			if (dragging)
			{
				Vector2 startPos = NodeToGraphPoint(pinPos);
				Vector2 endPos = NodeToGraphPoint(nowPos);

				TreeNodeBase hoverNode = behaviourTree.GetNodeFromID(graphEditor.GetDragBranchHoverID()) as TreeNodeBase;
				if (hoverNode != null)
				{
					TreeNodeBaseEditor nodeEditor = graphEditor.GetNodeEditor(hoverNode) as TreeNodeBaseEditor;
					endPos = GetNodeLinkPinPos(nodeEditor.parentLinkSlotPosition);
				}

				Vector2 startControl = startPos + kBezierTangentOffset;
				Vector2 endControl = endPos - kBezierTangentOffset;

				bezier = new Bezier2D(startPos, startControl, endPos, endControl);
			}

			if (eventType != EventType.Layout && eventType != EventType.Used)
			{
				Rect slotPosition = NodeToGraphRect(position);

				childLinkSlotPosition = slotPosition;

				for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
				{
					NodeLinkSlot linkSlot = linkSlots[slotIndex];
					NodeBranch branch = behaviourTree.nodeBranchies.GetFromID(linkSlot.branchID);
					if (branch != null)
					{
						Vector2 startPosition = NodeToGraphPoint(pinPos);
						Vector2 startControl = startPosition + kBezierTangentOffset;

						if (branch.bezier.SetStartPoint(startPosition, startControl))
						{
							Repaint();
						}
					}
				}
			}

			switch (eventType)
			{
				case EventType.MouseDown:
					if (currentEvent.button == 0 && position.Contains(currentEvent.mousePosition))
					{
						GUIUtility.hotControl = GUIUtility.keyboardControl = controlID;

						if (graphEditor != null)
						{
							graphEditor.BeginDragBranch(treeNode.nodeID, false);
							graphEditor.DragBranchBezie(bezier);
						}

						currentEvent.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
					{
						Node childNode = graphEditor.GetHoverChildNode(treeNode, NodeToGraphPoint(nowPos));

						if (childNode != null)
						{
							if (graphEditor != null)
							{
								graphEditor.DragBranchHoverID(childNode.nodeID);
							}
						}
						else
						{
							if (graphEditor != null)
							{
								graphEditor.DragBranchHoverID(0);
							}
						}

						currentEvent.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						if (currentEvent.button == 0)
						{
							if (graphEditor != null)
							{
								int hoverID = graphEditor.GetDragBranchHoverID();
								TreeNodeBase childNode = behaviourTree.GetNodeFromID(hoverID) as TreeNodeBase;

								if (childNode == null)
								{
									GenericMenu menu = new GenericMenu();

									MousePosition mousePosition = new MousePosition(currentEvent.mousePosition);

									menu.AddItem(EditorContents.createComposite, false, () =>
										{
											OnCreateComposite(mousePosition, (pos, classType) =>
											{
												Undo.IncrementCurrentGroup();
												int undoGroup = Undo.GetCurrentGroup();

												float defaultWidth = 300f;

												pos -= new Vector2(defaultWidth * 0.5f, 0f);

												CompositeNode compositeNode = graphEditor.CreateComposite(pos, classType);

												NodeBranch branch = behaviourTree.ConnectBranch(treeNode, compositeNode);

												if (branch != null)
												{
													branch.bezier = bezier;
													Repaint();
												}

												behaviourTree.CalculatePriority();

												Undo.CollapseUndoOperations(undoGroup);
											});
										});

									menu.AddItem(EditorContents.createAction, false, () =>
										{
											OnCreateAction(mousePosition, (pos, classType) =>
											{
												Undo.IncrementCurrentGroup();
												int undoGroup = Undo.GetCurrentGroup();

												pos -= new Vector2(kDefaultNodeWidth * 0.5f, 0f);

												ActionNode actionNode = graphEditor.CreateAction(pos, classType);

												NodeBranch branch = behaviourTree.ConnectBranch(treeNode, actionNode);

												if (branch != null)
												{
													branch.bezier = bezier;
													Repaint();
												}

												behaviourTree.CalculatePriority();

												Undo.CollapseUndoOperations(undoGroup);
											});
										});

									menu.ShowAsContext();
								}
								else
								{
									bool isLinked = false;
									for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
									{
										NodeLinkSlot linkSlot = linkSlots[slotIndex];
										NodeBranch branch = behaviourTree.nodeBranchies.GetFromID(linkSlot.branchID);
										if (branch != null && branch.childNodeID == hoverID)
										{
											isLinked = true;
											break;
										}
									}

									if (!isLinked)
									{
										if (behaviourTree.CheckLoop(treeNode, childNode))
										{
											Debug.LogError("Node has become an infinite loop.");
										}
										else
										{
											NodeBranch branch = behaviourTree.ConnectBranch(treeNode, childNode);

											if (branch != null)
											{
												branch.bezier = bezier;
												Repaint();
											}

											behaviourTree.CalculatePriority();
										}
									}
								}

								graphEditor.EndDragBranch();
							}

							GUIUtility.hotControl = 0;
							GUIUtility.keyboardControl = 0;
						}

						currentEvent.Use();
					}
					break;
				case EventType.KeyDown:
					if (GUIUtility.hotControl == controlID && currentEvent.keyCode == KeyCode.Escape)
					{
						if (graphEditor != null)
						{
							graphEditor.EndDragBranch();
						}

						GUIUtility.hotControl = 0;
						GUIUtility.keyboardControl = 0;
						currentEvent.Use();
					}
					break;
				case EventType.ContextClick:
					if (position.Contains(currentEvent.mousePosition))
					{
						GenericMenu menu = new GenericMenu();

						bool isLinked = false;
						for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
						{
							NodeLinkSlot linkSlot = linkSlots[slotIndex];
							NodeBranch branch = behaviourTree.nodeBranchies.GetFromID(linkSlot.branchID);
							if (branch != null)
							{
								isLinked = true;
								break;
							}
						}

						if (isLinked)
						{
							menu.AddItem(EditorContents.disconnectAll, false, () =>
							  {
								  Undo.IncrementCurrentGroup();
								  int undoGroup = Undo.GetCurrentGroup();

								  for (int slotIndex = slotCount - 1; slotIndex >= 0; slotIndex--)
								  {
									  NodeLinkSlot linkSlot = linkSlots[slotIndex];
									  NodeBranch branch = behaviourTree.nodeBranchies.GetFromID(linkSlot.branchID);
									  if (branch != null)
									  {
										  behaviourTree.DisconnectBranch(branch);
									  }
								  }

								  behaviourTree.CalculatePriority();

								  Undo.CollapseUndoOperations(undoGroup);
							  });
						}
						else
						{
							menu.AddDisabledItem(EditorContents.disconnectAll);
						}

						menu.ShowAsContext();

						currentEvent.Use();
					}
					break;
				case EventType.Repaint:
					if (dragging)
					{
						if (graphEditor != null)
						{
							graphEditor.DragBranchBezie(bezier);
						}
					}

					DrawLinkSlot(position, controlID, hasBranch, false);
					break;
			}

			EditorGUI.EndDisabledGroup();
		}

		public override bool IsShowNodeList()
		{
			return true;
		}

		public override void OnListElement(Rect rect)
		{
			if (treeNode.enablePriority)
			{
				GUIStyle style = Styles.countBadge;
				GUIContent content = new GUIContent(treeNode.priority.ToString());
				Vector2 size = style.CalcSize(content);
				Rect priorityRect = new Rect(rect.x + rect.width - size.x - 2, rect.center.y - size.y * 0.5f, size.x, size.y);
				EditorGUI.LabelField(priorityRect, content, style);
			}
		}

		protected override bool HasOutsideGUI()
		{
			return true;
		}

		protected override RectOffset GetOutsideOffset()
		{
			RectOffset offset = new RectOffset();

			TreeNodeBase treeNode = node as TreeNodeBase;

			if (treeNode.enablePriority)
			{
				GUIStyle style = Styles.countBadge;
				GUIContent content = new GUIContent(treeNode.priority.ToString());
				Vector2 size = style.CalcSize(content);
				offset.right = Mathf.Max(offset.right, (int)(size.x * 0.5f));
				offset.top = Mathf.Max(offset.top, (int)(size.y * 0.5f));
			}

			return offset;
		}

		protected override void OnOutsideGUI()
		{
			RectOffset overflowOffset = GetOverflowOffset();

			TreeNodeBase treeNode = node as TreeNodeBase;

			if (treeNode.enablePriority)
			{
				GUIStyle style = Styles.countBadge;
				GUIContent content = new GUIContent(treeNode.priority.ToString());
				Vector2 size = style.CalcSize(content);
				Rect countRect = new Rect(node.position.width - size.x * 0.5f, -size.y * 0.5f, size.x, size.y);
				countRect.position += new Vector2(overflowOffset.left, overflowOffset.top);

				EditorGUI.LabelField(countRect, content, style);
			}
		}

		Node GetChildNode(NodeLinkSlot childSlot)
		{
			BehaviourTreeInternal behaviourTree = graphEditor.nodeGraph as BehaviourTreeInternal;

			NodeBranch branch = behaviourTree.nodeBranchies.GetFromID(childSlot.branchID);
			if (branch == null)
			{
				return null;
			}

			return behaviourTree.GetNodeFromID(branch.childNodeID);
		}

		protected void RegisterDragChild(NodeLinkSlot childNodeLink)
		{
			Node childNode = GetChildNode(childNodeLink);
			if (childNode != null)
			{
				graphEditor.RegisterDragNode(childNode);

				TreeNodeBaseEditor childNodeEditor = graphEditor.GetNodeEditor(childNode) as TreeNodeBaseEditor;
				if (childNodeEditor != null)
				{
					childNodeEditor.RegisterDragChildren();
				}
			}
		}

		public virtual void RegisterDragChildren()
		{
		}

		public override void OnBeginDrag(Event evt)
		{
			base.OnBeginDrag(evt);

			if (evt.alt)
			{
				RegisterDragChildren();
			}
		}
	}
}