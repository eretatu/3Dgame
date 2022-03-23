//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using System;

namespace Arbor
{
	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class ParameterValueTypeAttribute : Attribute
	{
		public readonly Type type;

		public bool useReferenceType = false;
		public bool toList = false;

		public ParameterValueTypeAttribute(Type type)
		{
			this.type = type;
		}
	}
}