//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

using Arbor;
using Arbor.StateMachine.StateBehaviours;

namespace ArborEditor.StateMachine.StateBehaviours
{
	[CustomEditor(typeof(UISetTextFromParameter))]
	internal sealed class UISetTextFromParameterInspector : Editor
	{
		private SerializedProperty _TextProperty;
		private SerializedProperty _FormatProperty;
		private SerializedProperty _ChangeTimingUpdateProperty;
		private ParameterReferenceProperty _ParameterReferenceProperty;

		private void OnEnable()
		{
			_TextProperty = serializedObject.FindProperty("_Text");
			_FormatProperty = serializedObject.FindProperty("_Format");
			_ChangeTimingUpdateProperty = serializedObject.FindProperty("_ChangeTimingUpdate");
			_ParameterReferenceProperty = new ParameterReferenceProperty(serializedObject.FindProperty("_Parameter"));
		}

		static bool HasFormatString(Parameter.Type type)
		{
			switch (type)
			{
				case Parameter.Type.Int:
				case Parameter.Type.Long:
				case Parameter.Type.Float:
					return true;
			}

			return false;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_TextProperty);

			ParameterReferenceProperty parameterReferenceProperty = _ParameterReferenceProperty;
			EditorGUILayout.PropertyField(parameterReferenceProperty.property);

			bool useFormat = false;
			ParameterReferenceType parameterReferenceType = parameterReferenceProperty.type;
			if (parameterReferenceType == ParameterReferenceType.DataSlot)
			{
				useFormat = true;
			}
			else
			{
				Parameter parameter = parameterReferenceProperty.GetParameter();
				useFormat = parameter != null && HasFormatString(parameter.type);
			}

			if (useFormat)
			{
				EditorGUILayout.PropertyField(_FormatProperty);
			}

			EditorGUILayout.PropertyField(_ChangeTimingUpdateProperty);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
