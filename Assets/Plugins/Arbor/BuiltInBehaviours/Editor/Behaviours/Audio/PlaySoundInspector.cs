//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(PlaySound))]
	internal sealed class PlaySoundInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_AudioSource"));

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_IsSetClip"));

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Clip"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}