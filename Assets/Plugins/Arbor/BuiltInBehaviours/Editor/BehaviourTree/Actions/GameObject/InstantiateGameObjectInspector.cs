//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

using Arbor;
using Arbor.BehaviourTree.Actions;

namespace ArborEditor.BehaviourTree.Actions
{
	[CustomEditor(typeof(InstantiateGameObject))]
	internal sealed class InstantiateGameObjectInspector : Editor
	{
		SerializedProperty _PrefabProperty;
		SerializedProperty _ParentProperty;
		FlexibleEnumProperty<PostureType> _PostureTypeProperty;
		FlexibleFieldProperty _InitTransformProperty;
		FlexibleFieldProperty _InitPositionProperty;
		FlexibleFieldProperty _InitRotationProperty;
		FlexibleFieldProperty _InitSpaceProperty;
		SerializedProperty _UsePoolProperty;
		SerializedProperty _ParameterProperty;
		SerializedProperty _OutputProperty;

		private void OnEnable()
		{
			_PrefabProperty = serializedObject.FindProperty("_Prefab");
			_ParentProperty = serializedObject.FindProperty("_Parent");

			_PostureTypeProperty = new FlexibleEnumProperty<PostureType>(serializedObject.FindProperty("_PostureType"));
			_InitTransformProperty = new FlexibleFieldProperty(serializedObject.FindProperty("_InitTransform"));
			_InitPositionProperty = new FlexibleFieldProperty(serializedObject.FindProperty("_InitPosition"));
			_InitRotationProperty = new FlexibleFieldProperty(serializedObject.FindProperty("_InitRotation"));
			_InitSpaceProperty = new FlexibleFieldProperty(serializedObject.FindProperty("_InitSpace"));

			_UsePoolProperty = serializedObject.FindProperty("_UsePool");
			_ParameterProperty = serializedObject.FindProperty("_Parameter");
			_OutputProperty = serializedObject.FindProperty("_Output");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_PrefabProperty);
			EditorGUILayout.PropertyField(_ParentProperty);

			FlexibleType postureFlexibleType = _PostureTypeProperty.type;
			PostureType postureType = _PostureTypeProperty.value;

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_PostureTypeProperty.property);
			if (EditorGUI.EndChangeCheck())
			{
				FlexibleType newPostureFlexibleType = _PostureTypeProperty.type;
				PostureType newPostureType = _PostureTypeProperty.value;
				if (postureFlexibleType != newPostureFlexibleType || postureType != newPostureType)
				{
					if (newPostureFlexibleType == FlexibleType.Constant)
					{
						serializedObject.ApplyModifiedProperties();

						if (newPostureType != PostureType.Transform)
						{
							_InitTransformProperty.Disconnect();
						}
						if (newPostureType != PostureType.Directly)
						{
							_InitPositionProperty.Disconnect();
							_InitRotationProperty.Disconnect();
							_InitSpaceProperty.Disconnect();
						}

						GUIUtility.ExitGUI();
					}
				}

				postureFlexibleType = newPostureFlexibleType;
				postureType = newPostureType;
			}

			if (postureFlexibleType == FlexibleType.Constant)
			{
				int indentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel++;

				switch (postureType)
				{
					case PostureType.Transform:
						{
							EditorGUILayout.PropertyField(_InitTransformProperty.property);
						}
						break;
					case PostureType.Directly:
						{
							EditorGUILayout.PropertyField(_InitPositionProperty.property);
							EditorGUILayout.PropertyField(_InitRotationProperty.property);
							EditorGUILayout.PropertyField(_InitSpaceProperty.property);
						}
						break;
				}

				EditorGUI.indentLevel = indentLevel;
			}
			else
			{
				int indentLevel = EditorGUI.indentLevel;

				EditorGUILayout.LabelField("PostureType: Transform", EditorStyles.miniBoldLabel);

				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_InitTransformProperty.property);
				EditorGUI.indentLevel = indentLevel;

				EditorGUILayout.LabelField("PostureType: Directly", EditorStyles.miniBoldLabel);

				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_InitPositionProperty.property);
				EditorGUILayout.PropertyField(_InitRotationProperty.property);
				EditorGUILayout.PropertyField(_InitSpaceProperty.property);
				EditorGUI.indentLevel = indentLevel;
			}

			EditorGUILayout.PropertyField(_UsePoolProperty);
			EditorGUILayout.PropertyField(_ParameterProperty);
			EditorGUILayout.PropertyField(_OutputProperty);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
