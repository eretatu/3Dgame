//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using System.Collections;

namespace ArborEditor
{
	using Arbor;

	public struct MinimapViewport
	{
		private Matrix4x4 _GraphToMinimapTransform;
		private Matrix4x4 _MinimapToGraphTransform;

		public float zoomLevel;
		public Rect viewportRect;

		public MinimapViewport(Rect minimapRect, Rect graphExtents)
		{
			// aspect fit
			float widthRatio = minimapRect.width / graphExtents.width;
			float heightRatio = minimapRect.height / graphExtents.height;

			zoomLevel = widthRatio > heightRatio ? heightRatio : widthRatio;
			float resizedWidth = graphExtents.width * zoomLevel;
			float resizedHeight = graphExtents.height * zoomLevel;

			viewportRect = new Rect(minimapRect.center.x - resizedWidth * 0.5f, minimapRect.center.y - resizedHeight * 0.5f, resizedWidth, resizedHeight);

			Vector3 minimapScale = new Vector3(viewportRect.width / graphExtents.width, viewportRect.height / graphExtents.height, 1f);
			_GraphToMinimapTransform = Matrix4x4.TRS(viewportRect.position, Quaternion.identity, minimapScale) * Matrix4x4.TRS(-graphExtents.position, Quaternion.identity, Vector3.one);
			_MinimapToGraphTransform = _GraphToMinimapTransform.inverse;
		}

		public Rect GraphToMinimapRect(Rect rect)
		{
			rect.min = GraphToMinimapPoint(rect.min);
			rect.max = GraphToMinimapPoint(rect.max);

			Vector2 size = rect.size;
			size.x = Mathf.Max(size.x, 1f);
			size.y = Mathf.Max(size.y, 1f);
			rect.size = size;

			return rect;
		}

		public Bezier2D GraphToMinimapBezier(Bezier2D bezier)
		{
			if (bezier == null)
			{
				return null;
			}

			Bezier2D newBezier = new Bezier2D();

			newBezier.startPosition = GraphToMinimapPoint(bezier.startPosition);
			newBezier.startControl = GraphToMinimapPoint(bezier.startControl);
			newBezier.endPosition = GraphToMinimapPoint(bezier.endPosition);
			newBezier.endControl = GraphToMinimapPoint(bezier.endControl);

			return newBezier;
		}

		public Vector2 GraphToMinimapPoint(Vector2 point)
		{
			return _GraphToMinimapTransform.MultiplyPoint(point);
		}

		public Vector2 MinimapToGraphPoint(Vector2 point)
		{
			return _MinimapToGraphTransform.MultiplyPoint(point);
		}
	}
}