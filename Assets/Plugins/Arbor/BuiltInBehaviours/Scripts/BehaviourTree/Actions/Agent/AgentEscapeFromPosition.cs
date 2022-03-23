//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.BehaviourTree.Actions
{
#if ARBOR_DOC_JA
	/// <summary>
	/// AgentをTargetから逃げるように移動させる。
	/// </summary>
#else
	/// <summary>
	/// Move the Agent to escape from Target.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Agent/AgentEscapeFromPosition")]
	[BuiltInBehaviour]
	public sealed class AgentEscapeFromPosition : AgentMoveBase
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// 離れる距離
		/// </summary>
#else
		/// <summary>
		/// Distance away
		/// </summary>
#endif
		[SerializeField] private FlexibleFloat _Distance = new FlexibleFloat();

#if ARBOR_DOC_JA
		/// <summary>
		/// 対象の位置
		/// </summary>
#else
		/// <summary>
		/// Target position
		/// </summary>
#endif
		[SerializeField]
		private FlexibleVector3 _TargetPosition = new FlexibleVector3();

		#endregion // Serialize fields

		protected override void OnExecute()
		{
			AgentController agentController = cachedAgentController;
			if (agentController != null)
			{
				agentController.Escape(_Speed.value, _Distance.value, _TargetPosition.value);

				if (agentController.isDone)
				{
					FinishExecute(true);
				}
			}
			else
			{
				FinishExecute(false);
			}
		}
	}
}