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

	[CustomEditor(typeof(AddVelocityRigidbody))]
	internal sealed class AddVelocityRigidbodyInspector : Editor
	{
		private SerializedProperty _TargetProperty;
		private SerializedProperty _ExecuteMethodFlagsProperty;
		private FlexibleEnumProperty<DirectionType> _DirectionTypeProperty;
		private SerializedProperty _DirectionProperty;
		private SerializedProperty _SpeedProperty;
		private SerializedProperty _SpaceProperty;

		void OnEnable()
		{
			_TargetProperty = serializedObject.FindProperty("_Target");
			_ExecuteMethodFlagsProperty = serializedObject.FindProperty("_ExecuteMethodFlags");
			_DirectionTypeProperty = new FlexibleEnumProperty<DirectionType>(serializedObject.FindProperty("_DirectionType"));
			_DirectionProperty = serializedObject.FindProperty("_Direction");
			_SpeedProperty = serializedObject.FindProperty("_Speed");
			_SpaceProperty = serializedObject.FindProperty("_Space");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_TargetProperty);

			EditorGUILayout.PropertyField(_ExecuteMethodFlagsProperty);

			EditorGUILayout.PropertyField(_DirectionTypeProperty.property);

			int indentLevel = EditorGUI.indentLevel;

			GUIContent directionContent = null;
			if (_DirectionTypeProperty.type == FlexibleType.Constant)
			{
				DirectionType directionType = _DirectionTypeProperty.value;

				switch (directionType)
				{
					case DirectionType.EulerAngle:
						directionContent = EditorGUITools.GetTextContent("Angle");
						break;
					case DirectionType.Vector:
						directionContent = EditorGUITools.GetTextContent("Direction");
						break;
				}

				EditorGUI.indentLevel++;
			}

			EditorGUILayout.PropertyField(_DirectionProperty, directionContent);

			EditorGUI.indentLevel = indentLevel;

			EditorGUILayout.PropertyField(_SpeedProperty);
			EditorGUILayout.PropertyField(_SpaceProperty);

			serializedObject.ApplyModifiedProperties();

		}
	}
}
