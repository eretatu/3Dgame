//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(OnCollisionEnter2DTransition))]
	internal sealed class OnCollisionEnter2DTransitionInspector : CheckTagBehaviourBaseInspector
	{
		protected override void OnFieldGUI()
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Collision2D"));
		}
	}
}
