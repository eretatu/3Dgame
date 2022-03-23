//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace ArborEditor.IMGUI.Controls
{
	[System.Serializable]
	public class TreeViewState
	{
		public Vector2 scrollPos;

		[SerializeField]
		private List<int> _ExpandedIDs = new List<int>();

		[SerializeField]
		private List<int> _FilterExpandedIDs = new List<int>();

		[SerializeField]
		private List<int> _SelectedItemIDs = new List<int>();

		public List<int> expandedIDs
		{
			get
			{
				return _ExpandedIDs;
			}
			set
			{
				_ExpandedIDs = value;
			}
		}

		public List<int> filterExpandedIDs
		{
			get
			{
				return _FilterExpandedIDs;
			}
			set
			{
				_FilterExpandedIDs = value;
			}
		}

		public List<int> selectedItemIDs
		{
			get
			{
				return _SelectedItemIDs;
			}
			set
			{
				_SelectedItemIDs = value;
			}
		}

		public void Clear()
		{
			scrollPos = Vector2.zero;
			_ExpandedIDs.Clear();
			_FilterExpandedIDs.Clear();
			_SelectedItemIDs.Clear();
		}
	}
}