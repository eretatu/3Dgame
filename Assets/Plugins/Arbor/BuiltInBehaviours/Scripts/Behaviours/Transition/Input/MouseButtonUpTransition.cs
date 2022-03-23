//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.StateMachine.StateBehaviours
{
#if ARBOR_DOC_JA
	/// <summary>
	/// マウスボタンが離されたときにステートを遷移する。
	/// </summary>
#else
	/// <summary>
	/// It will transition the state when the mouse button is released.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Transition/Input/MouseButtonUpTransition")]
	[BuiltInBehaviour]
	public sealed class MouseButtonUpTransition : MouseButtonBehaviourBase
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// 遷移先ステート。<br />
		/// 遷移メソッド : Update
		/// </summary>
#else
		/// <summary>
		/// Transition destination state.<br />
		/// Transition Method : Update
		/// </summary>
#endif
		[SerializeField] private StateLink _NextState = new StateLink();

		#endregion // Serialize fields

		protected override void OnUpdate()
		{
			if (Input.GetMouseButtonUp(button))
			{
				Transition(_NextState);
			}
		}
	}
}
