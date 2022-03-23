//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(FindWithTagGameObject))]
	internal sealed class FindWithTagGameObjectInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Reference"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Output"));

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Tag"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}
