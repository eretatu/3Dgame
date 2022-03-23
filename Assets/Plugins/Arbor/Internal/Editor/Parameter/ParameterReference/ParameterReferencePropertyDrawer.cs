//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ArborEditor
{
	using Arbor;

	public sealed class ParameterReferenceEditorGUI
	{
		public delegate bool DelegateCheckType(Parameter parameter);

		static int GetSelectParameter(int id, ParameterContainerInternal container, List<string> names, List<int> ids, DelegateCheckType checkType)
		{
			int selected = -1;

			int parameterCount = container.parameterCount;

			if (parameterCount > 0)
			{
				for (int paramIndex = 0; paramIndex < parameterCount; paramIndex++)
				{
					Parameter parameter = container.GetParameterFromIndex(paramIndex);

					if (checkType != null && !checkType(parameter))
					{
						continue;
					}

					if (parameter.id == id)
					{
						selected = names.Count;
					}

					names.Add(parameter.name);
					ids.Add(parameter.id);
				}
			}

			return selected;
		}

		private ParameterReferenceProperty _ParameterReferenceProperty = null;

		[System.NonSerialized]
		private Parameter _DraggingParameter = null;

		public bool isDraggingParameter
		{
			get
			{
				return _DraggingParameter != null;
			}
		}

		public ParameterReferenceEditorGUI(ParameterReferenceProperty parameterReferenceProperty)
		{
			_ParameterReferenceProperty = parameterReferenceProperty;
		}

		public void ParameterField(Rect position)
		{
			ParameterReferenceType parameterReferenceType = _ParameterReferenceProperty.type;
			SerializedProperty containerProperty = _ParameterReferenceProperty.containerProperty;
			switch (parameterReferenceType)
			{
				case ParameterReferenceType.Constant:
					{
						ParameterContainerBase containerBase = containerProperty.objectReferenceValue as ParameterContainerBase;
						ParameterContainerInternal container = null;
						if (containerBase != null)
						{
							container = containerBase.defaultContainer;
						}

						List<string> names = new List<string>();
						List<int> ids = new List<int>();

						int selected = -1;

						if (container != null)
						{
							selected = GetSelectParameter(_ParameterReferenceProperty.id, container, names, ids, _ParameterReferenceProperty.CheckType);
						}

						if (names.Count > 0)
						{
							selected = EditorGUI.Popup(position, "Parameter", selected, names.ToArray());

							if (selected >= 0 && ids[selected] != _ParameterReferenceProperty.id)
							{
								_ParameterReferenceProperty.id = ids[selected];
								_ParameterReferenceProperty.name = names[selected];
							}
						}
						else
						{
							EditorGUI.BeginDisabledGroup(true);

							EditorGUI.LabelField(position, "Parameter", "", EditorStyles.popup);

							EditorGUI.EndDisabledGroup();
						}
					}
					break;
				case ParameterReferenceType.DataSlot:
					{
						EditorGUI.PropertyField(position, _ParameterReferenceProperty.nameProperty, EditorGUITools.GetTextContent("Parameter"));
					}
					break;
			}
		}

		public void ParameterFieldLayout()
		{
			Rect position = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight);
			ParameterField(position);
		}

		public void HandleDragParameter()
		{
			Event evt = Event.current;

			if (evt.type != EventType.Repaint && evt.type != EventType.Layout)
			{
				return;
			}

			Parameter nextDraggingParameter = null;

			if (ArborEditorWindow.isInNodeEditor)
			{
				ArborEditorWindow window = ArborEditorWindow.activeWindow;
				if (window != null)
				{
					NodeGraphEditor graphEditor = window.graphEditor;
					if (graphEditor != null)
					{
						Parameter parameter = graphEditor.draggingParameter;
						if (parameter != null && _ParameterReferenceProperty.CheckType(parameter))
						{
							nextDraggingParameter = parameter;
						}
					}

					if (_DraggingParameter != nextDraggingParameter && evt.type == EventType.Repaint)
					{
						window.DoRepaint();
					}
				}
			}

			_DraggingParameter = nextDraggingParameter;
			//_IsDraggingParameter = _DraggingParameter != null;
			//Debug.Log("HandleDragParameter : Repaint : " + _IsDraggingParameter);
		}

		public float GetDropParameterHeight()
		{
			if (!isDraggingParameter)
			{
				return 0f;
			}

			return Styles.dropField.CalcSize(EditorContents.dropParameter).y;
		}

		public void DropParameterLayout()
		{
			if (!isDraggingParameter)
			{
				return;
			}

			GUIContent label = EditorGUITools.GetTextContent(_ParameterReferenceProperty.property.displayName);
			EditorGUILayout.LabelField(label);

			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel++;

			Rect position = GUILayoutUtility.GetRect(EditorContents.dropParameter, Styles.dropField);

			DropParameter(position);

			EditorGUI.indentLevel = indentLevel;
		}

		public void DropParameter(Rect position)
		{
			if (!isDraggingParameter)
			{
				return;
			}

			position = EditorGUI.IndentedRect(position);

			GUIContent content = EditorContents.dropParameter;

			Event current = Event.current;
			switch (current.type)
			{
				case EventType.DragUpdated:
				case EventType.DragPerform:
					if (position.Contains(current.mousePosition))
					{
						if (_DraggingParameter != null)
						{
							DragAndDrop.visualMode = DragAndDropVisualMode.Link;

							if (current.type == EventType.DragPerform)
							{
								_ParameterReferenceProperty.SetParameter(_DraggingParameter);
								GUI.changed = true;

								DragAndDrop.AcceptDrag();
								DragAndDrop.activeControlID = 0;
							}
						}
						else
						{
							DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
						}

						current.Use();
					}
					break;
				case EventType.Repaint:
					{
						Styles.dropField.Draw(position, content, false, false, position.Contains(current.mousePosition), false);
					}
					break;
			}
		}
	}

	internal sealed class ParameterReferencePropertyEditor : PropertyEditor, IPropertyChanged
	{
		ParameterReferenceProperty _ParameterReferenceProperty = null;
		ParameterReferenceEditorGUI _EditorGUI = null;

		private DataSlot _DataSlot = null;

		private bool _IsSetCallback = false;
		private bool _IsDirtyCallback = false;

		protected override void OnInitialize()
		{
			base.OnInitialize();

			_ParameterReferenceProperty = new ParameterReferenceProperty(property, fieldInfo);
			_EditorGUI = new ParameterReferenceEditorGUI(_ParameterReferenceProperty);

			EnableConnectionChanged();
		}

		protected override void OnDestroy()
		{
			DisableConnectionChanged();
		}

		void EnableConnectionChanged()
		{
			_DataSlot = _ParameterReferenceProperty.slotProperty.slot;
			if (_DataSlot != null)
			{
				EditorCallbackUtility.RegisterPropertyChanged(this);

				_DataSlot.onConnectionChanged += OnConnectionChanged;

				_IsSetCallback = true;
			}
		}

		void DisableConnectionChanged()
		{
			if (_DataSlot != null)
			{
				if (_IsSetCallback)
				{
					_DataSlot.onConnectionChanged -= OnConnectionChanged;

					EditorCallbackUtility.UnregisterPropertyChanged(this);

					_IsSetCallback = false;
				}

				_DataSlot = null;
			}
		}

		void IPropertyChanged.OnPropertyChanged(PropertyChangedType propertyChangedType)
		{
			_IsDirtyCallback = true;
		}

		void UpdateCallback()
		{
			if (_IsDirtyCallback)
			{
				if (_IsSetCallback)
				{
					DisableConnectionChanged();
					EnableConnectionChanged();
				}

				_IsDirtyCallback = false;
			}
		}

		private bool _IsInGUI;

		void OnConnectionChanged(bool isConnect)
		{
			if (!property.IsValid())
			{
				return;
			}

			bool isInGUI = _IsInGUI || property.serializedObject.hasModifiedProperties;

			if (!isInGUI)
			{
				property.serializedObject.Update();
			}

			if (isConnect)
			{
				_ParameterReferenceProperty.type = ParameterReferenceType.DataSlot;
			}
			else if (_ParameterReferenceProperty.type == ParameterReferenceType.DataSlot && (ArborSettings.dataSlotShowMode == DataSlotShowMode.Outside || ArborSettings.dataSlotShowMode == DataSlotShowMode.Flexibly))
			{
				_ParameterReferenceProperty.type = ParameterReferenceType.Constant;
			}

			if (!isInGUI)
			{
				property.serializedObject.ApplyModifiedProperties();
			}
		}

		bool CheckType(Parameter parameter)
		{
			return _ParameterReferenceProperty.CheckType(parameter);
		}

		Rect DoGUI(Rect position, GUIContent label, bool isDraw)
		{
			_EditorGUI.HandleDragParameter();

			UpdateCallback();

			SerializedProperty containerProperty = _ParameterReferenceProperty.containerProperty;

			ParameterReferenceType parameterReferenceType = _ParameterReferenceProperty.type;

			Rect lineRect = new Rect(position);

			lineRect.height = EditorGUIUtility.singleLineHeight;

			if (isDraw)
			{
				EditorGUI.LabelField(lineRect, label);
			}

			lineRect.y += lineRect.height + EditorGUIUtility.standardVerticalSpacing;
			lineRect.height = 0;

			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel++;

			GUIContent containerLabel = EditorGUITools.GetTextContent(containerProperty.displayName);

			switch (parameterReferenceType)
			{
				case ParameterReferenceType.Constant:
					{
						lineRect.height = EditorGUI.GetPropertyHeight(containerProperty, containerLabel);
					}
					break;
				case ParameterReferenceType.DataSlot:
					{
						lineRect.height = EditorGUI.GetPropertyHeight(_ParameterReferenceProperty.slotProperty.property, containerLabel);
					}
					break;
			}

			if (isDraw)
			{
				Rect fieldPosition = EditorGUITools.SubtractDropdownWidth(lineRect);

				if (_ParameterReferenceProperty.IsShowOutsideSlot())
				{
					BehaviourEditorGUI.AddInputSlotLink(fieldPosition, _ParameterReferenceProperty.slotProperty.property);
				}

				switch (parameterReferenceType)
				{
					case ParameterReferenceType.Constant:
						{
							EditorGUI.PropertyField(fieldPosition, containerProperty, containerLabel);
						}
						break;
					case ParameterReferenceType.DataSlot:
						{
							EditorGUI.PropertyField(fieldPosition, _ParameterReferenceProperty.slotProperty.property, containerLabel);
						}
						break;
				}
			}

			Rect popupRect = EditorGUITools.GetDropdownRect(lineRect);

			EditorGUI.BeginChangeCheck();
			ParameterReferenceType newParameterReferenceType = EditorGUITools.EnumPopupUnIndent(popupRect, GUIContent.none, parameterReferenceType, Styles.shurikenDropDown);
			if (EditorGUI.EndChangeCheck())
			{
				if (parameterReferenceType == ParameterReferenceType.DataSlot)
				{
					_ParameterReferenceProperty.slotProperty.Disconnect();
				}
				_ParameterReferenceProperty.type = newParameterReferenceType;
			}

			lineRect.y += lineRect.height + EditorGUIUtility.standardVerticalSpacing;
			lineRect.height = EditorGUIUtility.singleLineHeight;

			if (isDraw)
			{
				_EditorGUI.ParameterField(lineRect);
			}

			lineRect.y += lineRect.height;
			lineRect.height = 0;

			//bool isDraggingParameter = ParameterReferenceEditorGUI.IsHoverableParameter(_ParameterReferenceProperty);
			bool isDraggingParameter = _EditorGUI.isDraggingParameter;
			if (isDraggingParameter)
			{
				lineRect.height = _EditorGUI.GetDropParameterHeight();

				if (isDraw)
				{
					_EditorGUI.DropParameter(lineRect);
				}

				lineRect.y += lineRect.height + EditorGUIUtility.standardVerticalSpacing;
				lineRect.height = 0;
			}

			EditorGUI.indentLevel = indentLevel;

			position.yMax = Mathf.Max(position.yMax, lineRect.yMax);

			return position;
		}

		protected override void OnGUI(Rect position, GUIContent label)
		{
			_IsInGUI = true;

			label = EditorGUI.BeginProperty(position, label, property);
			DoGUI(position, label, true);
			EditorGUI.EndProperty();

			_IsInGUI = false;
		}

		protected override float GetHeight(GUIContent label)
		{
			Rect position = DoGUI(new Rect(), label, false);
			return position.height;
		}
	}

	[CustomPropertyDrawer(typeof(ParameterReference), true)]
	internal sealed class ParameterReferencePropertyDrawer : PropertyEditorDrawer<ParameterReferencePropertyEditor>
	{
	}
}
