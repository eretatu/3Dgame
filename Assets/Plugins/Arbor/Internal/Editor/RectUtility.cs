//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace ArborEditor
{
	internal static class RectUtility
	{
		public static Rect ScaleSizeBy(Rect rect, float scale)
		{
			return ScaleSizeBy(rect, scale, rect.center);
		}

		public static Rect ScaleSizeBy(Rect rect, float scale, Vector2 pivotPoint)
		{
			Rect result = rect;

			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;

			result.xMin *= scale;
			result.yMin *= scale;
			result.xMax *= scale;
			result.yMax *= scale;

			result.x += pivotPoint.x;
			result.y += pivotPoint.y;

			return result;
		}
	}

}
