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
	internal sealed class GraphView : GraphLayout
	{
		public static readonly Vector2 kDefaultScrollerValues = new Vector2(0, 100);

		private ArborEditorWindow m_Window;

		public enum ShowMode
		{
			Normal,
			ForceShow,
			ForceHide,
		}

		public ShowMode horizontalShowMode
		{
			get; set;
		}
		public ShowMode verticalShowMode
		{
			get; set;
		}

		public bool needsHorizontal
		{
			get
			{
				switch (horizontalShowMode)
				{
					case ShowMode.Normal:
						Rect graphExtents = m_Window.graphExtents;
						Rect viewportLayout = m_Window.graphViewport;
						return graphExtents.width - viewportLayout.width > 0;
					case ShowMode.ForceShow:
						return true;
					case ShowMode.ForceHide:
						return false;
				}

				return false;
			}
		}

		public bool needsVertical
		{
			get
			{
				switch(verticalShowMode)
				{
					case ShowMode.Normal:
						Rect graphExtents = m_Window.graphExtents;
						Rect viewportLayout = m_Window.graphViewport;
						return graphExtents.height - viewportLayout.height > 0;
					case ShowMode.ForceShow:
						return true;
					case ShowMode.ForceHide:
						return false;
				}

				return false;
			}
		}

		public Vector2 scrollOffset
		{
			get
			{
				return new Vector2(horizontalScroller.value, verticalScroller.value) + m_Window.graphExtents.min;
			}
		}

		private bool _EnableChangeScroll = true;
		private int _DisableDelayChangeScrollCount = 0;

		public void SetScrollOffset(Vector2 value, bool updateTransform, bool endFrameSelected)
		{
			Vector2 tmpValue = value;
			
			bool changed = false;

			value -= m_Window.graphExtents.min;
			value.x = Mathf.Clamp(value.x, horizontalScroller.lowValue, horizontalScroller.highValue);
			value.y = Mathf.Clamp(value.y, verticalScroller.lowValue, verticalScroller.highValue);

			bool EnableChangeScroll = _EnableChangeScroll;
			_EnableChangeScroll = false;

			if (horizontalScroller.value != value.x)
			{
				Slider slider = horizontalScroller.slider;
				var newValue = Mathf.Clamp(value.x, slider.lowValue, slider.highValue);

				if (slider.value != newValue)
				{
					_DisableDelayChangeScrollCount++;

					horizontalScroller.value = value.x;
					changed = true;
				}
			}

			if (verticalScroller.value != value.y)
			{
				Slider slider = verticalScroller.slider;
				var newValue = Mathf.Clamp(value.y, slider.lowValue, slider.highValue);

				if (slider.value != newValue)
				{
					_DisableDelayChangeScrollCount++;

					verticalScroller.value = value.y;
					changed = true;
				}
			}

			if (tmpValue != scrollOffset)
			{
				changed = true;
			}

			_EnableChangeScroll = EnableChangeScroll;

			if (changed && updateTransform)
			{
				UpdateContentViewTransform(endFrameSelected);
			}
		}

		private GraphLayout m_ContentContainer;

		void UpdateContentViewTransform(Vector2 scrollOffset, bool endFrameSelected)
		{
			// Adjust contentContainer's position
			m_Window.SetScroll(scrollOffset, false, endFrameSelected);

			MarkDirtyRepaint();
		}

		void UpdateContentViewTransform(bool endFrameSelected)
		{
			UpdateContentViewTransform(scrollOffset, endFrameSelected);
		}

		// Represents the visible part of contentContainer
		public GraphLayout contentViewport
		{
			get; private set;
		}

		public Scroller horizontalScroller
		{
			get; private set;
		}
		public Scroller verticalScroller
		{
			get; private set;
		}

		public override VisualElement contentContainer // Contains full content, potentially partially visible
		{
			get
			{
				return m_ContentContainer;
			}
		}

		public GraphLayout contentView
		{
			get
			{
				return m_ContentContainer;
			}
		}

		void OnChangedScrollValue()
		{
			if (_DisableDelayChangeScrollCount == 0)
			{
				if (_EnableChangeScroll)
				{

					UpdateContentViewTransform(true);
				}
			}
			else
			{
				_DisableDelayChangeScrollCount--;
			}
		}

		private Vector2 _LastScrollerValue = Vector2.zero;

		public GraphView(ArborEditorWindow window)
		{
			m_Window = window;

			style.flexGrow = 1;
			style.overflow = Overflow.Hidden;

			contentViewport = new GraphLayout() {
				name = "ContentViewport",
				style =
				{
					flexDirection = FlexDirection.Row,
#if UNITY_2019_1_OR_NEWER
					position = Position.Absolute,
					left = 0,
					top = 0,
					right = 0,
					bottom = 0,
#else
					positionType = PositionType.Absolute,
					positionLeft = 0,
					positionTop = 0,
					positionRight = 0,
					positionBottom = 0,
#endif

					overflow = Overflow.Hidden,
				}
			};
#if UNITY_2019_1_OR_NEWER
			hierarchy.Add(contentViewport);
#else
			contentViewport.clippingOptions = ClippingOptions.ClipContents;
			shadow.Add(contentViewport);
#endif

			// Basic content container; its constraints should be defined in the USS file
			m_ContentContainer = new GraphLayout() {
				name = "ContentView",
				style =
				{
					flexGrow = 1f,
				}
			};
			contentViewport.Add(m_ContentContainer);

			horizontalScroller = new Scroller(0f, 100f,
				(value) =>
				{
					if (_LastScrollerValue.x != value)
					{
						OnChangedScrollValue();
					}
					_LastScrollerValue.x = value;
				},
				SliderDirection.Horizontal
				)
			{
				name = "HorizontalScroller",
#if UNITY_2019_1_OR_NEWER
				viewDataKey = "HorizontalScroller",
#else
				persistenceKey = "HorizontalScroller",
#endif
				style =
				{
#if UNITY_2019_1_OR_NEWER
					position = Position.Absolute,
					left = 0f,
					bottom = 0f,
					right = 17f,
#else
					positionType = PositionType.Absolute,
					positionLeft = 0f,
					positionBottom = 0f,
					positionRight = 17f,
#endif
				}
			};
#if UNITY_2019_1_OR_NEWER
			hierarchy.Add(horizontalScroller);
#else
			shadow.Add(horizontalScroller);
#endif

			verticalScroller = new Scroller(0f, 100f,
					(value) =>
					{
						if (_LastScrollerValue.y != value)
						{
							OnChangedScrollValue();
						}
						_LastScrollerValue.y = value;
					},
					SliderDirection.Vertical
					)
			{
				name = "VerticalScroller",
#if UNITY_2019_1_OR_NEWER
				viewDataKey = "VerticalScroller",
#else
				persistenceKey = "VerticalScroller",
#endif
				style =
				{
#if UNITY_2019_1_OR_NEWER
					position = Position.Absolute,
					top = 0f,
					bottom = 17f,
					right = 0f,
#else
					positionType = PositionType.Absolute,
					positionTop = 0f,
					positionBottom = 17f,
					positionRight = 0f,
#endif
				}
			};

#if UNITY_2019_1_OR_NEWER
			hierarchy.Add(verticalScroller);
#else
			shadow.Add(verticalScroller);
#endif
		}

		protected override void ExecuteDefaultAction(EventBase evt)
		{
			base.ExecuteDefaultAction(evt);

			if (UIElementsUtility.IsLayoutEvent(evt))
			{
				OnPostLayout(true);
			}
		}

		private Vector2 _LastViewportSize = Vector2.zero;
		private bool _IsInitialize = false;
		
		void UpdateView()
		{
			bool enableChangeScroll = _EnableChangeScroll;
			_EnableChangeScroll = false;

			Vector2 oldScrollOffset = scrollOffset;
			
			Rect lastGraphExtents = m_Window.graphExtents;

			m_Window.UpdateGraphExtents();

			Rect graphExtents = m_Window.graphExtents;
			Rect viewportLayout = m_Window.graphViewport;

			if (graphExtents.width > Mathf.Epsilon)
			{
				horizontalScroller.Adjust(viewportLayout.width / graphExtents.width);
			}
			if (graphExtents.height > Mathf.Epsilon)
			{
				verticalScroller.Adjust(viewportLayout.height / graphExtents.height);
			}

			// Set availability
			horizontalScroller.SetEnabled(graphExtents.width - viewportLayout.width > 0);
			verticalScroller.SetEnabled(graphExtents.height - viewportLayout.height > 0);

			// Expand content if scrollbars are hidden
#if UNITY_2019_1_OR_NEWER
			contentViewport.style.right = needsVertical ? verticalScroller.layout.width : 0;
			horizontalScroller.style.right = needsVertical ? verticalScroller.layout.width : 0;
			contentViewport.style.bottom = needsHorizontal ? horizontalScroller.layout.height : 0;
			verticalScroller.style.bottom = needsHorizontal ? horizontalScroller.layout.height : 0;
#else
			contentViewport.style.positionRight = needsVertical ? verticalScroller.layout.width : 0;
			horizontalScroller.style.positionRight = needsVertical ? verticalScroller.layout.width : 0;
			contentViewport.style.positionBottom = needsHorizontal ? horizontalScroller.layout.height : 0;
			verticalScroller.style.positionBottom = needsHorizontal ? horizontalScroller.layout.height : 0;
#endif

			Vector2 scrollValue = scrollOffset;

			if (needsHorizontal)
			{
				horizontalScroller.lowValue = 0;
				horizontalScroller.highValue = Mathf.Max(graphExtents.width - viewportLayout.width, 1);
			}
			else
			{
				horizontalScroller.value = 0.0f;
			}

			if (needsVertical)
			{
				verticalScroller.lowValue = 0;
				verticalScroller.highValue = Mathf.Max(graphExtents.height - viewportLayout.height, 1);
			}
			else
			{
				verticalScroller.value = 0.0f;
			}

			// Set visibility and remove/add content viewport margin as necessary
			if (horizontalScroller.visible != needsHorizontal)
			{
				horizontalScroller.visible = needsHorizontal;
				if (needsHorizontal)
				{
#if UNITY_2019_1_OR_NEWER
					contentViewport.style.bottom = 17;
#else
					contentViewport.style.positionBottom = 17;
#endif
				}
				else
				{
#if UNITY_2019_1_OR_NEWER
					contentViewport.style.bottom = 0;
#else
					contentViewport.style.positionBottom = 0;
#endif
				}
			}

			if (verticalScroller.visible != needsVertical)
			{
				verticalScroller.visible = needsVertical;
				if (needsVertical)
				{
#if UNITY_2019_1_OR_NEWER
					contentViewport.style.right = 17;
#else
					contentViewport.style.positionRight = 17;
#endif
				}
				else
				{
#if UNITY_2019_1_OR_NEWER
					contentViewport.style.right = 0;
#else
					contentViewport.style.positionRight = 0;
#endif
				}
			}

			_EnableChangeScroll = enableChangeScroll;

			bool changeExtents = graphExtents != lastGraphExtents;
			bool changeViewport = _LastViewportSize != viewportLayout.size;
			if (!_IsInitialize || changeExtents || changeViewport)
			{
				SetScrollOffset(oldScrollOffset, true, false);
			}

			_IsInitialize = true;
			_LastViewportSize = viewportLayout.size;

			m_Window.OnPostLayout();
		}

		private void OnPostLayout(bool hasNewLayout)
		{
			if (!hasNewLayout)
				return;

			UpdateView();
		}

		public void UpdateLayout()
		{
			if (!_IsInitialize)
			{
#if !UNITY_2019_1_OR_NEWER
#pragma warning disable 0618 // Unity 2018.3.0b3 : Dirty layout is internal only...
				Dirty(ChangeType.Layout);
#pragma warning restore 0618
#endif

				return;
			}

			UpdateView();
		}
	}
}

#endif