//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.BehaviourTree.Actions
{
	public class AgentMoveBaseInspector : AgentBaseInspector
	{
		SerializedProperty _SpeedProperty;
		SerializedProperty _StopOnEndProperty;

		protected override void OnEnable()
		{
			base.OnEnable();

			_SpeedProperty = serializedObject.FindProperty("_Speed");
			_StopOnEndProperty = serializedObject.FindProperty("_StopOnEnd");
		}


		protected override void OnGUI()
		{
			base.OnGUI();

			EditorGUILayout.PropertyField(_SpeedProperty);
			EditorGUILayout.PropertyField(_StopOnEndProperty);
		}
	}
}