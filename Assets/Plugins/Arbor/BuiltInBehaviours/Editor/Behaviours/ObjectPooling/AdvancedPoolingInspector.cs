//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor.StateMachine.StateBehaviours.ObjectPooling
{
	using Arbor.StateMachine.StateBehaviours.ObjectPooling;

	[CustomEditor(typeof(AdvancedPooling))]
	internal sealed class AdvancedPoolingInspector : Editor
	{
		// Paths
		private const string kPoolingItemsPath = "_PoolingItems";

		private SerializedProperty _PoolingItems;

		void OnEnable()
		{
			_PoolingItems = serializedObject.FindProperty(kPoolingItemsPath);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_PoolingItems);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
