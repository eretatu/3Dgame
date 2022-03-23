//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;


namespace Arbor.BehaviourTree.Actions
{
	[AddComponentMenu("")]
	[HideBehaviour]
	public class AgentRotateBase : AgentBase
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

#if ARBOR_DOC_JA
		/// <summary>
		/// アクション終了時に停止するかどうか
		/// </summary>
#else
		/// <summary>
		/// Whether to stop at the end of the action
		/// </summary>
#endif
		[SerializeField] private FlexibleBool _StopOnEnd = new FlexibleBool(true);

		#endregion // Serialize fields

		protected override void OnEnd()
		{
			AgentController agentController = cachedAgentController;
			if (_StopOnEnd.value && agentController != null)
			{
				agentController.Stop();
			}
		}
	}
}