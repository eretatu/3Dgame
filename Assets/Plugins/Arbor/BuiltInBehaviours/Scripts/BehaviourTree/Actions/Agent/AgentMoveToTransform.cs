//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.BehaviourTree.Actions
{
#if ARBOR_DOC_JA
	/// <summary>
	/// AgentをTargetに近づくように移動させる。
	/// </summary>
#else
	/// <summary>
	/// Move Agent so that it approaches Target.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Agent/AgentMoveToTransform")]
	[BuiltInBehaviour]
	public sealed class AgentMoveToTransform : AgentMoveBase
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// 対象のTransform
		/// </summary>
#else
		/// <summary>
		/// Target Transform
		/// </summary>
#endif
		[SerializeField]
		private FlexibleTransform _TargetTransform = new FlexibleTransform();

#if ARBOR_DOC_JA
		/// <summary>
		/// 停止する距離
		/// </summary>
#else
		/// <summary>
		/// Distance to stop
		/// </summary>
#endif
		[SerializeField]
		private FlexibleFloat _StoppingDistance = new FlexibleFloat();

		#endregion // Serialize fields

		protected override void OnExecute()
		{
			AgentController agentController = cachedAgentController;
			if (agentController != null)
			{
				agentController.Follow(_Speed.value, _StoppingDistance.value, _TargetTransform.value);

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