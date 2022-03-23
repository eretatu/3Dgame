//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.StateMachine.StateBehaviours
{
	internal abstract class CheckTagBehaviourBaseInspector : Editor
	{
		protected virtual void OnFieldGUI()
		{
		}

		public sealed override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_TagChecker"), GUIContent.none, true);

			OnFieldGUI();

			serializedObject.ApplyModifiedProperties();
		}
	}
}