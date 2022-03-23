//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Arbor;

namespace ArborEditor
{
	[System.Serializable]
	public class BehaviourEditorGUI
	{
		private NodeEditor _NodeEditor;
		private Object _BehaviourObj;
		private Editor _Editor;

		public int behaviourIndex = 0;

		public NodeEditor nodeEditor
		{
			get
			{
				return _NodeEditor;
			}
		}

		public Object behaviourObj
		{
			get
			{
				return _BehaviourObj;
			}
		}

		public Editor editor
		{
			get
			{
				return _Editor;
			}
		}

		public enum MarginType
		{
			Editor,
			ForceDefault,
			ForceFull,
		}

		public MarginType marginType = MarginType.Editor;

		public bool expanded = true;

		public Color backgroundColor = Color.white;

		public void Initialize(NodeEditor nodeEditor, Object behaviourObj)
		{
			_NodeEditor = nodeEditor;
			_BehaviourObj = behaviourObj;
			if (_BehaviourObj is NodeBehaviour)
			{
				_Editor = NodeBehaviourEditor.CreateEditor(_NodeEditor, _BehaviourObj);
			}
		}

		public void Validate()
		{
			if (_Editor != null && !ComponentUtility.IsValidObject(_Editor.target))
			{
				DestroyEditor();
			}

			if (_Editor == null && _BehaviourObj is NodeBehaviour)
			{
				_Editor = NodeBehaviourEditor.CreateEditor(_NodeEditor, _BehaviourObj);
			}
		}

		public void Repair(Object behaviourObj)
		{
			DestroyEditor();

			_BehaviourObj = behaviourObj;

			if (_Editor == null && _BehaviourObj is NodeBehaviour)
			{
				_Editor = NodeBehaviourEditor.CreateEditor(_NodeEditor, _BehaviourObj);
			}
		}

		public void DestroyEditor()
		{
			if (_Editor != null)
			{
				Object.DestroyImmediate(_Editor);
				_Editor = null;
			}
		}

		private static readonly int s_BehaviourTitlebarHash = "s_BehaviourTitlebarHash".GetHashCode();

		protected virtual bool HasTitlebar()
		{
			return false;
		}

		public virtual bool GetExpanded()
		{
			return true;
		}

		public virtual void SetExpanded(bool expanded)
		{
		}

		protected virtual bool HasBehaviourEnable()
		{
			return false;
		}

		protected virtual bool GetBehaviourEnable()
		{
			return false;
		}

		protected virtual void SetBehaviourEnable(bool enable)
		{
		}

		protected virtual void SetPopupMenu(GenericMenu menu)
		{
		}

		static void EditScriptBehaviourContextMenu(object obj)
		{
			MonoScript script = obj as MonoScript;

			AssetDatabase.OpenAsset(script);
		}

		public void SetContextMenu(GenericMenu menu, Rect popupPosition)
		{
			int menuItemCount = menu.GetItemCount();

			SetPopupMenu(menu);

			if (menu.GetItemCount() > menuItemCount)
			{
				menu.AddSeparator("");
			}

			MonoScript script = EditorGUITools.GetMonoScript(_BehaviourObj);

			if (script != null)
			{
				menu.AddItem(EditorContents.editScript, false, EditScriptBehaviourContextMenu, script);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.editScript);
			}

			MonoScript editorScript = EditorGUITools.GetMonoScript(_Editor);

			if (editorScript != null && (editorScript.hideFlags & HideFlags.NotEditable) != HideFlags.NotEditable)
			{
				var editorScriptType = editorScript.GetClass();
				if (editorScriptType != null
#if !ARBOR_DISABLE_DEFAULT_EDITOR
					&& editorScriptType != typeof(NodeBehaviourDefaultEditor)
#endif
					)
				{
					menu.AddItem(EditorContents.editEditorScript, false, EditScriptBehaviourContextMenu, editorScript);
				}
			}

			EditorGUITools.AddContextMenu(menu, _BehaviourObj);
		}

		void DoContextMenu(Rect popupPosition)
		{
			GenericMenu menu = new GenericMenu();

			SetContextMenu(menu, popupPosition);

			menu.DropDown(popupPosition);
		}

		void OnPresetChanged()
		{
			NodeBehaviourEditor editor = this.editor as NodeBehaviourEditor;
			if (editor != null)
			{
				editor.OnPresetApplied();
			}

			nodeEditor.graphEditor.Repaint();
		}

		internal static class Default
		{
			delegate Rect DelegateGetIconRect(Rect position, GUIStyle baseStyle);
			delegate Rect DelegateGetTextRect(Rect position, Rect iconRect, Rect settingsRect, GUIStyle baseStyle, GUIStyle textStyle);
			delegate Rect DelegateGetSettingsRect(Rect position, GUIStyle baseStyle, GUIStyle iconButtonStyle);

			static readonly DelegateGetIconRect _GetIconRect;
			static readonly DelegateGetTextRect _GetTextRect;
			static readonly DelegateGetSettingsRect _GetSettingsRect;

			public const int kInspTitlebarIconWidth = 16;
			public static readonly int titlebarSpacing;

			static Default()
			{
				var editorGUIType = typeof(EditorGUI);
				var field = typeof(EditorGUI).GetField("kInspTitlebarSpacing", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
				if (field != null)
				{
					titlebarSpacing = (int)field.GetValue(null);
				}
				else
				{
					titlebarSpacing = 2;
				}

				_GetIconRect = EditorGUITools.GetDelegate<DelegateGetIconRect>(editorGUIType, "GetIconRect", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
				_GetTextRect = EditorGUITools.GetDelegate<DelegateGetTextRect>(editorGUIType, "GetTextRect", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
				_GetSettingsRect = EditorGUITools.GetDelegate<DelegateGetSettingsRect>(editorGUIType, "GetSettingsRect", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			}

			public static Rect GetIconRect(Rect position, GUIStyle baseStyle)
			{
				if (_GetIconRect != null)
				{
					return _GetIconRect(position, baseStyle);
				}
				else
				{
					return new Rect(position.x + baseStyle.padding.left, position.y + baseStyle.padding.top, kInspTitlebarIconWidth, kInspTitlebarIconWidth);
				}
			}

			public static Rect GetTextRect(Rect position, Rect iconRect, Rect settingsRect, GUIStyle baseStyle, GUIStyle textStyle)
			{
				if (_GetTextRect != null)
				{
					return _GetTextRect(position, iconRect, settingsRect, baseStyle, textStyle);
				}
				else
				{
					return new Rect(iconRect.xMax + titlebarSpacing + titlebarSpacing + kInspTitlebarIconWidth,
						position.y + (float)baseStyle.padding.top,
						100, textStyle.fixedHeight)
					{
						xMax = settingsRect.xMin - titlebarSpacing
					};
				}
			}

			public static Rect GetSettingsRect(Rect position, GUIStyle baseStyle, GUIStyle iconButtonStyle)
			{
				if (_GetSettingsRect != null)
				{
					return _GetSettingsRect(position, baseStyle, iconButtonStyle);
				}
				else
				{
					Vector2 settingsElementSize = iconButtonStyle.CalcSize(EditorContents.popupIcon);
					return new Rect(position.xMax - baseStyle.padding.right - titlebarSpacing - kInspTitlebarIconWidth,
						position.y + baseStyle.padding.top, settingsElementSize.x, settingsElementSize.y);
				}
			}
		}

		bool IsEditorEnabled(Editor editor)
		{
			return EditorGUITools.IsEditorEnabled(editor) && !(nodeEditor.graphEditor != null && nodeEditor.graphEditor.isExternalGraph);
		}

		protected bool BehaviourTitlebar(Rect position, bool foldout)
		{
			using (new ProfilerScope("EditorGUITools.BehvaiourTitlebar"))
			{
				int controlId = GUIUtility.GetControlID(s_BehaviourTitlebarHash, FocusType.Passive, position);

				GUIStyle baseStyle = Styles.titlebar;
				GUIStyle textStyle = Styles.titlebarText;
				GUIStyle iconButtonStyle = Styles.iconButton;

				Event current = Event.current;

				bool isDragging = BehaviourDragInfo.GetDragControlID() == controlId;
				bool isHover = position.Contains(current.mousePosition) || isDragging;
				bool isActive = GUIUtility.hotControl == controlId || isDragging;
				bool on = foldout;
				bool hasKeyboardFocus = GUIUtility.keyboardControl == controlId;

				//foldout = EditorGUI.Foldout( position,foldout,GUIContent.none,s_BehaviourTitlebar );

				NodeBehaviour behaviour = _BehaviourObj as NodeBehaviour;

				BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(_BehaviourObj);

				Rect iconPosition = Default.GetIconRect(position, baseStyle);
				Rect popupPosition = Default.GetSettingsRect(position, baseStyle, iconButtonStyle);

				Rect textPosition = Default.GetTextRect(position, iconPosition, popupPosition, baseStyle, textStyle);

				Rect togglePosition = iconPosition;
				togglePosition.x = iconPosition.xMax + Default.titlebarSpacing;

				if (current.type == EventType.Repaint)
				{
					Color savedBackgroundColor = GUI.backgroundColor;
					GUI.backgroundColor = backgroundColor;

					baseStyle.Draw(position, GUIContent.none, isHover, isActive, on, hasKeyboardFocus);

					GUI.backgroundColor = savedBackgroundColor;
				}

				bool enabled = IsEditorEnabled(editor);

				if (behaviour != null && HasBehaviourEnable())
				{
					using (new EditorGUI.DisabledScope(!enabled))
					{
						EditorGUI.BeginChangeCheck();
						bool behaviourEnabled = EditorGUI.Toggle(togglePosition, GetBehaviourEnable());
						if (EditorGUI.EndChangeCheck())
						{
							Undo.RecordObject(_BehaviourObj, (!behaviourEnabled ? "Disable" : "Enable") + " Behaviour");
							SetBehaviourEnable(behaviourEnabled);
							EditorUtility.SetDirty(_BehaviourObj);
						}
					}
				}

				// popup
				if (EditorGUI.DropdownButton(popupPosition, EditorContents.popupIcon, FocusType.Passive, Styles.iconButton))
				{
					DoContextMenu(popupPosition);
				}

				Rect headerItemRect = popupPosition;
				headerItemRect.x -= Default.kInspTitlebarIconWidth + Default.titlebarSpacing;

				if (Presets.PresetContextMenu.PresetButton(headerItemRect, _BehaviourObj, OnPresetChanged))
				{
					headerItemRect.x -= headerItemRect.width;
				}

				if (EditorGUITools.HelpButton(headerItemRect, _BehaviourObj))
				{
					headerItemRect.x -= headerItemRect.width;
				}

				textPosition.xMax = headerItemRect.xMin - Default.titlebarSpacing;

				EventType typeForControl = current.GetTypeForControl(controlId);
				switch (typeForControl)
				{
					case EventType.ContextClick:
						if (position.Contains(current.mousePosition))
						{
							DoContextMenu(popupPosition);

							current.Use();
						}
						break;
					case EventType.MouseDown:
						if (togglePosition.Contains(current.mousePosition))
						{
							if (current.button == 0 && (Application.platform != RuntimePlatform.OSXEditor || !current.control))
							{

							}
						}
						else if (position.Contains(current.mousePosition))
						{
							if (current.button == 0 && (Application.platform != RuntimePlatform.OSXEditor || !current.control))
							{
								GUIUtility.hotControl = GUIUtility.keyboardControl = controlId;
								DragAndDropUtility.Begin(controlId);
								current.Use();
							}
						}
						break;
					case EventType.MouseDrag:
						if (GUIUtility.hotControl == controlId && DragAndDropUtility.CanDrag(controlId))
						{
							DragAndDropUtility.End(controlId);
							GUIUtility.hotControl = 0;

							BehaviourDragInfo.BeginDragBehaviour(this, controlId);

							current.Use();
						}
						break;
					case EventType.MouseUp:
						if (GUIUtility.hotControl == controlId)
						{
							if (current.button == 0)
							{
								GUIUtility.hotControl = 0;

								if (position.Contains(current.mousePosition))
								{
									foldout = !foldout;
								}
							}

							current.Use();
						}
						break;
					case EventType.KeyDown:
						if (GUIUtility.keyboardControl == controlId)
						{
							if (current.keyCode == KeyCode.LeftArrow)
							{
								foldout = false;
								current.Use();
							}
							if (current.keyCode == KeyCode.RightArrow)
							{
								foldout = true;
								current.Use();
								break;
							}
						}
						break;
					case EventType.Repaint:
						using (new EditorGUI.DisabledScope(!enabled))
						{
							GUIStyle.none.Draw(iconPosition, EditorGUITools.GetThumbnailContent(_BehaviourObj), isHover, isActive, on, hasKeyboardFocus);
						}
						textStyle.Draw(textPosition, behaviourInfo.titleContent, isHover, isActive, on, hasKeyboardFocus);
						Styles.foldout.Draw(new Rect(position.x + 3f, position.y + 3f, 16f, 16f), isHover, isActive, on, hasKeyboardFocus);

#if ARBOR_DEBUG
						Vector2 titleTextSize = Styles.titlebarText.CalcSize(behaviourInfo.titleContent);

						GUIContent behaviourIndexContent = new GUIContent(behaviourIndex.ToString());
						GUIStyle badgeStyle = Styles.countBadge;
						Vector2 behaviourIndexSize = badgeStyle.CalcSize(behaviourIndexContent);
						Rect behaviourIndexRect = new Rect(textPosition.x + titleTextSize.x, textPosition.y, behaviourIndexSize.x, behaviourIndexSize.y);
						badgeStyle.Draw(behaviourIndexRect, behaviourIndexContent, false, false, false, false);
#endif
						break;
				}

				return foldout;
			}
		}

		protected bool BehaviourTitlebar(bool foldout)
		{
			Rect position = GUILayoutUtility.GetRect(GUIContent.none, Styles.titlebar);

			return BehaviourTitlebar(position, foldout);
		}

		public virtual void OnTopGUI()
		{
		}

		public virtual void OnBottomGUI()
		{
		}

		protected virtual void OnUnderlayGUI(Rect rect)
		{
		}

		public bool IsVisibleDataLinkGUI()
		{
			NodeBehaviour nodeBehaviour = _BehaviourObj as NodeBehaviour;
			if (nodeBehaviour == null || !GetExpanded())
			{
				return false;
			}

			for (int i = 0, count = nodeBehaviour.dataSlotCount; i < count; ++i)
			{
				DataSlot slot = nodeBehaviour.GetDataSlot(i);
				if (slot == null)
				{
					continue;
				}

				if (slot.slotType == SlotType.Input && slot.isVisible)
				{
					return true;
				}
			}

			return false;
		}

		private sealed class InputSlotLinker
		{
			public SerializedPropertyKey propertyKey;
			public Rect position;
			public bool enabled;

			public void OnLinkGUI(RectOffset outsideOffset)
			{
				using (new EditorGUI.DisabledGroupScope(!enabled))
				{
					Vector2 slotOffset = new Vector2(-outsideOffset.left, -outsideOffset.top);

					Rect position = this.position;

					position.x = 0f;
					position.y += outsideOffset.top;
					position.width = outsideOffset.left + 6f;
					position.height = EditorGUIUtility.singleLineHeight;

					SerializedProperty property = propertyKey.GetProperty();

					DataSlotGUI slotGUI = DataSlotGUI.GetGUI(property);

					if (slotGUI != null)
					{
						slotGUI.DoGUI(position, property, GUIContent.none, slotOffset);
					}

					if (Event.current.type == EventType.Repaint)
					{
						EditorGUIUtility.AddCursorRect(position, MouseCursor.Arrow);
					}
				}
			}
		}

		private static Dictionary<Object, List<InputSlotLinker>> _InputSlotLinkers = new Dictionary<Object, List<InputSlotLinker>>();

#if UNITY_2019_3_OR_NEWER
		[InitializeOnEnterPlayMode]
		static void OnEnterPlayMode()
		{
			_InputSlotLinkers.Clear();
		}
#endif

		public static void ClearInputSlotLink(Object obj)
		{
			List<InputSlotLinker> linkers = null;
			if (obj != null && _InputSlotLinkers.TryGetValue(obj, out linkers))
			{
				linkers.Clear();
			}
		}

		public static void AddInputSlotLink(Rect position, SerializedProperty property)
		{
			Object obj = property.serializedObject.targetObject;

			List<InputSlotLinker> linkers = null;
			if (!_InputSlotLinkers.TryGetValue(obj, out linkers))
			{
				linkers = new List<InputSlotLinker>();
				_InputSlotLinkers.Add(obj, linkers);
			}

			InputSlotLinker element = new InputSlotLinker()
			{
				position = position,
				propertyKey = new SerializedPropertyKey(property),
				enabled = GUI.enabled,
			};
			linkers.Add(element);
		}

		public void DataLinkGUI(RectOffset outsideOffset)
		{
			if (!GetExpanded())
			{
				return;
			}

			Editor editor = this.editor;

			if (editor == null)
			{
				return;
			}

			editor.serializedObject.Update();

			List<InputSlotLinker> linkers = null;
			if (editor.target != null && _InputSlotLinkers.TryGetValue(editor.target, out linkers))
			{
				for (int i = 0; i < linkers.Count; i++)
				{
					InputSlotLinker linker = linkers[i];
					linker.OnLinkGUI(outsideOffset);
				}
			}

			editor.serializedObject.ApplyModifiedProperties();
		}

		GUIStyle GetMarginStyle()
		{
			switch (marginType)
			{
				case MarginType.Editor:
					return (editor == null || editor.UseDefaultMargins()) ? EditorStyles.inspectorDefaultMargins : EditorStyles.inspectorFullWidthMargins;
				case MarginType.ForceDefault:
					return EditorStyles.inspectorDefaultMargins;
				case MarginType.ForceFull:
					return EditorStyles.inspectorFullWidthMargins;
			}

			return EditorStyles.inspectorDefaultMargins;
		}

		private bool _InOnGUI = false;
		private bool _HeaderSpaced = true;

		protected void HeaderSpace()
		{
			if (!_InOnGUI || _HeaderSpaced)
			{
				return;
			}

			GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
			_HeaderSpaced = true;
		}

		private bool _IsValidated = false;

		void BeginInspectorGUI()
		{
			_IsValidated = false;

			NodeBehaviour behaviour = behaviourObj as NodeBehaviour;
			if (behaviour != null)
			{
				behaviour.onValidate += OnValidate;
			}
		}

		void OnValidate()
		{
			_IsValidated = true;
		}

		void EndInspectorGUI()
		{
			NodeBehaviour behaviour = behaviourObj as NodeBehaviour;
			if (behaviour != null)
			{
				behaviour.onValidate -= OnValidate;
				if (_IsValidated && behaviour.nodeGraph != null)
				{
					behaviour.nodeGraph.RefreshDataBranchies();
				}
			}

			_IsValidated = false;
		}

		public void OnGUI()
		{
			_InOnGUI = true;

			try
			{
				_HeaderSpaced = false;
				Editor editor = this.editor;

				ClearInputSlotLink(behaviourObj);

				Rect rect = EditorGUILayout.BeginVertical();

				OnUnderlayGUI(rect);

				bool expanded = true;

				bool hasTitlebar = HasTitlebar();
				if (hasTitlebar)
				{
					expanded = GetExpanded();
					bool resultExpanded = BehaviourTitlebar(expanded);
					if (expanded != resultExpanded)
					{
						expanded = resultExpanded;

						SetExpanded(expanded);
					}

					_HeaderSpaced = false;
				}
				else
				{
					_HeaderSpaced = true;
				}

				if (_NodeEditor != null && _NodeEditor.graphEditor != null && (object)behaviourObj != null && Event.current.type == EventType.Repaint)
				{
					Rect lastRect = hasTitlebar ? _NodeEditor.NodeToGraphRect(GUILayoutUtility.GetLastRect()) : _NodeEditor.node.position;
					lastRect.height += EditorGUIUtility.standardVerticalSpacing;

					_NodeEditor.graphEditor.SetBehaviourPosition(_BehaviourObj, lastRect);
				}

				BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(_BehaviourObj);

				if (behaviourInfo.obsolete != null)
				{
					EditorGUILayout.HelpBox(behaviourInfo.obsolete.Message, behaviourInfo.obsolete.IsError ? MessageType.Error : MessageType.Warning);
					_HeaderSpaced = true;
				}

				OnTopGUI();

				if (expanded)
				{
					_HeaderSpaced = true;

					GUIStyle marginStyle = GetMarginStyle();

					EditorGUILayout.BeginVertical(marginStyle);

					if (editor != null)
					{
						using (new EditorGUI.DisabledScope(!IsEditorEnabled(editor)))
						{
							using (new ProfilerScope("OnInspector"))
							{
								try
								{
									BeginInspectorGUI();
									editor.OnInspectorGUI();
									EndInspectorGUI();
								}
								catch (System.Exception ex)
								{
									if (EditorGUITools.ShouldRethrowException(ex))
									{
										throw;
									}
									else
									{
										Debug.LogException(ex);
									}
								}

								if (editor.RequiresConstantRepaint())
								{
									if (nodeEditor != null && nodeEditor.graphEditor != null)
									{
										nodeEditor.graphEditor.Repaint();
									}
								}
							}
						}
					}
					else
					{
						if (behaviourObj != null)
						{
							MonoScript script = EditorGUITools.GetMonoScript(behaviourObj);
							if (script != null)
							{
								EditorGUI.BeginDisabledGroup(true);
								EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
								EditorGUI.EndDisabledGroup();
							}
						}

						EditorGUILayout.HelpBox(Localization.GetWord("MissingError"), MessageType.Error);
					}

					EditorGUILayout.EndVertical();
				}

				OnBottomGUI();

				EditorGUILayout.EndVertical();
			}
			finally
			{
				_InOnGUI = false;
			}
		}
	}
}