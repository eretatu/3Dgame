//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

using Arbor;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(Arbor.StateMachine.StateBehaviours.AnimatorStateTransition))]
	internal sealed class AnimatorStateTransitionInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			SerializedProperty animatorProperty = serializedObject.FindProperty("_Animator");
			FlexibleFieldProperty layerNameProperty = new FlexibleFieldProperty(serializedObject.FindProperty("_LayerName"));
			FlexibleFieldProperty stateNameProperty = new FlexibleFieldProperty(serializedObject.FindProperty("_StateName"));

			EditorGUILayout.PropertyField(animatorProperty);

			if (animatorProperty.FindPropertyRelative("_Type").enumValueIndex == EnumUtility.GetIndexFromValue(FlexibleType.Constant))
			{
				Animator animator = animatorProperty.FindPropertyRelative("_Value").objectReferenceValue as Animator;

				EditorGUITools.AnimatorStateField(animator, layerNameProperty, stateNameProperty);
			}
			else
			{
				EditorGUILayout.PropertyField(layerNameProperty.property);
				EditorGUILayout.PropertyField(stateNameProperty.property);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
