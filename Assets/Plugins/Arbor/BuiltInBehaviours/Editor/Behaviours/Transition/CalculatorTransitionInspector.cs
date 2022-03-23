//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.StateMachine.StateBehaviours
{
	using Arbor.StateMachine.StateBehaviours;

	[CustomEditor(typeof(CalculatorTransition))]
	internal sealed class CalculatorTransitionInspector : Editor
	{
		SerializedProperty _TriggerFlagsProperty;
		SerializedProperty _ConditionsProperty;

		void OnEnable()
		{
			_TriggerFlagsProperty = serializedObject.FindProperty("_TriggerFlags");
			_ConditionsProperty = serializedObject.FindProperty("_ConditionList");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_TriggerFlagsProperty);
			EditorGUILayout.PropertyField(_ConditionsProperty);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
