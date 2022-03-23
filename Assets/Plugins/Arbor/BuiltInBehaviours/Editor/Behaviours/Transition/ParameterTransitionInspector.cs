//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;
using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(ParameterTransition))]
	internal sealed class ParameterTransitionInspector : Editor
	{
		SerializedProperty _ConditionsProperty;

		private void OnEnable()
		{
			_ConditionsProperty = serializedObject.FindProperty("_ConditionList");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_ConditionsProperty, true);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
