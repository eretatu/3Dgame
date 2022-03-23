//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
#if !ARBOR_DLL

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ArborEditor.Internal
{
	internal sealed class ZoomManipulator : MouseManipulator
	{
		private VisualElement m_GraphUI;
		private ArborEditorWindow m_Window;
		private Vector2 m_Start;
		private Vector2 m_Last;
		private Vector2 m_ZoomCenter;

		public float zoomStep
		{
			get;
			set;
		}

		public bool isActive
		{
			get;
			private set;
		}

		public ZoomManipulator(VisualElement graphUI, ArborEditorWindow window) : base()
		{
			m_GraphUI = graphUI;
			m_Window = window;
			zoomStep = 0.01f;

			ManipulatorActivationFilter filter = new ManipulatorActivationFilter();
			filter.button = MouseButton.RightMouse;
			filter.modifiers = EventModifiers.Alt;
			activators.Add(filter);
		}

		protected override void RegisterCallbacksOnTarget()
		{
			target.RegisterCallback<WheelEvent>(OnScroll, TrickleDown.TrickleDown);
			target.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
			target.RegisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
			target.RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<WheelEvent>(OnScroll, TrickleDown.TrickleDown);
			target.UnregisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
			target.UnregisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
			target.UnregisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
		}

		private void OnScroll(WheelEvent e)
		{
			if (ArborSettings.mouseWheelMode == MouseWheelMode.Zoom)
			{
				m_Window.OnZoom(target.ChangeCoordinatesTo(m_GraphUI, e.localMousePosition), 1.0f - e.delta.y * zoomStep);
				e.StopPropagation();
			}
		}

		void OnMouseDown(MouseDownEvent e)
		{
			if (!CanStartManipulation(e))
			{
				return;
			}
			m_Start = m_Last = e.localMousePosition;
			m_ZoomCenter = target.ChangeCoordinatesTo(m_GraphUI, m_Start);
			isActive = true;
			target.CaptureMouse();
			e.StopPropagation();
		}

		void OnMouseMove(MouseMoveEvent e)
		{
			if (!isActive || !target.HasMouseCapture())
			{
				return;
			}
			Vector2 vector2 = e.localMousePosition - m_Last;
			m_Window.OnZoom(m_ZoomCenter, 1.0f + (vector2.x + vector2.y) * zoomStep);
			e.StopPropagation();
			m_Last = e.localMousePosition;
		}

		void OnMouseUp(MouseUpEvent e)
		{
			if (!isActive || !CanStopManipulation(e))
			{
				return;
			}
			isActive = false;
			target.ReleaseMouse();
			e.StopPropagation();
		}
	}
}

#endif