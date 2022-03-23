﻿//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.Calculators
{
#if ARBOR_DOC_JA
	/// <summary>
	/// 2つのベクトルが等しい場合にtrueを返す。
	/// </summary>
#else
	/// <summary>
	/// Returns true if two vectors are equal.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Vector3Int/Vector3Int.Equals")]
	[BehaviourTitle("Vector3Int.Equals")]
	[BuiltInBehaviour]
	public sealed class Vector3IntEqualsCalculator : Calculator
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// 値1
		/// </summary>
#else
		/// <summary>
		/// Value 1
		/// </summary>
#endif
		[SerializeField] private FlexibleVector3Int _Value1 = new FlexibleVector3Int();

#if ARBOR_DOC_JA
		/// <summary>
		/// 値2
		/// </summary>
#else
		/// <summary>
		/// Value 2
		/// </summary>
#endif
		[SerializeField] private FlexibleVector3Int _Value2 = new FlexibleVector3Int();

#if ARBOR_DOC_JA
		/// <summary>
		/// 結果出力
		/// </summary>
#else
		/// <summary>
		/// Output result
		/// </summary>
#endif
		[SerializeField] private OutputSlotBool _Result = new OutputSlotBool();

		#endregion // Serialize fields

		public override void OnCalculate()
		{
			Vector3Int value1 = _Value1.value;
			Vector3Int value2 = _Value2.value;
			_Result.SetValue(value1 == value2);
		}
	}
}