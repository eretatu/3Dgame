//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(SendEventGameObject))]
	internal sealed class SendEventGameObjectInspector : Editor
	{
		private SerializedProperty _OnStateAwakeProperty;
		private SerializedProperty _OnStateBeginProperty;
		private SerializedProperty _OnStateEndProperty;

		private void OnEnable()
		{
			_OnStateAwakeProperty = serializedObject.FindProperty("_OnStateAwake");
			_OnStateBeginProperty = serializedObject.FindProperty("_OnStateBegin");
			_OnStateEndProperty = serializedObject.FindProperty("_OnStateEnd");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_OnStateAwakeProperty);
			EditorGUILayout.PropertyField(_OnStateBeginProperty);
			EditorGUILayout.PropertyField(_OnStateEndProperty);

			serializedObject.ApplyModifiedProperties();
		}
	}
}