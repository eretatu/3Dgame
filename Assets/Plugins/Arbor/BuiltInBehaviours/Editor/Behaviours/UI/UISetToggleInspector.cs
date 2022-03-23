//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor;
using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(UISetToggle))]
	internal sealed class UISetToggleInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Toggle"));

			SerializedProperty valueProperty = serializedObject.FindProperty("_Value");
			EditorGUILayout.PropertyField(valueProperty);

			SerializedProperty typeProperty = valueProperty.FindPropertyRelative("_Type");
			FlexiblePrimitiveType type = EnumUtility.GetValueFromIndex<FlexiblePrimitiveType>(typeProperty.enumValueIndex);
			if (type == FlexiblePrimitiveType.Parameter)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_ChangeTimingUpdate"));
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}