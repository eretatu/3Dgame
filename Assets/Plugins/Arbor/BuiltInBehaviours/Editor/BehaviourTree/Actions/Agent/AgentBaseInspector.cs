//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.BehaviourTree.Actions
{
	public class AgentBaseInspector : Editor
	{
		SerializedProperty _AgentControllerProperty;

		protected virtual void OnEnable()
		{
			_AgentControllerProperty = serializedObject.FindProperty("_AgentController");
		}

		protected virtual void OnGUI()
		{
			EditorGUILayout.PropertyField(_AgentControllerProperty);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			OnGUI();

			serializedObject.ApplyModifiedProperties();
		}
	}
}