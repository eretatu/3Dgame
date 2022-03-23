//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.Events.Legacy
{
#if !NETFX_CORE
	[System.Reflection.Obfuscation(Exclude = true)]
#endif
	[System.Serializable]
	internal sealed class InputSlotParameter
	{
		[SerializeField]
		[HideSlotFields]
		private InputSlotTypable _Value = new InputSlotTypable();

		public InputSlotTypable GetSlot()
		{
			return _Value;
		}
	}
}