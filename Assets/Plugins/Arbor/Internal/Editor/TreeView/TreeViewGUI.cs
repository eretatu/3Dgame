//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ArborEditor.IMGUI.Controls
{
	using Arbor;

	public class TreeViewGUI
	{
		static int s_TreeViewKeyboardHash = "s_TreeViewKeyboardHash".GetHashCode();

		public static readonly float k_BaseIndent = 2f;
		public static readonly float k_IndentWidth = 14f;
		public static readonly float k_FoldoutWidth = 12f;
		public static readonly float k_IconWidth = 16f;
		public static readonly float k_SpaceBetweenIconAndText = 2f;

		public event System.Action<TreeViewItem> onSubmit;
		public event System.Action<TreeViewItem> contextClickItemCallback;
		public event System.Action<RenameEndedArgs> onRenameEnded;

		public bool selectSubmit = false;
		public bool renamable = false;

		private TreeView _TreeView;
		private List<TreeViewItem> _ViewItems = new List<TreeViewItem>();
		private int _ViewCount;
		private bool _IsDirtyTreeView = true;
		private bool _ScrollToSelected = false;
		private RenameOverlay _RenameOverlay = new RenameOverlay();
		private bool _GrabKeyboardFocus;
		private int _KeyboardControlID;
		private bool _HadFocusLastEvent;                           // Cached from last event for keyboard focus changed event
		private bool _AllowRenameOnMouseUp = true;

		public EditorWindow hostWindow
		{
			get;
			private set;
		}

		public TreeViewState state
		{
			get;
			private set;
		}

		List<int> expandedIDs
		{
			get
			{
				if (_TreeView.currentRoot == _TreeView.root)
				{
					return state.expandedIDs;
				}
				else
				{
					return state.filterExpandedIDs;
				}
			}
		}

		public TreeViewItem selectedItem
		{
			get
			{
				var selectedItemIDs = state.selectedItemIDs;
				if (selectedItemIDs == null || selectedItemIDs.Count == 0)
				{
					return null;
				}

				return _TreeView.FindItem(selectedItemIDs[0]);
			}
		}

		public TreeViewGUI(EditorWindow window, TreeView treeView, TreeViewState state)
		{
			hostWindow = window;
			_TreeView = treeView;
			this.state = state;
		}

		void OnEvent()
		{
			_RenameOverlay.OnEvent();
		}

		bool IsRenaming(int id)
		{
			return _RenameOverlay.IsRenaming() && _RenameOverlay.userData == id && !_RenameOverlay.isWaitingForDelay;
		}

		public bool BeginNameEditing(float delay)
		{
			if (!renamable)
			{
				return false;
			}

			var selectedItem = this.selectedItem;
			if (selectedItem != null)
			{
				return _RenameOverlay.BeginRename(selectedItem.displayName, selectedItem.id, delay);
			}

			return false;
		}

		void DoRenameOverlay()
		{
			if (_RenameOverlay.IsRenaming())
			{
				if (!_RenameOverlay.OnGUI())
				{
					EndRename();
				}
			}
		}

		void EndNameEditing(bool acceptChanges)
		{
			if (_RenameOverlay.IsRenaming())
			{
				_RenameOverlay.EndRename(acceptChanges);
				EndRename();
			}
		}

		void EndRename()
		{
			if (_RenameOverlay.HasKeyboardFocus())
			{
				_GrabKeyboardFocus = true;
			}

			if (onRenameEnded != null)
			{
				var renameEndedArgs = new RenameEndedArgs
				{
					acceptedRename = _RenameOverlay.userAcceptedRename,
					itemID = _RenameOverlay.userData,
					originalName = _RenameOverlay.originalName,
					newName = _RenameOverlay.name
				};
				onRenameEnded(renameEndedArgs);
			}
		}

		void ListupTreeView(TreeViewItem group)
		{
			int elementCount = group.children.Count;
			for (int elementIndex = 0; elementIndex < elementCount; elementIndex++)
			{
				TreeViewItem item = group.children[elementIndex];

				_ViewItems.Add(item);

				if (item.children.Count > 0 && IsExpanded(item))
				{
					ListupTreeView(item);
				}
			}
		}

		private void UpdateViewTree()
		{
			_ViewItems.Clear();

			ListupTreeView(_TreeView.currentRoot);

			_ViewCount = _ViewItems.Count;
		}

		void Repaint()
		{
			if (hostWindow != null)
			{
				hostWindow.Repaint();
			}
		}

		bool HasFocus()
		{
			bool hasKeyFocus = (hostWindow.HasFocus());
			return hasKeyFocus && (GUIUtility.keyboardControl == _KeyboardControlID);
		}

		void BeginItemsGUI()
		{
			// Input for rename overlay (repainted in EndRowGUI to ensure rendered on top)
			if (Event.current.type != EventType.Repaint)
			{
				DoRenameOverlay();
			}
		}

		void EndItemsGUI()
		{
			if (Event.current.type == EventType.Repaint)
			{
				DoRenameOverlay();
			}
		}

		public void DirtyTree()
		{
			_IsDirtyTreeView = true;
		}

		public bool IsExpanded(int id)
		{
			return expandedIDs.BinarySearch(id) >= 0;
		}

		public bool IsExpanded(TreeViewItem item)
		{
			return IsExpanded(item.id);
		}

		public bool SetExpanded(int id, bool expand, bool sort = true)
		{
			bool expanded = IsExpanded(id);
			if (expand == expanded)
			{
				return false;
			}

			if (expand)
			{
				expandedIDs.Add(id);
				if (sort)
				{
					expandedIDs.Sort();
				}
			}
			else
			{
				expandedIDs.Remove(id);
			}
			_IsDirtyTreeView = true;

			return true;
		}

		public bool SetExpanded(TreeViewItem item, bool expand)
		{
			return SetExpanded(item.id, expand);
		}

		public bool IsSelected(TreeViewItem item)
		{
			if (item == null)
			{
				return false;
			}

			return state.selectedItemIDs.Contains(item.id);
		}

		public void SetSelectedItem(TreeViewItem item, bool scrollTo)
		{
			if (item == null)
			{
				state.selectedItemIDs.Clear();
				return;
			}

			if (IsSelected(item))
			{
				return;
			}

			state.selectedItemIDs.Clear();
			state.selectedItemIDs.Add(item.id);
			_ScrollToSelected = scrollTo;

			if (selectSubmit)
			{
				SubmitItem(item);
			}
		}

		private bool SubmitItem(TreeViewItem item)
		{
			TreeViewSearchItem searchItemElement = item as TreeViewSearchItem;
			if (searchItemElement != null)
			{
				if (searchItemElement.disable)
				{
					return false;
				}

				item = searchItemElement.original;
			}

			if (item != null && !item.disable)
			{
				if (onSubmit != null)
				{
					onSubmit(item);
				}

				return true;
			}

			return false;
		}

		public bool ScrollToSelected()
		{
			if (!_ScrollToSelected || Event.current.type != EventType.Repaint)
			{
				return false;
			}

			_ScrollToSelected = false;

			var selectedItem = this.selectedItem;
			if (selectedItem == null)
			{
				return false;
			}

			bool changed = false;

			Rect lastRect = GUILayoutUtility.GetLastRect();
			Rect selectRect = selectedItem.position;
			Vector2 scrollPos = state.scrollPos;
			if (scrollPos.y < selectRect.yMax - lastRect.height)
			{
				scrollPos.y = selectRect.yMax - lastRect.height;
				changed = true;
			}
			if (scrollPos.y > selectRect.y)
			{
				scrollPos.y = selectRect.y;
				changed = true;
			}
			state.scrollPos = scrollPos;

			return changed;
		}

		void SelectNext()
		{
			var selectedItem = this.selectedItem;
			if (selectedItem != null)
			{
				if (selectedItem.children.Count > 0 && IsExpanded(selectedItem))
				{
					SetSelectedItem(selectedItem.children[0], true);
				}
				else
				{
					TreeViewItem currentItem = selectedItem;
					TreeViewItem parent = selectedItem.parent;
					while (parent != null)
					{
						int index = parent.children.IndexOf(currentItem) + 1;

						if (index < parent.children.Count)
						{
							SetSelectedItem(parent.children[index], true);
							break;
						}
						else
						{
							currentItem = parent;
							parent = parent.parent;
						}
					}
				}
			}
			else
			{
				TreeViewItem root = _TreeView.currentRoot;
				if (root != null && root.children.Count > 0)
				{
					SetSelectedItem(root.children[0], true);
				}
			}
		}

		void SelectPrev()
		{
			var selectedItem = this.selectedItem;
			if (selectedItem != null)
			{
				TreeViewItem parent = selectedItem.parent;
				if (parent != null)
				{
					int index = parent.children.IndexOf(selectedItem) - 1;

					if (index >= 0)
					{
						TreeViewItem item = parent.children[index];
						if (item.children.Count > 0 && IsExpanded(item))
						{
							SetSelectedItem(item.children[item.children.Count - 1], true);
						}
						else
						{
							SetSelectedItem(parent.children[index], true);
						}
					}
					else
					{
						if (parent.parent != null)
						{
							SetSelectedItem(parent, true);
						}
					}
				}
			}
			else
			{
				TreeViewItem root = _TreeView.currentRoot;
				if (root != null && root.children.Count > 0)
				{
					var item = root.children[0];
					SetSelectedItem(item, true);
				}
			}
		}

		void SelectParent()
		{
			var selectedItem = this.selectedItem;
			if (selectedItem == null)
			{
				return;
			}

			if (selectedItem.children.Count > 0 && IsExpanded(selectedItem))
			{
				SetExpanded(selectedItem, false);
				_IsDirtyTreeView = true;
			}
			else
			{
				TreeViewItem parent = selectedItem.parent;
				if (parent != null)
				{
					if (parent.parent != null)
					{
						SetSelectedItem(parent, true);
					}
					else
					{
						int index = parent.children.IndexOf(selectedItem) - 1;
						if (index >= 0)
						{
							var item = parent.children[index];
							SetSelectedItem(item, true);
						}
					}
				}
			}
		}

		void SelectChild()
		{
			var selectedItem = this.selectedItem;
			if (selectedItem == null)
			{
				return;
			}

			if (selectedItem.children.Count > 0 && !IsExpanded(selectedItem))
			{
				SetExpanded(selectedItem, true);
				_IsDirtyTreeView = true;
			}
			else
			{
				bool find = false;

				for (int i = 0, count = selectedItem.children.Count; i < count; ++i)
				{
					TreeViewItem item = selectedItem.children[i];
					if (item.children.Count > 0)
					{
						SetSelectedItem(item, true);

						find = true;
						break;
					}
				}

				if (!find)
				{
					TreeViewItem currentItem = selectedItem;
					while (currentItem != null && currentItem.parent != null)
					{
						TreeViewItem parent = currentItem.parent;
						int index = parent.children.IndexOf(currentItem) + 1;
						for (int i = index, count = parent.children.Count; i < count; ++i)
						{
							TreeViewItem item = parent.children[i];
							if (item.children.Count > 0)
							{
								SetSelectedItem(item, true);

								find = true;
								break;
							}
						}

						if (find)
						{
							break;
						}

						currentItem = parent;
					}
				}
			}
		}

		public float GetFoldoutIndent(TreeViewItem item)
		{
			return k_BaseIndent + (item.depth - 1) * k_IndentWidth;
		}

		public float GetContentIndent(TreeViewItem item)
		{
			return GetFoldoutIndent(item) + k_FoldoutWidth;
		}

		protected Texture GetIconForItem(TreeViewItem item)
		{
			return item.icon;
		}

		public Rect GetRenameRect(Rect rowRect, TreeViewItem item)
		{
			float offset = GetContentIndent(item);

			if (GetIconForItem(item) != null)
				offset += k_SpaceBetweenIconAndText + k_IconWidth;

			// By default we top align the rename rect to follow the label style, foldout and controls alignment
			return new Rect(rowRect.x + offset, rowRect.y, rowRect.width - offset, EditorGUIUtility.singleLineHeight);
		}

		static internal int GetItemControlID(TreeViewItem item)
		{
			return ((item != null) ? item.id : 0) + 10000000;
		}

		internal static bool HasHolddownKeyModifiers(Event evt)
		{
			return evt.shift | evt.control | evt.alt | evt.command;
		}

		protected virtual void DoItemGUI(Rect position, TreeViewItem item, bool selected, bool focused)
		{
			Vector2 iconSize = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(new Vector2(k_IconWidth, k_IconWidth));

			bool isRenamingThisItem = IsRenaming(item.id);

			if (isRenamingThisItem && Event.current.type == EventType.Repaint)
			{
				_RenameOverlay.editFieldRect = GetRenameRect(position, item);
			}

			string label = item.displayName;
			if (isRenamingThisItem)
			{
				selected = false;
				label = "";
			}

			if (Event.current.type == EventType.Repaint)
			{
				if (selected)
				{
					Styles.treeSelectionStyle.Draw(position, GUIContent.none, false, false, true, focused);
				}
			}

			OnContentGUI(position, item, label, selected, focused);

			if (item.children.Count > 0)
			{
				DoFoldout(position, item);
			}

			EditorGUIUtility.SetIconSize(iconSize);
		}

		void HandleUnusedMouseEventsForItem(Rect rect, TreeViewItem item)
		{
			int itemControlID = GetItemControlID(item);

			Event evt = Event.current;

			switch (evt.GetTypeForControl(itemControlID))
			{
				case EventType.MouseDown:
					if (rect.Contains(evt.mousePosition))
					{
						if (evt.button == 0)
						{
							GUIUtility.keyboardControl = _KeyboardControlID;
							Repaint();

							if (evt.clickCount == 2)
							{
							}
							else
							{
								if (_AllowRenameOnMouseUp)
								{
									_AllowRenameOnMouseUp = selectedItem == item;
								}
								SetSelectedItem(item, false);
								if (!selectSubmit)
								{
									SubmitItem(item);

									EditorGUIUtility.ExitGUI();
								}

								GUIUtility.hotControl = itemControlID;
							}

							evt.Use();
						}
						else if (evt.button == 1)
						{
						}
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == itemControlID)
					{
						GUIUtility.hotControl = 0;
						evt.Use();

						if (rect.Contains(evt.mousePosition))
						{
							Rect renameActivationRect = GetRenameRect(rect, item);
							if (_AllowRenameOnMouseUp && selectedItem == item && renameActivationRect.Contains(evt.mousePosition) && !HasHolddownKeyModifiers(evt))
							{
								BeginNameEditing(0.5f);
							}
						}
					}
					break;
				case EventType.ContextClick:
					if (rect.Contains(evt.mousePosition))
					{
						if (contextClickItemCallback != null)
						{
							contextClickItemCallback(selectedItem);
						}
					}
					break;
			}
		}

		protected virtual Rect DoFoldout(Rect rect, TreeViewItem item)
		{
			float indent = GetFoldoutIndent(item);
			GUIStyle foldoutStyle = Styles.groupFoldout;
			Vector2 size = foldoutStyle.CalcSize(GUIContent.none);
			Rect foldoutRect = new Rect(rect.x + indent, rect.y, size.x, size.y);

			bool expanded = IsExpanded(item.id);
			EditorGUI.BeginChangeCheck();
			bool newExpand = GUI.Toggle(foldoutRect, expanded, GUIContent.none, foldoutStyle);
			if (EditorGUI.EndChangeCheck())
			{
				SetExpanded(item.id, newExpand);
			}

			return foldoutRect;
		}

		protected virtual void OnContentGUI(Rect rect, TreeViewItem item, string label, bool selected, bool focused)
		{
			if (Event.current.rawType != EventType.Repaint)
			{
				return;
			}

			float contentIndent = GetContentIndent(item);
			rect.xMin += contentIndent;

			GUIStyle treeStyle = Styles.treeLineStyle;

			if (item.disable)
			{
				treeStyle = Styles.disabledTreeStyle;
			}

			Texture icon = GetIconForItem(item);
			if (icon != null)
			{
				Rect iconRect = rect;
				iconRect.width = k_IconWidth;

				GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

				rect.xMin += k_IconWidth + k_SpaceBetweenIconAndText;
			}

			treeStyle.Draw(rect, label, false, false, selected, focused);
		}

		void HandleKeyboard()
		{
			Event current = Event.current;

			int controlID = _KeyboardControlID;

			var eventType = current.GetTypeForControl(controlID);

			if (eventType != EventType.KeyDown)
			{
				return;
			}

			switch (current.keyCode)
			{
				case KeyCode.DownArrow:
					{
						SelectNext();
						current.Use();
					}
					break;
				case KeyCode.UpArrow:
					{
						SelectPrev();
						current.Use();
					}
					break;
				case KeyCode.Return:
				case KeyCode.KeypadEnter:
					{
						if (renamable)
						{
							if (Application.platform == RuntimePlatform.OSXEditor)
							{
								if (BeginNameEditing(0f))
								{
									current.Use();
								}
							}
						}
						else
						{
							var selectedItem = this.selectedItem;
							if (selectedItem != null)
							{
								SubmitItem(selectedItem);
							}
							current.Use();
						}
					}
					break;
				case KeyCode.F2:
					{
						if (renamable)
						{
							if (Application.platform != RuntimePlatform.OSXEditor)
							{
								if (BeginNameEditing(0f))
								{
									current.Use();
								}
							}
						}
					}
					break;
				case KeyCode.LeftArrow:
					{
						SelectParent();
						current.Use();
					}
					break;
				case KeyCode.RightArrow:
					{
						SelectChild();
						current.Use();
					}
					break;
			}
		}

		public void OnFocus(bool focused)
		{
			if (!focused && _RenameOverlay.IsRenaming())
			{
				EndNameEditing(true);
			}
		}

		public void DoTreeViewGUI(int keyControlID)
		{
			using (var verticalScope = new EditorGUILayout.VerticalScope())
			{
				Rect rect = verticalScope.rect;

				_KeyboardControlID = keyControlID;

				OnEvent();

				Event currentEvent = Event.current;
				if (currentEvent.type == EventType.Layout && _IsDirtyTreeView)
				{
					UpdateViewTree();

					_IsDirtyTreeView = false;
				}

				if (!hostWindow.HasFocus() && _RenameOverlay.IsRenaming())
				{
					EndNameEditing(true);
				}

				if (_GrabKeyboardFocus || (currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition)))
				{
					_GrabKeyboardFocus = false;
					GUIUtility.keyboardControl = _KeyboardControlID;
					_AllowRenameOnMouseUp = true;
					Repaint();
				}

				bool hasFocus = HasFocus();

				if (hasFocus != _HadFocusLastEvent && currentEvent.type != EventType.Layout)
				{
					_HadFocusLastEvent = hasFocus;

					if (hasFocus)
					{
						if (currentEvent.type == EventType.MouseDown)
						{
							_AllowRenameOnMouseUp = false;
						}
					}
				}

				state.scrollPos = EditorGUILayout.BeginScrollView(state.scrollPos);

				float lineHeight = 16f;

				int listCount = _ViewItems.Count;

				Rect totalPosition = GUILayoutUtility.GetRect(0.0f, listCount * lineHeight);

				int startIndex = Mathf.FloorToInt(state.scrollPos.y / lineHeight);
				int endIndex = Mathf.Min(startIndex + _ViewCount, listCount);

				Rect elementPosition = totalPosition;
				elementPosition.height = lineHeight;
				
				BeginItemsGUI();

				for (int i = 0; i < listCount; i++)
				{
					TreeViewItem item = _ViewItems[i];

					if (currentEvent.type != EventType.Layout)
					{
						item.position = elementPosition;

						var searchItem = item as TreeViewSearchItem;
						if (searchItem != null)
						{
							searchItem.original.position = elementPosition;
						}
					}

					if (startIndex <= i && i < endIndex)
					{
						bool selected = IsSelected(item);
						bool focused = hasFocus;
						//if (!selected)
						//{
						//	TreeViewItem currentItem = item;
						//	TreeViewSearchItem searchItem = currentItem as TreeViewSearchItem;
						//	if (searchItem != null)
						//	{
						//		currentItem = searchItem.original;
						//	}

						//	if (IsSelected(currentItem))
						//	{
						//		selected = true;
						//		focused = false;
						//	}
						//}

						DoItemGUI(elementPosition, item, selected, focused);

						HandleUnusedMouseEventsForItem(elementPosition, item);
					}

					elementPosition.y += elementPosition.height;
				}

				EndItemsGUI();

				EditorGUILayout.EndScrollView();

				EventType eventType = currentEvent.type;
				if (eventType != EventType.Layout && eventType != EventType.Used)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();

					_ViewCount = Mathf.FloorToInt(lastRect.height / lineHeight) + 1;
				}

				HandleKeyboard();

				if (ScrollToSelected())
				{
					Repaint();
				}
			}
		}

		public void DoTreeViewGUI()
		{
			int controlID = GUIUtility.GetControlID(s_TreeViewKeyboardHash, FocusType.Keyboard);
			DoTreeViewGUI(controlID);
		}

		void SetFilterExpandedItems(TreeViewItem root, bool expand)
		{
			state.filterExpandedIDs.Clear();

			if (root == null)
			{
				throw new System.ArgumentNullException("root", "The root is null");
			}

			Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
			stack.Push(root);
			while (stack.Count > 0)
			{
				TreeViewItem current = stack.Pop();

				state.filterExpandedIDs.Add(current.id);

				if (current.children == null)
				{
					continue;
				}

				int childCount = current.children.Count;
				for (int i = 0; i < childCount; i++)
				{
					var child = current.children[i];
					if (child == null)
					{
						continue;
					}

					stack.Push(child);
				}
			}

			state.filterExpandedIDs.Sort();
		}

		public void RebuildSearch(bool hasSearch, string searchWord)
		{
			using (new ProfilerScope("RebuildSearch"))
			{
				_TreeView.RebuildSearch(hasSearch, searchWord);

				if (_TreeView.searchRoot != null)
				{
					SetFilterExpandedItems(_TreeView.searchRoot, true);
				}
				else if (_TreeView.filterRoot != null)
				{
					SetFilterExpandedItems(_TreeView.filterRoot, true);
				}

				_ScrollToSelected = true;
				_IsDirtyTreeView = true;
			}
		}
	}
}
