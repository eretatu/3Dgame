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
	/// AgentをTargetから逃げるように移動させる。
	/// </summary>
#else
	/// <summary>
	/// Move the Agent to escape from Target.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Agent/AgentEscape")]
	[BuiltInBehaviour]
	public sealed class AgentEscape : AgentMoveBase
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
		[SerializeField]
		private FlexibleFloat _Distance = new FlexibleFloat(0f);

#if ARBOR_DOC_JA
		/// <summary>
		/// 逃げたい対象のTransform
		/// </summary>
#else
		/// <summary>
		/// Transform of object to escape
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
		private int _AgentEscape_SerializeVersion = 0;

		#region old

		[SerializeField]
		[FormerlySerializedAs("_Distance")]
		[HideInInspector]
		private float _OldDistance = 0f;

		#endregion // old

		#endregion

		private const int kCurrentSerializeVersion = 1;

		protected override void OnUpdateAgent()
		{
			AgentController agentController = cachedAgentController;
			if (agentController != null)
			{
				agentController.Escape(_Speed.value, _Distance.value, _Target.value);
			}
		}

		protected override void OnDone()
		{
			Transition(_Done);
		}

		protected override void Reset()
		{
			base.Reset();

			_AgentEscape_SerializeVersion = kCurrentSerializeVersion;
		}

		void SerializeVer1()
		{
			_Distance = (FlexibleFloat)_OldDistance;
		}

		void Serialize()
		{
			while (_AgentEscape_SerializeVersion != kCurrentSerializeVersion)
			{
				switch (_AgentEscape_SerializeVersion)
				{
					case 0:
						SerializeVer1();
						_AgentEscape_SerializeVersion++;
						break;
					default:
						_AgentEscape_SerializeVersion = kCurrentSerializeVersion;
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
