//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace ArborEditor
{
	using Arbor;
	using Arbor.Serialization;
	using Arbor.Playables;

	[System.Serializable]
	public sealed class StateBehaviourEditorGUI : BehaviourEditorGUI
	{
		public StateEditor stateEditor
		{
			get
			{
				return nodeEditor as StateEditor;
			}
		}

		public State state
		{
			get
			{
				return (nodeEditor != null) ? nodeEditor.node as State : null;
			}
		}

		public List<StateEditor.StateLinkProperty> stateLinkProperties = new List<StateEditor.StateLinkProperty>();

		private Dictionary<SerializedPropertyKey, StateEditor.StateLinkProperty> _UpdateStateLinkProperties = new Dictionary<SerializedPropertyKey, StateEditor.StateLinkProperty>();

		private static System.Text.StringBuilder s_StateLinkPropertyBuilder = new System.Text.StringBuilder();

		void AddStateLinkProperty(SerializedProperty property, System.Reflection.FieldInfo fieldInfo, StateLink stateLink)
		{
			if (stateLink == null)
			{
				return;
			}

			SerializedPropertyKey key = new SerializedPropertyKey(property);

			StateEditor.StateLinkProperty stateLinkProperty = null;
			if (!_UpdateStateLinkProperties.TryGetValue(key, out stateLinkProperty))
			{
				stateLinkProperty = new StateEditor.StateLinkProperty(stateEditor, behaviourObj as StateBehaviour);
			}

			string label = stateLink.name;
			if (string.IsNullOrEmpty(label))
			{
				label = s_StateLinkPropertyBuilder.ToString();
			}
			stateLinkProperty.label = EditorGUITools.GetTextContent(label);
			stateLinkProperty.property = property.Copy();
			stateLinkProperty.stateLinkGUI.stateLink = stateLink;
			stateLinkProperty.stateLinkGUI.fieldInfo = fieldInfo;

			stateLinkProperties.Add(stateLinkProperty);
		}

		void UpdateStateLinkProperty(SerializedProperty property, System.Reflection.FieldInfo fieldInfo, System.Type fieldType, object value)
		{
			if (property.isArray)
			{
				System.Type elementType = SerializationUtility.ElementTypeOfArray(fieldType);

				int currentLength = s_StateLinkPropertyBuilder.Length;

				IList list = (IList)value;

				for (int i = 0; i < property.arraySize; i++)
				{
					s_StateLinkPropertyBuilder.Append("[");
					s_StateLinkPropertyBuilder.Append(i);
					s_StateLinkPropertyBuilder.Append("]");

					SerializedProperty elementProperty = property.GetArrayElementAtIndex(i);

					object elementValue = list[i];

					if (elementType == typeof(StateLink))
					{
						AddStateLinkProperty(elementProperty, fieldInfo, elementValue as StateLink);
					}
					else
					{
						UpdateStateLinkProperty(elementProperty, fieldInfo, elementType, elementValue);
					}

					s_StateLinkPropertyBuilder.Length = currentLength;
				}
			}
			else if (fieldType == typeof(StateLink))
			{
				AddStateLinkProperty(property, fieldInfo, value as StateLink);
			}
			else
			{
#if UNITY_2019_3_OR_NEWER
				if (property.propertyType == SerializedPropertyType.ManagedReference)
				{
					if(value == null)
					{
						return;
					}

					fieldType = property.GetTypeFromManagedReferenceFullTypeName();
					if (fieldType == null)
					{
						return;
					}
				}
#endif

				int currentLength = s_StateLinkPropertyBuilder.Length;

				foreach (Arbor.DynamicReflection.DynamicField dynamicField in EachField<StateLink>.GetFields(fieldType))
				{
					System.Reflection.FieldInfo fi = dynamicField.fieldInfo;

					SerializedProperty p = property.FindPropertyRelative(fi.Name);
					object elementValue = dynamicField.GetValue(value);

					s_StateLinkPropertyBuilder.Append("/");
					s_StateLinkPropertyBuilder.Append(p.displayName);

					UpdateStateLinkProperty(p, fi, fi.FieldType, elementValue);

					s_StateLinkPropertyBuilder.Length = currentLength;
				}
			}
		}

		private void UpdateStateLinkInternal()
		{
			using (new ProfilerScope("UpdateStateLink"))
			{
				if (editor == null)
				{
					return;
				}

				SerializedObject serializedObject = editor.serializedObject;

				Object targetObject = serializedObject.targetObject;

				System.Type classType = targetObject.GetType();

				if (stateLinkProperties == null)
				{
					stateLinkProperties = new List<StateEditor.StateLinkProperty>();
				}

				_UpdateStateLinkProperties.Clear();

				for (int stateLinkIndex = 0; stateLinkIndex < stateLinkProperties.Count; stateLinkIndex++)
				{
					StateEditor.StateLinkProperty stateLinkProperty = stateLinkProperties[stateLinkIndex];
					if (!stateLinkProperty.property.IsValid())
					{
						continue;
					}
					var key = new SerializedPropertyKey(stateLinkProperty.property);
					_UpdateStateLinkProperties.Add(key, stateLinkProperty);
				}

				stateLinkProperties.Clear();

				foreach (Arbor.DynamicReflection.DynamicField dynamicField in EachField<StateLink>.GetFields(classType))
				{
					System.Reflection.FieldInfo fieldInfo = dynamicField.fieldInfo;

					object value = dynamicField.GetValue(targetObject);

					SerializedProperty property = serializedObject.FindProperty(fieldInfo.Name);

					s_StateLinkPropertyBuilder.Length = 0;
					s_StateLinkPropertyBuilder.Append(property.displayName);

					UpdateStateLinkProperty(property, fieldInfo, fieldInfo.FieldType, value);
				}
			}
		}

		public void UpdateStateLink()
		{
			if (editor == null)
			{
				return;
			}

			SerializedObject serializedObject = editor.serializedObject;

			serializedObject.Update();

			UpdateStateLinkInternal();

			serializedObject.ApplyModifiedProperties();
		}

		protected override bool HasTitlebar()
		{
			return true;
		}

		public override bool GetExpanded()
		{
			StateBehaviour behaviour = behaviourObj as StateBehaviour;
			return (behaviour != null) ? BehaviourEditorUtility.GetExpanded(behaviour, behaviour.expanded) : this.expanded;
		}

		public override void SetExpanded(bool expanded)
		{
			StateBehaviour behaviour = behaviourObj as StateBehaviour;
			if (behaviour != null)
			{
				if ((behaviour.hideFlags & HideFlags.NotEditable) != HideFlags.NotEditable)
				{
					behaviour.expanded = expanded;
					EditorUtility.SetDirty(behaviour);
				}
				BehaviourEditorUtility.SetExpanded(behaviour, expanded);
			}
			else
			{
				this.expanded = expanded;
			}
		}

		protected override bool HasBehaviourEnable()
		{
			return true;
		}

		protected override bool GetBehaviourEnable()
		{
			StateBehaviour behaviour = behaviourObj as StateBehaviour;
			return behaviour.behaviourEnabled;
		}

		protected override void SetBehaviourEnable(bool enable)
		{
			StateBehaviour behaviour = behaviourObj as StateBehaviour;
			behaviour.behaviourEnabled = enable;
		}

		protected override void SetPopupMenu(GenericMenu menu)
		{
			bool editable = nodeEditor.graphEditor.editable;

			int behaviourCount = state.behaviourCount;

			if (behaviourIndex >= 1 && editable)
			{
				menu.AddItem(EditorContents.moveUp, false, MoveUpBehaviourContextMenu);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.moveUp);
			}

			if (behaviourIndex < behaviourCount - 1 && editable)
			{
				menu.AddItem(EditorContents.moveDown, false, MoveDownBehaviourContextMenu);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.moveDown);
			}

			StateBehaviour behaviour = behaviourObj as StateBehaviour;
			if (behaviour != null)
			{
				menu.AddItem(EditorContents.copy, false, CopyBehaviourContextMenu);
				if (Clipboard.CompareBehaviourType(behaviourObj.GetType(), false) && editable)
				{
					menu.AddItem(EditorContents.paste, false, PasteBehaviourContextMenu);
				}
				else
				{
					menu.AddDisabledItem(EditorContents.paste);
				}
			}

			if (editable)
			{
				menu.AddItem(EditorContents.delete, false, DeleteBehaviourContextMenu);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.delete);
			}
		}

		void MoveUpBehaviourContextMenu()
		{
			if (stateEditor != null)
			{
				stateEditor.MoveBehaviour(behaviourIndex, behaviourIndex - 1);
			}
		}

		void MoveDownBehaviourContextMenu()
		{
			if (stateEditor != null)
			{
				stateEditor.MoveBehaviour(behaviourIndex, behaviourIndex + 1);
			}
		}

		void CopyBehaviourContextMenu()
		{
			StateBehaviour behaviour = behaviourObj as StateBehaviour;

			Clipboard.CopyBehaviour(behaviour);
		}

		void PasteBehaviourContextMenu()
		{
			StateBehaviour behaviour = behaviourObj as StateBehaviour;

			Undo.IncrementCurrentGroup();

			Undo.RecordObject(behaviour, "Paste Behaviour");

			Clipboard.PasteBehaviourValues(behaviour);

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			EditorUtility.SetDirty(behaviour);
		}

		void DeleteBehaviourContextMenu()
		{
			if (stateEditor != null)
			{
				stateEditor.RemoveBehaviour(behaviourIndex);
			}
		}

		public bool StateLinkGUI(GUIStyle headerStyle)
		{
			if (editor == null)
			{
				return false;
			}

			editor.serializedObject.Update();

			UpdateStateLinkInternal();

			bool hasStateLinks = false;

			if (stateLinkProperties != null)
			{
				int stateLinkCount = stateLinkProperties.Count;
				if (stateLinkCount > 0)
				{
					hasStateLinks = true;

					if (headerStyle != null && headerStyle != GUIStyle.none)
					{
						BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(behaviourObj);
						GUILayout.Box(behaviourInfo.titleContent, headerStyle);
					}
					else
					{
						HeaderSpace();
					}
					
					EditorGUILayout.BeginVertical(Styles.stateLinkMargin);

					for (int i = 0; i < stateLinkCount; i++)
					{
						StateEditor.StateLinkProperty stateLinkProperty = stateLinkProperties[i];
						SingleStateLinkField(stateLinkProperty);
					}

					EditorGUILayout.EndVertical();
				}
			}

			editor.serializedObject.ApplyModifiedProperties();

			return hasStateLinks;
		}

		public override void OnTopGUI()
		{
			if (ArborSettings.stateLinkShowMode == StateLinkShowMode.BehaviourTop)
			{
				StateLinkGUI(null);
			}
		}

		public override void OnBottomGUI()
		{
			if (ArborSettings.stateLinkShowMode == StateLinkShowMode.BehaviourBottom)
			{
				StateLinkGUI(null);
			}
		}

		void SingleStateLinkField(StateEditor.StateLinkProperty stateLinkProperty)
		{
			using (new ProfilerScope("SingleStateLinkField"))
			{
				EditorGUI.BeginDisabledGroup(nodeEditor.graphEditor != null && !nodeEditor.graphEditor.editable);

				SerializedProperty property = stateLinkProperty.property;
				StateBehaviour behaviour = property.serializedObject.targetObject as StateBehaviour;

				if (behaviour == null || behaviour.nodeID == 0 || behaviour.stateMachine == null || property.isArray)
				{
					GUIContent helpContent = EditorGUITools.GetHelpBoxContent("StateLink can only be used with ArborEditor.", MessageType.Error);

					Rect position = GUILayoutUtility.GetRect(helpContent, EditorStyles.helpBox);
					EditorGUI.HelpBox(position, helpContent.text, MessageType.Error);
				}
				else
				{
					StateLinkGUI stateLinkGUI = stateLinkProperty.stateLinkGUI;

					stateLinkGUI.StateLinkField(stateLinkProperty.label);
				}

				EditorGUI.EndDisabledGroup();
			}
		}

		protected override void OnUnderlayGUI(Rect rect)
		{
			Event currentEvent = Event.current;
			if (currentEvent.type != EventType.Repaint || !Application.isPlaying)
			{
				return;
			}

			StateBehaviour stateBehaviour = behaviourObj as StateBehaviour;

			if (stateBehaviour == null || !stateBehaviour.behaviourEnabled)
			{
				return;
			}

			ArborFSMInternal stateMachine = stateBehaviour.stateMachine;
			if (stateMachine == null ||
				stateMachine.playState == PlayState.Stopping ||
				stateMachine.currentState != state)
			{
				return;
			}

			if (!stateBehaviour.IsActive())
			{
				Color conditionColor = StateMachineGraphEditor.reservedColor;

				rect.width = 5f;
				EditorGUI.DrawRect(rect, conditionColor);
			}
		}
	}
}