//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	using Arbor;

	internal sealed class InputSlotGUI : DataSlotGUI
	{
		void DoInputSlotContextClickMenu(DataBranch currentBranch)
		{
			GenericMenu menu = new GenericMenu();

			if (currentBranch != null)
			{
				menu.AddItem(EditorContents.disconnect, false, () =>
				{
					nodeGraph.DeleteDataBranch(currentBranch);
					currentBranch = null;
				});
			}
			else
			{
				menu.AddDisabledItem(EditorContents.disconnect);
			}

			menu.ShowAsContext();
		}

		public override void RebuildConnectGUI()
		{
			DataBranch branch = null;

			DataSlot slot = this.slot;
			InputSlotBase inputSlot = slot as InputSlotBase;
			if (inputSlot != null)
			{
				branch = inputSlot.branch;

				bool clearBranchID = false;
				if (branch == null)
				{
					clearBranchID = inputSlot.branchID != 0;
				}
				else if (branch != null)
				{
					DataSlot branchInputSlot = branch.inputSlot;
					if (!object.ReferenceEquals(branchInputSlot, inputSlot))
					{
						//branch.ClearInputSlotField();
						//branchInputSlot = branch.inputSlot;
						//Debug.Log("inputSlot.branchID = 0");
						clearBranchID = true;
					}
				}

				if (clearBranchID)
				{
					inputSlot.ClearBranch();
					EditorUtility.SetDirty(targetObject);
					branch = null;
				}
			}

			if (branch != null)
			{
				if (!branch.isValidOutputSlot || !DataSlot.IsConnectable(slot, branch.outputSlot))
				{
					inputSlot.nodeGraph.DeleteDataBranch(branch);
				}
			}
		}

		private int _Button;

		protected override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			DataSlot slot = this.slot;
			InputSlotBase inputSlot = slot as InputSlotBase;

			ArborEditorWindow window = ArborEditorWindow.activeWindow;

			int controlID = EditorGUIUtility.GetControlID(s_DataSlotHash, FocusType.Passive, position);
			DataBranch branch = inputSlot.branch;

			Event current = Event.current;

			Vector2 pinPos = new Vector2(position.x, position.center.y);
			Vector2 pinControlPos = pinPos - EditorGUITools.kBezierTangentOffset;

			Vector2 targetPos = current.mousePosition;
			Vector2 targetControlPos = targetPos + EditorGUITools.kBezierTangentOffset;

			pinPos += nodePosition;
			pinControlPos += nodePosition;
			targetPos += nodePosition;
			targetControlPos += nodePosition;

			if (_HoverSlot != null)
			{
				DataBranchRerouteNode hoverRerouteNode = _HoverNode as DataBranchRerouteNode;
				if (hoverRerouteNode != null)
				{
					targetPos = _HoverSlot.position.center;
					targetControlPos = targetPos + hoverRerouteNode.direction * EditorGUITools.kBezierTangent;
				}
				else
				{
					targetPos = new Vector2(_HoverNode.position.xMax, _HoverSlot.position.center.y);
					targetControlPos = targetPos + EditorGUITools.kBezierTangentOffset;
				}
			}

			switch (current.GetTypeForControl(controlID))
			{
				case EventType.MouseMove:
					if (position.Contains(current.mousePosition))
					{
						current.Use();
					}
					break;
				case EventType.MouseDown:
					if (position.Contains(current.mousePosition))
					{
						GUIUtility.hotControl = GUIUtility.keyboardControl = controlID;

						_Button = current.button;

						if (_Button == 0)
						{
							if (window != null)
							{
								BeginDragSlot(node, slot, targetObject);

								if (branch != null)
								{
									branch.enabled = false;
								}

								window.graphEditor.BeginDragDataBranch(node.nodeID);
								window.graphEditor.DragDataBranchBezier(targetPos, targetControlPos, pinPos, pinControlPos);
							}
						}

						current.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
					{
						if (current.button == 0)
						{
							DragAndDrop.PrepareStartDrag();

							UpdateHoverSlot(nodeGraph, node, targetObject, inputSlot, window.UnclipToGraph(current.mousePosition));

							current.Use();
						}
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						if (current.button == _Button)
						{
							GUIUtility.hotControl = 0;

							if (current.button == 1 || Application.platform == RuntimePlatform.OSXEditor && current.control)
							{
								if (position.Contains(current.mousePosition))
								{
									DoInputSlotContextClickMenu(branch);
								}
							}
							else if (current.button == 0)
							{
								if (window != null)
								{
									EndDragSlot();

									window.graphEditor.EndDragDataBranch();

									if (_HoverSlot == null)
									{
										GenericMenu menu = new GenericMenu();

										Vector2 mousePosition = window.UnclipToGraph(current.mousePosition);

										DataBranch currentBranch = branch;
										Bezier2D lineBezier = new Bezier2D(targetPos, targetControlPos, pinPos, pinControlPos);

										if (inputSlot.connectableType != null || inputSlot.GetConstraint() == null)
										{
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

												DataBranchRerouteNode newRerouteNode = nodeGraph.CreateDataBranchRerouteNode(EditorGUITools.SnapToGrid(mousePosition), inputSlot.connectableType);

												Undo.RecordObject(nodeGraph, "Reroute");

												RerouteSlot rerouteSlot = newRerouteNode.link;

												currentBranch = nodeGraph.ConnectDataBranch(node.nodeID, targetObject, inputSlot, newRerouteNode.nodeID, null, rerouteSlot);
												if (currentBranch != null)
												{
													currentBranch.enabled = true;
													currentBranch.lineBezier = lineBezier;
												}

												Undo.CollapseUndoOperations(undoGroup);

												EditorUtility.SetDirty(nodeGraph);
											});
										}
										else
										{
											menu.AddDisabledItem(EditorContents.reroute);
										}

										if (currentBranch != null)
										{
											menu.AddSeparator("");
											menu.AddItem(EditorContents.disconnect, false, () =>
											{
												nodeGraph.DeleteDataBranch(currentBranch);
											});
										}

										menu.ShowAsContext();
									}
									else if (_HoverSlot != null)
									{
										if (branch != null)
										{
											nodeGraph.DeleteDataBranch(branch);
											branch = null;
										}

										branch = nodeGraph.ConnectDataBranch(node.nodeID, targetObject, inputSlot, _HoverNode.nodeID, _HoverObj, _HoverSlot);
										if (branch != null)
										{
											branch.lineBezier = new Bezier2D(targetPos, targetControlPos, pinPos, pinControlPos);
										}
									}

									if (branch != null)
									{
										branch.enabled = true;
									}

									ClearHoverSlot();
								}

							}
						}

						current.Use();
					}
					break;
				case EventType.KeyDown:
					if (GUIUtility.hotControl == controlID && current.keyCode == KeyCode.Escape)
					{
						GUIUtility.hotControl = 0;

						if (window != null)
						{
							EndDragSlot();

							window.graphEditor.EndDragDataBranch();

							if (branch != null)
							{
								branch.enabled = true;
							}

							ClearHoverSlot();
						}

						current.Use();
					}
					break;
				case EventType.Repaint:
					{
						if (GUIUtility.hotControl == controlID && current.button == 0)
						{
							window.graphEditor.DragDataBranchBezier(targetPos, targetControlPos, pinPos, pinControlPos);
						}
						else
						{
							if (branch != null)
							{
								branch.lineBezier.endPosition = pinPos;
								branch.lineBezier.endControl = pinControlPos;
							}
						}

						bool on = branch != null;

						DrawSlot(position, label, controlID, on, true);
					}
					break;
			}
		}
	}
}