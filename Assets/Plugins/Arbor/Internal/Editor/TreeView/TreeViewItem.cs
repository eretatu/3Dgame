//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ArborEditor.IMGUI.Controls
{
	public class TreeViewItem : System.IComparable<TreeViewItem>
	{
		public int id;
		public TreeViewItem parent = null;

		public int depth;
		private string _DisplayName;
		public Texture2D icon;

		public bool disable = false;

		public List<TreeViewItem> children = new List<TreeViewItem>();

		public Rect position;

		public virtual string displayName
		{
			get
			{
				return _DisplayName;
			}
			set
			{
				_DisplayName = value;
			}
		}

		public string searchName
		{
			get
			{
				return displayName.ToLower().Replace(" ", string.Empty);
			}
		}

		public TreeViewItem(int id, string name, Texture2D icon)
		{
			this.id = id;
			this.icon = icon;
			_DisplayName = name;
		}

		public int CompareTo(TreeViewItem item)
		{
			return displayName.CompareTo(item.displayName);
		}

		public void AddChild(TreeViewItem child)
		{
			child.parent = this;
			children.Add(child);
		}

		public void AddChildren(IList<TreeViewItem> children)
		{
			if (children == null)
			{
				return;
			}

			int childCount = children.Count;
			for (int i = 0; i < childCount; i++)
			{
				AddChild(children[i]);
			}
		}
	}
}