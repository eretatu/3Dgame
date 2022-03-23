//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace ArborEditor
{
	using Arbor;

	[CustomEditor(typeof(ParameterContainerInternal), true)]
	internal sealed class ParameterContainerInternalInspector : Editor, IPropertyChanged
	{
		static readonly string s_InvalidParameterMessage = "Invalid Parameter";

		private static readonly int s_DragParameterHash = "s_DragParameterHash".GetHashCode();
		private static readonly GUIContent s_DragContent = new GUIContent();

		internal bool isInParametersPanel = false;
		internal event System.Action onRemoveComponent = null;

		static bool IsTypeObject(Parameter.Type type)
		{
			switch (type)
			{
				case Parameter.Type.GameObject:
				case Parameter.Type.Transform:
				case Parameter.Type.RectTransform:
				case Parameter.Type.Rigidbody:
				case Parameter.Type.Rigidbody2D:
				case Parameter.Type.Component:
				case Parameter.Type.AssetObject:
				case Parameter.Type.Variable:
				case Parameter.Type.VariableList:
					return true;
			}

			return false;
		}

		static System.Type GetObjectType(Parameter.Type type)
		{
			switch (type)
			{
				case Parameter.Type.GameObject:
					return typeof(GameObject);
				case Parameter.Type.Transform:
					return typeof(Transform);
				case Parameter.Type.RectTransform:
					return typeof(RectTransform);
				case Parameter.Type.Rigidbody:
					return typeof(Rigidbody);
				case Parameter.Type.Rigidbody2D:
					return typeof(Rigidbody2D);
				case Parameter.Type.Component:
					return typeof(Component);
				case Parameter.Type.AssetObject:
					return typeof(Object);
				case Parameter.Type.Variable:
					return typeof(VariableBase);
				case Parameter.Type.VariableList:
					return typeof(VariableListBase);
				default:
					throw new System.ArgumentException("Parameter type not an Object type(" + type + ")");
			}
		}

		ParameterContainerInternal _ParameterContainer;
		ReorderableListEx _ParametersList;
		ReorderableListEx _SearchParametersList;

		private SerializedProperty _ParameterListProperty;

		private SerializedProperty _IntParameters;
		private SerializedProperty _LongParameters;
		private SerializedProperty _FloatParameters;
		private SerializedProperty _BoolParameters;
		private SerializedProperty _StringParameters;
		private SerializedProperty _Vector2Parameters;
		private SerializedProperty _Vector3Parameters;
		private SerializedProperty _QuaternionParameters;
		private SerializedProperty _RectParameters;
		private SerializedProperty _BoundsParameters;
		private SerializedProperty _ColorParameters;
		private SerializedProperty _Vector4Parameters;
		private SerializedProperty _Vector2IntParameters;
		private SerializedProperty _Vector3IntParameters;
		private SerializedProperty _RectIntParameters;
		private SerializedProperty _BoundsIntParameters;
		private SerializedProperty _ObjectParameters;

		Dictionary<Object, SerializedObject> _VariableParameterObjects = new Dictionary<Object, SerializedObject>();

		private SerializedProperty _IntListParameters;
		private SerializedProperty _LongListParameters;
		private SerializedProperty _FloatListParameters;
		private SerializedProperty _BoolListParameters;
		private SerializedProperty _StringListParameters;
		private SerializedProperty _EnumListParameters;
		private SerializedProperty _Vector2ListParameters;
		private SerializedProperty _Vector3ListParameters;
		private SerializedProperty _QuaternionListParameters;
		private SerializedProperty _RectListParameters;
		private SerializedProperty _BoundsListParameters;
		private SerializedProperty _ColorListParameters;
		private SerializedProperty _Vector4ListParameters;
		private SerializedProperty _Vector2IntListParameters;
		private SerializedProperty _Vector3IntListParameters;
		private SerializedProperty _RectIntListParameters;
		private SerializedProperty _BoundsIntListParameters;
		private SerializedProperty _GameObjectListParameters;
		private SerializedProperty _ComponentListParameters;
		private SerializedProperty _AssetObjectListParameters;

		private PropertyHeightCache _PropertyHeightCache = new PropertyHeightCache();
		private LayoutArea _LayoutArea = new LayoutArea();

		SerializedProperty parameterListProperty
		{
			get
			{
				if (_ParameterListProperty == null)
				{
					_ParameterListProperty = serializedObject.FindProperty("_Parameters");
				}
				return _ParameterListProperty;
			}
		}

		ReorderableListEx parameterList
		{
			get
			{
				int[] activeElements = this.activeElements;
				if (activeElements != null)
				{
					if (_SearchParametersList == null)
					{
						_SearchParametersList = new ReorderableListEx(activeElements, typeof(int), false, false, false, false)
						{
							headerHeight = 0f,
							footerHeight = 0f,
							elementHeightCallback = GetElementHeight,
							drawElementCallback = DrawElement,
						};
					}

					_SearchParametersList.list = activeElements;

					return _SearchParametersList;
				}
				if (_ParametersList == null)
				{
					_ParametersList = new ReorderableListEx(serializedObject, parameterListProperty, true, false, false, false)
					{
						headerHeight = 0f,
						footerHeight = 0f,
						elementHeightCallback = GetElementHeight,
						drawElementCallback = DrawElement,
					};
				}

				_ParametersList.serializedProperty = parameterListProperty;
				_ParametersList.draggable = parameterListProperty.editable;

				return _ParametersList;
			}
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnEnable()
		{
			_ParameterContainer = target as ParameterContainerInternal;
			_ParameterContainer.Refresh();

			_IntParameters = serializedObject.FindProperty("_IntParameters");
			_LongParameters = serializedObject.FindProperty("_LongParameters");
			_FloatParameters = serializedObject.FindProperty("_FloatParameters");
			_BoolParameters = serializedObject.FindProperty("_BoolParameters");
			_StringParameters = serializedObject.FindProperty("_StringParameters");
			_Vector2Parameters = serializedObject.FindProperty("_Vector2Parameters");
			_Vector3Parameters = serializedObject.FindProperty("_Vector3Parameters");
			_QuaternionParameters = serializedObject.FindProperty("_QuaternionParameters");
			_RectParameters = serializedObject.FindProperty("_RectParameters");
			_BoundsParameters = serializedObject.FindProperty("_BoundsParameters");
			_ColorParameters = serializedObject.FindProperty("_ColorParameters");
			_Vector4Parameters = serializedObject.FindProperty("_Vector4Parameters");
			_Vector2IntParameters = serializedObject.FindProperty("_Vector2IntParameters");
			_Vector3IntParameters = serializedObject.FindProperty("_Vector3IntParameters");
			_RectIntParameters = serializedObject.FindProperty("_RectIntParameters");
			_BoundsIntParameters = serializedObject.FindProperty("_BoundsIntParameters");
			_ObjectParameters = serializedObject.FindProperty("_ObjectParameters");

			_IntListParameters = serializedObject.FindProperty("_IntListParameters");
			_LongListParameters = serializedObject.FindProperty("_LongListParameters");
			_FloatListParameters = serializedObject.FindProperty("_FloatListParameters");
			_BoolListParameters = serializedObject.FindProperty("_BoolListParameters");
			_StringListParameters = serializedObject.FindProperty("_StringListParameters");
			_EnumListParameters = serializedObject.FindProperty("_EnumListParameters");
			_Vector2ListParameters = serializedObject.FindProperty("_Vector2ListParameters");
			_Vector3ListParameters = serializedObject.FindProperty("_Vector3ListParameters");
			_QuaternionListParameters = serializedObject.FindProperty("_QuaternionListParameters");
			_RectListParameters = serializedObject.FindProperty("_RectListParameters");
			_BoundsListParameters = serializedObject.FindProperty("_BoundsListParameters");
			_ColorListParameters = serializedObject.FindProperty("_ColorListParameters");
			_Vector4ListParameters = serializedObject.FindProperty("_Vector4ListParameters");
			_Vector2IntListParameters = serializedObject.FindProperty("_Vector2IntListParameters");
			_Vector3IntListParameters = serializedObject.FindProperty("_Vector3IntListParameters");
			_RectIntListParameters = serializedObject.FindProperty("_RectIntListParameters");
			_BoundsIntListParameters = serializedObject.FindProperty("_BoundsIntListParameters");
			_GameObjectListParameters = serializedObject.FindProperty("_GameObjectListParameters");
			_ComponentListParameters = serializedObject.FindProperty("_ComponentListParameters");
			_AssetObjectListParameters = serializedObject.FindProperty("_AssetObjectListParameters");

			EditorCallbackUtility.RegisterPropertyChanged(this);
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		private void OnDisable()
		{
			EditorCallbackUtility.UnregisterPropertyChanged(this);
		}

		void OnAddDropdown(Rect buttonRect, ReorderableList list)
		{
			GenericMenu genericMenu = new GenericMenu();
			for (int i = 0, count = ParameterTypeMenuItem.menuItems.Length; i < count; ++i)
			{
				var menuItem = ParameterTypeMenuItem.menuItems[i];
				if (menuItem.isSeparator)
				{
					genericMenu.AddSeparator(menuItem.content.text);
				}
				else
				{
					switch (menuItem.type)
					{
						case Parameter.Type.Variable:
							VariableEditorUtility.GenerateVariableMenus(genericMenu, null, true, AddVariableMenu);
							break;
						case Parameter.Type.VariableList:
							VariableEditorUtility.GenerateVariableListMenus(genericMenu, null, true, AddVariableListMenu);
							break;
						default:
							genericMenu.AddItem(menuItem.content, false, AddParameterMenu, menuItem.type);
							break;
					}
				}
			}
			genericMenu.DropDown(buttonRect);
		}

		void OnRemove(ReorderableList list)
		{
			int elementIndex = GetElementIndex(list.index);
			Parameter parameter = _ParameterContainer.GetParameterFromIndex(elementIndex);

			switch (parameter.type)
			{
				case Parameter.Type.Variable:
					RemoveVariableSerializedObject(parameter.variableObject);
					break;
				case Parameter.Type.VariableList:
					RemoveVariableSerializedObject(parameter.variableListObject);
					break;
			}

			EditorWindow focusedWindow = EditorWindow.focusedWindow;

			EditorApplication.delayCall += () =>
			{
				_ParameterContainer.DeleteParam(parameter);

				_SearchElements = null;
				if (focusedWindow != null)
				{
					focusedWindow.Repaint();
				}
			};
		}

		SerializedObject GetVariableSerializedObject(Object obj)
		{
			SerializedObject serializedObject = null;
			if ((object)obj != null && !_VariableParameterObjects.TryGetValue(obj, out serializedObject))
			{
				serializedObject = new SerializedObject(obj);
				_VariableParameterObjects.Add(obj, serializedObject);
			}

			return serializedObject;
		}

		void RemoveVariableSerializedObject(Object obj)
		{
			SerializedObject serializedObject = null;
			if ((object)obj != null && _VariableParameterObjects.TryGetValue(obj, out serializedObject))
			{
				serializedObject.Dispose();
				_VariableParameterObjects.Remove(obj);
			}
		}

		void ClearCache()
		{
			_PropertyHeightCache.Clear();
		}

		SerializedProperty GetParametersProperty(Parameter.Type type)
		{
			switch (type)
			{
				case Parameter.Type.Int:
				case Parameter.Type.Enum:
					return _IntParameters;
				case Parameter.Type.Long:
					return _LongParameters;
				case Parameter.Type.Float:
					return _FloatParameters;
				case Parameter.Type.Bool:
					return _BoolParameters;
				case Parameter.Type.String:
					return _StringParameters;
				case Parameter.Type.Vector2:
					return _Vector2Parameters;
				case Parameter.Type.Vector3:
					return _Vector3Parameters;
				case Parameter.Type.Quaternion:
					return _QuaternionParameters;
				case Parameter.Type.Rect:
					return _RectParameters;
				case Parameter.Type.Bounds:
					return _BoundsParameters;
				case Parameter.Type.Color:
					return _ColorParameters;
				case Parameter.Type.Vector4:
					return _Vector4Parameters;
				case Parameter.Type.Vector2Int:
					return _Vector2IntParameters;
				case Parameter.Type.Vector3Int:
					return _Vector3IntParameters;
				case Parameter.Type.RectInt:
					return _RectIntParameters;
				case Parameter.Type.BoundsInt:
					return _BoundsIntParameters;
				case Parameter.Type.Transform:
				case Parameter.Type.RectTransform:
				case Parameter.Type.Rigidbody:
				case Parameter.Type.Rigidbody2D:
				case Parameter.Type.Component:
				case Parameter.Type.Variable:
				case Parameter.Type.VariableList:
				case Parameter.Type.GameObject:
				case Parameter.Type.AssetObject:
					return _ObjectParameters;
				case Parameter.Type.IntList:
					return _IntListParameters;
				case Parameter.Type.LongList:
					return _LongListParameters;
				case Parameter.Type.FloatList:
					return _FloatListParameters;
				case Parameter.Type.BoolList:
					return _BoolListParameters;
				case Parameter.Type.StringList:
					return _StringListParameters;
				case Parameter.Type.EnumList:
					return _EnumListParameters;
				case Parameter.Type.Vector2List:
					return _Vector2ListParameters;
				case Parameter.Type.Vector3List:
					return _Vector3ListParameters;
				case Parameter.Type.QuaternionList:
					return _QuaternionListParameters;
				case Parameter.Type.RectList:
					return _RectListParameters;
				case Parameter.Type.BoundsList:
					return _BoundsListParameters;
				case Parameter.Type.ColorList:
					return _ColorListParameters;
				case Parameter.Type.Vector4List:
					return _Vector4ListParameters;
				case Parameter.Type.Vector2IntList:
					return _Vector2IntListParameters;
				case Parameter.Type.Vector3IntList:
					return _Vector3IntListParameters;
				case Parameter.Type.RectIntList:
					return _RectIntListParameters;
				case Parameter.Type.BoundsIntList:
					return _BoundsIntListParameters;
				case Parameter.Type.GameObjectList:
					return _GameObjectListParameters;
				case Parameter.Type.ComponentList:
					return _ComponentListParameters;
				case Parameter.Type.AssetObjectList:
					return _AssetObjectListParameters;
				default:
					throw new System.NotImplementedException("It is an unimplemented Parameter type(" + type + ")");
			}
		}

		void DragParameter(Rect dragRect, SerializedProperty property)
		{
			ParameterContainerInternal container = property.FindPropertyRelative("container").objectReferenceValue as ParameterContainerInternal;
			Parameter parameter = container.GetParam(property.FindPropertyRelative("id").intValue);

			dragRect.xMin = dragRect.xMax - 18f;

			GUIStyle dragStyle = ArborEditor.Styles.dropField;
			GUIStyle dragPinStyle = ArborEditor.Styles.GetDataInPin(parameter.valueType);

			GUIContent content = s_DragContent;
			content.tooltip = Localization.GetWord("Drag to place");

			content = EditorGUI.BeginProperty(dragRect, content, property);

			int controlID = GUIUtility.GetControlID(s_DragParameterHash, FocusType.Passive, dragRect);

			Event current = Event.current;

			EventType eventType = current.GetTypeForControl(controlID);
			switch (eventType)
			{
				case EventType.MouseDown:
					if (dragRect.Contains(current.mousePosition))
					{
						DragAndDrop.PrepareStartDrag();

						ParameterDraggingObject draggingObject = ParameterDraggingObject.instance;

						draggingObject.parameter = parameter;

						DragAndDrop.objectReferences = new Object[] { draggingObject };
						DragAndDrop.paths = null;
						DragAndDrop.StartDrag(property.FindPropertyRelative("name").stringValue);

						current.Use();
					}
					break;
				case EventType.Repaint:
					Color backgroundColor = GUI.backgroundColor;
					GUI.backgroundColor = EditorGUITools.GetTypeColor(parameter.valueType);

					dragStyle.Draw(dragRect, GUIContent.none, controlID);
					dragPinStyle.Draw(DataSlotGUI.Defaults.dataPinPadding.Remove(dragRect), content, controlID);

					GUI.backgroundColor = backgroundColor;
					break;
			}

			EditorGUI.EndProperty();
		}

		private sealed class VariableListEditor : ListParameterEditorBase
		{
			protected override sealed SerializedProperty GetListProperty()
			{
				return property;
			}
		}

		void DoElementGUI(SerializedProperty property, bool isLast)
		{
			SerializedProperty nameProperty = property.FindPropertyRelative("name");

			SerializedProperty parameterIndexProperty = property.FindPropertyRelative("_ParameterIndex");
			int parameterIndex = parameterIndexProperty.intValue;

			Parameter.Type type = EnumUtility.GetValueFromIndex<Parameter.Type>(property.FindPropertyRelative("type").enumValueIndex);
			SerializedProperty parametersProperty = GetParametersProperty(type);

			SerializedProperty valueProperty = null;
			if (parametersProperty != null && 0 <= parameterIndex && parameterIndex < parametersProperty.arraySize)
			{
				valueProperty = parametersProperty.GetArrayElementAtIndex(parameterIndex);
			}

			if (_LayoutArea.rect.width - EditorGUIUtility.labelWidth < EditorGUIUtility.fieldWidth)
			{
				EditorGUIUtility.labelWidth = _LayoutArea.rect.width - EditorGUIUtility.fieldWidth;
			}

			string label = string.Empty;
			if ((type == Parameter.Type.Variable || type == Parameter.Type.VariableList) && valueProperty != null)
			{
				Object variableObj = valueProperty.objectReferenceValue;
				if (variableObj != null)
				{
					var behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(variableObj);
					label = behaviourInfo.titleContent.text;
				}
				else
				{
					label = "Invalid";
				}
			}
			else
			{
				label = System.Enum.GetName(typeof(Parameter.Type), type);
			}

			float verticalSpacing = Mathf.Floor((parameterList.elementHeight - EditorGUIUtility.singleLineHeight) * 0.5f) - 2f;

			_LayoutArea.Space(verticalSpacing);

			_LayoutArea.BeginHorizontal();

			_LayoutArea.TextField(EditorGUITools.GetTextContent(label), nameProperty, LayoutArea.Width(_LayoutArea.rect.width - 20f));

			Rect dragRect = _LayoutArea.GetRect(0f, EditorGUIUtility.singleLineHeight);

			if (_LayoutArea.IsDraw(dragRect))
			{
				DragParameter(dragRect, property);
			}

			_LayoutArea.EndHorizontal();

			_LayoutArea.Space(verticalSpacing);

			GUIContent valueContent = EditorGUITools.GetTextContent("Value");

			if (valueProperty != null)
			{
				if (type == Parameter.Type.Enum)
				{
					ClassTypeReferenceProperty referenceTypeProperty = new ClassTypeReferenceProperty(property.FindPropertyRelative("referenceType"));

					referenceTypeProperty.SetConstraint(ClassTypeConstraintEditorUtility.enumField);

					_LayoutArea.PropertyField(referenceTypeProperty.property, GUIContent.none, true);

					System.Type enumType = referenceTypeProperty.type;

					if (!EnumFieldUtility.IsEnum(enumType))
					{
						_LayoutArea.PropertyField(valueProperty, valueContent, true);
					}
					else
					{
						object enumValue = System.Enum.ToObject(enumType, valueProperty.intValue);
						if (AttributeHelper.HasAttribute<System.FlagsAttribute>(enumType))
						{
							enumValue = _LayoutArea.EnumMaskField(valueContent, (System.Enum)enumValue);
						}
						else
						{
							enumValue = _LayoutArea.EnumPopup(valueContent, (System.Enum)enumValue);
						}
						valueProperty.intValue = (int)enumValue;
					}
				}
				else if (type == Parameter.Type.Variable)
				{
					VariableBase variable = valueProperty.objectReferenceValue as VariableBase;
					if (variable != null)
					{
						SerializedObject serializedObject = GetVariableSerializedObject(variable);

						serializedObject.Update();

						SerializedProperty parameterProperty = serializedObject.FindProperty("_Parameter");

						if (parameterProperty != null)
						{
							_LayoutArea.PropertyField(parameterProperty, valueContent, true);
						}

						serializedObject.ApplyModifiedProperties();
					}
					else
					{
						_LayoutArea.HelpBox(s_InvalidParameterMessage, MessageType.Error);
					}
				}
				else if (type == Parameter.Type.EnumList)
				{
					ClassTypeReferenceProperty referenceTypeProperty = new ClassTypeReferenceProperty(property.FindPropertyRelative("referenceType"));

					referenceTypeProperty.SetConstraint(ClassTypeConstraintEditorUtility.enumField);

					_LayoutArea.PropertyField(referenceTypeProperty.property, GUIContent.none, true);

					System.Type enumType = referenceTypeProperty.type;

					bool isEnum = enumType != null && TypeUtility.IsEnum(enumType);

					if (isEnum)
					{
						valueProperty.SetStateData<System.Type>(enumType);

						_LayoutArea.PropertyField(valueProperty, valueContent, true);
					}
					else
					{
						_LayoutArea.HelpBox(Localization.GetWord("ParameterContainer.SelectReferenceType"), MessageType.Warning);
					}
				}
				else if (type == Parameter.Type.ComponentList)
				{
					ClassTypeReferenceProperty referenceTypeProperty = new ClassTypeReferenceProperty(property.FindPropertyRelative("referenceType"));

					referenceTypeProperty.SetConstraint(ClassTypeConstraintEditorUtility.component);

					System.Type objectType = referenceTypeProperty.type ?? typeof(Component);

					EditorGUI.BeginChangeCheck();
					_LayoutArea.PropertyField(referenceTypeProperty.property, GUIContent.none, true);
					if (EditorGUI.EndChangeCheck())
					{
						objectType = referenceTypeProperty.type ?? objectType;

						SerializedProperty listProperty = valueProperty.FindPropertyRelative("list");
						for (int i = 0; i < listProperty.arraySize; i++)
						{
							SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(i);
							if (elementProperty.objectReferenceValue != null)
							{
								System.Type valueType = elementProperty.objectReferenceValue.GetType();
								if (!objectType.IsAssignableFrom(valueType))
								{
									elementProperty.objectReferenceValue = null;
								}
							}
						}
					}

					valueProperty.SetStateData<System.Type>(objectType);

					_LayoutArea.PropertyField(valueProperty, valueContent, true);
				}
				else if (type == Parameter.Type.AssetObjectList)
				{
					ClassTypeReferenceProperty referenceTypeProperty = new ClassTypeReferenceProperty(property.FindPropertyRelative("referenceType"));

					referenceTypeProperty.SetConstraint(ClassTypeConstraintEditorUtility.asset);

					System.Type objectType = referenceTypeProperty.type ?? typeof(Object);

					EditorGUI.BeginChangeCheck();
					_LayoutArea.PropertyField(referenceTypeProperty.property, GUIContent.none, true);
					if (EditorGUI.EndChangeCheck())
					{
						objectType = referenceTypeProperty.type ?? objectType;

						SerializedProperty listProperty = valueProperty.FindPropertyRelative("list");
						for (int i = 0; i < listProperty.arraySize; i++)
						{
							SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(i);
							if (elementProperty.objectReferenceValue != null)
							{
								System.Type valueType = elementProperty.objectReferenceValue.GetType();
								if (!objectType.IsAssignableFrom(valueType))
								{
									elementProperty.objectReferenceValue = null;
								}
							}
						}
					}

					valueProperty.SetStateData<System.Type>(objectType);

					_LayoutArea.PropertyField(valueProperty, valueContent, true);
				}
				else if (type == Parameter.Type.VariableList)
				{
					VariableListBase variable = valueProperty.objectReferenceValue as VariableListBase;
					if (variable != null)
					{
						SerializedObject serializedObject = GetVariableSerializedObject(variable);

						serializedObject.Update();

						SerializedProperty parameterProperty = serializedObject.FindProperty("_Parameter");

						if (parameterProperty != null)
						{
							System.Type fieldType;
							var fieldInfo = EditorGUITools.GetFieldInfoFromProperty(parameterProperty, out fieldType);
							PropertyEditor propertyEditor = PropertyEditorUtility.GetPropertyEditor<VariableListEditor>(parameterProperty, fieldInfo);

							if (propertyEditor != null)
							{
								float height = propertyEditor.DoGetHeight(valueContent);
								Rect rect = _LayoutArea.GetRect(0, height);

								if (_LayoutArea.IsDraw(rect))
								{
									propertyEditor.DoOnGUI(rect, valueContent);
								}
							}
							else
							{
								_LayoutArea.PropertyField(parameterProperty, valueContent, true);
							}
						}

						serializedObject.ApplyModifiedProperties();
					}
					else
					{
						_LayoutArea.HelpBox(s_InvalidParameterMessage, MessageType.Error);
					}
				}
				else if (IsTypeObject(type))
				{
					System.Type objectType = GetObjectType(type);
					bool allowSceneObjects = true;

					if (type == Parameter.Type.Component || type == Parameter.Type.AssetObject)
					{
						ClassTypeReferenceProperty referenceTypeProperty = new ClassTypeReferenceProperty(property.FindPropertyRelative("referenceType"));

						if (type == Parameter.Type.Component)
						{
							referenceTypeProperty.SetConstraint(ClassTypeConstraintEditorUtility.component);
						}
						else if (type == Parameter.Type.AssetObject)
						{
							referenceTypeProperty.SetConstraint(ClassTypeConstraintEditorUtility.asset);
							allowSceneObjects = false;
						}

						objectType = referenceTypeProperty.type ?? objectType;

						EditorGUI.BeginChangeCheck();
						_LayoutArea.PropertyField(referenceTypeProperty.property, GUIContent.none, true);
						if (EditorGUI.EndChangeCheck())
						{
							objectType = referenceTypeProperty.type ?? objectType;

							if (valueProperty.objectReferenceValue != null)
							{
								System.Type valueType = valueProperty.objectReferenceValue.GetType();
								if (!objectType.IsAssignableFrom(valueType))
								{
									valueProperty.objectReferenceValue = null;
								}
							}
						}
					}

					_LayoutArea.ObjectField(valueContent, valueProperty, objectType, allowSceneObjects);
				}
				else
				{
					_LayoutArea.PropertyField(valueProperty, valueContent, true);
				}
			}
			else
			{
				_LayoutArea.LabelField(valueContent, EditorGUITools.GetTextContent("Error: not found value"));
			}

			if (isInParametersPanel)
			{
				_LayoutArea.BeginHorizontal();

				SerializedProperty isPublicSetProperty = property.FindPropertyRelative("_IsPublicSet");
				SerializedProperty isPublicGetProperty = property.FindPropertyRelative("_IsPublicGet");

				_LayoutArea.VisibilityToggle(EditorGUITools.GetTextContent("Set"), isPublicSetProperty, LayoutArea.Width(50f));
				_LayoutArea.VisibilityToggle(EditorGUITools.GetTextContent("Get"), isPublicGetProperty, LayoutArea.Width(50f));

				_LayoutArea.EndHorizontal();
			}

			EditorGUIUtility.labelWidth = 0f;
		}

		float GetElementHeight(int index)
		{
			int elementIndex = GetElementIndex(index);
			SerializedProperty property = parameterListProperty.GetArrayElementAtIndex(elementIndex);

			float height = 0f;
			if (!_PropertyHeightCache.TryGetHeight(property, out height))
			{
				_LayoutArea.Begin(new Rect(), true, new RectOffset(0, 0, 0, 2));

				DoElementGUI(property, parameterList.count - 1 == index);

				_LayoutArea.End();

				height = _LayoutArea.rect.height;

				_PropertyHeightCache.AddHeight(property, height);
			}

			return height;
		}

		void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			_LayoutArea.Begin(rect, false, new RectOffset(0, 0, 0, 2));

			int elementIndex = GetElementIndex(index);
			SerializedProperty property = parameterListProperty.GetArrayElementAtIndex(elementIndex);
			DoElementGUI(property, parameterList.count - 1 == index);

			_LayoutArea.End();
		}

		void AddParameterMenu(object value)
		{
			Undo.RecordObject(_ParameterContainer, "Parameter Added");
			Parameter.Type type = (Parameter.Type)value;
			_ParameterContainer.AddParam("New " + type.ToString(), type);
		}

		void AddVariableMenu(System.Type classType)
		{
			Undo.IncrementCurrentGroup();

			VariableBase variable = VariableBase.Create(_ParameterContainer, classType) as VariableBase;

			var behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(classType);

			Undo.RecordObject(_ParameterContainer, "Parameter Added");
			Parameter parameter = _ParameterContainer.AddParam("New " + behaviourInfo.titleContent.text, Parameter.Type.Variable);

			parameter.variableObject = variable;

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
		}

		void AddVariableMenu(object obj)
		{
			System.Type classType = (System.Type)obj;

			AddVariableMenu(classType);
		}

		void AddVariableListMenu(System.Type classType)
		{
			Undo.IncrementCurrentGroup();

			VariableListBase variable = VariableListBase.Create(_ParameterContainer, classType) as VariableListBase;

			var behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(classType);

			Undo.RecordObject(_ParameterContainer, "Parameter Added");
			Parameter parameter = _ParameterContainer.AddParam("New " + behaviourInfo.titleContent.text, Parameter.Type.VariableList);

			parameter.variableListObject = variable;

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
		}

		void AddVariableListMenu(object obj)
		{
			System.Type classType = (System.Type)obj;

			AddVariableListMenu(classType);
		}

		enum SearchMode
		{
			Name,
			Type,
		}

		private string _SearchText = "";
		private SearchMode _SearchMode = SearchMode.Name;
		private Parameter.Type _SearchParameterType = Parameter.Type.Int;
		private Vector2 _ScrollPos = Vector2.zero;

		private int[] _SearchElements = null;

		int GetElementIndex(int index)
		{
			int[] elements = activeElements;
			if (elements != null)
			{
				return elements[index];
			}

			return index;
		}

		void CreateSearchParameterList(string searchText, SearchMode searchMode, Parameter.Type searchParameterType)
		{
			string[] strArray = searchText.ToLower().Split(' ');
			List<int> elementList1 = new List<int>();
			List<int> elementList2 = new List<int>();

			for (int parameterIndex = 0; parameterIndex < parameterListProperty.arraySize; parameterIndex++)
			{
				SerializedProperty parameterProperty = parameterListProperty.GetArrayElementAtIndex(parameterIndex);

				SerializedProperty typeProperty = parameterProperty.FindPropertyRelative("type");

				Parameter.Type type = EnumUtility.GetValueFromIndex<Parameter.Type>(typeProperty.enumValueIndex);

				if (searchMode == SearchMode.Name || (searchMode == SearchMode.Type && searchParameterType == type))
				{
					SerializedProperty nameProperty = parameterProperty.FindPropertyRelative("name");

					string name = nameProperty.stringValue;

					string str1 = name.ToLower().Replace(" ", string.Empty);
					bool flag1 = true;
					bool flag2 = false;
					for (int searchIndex = 0; searchIndex < strArray.Length; ++searchIndex)
					{
						string str2 = strArray[searchIndex];
						if (str1.Contains(str2))
						{
							if (searchIndex == 0 && str1.StartsWith(str2, System.StringComparison.Ordinal))
							{
								flag2 = true;
							}
						}
						else
						{
							flag1 = false;
							break;
						}
					}
					if (flag1)
					{
						if (flag2)
						{
							elementList1.Add(parameterIndex);
						}
						else
						{
							elementList2.Add(parameterIndex);
						}
					}
				}
			}

			List<int> elementList3 = new List<int>();
			elementList3.AddRange(elementList1);
			elementList3.AddRange(elementList2);
			_SearchElements = elementList3.ToArray();
		}

		int[] activeElements
		{
			get
			{
				if (string.IsNullOrEmpty(_SearchText) && _SearchMode == SearchMode.Name)
				{
					return null;
				}

				if (_SearchElements == null)
				{
					CreateSearchParameterList(_SearchText, _SearchMode, _SearchParameterType);
				}

				return _SearchElements;
			}
		}

		void RebuildSearchElements()
		{
			_SearchElements = null;
			Repaint();
		}

		void IPropertyChanged.OnPropertyChanged(PropertyChangedType propertyChangedType)
		{
			if (propertyChangedType != PropertyChangedType.UndoRedoPerformed)
			{
				return;
			}

			RebuildSearchElements();
		}

		private static class SearchGUI
		{
			public static readonly string[] searchModeNames;

			static SearchGUI()
			{
				ParameterTypeMenuItem[] menuItems = ParameterTypeMenuItem.menuItems;
				searchModeNames = new string[2 + menuItems.Length];

				searchModeNames[0] = "Name";
				searchModeNames[1] = "";

				for (int i = 0; i < menuItems.Length; i++)
				{
					searchModeNames[2 + i] = menuItems[i].content.text;
				}
			}

			public static int GetIndex(SearchMode searchMode, Parameter.Type parameterType)
			{
				if (searchMode == SearchMode.Name)
				{
					return 0;
				}

				int index = ParameterTypeMenuItem.GetIndex(parameterType);
				if (index < 0)
				{
					return -1;
				}

				return index + 2;
			}

			public static void GetSeachMode(int index, out SearchMode seachMode, out Parameter.Type parameterType)
			{
				if (index == 0)
				{
					seachMode = SearchMode.Name;
					parameterType = Parameter.Type.Int;
				}
				else
				{
					seachMode = SearchMode.Type;
					parameterType = ParameterTypeMenuItem.menuItems[index - 2].type;
				}
			}
		}

		void ListHeaderGUI()
		{
			GUIStyle headerStyle = isInParametersPanel ? ArborEditor.Styles.toolbar : GUIStyle.none;
			EditorGUILayout.BeginHorizontal(headerStyle, GUILayout.Height(EditorGUITools.toolbarHeight));

			GUILayout.Space(10f);

			int selectedIndex = SearchGUI.GetIndex(_SearchMode, _SearchParameterType);

			GUI.SetNextControlName("ParameterSearch");
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape && GUI.GetNameOfFocusedControl() == "ParameterSearch")
			{
				_SearchText = string.Empty;
				CreateSearchParameterList(_SearchText, _SearchMode, _SearchParameterType);
			}

			EditorGUI.BeginChangeCheck();
			string searchText = EditorGUITools.ToolbarSearchField(_SearchText, SearchGUI.searchModeNames, ref selectedIndex);
			if (EditorGUI.EndChangeCheck())
			{
				_SearchText = searchText;

				SearchGUI.GetSeachMode(selectedIndex, out _SearchMode, out _SearchParameterType);

				CreateSearchParameterList(_SearchText, _SearchMode, _SearchParameterType);
			}

			GUILayout.FlexibleSpace();

			bool editable = (target.hideFlags & HideFlags.NotEditable) != HideFlags.NotEditable;

			EditorGUI.BeginDisabledGroup(!editable);

			using (new EditorGUI.DisabledGroupScope(activeElements != null))
			{
				Rect addButtonRect = GUILayoutUtility.GetRect(EditorContents.iconToolbarPlusMore, ArborEditor.Styles.invisibleButton);
				if (GUI.Button(addButtonRect, EditorContents.iconToolbarPlusMore, ArborEditor.Styles.invisibleButton))
				{
					OnAddDropdown(addButtonRect, parameterList);
				}
			}

			using (new EditorGUI.DisabledGroupScope(parameterList.index < 0 || parameterList.count <= parameterList.index))
			{
				if (GUILayout.Button(EditorContents.iconToolbarMinus, ArborEditor.Styles.invisibleButton))
				{
					OnRemove(parameterList);
					RebuildSearchElements();
				}
			}

			if (isInParametersPanel)
			{
				if (GUILayout.Button(EditorContents.popupIcon, ArborEditor.Styles.invisibleButton))
				{
					GenericMenu menu = new GenericMenu();
					menu.AddItem(Localization.GetTextContent("Remove Component"), false, () =>
					{
						int undoGroup = Undo.GetCurrentGroup();

						NodeGraph nodeGraph = _ParameterContainer.owner as NodeGraph;

						Undo.RecordObject(nodeGraph, "Destroy ParameterContainer");

						ParameterContainerEditorUtility.SetParameterContainer(nodeGraph, null);

						ParameterContainerInternal.Destroy(_ParameterContainer);

						Undo.CollapseUndoOperations(undoGroup);

						EditorUtility.SetDirty(nodeGraph);

						if (onRemoveComponent != null)
						{
							onRemoveComponent();
						}
					});
					
					menu.ShowAsContext();
				}
			}

			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndHorizontal();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if (Event.current.type == EventType.Layout)
			{
				ClearCache();
			}

			var oldIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			if (!isInParametersPanel)
			{
				EditorGUILayout.BeginVertical(ArborEditor.Styles.parameterListHeader);

				EditorGUILayout.PrefixLabel(parameterListProperty.displayName);

				ListHeaderGUI();

				EditorGUILayout.EndVertical();
			}
			else
			{
				ListHeaderGUI();
			}

			ReorderableList parameterList = this.parameterList;

			if (isInParametersPanel)
			{
				_ScrollPos = EditorGUILayout.BeginScrollView(_ScrollPos);
			}

			//EditorGUI.BeginDisabledGroup(!editable);

			parameterList.DoLayoutList();

			//EditorGUI.EndDisabledGroup();

			if (isInParametersPanel)
			{
				EditorGUILayout.EndScrollView();
			}
			EditorGUI.indentLevel = oldIndentLevel;

			serializedObject.ApplyModifiedProperties();
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnDestroy()
		{
			if (!target && (object)_ParameterContainer != null && !Application.isPlaying)
			{
				_ParameterContainer.DestroySubComponents();
			}
		}
	}
}
