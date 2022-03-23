//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	using Arbor;
	public class NodeGraphInspector : Editor, IPropertyChanged
	{
		private static readonly GUIContent _NameContent = new GUIContent("Name");

		NodeGraph _NodeGraph;

		[System.Reflection.Obfuscation(Exclude = true)]
		protected virtual void OnEnable()
		{
			_NodeGraph = target as NodeGraph;
			ComponentUtility.RefreshNodeGraph(_NodeGraph);

			EditorCallbackUtility.RegisterPropertyChanged(this);
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		protected virtual void OnDisable()
		{
			EditorCallbackUtility.UnregisterPropertyChanged(this);
		}

		void IPropertyChanged.OnPropertyChanged(PropertyChangedType propertyChangedType)
		{
			if (propertyChangedType != PropertyChangedType.UndoRedoPerformed)
			{
				return;
			}

			if (target == null)
			{
				_NodeGraph = null;
			}
		}

		protected virtual void OnCustomGUI()
		{
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("graphName"), _NameContent);
			NodeGraph rootGraph = _NodeGraph.rootGraph;
			if (_NodeGraph.rootGraph == _NodeGraph)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("playOnStart"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("updateSettings"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("debugInfiniteLoopSettings"), true);

				OnCustomGUI();
			}
			else
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.ObjectField("Root Graph", rootGraph, typeof(NodeGraph), true);
				EditorGUI.EndDisabledGroup();
			}

			serializedObject.ApplyModifiedProperties();

			if (EditorGUITools.ButtonForceEnabled("Open Editor"))
			{
				ArborEditorWindow.Open(_NodeGraph);
			}
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnDestroy()
		{
			if (!target && (object)_NodeGraph != null && !Application.isPlaying)
			{
				_NodeGraph.OnDestroy();
			}
		}
	}
}