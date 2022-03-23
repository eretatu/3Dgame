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

	[CustomEditor(typeof(AddForceRigidbody2D))]
	internal sealed class AddForceRigidbody2DInspector : Editor
	{
		private SerializedProperty _TargetProperty;
		private SerializedProperty _ExecuteMethodFlagsProperty;
		private FlexibleEnumProperty<DirectionType> _DirectionTypeProperty;
		private FlexibleNumericProperty _AngleProperty;
		private FlexibleFieldProperty _DirectionProperty;
		private SerializedProperty _PowerProperty;
		private SerializedProperty _ForceModeProperty;
		private SerializedProperty _SpaceProperty;

		void OnEnable()
		{
			_TargetProperty = serializedObject.FindProperty("_Target");
			_ExecuteMethodFlagsProperty = serializedObject.FindProperty("_ExecuteMethodFlags");
			_DirectionTypeProperty = new FlexibleEnumProperty<DirectionType>(serializedObject.FindProperty("_DirectionType"));
			_AngleProperty = new FlexibleNumericProperty(serializedObject.FindProperty("_Angle"));
			_DirectionProperty = new FlexibleFieldProperty(serializedObject.FindProperty("_Direction"));
			_PowerProperty = serializedObject.FindProperty("_Power");
			_ForceModeProperty = serializedObject.FindProperty("_ForceMode");
			_SpaceProperty = serializedObject.FindProperty("_Space");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_TargetProperty);

			EditorGUILayout.PropertyField(_ExecuteMethodFlagsProperty);

			FlexibleType directionTypeFlexibleType = _DirectionTypeProperty.type;
			DirectionType directionType = _DirectionTypeProperty.value;

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_DirectionTypeProperty.property);
			if (EditorGUI.EndChangeCheck())
			{
				FlexibleType newDirectionTypeFlexibleType = _DirectionTypeProperty.type;
				DirectionType newDirectionType = _DirectionTypeProperty.value;
				if (directionTypeFlexibleType != newDirectionTypeFlexibleType || directionType != newDirectionType)
				{
					if (newDirectionTypeFlexibleType == FlexibleType.Constant)
					{
						serializedObject.ApplyModifiedProperties();

						switch (newDirectionType)
						{
							case DirectionType.EulerAngle:
								_DirectionProperty.Disconnect();
								break;
							case DirectionType.Vector:
								_AngleProperty.Disconnect();
								break;
						}

						GUIUtility.ExitGUI();
					}
				}
				directionTypeFlexibleType = newDirectionTypeFlexibleType;
				directionType = newDirectionType;
			}

			if (directionTypeFlexibleType == FlexibleType.Constant)
			{
				int indentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel++;

				switch (directionType)
				{
					case DirectionType.EulerAngle:
						EditorGUILayout.PropertyField(_AngleProperty.property);
						break;
					case DirectionType.Vector:
						EditorGUILayout.PropertyField(_DirectionProperty.property);
						break;
				}

				EditorGUI.indentLevel = indentLevel;
			}
			else
			{
				int indentLevel = EditorGUI.indentLevel;
				EditorGUILayout.LabelField("DirectionType: EulerAngle", EditorStyles.miniBoldLabel);

				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_AngleProperty.property);
				EditorGUI.indentLevel = indentLevel;

				EditorGUILayout.LabelField("DirectionType: Vector", EditorStyles.miniBoldLabel);

				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_DirectionProperty.property);
				EditorGUI.indentLevel = indentLevel;
			}

			EditorGUILayout.PropertyField(_PowerProperty);
			EditorGUILayout.PropertyField(_ForceModeProperty);
			EditorGUILayout.PropertyField(_SpaceProperty);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
