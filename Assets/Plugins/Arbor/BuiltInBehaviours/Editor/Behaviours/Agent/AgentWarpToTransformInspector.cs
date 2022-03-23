//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(AgentWarpToTransform))]
	internal sealed class AgentWarpToTransformInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_AgentController"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Target"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}