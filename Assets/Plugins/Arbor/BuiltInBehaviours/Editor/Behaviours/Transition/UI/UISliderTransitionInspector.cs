//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(UISliderTransition))]
	internal sealed class UISliderTransitionInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Slider"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_ChangeTimingTransition"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Threshold"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}