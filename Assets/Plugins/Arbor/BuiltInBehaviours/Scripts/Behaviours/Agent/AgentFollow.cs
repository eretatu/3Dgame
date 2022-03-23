//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.Serialization;

namespace Arbor.StateMachine.StateBehaviours
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
	[AddBehaviourMenu("Agent/AgentFollow")]
	[BuiltInBehaviour]
	public sealed class AgentFollow : AgentMoveBase
	{
		#region Serialize fields

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
		private FlexibleFloat _StoppingDistance = new FlexibleFloat(0f);

#if ARBOR_DOC_JA
		/// <summary>
		/// 近づきたい対象のTransform
		/// </summary>
#else
		/// <summary>
		/// The target Transform to be approached
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

		[SerializeField]
		[HideInInspector]
		private int _AgentFollow_SerializeVersion = 0;

		#region old

		[SerializeField]
		[FormerlySerializedAs("_StoppingDistance")]
		[HideInInspector]
		private float _OldStoppingDistance = 0f;

		#endregion // old

		#endregion

		private const int kCurrentSerializeVersion = 1;

		protected override void OnUpdateAgent()
		{
			AgentController agentController = cachedAgentController;
			if (agentController != null)
			{
				agentController.Follow(_Speed.value, _StoppingDistance.value, _Target.value);
			}
		}

		protected override void OnDone()
		{
			Transition(_Done);
		}

		void SerializeVer1()
		{
			_StoppingDistance = (FlexibleFloat)_OldStoppingDistance;
		}

		void Serialize()
		{
			while (_AgentFollow_SerializeVersion != kCurrentSerializeVersion)
			{
				switch (_AgentFollow_SerializeVersion)
				{
					case 0:
						SerializeVer1();
						_AgentFollow_SerializeVersion++;
						break;
					default:
						_AgentFollow_SerializeVersion = kCurrentSerializeVersion;
						break;
				}
			}
		}

		protected override void Reset()
		{
			base.Reset();

			_AgentFollow_SerializeVersion = kCurrentSerializeVersion;
		}

		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();

			Serialize();
		}

		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();

			Serialize();
		}
	}
}
