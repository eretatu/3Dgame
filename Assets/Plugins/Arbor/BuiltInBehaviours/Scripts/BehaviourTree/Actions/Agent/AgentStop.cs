//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.BehaviourTree.Actions
{
#if ARBOR_DOC_JA
	/// <summary>
	/// Agentを停止させる
	/// </summary>
#else
	/// <summary>
	/// Stop the Agent
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Agent/AgentStop")]
	[BuiltInBehaviour]
	public sealed class AgentStop : AgentBase
	{
		protected override void OnExecute()
		{
			AgentController agentController = cachedAgentController;
			if (agentController != null)
			{
				agentController.Stop();
			}
			FinishExecute(true);
		}
	}
}