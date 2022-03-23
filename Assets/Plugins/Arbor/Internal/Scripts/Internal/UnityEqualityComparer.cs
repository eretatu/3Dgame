//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace Arbor.Internal
{
	internal static class UnityEqualityComparer
	{
		static readonly IEqualityComparer<Color32> color32EqualityComparer = new Color32EqualityComparer();

		public static IEqualityComparer<T> GetComparer<T>()
		{
			return Cache<T>.Comparer;
		}

		static class Cache<T>
		{
			public readonly static IEqualityComparer<T> Comparer;

			static Cache()
			{
				var comparer = GetDefaultComparer(typeof(T));
				if (comparer != null)
				{
					Comparer = (IEqualityComparer<T>)comparer;
				}
				else
				{
					Comparer = EqualityComparer<T>.Default;
				}
			}
		}

		static object GetDefaultComparer(System.Type t)
		{
			if (t == typeof(Color32)) return color32EqualityComparer;

			return null;
		}

		sealed class Color32EqualityComparer : IEqualityComparer<Color32>
		{
			public bool Equals(Color32 x, Color32 y)
			{
				return x.a.Equals(y.a) && x.r.Equals(y.r) && x.g.Equals(y.g) && x.b.Equals(y.b);
			}

			public int GetHashCode(Color32 obj)
			{
				return obj.a.GetHashCode() ^ obj.r.GetHashCode() << 2 ^ obj.g.GetHashCode() >> 2 ^ obj.b.GetHashCode() >> 1;
			}
		}
	}
}
