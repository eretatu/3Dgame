//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.BehaviourTree.Decorators
{
	using Arbor.BehaviourTree.Decorators;

	[CustomEditor(typeof(ParameterCheck))]
	internal sealed class ParameterCheckInspector : Editor
	{
		SerializedProperty _AbortFlagsProperty;
		SerializedProperty _ConditionsProperty;

		private void OnEnable()
		{
			_AbortFlagsProperty = serializedObject.FindProperty("_AbortFlags");
			_ConditionsProperty = serializedObject.FindProperty("_ConditionList");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_AbortFlagsProperty);
			EditorGUILayout.PropertyField(_ConditionsProperty);

			serializedObject.ApplyModifiedProperties();
		}
	}
}