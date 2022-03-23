//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(AgentMoveToPosition))]
	internal sealed class AgentMoveToPositionInspector : AgentMoveBaseInspector
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawBase();

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_StoppingDistance"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Position"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}