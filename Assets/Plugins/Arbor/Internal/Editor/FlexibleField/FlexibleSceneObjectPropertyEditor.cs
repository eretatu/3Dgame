//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	using Arbor;

	internal abstract class FlexibleSceneObjectPropertyEditor : PropertyEditor, IPropertyChanged
	{
		private bool _IsInGUI;

		protected FlexibleSceneObjectProperty flexibleProperty
		{
			get;
			private set;
		}

		private ParameterReferenceEditorGUI _ParameterReferenceEditorGUI = null;

		private DataSlot _DataSlot;

		private bool _IsSetCallback = false;
		private bool _IsDirtyCallback = false;

		void EnableConnectionChanged()
		{
			_DataSlot = flexibleProperty.slotProperty.slot;
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

		void SetType(FlexibleSceneObjectType type)
		{
			DisableConnectionChanged();

			flexibleProperty.type = type;

			EnableConnectionChanged();
		}

		protected virtual FlexibleSceneObjectProperty CreateProperty(SerializedProperty property)
		{
			return new FlexibleSceneObjectProperty(property);
		}

		protected override void OnInitialize()
		{
			flexibleProperty = CreateProperty(property);

			_ParameterReferenceEditorGUI = new ParameterReferenceEditorGUI(flexibleProperty.parameterProperty);

			EnableConnectionChanged();
		}

		protected override void OnDestroy()
		{
			DisableConnectionChanged();
		}

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

			FlexibleSceneObjectType type = flexibleProperty.type;
			FlexibleSceneObjectType newType = type;

			if (isConnect)
			{
				newType = FlexibleSceneObjectType.DataSlot;
			}
			else if (type == FlexibleSceneObjectType.DataSlot && (ArborSettings.dataSlotShowMode == DataSlotShowMode.Outside || ArborSettings.dataSlotShowMode == DataSlotShowMode.Flexibly))
			{
				newType = FlexibleSceneObjectType.Constant;
			}

			if (type != newType)
			{
				SetType(newType);
			}

			if (!isInGUI)
			{
				property.serializedObject.ApplyModifiedProperties();
			}
		}

		protected virtual void OnConstantGUI(Rect position, GUIContent label)
		{
			EditorGUI.PropertyField(position, flexibleProperty.valueProperty, label, true);
		}

		protected virtual void OnParameterGUI(Rect position, GUIContent label)
		{
			EditorGUI.PropertyField(position, flexibleProperty.parameterProperty.property, label, true);
		}

		protected virtual void OnDataSlotGUI(Rect position, GUIContent label)
		{
			EditorGUI.PropertyField(position, flexibleProperty.slotProperty.property, label, true);
		}

		protected virtual void OnHierarchyGUI(Rect position, GUIContent label)
		{
#if ARBOR_DEBUG
			Rect linePosition = position;
			linePosition.height = EditorGUI.GetPropertyHeight(flexibleProperty.hierarchyTypeProperty, label, true);
			EditorGUI.PropertyField(linePosition, flexibleProperty.hierarchyTypeProperty, label, true);

			linePosition.y += linePosition.height;

			FlexibleSceneObjectBase flexibleInstance = EditorGUITools.GetPropertyObject<FlexibleSceneObjectBase>(property);
			linePosition.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(linePosition, "Owner Object", (flexibleInstance.ownerObject == property.serializedObject.targetObject).ToString());
#else
			EditorGUI.PropertyField(position, flexibleProperty.hierarchyTypeProperty, label, true);
#endif
		}

		protected virtual float GetConstantHeight(GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(flexibleProperty.valueProperty, label, true);
		}

		protected virtual float GetParameterHeight(GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(flexibleProperty.parameterProperty.property, label, true);
		}

		protected virtual float GetDataSlotHeight(GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(flexibleProperty.slotProperty.property, label, true);
		}

		protected virtual float GetHierarchyHeight(GUIContent label)
		{
#if ARBOR_DEBUG
			float height = EditorGUI.GetPropertyHeight(flexibleProperty.hierarchyTypeProperty, label, true);

			height += EditorGUIUtility.singleLineHeight;

			return height;
#else
			return EditorGUI.GetPropertyHeight(flexibleProperty.hierarchyTypeProperty, label, true);
#endif
		}

		protected abstract System.Type GetConstantObjectType();

		protected Object ValidateObjectFieldAssignment(Object obj)
		{
			System.Type objType = GetConstantObjectType();
			if (objType.IsAssignableFrom(obj.GetType()))
			{
				return obj;
			}
			else if (obj is GameObject && typeof(Component).IsAssignableFrom(objType))
			{
				GameObject go = obj as GameObject;
				return go.GetComponent(objType);
			}

			return null;
		}

		bool IsHoverableObject()
		{
			var objectReferences = DragAndDrop.objectReferences;
			for (int objectIndex = 0; objectIndex < objectReferences.Length; objectIndex++)
			{
				Object draggedObject = objectReferences[objectIndex];
				if (draggedObject != null && ValidateObjectFieldAssignment(draggedObject) != null)
				{
					return true;
				}
			}

			return false;
		}

		float GetDropObjectHeight()
		{
			if (!IsHoverableObject())
			{
				return 0f;
			}

			return Styles.dropField.CalcSize(EditorContents.dropObject).y;
		}

		void DropConstantObject(Rect position)
		{
			if (!IsHoverableObject())
			{
				return;
			}

			position = EditorGUI.IndentedRect(position);

			GUIContent content = EditorContents.dropObject;

			Event current = Event.current;
			switch (current.type)
			{
				case EventType.DragUpdated:
				case EventType.DragPerform:
					if (position.Contains(current.mousePosition))
					{
						var objectReferences = DragAndDrop.objectReferences;
						for (int objectIndex = 0; objectIndex < objectReferences.Length; objectIndex++)
						{
							Object draggedObject = objectReferences[objectIndex];
							if (draggedObject != null)
							{
								Object obj = ValidateObjectFieldAssignment(draggedObject);
								if (obj != null)
								{
									DragAndDrop.visualMode = DragAndDropVisualMode.Link;

									if (current.type == EventType.DragPerform)
									{
										flexibleProperty.valueProperty.objectReferenceValue = obj;
										SetType(FlexibleSceneObjectType.Constant);

										DragAndDrop.AcceptDrag();
										DragAndDrop.activeControlID = 0;
									}
								}
								else
								{
									DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
								}

								current.Use();

								break;
							}
						}
					}
					break;
				case EventType.Repaint:
					{
						Styles.dropField.Draw(position, content, false, false, position.Contains(current.mousePosition), false);
					}
					break;
			}
		}

		protected override void OnGUI(Rect position, GUIContent label)
		{
			UpdateCallback();

			_IsInGUI = true;

			label = EditorGUI.BeginProperty(position, label, property);

			_ParameterReferenceEditorGUI.HandleDragParameter();

			FlexibleSceneObjectType type = flexibleProperty.type;

			Rect fieldAreaPosition = position;

			fieldAreaPosition.height = GetFieldAreaHeight(label);

			position.yMin += fieldAreaPosition.height;

			Rect fieldPosition = EditorGUITools.SubtractDropdownWidth(fieldAreaPosition);

			if (flexibleProperty.IsShowOutsideSlot())
			{
				BehaviourEditorGUI.AddInputSlotLink(fieldPosition, flexibleProperty.slotProperty.property);
			}

			switch (type)
			{
				case FlexibleSceneObjectType.Constant:
					OnConstantGUI(fieldPosition, label);
					break;
				case FlexibleSceneObjectType.Parameter:
					OnParameterGUI(fieldPosition, label);
					break;
				case FlexibleSceneObjectType.DataSlot:
					OnDataSlotGUI(fieldPosition, label);
					break;
				case FlexibleSceneObjectType.Hierarchy:
					OnHierarchyGUI(fieldPosition, label);
					break;
			}

			Rect popupRect = EditorGUITools.GetDropdownRect(fieldAreaPosition);

			EditorGUI.BeginChangeCheck();
			FlexibleSceneObjectType newType = EditorGUITools.EnumPopupUnIndent(popupRect, GUIContent.none, type, Styles.shurikenDropDown);
			if (EditorGUI.EndChangeCheck())
			{
				SetType(newType);
			}

			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel++;

			if (type != FlexibleSceneObjectType.Constant)
			{
				EditorGUI.BeginChangeCheck();
				DropConstantObject(position);
				if (EditorGUI.EndChangeCheck())
				{
					SetType(FlexibleSceneObjectType.Constant);
				}
			}

			if (type != FlexibleSceneObjectType.Parameter)
			{
				EditorGUI.BeginChangeCheck();
				_ParameterReferenceEditorGUI.DropParameter(position);
				if (EditorGUI.EndChangeCheck())
				{
					SetType(FlexibleSceneObjectType.Parameter);
				}
			}

			EditorGUI.indentLevel = indentLevel;

			EditorGUI.EndProperty();

			_IsInGUI = false;
		}

		float GetFieldAreaHeight(GUIContent label)
		{
			float height = EditorGUIUtility.singleLineHeight;

			FlexibleSceneObjectType type = flexibleProperty.type;

			switch (type)
			{
				case FlexibleSceneObjectType.Constant:
					height = GetConstantHeight(label);
					break;
				case FlexibleSceneObjectType.Parameter:
					height = GetParameterHeight(label);
					break;
				case FlexibleSceneObjectType.DataSlot:
					height = GetDataSlotHeight(label);
					break;
				case FlexibleSceneObjectType.Hierarchy:
					height = GetHierarchyHeight(label);
					break;
			}

			return height;
		}

		protected override float GetHeight(GUIContent label)
		{
			_ParameterReferenceEditorGUI.HandleDragParameter();

			float height = GetFieldAreaHeight(label);

			FlexibleSceneObjectType type = flexibleProperty.type;

			if (type != FlexibleSceneObjectType.Constant)
			{
				height += GetDropObjectHeight();
			}

			if (type != FlexibleSceneObjectType.Parameter)
			{
				height += _ParameterReferenceEditorGUI.GetDropParameterHeight();
			}

			return height;
		}
	}
}