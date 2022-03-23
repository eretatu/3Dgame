//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(AgentMoveOnWaypoint))]
	internal sealed class AgentMoveOnWaypointInspector : Editor
	{
		SerializedProperty _AgentControllerProperty;
		SerializedProperty _SpeedProperty;
		SerializedProperty _StopOnStateEndProperty;
		SerializedProperty _WaypointProperty;
		SerializedProperty _ClearDestPointProperty;
		SerializedProperty _TypeProperty;
		SerializedProperty _StoppingDistanceProperty;

		void OnEnable()
		{
			_AgentControllerProperty = serializedObject.FindProperty("_AgentController");
			_SpeedProperty = serializedObject.FindProperty("_Speed");
			_StopOnStateEndProperty = serializedObject.FindProperty("_StopOnStateEnd");
			_WaypointProperty = serializedObject.FindProperty("_Waypoint");
			_ClearDestPointProperty = serializedObject.FindProperty("_ClearDestPoint");
			_TypeProperty = serializedObject.FindProperty("_Type");
			_StoppingDistanceProperty = serializedObject.FindProperty("_StoppingDistance");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_AgentControllerProperty);
			EditorGUILayout.PropertyField(_SpeedProperty);
			EditorGUILayout.PropertyField(_StopOnStateEndProperty);

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_WaypointProperty);
			EditorGUILayout.PropertyField(_ClearDestPointProperty);
			EditorGUILayout.PropertyField(_TypeProperty);
			EditorGUILayout.PropertyField(_StoppingDistanceProperty);

			serializedObject.ApplyModifiedProperties();
		}
	}
}