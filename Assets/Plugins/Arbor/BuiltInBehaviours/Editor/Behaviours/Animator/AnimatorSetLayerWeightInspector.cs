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
	[CustomEditor(typeof(AnimatorSetLayerWeight))]
	internal sealed class AnimatorSetLayerWeightInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			SerializedProperty animatorProperty = serializedObject.FindProperty("_Animator");
			FlexibleFieldProperty layerNameProperty = new FlexibleFieldProperty(serializedObject.FindProperty("_LayerName"));

			EditorGUILayout.PropertyField(animatorProperty);

			if (animatorProperty.FindPropertyRelative("_Type").enumValueIndex == EnumUtility.GetIndexFromValue(FlexibleType.Constant))
			{
				Animator animator = animatorProperty.FindPropertyRelative("_Value").objectReferenceValue as Animator;

				EditorGUITools.AnimatorLayerField(animator, layerNameProperty);
			}
			else
			{
				EditorGUILayout.PropertyField(layerNameProperty.property);
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Weight"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}
