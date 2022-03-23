//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.Utilities
{
	public static class AnimatorUtility
	{
		public static int GetLayerIndex(Animator animator, string layerName)
		{
			for (int i = 0; i < animator.layerCount; i++)
			{
				if (animator.GetLayerName(i) == layerName)
				{
					return i;
				}
			}

			return -1;
		}

		public static bool IsInTransition(Animator animator, int layerIndex, string stateName)
		{
			if (animator.IsInTransition(layerIndex))
			{
				AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(layerIndex);
				return nextState.IsName(stateName);
			}

			return false;
		}
	}
}