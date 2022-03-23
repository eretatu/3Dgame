//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	using Arbor;

	[System.Reflection.Obfuscation(Exclude = true)]
	[System.Serializable]
	internal abstract class Panel
	{
		public ArborEditorWindow hostWindow
		{
			get;
			private set;
		}

		public virtual void Setup(ArborEditorWindow hostWindow)
		{
			this.hostWindow = hostWindow;
		}

		public virtual void OnFocus(bool focused)
		{
		}

		public abstract void OnGUI(Rect position);
	}
}
