//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

using Arbor;

namespace ArborEditor
{
	[CustomEditor(typeof(AgentController))]
	internal sealed class AgentControllerInspector : Editor
	{
		SerializedProperty _Agent;
		SerializedProperty _Animator;
		SerializedProperty _MovingParameter;
		SerializedProperty _MovingSpeedThreshold;
		SerializedProperty _SpeedParameter;
		SerializedProperty _SpeedType;
		SerializedProperty _SpeedDivValue;
		SerializedProperty _SpeedDampTime;
		SerializedProperty _MovementType;
		SerializedProperty _MovementDivValue;
		SerializedProperty _MovementXParameter;
		SerializedProperty _MovementXDampTime;
		SerializedProperty _MovementYParameter;
		SerializedProperty _MovementYDampTime;
		SerializedProperty _MovementZParameter;
		SerializedProperty _MovementZDampTime;
		SerializedProperty _TurnParameter;
		SerializedProperty _TurnType;
		SerializedProperty _TurnDampTime;

		void OnEnable()
		{
			_Agent = serializedObject.FindProperty("_Agent");
			_Animator = serializedObject.FindProperty("_Animator");
			_MovingParameter = serializedObject.FindProperty("_MovingParameter");
			_MovingSpeedThreshold = serializedObject.FindProperty("_MovingSpeedThreshold");
			_SpeedParameter = serializedObject.FindProperty("_SpeedParameter");
			_SpeedType = serializedObject.FindProperty("_SpeedType");
			_SpeedDivValue = serializedObject.FindProperty("_SpeedDivValue");
			_SpeedDampTime = serializedObject.FindProperty("_SpeedDampTime");
			_MovementType = serializedObject.FindProperty("_MovementType");
			_MovementDivValue = serializedObject.FindProperty("_MovementDivValue");
			_MovementXParameter = serializedObject.FindProperty("_MovementXParameter");
			_MovementXDampTime = serializedObject.FindProperty("_MovementXDampTime");
			_MovementYParameter = serializedObject.FindProperty("_MovementYParameter");
			_MovementYDampTime = serializedObject.FindProperty("_MovementYDampTime");
			_MovementZParameter = serializedObject.FindProperty("_MovementZParameter");
			_MovementZDampTime = serializedObject.FindProperty("_MovementZDampTime");
			_TurnParameter = serializedObject.FindProperty("_TurnParameter");
			_TurnType = serializedObject.FindProperty("_TurnType");
			_TurnDampTime = serializedObject.FindProperty("_TurnDampTime");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_Agent);

			EditorGUILayout.PropertyField(_Animator);

			Animator animator = _Animator.objectReferenceValue as Animator;

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Moving", EditorStyles.boldLabel);
			EditorGUITools.AnimatorParameterField(animator, _MovingParameter, null, EditorGUITools.GetTextContent(_MovingParameter.displayName), AnimatorControllerParameterType.Bool);
			EditorGUILayout.PropertyField(_MovingSpeedThreshold);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Speed", EditorStyles.boldLabel);
			EditorGUITools.AnimatorParameterField(animator, _SpeedParameter, null, EditorGUITools.GetTextContent(_SpeedParameter.displayName), AnimatorControllerParameterType.Float);
			EditorGUILayout.PropertyField(_SpeedType);
			if(EnumUtility.GetValueFromIndex<AgentController.SpeedType>(_SpeedType.enumValueIndex) == AgentController.SpeedType.DivValue)
			{
				EditorGUILayout.PropertyField(_SpeedDivValue);
			}
			EditorGUILayout.PropertyField(_SpeedDampTime);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_MovementType);
			if(EnumUtility.GetValueFromIndex<AgentController.MovementType>(_MovementType.enumValueIndex) == AgentController.MovementType.DivValue)
			{
				EditorGUILayout.PropertyField(_MovementDivValue);
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("MovementX", EditorStyles.boldLabel);
			EditorGUITools.AnimatorParameterField(animator, _MovementXParameter, null, EditorGUITools.GetTextContent(_MovementXParameter.displayName), AnimatorControllerParameterType.Float);
			EditorGUILayout.PropertyField(_MovementXDampTime);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("MovementY", EditorStyles.boldLabel);
			EditorGUITools.AnimatorParameterField(animator, _MovementYParameter, null, EditorGUITools.GetTextContent(_MovementYParameter.displayName), AnimatorControllerParameterType.Float);
			EditorGUILayout.PropertyField(_MovementYDampTime);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("MovementZ", EditorStyles.boldLabel);
			EditorGUITools.AnimatorParameterField(animator, _MovementZParameter, null, EditorGUITools.GetTextContent(_MovementZParameter.displayName), AnimatorControllerParameterType.Float);
			EditorGUILayout.PropertyField(_MovementZDampTime);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Turn", EditorStyles.boldLabel);
			EditorGUITools.AnimatorParameterField(animator, _TurnParameter, null, EditorGUITools.GetTextContent(_TurnParameter.displayName), AnimatorControllerParameterType.Float);
			EditorGUILayout.PropertyField(_TurnType);
			EditorGUILayout.PropertyField(_TurnDampTime);

			serializedObject.ApplyModifiedProperties();
		}
	}
}