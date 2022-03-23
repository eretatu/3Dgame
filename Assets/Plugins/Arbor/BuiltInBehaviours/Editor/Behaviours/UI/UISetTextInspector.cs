//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor;
using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(UISetText))]
	internal sealed class UISetTextInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Text"));

			SerializedProperty stringProperty = serializedObject.FindProperty("_String");
			EditorGUILayout.PropertyField(stringProperty);

			SerializedProperty typeProperty = stringProperty.FindPropertyRelative("_Type");
			FlexibleType type = EnumUtility.GetValueFromIndex<FlexibleType>(typeProperty.enumValueIndex);
			if (type == FlexibleType.Parameter)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_ChangeTimingUpdate"));
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}