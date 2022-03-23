//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.Calculators
{
	using Arbor;
	using Arbor.Calculators;

	[CustomEditor(typeof(EnumFlagsAddCalculator))]
	internal sealed class EnumFlagsAddCalculatorInspector : Editor
	{
		private FlexibleFieldProperty _Value1Property;
		private FlexibleFieldProperty _Value2Property;
		private OutputSlotTypableProperty _ResultProperty;

		private void OnEnable()
		{
			_Value1Property = new FlexibleFieldProperty(serializedObject.FindProperty("_Value1"));
			_Value2Property = new FlexibleFieldProperty(serializedObject.FindProperty("_Value2"));
			_ResultProperty = new OutputSlotTypableProperty(serializedObject.FindProperty("_Result"));
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			ClassTypeReferenceProperty typeProperty = _ResultProperty.typeProperty;

			System.Type enumType = typeProperty.type;

			typeProperty.SetConstraint(ClassTypeConstraintEditorUtility.enumFlags);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(typeProperty.property, true);
			if (EditorGUI.EndChangeCheck() && enumType != typeProperty.type)
			{
				_Value1Property.Disconnect();
				_Value2Property.Disconnect();
				_ResultProperty.Disconnect();

				serializedObject.ApplyModifiedProperties();

				GUIUtility.ExitGUI(); // throw ExitGUIException
			}

			EditorGUILayout.PropertyField(_Value1Property.property, true);
			EditorGUILayout.PropertyField(_Value2Property.property, true);
			EditorGUILayout.PropertyField(_ResultProperty.property, true);

			serializedObject.ApplyModifiedProperties();
		}
	}
}