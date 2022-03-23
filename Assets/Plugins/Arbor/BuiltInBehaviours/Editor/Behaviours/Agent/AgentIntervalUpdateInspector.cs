//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.StateMachine.StateBehaviours
{
	using Arbor;

	public class AgentIntervalUpdateInspector : Editor
	{
		SerializedProperty _AgentControllerProperty;
		FlexibleEnumProperty<AgentUpdateType> _UpdateTypeProperty;

		FlexibleEnumProperty<TimeType> _TimeTypeProperty;
		FlexibleNumericProperty _IntervalProperty;

		SerializedProperty _StopOnStateEndProperty;

		protected virtual void OnEnable()
		{
			_AgentControllerProperty = serializedObject.FindProperty("_AgentController");
			_UpdateTypeProperty = new FlexibleEnumProperty<AgentUpdateType>(serializedObject.FindProperty("_UpdateType"));
			_TimeTypeProperty = new FlexibleEnumProperty<TimeType>(serializedObject.FindProperty("_TimeType"));
			_IntervalProperty = new FlexibleNumericProperty(serializedObject.FindProperty("_Interval"));
			_StopOnStateEndProperty = serializedObject.FindProperty("_StopOnStateEnd");
		}

		protected virtual void DrawBase()
		{
			EditorGUILayout.PropertyField(_AgentControllerProperty);

			FlexibleType updateTypeFlexibleType = _UpdateTypeProperty.type;
			AgentUpdateType updateType = _UpdateTypeProperty.value;

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_UpdateTypeProperty.property);
			if (EditorGUI.EndChangeCheck())
			{
				FlexibleType newUpdateTypeFlexibleType = _UpdateTypeProperty.type;
				AgentUpdateType newUpdateType = _UpdateTypeProperty.value;

				if (updateTypeFlexibleType != newUpdateTypeFlexibleType || updateType != newUpdateType)
				{
					if (newUpdateTypeFlexibleType == FlexibleType.Constant)
					{
						if (newUpdateType != AgentUpdateType.Time && newUpdateType != AgentUpdateType.Done)
						{
							serializedObject.ApplyModifiedProperties();

							_TimeTypeProperty.Disconnect();
							_IntervalProperty.Disconnect();

							GUIUtility.ExitGUI();
						}

					}
				}
				updateTypeFlexibleType = newUpdateTypeFlexibleType;
				updateType = newUpdateType;
			}

			if (updateTypeFlexibleType == FlexibleType.Constant)
			{
				int indentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel++;

				switch (updateType)
				{
					case AgentUpdateType.Time:
						{
							EditorGUILayout.PropertyField(_TimeTypeProperty.property);
							EditorGUILayout.PropertyField(_IntervalProperty.property);
						}
						break;
					case AgentUpdateType.Done:
						{
							EditorGUILayout.PropertyField(_TimeTypeProperty.property);
							EditorGUILayout.PropertyField(_IntervalProperty.property);
						}
						break;
					case AgentUpdateType.StartOnly:
						{
							// No property
						}
						break;
					case AgentUpdateType.Always:
						{
							// No property
						}
						break;
				}

				EditorGUI.indentLevel = indentLevel;
			}
			else
			{
				int indentLevel = EditorGUI.indentLevel;

				EditorGUILayout.LabelField("Time Parameter", EditorStyles.miniBoldLabel);

				EditorGUI.indentLevel++;

				EditorGUILayout.PropertyField(_TimeTypeProperty.property);
				EditorGUILayout.PropertyField(_IntervalProperty.property);

				EditorGUI.indentLevel = indentLevel;
			}
			EditorGUILayout.PropertyField(_StopOnStateEndProperty);
		}
	}
}