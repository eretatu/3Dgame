//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.StateMachine.StateBehaviours
{
#if ARBOR_DOC_JA
	/// <summary>
	/// 指定した位置へgentを移動させる。
	/// </summary>
#else
	/// <summary>
	/// Move the Agent to the specified position.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Agent/AgentMoveToPosition")]
	[BuiltInBehaviour]
	public sealed class AgentMoveToPosition : AgentMoveBase
	{
#if ARBOR_DOC_JA
		/// <summary>
		/// 停止する距離
		/// </summary>
#else
		/// <summary>
		/// Distance to stop
		/// </summary>
#endif
		[SerializeField] private FlexibleFloat _StoppingDistance = new FlexibleFloat(0f);

#if ARBOR_DOC_JA
		/// <summary>
		/// Agentを移動させる位置
		/// </summary>
#else
		/// <summary>
		/// Position to move Agent
		/// </summary>
#endif
		[SerializeField]
		private FlexibleVector3 _Position = new FlexibleVector3();

#if ARBOR_DOC_JA
		/// <summary>
		/// 移動完了した時のステート遷移<br />
		/// 遷移メソッド : OnStateUpdate
		/// </summary>
#else
		/// <summary>
		/// State transition at the time of movement completion<br />
		/// Transition Method : OnStateUpdate
		/// </summary>
#endif
		[SerializeField] private StateLink _Done = new StateLink();

		protected override void OnUpdateAgent()
		{
			AgentController agentController = cachedAgentController;
			if (agentController != null)
			{
				agentController.Follow(_Speed.value, _StoppingDistance.value, _Position.value);
			}
		}

		protected override void OnDone()
		{
			Transition(_Done);
		}
	}
}