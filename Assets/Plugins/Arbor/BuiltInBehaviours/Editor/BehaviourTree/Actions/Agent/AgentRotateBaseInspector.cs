//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.BehaviourTree.Actions
{
	public class AgentRotateBaseInspector : AgentBaseInspector
	{
		SerializedProperty _AngularSpeedProperty;
		SerializedProperty _StopOnEndProperty;

		protected override void OnEnable()
		{
			base.OnEnable();

			_AngularSpeedProperty = serializedObject.FindProperty("_AngularSpeed");
			_StopOnEndProperty = serializedObject.FindProperty("_StopOnEnd");
		}


		protected override void OnGUI()
		{
			base.OnGUI();

			EditorGUILayout.PropertyField(_AngularSpeedProperty);
			EditorGUILayout.PropertyField(_StopOnEndProperty);
		}
	}
}