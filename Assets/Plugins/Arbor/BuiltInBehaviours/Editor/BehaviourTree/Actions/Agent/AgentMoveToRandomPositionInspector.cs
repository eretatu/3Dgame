//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.BehaviourTree.Actions
{
	using Arbor;
	using Arbor.BehaviourTree.Actions;

	[CustomEditor(typeof(AgentMoveToRandomPosition))]
	internal sealed class AgentMoveToRandomPositionInspector : AgentMoveBaseInspector
	{
		FlexibleEnumProperty<AgentUpdateType> _UpdateTypeProperty;
		FlexibleEnumProperty<TimeType> _TimeTypeProperty;
		FlexibleNumericProperty _IntervalProperty;
		SerializedProperty _RadiusProperty;
		SerializedProperty _StoppingDistanceProperty;
		FlexibleEnumProperty<PatrolCenterType> _CenterTypeProperty;
		FlexibleSceneObjectProperty _CenterTransformProperty;
		FlexibleFieldProperty _CenterPositionProperty;

		protected override void OnEnable()
		{
			base.OnEnable();

			_UpdateTypeProperty = new FlexibleEnumProperty<AgentUpdateType>(serializedObject.FindProperty("_UpdateType"));
			_TimeTypeProperty = new FlexibleEnumProperty<TimeType>(serializedObject.FindProperty("_TimeType"));
			_IntervalProperty = new FlexibleNumericProperty(serializedObject.FindProperty("_Interval"));
			_RadiusProperty = serializedObject.FindProperty("_Radius");
			_StoppingDistanceProperty = serializedObject.FindProperty("_StoppingDistance");
			_CenterTypeProperty = new FlexibleEnumProperty<PatrolCenterType>(serializedObject.FindProperty("_CenterType"));
			_CenterTransformProperty = new FlexibleSceneObjectProperty(serializedObject.FindProperty("_CenterTransform"));
			_CenterPositionProperty = new FlexibleFieldProperty(serializedObject.FindProperty("_CenterPosition"));
		}

		protected override void OnGUI()
		{
			base.OnGUI();

			EditorGUILayout.Space();

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

					updateTypeFlexibleType = newUpdateTypeFlexibleType;
					updateType = newUpdateType;
				}
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

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_RadiusProperty);
			EditorGUILayout.PropertyField(_StoppingDistanceProperty);

			FlexibleType centerTypeFlexibleType = _CenterTypeProperty.type;
			PatrolCenterType centerType = _CenterTypeProperty.value;

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_CenterTypeProperty.property);
			if (EditorGUI.EndChangeCheck())
			{
				FlexibleType newCenterTypeFlexibleType = _CenterTypeProperty.type;
				PatrolCenterType newCenterType = _CenterTypeProperty.value;
				if (centerTypeFlexibleType != newCenterTypeFlexibleType || centerType != newCenterType)
				{
					if (newCenterTypeFlexibleType == FlexibleType.Constant)
					{
						serializedObject.ApplyModifiedProperties();

						if (newCenterType != PatrolCenterType.Transform)
						{
							_CenterTransformProperty.Disconnect();
						}
						if (newCenterType != PatrolCenterType.Custom)
						{
							_CenterPositionProperty.Disconnect();
						}

						GUIUtility.ExitGUI();
					}
				}

				centerTypeFlexibleType = newCenterTypeFlexibleType;
				centerType = newCenterType;
			}

			if (centerTypeFlexibleType == FlexibleType.Constant)
			{
				int indentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel++;

				switch (centerType)
				{
					case PatrolCenterType.InitialPlacementPosition:
						break;
					case PatrolCenterType.StateStartPosition:
						break;
					case PatrolCenterType.Transform:
						EditorGUILayout.PropertyField(_CenterTransformProperty.property);
						break;
					case PatrolCenterType.Custom:
						EditorGUILayout.PropertyField(_CenterPositionProperty.property);
						break;
				}

				EditorGUI.indentLevel = indentLevel;
			}
			else
			{
				int indentLevel = EditorGUI.indentLevel;

				EditorGUILayout.LabelField("PatrolCenterType: Transform", EditorStyles.miniBoldLabel);

				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_CenterTransformProperty.property);
				EditorGUI.indentLevel = indentLevel;

				EditorGUILayout.LabelField("PatrolCenterType: Custom", EditorStyles.miniBoldLabel);

				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_CenterPositionProperty.property);
				EditorGUI.indentLevel = indentLevel;
			}
		}
	}
}