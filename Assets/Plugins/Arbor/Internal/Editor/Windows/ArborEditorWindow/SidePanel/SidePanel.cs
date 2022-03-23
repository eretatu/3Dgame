//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace ArborEditor
{
	using Arbor;

	[System.Reflection.Obfuscation(Exclude = true)]
	[System.Serializable]
	internal class SidePanel : Panel
	{
		[SerializeField]
		private GraphTabPanel _SidePanelGraphTab = new GraphTabPanel();

		[SerializeField]
		private NodeListTabPanel _SidePanelNodeListTab = new NodeListTabPanel();

		[SerializeField]
		private ParametersTabPanel _SidePanelParametersTab = new ParametersTabPanel();

		[SerializeField]
		private MinimapResizer _MinimapResizer = new MinimapResizer();

		public GraphTabPanel graphTabPanel
		{
			get
			{
				return _SidePanelGraphTab;
			}
		}

		public NodeListTabPanel nodeListTabPanel
		{
			get
			{
				return _SidePanelNodeListTab;
			}
		}

		public ParametersTabPanel parametersTabPanel
		{
			get
			{
				return _SidePanelParametersTab;
			}
		}

		public override void Setup(ArborEditorWindow hostWindow)
		{
			base.Setup(hostWindow);

			_SidePanelGraphTab.Setup(hostWindow);
			_SidePanelNodeListTab.Setup(hostWindow);
			_SidePanelParametersTab.Setup(hostWindow);
		}

		public override void OnFocus(bool focused)
		{
			switch (ArborSettings.sidePanelTab)
			{
				case SidePanelTab.Graph:
					_SidePanelGraphTab.OnFocus(focused);
					break;
				case SidePanelTab.NodeList:
					_SidePanelNodeListTab.OnFocus(focused);
					break;
				case SidePanelTab.Parameters:
					_SidePanelParametersTab.OnFocus(focused);
					break;
			}
		}

		void SideToolbarGUI()
		{
			EditorGUILayout.BeginHorizontal(Styles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUITools.toolbarHeight));

			EditorGUI.BeginChangeCheck();
			bool isGraphTab = GUILayout.Toggle(ArborSettings.sidePanelTab == SidePanelTab.Graph, EditorContents.graph, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
			if (EditorGUI.EndChangeCheck() && isGraphTab)
			{
				ArborSettings.sidePanelTab = SidePanelTab.Graph;
			}

			EditorGUI.BeginChangeCheck();
			bool isNodeListTab = GUILayout.Toggle(ArborSettings.sidePanelTab == SidePanelTab.NodeList, EditorContents.nodeList, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
			if (EditorGUI.EndChangeCheck() && isNodeListTab)
			{
				ArborSettings.sidePanelTab = SidePanelTab.NodeList;
			}

			EditorGUI.BeginChangeCheck();
			bool isParameterTab = GUILayout.Toggle(ArborSettings.sidePanelTab == SidePanelTab.Parameters, EditorContents.parameters, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
			if (EditorGUI.EndChangeCheck() && isParameterTab)
			{
				ArborSettings.sidePanelTab = SidePanelTab.Parameters;
			}

			GUILayout.FlexibleSpace();

			hostWindow.SidePanelToggle();

			EditorGUILayout.EndHorizontal();
		}

		public void ToolbarGUI(Rect rect)
		{
			GUILayout.BeginArea(rect);

			rect.position = Vector2.zero;

			// When EventType.Layout, GUIClip.GetTopRect() is an invalid value, so overwrite it with BeginGroup.
			GUI.BeginGroup(rect);

			SideToolbarGUI();

			if (Event.current.type == EventType.Repaint)
			{
				Rect borderRect = rect;
				borderRect.xMin = borderRect.xMax - 1f;
				EditorGUI.DrawRect(borderRect, EditorGUITools.GetSplitColor(ArborEditorWindow.isDarkSkin));
			}

			GUI.EndGroup();

			GUILayout.EndArea();
		}

		static readonly float s_MinimapMinRemainingSize = 100f; // Minimum height of GUI displayed on tabs
		static readonly float s_MinimapMinGraphExtents = 1000f;

		static int s_MinimapHash = "s_MinimapHash".GetHashCode();
		private int _MinimapControlID;

		private Vector2 _MinimapScrollPos;

		void Minimap(Rect minimapRect, int controlID)
		{
			NodeGraphEditor graphEditor = hostWindow.graphEditor;

			GUI.BeginClip(minimapRect);

			minimapRect.position = Vector2.zero;

			Rect extents = ArborEditorWindow.zoomable? new RectOffset(100, 100, 100, 100).Add(hostWindow.graphExtentsRaw) : hostWindow.graphExtents;

			Vector2 center = extents.center;
			if (extents.width < s_MinimapMinGraphExtents)
			{
				extents.xMin = center.x - s_MinimapMinGraphExtents * 0.5f;
				extents.xMax = center.x + s_MinimapMinGraphExtents * 0.5f;
			}
			if (extents.height < s_MinimapMinGraphExtents)
			{
				extents.yMin = center.y - s_MinimapMinGraphExtents * 0.5f;
				extents.yMax = center.y + s_MinimapMinGraphExtents * 0.5f;
			}

			MinimapViewport minimapViewport = new MinimapViewport(minimapRect, extents);

			Event evt = Event.current;

			EventType eventType = evt.GetTypeForControl(controlID);
			switch (eventType)
			{
				case EventType.MouseDown:
					{
						Rect viewportRect = minimapViewport.GraphToMinimapRect(hostWindow.graphViewport);
						if (evt.button == 0 && viewportRect.Contains(evt.mousePosition))
						{
							_MinimapScrollPos = evt.mousePosition - viewportRect.position;
							GUIUtility.hotControl = controlID;
							evt.Use();
						}
					}
					break;
				case EventType.MouseMove:
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
					{
						Vector2 position = minimapViewport.MinimapToGraphPoint(evt.mousePosition - _MinimapScrollPos);
						position.x = (int)position.x;
						position.y = (int)position.y;
						hostWindow.SetScroll(position, true, true);
						evt.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						GUIUtility.hotControl = 0;
						evt.Use();
					}
					break;
				case EventType.Repaint:
					{
						graphEditor.DrawMinimap(minimapViewport);

						Rect viewportRect = minimapViewport.GraphToMinimapRect(hostWindow.graphViewport);

						EditorGUI.DrawRect(viewportRect, new Color(0.0f, 0.5f, 1.0f, 0.2f));

						Color backgroundColor = GUI.backgroundColor;
						GUI.backgroundColor = new Color(1f, 1f, 1f, 1f);

						Styles.whiteBorderBold.Draw(viewportRect, GUIContent.none, false, false, false, false);
						if (viewportRect.width > 15f && viewportRect.height >= 15f)
						{
							GUI.backgroundColor = new Color(1f, 1f, 1f, 0.3f);

							Styles.whiteCross.Draw(new Rect(viewportRect.center.x - 5f, viewportRect.center.y - 5f, 10f, 10f), false, false, false, false);
						}

						GUI.backgroundColor = backgroundColor;
					}
					break;
			}

			GUI.EndClip();
		}

		void SidePanelMinimapGUI(Rect position)
		{
			NodeGraphEditor graphEditor = hostWindow.graphEditor;
			if (graphEditor == null)
			{
				return;
			}

			GUILayout.FlexibleSpace();

			using (new ProfilerScope("MinimapPanel"))
			{
				Rect windowRect = position;
				float bottomToolbarHeight = Styles.preToolbar.fixedHeight;

				float maxPreviewSize = windowRect.yMax - s_MinimapMinRemainingSize - bottomToolbarHeight;
				float minPreviewSize = 100f;

				if (maxPreviewSize >= minPreviewSize)
				{
					Rect rect = EditorGUILayout.BeginHorizontal(Styles.preToolbar, GUILayout.Height(bottomToolbarHeight));
					Rect dragRect;
					Rect dragIconRect = new Rect();
					const float dragPadding = 3f;
					const float minDragWidth = 20f;
					{
						GUILayout.FlexibleSpace();
						dragRect = GUILayoutUtility.GetLastRect();

						GUIContent title = EditorContents.minimap;

						dragIconRect.x = dragRect.x + dragPadding;
						dragIconRect.y = dragRect.y + (bottomToolbarHeight - Styles.dragHandle.fixedHeight) / 2;
						dragIconRect.width = dragRect.width - dragPadding * 2;
						dragIconRect.height = Styles.dragHandle.fixedHeight;

						{
							float maxLabelWidth = (dragIconRect.xMax - dragRect.xMin) - dragPadding - minDragWidth;
							float labelWidth = Mathf.Min(maxLabelWidth, Styles.preToolbar2.CalcSize(title).x);
							Rect labelRect = new Rect(dragRect.x, dragRect.y, labelWidth, dragRect.height);

							dragIconRect.xMin = labelRect.xMax + dragPadding;

							GUI.Label(labelRect, title, Styles.preToolbarLabel);
						}

						if (Event.current.type == EventType.Repaint)
						{
							// workaround: To properly center the image because it already has a 1px bottom padding
							dragIconRect.y += 1;
							Styles.dragHandle.Draw(dragIconRect, GUIContent.none, false, false, false, false);
						}
					}
					EditorGUILayout.EndHorizontal();

					float previewSize = _MinimapResizer.ResizeHandle(windowRect, minPreviewSize, maxPreviewSize, rect, dragRect);
					if (_MinimapResizer.GetExpanded())
					{
						GUILayout.BeginVertical(Styles.graphBackground, GUILayout.Height(previewSize));

						_MinimapControlID = GUIUtility.GetControlID(s_MinimapHash, FocusType.Passive);

						Rect minimapRect = GUILayoutUtility.GetRect(0, 10240, 0, 10240);
						Minimap(minimapRect, _MinimapControlID);

						if (ArborEditorWindow.zoomable)
						{
							float zoomLevel = hostWindow.graphScale.x * 100;
							EditorGUI.BeginChangeCheck();
							zoomLevel = EditorGUILayout.Slider(zoomLevel, 10f, 100f) / 100f;
							if (EditorGUI.EndChangeCheck())
							{
								hostWindow.SetZoom(hostWindow.graphViewport.center, new Vector3(zoomLevel, zoomLevel, 1f), true);
							}
						}

						GUILayout.EndVertical();
					}
				}
			}
		}

		public override void OnGUI(Rect rect)
		{
			using (new ProfilerScope("SidePanelGUI"))
			{
				GUILayout.BeginArea(rect);

				rect.position = Vector2.zero;

				// When EventType.Layout, GUIClip.GetTopRect() is an invalid value, so overwrite it with BeginGroup.
				GUI.BeginGroup(rect);

				bool hierarchyMode = EditorGUIUtility.hierarchyMode;
				EditorGUIUtility.hierarchyMode = true;

				bool wideMode = EditorGUIUtility.wideMode;
				EditorGUIUtility.wideMode = rect.width > EditorGUITools.kWideModeMinWidth;

				try
				{
					switch (ArborSettings.sidePanelTab)
					{
						case SidePanelTab.Graph:
							_SidePanelGraphTab.OnGUI(rect);
							break;
						case SidePanelTab.NodeList:
							_SidePanelNodeListTab.OnGUI(rect);
							break;
						case SidePanelTab.Parameters:
							_SidePanelParametersTab.OnGUI(rect);
							break;
					}

					SidePanelMinimapGUI(rect);

					if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
					{
						GUIUtility.keyboardControl = 0;
						Event.current.Use();
					}

					if (Event.current.type == EventType.Repaint)
					{
						Rect borderRect = rect;
						borderRect.xMin = borderRect.xMax - 1f;
						EditorGUI.DrawRect(borderRect, EditorGUITools.GetSplitColor(ArborEditorWindow.isDarkSkin));
					}
				}
				finally
				{
					EditorGUIUtility.wideMode = wideMode;
					EditorGUIUtility.hierarchyMode = hierarchyMode;
				}

				GUI.EndGroup();

				GUILayout.EndArea();
			}
		}
	}
}