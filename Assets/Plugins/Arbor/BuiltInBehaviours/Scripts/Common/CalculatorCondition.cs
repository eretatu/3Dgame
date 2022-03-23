//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor
{
#if ARBOR_DOC_JA
	/// <summary>
	/// Calculatorの状態チェッククラス
	/// </summary>
#else
	/// <summary>
	/// Condition check class of Calculator
	/// </summary>
#endif
	[System.Serializable]
	[Arbor.Internal.Documentable]
	public sealed class CalculatorCondition
	{
		#region enum

#if ARBOR_DOC_JA
		/// <summary>
		/// 値の型
		/// </summary>
#else
		/// <summary>
		/// Value type
		/// </summary>
#endif
		[Arbor.Internal.Documentable]
		public enum Type
		{
			/// <summary>
			/// FlexibleInt
			/// </summary>
			Int,

			/// <summary>
			/// FlexileFloat
			/// </summary>
			Float,

			/// <summary>
			/// FlexibleBool
			/// </summary>
			Bool,
		}

		#endregion // enum

		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// 値の型
		/// </summary>
#else
		/// <summary>
		/// Value type
		/// </summary>
#endif
		[SerializeField]
		internal Type _Type = Type.Int;

#if ARBOR_DOC_JA
		/// <summary>
		/// 比較タイプ
		/// </summary>
#else
		/// <summary>
		/// Compare type
		/// </summary>
#endif
		[SerializeField]
		internal CompareType _CompareType = CompareType.Equals;

		[SerializeField]
		internal int _ParameterIndex = 0;

		#endregion // Serialize fields

		[System.NonSerialized]
		private CalculatorConditionList _Owner = null;

		public CalculatorConditionList owner
		{
			get
			{
				return _Owner;
			}
			internal set
			{
				_Owner = value;
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 値を比較する.
		/// </summary>
		/// <returns>比較結果</returns>
#else
		/// <summary>
		/// Compare the values.
		/// </summary>
		/// <returns>Comparison result</returns>
#endif
		public bool Compare()
		{
			switch (_Type)
			{
				case Type.Int:
					CalculatorConditionList.IntParameter intParameter = owner._IntParameters[_ParameterIndex];
					return intParameter.Compare(_CompareType);
				case Type.Float:
					CalculatorConditionList.FloatParameter floatParameter = owner._FloatParameters[_ParameterIndex];
					return floatParameter.Compare(_CompareType);
				case Type.Bool:
					CalculatorConditionList.BoolParameter boolParameter = owner._BoolParameters[_ParameterIndex];
					return boolParameter.Compare();
			}
			return false;
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// CalculatorConditionコンストラクタ
		/// </summary>
		/// <param name="type">値の型</param>
#else
		/// <summary>
		/// CalculatorCondition constructor
		/// </summary>
		/// <param name="type">Value type</param>
#endif
		public CalculatorCondition(Type type)
		{
			_Type = type;
		}
	}
}