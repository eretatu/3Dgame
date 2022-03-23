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
	using ArborEditor.IMGUI.Controls;

	public abstract class TreePopupWindow<T> : EditorWindow
	{
		protected static TreePopupWindow<T> s_Instance;

		protected int _ControlID;

		const string kTreePopupWindowChangedMessage = "TreePopupWindowChanged";
		const string kTreePopupSearchControlName = "TreePopupSearchControlName";

		private static int s_SearchFieldHash = "s_SearchFieldHash".GetHashCode();

		[SerializeField]
		private TreeViewState _TreeViewState = new TreeViewState();

		[System.NonSerialized]
		protected TreeView _TreeView = null;

		[System.NonSerialized]
		protected TreePopupTreeViewGUI<T> _TreeViewGUI = null;

		private bool _FocusToSearchBar = false;

		private int _SearchFieldControlID;
		private bool _HasSeachFilterFocus = false;
		private int _TreeViewKeyboardControlID;
		private bool _DidSelectSearchResult;

		protected EditorWindow _SourceView;

		protected abstract string searchWord
		{
			get;
			set;
		}

		private bool hasSearch
		{
			get
			{
				return !string.IsNullOrEmpty(searchWord);
			}
		}

		protected abstract void OnCreateTree(TreeViewItem root);

		protected void CreateTree()
		{
			if (_TreeView == null)
			{
				InitTreeView();
			}

			TreeViewItem root = _TreeView.root;
			root.children.Clear();

			OnCreateTree(root);

			_TreeView.SetupDepths();

			_TreeViewGUI.DirtyTree();

			_IsCreatedTree = true;

			RebuildSearch();

			_FocusToSearchBar = true;
		}

		void DrawBackground()
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}

			Rect rect = position;
			rect.position = Vector2.zero;

			Styles.background.Draw(rect, false, false, false, false);
		}

		protected static T GetSelectedValueForControl(int controlID, T selected)
		{
			Event current = Event.current;
			if (current.type == EventType.ExecuteCommand && current.commandName == kTreePopupWindowChangedMessage)
			{
				if (s_Instance != null && s_Instance._ControlID == controlID)
				{
					selected = s_Instance.selectedValue;
					GUI.changed = true;

					current.Use();
				}
			}
			return selected;
		}

		protected virtual bool isReady
		{
			get
			{
				return true;
			}
		}

		private bool _IsCreatedTree;
		public T selectedValue
		{
			get;
			private set;
		}

		protected void Init(Rect buttonRect, int controlID, T selected)
		{
			_ControlID = controlID;
			selectedValue = selected;
			_SourceView = EditorWindow.focusedWindow;

			if (isReady)
			{
				CreateTree();
			}
			else
			{
				_IsCreatedTree = false;
			}

			Vector2 center = buttonRect.center;
			buttonRect.width = 300f;
			buttonRect.center = center;
			ShowAsDropDown(buttonRect, new Vector2(300f, 320f));
		}

		protected void RebuildSearch()
		{
			if (!_IsCreatedTree)
			{
				return;
			}

			_TreeViewGUI.RebuildSearch(hasSearch, searchWord);
		}

		void SetSearchFilter(string searchFilter)
		{
			searchWord = searchFilter;

			RebuildSearch();

			if (_DidSelectSearchResult && string.IsNullOrEmpty(searchFilter))
			{
				_DidSelectSearchResult = false;

				if (GUIUtility.keyboardControl == 0)
				{
					GUIUtility.keyboardControl = _TreeViewKeyboardControlID;
				}
			}
		}

		void SearchGUI()
		{
			GUI.SetNextControlName(kTreePopupSearchControlName);
			if (_FocusToSearchBar)
			{
				EditorGUI.FocusTextInControl(kTreePopupSearchControlName);
				if (Event.current.type == EventType.Repaint)
				{
					_FocusToSearchBar = false;
				}
			}

			Rect rect = GUILayoutUtility.GetRect(0, EditorGUITools.kLabelFloatMaxW * 1.5f, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight, Styles.toolbarSearchFieldRaw);

			_SearchFieldControlID = GUIUtility.GetControlID(s_SearchFieldHash, FocusType.Keyboard, rect);

			EditorGUI.BeginChangeCheck();
			int i = 0;
			string str = EditorGUITools.ToolbarSearchField(_SearchFieldControlID, rect, null, ref i, searchWord);
			if (EditorGUI.EndChangeCheck())
			{
				if (str != searchWord)
				{
					SetSearchFilter(str);
				}
			}

			_HasSeachFilterFocus = GUIUtility.keyboardControl == _SearchFieldControlID;

			ITreeFilter filterSettings = this as ITreeFilter;
			if (filterSettings != null && filterSettings.useFilter)
			{
				GUILayout.Space(2f);

				GUIStyle style = EditorStyles.toolbarButton;
				GUIContent content = EditorContents.filterIcon;

				Color contentColor = GUI.contentColor;
				GUI.contentColor = ArborEditorWindow.isDarkSkin ? Color.white : Color.black;

				EditorGUI.BeginChangeCheck();
				filterSettings.openFilter = GUILayout.Toggle(filterSettings.openFilter, content, style);
				if (EditorGUI.EndChangeCheck())
				{
					RebuildSearch();
				}

				GUI.contentColor = contentColor;
			}
		}

		void DoToolbar()
		{
			EditorGUILayout.BeginHorizontal(Styles.popupWindowToolbar);

			Event evt = Event.current;
			if (_HasSeachFilterFocus && evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.DownArrow || evt.keyCode == KeyCode.UpArrow))
			{
				GUIUtility.keyboardControl = _TreeViewKeyboardControlID;
				_DidSelectSearchResult = !string.IsNullOrEmpty(searchWord);
				evt.Use();
			}

			SearchGUI();

			EditorGUILayout.EndHorizontal();

			ITreeFilter filterSettings = this as ITreeFilter;
			if (filterSettings != null && filterSettings.useFilter && filterSettings.openFilter)
			{
				GUILayout.Space(3f);

				filterSettings.OnFilterSettingsGUI();

				GUILayout.Space(3f);

				EditorGUITools.DrawSeparator(ArborEditorWindow.isDarkSkin);
			}
		}

		protected void DoTreeGUI()
		{
			DoToolbar();

			_TreeViewGUI.DoTreeViewGUI(_TreeViewKeyboardControlID);
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnEnable()
		{
			wantsMouseMove = true;
		}

		void InitTreeView()
		{
			if (_TreeViewState == null)
			{
				_TreeViewState = new TreeViewState();
			}

			_TreeView = new TreeView();
			_TreeView.filter = this as ITreeFilter;

			_TreeViewGUI = new TreePopupTreeViewGUI<T>(this, _TreeView, _TreeViewState);
			_TreeViewGUI.onSubmit += OnSelect;
		}

		void OnSelect(TreeViewItem select)
		{
			var valueItem = select as TreeViewValueItem<T>;
			if (valueItem != null)
			{
				selectedValue = valueItem.value;
				_SourceView.SendEvent(EditorGUIUtility.CommandEvent(kTreePopupWindowChangedMessage));
			}
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnDisable()
		{
			_ControlID = 0;
			s_Instance = null;
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnInspectorUpdate()
		{
			if (EditorApplication.isCompiling)
			{
				Close();
			}
			else if (!_IsCreatedTree)
			{
				if (ClassList.isReady)
				{
					CreateTree();
				}

				Repaint();
			}
		}

		void HandleKeyboard()
		{
			Event current = Event.current;

			if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
			{
				Close();
				current.Use();
			}
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnGUI()
		{
			_TreeViewKeyboardControlID = GUIUtility.GetControlID(FocusType.Keyboard);

			DrawBackground();

			if (isReady && _IsCreatedTree)
			{
				HandleKeyboard();

				DoTreeGUI();

				Event current = Event.current;
				if (current.type == EventType.MouseDown)
				{
					GUIUtility.keyboardControl = 0;
				}
			}
			else
			{
				Rect rect = position;
				rect.x = rect.y = 0;
				EditorGUITools.DrawIndicator(rect, Localization.GetWord("Loading"));
				//EditorGUILayout.HelpBox(Localization.GetWord("Loading"), MessageType.Info, true);
			}
		}
	}
}