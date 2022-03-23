//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using System;

namespace Arbor
{
#if ARBOR_DOC_JA
	/// <summary>
	/// FlexibleEnumAnyやenum型Parameterのユーティリティクラス
	/// </summary>
#else
	/// <summary>
	/// Utility class of FlexibleEnumAny or enum type Parameter
	/// </summary>
#endif
	public static class EnumFieldUtility
	{
#if ARBOR_DOC_JA
		/// <summary>
		/// enum型チェック
		/// </summary>
		/// <param name="enumType">enum型</param>
		/// <returns>enum型であればtrueを返す</returns>
#else
		/// <summary>
		/// Enum type check
		/// </summary>
		/// <param name="enumType">enum type</param>
		/// <returns>Return true if it is an enum type</returns>
#endif
		public static bool IsEnum(Type enumType)
		{
			return enumType != null && TypeUtility.IsEnum(enumType) && Enum.GetUnderlyingType(enumType) == typeof(int);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// System.FlagsAttributeを持つenum型チェック
		/// </summary>
		/// <param name="enumType">enum型</param>
		/// <returns>System.FlagsAttributeを持つenum型であればtrueを返す。</returns>
#else
		/// <summary>
		/// Enum type check with System.FlagsAttribute
		/// </summary>
		/// <param name="enumType">enum type</param>
		/// <returns>Returns true if it is an enum type with System.FlagsAttribute.</returns>
#endif
		public static bool IsEnumFlags(Type enumType)
		{
			return IsEnum(enumType) && AttributeHelper.HasAttribute<FlagsAttribute>(enumType);
		}
	}
}