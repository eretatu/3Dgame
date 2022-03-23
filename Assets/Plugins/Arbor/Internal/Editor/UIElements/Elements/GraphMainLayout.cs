//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
#if !ARBOR_DLL

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ArborEditor.Internal
{
	internal sealed class GraphMainLayout : VisualSplitter
	{
		public event HandleEventDelegate handleEventDelegate;

		public GraphMainLayout() : base()
		{
		}

		protected override void ExecuteDefaultAction(EventBase evtBase)
		{
			if (handleEventDelegate != null)
			{
				handleEventDelegate(evtBase);
			}
			base.ExecuteDefaultAction(evtBase);
		}

		public delegate void HandleEventDelegate(EventBase evtBase);
	}
}
#endif