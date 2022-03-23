//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	using Arbor;
	using System.Collections.Generic;

	internal class DataSlotGUI
	{
		protected static readonly int s_DataSlotHash = "s_DataSlotHash".GetHashCode();

		static DataSlot _DragSlot;
		static Node _DragNode;
		static Object _DragBehaviour;
		public static Node _HoverNode;
		public static DataSlot _HoverSlot;
		public static Object _HoverObj;

		private static HashSet<int> _HighlightControlIDs = new HashSet<int>();

		public static void BeginDragSlot(Node node, DataSlot slot, Object behaviour)
		{
			_DragNode = node;
			_DragSlot = slot;
			_DragBehaviour = behaviour;
		}

		public static void EndDragSlot()
		{
			_DragNode = null;
			_DragSlot = null;
			_DragBehaviour = null;

			var graphEditor = ArborEditorWindow.activeWindow.graphEditor;
			foreach (int controlID in _HighlightControlIDs)
			{
				graphEditor.CloseHighlightControl(controlID);
			}
			_HighlightControlIDs.Clear();
		}

		public static bool IsDragSlotConnectable(Node node, DataSlot slot, Object behaviour)
		{
			if (_DragNode == null || _DragSlot == null || node == null || slot == null || _DragNode == node ||
				!_DragSlot.IsConnectable(slot))
			{
				return false;
			}

			Node inputNode = null;
			Object inputObj = null;

			Node outputNode = null;
			Object outputObj = null;

			switch (_DragSlot.slotType)
			{
				case SlotType.Input:
					inputNode = _DragNode;
					inputObj = _DragBehaviour;

					outputNode = node;
					outputObj = behaviour;
					break;
				case SlotType.Output:
				case SlotType.Reroute:
					inputNode = node;
					inputObj = behaviour;

					outputNode = _DragNode;
					outputObj = _DragBehaviour;
					break;
			}

			return !node.nodeGraph.CheckLoopDataBranch(inputNode.nodeID, inputObj, outputNode.nodeID, outputObj);
		}

		static InputSlotBase GetInputSlotFromPosition(Object obj, Vector2 position, DataSlot outputSlot)
		{
			NodeBehaviour nodeBehaviour = obj as NodeBehaviour;
			if (nodeBehaviour != null)
			{
				int slotCount = nodeBehaviour.dataSlotCount;
				for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
				{
					InputSlotBase inputSlot = nodeBehaviour.GetDataSlot(slotIndex) as InputSlotBase;
					if (inputSlot == null)
					{
						continue;
					}

					if (inputSlot.enabledGUI && inputSlot.isVisible && inputSlot.position.Contains(position) && DataSlot.IsConnectable(inputSlot, outputSlot))
					{
						return inputSlot;
					}
				}
			}

			return null;
		}

		static OutputSlotBase GetOutputSlotFromPosition(Object obj, Vector2 position, DataSlot inputSlot)
		{
			NodeBehaviour nodeBehaviour = obj as NodeBehaviour;
			if (nodeBehaviour != null)
			{
				int slotCount = nodeBehaviour.dataSlotCount;
				for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
				{
					OutputSlotBase outputSlot = nodeBehaviour.GetDataSlot(slotIndex) as OutputSlotBase;
					if (outputSlot == null)
					{
						continue;
					}

					if (outputSlot.enabledGUI && outputSlot.isVisible && outputSlot.position.Contains(position) && DataSlot.IsConnectable(inputSlot, outputSlot))
					{
						return outputSlot;
					}
				}
			}

			return null;
		}

		public static void UpdateHoverSlot(NodeGraph nodGraph, Node targetNode, Object targetObj, DataSlot sourceSlot, Vector2 position)
		{
			_HoverSlot = GetSlotFromPosition(nodGraph, targetNode, targetObj, sourceSlot, position, out _HoverObj, out _HoverNode);
		}

		public static void ClearHoverSlot()
		{
			_HoverSlot = null;
			_HoverObj = null;
			_HoverNode = null;
		}

		static DataSlot GetSlotFromPosition(NodeGraph nodeGraph, Node targetNode, Object targetObj, DataSlot sourceSlot, Vector2 position, out Object obj, out Node hoverNode)
		{
			for (int i = 0, count = nodeGraph.nodeCount; i < count; i++)
			{
				Node node = nodeGraph.GetNodeFromIndex(i);

				INodeBehaviourContainer behaviours = node as INodeBehaviourContainer;
				if (behaviours != null)
				{
					int behaviourCount = behaviours.GetNodeBehaviourCount();
					for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
					{
						NodeBehaviour behaviour = behaviours.GetNodeBehaviour<NodeBehaviour>(behaviourIndex);
						if (targetNode == node || targetObj == behaviour)
						{
							continue;
						}
						DataSlot findSlot = null;
						switch (sourceSlot.slotType)
						{
							case SlotType.Input:
								if (!nodeGraph.CheckLoopDataBranch(targetNode.nodeID, targetObj, node.nodeID, behaviour))
								{
									findSlot = GetOutputSlotFromPosition(behaviour, position, sourceSlot);
								}
								break;
							case SlotType.Output:
							case SlotType.Reroute:
								if (!nodeGraph.CheckLoopDataBranch(node.nodeID, behaviour, targetNode.nodeID, targetObj))
								{
									findSlot = GetInputSlotFromPosition(behaviour, position, sourceSlot);
								}
								break;
						}

						if (findSlot != null)
						{
							obj = behaviour;
							hoverNode = node;
							return findSlot;
						}
					}
				}
				else
				{
					DataBranchRerouteNode rerouteNode = node as DataBranchRerouteNode;
					if (rerouteNode != null)
					{
						DataSlot slot = rerouteNode.link;
						if (slot.isVisible && slot.position.Contains(position) && sourceSlot.IsConnectable(slot))
						{
							switch (sourceSlot.slotType)
							{
								case SlotType.Input:
									if (!nodeGraph.CheckLoopDataBranch(targetNode.nodeID, targetObj, node.nodeID, null))
									{
										obj = null;
										hoverNode = node;
										return slot;
									}
									break;
								case SlotType.Output:
								case SlotType.Reroute:
									if (!nodeGraph.CheckLoopDataBranch(node.nodeID, null, targetNode.nodeID, targetObj))
									{
										obj = null;
										hoverNode = node;
										return slot;
									}
									break;
							}
						}
					}
				}
			}

			obj = null;
			hoverNode = null;
			return null;
		}

		public static DataSlotGUI GetGUI(SerializedProperty property, DataSlot slot)
		{
			if (slot == null)
			{
				return null;
			}

			DataSlotGUI slotGUI = null;

			switch (slot.slotType)
			{
				case SlotType.Input:
					slotGUI = new InputSlotGUI();
					break;
				case SlotType.Output:
					slotGUI = new OutputSlotGUI();
					break;
			}

			if (slotGUI == null)
			{
				return null;
			}

			if (!slotGUI.Initialize(property, slot))
			{
				return null;
			}

			return slotGUI;
		}

		public static DataSlotGUI GetGUI(SerializedProperty property)
		{
			if (property == null || !property.IsValid())
			{
				return null;
			}

			DataSlot slot = EditorGUITools.GetPropertyObject<DataSlot>(property);

			return GetGUI(property, slot);
		}

		internal static class Defaults
		{
			public static readonly RectOffset dataPinPadding = new RectOffset(2, 2, 0, 0);
			public static readonly RectOffset highLightPadding = new RectOffset(4, 2, 0, 0);
		}

		public Object targetObject
		{
			get;
			private set;
		}

		public NodeGraph nodeGraph
		{
			get;
			private set;
		}

		public Node node
		{
			get;
			private set;
		}

		public Vector2 nodePosition
		{
			get;
			private set;
		}

		public DataSlot slot
		{
			get;
			private set;
		}

		public bool Initialize(SerializedProperty property, DataSlot slot)
		{
			this.slot = slot;

			targetObject = property.serializedObject.targetObject;

			NodeBehaviour nodeBehaviour = targetObject as NodeBehaviour;
			if (nodeBehaviour != null)
			{
				nodeGraph = nodeBehaviour.nodeGraph;
				node = nodeBehaviour.node;

				if (slot != null && !slot.isValidField)
				{
					nodeBehaviour.RebuildDataSlotFields();
				}
			}

			if (!slot.isValidField)
			{
				return false;
			}

			RebuildConnectGUI();

			return true;
		}

		public virtual void RebuildConnectGUI()
		{
		}

		public void DoGUI(Rect position, SerializedProperty property, GUIContent label, Vector2 offset)
		{
			DataSlot slot = this.slot;
			if (slot == null)
			{
				return;
			}

			nodePosition = new Vector2(node.position.x, node.position.y) + offset;

			slot.SetVisible();
			slot.enabledGUI = GUI.enabled;

			if (Event.current.type == EventType.Repaint)
			{
				slot.position = new Rect(position.x + nodePosition.x, position.y + nodePosition.y, position.width, position.height);
			}

			label = new GUIContent(label);
			label.tooltip = slot.connectableTypeName;

			label = EditorGUI.BeginProperty(position, label, property);

			OnGUI(position, property, label);

			EditorGUI.EndProperty();
		}

		protected void DrawSlot(Rect position, GUIContent label, int controlID, bool on, bool isInput)
		{
			DataSlot slot = this.slot;

			bool isDragConnectable = IsDragSlotConnectable(node, slot, targetObject);

			bool isActive = GUIUtility.hotControl == controlID;

			bool isDragHover = slot == _HoverSlot;

			GUIStyle buttonStyle = isActive ? Styles.dataLinkSlotActive : Styles.dataLinkSlot;

			Color slotColor = isActive ? NodeGraphEditor.dragBezierColor : EditorGUITools.GetTypeColor(slot.connectableType);
			Color slotBackgroundColor = EditorGUITools.GetSlotBackgroundColor(slotColor, isActive, on);

			Color backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = slotBackgroundColor;

			buttonStyle.Draw(position, GUIContent.none, controlID, on);

			GUI.backgroundColor = backgroundColor;

			if (on || isActive || isDragHover)
			{
				Vector2 p1 = isInput ? new Vector2(0, position.center.y + 0.5f) : new Vector2(position.xMax - 6f, position.center.y + 0.5f);
				Vector2 p2 = isInput ? new Vector2(position.xMin + 8f, position.center.y + 0.5f) : new Vector2(node.position.width, position.center.y + 0.5f);

				EditorGUITools.DrawLines(Styles.outlineConnectionTexture, slotColor, 8.0f, p1, p2);
			}

			GUI.backgroundColor = slotColor;
			GUIStyle pinStyle = isInput ? Styles.GetDataInPin(slot.connectableType) : Styles.GetDataOutPin(slot.connectableType);
			pinStyle.Draw(Defaults.dataPinPadding.Remove(position), label, controlID, on || isActive || isDragHover);
			GUI.backgroundColor = backgroundColor;

			if (isDragConnectable)
			{
				_HighlightControlIDs.Add(controlID);
				ArborEditorWindow.activeWindow.graphEditor.ShowHightlightControl(NodeEditor.currentEditor.NodeToGraphRect(position), controlID, Styles.highlight);
			}
		}

		protected virtual void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
		}
	}
}