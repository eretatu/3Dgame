//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.Example
{
	/// <summary>
	/// A behaviour that displays the active node information.
	/// </summary>
	[AddComponentMenu("")]
	[AddBehaviourMenu("Example/DebugGraphBehaviour")]
	public sealed class DebugGraphBehaviour : StateBehaviour
	{
		[SerializeField]
		private StateLink _NextState = new StateLink();

		/// <summary>
		/// DebugGraph
		/// </summary>
		private DebugGraph _DebugGraph = null;

		/// <summary>
		/// Called only once when a state becomes active.
		/// </summary>
		public override void OnStateAwake()
		{
			_DebugGraph = new DebugGraph(nodeGraph);
		}

		/// <summary>
		/// Called when entering a state.
		/// </summary>
		public override void OnStateBegin()
		{
			// Initialize
			_DebugGraph.InitializeEvent();

			// Output node to log
			_DebugGraph.Log(node.ToString());
		}

		/// <summary>
		/// Called when updating a state.
		/// </summary>
		public override void OnStateUpdate()
		{
			if (_DebugGraph.isNextClick)
			{
				Transition(_NextState);
			}
		}

		/// <summary>
		/// Called when leaving a state.
		/// </summary>
		public override void OnStateEnd()
		{
			_DebugGraph.ReleaseEvent();
		}
	}
}