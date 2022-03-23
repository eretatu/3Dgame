//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(CalcAnimatorParameter))]
	internal sealed class CalcAnimatorParameterInspector : Editor
	{
		private SerializedProperty _ExecuteMethodFlagsProperty;
		private SerializedProperty _ReferenceProperty;
		private FlexibleEnumProperty<CalcAnimatorParameter.Function> _FunctionProperty;
		private FlexibleNumericProperty _FloatValueProperty;
		private FlexibleNumericProperty _IntValueProperty;
		private FlexibleBoolProperty _BoolValueProperty;

		void OnEnable()
		{
			_ExecuteMethodFlagsProperty = serializedObject.FindProperty("_ExecuteMethodFlags");
			_ReferenceProperty = serializedObject.FindProperty("_Reference");
			_FloatValueProperty = new FlexibleNumericProperty(serializedObject.FindProperty("_FloatValue"));
			_IntValueProperty = new FlexibleNumericProperty(serializedObject.FindProperty("_IntValue"));
			_BoolValueProperty = new FlexibleBoolProperty(serializedObject.FindProperty("_BoolValue"));
			_FunctionProperty = new FlexibleEnumProperty<CalcAnimatorParameter.Function>(serializedObject.FindProperty("_Function"));
		}

		static AnimatorControllerParameter GetParameter(Animator animator, string name)
		{
			if (animator == null)
			{
				return null;
			}

			AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
			if (animatorController == null)
			{
				return null;
			}

			var parameters = animatorController.parameters;
			for (int i = 0; i < parameters.Length; i++)
			{
				AnimatorControllerParameter parameter = parameters[i];
				if (parameter.name == name)
				{
					return parameter;
				}
			}

			return null;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_ExecuteMethodFlagsProperty);

			SerializedProperty typeProperty = _ReferenceProperty.FindPropertyRelative("type");
			
			AnimatorControllerParameterType parameterType = EnumUtility.GetValueFromIndex<AnimatorControllerParameterType>(typeProperty.enumValueIndex);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_ReferenceProperty);
			if (EditorGUI.EndChangeCheck())
			{
				AnimatorControllerParameterType newParameterType = EnumUtility.GetValueFromIndex<AnimatorControllerParameterType>(typeProperty.enumValueIndex);
				if (parameterType != newParameterType)
				{
					serializedObject.ApplyModifiedProperties();

					switch (parameterType)
					{
						case AnimatorControllerParameterType.Bool:
							_BoolValueProperty.Disconnect();
							break;
						case AnimatorControllerParameterType.Float:
							_FloatValueProperty.Disconnect();
							break;
						case AnimatorControllerParameterType.Int:
							_IntValueProperty.Disconnect();
							break;
					}

					switch (newParameterType)
					{
						case AnimatorControllerParameterType.Bool:
							_FunctionProperty.Disconnect();
							break;
						case AnimatorControllerParameterType.Trigger:
							_FunctionProperty.Disconnect();
							break;
					}

					GUIUtility.ExitGUI();
				}
				parameterType = newParameterType;
			}

			switch (parameterType)
			{
				case AnimatorControllerParameterType.Float:
					{
						EditorGUILayout.PropertyField(_FunctionProperty.property);
						EditorGUILayout.PropertyField(_FloatValueProperty.property, EditorGUITools.GetTextContent("Float Value"));
					}
					break;
				case AnimatorControllerParameterType.Int:
					{
						EditorGUILayout.PropertyField(_FunctionProperty.property);
						EditorGUILayout.PropertyField(_IntValueProperty.property, EditorGUITools.GetTextContent("Int Value"));
					}
					break;
				case AnimatorControllerParameterType.Bool:
					{
						EditorGUILayout.PropertyField(_BoolValueProperty.property, EditorGUITools.GetTextContent("Bool Value"));
					}
					break;
				case AnimatorControllerParameterType.Trigger:
					break;
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
