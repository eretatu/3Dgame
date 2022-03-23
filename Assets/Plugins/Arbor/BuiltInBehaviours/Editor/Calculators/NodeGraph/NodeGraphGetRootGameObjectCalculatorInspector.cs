//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.Calculators
{
	using Arbor.Calculators;

	[CustomEditor(typeof(NodeGraphGetRootGameObjectCalculator))]
	internal sealed class NodeGraphGetRootGameObjectCalculatorInspector : CalculatorBehaviourEditor
	{
		private SerializedProperty _OutputProperty;

		private void OnEnable()
		{
			_OutputProperty = serializedObject.FindProperty("_Output");
		}

		public override bool IsResizableNode()
		{
			return false;
		}

		public override float GetNodeWidth()
		{
			return 200f;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_OutputProperty);

			serializedObject.ApplyModifiedProperties();
		}
	}
}