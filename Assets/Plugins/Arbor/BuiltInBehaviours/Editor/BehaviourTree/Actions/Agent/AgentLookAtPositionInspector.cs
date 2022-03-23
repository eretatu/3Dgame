//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.BehaviourTree.Actions
{
	using Arbor.BehaviourTree.Actions;

	[CustomEditor(typeof(AgentLookAtPosition))]
	internal sealed class AgentLookAtPositionInspector : AgentRotateBaseInspector
	{
		SerializedProperty _TargetPositionProperty;

		protected override void OnEnable()
		{
			base.OnEnable();

			_TargetPositionProperty = serializedObject.FindProperty("_TargetPosition");
		}


		protected override void OnGUI()
		{
			base.OnGUI();

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_TargetPositionProperty);
		}
	}
}