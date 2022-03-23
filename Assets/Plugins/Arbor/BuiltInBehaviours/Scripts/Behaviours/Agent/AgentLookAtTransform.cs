//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.StateMachine.StateBehaviours
{
#if ARBOR_DOC_JA
	/// <summary>
	/// 指定Trasnformの方向へ回転する。
	/// </summary>
#else
	/// <summary>
	/// Rotates in the direction of the specified Transform.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Agent/AgentLookAtTransform")]
	[BuiltInBehaviour]
	public sealed class AgentLookAtTransform : AgentRotateBase
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// 対象のTransform
		/// </summary>
#else
		/// <summary>
		/// Transform of target
		/// </summary>
#endif
		[SerializeField] private FlexibleTransform _Target = new FlexibleTransform();

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

		#endregion

		protected override void OnUpdateAgent()
		{
			AgentController agentController = cachedAgentController;
			if (agentController != null)
			{
				agentController.LookAt(_AngularSpeed.value, _Target.value);
			}
		}

		protected override void OnDone()
		{
			Transition(_Done);
		}
	}
}
