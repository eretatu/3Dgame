//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.Serialization;

namespace Arbor.BehaviourTree.Actions
{
#if ARBOR_DOC_JA
	/// <summary>
	/// Waypointに沿ってAgentを移動させる。
	/// </summary>
#else
	/// <summary>
	/// Move the Agent along the Waypoint.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Agent/AgentMoveOnWaypoint")]
	[BuiltInBehaviour]
	public sealed class AgentMoveOnWaypoint : AgentMoveBase
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// Agentを移動させる経路
		/// </summary>
#else
		/// <summary>
		/// Route to move Agent
		/// </summary>
#endif
		[SerializeField]
		[SlotType(typeof(Waypoint))]
		private FlexibleComponent _Waypoint = new FlexibleComponent();

#if ARBOR_DOC_JA
		/// <summary>
		/// 再開時にWaypointの最初の位置からやり直すフラグ
		/// </summary>
#else
		/// <summary>
		/// Flag to restart from the first position of Waypoint when restarting
		/// </summary>
#endif
		[SerializeField]
		private FlexibleBool _ClearDestPoint = new FlexibleBool();

#if ARBOR_DOC_JA
		/// <summary>
		/// 再生タイプ。
		/// </summary>
#else
		/// <summary>
		/// Play type.
		/// </summary>
#endif
		[SerializeField]
		[Internal.DocumentType(typeof(MoveWaypointType))]
		private FlexibleMoveWayppintType _Type = new FlexibleMoveWayppintType(MoveWaypointType.Once);

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

		[SerializeField]
		[HideInInspector]
		private int _AgentMoveOnWaypoint_SerializeVersion = 0;

		#region old

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("_Type")]
		private MoveWaypointType _OldType = MoveWaypointType.Once;

		#endregion // old

		#endregion // Serialize fields

		private const int kCurrentSerializeVersion = 1;

		private WaypointProcessor _Processor = new WaypointProcessor();

		void GotoNextPoint(bool moveDone)
		{
			AgentController agentController = cachedAgentController;
			if (agentController == null || !_Processor.isValid || _Processor.isDone)
			{
				return;
			}

			if (moveDone)
			{
				_Processor.Next(_Type.value);
			}

			agentController.Follow(_Speed.value, _StoppingDistance.value, _Processor.nextPoint);
		}

		protected override void OnStart()
		{
			base.OnStart();

			_Processor.Initialize(_Waypoint.value as Waypoint, _ClearDestPoint.value);
			GotoNextPoint(false);
		}

		protected override void OnExecute()
		{
			AgentController agentController = cachedAgentController;
			if (agentController != null && agentController.isDone)
			{
				GotoNextPoint(true);
				if (_Processor.isDone)
				{
					FinishExecute(true);
				}
			}
		}

		protected override void Reset()
		{
			base.Reset();

			_AgentMoveOnWaypoint_SerializeVersion = kCurrentSerializeVersion;
		}

		void SerializeVer1()
		{
			_Type = (FlexibleMoveWayppintType)_OldType;
		}

		void Serialize()
		{
			while (_AgentMoveOnWaypoint_SerializeVersion != kCurrentSerializeVersion)
			{
				switch (_AgentMoveOnWaypoint_SerializeVersion)
				{
					case 0:
						SerializeVer1();
						_AgentMoveOnWaypoint_SerializeVersion++;
						break;
					default:
						_AgentMoveOnWaypoint_SerializeVersion = kCurrentSerializeVersion;
						break;
				}
			}
		}

		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();

			Serialize();
		}

		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();

			Serialize();
		}
	}
}