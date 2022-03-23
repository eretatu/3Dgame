//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.BehaviourTree.Actions
{
	using Arbor.BehaviourTree.Actions;

	[CustomEditor(typeof(AgentMoveToTransform))]
	internal sealed class AgentMoveToTransformInspector : AgentMoveBaseInspector
	{
		SerializedProperty _TargetTransformProperty;
		SerializedProperty _StoppingDistanceProperty;

		protected override void OnEnable()
		{
			base.OnEnable();

			_TargetTransformProperty = serializedObject.FindProperty("_TargetTransform");
			_StoppingDistanceProperty = serializedObject.FindProperty("_StoppingDistance");
		}

		protected override void OnGUI()
		{
			base.OnGUI();

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_TargetTransformProperty);
			EditorGUILayout.PropertyField(_StoppingDistanceProperty);
		}
	}
}
