//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(PlaySoundAtTransform))]
	internal sealed class PlaySoundAtTransformInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Clip"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Target"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Volume"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_OutputAudioMixerGroup"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_SpatialBlend"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}
