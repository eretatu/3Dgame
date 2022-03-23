//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(LoadLevel))]
	internal sealed class LoadLevelInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_LevelName"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_LoadSceneMode"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_IsActiveScene"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}
