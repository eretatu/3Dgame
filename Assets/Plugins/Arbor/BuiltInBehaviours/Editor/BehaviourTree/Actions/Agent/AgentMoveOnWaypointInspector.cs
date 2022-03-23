//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.BehaviourTree.Actions
{
	using Arbor.BehaviourTree.Actions;

	[CustomEditor(typeof(AgentMoveOnWaypoint))]
	internal sealed class AgentMoveOnWaypointInspector : AgentMoveBaseInspector
	{
		SerializedProperty _WaypointProperty;
		SerializedProperty _ClearDestPointProperty;
		SerializedProperty _TypeProperty;
		SerializedProperty _StoppingDistanceProperty;

		protected override void OnEnable()
		{
			base.OnEnable();

			_WaypointProperty = serializedObject.FindProperty("_Waypoint");
			_ClearDestPointProperty = serializedObject.FindProperty("_ClearDestPoint");
			_TypeProperty = serializedObject.FindProperty("_Type");
			_StoppingDistanceProperty = serializedObject.FindProperty("_StoppingDistance");
		}

		protected override void OnGUI()
		{
			base.OnGUI();

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_WaypointProperty);
			EditorGUILayout.PropertyField(_ClearDestPointProperty);
			EditorGUILayout.PropertyField(_TypeProperty);
			EditorGUILayout.PropertyField(_StoppingDistanceProperty);
		}
	}
}
