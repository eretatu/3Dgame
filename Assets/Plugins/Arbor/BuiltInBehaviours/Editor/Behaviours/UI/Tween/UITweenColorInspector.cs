//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(UITweenColor))]
	internal sealed class UITweenColorInspector : TweenBaseInspector
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawBase();

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Target"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Gradient"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}