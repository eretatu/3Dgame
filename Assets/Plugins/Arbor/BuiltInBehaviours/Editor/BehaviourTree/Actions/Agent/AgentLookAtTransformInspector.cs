//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.BehaviourTree.Actions
{
	using Arbor.BehaviourTree.Actions;

	[CustomEditor(typeof(AgentLookAtTransform))]
	internal sealed class AgentLookAtTransformInspector : AgentRotateBaseInspector
	{
		SerializedProperty _TargetTransformProperty;

		protected override void OnEnable()
		{
			base.OnEnable();

			_TargetTransformProperty = serializedObject.FindProperty("_TargetTransform");
		}


		protected override void OnGUI()
		{
			base.OnGUI();

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_TargetTransformProperty);
		}
	}
}