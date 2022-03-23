//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.BehaviourTree
{
	using Arbor.BehaviourTree;

	[CustomPropertyDrawer(typeof(AbortFlags))]
	internal sealed class AbortFlagsDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Decorator decorator = property.serializedObject.targetObject as Decorator;
			if (decorator != null && !decorator.HasConditionCheck())
			{
				return;
			}
			AbortFlags abortFlags = (AbortFlags)property.intValue;
			EditorGUI.BeginChangeCheck();
			abortFlags = (AbortFlags)EditorGUI.EnumFlagsField(position, label, abortFlags);
			if (EditorGUI.EndChangeCheck())
			{
				property.intValue = (int)abortFlags;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			Decorator decorator = property.serializedObject.targetObject as Decorator;
			if (decorator != null && !decorator.HasConditionCheck())
			{
				return -EditorGUIUtility.standardVerticalSpacing;
			}

			return base.GetPropertyHeight(property, label);
		}
	}
}