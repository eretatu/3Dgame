//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace Arbor.StateMachine.StateBehaviours
{
#if ARBOR_DOC_JA
	/// <summary>
	/// Parameterの値を判定して遷移する。
	/// </summary>
	/// <remarks>
	/// 複数のConditionを設定した場合は、すべての比較結果が真になったときのみ遷移する。
	/// </remarks>
#else
	/// <summary>
	/// It determines the value of Parameter and makes a transition.
	/// </summary>
	/// <remarks>
	/// When more than one Condition is set, it transits only when all comparison results become true.
	/// </remarks>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Transition/ParameterTransition")]
	[BuiltInBehaviour]
	public sealed class ParameterTransition : StateBehaviour, INodeBehaviourSerializationCallbackReceiver
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// 判定条件を設定する。
		/// <list type="bullet">
		/// <item><term>＋ボタン</term><description>条件を追加。</description></item>
		/// </list>
		/// </summary>
#else
		/// <summary>
		/// Set the judgment condition.
		/// <list type="bullet">
		/// <item><term>+ Button</term><description>Add condition.</description></item>
		/// </list>
		/// </summary>
#endif
		[SerializeField]
		private ParameterConditionList _ConditionList = new ParameterConditionList();

#if ARBOR_DOC_JA
		/// <summary>
		/// 遷移先ステート。<br />
		/// 遷移メソッド : OnStateBegin, Parameter.onChanged
		/// </summary>
#else
		/// <summary>
		/// Transition destination state.<br />
		/// Transition Method : OnStateBegin, Parameter.onChanged
		/// </summary>
#endif
		[SerializeField] private StateLink _NextState = new StateLink();

		[SerializeField]
		[HideInInspector]
		private int _SerializeVersion;

		#region old

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("_Condisions")]
		[FormerlySerializedAs("_Conditions")]
		private List<ParameterConditionLegacy> _OldConditions = new List<ParameterConditionLegacy>();

		#endregion // old

		#endregion // Serialize fields

		private const int kCurrentSerializeVersion = 2;

		private List<Parameter> _Parameters = new List<Parameter>();

		void SetOnChanged()
		{
			ReleaseOnChanged();

			int conditionCount = _ConditionList.conditionCount;
			for (int conditionIndex = 0; conditionIndex < conditionCount; conditionIndex++)
			{
				ParameterCondition condition = _ConditionList.GetCondition(conditionIndex);
				Parameter parameter = condition.reference.parameter;
				if (parameter != null)
				{
					parameter.onChanged += OnChangedParam;
					_Parameters.Add(parameter);
				}
			}
		}

		void ReleaseOnChanged()
		{
			for (int i = 0; i < _Parameters.Count; i++)
			{
				Parameter parameter = _Parameters[i];
				parameter.onChanged -= OnChangedParam;
			}
			_Parameters.Clear();
		}

		void OnEnable()
		{
			SetOnChanged();
		}

		void OnDisable()
		{
			ReleaseOnChanged();
		}

		protected override void OnValidate()
		{
			base.OnValidate();

			if (Application.isPlaying && isActiveAndEnabled)
			{
				SetOnChanged();
			}
		}

		// Use this for enter state
		public override void OnStateBegin()
		{
			if (CheckCondition())
			{
				Transition(_NextState);
			}
		}

		void OnChangedParam(Parameter parameter)
		{
			if (CheckCondition())
			{
				Transition(_NextState);
			}
		}

		bool CheckCondition()
		{
			return _ConditionList.CheckCondition();
		}

		void Reset()
		{
			_SerializeVersion = kCurrentSerializeVersion;
		}

		void SerializeVer1()
		{
			int conditionCount = _OldConditions.Count;
			for (int conditionIndex = 0; conditionIndex < conditionCount; conditionIndex++)
			{
				ParameterConditionLegacy condition = _OldConditions[conditionIndex];
				condition.ParameterTransitionSerializeVer1();
			}
		}

		void SerializeVer2()
		{
			_ConditionList.ImportLegacy(_OldConditions);
			_OldConditions.Clear();
		}

		void Serialize()
		{
			while (_SerializeVersion != kCurrentSerializeVersion)
			{
				switch (_SerializeVersion)
				{
					case 0:
						SerializeVer1();
						_SerializeVersion++;
						break;
					case 1:
						SerializeVer2();
						_SerializeVersion++;
						break;
					default:
						_SerializeVersion = kCurrentSerializeVersion;
						break;
				}
			}
		}

		void INodeBehaviourSerializationCallbackReceiver.OnBeforeSerialize()
		{
			Serialize();
		}

		void INodeBehaviourSerializationCallbackReceiver.OnAfterDeserialize()
		{
			Serialize();
		}

		protected override void OnPreDestroy()
		{
			if (_OldConditions != null)
			{
				for (int i = 0; i < _OldConditions.Count; i++)
				{
					_OldConditions[i].Destroy();
				}
			}
			_ConditionList.Destroy();
		}
	}
}

