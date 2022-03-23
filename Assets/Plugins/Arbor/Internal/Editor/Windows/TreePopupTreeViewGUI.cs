//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ArborEditor
{
	using ArborEditor.IMGUI.Controls;

	public class TreePopupTreeViewGUI<T> : TreeViewGUI
	{
		public TreePopupTreeViewGUI(TreePopupWindow<T> window, TreeView treeView, TreeViewState state) : base(window, treeView, state)
		{
		}

		protected override void OnContentGUI(Rect rect, TreeViewItem item, string label, bool selected, bool focused)
		{
			base.OnContentGUI(rect, item, label, selected, focused);

			if (Event.current.rawType != EventType.Repaint)
			{
				return;
			}

			var window = hostWindow as TreePopupWindow<T>;

			var searchItem = item as TreeViewSearchItem;
			if (searchItem != null)
			{
				item = searchItem.original;
			}

			var valueItem = item as TreeViewValueItem<T>;
			if (window == null || valueItem == null)
			{
				return;
			}

			bool selectedValue = EqualityComparer<T>.Default.Equals(valueItem.value, window.selectedValue);

			if (selectedValue)
			{
				Rect selectRect = new Rect(rect.xMax - k_IconWidth, rect.y, k_IconWidth, k_IconWidth);

				GUI.DrawTexture(selectRect, Defaults.successTex, ScaleMode.ScaleToFit);
			}
		}

		protected static class Defaults
		{
			public static readonly Texture2D successTex;

			static Defaults()
			{
				successTex = EditorGUIUtility.IconContent("TestPassed").image as Texture2D;
			}
		}
	}
}