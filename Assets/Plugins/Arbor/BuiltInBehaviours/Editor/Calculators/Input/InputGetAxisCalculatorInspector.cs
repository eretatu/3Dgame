//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.Calculators
{
	using Arbor.Calculators;

	[CustomEditor(typeof(InputGetAxisCalculator))]
	internal sealed class InputGetAxisCalculatorInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_AxisName"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Output"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}