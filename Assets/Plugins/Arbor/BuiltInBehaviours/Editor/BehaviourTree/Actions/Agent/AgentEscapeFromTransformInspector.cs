//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.BehaviourTree.Actions
{
	using Arbor.BehaviourTree.Actions;

	[CustomEditor(typeof(AgentEscapeFromTransform))]
	internal sealed class AgentEscapeFromTransformInspector : AgentMoveBaseInspector
	{
		SerializedProperty _DistanceProperty;
		SerializedProperty _TargetTransformProperty;

		protected override void OnEnable()
		{
			base.OnEnable();

			_DistanceProperty = serializedObject.FindProperty("_Distance");
			_TargetTransformProperty = serializedObject.FindProperty("_TargetTransform");
		}

		protected override void OnGUI()
		{
			base.OnGUI();

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_DistanceProperty);
			EditorGUILayout.PropertyField(_TargetTransformProperty);
		}
	}
}
