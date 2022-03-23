//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.BehaviourTree.Actions
{
	using Arbor.BehaviourTree.Actions;

	[CustomEditor(typeof(AgentMoveToPosition))]
	internal sealed class AgentMoveToPositionInspector : AgentMoveBaseInspector
	{
		SerializedProperty _TargetPositionProperty;
		SerializedProperty _StoppingDistanceProperty;

		protected override void OnEnable()
		{
			base.OnEnable();

			_TargetPositionProperty = serializedObject.FindProperty("_TargetPosition");
			_StoppingDistanceProperty = serializedObject.FindProperty("_StoppingDistance");
		}

		protected override void OnGUI()
		{
			base.OnGUI();

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_TargetPositionProperty);
			EditorGUILayout.PropertyField(_StoppingDistanceProperty);
		}
	}
}
