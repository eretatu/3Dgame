//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.StateMachine.StateBehaviours
{
	[AddComponentMenu("")]
	[HideBehaviour()]
	public abstract class AgentRotateBase : AgentIntervalUpdate
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// 角速度
		/// </summary>
#else
		/// <summary>
		/// Angular Speed
		/// </summary>
#endif
		[SerializeField]
		protected FlexibleFloat _AngularSpeed = new FlexibleFloat(120f);

		#endregion // Serialize fields
	}
}
