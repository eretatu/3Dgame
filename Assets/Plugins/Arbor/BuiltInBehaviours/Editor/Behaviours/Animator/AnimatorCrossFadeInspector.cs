//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

using Arbor;
using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(AnimatorCrossFade))]
	internal sealed class AnimatorCrossFadeInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			FlexibleComponentProperty animatorProperty = new FlexibleComponentProperty(serializedObject.FindProperty("_Animator"));
			FlexibleFieldProperty layerNameProperty = new FlexibleFieldProperty(serializedObject.FindProperty("_LayerName"));
			FlexibleFieldProperty stateNameProperty = new FlexibleFieldProperty(serializedObject.FindProperty("_StateName"));

			EditorGUILayout.PropertyField(animatorProperty.property);

			if (animatorProperty.type == FlexibleSceneObjectType.Constant)
			{
				Animator animator = animatorProperty.valueProperty.objectReferenceValue as Animator;

				EditorGUITools.AnimatorStateField(animator, layerNameProperty, stateNameProperty);
			}
			else
			{
				EditorGUILayout.PropertyField(layerNameProperty.property);
				EditorGUILayout.PropertyField(stateNameProperty.property);
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_InFixedTime"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_TransitionDuration"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_TimeOffset"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_CheckInTransition"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}
