//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.StateMachine.StateBehaviours
{
	public class AgentRotateBaseInspector : AgentIntervalUpdateInspector
	{
		protected override void DrawBase()
		{
			base.DrawBase();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_AngularSpeed"));
		}
	}
}