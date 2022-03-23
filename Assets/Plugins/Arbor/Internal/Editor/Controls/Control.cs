//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace ArborEditor
{
	internal abstract class Control : ScriptableObject
	{
		[SerializeField]
		private FocusType _FocusType = FocusType.Passive;

		public int controlID
		{
			get;
			private set;
		}

		public FocusType focusType
		{
			get
			{
				return _FocusType;
			}
			set
			{
				_FocusType = value;
			}
		}

		public void UpdateControlID()
		{
			controlID = GUIUtility.GetControlID(GetInstanceID(), _FocusType, GetPosition());
			OnUpdateControlID();
		}

		protected virtual void OnUpdateControlID()
		{
		}

		public abstract Rect GetPosition();

		public virtual void OnGUI()
		{
		}
	}
}