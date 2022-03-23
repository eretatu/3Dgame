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
using UnityEditor;

namespace ArborEditor.Internal
{
	internal sealed class PanManipulator : MouseManipulator
	{
		private VisualElement m_GraphUI;
		private ArborEditorWindow m_Window;
		
		public bool isActive
		{
			get;
			private set;
		}

		public PanManipulator(VisualElement graphUI, ArborEditorWindow window) : base()
		{
			m_GraphUI = graphUI;
			m_Window = window;

			ManipulatorActivationFilter filter1 = new ManipulatorActivationFilter();
			filter1.button = MouseButton.LeftMouse;
			filter1.modifiers = EventModifiers.Alt;
			activators.Add(filter1);

			ManipulatorActivationFilter filter2 = new ManipulatorActivationFilter();
			filter2.button = MouseButton.MiddleMouse;
			activators.Add(filter2);
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
			if (ArborSettings.mouseWheelMode == MouseWheelMode.Scroll)
			{
				m_Window.OnScroll(e.delta * 20.0f);
				e.StopPropagation();
			}
		}

		void OnMouseDown(MouseDownEvent e)
		{
			if (!CanStartManipulation(e) || (Event.current != null && Event.current.type == EventType.Used) )
			{
				return;
			}
			if (e.button == 0 &&  e.altKey)
			{
				Vector2 mousePosition = m_Window.WindowToGraphPoint(e.localMousePosition);
				if (m_Window.graphEditor.ContainsNodes(mousePosition))
				{
					return;
				}
			}
			isActive = true;
			EditorGUIUtility.SetWantsMouseJumping(1);
			target.CaptureMouse();
			e.StopPropagation();
		}

		void OnMouseMove(MouseMoveEvent e)
		{
			if (!isActive || !target.HasMouseCapture())
			{
				return;
			}

			Vector2 delta = e.mouseDelta;
			Vector3 scale = m_GraphUI.transform.scale;
			delta = Vector2.Scale(delta, new Vector3(1f/scale.x, 1f/scale.y, 1f/scale.z));

			m_Window.OnScroll(-delta);

			e.StopPropagation();
		}

		void OnMouseUp(MouseUpEvent e)
		{
			if (!isActive || !CanStopManipulation(e))
			{
				return;
			}
			isActive = false;
			target.ReleaseMouse();
			EditorGUIUtility.SetWantsMouseJumping(0);
			e.StopPropagation();
		}
	}
}

#endif