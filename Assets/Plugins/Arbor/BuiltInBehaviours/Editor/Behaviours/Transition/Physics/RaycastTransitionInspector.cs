//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(RaycastTransition))]
	internal sealed class RaycastTransitionInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Origin"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Direction"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Distance"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_LayerMask"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_CheckUpdate"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_IsCheckTag"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Tag"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_RaycastHit"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}
