//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.BehaviourTree.Decorators
{
	using Arbor.BehaviourTree.Decorators;

	[CustomEditor(typeof(CalculatorConditionalLoop))]
	internal sealed class CalculatorConditionalLoopInspector : Editor
	{
		SerializedProperty _ConditionsProperty;

		void OnEnable()
		{
			_ConditionsProperty = serializedObject.FindProperty("_ConditionList");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_ConditionsProperty);

			serializedObject.ApplyModifiedProperties();
		}
	}
}