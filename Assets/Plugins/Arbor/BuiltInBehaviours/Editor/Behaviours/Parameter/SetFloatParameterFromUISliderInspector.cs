//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(SetFloatParameterFromUISlider))]
	internal sealed class SetFloatParameterFromUISliderInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Parameter"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Slider"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_ChangeTimingUpdate"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}