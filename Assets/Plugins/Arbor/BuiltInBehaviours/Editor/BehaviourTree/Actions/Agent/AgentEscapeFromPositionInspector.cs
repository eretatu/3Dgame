//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.BehaviourTree.Actions
{
	using Arbor.BehaviourTree.Actions;

	[CustomEditor(typeof(AgentEscapeFromPosition))]
	internal sealed class AgentEscapeFromPositionInspector : AgentMoveBaseInspector
	{
		SerializedProperty _DistanceProperty;
		SerializedProperty _TargetPositionProperty;

		protected override void OnEnable()
		{
			base.OnEnable();

			_DistanceProperty = serializedObject.FindProperty("_Distance");
			_TargetPositionProperty = serializedObject.FindProperty("_TargetPosition");
		}

		protected override void OnGUI()
		{
			base.OnGUI();

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_DistanceProperty);
			EditorGUILayout.PropertyField(_TargetPositionProperty);
		}
	}
}
