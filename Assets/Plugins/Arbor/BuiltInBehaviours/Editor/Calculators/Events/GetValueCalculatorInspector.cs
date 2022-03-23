//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.Calculators
{
	using Arbor.Calculators;

	[CustomEditor(typeof(GetValueCalculator))]
	internal sealed class GetValueCalculatorInspector : NodeBehaviourEditor
	{
		SerializedProperty _PersistentProperty;

		void OnEnable()
		{
			_PersistentProperty = serializedObject.FindProperty("_Persistent");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_PersistentProperty, GUIContent.none, true);

			serializedObject.ApplyModifiedProperties();
		}
	}
}