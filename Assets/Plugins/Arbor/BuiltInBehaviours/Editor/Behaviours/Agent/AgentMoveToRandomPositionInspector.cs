//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.StateMachine.StateBehaviours
{
	using Arbor;
	using Arbor.StateMachine.StateBehaviours;

	[CustomEditor(typeof(AgentMoveToRandomPosition))]
	internal sealed class AgentMoveToRandomPositionInspector : AgentMoveBaseInspector
	{
		SerializedProperty _RadiusProperty;
		SerializedProperty _StoppingDistanceProperty;
		FlexibleEnumProperty<PatrolCenterType> _CenterTypeProperty;
		FlexibleComponentProperty _CenterTransformProperty;
		FlexibleFieldProperty _CenterPositionProperty;

		protected override void OnEnable()
		{
			base.OnEnable();

			_RadiusProperty = serializedObject.FindProperty("_Radius");
			_StoppingDistanceProperty = serializedObject.FindProperty("_StoppingDistance");
			_CenterTypeProperty = new FlexibleEnumProperty<PatrolCenterType>(serializedObject.FindProperty("_CenterType"));
			_CenterTransformProperty = new FlexibleComponentProperty(serializedObject.FindProperty("_CenterTransform"));
			_CenterPositionProperty = new FlexibleFieldProperty(serializedObject.FindProperty("_CenterPosition"));
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawBase();

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

				centerType = newCenterType;
				centerTypeFlexibleType = newCenterTypeFlexibleType;
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

			serializedObject.ApplyModifiedProperties();
		}
	}
}