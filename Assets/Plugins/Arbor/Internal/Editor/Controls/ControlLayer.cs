//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace ArborEditor
{
	[System.Reflection.Obfuscation(Exclude = true)]
	[System.Serializable]
	internal sealed class ControlLayer
	{
		[SerializeField]
		private List<Control> _Controls = new List<Control>();

		[SerializeField]
		private List<Control> _DisplayControls = new List<Control>();

		public int controlCount
		{
			get
			{
				return _Controls.Count;
			}
		}

		public bool Contains(Vector2 position)
		{
			for (int i = 0, count = _Controls.Count; i < count; i++)
			{
				Control control = _Controls[i];
				if (control.GetPosition().Contains(position))
				{
					return true;
				}
			}

			return false;
		}

		public Control GetControlFromIndex(int index)
		{
			return _Controls[index];
		}

		public int order;

		void DoControls(bool repaint)
		{
			int displayCount = _DisplayControls.Count;

			if (repaint)
			{
				if (Event.current.type == EventType.Repaint)
				{
					for (int i = 0; i < displayCount; i++)
					{
						Control control = _DisplayControls[i];
						if (control != null)
						{
							control.OnGUI();
						}
					}
				}
			}
			else
			{
				if (Event.current.type != EventType.Repaint)
				{
					for (int i = displayCount - 1; i >= 0; i--)
					{
						Control control = _DisplayControls[i];
						if (control != null)
						{
							control.OnGUI();
						}
					}
				}
			}
		}

		public void Update()
		{
			int count = _Controls.Count;
			for (int i = 0; i < count; i++)
			{
				Control control = _Controls[i];
				if (control != null)
				{
					control.UpdateControlID();
				}
			}
		}

		public void BeginLayer()
		{
			DoControls(order < 0);
		}

		public void EndLayer()
		{
			DoControls(order >= 0);
		}

		public void Focus(Control control)
		{
			if (_DisplayControls.Remove(control))
			{
				_DisplayControls.Add(control);
			}
		}

		public void Add(Control control)
		{
			_Controls.Add(control);
			_DisplayControls.Add(control);
		}

		public void Remove(Control control)
		{
			if (_Controls.Remove(control))
			{
				_DisplayControls.Remove(control);
				Object.DestroyImmediate(control);
			}
		}

		public void Clear()
		{
			int count = _Controls.Count;
			for (int i = 0; i < count; i++)
			{
				Control control = _Controls[i];
				if (control != null)
				{
					Object.DestroyImmediate(control);
				}
			}

			_Controls.Clear();
			_DisplayControls.Clear();
		}
	}
}