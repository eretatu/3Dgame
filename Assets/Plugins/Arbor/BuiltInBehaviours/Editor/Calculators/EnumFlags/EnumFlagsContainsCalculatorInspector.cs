//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.Calculators
{
	using Arbor.Calculators;

	[CustomEditor(typeof(EnumFlagsContainsCalculator))]
	internal sealed class EnumFlagsContainsCalculatorInspector : Editor
	{
		private ClassTypeReferenceProperty _TypeProperty;
		private FlexibleFieldProperty _Value1Property;
		private FlexibleFieldProperty _Value2Property;
		private OutputSlotBaseProperty _ResultProperty;

		private void OnEnable()
		{
			_TypeProperty = new ClassTypeReferenceProperty(serializedObject.FindProperty("_Type"));
			_Value1Property = new FlexibleFieldProperty(serializedObject.FindProperty("_Value1"));
			_Value2Property = new FlexibleFieldProperty(serializedObject.FindProperty("_Value2"));
			_ResultProperty = new OutputSlotBaseProperty(serializedObject.FindProperty("_Result"));
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			System.Type enumType = _TypeProperty.type;

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_TypeProperty.property, true);
			if (EditorGUI.EndChangeCheck() && enumType != _TypeProperty.type)
			{
				_Value1Property.Disconnect();
				_Value2Property.Disconnect();

				serializedObject.ApplyModifiedProperties();

				GUIUtility.ExitGUI();
			}

			EditorGUILayout.PropertyField(_Value1Property.property, true);
			EditorGUILayout.PropertyField(_Value2Property.property, true);
			EditorGUILayout.PropertyField(_ResultProperty.property, true);

			serializedObject.ApplyModifiedProperties();
		}
	}
}