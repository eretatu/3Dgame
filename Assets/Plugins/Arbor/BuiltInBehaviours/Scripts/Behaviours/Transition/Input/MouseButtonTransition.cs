//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.StateMachine.StateBehaviours
{
#if ARBOR_DOC_JA
	/// <summary>
	/// マウスボタンが押されているかでステートを遷移する。
	/// </summary>
#else
	/// <summary>
	/// It will transition the state on whether the mouse button is pressed.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Transition/Input/MouseButtonTransition")]
	[BuiltInBehaviour]
	public sealed class MouseButtonTransition : MouseButtonBehaviourBase
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

		// Update is called once per frame
		protected override void OnUpdate()
		{
			if (Input.GetMouseButton(button))
			{
				Transition(_NextState);
			}
		}
	}
}
