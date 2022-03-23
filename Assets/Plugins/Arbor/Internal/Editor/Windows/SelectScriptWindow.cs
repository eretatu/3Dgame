//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using Arbor;

namespace ArborEditor
{
	[System.Reflection.Obfuscation(Exclude = true)]
	[InitializeOnLoad]
	public abstract class SelectScriptWindow : EditorWindow
	{
		private class Element : System.IComparable
		{
			public int level;
			public GUIContent content;

			public string name
			{
				get
				{
					return content.text;
				}
			}

			public int CompareTo(object obj)
			{
				return name.CompareTo((obj as Element).name);
			}
		}

		private sealed class ScriptElement : Element
		{
			public System.Type classType;

			public ScriptElement(int level, string name, System.Type classType, Texture icon)
			{
				this.level = level;
				this.classType = classType;

				this.content = new GUIContent(name, icon);
			}
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		[System.Serializable]
		private sealed class GroupElement : Element
		{
			public Vector2 scroll;
			public int selectedIndex;

			public GroupElement(int level, string name)
			{
				this.level = level;
				this.content = EditorGUITools.GetTextContent(name);
			}
		}

		private sealed class ScriptMenuItem
		{
			public string menuName;
			public Texture icon;
			public System.Type classType;
		};

		private static bool s_DirtyList;

		private static class ScriptMenus
		{
			private static List<ScriptMenuItem> s_ScriptMenuItems = new List<ScriptMenuItem>();

			static ScriptMenus()
			{
				var scriptTypes = ScriptsUtility.scriptTypes;
				for (int typeIndex = 0; typeIndex < scriptTypes.Count; typeIndex++)
				{
					System.Type classType = scriptTypes[typeIndex];

					if (classType.IsAbstract || AttributeHelper.HasAttribute<HideBehaviour>(classType))
					{
						continue;
					}

					string menuName = BehaviourInfo.GetBehaviourMenu(classType);
					if (string.IsNullOrEmpty(menuName))
					{
						continue;
					}

					ScriptMenuItem menuItem = new ScriptMenuItem();
					menuItem.classType = classType;
					menuItem.icon = Icons.GetTypeIcon(classType);
					menuItem.menuName = menuName;

					s_ScriptMenuItems.Add(menuItem);
				}

				s_ScriptMenuItems.Sort((a, b) =>
				{
					string[] aNames = a.menuName.Split('/');
					string[] bNames = b.menuName.Split('/');
					int i = 0;

					while (i < aNames.Length && i < bNames.Length)
					{
						int compare = 0;
						if (i + 1 >= aNames.Length || i + 1 >= bNames.Length)
						{
							compare = bNames.Length - aNames.Length;
						}
						if (compare == 0)
						{
							compare = aNames[i].CompareTo(bNames[i]);
						}
						if (compare != 0)
						{
							return compare;
						}
						i++;
					}
					return bNames.Length - aNames.Length;
				});
			}

			public static List<ScriptMenuItem> GetItems(System.Type targetClassType)
			{
				List<ScriptMenuItem> items = new List<ScriptMenuItem>();
				var scriptMenuItems = s_ScriptMenuItems;
				for (int scriptIndex = 0; scriptIndex < scriptMenuItems.Count; scriptIndex++)
				{
					ScriptMenuItem scriptMenuItem = scriptMenuItems[scriptIndex];
					if (scriptMenuItem == null || !scriptMenuItem.classType.IsSubclassOf(targetClassType))
					{
						continue;
					}

					items.Add(scriptMenuItem);
				}

				return items;
			}
		}

		private Element[] _Tree;
		private Element[] _SearchResultTree;

		private List<GroupElement> _Stack = new List<GroupElement>();
		private float _Anim = 1f;
		private int _AnimTarget = 1;
		private long _LastTime;
		private bool _ScrollToSelected;

		public ArborEditorWindow hostWindow
		{
			get;
			private set;
		}

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

		private GroupElement activeParent
		{
			get
			{
				return _Stack[_Stack.Count - 2 + _AnimTarget];
			}
		}

		private Element[] activeTree
		{
			get
			{
				if (hasSearch)
				{
					return _SearchResultTree;
				}
				return _Tree;
			}
		}

		private Element activeElement
		{
			get
			{
				if (activeTree == null)
				{
					return null;
				}
				List<Element> children = GetChildren(activeTree, activeParent);
				if (children.Count == 0)
				{
					return null;
				}
				return children[activeParent.selectedIndex];
			}
		}

		static SelectScriptWindow()
		{
			s_DirtyList = true;
		}

		protected abstract System.Type GetClassType();
		protected abstract string GetRootElementName();

		private List<Element> GetChildren(Element[] tree, Element parent)
		{
			List<Element> list = new List<Element>();
			int num = -1;
			int index;
			for (index = 0; index < tree.Length; ++index)
			{
				if (tree[index] == parent)
				{
					num = parent.level + 1;
					++index;
					break;
				}
			}
			if (num == -1)
				return list;
			for (; index < tree.Length; ++index)
			{
				Element element = tree[index];
				if (element.level >= num)
				{
					if (element.level <= num || hasSearch)
						list.Add(element);
				}
				else
					break;
			}
			return list;
		}

		private void CreateScriptTree()
		{
			System.Type targetClassType = GetClassType();

			List<ScriptMenuItem> items = ScriptMenus.GetItems(targetClassType);

			List<string> list1 = new List<string>();
			List<Element> list2 = new List<Element>();

			list2.Add(new GroupElement(0, GetRootElementName()));

			int itemCount = items.Count;
			for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
			{
				ScriptMenuItem item = items[itemIndex];

				string str = item.menuName;
				string[] strArray = str.Split(new char[] { '/' });

				while (strArray.Length - 1 < list1.Count)
					list1.RemoveAt(list1.Count - 1);
				while (list1.Count > 0 && strArray[list1.Count - 1] != list1[list1.Count - 1])
					list1.RemoveAt(list1.Count - 1);
				while (strArray.Length - 1 > list1.Count)
				{
					list2.Add(new GroupElement(list1.Count + 1, strArray[list1.Count]));
					list1.Add(strArray[list1.Count]);
				}
				list2.Add(new ScriptElement(list1.Count + 1, strArray[strArray.Length - 1], item.classType, item.icon));
			}

			_Tree = list2.ToArray();
			if (_Stack.Count == 0)
			{
				_Stack.Add(_Tree[0] as GroupElement);
			}
			else
			{
				GroupElement groupElement = _Tree[0] as GroupElement;
				int level = 0;
				while (true)
				{
					GroupElement groupElement2 = _Stack[level];
					_Stack[level] = groupElement;
					_Stack[level].selectedIndex = groupElement2.selectedIndex;
					_Stack[level].scroll = groupElement2.scroll;
					level++;
					if (level == _Stack.Count)
					{
						break;
					}
					List<Element> children = GetChildren(activeTree, groupElement);
					Element element = children.FirstOrDefault((c) => c.name == _Stack[level].name);
					if (element != null && element is GroupElement)
					{
						groupElement = (element as GroupElement);
					}
					else
					{
						while (_Stack.Count > level)
						{
							_Stack.RemoveAt(level);
						}
					}
				}
			}
			s_DirtyList = false;
			RebuildSearch();
		}

		private void RebuildSearch()
		{
			if (!hasSearch)
			{
				_SearchResultTree = null;
				if (_Stack[_Stack.Count - 1].name == "Search")
				{
					_Stack.Clear();
					_Stack.Add(_Tree[0] as GroupElement);
				}
				_AnimTarget = 1;
				_LastTime = System.DateTime.Now.Ticks;
			}
			else
			{
				string[] searchWords = searchWord.ToLower().Split(new char[] { ' ' });

				List<Element> matchesStart = new List<Element>();
				List<Element> matchesWithin = new List<Element>();

				for (int elementIndex = 0; elementIndex < _Tree.Length; elementIndex++)
				{
					Element element = _Tree[elementIndex];
					if (!(element is ScriptElement))
					{
						continue;
					}

					string name = element.name.ToLower().Replace(" ", "");

					bool didMatchAll = true;
					bool didMatchStart = false;

					for (int wordIndex = 0; wordIndex < searchWords.Length; ++wordIndex)
					{
						string word = searchWords[wordIndex];
						if (name.Contains(word))
						{
							if (wordIndex == 0 && name.StartsWith(word, System.StringComparison.Ordinal))
							{
								didMatchStart = true;
							}
						}
						else
						{
							didMatchAll = false;
							break;
						}
					}
					if (didMatchAll)
					{
						if (didMatchStart)
							matchesStart.Add(element);
						else
							matchesWithin.Add(element);
					}
				}

				matchesStart.Sort();
				matchesWithin.Sort();

				List<Element> searchTree = new List<Element>();
				searchTree.Add((Element)new GroupElement(0, "Search"));
				searchTree.AddRange(matchesStart);
				searchTree.AddRange(matchesWithin);

				_SearchResultTree = searchTree.ToArray();
				_Stack.Clear();
				_Stack.Add(_SearchResultTree[0] as GroupElement);

				_Anim = 1f;
				_AnimTarget = 1;

				if (GetChildren(activeTree, activeParent).Count >= 1)
				{
					activeParent.selectedIndex = 0;
				}
				else
				{
					activeParent.selectedIndex = -1;
				}
			}
		}

		private GroupElement GetElementRelative(int rel)
		{
			int index = _Stack.Count + rel - 1;
			if (index < 0)
			{
				return null;
			}
			return _Stack[index];
		}

		private void GoToParent()
		{
			if (_Stack.Count <= 1)
			{
				return;
			}
			_AnimTarget = 0;
			_LastTime = System.DateTime.Now.Ticks;
		}

		private void ListGUI(Element[] tree, float anim, GroupElement parent, GroupElement grandParent)
		{
			anim = Mathf.Floor(anim) + Mathf.SmoothStep(0.0f, 1f, Mathf.Repeat(anim, 1f));
			Rect position1 = this.position;

			float headerHeight = EditorGUITools.useNewSearchField ? Styles.toolbarSearchField.fixedHeight + Styles.toolbarSearchField.margin.vertical : 30f;
			position1.x = (float)((double)this.position.width * (1.0 - (double)anim) + 1.0);
			position1.y = headerHeight;
			position1.height -= headerHeight;
			position1.width -= 2f;
			GUILayout.BeginArea(position1);
			Rect rect = GUILayoutUtility.GetRect(10f, 25f);
			string name = parent.name;
			GUI.Label(rect, name, Styles.header);
			if (grandParent != null)
			{
				if (Event.current.type == EventType.Repaint)
				{
					GUIStyle leftArrowStyle = Styles.leftArrow;
					Vector2 size = new Vector2(leftArrowStyle.fixedWidth, leftArrowStyle.fixedHeight);
					Rect position2 = new Rect(rect.x + 4f, rect.y + (rect.height - size.y)*0.5f, size.x, size.y);
					leftArrowStyle.Draw(position2, false, false, false, false);
				}
				if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
				{
					this.GoToParent();
					Event.current.Use();
				}
			}
			ListGUI(tree, parent);
			GUILayout.EndArea();
		}

		protected abstract void OnSelect(System.Type classType);

		private void GoToChild(Element e, bool addIfComponent)
		{
			if (e is ScriptElement)
			{
				if (!addIfComponent)
					return;

				_IsSelected = true;

				ScriptElement behaviourElement = e as ScriptElement;

				OnSelect(behaviourElement.classType);

				hostWindow.DoRepaint();

				Close();
			}
			else
			{
				if (hasSearch)
					return;
				_LastTime = System.DateTime.Now.Ticks;
				if (_AnimTarget == 0)
				{
					_AnimTarget = 1;
				}
				else
				{
					if (_Anim != 1.0f)
						return;
					_Anim = 0.0f;
					_Stack.Add(e as GroupElement);
				}
			}
		}

		private void ListGUI(Element[] tree, GroupElement parent)
		{
			parent.scroll = GUILayout.BeginScrollView(parent.scroll);
			Vector2 iconSize = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
			List<Element> children = GetChildren(tree, parent);
			Rect rect1 = new Rect();
			for (int index1 = 0; index1 < children.Count; ++index1)
			{
				Element e = children[index1];
				ScriptElement behaviourElement = e as ScriptElement;

				GUIStyle baseStyle = behaviourElement == null ? Styles.groupButton : Styles.componentButton;

				Rect rect2 = GUILayoutUtility.GetRect(16.0f, 20.0f, GUILayout.ExpandWidth(true));
				if ((Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDown) && (parent.selectedIndex != index1 && rect2.Contains(Event.current.mousePosition)))
				{
					parent.selectedIndex = index1;
					this.Repaint();
				}
				bool flag = false;
				if (index1 == parent.selectedIndex)
				{
					flag = true;
					rect1 = rect2;
				}
				if (Event.current.type == EventType.Repaint)
				{
					baseStyle.Draw(rect2, e.content, false, false, flag, flag);
					if (behaviourElement == null)
					{
						GUIStyle rightArrowStyle = Styles.rightArrow;
						Vector2 size = new Vector2(rightArrowStyle.fixedWidth, rightArrowStyle.fixedHeight);
						Rect position = new Rect(rect2.xMax - size.x, rect2.y + (rect2.height - size.y)*0.5f, size.x, size.y);
						rightArrowStyle.Draw(position, false, false, false, false);
					}
				}
				if (behaviourElement != null)
				{
					Rect rect3 = rect2;
					rect3.xMax -= 2f;
					Rect position = BehaviourEditorGUI.Default.GetSettingsRect(rect3, baseStyle, Styles.iconButton);
					EditorGUITools.HelpButton(position, behaviourElement.classType);
				}
				if (Event.current.type == EventType.MouseDown && rect2.Contains(Event.current.mousePosition))
				{
					Event.current.Use();
					parent.selectedIndex = index1;
					GoToChild(e, true);
				}
			}
			EditorGUIUtility.SetIconSize(iconSize);
			GUILayout.EndScrollView();
			if (!_ScrollToSelected || Event.current.type != EventType.Repaint)
				return;
			_ScrollToSelected = false;
			Rect lastRect = GUILayoutUtility.GetLastRect();
			if (rect1.yMax - lastRect.height > parent.scroll.y)
			{
				parent.scroll.y = rect1.yMax - lastRect.height;
				Repaint();
			}
			if (rect1.y >= parent.scroll.y)
			{
				return;
			}
			parent.scroll.y = rect1.y;
			Repaint();
		}

		protected void Open(ArborEditorWindow hostWindow, Rect buttonRect)
		{
			this.hostWindow = hostWindow;

			CreateScriptTree();

			Defaults.ShowAsDropDown(this, buttonRect, new Vector2(300f, 320f));
		}

		private bool _IsCompiling = false;

		[System.Reflection.Obfuscation(Exclude = true)]
		void Update()
		{
			bool endCompile = false;
			if (EditorApplication.isCompiling)
			{
				_IsCompiling = true;
			}
			else if(_IsCompiling)
			{
				endCompile = true;
				_IsCompiling = false;
			}

			// Close on lost focus.
			// OnLostFocus() will be called even if the UnityEditor application loses focus, so check with Update.
			if (endCompile || UnityEditorInternal.InternalEditorUtility.isApplicationActive && EditorWindow.focusedWindow != this)
			{
				Close();
			}
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

		void SearchGUI()
		{
			GUI.SetNextControlName("ArborBehaviourSearch");
			string str = EditorGUITools.DropdownSearchField(searchWord);
			if (str != searchWord)
			{
				searchWord = str;
				RebuildSearch();
			}
		}

		private void HandleKeyboard()
		{
			Event current = Event.current;
			if (current.type != EventType.KeyDown)
			{
				return;
			}

			if (current.keyCode == KeyCode.DownArrow)
			{
				++activeParent.selectedIndex;
				activeParent.selectedIndex = Mathf.Min(activeParent.selectedIndex, GetChildren(activeTree, activeParent).Count - 1);
				_ScrollToSelected = true;
				current.Use();
			}
			if (current.keyCode == KeyCode.UpArrow)
			{
				--activeParent.selectedIndex;
				activeParent.selectedIndex = Mathf.Max(activeParent.selectedIndex, 0);
				_ScrollToSelected = true;
				current.Use();
			}
			if (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
			{
				GoToChild(activeElement, true);
				current.Use();
			}
			if (hasSearch)
			{
				return;
			}
			if (current.keyCode == KeyCode.LeftArrow || current.keyCode == KeyCode.Backspace)
			{
				GoToParent();
				current.Use();
			}
			if (current.keyCode == KeyCode.RightArrow)
			{
				GoToChild(activeElement, false);
				current.Use();
			}
			if (current.keyCode != KeyCode.Escape)
			{
				return;
			}
			Close();
			current.Use();
		}

		private bool _IsSelected = false;

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnEnable()
		{
			wantsMouseMove = true;
			_IsSelected = false;
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnDisable()
		{
			if (!_IsSelected)
			{
				OnCancel();
			}

			OnClose();
		}

		protected virtual void OnCancel()
		{
		}

		protected virtual void OnClose()
		{
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnGUI()
		{
			DrawBackground();

			if (s_DirtyList)
			{
				CreateScriptTree();
			}

			HandleKeyboard();

			if (!EditorGUITools.useNewSearchField)
			{
				GUILayout.Space(7f);
			}
			EditorGUI.FocusTextInControl("ArborBehaviourSearch");

			SearchGUI();

			ListGUI(activeTree, _Anim, GetElementRelative(0), GetElementRelative(-1));
			if (_Anim < 1.0)
			{
				ListGUI(activeTree, _Anim + 1f, GetElementRelative(-1), GetElementRelative(-2));
			}

			if (_Anim == _AnimTarget || Event.current.type != EventType.Repaint)
				return;
			long ticks = System.DateTime.Now.Ticks;
			float num = (float)(ticks - _LastTime) / 1E+07f;
			_LastTime = ticks;
			_Anim = Mathf.MoveTowards(_Anim, _AnimTarget, num * 4f);
			if (_AnimTarget == 0 && _Anim == 0.0f)
			{
				_Anim = 1f;
				_AnimTarget = 1;
				_Stack.RemoveAt(_Stack.Count - 1);
			}
			Repaint();
		}

		private static class Defaults
		{
			private static readonly System.Reflection.MethodInfo s_ShowAsDropDownFitToScreen;
			private static readonly System.Reflection.FieldInfo s_Parent;
			private static readonly System.Reflection.PropertyInfo s_Window;
			private static readonly System.Reflection.FieldInfo s_DontSaveToLayout;

			static Defaults()
			{
				System.Type typeEditorWindow = typeof(EditorWindow);
				s_ShowAsDropDownFitToScreen = typeEditorWindow.GetMethod("ShowAsDropDownFitToScreen", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				s_Parent = typeEditorWindow.GetField("m_Parent", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
				s_Window = s_Parent.FieldType.GetProperty("window", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
				s_DontSaveToLayout = s_Window.PropertyType.GetField("m_DontSaveToLayout", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
			}

			public static void ShowAsDropDown(EditorWindow window, Rect buttonRect, Vector2 windowSize)
			{
				window.position = (Rect)s_ShowAsDropDownFitToScreen.Invoke(window, new object[] { buttonRect, windowSize, null });

				window.ShowPopup();

				window.position = (Rect)s_ShowAsDropDownFitToScreen.Invoke(window, new object[] { buttonRect, windowSize, null });

				// Default to none resizable window
				window.minSize = new Vector2(window.position.width, window.position.height);
				window.maxSize = new Vector2(window.position.width, window.position.height);

				if (EditorWindow.focusedWindow != window)
				{
					window.Focus();
				}
				else
				{
					window.Repaint();
				}

				object parentValue = s_Parent.GetValue(window);
				object windowValue = s_Window.GetValue(parentValue, null);
				s_DontSaveToLayout.SetValue(windowValue, true);
			}
		}
	}
}