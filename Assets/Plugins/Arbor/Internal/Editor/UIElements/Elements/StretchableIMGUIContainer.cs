//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
#if !ARBOR_DLL

using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
#endif

namespace ArborEditor.Internal
{
	internal sealed class StretchableIMGUIContainer : IMGUIContainer
	{
		public enum StretchMode
		{
			None,
			Flex,
			Absolute,
		}

		public delegate bool ContainsPointCallback(Vector2 localPoint);

		public event ContainsPointCallback containsPointCallback;

		public StretchableIMGUIContainer(System.Action onGUIHandler, StretchMode stretchMode) : base(onGUIHandler)
		{
			switch (stretchMode)
			{
				case StretchMode.None:
					break;
				case StretchMode.Flex:
					style.flexGrow = 1f;
					break;
				case StretchMode.Absolute:
#if UNITY_2019_1_OR_NEWER
					style.position = Position.Absolute;
					style.top = 0f;
					style.bottom = 0f;
					style.left = 0f;
					style.right = 0f;
#else
					style.positionType = PositionType.Absolute;
					style.positionTop = 0f;
					style.positionBottom = 0f;
					style.positionLeft = 0f;
					style.positionRight = 0f;
#endif
					break;
			}
		}

		public override bool ContainsPoint(Vector2 localPoint)
		{
			Matrix4x4 graphMatrix = transform.matrix.inverse;
			Vector2 min = graphMatrix.MultiplyPoint(layout.min);
			Vector2 max = graphMatrix.MultiplyPoint(layout.max);
			Rect localLayout = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

			return localLayout.Contains(localPoint) && (containsPointCallback!=null ? containsPointCallback(localPoint) : true );
		}

#if ARBOR_DEBUG
		bool VisualElementCanGrabFocus()
		{
			return enabledInHierarchy && canGrabFocus;
		}

		public void DebugDefaultAction(EventBase evt)
		{
			System.Reflection.FieldInfo hasFocusableControlsFieldInfo = typeof(IMGUIContainer).GetField("hasFocusableControls", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			System.Reflection.PropertyInfo imguiKeyboardControlPropertyInfo = typeof(FocusController).GetProperty("imguiKeyboardControl", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			bool hasFocusableControls = (bool)hasFocusableControlsFieldInfo.GetValue(this);
			int immguiKeyboardControl = focusController != null? (int)imguiKeyboardControlPropertyInfo.GetValue(focusController, null) : 0;

#if UNITY_2019_1_OR_NEWER
			long eventTypeId = evt.eventTypeId;
#else
			long eventTypeId = evt.GetEventTypeId();
#endif
			if (eventTypeId == BlurEvent.TypeId())
			{
				Debug.Log(name + " : BlurEvent , " + base.canGrabFocus + " , " + VisualElementCanGrabFocus() + " , " + hasFocusableControls + " , " + immguiKeyboardControl + " , " + GUIUtility.keyboardControl);
			}
			else if (eventTypeId == FocusEvent.TypeId())
			{
				Debug.Log(name + " : FocusEvent , " + base.canGrabFocus + " , " + VisualElementCanGrabFocus() + " , " + hasFocusableControls + " , " + immguiKeyboardControl + " , " + GUIUtility.keyboardControl);
			}
		}

		protected override void ExecuteDefaultAction(EventBase evt)
		{
			DebugDefaultAction(evt);

			base.ExecuteDefaultAction(evt);
		}
#endif
	}
}

#endif