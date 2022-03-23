//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
#if !ARBOR_DLL
#define ARBOR_EDITOR_USE_UIELEMENTS
#define ARBOR_EDITOR_EXTENSIBLE
#define ARBOR_EDITOR_CAPTURABLE
#endif

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

#if ARBOR_EDITOR_USE_UIELEMENTS || ARBOR_EDITOR_CAPTURABLE
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEditor.Experimental.UIElements;
#endif
#endif

namespace ArborEditor
{
	using Arbor;
	using Arbor.DynamicReflection;
	using Arbor.Playables;
	using ArborEditor.UpdateCheck;
	using ArborEditor.IMGUI.Controls;

#if ARBOR_EDITOR_USE_UIELEMENTS
	using ArborEditor.Internal;
#endif

	[System.Reflection.Obfuscation(Exclude = true)]
	public sealed class ArborEditorWindow : EditorWindow, IHasCustomMenu, IPropertyChanged, IUpdateCallback, IHierarchyChangedCallback
	{
		#region static
		private static GUIContent s_DefaultTitleContent = null;
		private const float k_ShowLogoTime = 3.0f;
		private const float k_FadeLogoTime = 1.0f;

#if ARBOR_EDITOR_EXTENSIBLE
		public static event System.Action<NodeGraph> toolbarGUI;
		public static event System.Action<NodeGraph, Rect> underlayGUI;
		public static event System.Action<NodeGraph, Rect> overlayGUI;
		public static ISkin skin;
#endif

#if ARBOR_EDITOR_USE_UIELEMENTS
#if UNITY_2020_1_OR_NEWER
		private static readonly MethodInfo s_GetWorldBoundingBoxMethod;
		private static readonly MethodInfo s_SetLastWorldClipMethod;
#endif
#endif

		public static ArborEditorWindow activeWindow
		{
			get;
			private set;
		}

		private static GUIContent defaultTitleContent
		{
			get
			{
				if (s_DefaultTitleContent == null)
				{
					s_DefaultTitleContent = new GUIContent("Arbor Editor", EditorGUIUtility.isProSkin ? Icons.logoIcon_DarkSkin : Icons.logoIcon_LightSkin);
				}
				return s_DefaultTitleContent;
			}
		}

		public static bool zoomable
		{
			get
			{
#if ARBOR_EDITOR_USE_UIELEMENTS
				return true;
#else
				return false;
#endif
			}
		}

		public static bool nodeCommentAffectsZoom
		{
			get
			{
				return zoomable && ArborSettings.nodeCommentAffectsZoom;
			}
		}

		public static bool captuarable
		{
			get
			{
#if ARBOR_EDITOR_CAPTURABLE
				return true;
#else
				return false;
#endif
			}
		}

		public static bool isBuildDocuments
		{
			get;
			set;
		}

		public static bool isInNodeEditor
		{
			get
			{
				if (isBuildDocuments)
				{
					return true;
				}

				ArborEditorWindow window = activeWindow;
				if (window != null)
				{
					return window._IsInNodeEditor;
				}

				return false;
			}
		}

		internal static bool isDarkSkin
		{
			get
			{
#if ARBOR_EDITOR_EXTENSIBLE
				if (skin != null)
				{
					return skin.isDarkSkin;
				}
#endif
				return EditorGUIUtility.isProSkin;
			}
		}

		static ArborEditorWindow()
		{
#if ARBOR_EDITOR_USE_UIELEMENTS
#if UNITY_2020_1_OR_NEWER
			var worldBoundingBoxProperty = typeof(VisualElement).GetProperty("worldBoundingBox", BindingFlags.Instance | BindingFlags.NonPublic);
			s_GetWorldBoundingBoxMethod = worldBoundingBoxProperty.GetGetMethod(true);
			
			var lastWorldClipProperty = typeof(IMGUIContainer).GetProperty("lastWorldClip", BindingFlags.Instance | BindingFlags.NonPublic);
			s_SetLastWorldClipMethod = lastWorldClipProperty.GetSetMethod(true);
#endif
#endif
		}

		static ArborEditorWindow Open()
		{
			ArborEditorWindow window = ArborSettings.dockingOpen ? EditorWindow.GetWindow<ArborEditorWindow>(typeof(SceneView)) : EditorWindow.GetWindow<ArborEditorWindow>();
			window.titleContent = defaultTitleContent;
			return window;
		}

		[MenuItem("Window/Arbor/Arbor Editor")]
		public static void OpenFromMenu()
		{
			Open();
		}

		public static void Open(NodeGraph nodeGraph)
		{
			ArborEditorWindow window = Open();
			window.OpenInternal(nodeGraph);
		}

		#endregion // static

		#region Serialize fields
		[SerializeField]
		private NodeGraph _NodeGraphRoot = null;

		[SerializeField]
		private int _NodeGraphRootInstanceID = 0;

		[SerializeField]
		private NodeGraph _NodeGraphRootPrev = null;

		[SerializeField]
		private int _NodeGraphRootPrevInstanceID = 0;

		[SerializeField]
		private NodeGraph _NodeGraphCurrent = null;

		[SerializeField]
		private int _NodeGraphCurrentInstanceID = 0;

		[SerializeField]
		private NodeGraphEditor _GraphEditor = null;

		[SerializeField]
		private bool _IsLocked = false;

		[SerializeField]
		private SidePanel _SidePanel = new SidePanel();

		[SerializeField]
		private TransformCache _TransformCache = new TransformCache();

		[SerializeField]
		private TreeViewState _TreeViewState = new TreeViewState();

#if !ARBOR_EDITOR_USE_UIELEMENTS
		[SerializeField]
		private GraphGUI _GraphGUI = new GraphGUI();
#endif

		#endregion // Serialize fields

		#region fields

		private bool _FadeLogo = false;
		private double _FadeLogoBeginTime;

		[System.NonSerialized]
		private bool _NodeGraphRootAttachedCallback = false;

		[System.NonSerialized]
		private bool _NodeGraphCurrentAttachedCallback = false;

		private GraphTreeViewItem _SelectedGraphItem = null;

		private GraphTreeViewItem _NextGraph = null;

		private FrameSelected _FrameSelected = new FrameSelected();
		private FrameSelected _FrameSelectedZoom = new FrameSelected();

		private bool _IsPlaying = false;

		private bool _Initialized = false;

		private bool _IsLayoutSetup = false;

		private bool _IsSelection = false;
		private Rect _SelectionRect = new Rect();

		internal bool _GUIIsExiting = false;

		private bool _IsRepaint = false;

		private bool _IsUpdateLiveTracking = false;

		private bool _IsWindowVisible = false;

		private Rect _GraphExtents = new Rect(0, 0, 100, 100);

#if ARBOR_EDITOR_CAPTURABLE
		private bool _IsCapture = false;
		private EditorCapture _Capture = null;
		private Rect _GraphCaptureExtents = new Rect(0, 0, 100, 100);
#endif

#if ARBOR_EDITOR_USE_UIELEMENTS
		private GraphMainLayout _MainLayout;
		private GraphLayout _LeftPanelLayout;
		private GraphLayout _SideToolbarLayout;
		private GraphLayout _SidePanelLayout;
		private GraphLayout _RightPanelLayout;
		private GraphLayout _GraphPanel;
		private GraphView _GraphView;
		private StretchableIMGUIContainer _ToolbarUI;
		private StretchableIMGUIContainer _SideToolbarUI;
		private StretchableIMGUIContainer _SidePanelUI;
		private StretchableIMGUIContainer _NoGraphUI;
		private StretchableIMGUIContainer _GraphUnderlayUI;
		private StretchableIMGUIContainer _GraphUI;
#if UNITY_2020_1_OR_NEWER
		private System.Func<Rect> _GetWorldBoundingBox;
		private System.Action<Rect> _SetLastWorldClip;
#endif
#if UNITY_2019_2_OR_NEWER
		private VisualElement _GraphExtentsBoundingBoxElement;
#endif
		private StretchableIMGUIContainer _GraphOverlayUI;
		private StretchableIMGUIContainer _GraphBottomUI;
		private ZoomManipulator _ZoomManipulator;
		private PanManipulator _PanManipulator;

		[System.NonSerialized]
		private bool _MainLayoutInitialized = false;

		private Rect _GraphViewExtents = new Rect(0,0,100,100);
		private bool _EndFrameSelected = true;
#else
		Rect _SideToolbarRect;
		Rect _SidePanelRect;
		Rect _ToolBarRect;
		Rect _BreadcrumbRect;
		Rect _GraphRect;
#endif

		private bool _IsInNodeEditor;

		private GraphSettingsWindow _GraphSettingsWindow = null;

		private TreeView _TreeView = new TreeView();

		#endregion // fields

		#region properties

		public bool visibleLogo
		{
			get
			{
				switch (ArborSettings.showLogo)
				{
					case LogoShowMode.Hidden:
						return false;
					case LogoShowMode.FadeOut:
						return _FadeLogo && EditorApplication.timeSinceStartup - _FadeLogoBeginTime <= (k_ShowLogoTime + k_FadeLogoTime);
					case LogoShowMode.AlwaysShow:
						return true;
				}
				return false;
			}
		}

		public bool fadeLogo
		{
			get
			{
				if (!visibleLogo)
				{
					return false;
				}

				switch (ArborSettings.showLogo)
				{
					case LogoShowMode.Hidden:
						return false;
					case LogoShowMode.FadeOut:
						if (_FadeLogo)
						{
							float elapseTime = (float)(EditorApplication.timeSinceStartup - (_FadeLogoBeginTime + k_ShowLogoTime));
							return 0 <= elapseTime && elapseTime <= k_FadeLogoTime;
						}
						return false;
					case LogoShowMode.AlwaysShow:
						return false;
				}

				return false;
			}
		}

		public NodeGraphEditor graphEditor
		{
			get
			{
				return _GraphEditor;
			}
		}

		public NodeGraph rootGraph
		{
			get
			{
				return _NodeGraphRoot;
			}
		}

		public NodeGraph rootGraphPrev
		{
			get
			{
				return _NodeGraphRootPrev;
			}
		}

		public Rect graphExtents
		{
			get
			{
#if ARBOR_EDITOR_CAPTURABLE
				if (isCapture)
				{
					return _GraphCaptureExtents;
				}
				else
#endif
				{
#if ARBOR_EDITOR_USE_UIELEMENTS
					return _GraphViewExtents;
#else
					return _GraphGUI.extents;
#endif
				}
			}
		}

		public Rect graphExtentsRaw
		{
			get
			{
				return _GraphExtents;
			}
		}

		public Rect graphViewRect
		{
			get
			{
#if ARBOR_EDITOR_USE_UIELEMENTS
				return _GraphUI.layout;
#else
				return _GraphRect;
#endif
			}
		}

		public Rect graphViewportPosition
		{
			get
			{
#if ARBOR_EDITOR_USE_UIELEMENTS
				return _GraphUI.layout;
#else
				return _GraphGUI.viewportPosition;
#endif
			}
		}

		public Rect graphViewport
		{
			get
			{
#if ARBOR_EDITOR_USE_UIELEMENTS
				return WindowToGraphRect(_GraphUI.layout);
#else
				return _GraphGUI.viewArea;
#endif
			}
		}

		public Vector3 graphScale
		{
			get
			{
#if ARBOR_EDITOR_USE_UIELEMENTS
				return _GraphUI.transform.scale;
#else
				return Vector3.one;
#endif
			}
		}

		public Vector2 scrollPos
		{
			get
			{
#if ARBOR_EDITOR_USE_UIELEMENTS
				Vector3 scale = _GraphUI.transform.scale;
				return -Vector3.Scale(_GraphUI.transform.position, new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z));
#else
				return _GraphGUI.scrollPos;
#endif
			}
			set
			{
#if ARBOR_EDITOR_USE_UIELEMENTS
				Vector3 scale = _GraphUI.transform.scale;
				_GraphUI.transform.position = -Vector3.Scale(value, scale);
#else
				_GraphGUI.scrollPos = value;
#endif
			}
		}

		public Matrix4x4 graphMatrix
		{
			get
			{
				if (isCapture)
				{
					return Matrix4x4.identity;
				}

#if ARBOR_EDITOR_USE_UIELEMENTS
				return _GraphUI.transform.matrix;
#else
				return Matrix4x4.identity;
#endif
			}
		}

		public bool isCapture
		{
			get
			{
#if ARBOR_EDITOR_CAPTURABLE
				return captuarable && _IsCapture;
#else
				return false;
#endif
			}
		}

		public TreeView treeView
		{
			get
			{
				return _TreeView;
			}
		}

		public TreeViewState treeViewState
		{
			get
			{
				return _TreeViewState;
			}
		}

		#endregion // properties

		#region Unity methods

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnEnable()
		{
#if ARBOR_DEBUG
			ArborUpdateCheck updateCheck = ArborUpdateCheck.instance;
			updateCheck.CheckStart(OnUpdateCheckDone,true);
#else
			if (ArborVersion.isUpdateCheck)
			{
				ArborUpdateCheck updateCheck = ArborUpdateCheck.instance;
				updateCheck.CheckStart(OnUpdateCheckDone);
			}
#endif

			if (activeWindow == null)
			{
				activeWindow = this;
			}

#if !ARBOR_EDITOR_USE_UIELEMENTS
			_GraphGUI.hostWindow = this;
#endif

			if (_GraphEditor != null)
			{
				_GraphEditor.hostWindow = this;
				BeginFadeLogo();
			}

			_SidePanel.Setup(this);
			SetNodeGraphRoot(_NodeGraphRoot);

			wantsMouseMove = true;

			titleContent = defaultTitleContent;

			_Initialized = false;

			_FrameSelectedZoom.stoppingDistance = 0.001f;

#if ARBOR_EDITOR_USE_UIELEMENTS
			SetupGUI();
#endif
			DoRepaint();

			EditorCallbackUtility.RegisterUpdateCallback(this);
			EditorCallbackUtility.RegisterPropertyChanged(this);
			EditorCallbackUtility.RegisterHierarchyChangedCallback(this);
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		private void OnDisable()
		{
			if (activeWindow == this)
			{
				activeWindow = null;
			}

			if (!_IsWindowVisible)
			{
				DestroyGraphEditor();
			}

			EditorCallbackUtility.UnregisterUpdateCallback(this);
			EditorCallbackUtility.UnregisterPropertyChanged(this);
			EditorCallbackUtility.UnregisterHierarchyChangedCallback(this);
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		private void OnSelectionChange()
		{
			if (_IsLocked)
			{
				return;
			}

			GameObject gameObject = Selection.activeGameObject;
			if (gameObject == null)
			{
				return;
			}

			NodeGraph[] nodeGraphs = gameObject.GetComponents<NodeGraph>();
			if (nodeGraphs != null)
			{
				int graphCount = nodeGraphs.Length;
				for (int graphIndex = 0; graphIndex < graphCount; graphIndex++)
				{
					NodeGraph graph = nodeGraphs[graphIndex];
					if ((graph.hideFlags & HideFlags.HideInInspector) == HideFlags.None)
					{
						OpenInternal(graph);
						break;
					}
				}
			}
		}

		void IPropertyChanged.OnPropertyChanged(PropertyChangedType propertyChangedType)
		{
			if (propertyChangedType != PropertyChangedType.UndoRedoPerformed)
			{
				return;
			}

			ReatachIfNecessary();

			if (_GraphEditor != null)
			{
				_GraphEditor.OnUndoRedoPerformed();
			}

			OnChangedGraphTree();

			DoRepaint();
		}

		void IHierarchyChangedCallback.OnHierarchyChanged()
		{
			ReatachIfNecessary();
		}

		void IUpdateCallback.OnUpdate()
		{
			if (_NextGraph != null)
			{
				SetCurrentNodeGraph(_NextGraph);
				_NextGraph = null;
			}

			if (_IsWindowVisible)
			{
				if (_GraphEditor != null)
				{
					if (_IsUpdateLiveTracking)
					{
						if (_GraphEditor.LiveTracking())
						{
							// Change GraphEditor
							_IsUpdateLiveTracking = true;
						}
						else
						{
							_IsUpdateLiveTracking = false;
						}
					}

					if (IsDragScroll() || _IsRepaint || fadeLogo)
					{
						DoRepaint();
					}
				}

#if ARBOR_EDITOR_USE_UIELEMENTS
				UpdateDocked();
#endif
			}
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnBecameVisible()
		{
			_IsWindowVisible = true;
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnBecameInvisible()
		{
			_IsWindowVisible = false;
		}

#if ARBOR_EDITOR_EXTENSIBLE
		private bool _IsSkinChanged = false;

		void BeginSkin()
		{
			if (skin == null || _IsSkinChanged)
			{
				return;
			}

			skin.Begin();

			_IsSkinChanged = true;
		}

		void BeginSkin(Rect rect, bool isHostView)
		{
			BeginSkin();

			if (isHostView && Event.current.type == EventType.Repaint)
			{
				Styles.hostview.Draw(rect, GUIContent.none, false, false, false, false);
			}
		}

		void EndSkin()
		{
			if (skin == null || !_IsSkinChanged)
			{
				return;
			}

			skin.End();

			_IsSkinChanged = false;
		}
#endif

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnGUI()
		{
#if !ARBOR_EDITOR_USE_UIELEMENTS && ARBOR_EDITOR_EXTENSIBLE
			Rect hostPosition = position;
			hostPosition.position = Vector2.zero;
			BeginSkin(hostPosition, true);
#endif

			using (new ProfilerScope("ArborEditorWindow.OnGUI"))
			{
				bool isPlaying = EditorApplication.isPlaying;
				if (_IsPlaying != isPlaying)
				{
					_Initialized = false;
					_IsPlaying = isPlaying;
				}

				if (!_Initialized)
				{
					ReatachIfNecessary();

					if (_GraphEditor != null)
					{
						_GraphEditor.InitializeGraph();
						DoRepaint();

						if (isPlaying)
						{
							_IsUpdateLiveTracking = true;
						}
					}

					_Initialized = true;
				}
				else
				{
					if (_GraphEditor != null)
					{
						_GraphEditor.RebuildIfNecessary();
					}
				}

#if ARBOR_EDITOR_USE_UIELEMENTS
#if ARBOR_EDITOR_CAPTURABLE
				if (isCapture)
				{
					CaptureGUI();
				}
#endif
#else
				ResizeHandling(this.position.width, this.position.height - EditorStyles.toolbar.fixedHeight);
				CalculateRect();

				ToolbarGUI(_ToolBarRect);

				EditorGUILayout.BeginHorizontal();

				if (ArborSettings.openSidePanel)
				{
					_SidePanel.ToolbarGUI(_SideToolbarRect);
					_SidePanel.OnGUI(_SidePanelRect);
				}

				BreadcrumbGUI(_BreadcrumbRect);

#if ARBOR_EDITOR_CAPTURABLE
				if (isCapture)
				{
					CaptureGUI();
				}
				else
#endif
				{
					if (_GraphEditor == null)
					{
						NoGraphSelectedGUI(graphViewRect);
					}
					else
					{
						UnderlayGUI(graphViewportPosition);
						GraphViewGUI();
						OverlayGUI(graphViewportPosition);
					}
				}

				EditorGUILayout.EndHorizontal();
#endif
			}

#if !ARBOR_EDITOR_USE_UIELEMENTS && ARBOR_EDITOR_EXTENSIBLE
			EndSkin();
#endif
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		private void ShowButton(Rect r)
		{
			bool flag = GUI.Toggle(r, _IsLocked, GUIContent.none, Styles.lockButton);
			if (flag == _IsLocked)
			{
				return;
			}
			_IsLocked = flag;
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		private void OnLostFocus()
		{
			if (_GraphEditor != null)
			{
				_GraphEditor.EndRename(true);
			}
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnDestroy()
		{
			SetNodeGraphRoot(null);
			SetNodeGraphCurrent(null);

			if (_GraphEditor != null)
			{
				Object.DestroyImmediate(_GraphEditor);
				_GraphEditor = null;
			}
		}

		#endregion // Unity methods

		void OnUpdateCheckDone()
		{
			ArborUpdateCheck updateCheck = ArborUpdateCheck.instance;
			UpdateInfo updateInfo = updateCheck.updateInfo;
			if (updateInfo == null)
			{
				return;
			}

			bool isUpdated = updateCheck.isUpdated;
			if (isUpdated)
			{
				DoRepaint();
			}
		}

#if ARBOR_EDITOR_CAPTURABLE
		void CaptureGUI()
		{
			if (_Capture.BeginCaptureGUI())
			{
				UnderlayGUI(graphExtents);

				try
				{
					GraphGUI();

					OverlayGUI(graphExtents);
				}
				finally
				{
					if (_Capture.EndCaptureGUI())
					{
						string path = EditorUtility.SaveFilePanel("Save", ArborEditorCache.captureDirectory, _SelectedGraphItem.displayName, "png");
						if (!string.IsNullOrEmpty(path))
						{
							ArborEditorCache.captureDirectory = System.IO.Path.GetDirectoryName(path);
						}
						_Capture.SaveImage(path, true);
						_Capture.Destroy();
						_Capture = null;

						_IsCapture = false;

						DirtyGraphExtents();
					}
				}
			}
			else
			{
				_IsCapture = false;

				DirtyGraphExtents();
			}
		}
#endif

		void DrawLogo(Rect rect, float graphScale, bool forceDraw = false)
		{
			if (!(visibleLogo || forceDraw) || Event.current.type != EventType.Repaint)
			{
				return;
			}

			float alpha = 0.5f;
			if (!forceDraw && ArborSettings.showLogo == LogoShowMode.FadeOut)
			{
				float t = Mathf.Clamp01((float)(EditorApplication.timeSinceStartup - (_FadeLogoBeginTime + k_ShowLogoTime)) / k_FadeLogoTime);
				alpha = Mathf.Lerp(0.5f, 0f, t);
			}

			Color tempColor = GUI.color;
			GUI.color = new Color(1f, 1f, 1f, alpha);
			Texture2D logoTex = Icons.logo;

			float width = 256f / graphScale;
			float scale = width / logoTex.width;
			float height = logoTex.height * scale;

			Rect logoPosition = rect;
			logoPosition.xMax = logoPosition.xMin + width;
			logoPosition.yMax = logoPosition.yMin + height;
			GUI.DrawTexture(logoPosition, logoTex, ScaleMode.ScaleToFit);
			GUI.color = tempColor;
		}

		void UnderlayGUI(Rect rect)
		{
			if (_GraphEditor == null || _GraphEditor.nodeGraph == null)
			{
				return;
			}

			_GraphEditor.UpdateLayer(isCapture || !zoomable);

			EditorGUITools.DrawGridBackground(rect);

#if ARBOR_EDITOR_EXTENSIBLE
			if (underlayGUI != null)
			{
				GUI.BeginGroup(rect);

				Rect groupRect = rect;
				groupRect.position = Vector2.zero;

				GUILayout.BeginArea(groupRect);
				underlayGUI(_GraphEditor.nodeGraph, groupRect);
				GUILayout.EndArea();

				GUI.EndGroup();
			}
#endif

			GUIContent label = _GraphEditor.GetGraphLabel();
			if (label != null)
			{
				Vector2 size = Styles.graphLabel.CalcSize(label);
				Rect labelPosition = rect;
				labelPosition.xMin = labelPosition.xMax - size.x;
				labelPosition.yMin = labelPosition.yMax - size.y;
				GUI.Label(labelPosition, label, Styles.graphLabel);
			}

			if (Application.isPlaying && _GraphEditor.HasPlayState())
			{
				PlayState playState = _GraphEditor.GetPlayState();

				GUIContent playStateLabel = null;

				switch (playState)
				{
					case PlayState.Stopping:
						playStateLabel = Localization.GetTextContent("PlayState.Stopping");
						break;
					case PlayState.Playing:
						playStateLabel = Localization.GetTextContent("PlayState.Playing");
						break;
					case PlayState.Pausing:
						playStateLabel = Localization.GetTextContent("PlayState.Pausing");
						break;
					case PlayState.InactivePausing:
						playStateLabel = Localization.GetTextContent("PlayState.InactivePausing");
						break;
				}

				if (playStateLabel != null)
				{
					GUIStyle style = Styles.playStateLabel;

					Vector2 size = style.CalcSize(playStateLabel);
					Rect labelPosition = rect;
					labelPosition.xMin = labelPosition.xMax - size.x;
					labelPosition.yMin = labelPosition.yMax - size.y;
					labelPosition.width = size.x;
					labelPosition.height = size.y;
					GUI.Label(labelPosition, playStateLabel, style);
				}
			}
		}

		void OverlayGUI(Rect rect)
		{
			if (_GraphEditor == null || _GraphEditor.nodeGraph == null)
			{
				return;
			}

#if ARBOR_EDITOR_EXTENSIBLE
			if (overlayGUI != null)
			{
				GUI.BeginGroup(rect);

				Rect groupRect = rect;
				groupRect.position = Vector2.zero;

				GUILayout.BeginArea(groupRect);
				overlayGUI(_GraphEditor.nodeGraph, groupRect);
				GUILayout.EndArea();


				GUI.EndGroup();
			}
#endif

			if (!_GraphEditor.editable)
			{
				Color guiColor = GUI.color;
				GUI.color = Color.red;
				if (Event.current.type == EventType.Repaint)
				{
					Styles.graphHighlight.Draw(rect, false, false, false, false);
				}

				GUIStyle style = Styles.graphLabel;

				Vector2 size = style.CalcSize(EditorContents.notEditable);
				Rect labelPosition = rect;
				labelPosition.xMin = labelPosition.xMax - size.x - Styles.graphLabel.padding.right;
				labelPosition.yMax = labelPosition.yMin + size.y;
				GUI.Label(labelPosition, EditorContents.notEditable, style);

				GUI.color = guiColor;
			}

#if ARBOR_TRIAL
			DrawLogo(rect, 1f);
#else
			if (isCapture || ArborSettings.showLogo == LogoShowMode.FadeOut)
			{
				DrawLogo(rect, 1f, isCapture);
			}
#endif
		}

		internal void OnPostLayout()
		{
			if (!_IsLayoutSetup)
			{
				_IsLayoutSetup = true;
				CenterOnStoredPosition(_SelectedGraphItem);

#if !ARBOR_EDITOR_USE_UIELEMENTS
				DoRepaint();
#endif
			}
		}

		void DestroyGraphEditor()
		{
			if (_GraphEditor == null)
			{
				return;
			}

			Object.DestroyImmediate(_GraphEditor);
			_GraphEditor = null;
#if ARBOR_EDITOR_USE_UIELEMENTS
			if (_GraphPanel.Contains(_GraphView))
			{
				_GraphView.RemoveFromHierarchy();
				_GraphPanel.Add(_NoGraphUI);
			}
#endif
		}

		void Initialize()
		{
			DestroyGraphEditor();

			Undo.RecordObject(this, "Select NodeGraph");

			if (_NodeGraphRoot != null)
			{
				SetNodeGraphRoot(_NodeGraphRoot);
				var rootItem = FindTreeViewItem(_NodeGraphRoot) as GraphTreeViewItem;
				SetNodeGraphCurrent(rootItem);
			}
			else
			{
				SetNodeGraphRoot(null);
				SetNodeGraphCurrent(null);
			}

			_Initialized = false;

			EditorUtility.SetDirty(this);

			DoRepaint();
		}

		void InternalSelectRootGraph(NodeGraph rootGraph, bool isExternal)
		{
			int undoGroup = Undo.GetCurrentGroup();

			Undo.RecordObject(this, "Select NodeGraph");

			if (isExternal)
			{
				if (_NodeGraphRootPrev == null)
				{
					_NodeGraphRootPrev = _NodeGraphRoot;
					_NodeGraphRootPrevInstanceID = _NodeGraphRootPrev.GetInstanceID();
				}
			}
			else
			{
				_NodeGraphRootPrev = null;
				_NodeGraphRootPrevInstanceID = 0;
			}

			SetNodeGraphRoot(rootGraph);

			var rootItem = FindTreeViewItem(rootGraph) as GraphTreeViewItem;
			SetNodeGraphCurrent(rootItem);

			Undo.CollapseUndoOperations(undoGroup);

			EditorUtility.SetDirty(this);

			RebuildGraphEditor();

			BeginFadeLogo(true);

			DoRepaint();
		}

		void OpenInternal(NodeGraph nodeGraph)
		{
			NodeGraph rootGraph = nodeGraph.rootGraph;
			SelectRootGraph(rootGraph);
			if (rootGraph != nodeGraph)
			{
				var graphItem = treeView.FindItem(nodeGraph.GetInstanceID()) as GraphTreeViewItem;
				ChangeCurrentNodeGraph(graphItem);
			}
		}

		public void SelectRootGraph(NodeGraph nodeGraph)
		{
			if (_NodeGraphRootPrev == null && _NodeGraphRoot == nodeGraph && _NodeGraphCurrent == nodeGraph)
			{
				return;
			}

			InternalSelectRootGraph(nodeGraph, false);
		}

		public void SelectExternalGraph(GraphTreeViewItem graphItem)
		{
			NodeGraph rootGraph = graphItem.nodeGraph.rootGraph;

			InternalSelectRootGraph(rootGraph, true);
		}

		internal void BeginFadeLogo(bool forceFade = false)
		{
			if ((!_FadeLogo || forceFade) && ArborSettings.showLogo == LogoShowMode.FadeOut)
			{
				_FadeLogo = true;
				_FadeLogoBeginTime = EditorApplication.timeSinceStartup;
			}
		}

		void RebuildGraphEditor()
		{
			NodeGraph nodeGraphCurrent = _SelectedGraphItem != null ? _SelectedGraphItem.nodeGraph : null;

			DestroyGraphEditor();

			bool nextHasGraphEditor = nodeGraphCurrent != null;

			if (!nextHasGraphEditor)
			{
				return;
			}

			_GraphEditor = NodeGraphEditor.CreateEditor(this, nodeGraphCurrent, _SelectedGraphItem.isExternal);

#if ARBOR_EDITOR_USE_UIELEMENTS
			if (!_GraphPanel.Contains(_GraphView))
			{
				_NoGraphUI.RemoveFromHierarchy();
				_GraphPanel.Add(_GraphView);
			}
#endif

			_IsRepaint = true;
			_Initialized = false;

			DirtyGraphExtents();
		}

		public void ChangeCurrentNodeGraph(GraphTreeViewItem graphItem, bool liveTracking = false)
		{
			if (graphItem == null)
			{
				return;
			}

			if (_SelectedGraphItem == graphItem)
			{
				return;
			}

			if (!liveTracking && Application.isPlaying &&
				ArborSettings.liveTracking && ArborSettings.liveTrackingHierarchy &&
				_GraphEditor != null && _GraphEditor.GetPlayState() != PlayState.Stopping)
			{
				ArborSettings.liveTracking = false;
			}

			_NextGraph = graphItem;

			DoRepaint();
		}

		void SetCurrentNodeGraph(GraphTreeViewItem graphItem)
		{
			if (_SelectedGraphItem == graphItem)
			{
				return;
			}

			int undoGroup = Undo.GetCurrentGroup();

			Undo.RecordObject(this, "Select NodeGraph");

			SetNodeGraphCurrent(graphItem);

			Undo.CollapseUndoOperations(undoGroup);

			EditorUtility.SetDirty(this);

			RebuildGraphEditor();

			BeginFadeLogo(true);

			DoRepaint();
		}

		internal void DoRepaint()
		{
#if ARBOR_EDITOR_USE_UIELEMENTS
			_GraphUI.MarkDirtyRepaint();
#endif

			Repaint();
			_IsRepaint = false;
		}

		internal void BeginSelection()
		{
			_IsSelection = true;
			_SelectionRect = new Rect();
		}

		internal void SetSelectionRect(Rect rect)
		{
			_SelectionRect = GraphToWindowRect(rect);
		}

		internal void EndSelection()
		{
			_IsSelection = false;
		}

		void DrawSelection()
		{
			if (Event.current.type != EventType.Repaint || !_IsSelection || _SelectionRect == new Rect())
			{
				return;
			}

			Styles.selectionRect.Draw(_SelectionRect, false, false, false, false);
		}

		internal void SidePanelToggle()
		{
			EditorGUI.BeginChangeCheck();
			ArborSettings.openSidePanel = GUILayout.Toggle(ArborSettings.openSidePanel, ArborSettings.openSidePanel ? EditorContents.sidePanelOn : EditorContents.sidePanelOff, Styles.invisibleButton);
			if (EditorGUI.EndChangeCheck())
			{
#if ARBOR_EDITOR_USE_UIELEMENTS
				if (ArborSettings.openSidePanel)
				{
					_MainLayout.Insert(0,_LeftPanelLayout);
				}
				else
				{
					_MainLayout.Remove(_LeftPanelLayout);
				}
#endif
			}
		}

		void OpenCreateMenu(Rect buttonRect)
		{
			buttonRect = EditorGUITools.GUIToScreenRect(buttonRect);
			GraphMenuWindow.instance.Init(this, buttonRect);
		}

		void ToolbarGUI(Rect rect)
		{
			using (new ProfilerScope("ToolbarGUI"))
			{
				GUILayout.BeginArea(rect);

				EditorGUILayout.BeginHorizontal(Styles.toolbar, GUILayout.Height(EditorGUITools.toolbarHeight));

				if (!ArborSettings.openSidePanel)
				{
					using (new ProfilerScope("SidePanel Toggle"))
					{
						SidePanelToggle();

						EditorGUILayout.Space();
					}
				}

				using (new ProfilerScope("Create Field"))
				{
					GUIContent content = EditorContents.create;
					GUIStyle style = EditorStyles.toolbarDropDown;
					Rect buttonRect = GUILayoutUtility.GetRect(content, style);
					if (GUI.Button(buttonRect, content, style))
					{
						GUIUtility.keyboardControl = 0;

						OpenCreateMenu(buttonRect);
					}
				}

				using (new ProfilerScope("NodeGraph Field"))
				{
					EditorGUI.BeginChangeCheck();
					NodeGraph rootGraph = _NodeGraphRootPrev ?? _NodeGraphRoot;
					NodeGraph nodeGraph = EditorGUILayout.ObjectField(rootGraph, typeof(NodeGraph), true, GUILayout.Width(200)) as NodeGraph;
					if (EditorGUI.EndChangeCheck())
					{
						SelectRootGraph(nodeGraph);
					}
				}

				GUILayout.FlexibleSpace();

				if (_GraphEditor != null)
				{
#if ARBOR_EDITOR_EXTENSIBLE
					if (toolbarGUI != null)
					{
						toolbarGUI(_GraphEditor.nodeGraph);
					}
#endif

					using (new ProfilerScope("LiveTrace Button"))
					{
						EditorGUI.BeginChangeCheck();
						ArborSettings.liveTracking = GUILayout.Toggle(ArborSettings.liveTracking, EditorContents.liveTracking, EditorStyles.toolbarButton);
						if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
						{
							_IsUpdateLiveTracking = true;
						}
					}

					using (new ProfilerScope("View Button"))
					{
						GUIContent content = EditorContents.view;
						Rect buttonPosition = GUILayoutUtility.GetRect(content, EditorStyles.toolbarDropDown);
						if (GUI.Button(buttonPosition, content, EditorStyles.toolbarDropDown))
						{
							GenericMenu menu = new GenericMenu();

							_GraphEditor.SetViewMenu(menu);

							menu.DropDown(buttonPosition);
						}
					}

					using (new ProfilerScope("Debug Button"))
					{
						GUIContent content = EditorContents.debug;
						Rect buttonPosition = GUILayoutUtility.GetRect(content, EditorStyles.toolbarDropDown);
						if (GUI.Button(buttonPosition, content, EditorStyles.toolbarDropDown))
						{
							GenericMenu menu = new GenericMenu();

							_GraphEditor.SetDenugMenu(menu);

							menu.DropDown(buttonPosition);
						}
					}
#if ARBOR_EDITOR_CAPTURABLE
					using (new ProfilerScope("Capture Button"))
					{
						Color contentColor = GUI.contentColor;
						GUI.contentColor = isDarkSkin ? Color.white : Color.black;
						if (EditorGUITools.IconButton(EditorContents.captureIcon))
						{
							_GraphCaptureExtents = new RectOffset(100, 100, 100, 100).Add(_GraphExtents);

							if (_GraphCaptureExtents.width < 500)
							{
								float center = _GraphCaptureExtents.center.x;
								_GraphCaptureExtents.xMin = center - 250;
								_GraphCaptureExtents.xMax = center + 250;
							}
							if (_GraphCaptureExtents.height < 500)
							{
								float center = _GraphCaptureExtents.center.y;
								_GraphCaptureExtents.yMin = center - 250;
								_GraphCaptureExtents.yMax = center + 250;
							}

							_GraphCaptureExtents.x = Mathf.Floor(_GraphCaptureExtents.x);
							_GraphCaptureExtents.width = Mathf.Floor(_GraphCaptureExtents.width);
							_GraphCaptureExtents.y = Mathf.Floor(_GraphCaptureExtents.y);
							_GraphCaptureExtents.height = Mathf.Floor(_GraphCaptureExtents.height);

							int maxTextureSize = SystemInfo.maxTextureSize;
							if (_GraphCaptureExtents.width <= maxTextureSize && _GraphCaptureExtents.height <= maxTextureSize)
							{
								_Capture = new EditorCapture(this);
								if (_Capture.Initialize(_GraphCaptureExtents))
								{
									_IsCapture = true;
									DoRepaint();
								}
							}
							else
							{
								Debug.LogError("Screenshot failed : Graph size is too large.");
							}
						}
						GUI.contentColor = contentColor;
					}
#endif
				}

				ArborUpdateCheck updateCheck = ArborUpdateCheck.instance;
				if (updateCheck.isUpdated || updateCheck.isUpgrade)
				{
					using (new ProfilerScope("Update Button"))
					{
						Color contentColor = GUI.contentColor;
						GUI.contentColor = isDarkSkin ? Color.white : Color.black;
						if (EditorGUITools.IconButton(EditorContents.notificationIcon))
						{
							UpdateNotificationWindow.Open();
						}
						GUI.contentColor = contentColor;
					}
				}

				using (new ProfilerScope("Help Button"))
				{
					Rect helpButtonPosition;
					if (EditorGUITools.IconButton(EditorContents.helpIcon, out helpButtonPosition))
					{
						GenericMenu menu = new GenericMenu();
						menu.AddItem(EditorContents.assetStore, false, () =>
						{
							ArborVersion.OpenAssetStore();
						});
						menu.AddSeparator("");
						menu.AddItem(EditorContents.officialSite, false, () =>
						{
							Help.BrowseURL(Localization.GetWord("SiteURL"));
						});
						menu.AddItem(EditorContents.manual, false, () =>
						{
							Help.BrowseURL(Localization.GetWord("ManualURL"));
						});
						menu.AddItem(EditorContents.inspectorReference, false, () =>
						{
							Help.BrowseURL(Localization.GetWord("InspectorReferenceURL"));
						});
						menu.AddItem(EditorContents.scriptReference, false, () =>
						{
							Help.BrowseURL(Localization.GetWord("ScriptReferenceURL"));
						});
						menu.AddSeparator("");
						menu.AddItem(EditorContents.releaseNotes, false, () =>
						{
							Help.BrowseURL(Localization.GetWord("ReleaseNotesURL"));
						});
						menu.AddItem(EditorContents.forum, false, () =>
						{
							Help.BrowseURL(Localization.GetWord("ForumURL"));
						});
						menu.DropDown(helpButtonPosition);
					}
				}

				using (new ProfilerScope("Settings Button"))
				{
					GUIContent settingContent = EditorContents.popupIcon;
					Rect settingButtonPosition;
					if (EditorGUITools.IconButton(settingContent, out settingButtonPosition))
					{
						if (_GraphSettingsWindow == null)
						{
							_GraphSettingsWindow = new GraphSettingsWindow(this);
						}
						PopupWindowUtility.Show(settingButtonPosition, _GraphSettingsWindow, true);
					}
				}

				EditorGUILayout.EndHorizontal();

				GUILayout.EndArea();
			}
		}

		private List<TreeViewItem> _BreadCrumbGraphs = new List<TreeViewItem>();

		void BreadcrumbGUI(Rect rect)
		{
			using (new ProfilerScope("BreadcrumbGUI"))
			{
				if (Event.current.type == EventType.Repaint)
				{
					Styles.toolbar.Draw(rect, false, false, false, false);
				}

				rect = Styles.toolbar.padding.Remove(rect);

				rect.width = 0f;

				for (int i = 0; i < _BreadCrumbGraphs.Count; i++)
				{
					var item = _BreadCrumbGraphs[i];

					var graphItem = item as GraphTreeViewItem;

					GUIContent graphName = EditorGUITools.GetTextContent(item.displayName);

					GUIStyle style = GUIStyle.none;
					if (i != 0)
					{
						style = graphItem.isExternal? Styles.breadcrumbMidPrefab : Styles.breadcrumbMid;
					}
					else
					{
						style = graphItem.isExternal ? Styles.breadcrumbLeftPrefab : Styles.breadcrumbLeft;
					}

					GUIStyle guiStyle = i != 0 ? Styles.breadcrumbMidBg : Styles.breadcrumbLeftBg;

					Vector2 size = style.CalcSize(graphName);

					rect.width = size.x;

					bool on = i == _BreadCrumbGraphs.Count - 1;

					if (guiStyle != null && Event.current.type == EventType.Repaint)
					{
						guiStyle.Draw(rect, GUIContent.none, 0, on);
					}

					EditorGUI.BeginChangeCheck();
					GUI.Toggle(rect, on, graphName, style);
					if (EditorGUI.EndChangeCheck())
					{
						ChangeCurrentNodeGraph(graphItem);
					}

					rect.x += rect.width;
				}
			}
		}

		void OnDestroyNodeGraph(NodeGraph nodeGraph)
		{
			if (EditorApplication.isPlaying != EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			if (ReferenceEquals(_NodeGraphRoot, nodeGraph))
			{
				Undo.RecordObject(this, "Destroy NodeGraph");

				SetNodeGraphRoot(null);
				SetNodeGraphCurrent(null);
			}
			else if (ReferenceEquals(_NodeGraphCurrent, nodeGraph))
			{
				Undo.RecordObject(this, "Destroy NodeGraph");

				var rootItem = FindTreeViewItem(_NodeGraphRoot) as GraphTreeViewItem;
				SetNodeGraphCurrent(rootItem);
			}

			RebuildGraphEditor();

			EditorUtility.SetDirty(this);

			DoRepaint();
		}

		public void OnChangedGraphTree()
		{
			if (!_IsDirtyTreeDelay)
			{
				_IsDirtyTreeDelay = true;
				EditorApplication.delayCall += OnDirtyTreeDelay;
			}
		}

		static void AddGraphItem(TreeViewItem parent, NodeGraph nodeGraph)
		{
			if (nodeGraph == null)
			{
				return;
			}

			int nodeCount = nodeGraph.nodeCount;
			for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
			{
				INodeBehaviourContainer behaviours = nodeGraph.GetNodeFromIndex(nodeIndex) as INodeBehaviourContainer;
				if (behaviours == null)
				{
					continue;
				}

				int behaviourCount = behaviours.GetNodeBehaviourCount();
				for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
				{
					NodeBehaviour behaviour = behaviours.GetNodeBehaviour<NodeBehaviour>(behaviourIndex);
					ISubGraphBehaviour subGraphBehaviour = behaviour as ISubGraphBehaviour;
					if (subGraphBehaviour != null)
					{
						var referenceGraph = subGraphBehaviour.GetSubGraph();
						if (referenceGraph != null)
						{
							var newItem = new SubGraphTreeViewItem(behaviour.GetInstanceID(), subGraphBehaviour);
							parent.AddChild(newItem);

							if (!subGraphBehaviour.isExternal)
							{
								AddGraphItem(newItem, subGraphBehaviour.GetSubGraph());
							}
						}
					}
				}
			}
		}

		void BuildTree(NodeGraph rootGraph)
		{
			_TreeView.ClearTree();

			if (rootGraph != null)
			{
				var item = new GraphTreeViewItem(rootGraph);
				_TreeView.root.AddChild(item);

				AddGraphItem(item, rootGraph);
			}

			_TreeView.SetupDepths();

			_SidePanel.graphTabPanel.DirtyTree();

			UpdateBreadCrumb();

			Repaint();
		}

		TreeViewItem FindTreeViewItem(NodeGraph nodeGraph)
		{
			if (nodeGraph == null)
			{
				return null;
			}

			return _TreeView.FindItem((item) =>
			{
				GraphTreeViewItem graphItem = item as GraphTreeViewItem;
				return graphItem != null && graphItem.nodeGraph == nodeGraph;
			});
		}

		private bool _IsDirtyTreeDelay;

		void OnDirtyTreeDelay()
		{
			if (!_IsDirtyTreeDelay)
			{
				return;
			}

			BuildTree(_NodeGraphRoot);

			var item = FindTreeViewItem(_NodeGraphCurrent) as GraphTreeViewItem;
			SetNodeGraphCurrent(item);

			_IsDirtyTreeDelay = false;
		}
		
		void RegisterRootGraphCallback()
		{
			if (_NodeGraphRootAttachedCallback)
			{
				return;
			}

			_NodeGraphRoot.destroyCallback += OnDestroyNodeGraph;
			_NodeGraphRoot.stateChangedCallback += OnStateChanged;
			_NodeGraphRoot.onChangedGraphTree += OnChangedGraphTree;
			_NodeGraphRootAttachedCallback = true;
		}

		void UnregisterRootGraphCallback()
		{
			if (!_NodeGraphRootAttachedCallback)
			{
				return;
			}

			_NodeGraphRoot.destroyCallback -= OnDestroyNodeGraph;
			_NodeGraphRoot.stateChangedCallback -= OnStateChanged;
			_NodeGraphRoot.onChangedGraphTree -= OnChangedGraphTree;
			_NodeGraphRootAttachedCallback = false;
		}

		void SetNodeGraphRoot(NodeGraph nodeGraph)
		{
			if (!object.ReferenceEquals(_NodeGraphRoot,null))
			{
				UnregisterRootGraphCallback();
				_NodeGraphRootInstanceID = 0;
			}

			if (_NodeGraphRoot != nodeGraph)
			{
				_TreeViewState.Clear();
			}

			_NodeGraphRoot = nodeGraph;
			BuildTree(_NodeGraphRoot);

			if (_NodeGraphRoot != null)
			{
				RegisterRootGraphCallback();
				_NodeGraphRootInstanceID = _NodeGraphRoot.GetInstanceID();
			}
			else
			{
				_NodeGraphRootInstanceID = 0;
			}
		}

		void RegisterCurrentGraphCallback()
		{
			if (_NodeGraphCurrentAttachedCallback)
			{
				return;
			}

			_NodeGraphCurrent.destroyCallback += OnDestroyNodeGraph;
			_NodeGraphCurrent.stateChangedCallback += OnStateChanged;
			_NodeGraphCurrentAttachedCallback = true;
		}

		void UnregisterCurrentGraphCallback()
		{
			if (!_NodeGraphCurrentAttachedCallback)
			{
				return;
			}

			_NodeGraphCurrent.destroyCallback -= OnDestroyNodeGraph;
			_NodeGraphCurrent.stateChangedCallback -= OnStateChanged;
			_NodeGraphCurrentAttachedCallback = false;
		}

		void UpdateBreadCrumb()
		{
			_BreadCrumbGraphs.Clear();

			NodeGraph currentGraph = _NodeGraphCurrent;
			if (currentGraph == null && _NodeGraphCurrentInstanceID != 0)
			{
				currentGraph = EditorUtility.InstanceIDToObject(_NodeGraphCurrentInstanceID) as NodeGraph;
			}
			var currentItem = FindTreeViewItem(currentGraph);
			while (currentItem != null && currentItem.id != 0)
			{
				_BreadCrumbGraphs.Insert(0, currentItem);
				currentItem = currentItem.parent;
			}
		}

		void SetNodeGraphCurrent(GraphTreeViewItem graphItem)
		{
			NodeGraph nodeGraph = graphItem != null ? graphItem.nodeGraph : null;

			StoreCurrentTransform();

			if (!object.ReferenceEquals(_NodeGraphCurrent, null))
			{
				UnregisterCurrentGraphCallback();
				_NodeGraphCurrentInstanceID = 0;
			}

			_NodeGraphCurrent = nodeGraph;
			_SelectedGraphItem = graphItem;
			_SidePanel.graphTabPanel.SelectNopdeGraph(graphItem);

			if (_NodeGraphCurrent != null)
			{
				RegisterCurrentGraphCallback();
				_NodeGraphCurrentInstanceID = _NodeGraphCurrent.GetInstanceID();
			}
			else
			{
				_NodeGraphCurrentInstanceID = 0;
			}

			UpdateBreadCrumb();
		}

		private void ReatachIfNecessary()
		{
			bool reatached = false;

			if (_GraphEditor != null)
			{
				_GraphEditor.ReatachIfNecessary();
			}

			if (_NodeGraphRootPrev == null && _NodeGraphRootPrevInstanceID != 0)
			{
				_NodeGraphRootPrev = EditorUtility.InstanceIDToObject(_NodeGraphRootInstanceID) as NodeGraph;
				reatached = true;
			}

			bool setRoot = false;

			if (_NodeGraphRoot == null && _NodeGraphRootInstanceID != 0)
			{
				SetNodeGraphRoot(EditorUtility.InstanceIDToObject(_NodeGraphRootInstanceID) as NodeGraph);
				setRoot = true;
				reatached = true;
			}

			NodeGraph currentGraph = _NodeGraphCurrent;
			if (currentGraph == null && _NodeGraphCurrentInstanceID != 0)
			{
				currentGraph = EditorUtility.InstanceIDToObject(_NodeGraphCurrentInstanceID) as NodeGraph;
			}

			if (currentGraph != null && (_SelectedGraphItem == null || _SelectedGraphItem.nodeGraph != currentGraph))
			{
				if (!setRoot)
				{
					BuildTree(_NodeGraphRoot);
				}
				var currentItem = FindTreeViewItem(currentGraph) as GraphTreeViewItem;
				SetNodeGraphCurrent(currentItem);
				RebuildGraphEditor();
				reatached = true;
			}

			if (!reatached)
			{
				if (_NodeGraphRoot != null)
				{
					if (!_NodeGraphRootAttachedCallback)
					{
						RegisterRootGraphCallback();
					}
				}
				else
				{
					_NodeGraphRootAttachedCallback = false;
				}

				if (_NodeGraphCurrent != null)
				{
					if (!_NodeGraphCurrentAttachedCallback)
					{
						RegisterCurrentGraphCallback();
					}
				}
				else
				{
					_NodeGraphCurrentAttachedCallback = false;
				}
			}

			if (reatached)
			{
				if (_NodeGraphRoot == null || _NodeGraphCurrent == null)
				{
					Initialize();
				}
			}
			else
			{
				if ((_GraphEditor == null && _NodeGraphCurrent != null) || (_GraphEditor != null && _GraphEditor.nodeGraph != _NodeGraphCurrent))
				{
					RebuildGraphEditor();
				}
				else if (_GraphEditor != null && _GraphEditor.ReatachIfNecessary())
				{
					if (_GraphEditor.nodeGraph == null)
					{
						Initialize();
					}
				}
			}
		}

		internal void UpdateGraphExtents()
		{
			if (_GraphEditor == null)
			{
				return;
			}

			Rect extents = _GraphEditor.UpdateGraphExtents();
			_GraphExtents = extents;

			Rect graphPosition = graphViewport;

			extents.xMin -= graphPosition.width * 0.6f;
			extents.xMax += graphPosition.width * 0.6f;
			extents.yMin -= graphPosition.height * 0.6f;
			extents.yMax += graphPosition.height * 0.6f;

			extents.xMin = (int)extents.xMin;
			extents.xMax = (int)extents.xMax;
			extents.yMin = (int)extents.yMin;
			extents.yMax = (int)extents.yMax;

			if (_GraphEditor.isDragNodes)
			{
				if (graphPosition.xMin < extents.xMin)
				{
					extents.xMin = graphPosition.xMin;
				}
				if (extents.xMax < graphPosition.xMax)
				{
					extents.xMax = graphPosition.xMax;
				}

				if (graphPosition.yMin < extents.yMin)
				{
					extents.yMin = graphPosition.yMin;
				}
				if (extents.yMax < graphPosition.yMax)
				{
					extents.yMax = graphPosition.yMax;
				}
			}

			if (graphExtents != extents)
			{
#if ARBOR_EDITOR_USE_UIELEMENTS
#if UNITY_2019_2_OR_NEWER
				_GraphExtentsBoundingBoxElement.style.left = extents.x;
				_GraphExtentsBoundingBoxElement.style.top = extents.y;
				_GraphExtentsBoundingBoxElement.style.width = extents.width;
				_GraphExtentsBoundingBoxElement.style.height = extents.height;
#endif
				_GraphViewExtents = extents;
#else
				_GraphGUI.extents = extents;
				DoRepaint();
#endif
			}
		}

		public void FrameSelected(Vector2 frameSelectTarget)
		{
			_FrameSelected.Begin(frameSelectTarget);
			_FrameSelectedZoom.Begin(Vector2.one);

			DoRepaint();
		}

		public bool OverlapsVewArea(Rect position)
		{
			return graphViewport.Overlaps(position);
		}

		void UpdateScrollbar()
		{
#if ARBOR_EDITOR_USE_UIELEMENTS
			bool endFrameSelected = _EndFrameSelected;
			_EndFrameSelected = false;

			bool repaint = false;

			if (_FrameSelectedZoom.isPlaying)
			{
				Vector2 zoomScale = _FrameSelectedZoom.Update(this.graphScale,Vector2.zero);

				SetZoom(graphViewport.center, new Vector3(zoomScale.x, zoomScale.y, 1), false, !_FrameSelected.isPlaying);

				repaint = true;
			}

			if (_FrameSelected.isPlaying)
			{
				Vector2 scrollPos = _FrameSelected.Update(this.scrollPos, -graphViewport.size * 0.5f);

				SetScroll(scrollPos, true, false);

				repaint = true;
			}

			if (repaint)
			{
				DoRepaint();
			}

			_EndFrameSelected = endFrameSelected;
#else
			if (_FrameSelected.isPlaying)
			{
				switch (Event.current.type)
				{
					case EventType.MouseDown:
						if (graphViewportPosition.Contains(Event.current.mousePosition))
						{
							Vector2 scrollPos = this.scrollPos;
							scrollPos.x = (int)scrollPos.x;
							scrollPos.y = (int)scrollPos.y;
							SetScroll(scrollPos, true, true);
						}
						break;
					case EventType.Repaint:
						{
							Vector2 offset = -graphViewport.size * 0.5f;
							offset.x = Mathf.Floor(offset.x);
							offset.y = Mathf.Floor(offset.y);
							Vector2 scrollPos = _FrameSelected.Update(this.scrollPos, offset);

							SetScroll(scrollPos, true, false);

							DoRepaint();
						}
						break;
				}
			}

			if (_IsLayoutSetup)
			{
				EditorGUI.BeginChangeCheck();
				_GraphGUI.HandleScrollbar();
				if (EditorGUI.EndChangeCheck())
				{
					SetScroll(_GraphGUI.scrollPos, false, true);
				}
			}
#endif
		}

		bool IsDragNodeBehaviour()
		{
			BehaviourDragInfo behaviourDragInfo = BehaviourDragInfo.GetBehaviourDragInfo();
			return behaviourDragInfo != null && behaviourDragInfo.dragging;
		}

		void OnDragEnter()
		{
			if (_GraphEditor != null)
			{
				_GraphEditor.OnDragEnter();
			}
		}

		void OnDragUpdated()
		{
			if (_GraphEditor != null)
			{
				_GraphEditor.OnDragUpdated();
			}
		}

		void OnDragPerform(Vector2 graphMousePosition)
		{
			if (_GraphEditor != null)
			{
				_GraphEditor.OnDragPerform(graphMousePosition);
			}
		}

		void OnDragLeave()
		{
			if (_GraphEditor != null)
			{
				_GraphEditor.OnDragLeave();
			}

			DoRepaint();
		}

		void OnDragExited()
		{
			if (_GraphEditor != null)
			{
				_GraphEditor.OnDragExited();
			}
		}

#if !ARBOR_EDITOR_USE_UIELEMENTS
		private DragEventHandler _DragEventHandler = null;

		void HandleDragObject()
		{
			if (_DragEventHandler == null)
			{
				_DragEventHandler = new DragEventHandler()
				{
					onEnter = OnDragEnter,
					onUpdated = OnDragUpdated,
					onPerform = OnDragPerform,
					onLeave = OnDragLeave,
					onExited = OnDragExited
				};
			}

			_DragEventHandler.Handle(0, graphViewport);
		}
#endif

		bool IsDragObject()
		{
			return _GraphEditor.IsDragObject();
		}

		bool IsDragScroll()
		{
			return _GraphEditor.IsDragScroll() || IsDragNodeBehaviour() || IsDragObject();
		}

		private static class AutoScrollDefaults
		{
			public static readonly Color color;
			public static readonly RectOffset offset;

			static AutoScrollDefaults()
			{
				color = new Color(0.0f, 0.5f, 1.0f, 0.1f);
				offset = new RectOffset(30, 30, 30, 30);
			}
		}

		private bool _IsAutoScrolling = false;

		bool DoAutoScroll()
		{
			if (Event.current.type != EventType.Repaint)
			{
				return false;
			}

			if (!IsDragScroll())
			{
				_IsAutoScrolling = false;
				return false;
			}

			bool isDragObject = IsDragNodeBehaviour() || IsDragObject();

			Vector2 offset = Vector2.zero;

			Vector2 mousePosition = Event.current.mousePosition;

			RectOffset scrollAreaOffset = AutoScrollDefaults.offset;

			Rect viewport = graphViewport;
#if ARBOR_EDITOR_USE_UIELEMENTS
			Rect noScrollArea = WindowToGraphRect(scrollAreaOffset.Remove(graphViewportPosition));
#else
			Rect noScrollArea = scrollAreaOffset.Remove(viewport);
#endif

			if (isDragObject)
			{
				if (!_IsAutoScrolling)
				{
					_IsAutoScrolling = noScrollArea.Contains(mousePosition);
				}
			}
			else
			{
				_IsAutoScrolling = true;
			}

			if (!_IsAutoScrolling)
			{
				return false;
			}

			EditorGUI.DrawRect(Rect.MinMaxRect(viewport.xMin, viewport.yMin, noScrollArea.xMin, viewport.yMax), AutoScrollDefaults.color);
			EditorGUI.DrawRect(Rect.MinMaxRect(noScrollArea.xMax, viewport.yMin, viewport.xMax, viewport.yMax), AutoScrollDefaults.color);

			EditorGUI.DrawRect(Rect.MinMaxRect(noScrollArea.xMin, viewport.yMin, noScrollArea.xMax, noScrollArea.yMin), AutoScrollDefaults.color);
			EditorGUI.DrawRect(Rect.MinMaxRect(noScrollArea.xMin, noScrollArea.yMax, noScrollArea.xMax, viewport.yMax), AutoScrollDefaults.color);

			if (isDragObject && !viewport.Contains(mousePosition))
			{
				return false;
			}

			if (mousePosition.x < noScrollArea.xMin)
			{
				offset.x = mousePosition.x - noScrollArea.xMin;
			}
			else if (noScrollArea.xMax < mousePosition.x)
			{
				offset.x = mousePosition.x - noScrollArea.xMax;
			}

			if (mousePosition.y < noScrollArea.yMin)
			{
				offset.y = mousePosition.y - noScrollArea.yMin;
			}
			else if (noScrollArea.yMax < mousePosition.y)
			{
				offset.y = mousePosition.y - noScrollArea.yMax;
			}

			offset.x = Mathf.Clamp(offset.x, -10.0f, 10.0f);
			offset.y = Mathf.Clamp(offset.y, -10.0f, 10.0f);

			if (offset.sqrMagnitude > 0.0f)
			{
				Vector2 scrollPos = this.scrollPos;

				scrollPos += offset;
				scrollPos.x = (int)scrollPos.x;
				scrollPos.y = (int)scrollPos.y;

				SetScroll(scrollPos, true, false);

				return true;
			}

			return false;
		}

		internal void DirtyGraphExtents()
		{
			_IsLayoutSetup = false;
#if ARBOR_EDITOR_USE_UIELEMENTS
			_GraphView.UpdateLayout();
#else
			DoRepaint();
#endif
		}
		private Vector2 _NoGraphScrollPos = Vector2.zero;

		void NoGraphSelectedGUI(Rect rect)
		{
			using (new GUILayout.AreaScope(rect))
			{
				rect.position = Vector2.zero;

				using (GUILayout.ScrollViewScope scope = new GUILayout.ScrollViewScope(_NoGraphScrollPos))
				{
					_NoGraphScrollPos = scope.scrollPosition;

					GUILayout.FlexibleSpace();

					using (new GUILayout.HorizontalScope())
					{
						GUILayout.FlexibleSpace();

						GUILayout.Label(EditorContents.noGraphSelectedMessage);

						GUILayout.FlexibleSpace();
					}

					EditorGUILayout.Space();

					using (new GUILayout.HorizontalScope())
					{
						GUILayout.FlexibleSpace();

						using (new GUILayout.HorizontalScope())
						{
							GUIContent content = EditorContents.create;
							GUIStyle style = Styles.largeDropDown;
							Rect buttonRect = GUILayoutUtility.GetRect(content, style);
							if (GUI.Button(buttonRect, content, style))
							{
								OpenCreateMenu(buttonRect);
							}
						}

						GUILayout.FlexibleSpace();
					}

					EditorGUILayout.Space();

					float buttonWidth = 130;

					using (new GUILayout.HorizontalScope())
					{
						GUILayout.FlexibleSpace();

						if (GUILayout.Button(EditorContents.assetStore, EditorStyles.miniButtonLeft, GUILayout.Width(buttonWidth)))
						{
							ArborVersion.OpenAssetStore();
						}

						if (GUILayout.Button(EditorContents.officialSite, EditorStyles.miniButtonMid, GUILayout.Width(buttonWidth)))
						{
							Help.BrowseURL(Localization.GetWord("SiteURL"));
						}

						if (GUILayout.Button(EditorContents.releaseNotes, EditorStyles.miniButtonRight, GUILayout.Width(buttonWidth)))
						{
							Help.BrowseURL(Localization.GetWord("ReleaseNotesURL"));
						}

						GUILayout.FlexibleSpace();
					}

					using (new GUILayout.HorizontalScope())
					{
						GUILayout.FlexibleSpace();

						if (GUILayout.Button(EditorContents.manual, EditorStyles.miniButtonLeft, GUILayout.Width(buttonWidth)))
						{
							Help.BrowseURL(Localization.GetWord("ManualURL"));
						}

						if (GUILayout.Button(EditorContents.inspectorReference, EditorStyles.miniButtonMid, GUILayout.Width(buttonWidth)))
						{
							Help.BrowseURL(Localization.GetWord("InspectorReferenceURL"));
						}

						if (GUILayout.Button(EditorContents.scriptReference, EditorStyles.miniButtonRight, GUILayout.Width(buttonWidth)))
						{
							Help.BrowseURL(Localization.GetWord("ScriptReferenceURL"));
						}

						GUILayout.FlexibleSpace();
					}

					GUILayout.FlexibleSpace();
				}
			}
		}

		void GraphViewGUI()
		{
			if (_GraphEditor == null || _GraphEditor.nodeGraph == null)
			{
				return;
			}

#if !ARBOR_EDITOR_USE_UIELEMENTS
			_GraphGUI.position = _GraphRect;
#endif

			_GraphEditor.OnRenameEvent();

#if ARBOR_TRIAL
			GUIContent openContent = EditorGUITools.GetTextContent( "Open Asset Store" );
			Vector2 openButtonSize = Styles.largeButton.CalcSize( openContent );
			Rect openRect = new Rect(_GraphRect.xMin + 16.0f, _GraphRect.yMax - openButtonSize.y - 16.0f, openButtonSize.x, openButtonSize.y );

			if( Event.current.type != EventType.Repaint )
			{
				if( GUI.Button( openRect, openContent, Styles.largeButton ) )
				{
					ArborVersion.OpenAssetStore();
				}
			}
#endif

			UpdateScrollbar();

#if !ARBOR_EDITOR_USE_UIELEMENTS
			_GraphGUI.BeginGraphGUI();
#endif

			GraphGUI();

#if !ARBOR_EDITOR_USE_UIELEMENTS
			_GraphGUI.EndGraphGUI();
#endif

#if !ARBOR_EDITOR_USE_UIELEMENTS
			UpdateGraphExtents();
			OnPostLayout();
#endif

			_GraphEditor.UpdateVisibleNodes();

#if ARBOR_TRIAL
			if( Event.current.type == EventType.Repaint )
			{
				if( GUI.Button( openRect, openContent, Styles.largeButton ) )
				{
					ArborVersion.OpenAssetStore();
				}
			}
#endif
		}

		void GraphGUI()
		{
			if (_GraphEditor == null || _GraphEditor.nodeGraph == null)
			{
				return;
			}

			using (new ProfilerScope("GraphGUI"))
			{
				using (new ProfilerScope("BeginGraphGUI"))
				{
					if (ArborSettings.showGrid)
					{
						float zoomLevel = isCapture ? 1f : graphScale.x;
						EditorGUITools.DrawGrid(graphExtents, zoomLevel);
					}

#if !ARBOR_TRIAL
					if (!isCapture && ArborSettings.showLogo == LogoShowMode.AlwaysShow)
					{
						DrawLogo(graphViewport, graphScale.x);
					}
#endif

					_GraphEditor.BeginGraphGUI(isCapture || !zoomable);
				}

				using (new ProfilerScope("BeginWindows"))
				{
					_GUIIsExiting = false;

					BeginWindows();

					if (_GUIIsExiting)
					{
						GUIUtility.ExitGUI();
					}
				}

				using (new ProfilerScope("OnGraphGUI"))
				{
					_GraphEditor.OnGraphGUI();
				}

				using (new ProfilerScope("EndWindows"))
				{
					_GUIIsExiting = false;

					EndWindows();

					if (_GUIIsExiting)
					{
						GUIUtility.ExitGUI();
					}
				}

				using (new ProfilerScope("EndGraphGUI"))
				{
#if !ARBOR_EDITOR_USE_UIELEMENTS
					HandleDragObject();
#endif

					_GraphEditor.EndGraphGUI(isCapture || !zoomable);

#if !ARBOR_EDITOR_USE_UIELEMENTS
					DrawSelection();
#endif

					bool scrolled = false;
					if (DoAutoScroll())
					{
						scrolled = true;
					}

#if ARBOR_EDITOR_USE_UIELEMENTS
					if (Event.current.type == EventType.Repaint)
					{
						if (_PanManipulator.isActive)
						{
							EditorGUIUtility.AddCursorRect(graphViewport, MouseCursor.Pan);
						}
						else if( _ZoomManipulator.isActive)
						{
							EditorGUIUtility.AddCursorRect(graphViewport, MouseCursor.Zoom);
						}
					}
#else
					if (_GraphGUI.DragGrid())
					{
						StoreCurrentTransform();
						scrolled = true;
					}
#endif

					if (scrolled)
					{
						_FrameSelected.End();
						_FrameSelectedZoom.End();
					}
				}
			}
		}

		internal void BeginNode()
		{
			_IsInNodeEditor = true;

#if ARBOR_EDITOR_EXTENSIBLE
			EndSkin();
			BeginSkin();
#endif
		}

		internal void EndNode()
		{
#if ARBOR_EDITOR_EXTENSIBLE
			EndSkin();
			BeginSkin();
#endif

			_IsInNodeEditor = false;
		}

		void OnStateChanged(NodeGraph nodeGraph)
		{
			_IsRepaint = true;

			if (_GraphEditor != null && _GraphEditor.nodeGraph == nodeGraph)
			{
				_IsUpdateLiveTracking = true;
			}
		}

		private void FlipLocked()
		{
			_IsLocked = !_IsLocked;
		}

		public void AddItemsToMenu(GenericMenu menu)
		{
			menu.AddItem(EditorGUITools.GetTextContent("Lock"), _IsLocked, FlipLocked);
		}

		internal void SetScroll(Vector2 position, bool updateView, bool endFrameSelected)
		{
			if (!_IsLayoutSetup || isCapture)
			{
				return;
			}

			Rect extents = graphExtents;
			Rect viewport = graphViewport;
			position.x = Mathf.Clamp(position.x, extents.xMin, extents.xMax - viewport.width);
			position.y = Mathf.Clamp(position.y, extents.yMin, extents.yMax - viewport.height);

			this.scrollPos = position;

#if ARBOR_EDITOR_USE_UIELEMENTS
			if (updateView)
			{
				if (_GraphUI != null)
				{
					_GraphView.SetScrollOffset(position, false, false);
				}
			}

			endFrameSelected = endFrameSelected && _EndFrameSelected;
#endif

			if (endFrameSelected)
			{
				_FrameSelected.End();
				_FrameSelectedZoom.End();
			}

			StoreCurrentTransform();
		}

		internal void OnScroll(Vector2 delta)
		{
			SetScroll(scrollPos + delta, true, true);
		}

		internal void SetZoom(Vector2 zoomCenter, Vector3 zoomScale, bool endFrameSelected, bool updateScroll = true)
		{
#if ARBOR_EDITOR_USE_UIELEMENTS
			Vector3 position = _GraphUI.transform.position;
			Vector3 scale = _GraphUI.transform.scale;

			position += Vector3.Scale(zoomCenter, scale);
			zoomScale.x = Mathf.Clamp(zoomScale.x, 0.1f, 1f);
			zoomScale.y = Mathf.Clamp(zoomScale.y, 0.1f, 1f);

			_GraphUI.transform.position = position - Vector3.Scale(zoomCenter, zoomScale);
			_GraphUI.transform.scale = zoomScale;

			Vector2 scrollPos = this.scrollPos;

			_GraphView.UpdateLayout();

			if (updateScroll)
			{
				SetScroll(scrollPos, true, endFrameSelected);
			}
#endif
		}

		internal void OnZoom(Vector2 zoomCenter, float zoomScale)
		{
#if ARBOR_EDITOR_USE_UIELEMENTS
			Vector3 scale = _GraphUI.transform.scale;
			SetZoom(zoomCenter, Vector3.Scale(scale, new Vector3(zoomScale, zoomScale, 1f)), true);
#endif
		}

		private void CenterOnStoredPosition(GraphTreeViewItem graphItem)
		{
			if (!_IsLayoutSetup)
			{
				return;
			}

			if (graphItem != null && _TransformCache.HasTransform(graphItem.id))
			{
				int id = graphItem.id;
				Vector2 scrollPos = _TransformCache.GetPosition(id);
				Vector3 scale = _TransformCache.GetScale(id);

				SetZoom(Vector2.zero, scale, false);
				SetScroll(scrollPos, true, false);
			}
			else
			{
				SetZoom(Vector2.zero, Vector3.one, false);

				Vector2 center = graphExtents.center - graphViewRect.size * 0.5f;
				center.x = Mathf.Floor(center.x);
				center.y = Mathf.Floor(center.y);
				SetScroll(center, true, false);
			}
		}

		private void StoreCurrentTransform()
		{
			if (_SelectedGraphItem == null)
			{
				return;
			}

			if (!_IsLayoutSetup)
			{
				return;
			}

			int id = _SelectedGraphItem.id;
			_TransformCache.SetPosition(id, scrollPos);
			_TransformCache.SetScale(id, graphScale);
		}

		public Vector2 GraphToWindowPoint(Vector2 point)
		{
			if (isCapture)
			{
				return point;
			}
			return graphMatrix.MultiplyPoint(point);
		}

		public Rect GraphToWindowRect(Rect rect)
		{
			Matrix4x4 graphMatrix = this.graphMatrix;
			Vector2 min = graphMatrix.MultiplyPoint(rect.min);
			Vector2 max = graphMatrix.MultiplyPoint(rect.max);
			return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
		}

		public Vector2 WindowToGraphPoint(Vector2 point)
		{
			Matrix4x4 graphMatrix = this.graphMatrix.inverse;
			return graphMatrix.MultiplyPoint(point);
		}

		public Rect WindowToGraphRect(Rect rect)
		{
			Matrix4x4 graphMatrix = this.graphMatrix.inverse;
			Vector2 min = graphMatrix.MultiplyPoint(rect.min);
			Vector2 max = graphMatrix.MultiplyPoint(rect.max);
			return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
		}

		public Vector2 ClipToGraph(Vector2 absolutePos)
		{
#if ARBOR_EDITOR_USE_UIELEMENTS
			return GUIClip.Clip(absolutePos);
#else
			if (EditorWindowExtensions.HasBorderSize())
			{
				RectOffset borderSize = this.GetBorderSize();
				
				absolutePos -= graphViewport.position - graphViewportPosition.position - new Vector2(borderSize.left, borderSize.top);
				Vector2 pos = GUIClip.Clip(absolutePos);
				return pos;
			}
			else
			{
				absolutePos -= graphViewport.position - graphViewportPosition.position - position.position;
				Vector2 pos = GUIUtility.ScreenToGUIPoint(absolutePos);
				return pos;
			}
#endif
		}

		public Vector2 UnclipToGraph(Vector2 pos)
		{
#if ARBOR_EDITOR_USE_UIELEMENTS
			return GUIClip.Unclip(pos);
#else
			if (EditorWindowExtensions.HasBorderSize())
			{
				RectOffset borderSize = this.GetBorderSize();

				Vector2 absolutePos = GUIClip.Unclip(pos);
				absolutePos += graphViewport.position - graphViewportPosition.position - new Vector2(borderSize.left, borderSize.top);
				return absolutePos;
			}
			else
			{
				Vector2 absolutePos = GUIUtility.GUIToScreenPoint(pos);
				absolutePos += graphViewport.position - graphViewportPosition.position - position.position;
				return absolutePos;
			}
#endif
		}

#if ARBOR_EDITOR_USE_UIELEMENTS
		private static readonly string s_UnityVersion_2019_1_0_a5 = "2019.1.0a5";

		[System.NonSerialized]
		VisualElement _RootElement;

		[System.NonSerialized]
		bool _IsInitializedUpdateDocked = false;

		[System.NonSerialized]
		bool _IsUpdateDocked = false;

		[System.NonSerialized]
		bool _IsDocked = false;

		void UpdateDocked()
		{
			if (_IsInitializedUpdateDocked && !_IsUpdateDocked || _RootElement == null)
			{
				return;
			}

#if UNITY_2019_1_OR_NEWER
			float positionTop = (float)_RootElement.resolvedStyle.top;
#else
			float positionTop = (float)_RootElement.style.positionTop;
#endif

			if (!_IsInitializedUpdateDocked)
			{
				if (positionTop == 0f)
				{
					_IsUpdateDocked = true;
					return;
				}
			}

#if UNITY_2019_1_OR_NEWER
			float positionLeft = (float)_RootElement.resolvedStyle.left;
#else
			float positionLeft = (float)_RootElement.style.positionLeft;
#endif

			// Whether it is docked to the MainView.
			bool docked = positionLeft == 2f;

			if (!_IsInitializedUpdateDocked || _IsDocked != docked)
			{
				if (docked)
				{
#if UNITY_2019_1_OR_NEWER
					_MainLayout.style.top = 19f - positionTop;
#else
					_MainLayout.style.positionTop = 19f - positionTop;
#endif
				}
				else
				{
#if UNITY_2019_1_OR_NEWER
					_MainLayout.style.top = 20f - positionTop;
#else
					_MainLayout.style.positionTop = 23f - positionTop;
#endif
				}

				_IsDocked = docked;
				_IsInitializedUpdateDocked = true;
			}
		}

		static bool IsNanRect(Rect rect)
		{
			return float.IsNaN(rect.x) || float.IsNaN(rect.y) || float.IsNaN(rect.width) || float.IsNaN(rect.height);
		}

		void SetupGUI()
		{
#if UNITY_2019_1_OR_NEWER
			_RootElement = this.rootVisualElement;
#else
			_RootElement = this.GetRootVisualContainer();
#endif

			_MainLayout = new GraphMainLayout() {
				name = "MainLayout",
				style =
				{
					flexDirection = FlexDirection.Row,
#if UNITY_2019_1_OR_NEWER
					position = Position.Absolute,
					top = 0,
					bottom = 0,
					right = 0,
					left = 0,
#else
					positionType = PositionType.Absolute,
					positionTop = 0,
					positionBottom = 0,
					positionRight = 0,
					positionLeft = 0,
#endif
				}
			};

			_MainLayout.RegisterCallback<GeometryChangedEvent>(e =>
			{
				if (_MainLayoutInitialized)
				{
					return;
				}

				float leftPanelFlex = 1f;
				float rightPanelFlex = 3f;
				float totalFlex = leftPanelFlex + rightPanelFlex;

				Vector2 leftPanelMinSize = new Vector2(150f, 0f);
				Vector2 rightPanelMinSize = new Vector2(150f, 0f);

				float sidePanelWidth = ArborSettings.sidePanelWidth;
				leftPanelFlex = VisualSplitter.CalcFlex(new Vector2(sidePanelWidth, 0f), Vector2.zero, leftPanelMinSize, rightPanelMinSize, _MainLayout.layout.size, FlexDirection.Row, totalFlex);
				rightPanelFlex = totalFlex - leftPanelFlex;

				_LeftPanelLayout.style.flexGrow = leftPanelFlex;
				_LeftPanelLayout.style.minWidth = leftPanelMinSize.x;

				_RightPanelLayout.style.flexGrow = rightPanelFlex;
				_RightPanelLayout.style.minWidth = rightPanelMinSize.x;

				_MainLayoutInitialized = true;
			});

			_LeftPanelLayout = new GraphLayout() {
				name = "LeftPanel",
			};

			_LeftPanelLayout.RegisterCallback<GeometryChangedEvent>(e =>
			{
				if (_MainLayoutInitialized)
				{
					ArborSettings.sidePanelWidth = e.newRect.width;
				}
			});

			float toolbarHeight = EditorGUITools.toolbarHeight;
			if (Application.unityVersion == s_UnityVersion_2019_1_0_a5)
			{
				toolbarHeight = 20f;
			}

			_SideToolbarLayout = new GraphLayout()
			{
				name = "SideToolbar",
				style =
				{
					height = toolbarHeight,
				}
			};

			_SideToolbarUI = new StretchableIMGUIContainer(
				() => {
					Rect rect = _SideToolbarUI.layout;
					if (IsNanRect(rect))
					{
						return;
					}

					// _SideToolbarUI.layout is not updated correctly when the window size has been changed.
					rect.size = _SideToolbarLayout.layout.size;

					_SidePanel.ToolbarGUI(rect);
				}, StretchableIMGUIContainer.StretchMode.Flex)
			{
				name = "SideToolbarUI",
			};

			_SideToolbarLayout.Add(_SideToolbarUI);
			_LeftPanelLayout.Add(_SideToolbarLayout);

			_SidePanelLayout = new GraphLayout()
			{
				name = "SidePanelLayout",
				style =
				{
					flexGrow = 1f,
				}
			};

			_SidePanelUI = new StretchableIMGUIContainer(
				() => {
					Rect rect = _SidePanelUI.layout;
					if (IsNanRect(rect))
					{
						return;
					}

					// _SidePanelUI.layout is not updated correctly when the window size has been changed.
					rect.size = _SidePanelLayout.layout.size;

					OnDirtyTreeDelay();
					_SidePanel.OnGUI(rect);
				}, StretchableIMGUIContainer.StretchMode.Flex)
			{
				name = "SidePanelUI",
			};

			_SidePanelUI.RegisterCallback<FocusInEvent>((evt) => {
				_SidePanel.OnFocus(true);
			});
			_SidePanelUI.RegisterCallback<FocusOutEvent>((evt) => {
				_SidePanel.OnFocus(false);
			});

			_SidePanelLayout.Add(_SidePanelUI);

			_LeftPanelLayout.Add(_SidePanelLayout);

			if (ArborSettings.openSidePanel)
			{
				_MainLayout.Add(_LeftPanelLayout);
			}

			_RightPanelLayout = new GraphLayout() {
				name = "RightPanel",
			};

			GraphLayout toolbarLayout = new GraphLayout()
			{
				name = "Toolbar",
				style =
				{
					height = toolbarHeight,
				}
			};

			_ToolbarUI = new StretchableIMGUIContainer(
				() => {
					Rect rect = _ToolbarUI.layout;
					if (IsNanRect(rect))
					{
						return;
					}
					ToolbarGUI(rect);

				}, StretchableIMGUIContainer.StretchMode.Flex)
			{
				name = "ToolbarUI",
			};

			toolbarLayout.Add(_ToolbarUI);
			_RightPanelLayout.Add(toolbarLayout);

			_GraphPanel = new GraphLayout() {
				name = "GraphPanel",
				style =
				{
					flexGrow = 1f,
				}
			};

			_GraphView = new GraphView(this) {
				name = "GraphView",
			};

			_GraphUnderlayUI = new StretchableIMGUIContainer(
				() => {
					Rect rect = _GraphUnderlayUI.layout;
					if (IsNanRect(rect))
					{
						return;
					}
					UnderlayGUI(rect);
				}, StretchableIMGUIContainer.StretchMode.Absolute)
			{
				name = "GraphUnderlayUI",
			};

			_GraphView.contentContainer.Add(_GraphUnderlayUI);

			_GraphUI = new StretchableIMGUIContainer(
				() => {
					Rect rect = _GraphUI.layout;
					if (IsNanRect(rect))
					{
						return;
					}

					if (!isCapture)
					{
						if (Event.current.type == EventType.MouseMove)
						{
							if (_GraphEditor != null)
							{
								_GraphEditor.CloseAllPopupButtonControl();
							}
						}
						GraphViewGUI();
					}
				}, StretchableIMGUIContainer.StretchMode.Flex)
			{
				name = "GraphUI",
#if UNITY_2019_1_OR_NEWER
				//clippingOptions = VisualElement.ClippingOptions.NoClipping,
#else
				clippingOptions = VisualElement.ClippingOptions.NoClipping,
#endif
			};
			_GraphUI.style.overflow = Overflow.Visible;
#if UNITY_2021_2_OR_NEWER
			_GraphUI.style.transformOrigin = new TransformOrigin(0f, 0f, 0f);
#endif
			_GraphUI.RegisterCallback<DragEnterEvent>((e) =>
			{
				OnDragEnter();
			});
			_GraphUI.RegisterCallback<DragUpdatedEvent>((e) =>
			{
				OnDragUpdated();
			});
			_GraphUI.RegisterCallback<DragPerformEvent>((e) =>
			{
				OnDragPerform(e.localMousePosition);
			});
			_GraphUI.RegisterCallback<DragLeaveEvent>((e) =>
			{
				OnDragLeave();
			});
			_GraphUI.RegisterCallback<DragExitedEvent>((e) =>
			{
				OnDragExited();
			});
			_GraphView.contentContainer.Add(_GraphUI);

#if UNITY_2020_1_OR_NEWER
			_GetWorldBoundingBox = (System.Func<Rect>)System.Delegate.CreateDelegate(typeof(System.Func<Rect>), _GraphUI, s_GetWorldBoundingBoxMethod);
			_SetLastWorldClip = (System.Action<Rect>)System.Delegate.CreateDelegate(typeof(System.Action<Rect>), _GraphUI, s_SetLastWorldClipMethod);
			_GraphUI.generateVisualContent += (mgc) =>
			{
				_SetLastWorldClip(_GetWorldBoundingBox());
			};
#endif

#if UNITY_2019_2_OR_NEWER
			_GraphExtentsBoundingBoxElement = new VisualElement()
			{
				name = "GraphExtents",
				pickingMode = PickingMode.Ignore,
				style =
				{
					position = Position.Absolute,
					overflow = Overflow.Hidden,
					visibility = Visibility.Hidden,
				}
			};

			_GraphUI.hierarchy.Add(_GraphExtentsBoundingBoxElement);
#endif

			_GraphOverlayUI = new StretchableIMGUIContainer(
				() => {
					Rect rect = _GraphOverlayUI.layout;
					if (IsNanRect(rect))
					{
						return;
					}

					if (_GraphEditor != null)
					{
						_GraphEditor.UpdateOverlayLayer();
						_GraphEditor.BeginOverlayLayer();
						_GraphEditor.EndOverlayLayer();
					}
					DrawSelection();
					OverlayGUI(rect);
				}, StretchableIMGUIContainer.StretchMode.Absolute)
			{
				name = "GraphOverlayUI",
			};

			_GraphOverlayUI.containsPointCallback += (localPoint) =>
			{
				if (_GraphEditor != null)
				{
					return _GraphEditor.ContainsOverlayLayer(localPoint);
				}

				return true;
			};

			_GraphView.contentContainer.Add(_GraphOverlayUI);

			_ZoomManipulator = new ZoomManipulator(_GraphUI, this);
			_GraphView.contentContainer.AddManipulator(_ZoomManipulator);

			_PanManipulator = new PanManipulator(_GraphUI, this);
			_GraphView.contentContainer.AddManipulator(_PanManipulator);

			_NoGraphUI = new StretchableIMGUIContainer(
				() =>
				{
					Rect rect = _NoGraphUI.layout;
					if (IsNanRect(rect))
					{
						return;
					}
					NoGraphSelectedGUI(rect);
				}, StretchableIMGUIContainer.StretchMode.Flex);

			if (_GraphEditor != null)
			{
				_GraphPanel.Add(_GraphView);
			}
			else
			{
				_GraphPanel.Add(_NoGraphUI);
			}

			_RightPanelLayout.Add(_GraphPanel);

			float graphBottomHeight = EditorGUITools.toolbarHeight;
			if (Application.unityVersion == s_UnityVersion_2019_1_0_a5)
			{
				graphBottomHeight = 17f;
			}

			GraphLayout graphBottomLayout = new GraphLayout() {
				name = "GraphBottomBar",
				style =
				{
					height = graphBottomHeight,
				}
			};

			_GraphBottomUI = new StretchableIMGUIContainer(
				() => {
					Rect rect = _GraphBottomUI.layout;
					if (IsNanRect(rect))
					{
						return;
					}
					BreadcrumbGUI(rect);
				}, StretchableIMGUIContainer.StretchMode.Flex)
			{
				name = "GraphBottomUI",
			};

			graphBottomLayout.Add(_GraphBottomUI);

			_RightPanelLayout.Add(graphBottomLayout);

			_MainLayout.Add(_RightPanelLayout);

			_RootElement.Add(_MainLayout);

			DirtyGraphExtents();

			UpdateDocked();
		}
#else
		void CalculateRect()
		{
			_ToolBarRect = new Rect(0.0f, 0.0f, this.position.width, EditorStyles.toolbar.fixedHeight);

			if (ArborSettings.openSidePanel)
			{
				_SideToolbarRect = new Rect(0.0f, 0.0f, ArborSettings.sidePanelWidth, EditorStyles.toolbar.fixedHeight);
				_SidePanelRect = new Rect(0.0f, _SideToolbarRect.yMax, ArborSettings.sidePanelWidth, this.position.height - _SideToolbarRect.height);

				_ToolBarRect.xMin = _SidePanelRect.xMax;

				float graphWidth = this.position.width - _SidePanelRect.width;
				if (graphWidth < k_MinRightSideWidth)
				{
					graphWidth = k_MinRightSideWidth;
				}

				_BreadcrumbRect = new Rect(_SidePanelRect.xMax, this.position.height - EditorStyles.toolbar.fixedHeight, graphWidth, EditorStyles.toolbar.fixedHeight);

				float graphHeight = this.position.height - (_ToolBarRect.height + _BreadcrumbRect.height);
				_GraphRect = new Rect(_SidePanelRect.xMax, _ToolBarRect.yMax, graphWidth, graphHeight);
			}
			else
			{
				float graphWidth = this.position.width;

				_BreadcrumbRect = new Rect(0.0f, this.position.height - EditorStyles.toolbar.fixedHeight, graphWidth, EditorStyles.toolbar.fixedHeight);

				float graphHeight = this.position.height - (_ToolBarRect.height + _BreadcrumbRect.height);
				_GraphRect = new Rect(0.0f, _ToolBarRect.yMax, graphWidth, graphHeight);
			}
		}

		private static readonly int s_MouseDeltaReaderHash = "s_MouseDeltaReaderHash".GetHashCode();
		static Vector2 s_MouseDeltaReaderLastPos;

		static readonly float k_MinLeftSideWidth = 150f;
		static readonly float k_MinRightSideWidth = 110f;

		static Vector2 MouseDeltaReader(Rect position, bool activated)
		{
			int controlId = GUIUtility.GetControlID(s_MouseDeltaReaderHash, FocusType.Passive, position);
			Event current = Event.current;
			switch (current.GetTypeForControl(controlId))
			{
				case EventType.MouseDown:
					if (activated && GUIUtility.hotControl == 0 && (position.Contains(current.mousePosition) && current.button == 0))
					{
						GUIUtility.hotControl = controlId;
						GUIUtility.keyboardControl = 0;

						s_MouseDeltaReaderLastPos = GUIUtility.GUIToScreenPoint(current.mousePosition);

						current.Use();
						break;
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlId && current.button == 0)
					{
						GUIUtility.hotControl = 0;
						current.Use();
						break;
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlId)
					{
						Vector2 vector2_1 = GUIUtility.GUIToScreenPoint(current.mousePosition);
						Vector2 vector2_2 = vector2_1 - s_MouseDeltaReaderLastPos;
						s_MouseDeltaReaderLastPos = vector2_1;
						current.Use();
						return vector2_2;
					}
					break;
			}
			return Vector2.zero;
		}

		void ResizeHandling(float width, float height)
		{
			if (!ArborSettings.openSidePanel)
			{
				return;
			}

			Rect dragRect = new Rect(ArborSettings.sidePanelWidth, 0.0f, 5.0f, height);
			float minLeftSide = k_MinLeftSideWidth;
			float minRightSide = k_MinRightSideWidth;

			if (Event.current.type == EventType.Repaint)
			{
				EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.SplitResizeLeftRight);
			}
			float num = 0.0f;
			float x = MouseDeltaReader(dragRect, true).x;
			if (x != 0.0f)
			{
				dragRect.x += x;
			}

			if (dragRect.x < minLeftSide || minLeftSide > width - minRightSide)
			{
				num = minLeftSide;
			}
			else if (dragRect.x > width - minRightSide)
			{
				num = width - minRightSide;
			}
			else
			{
				num = dragRect.x;
			}

			if (num > 0.0)
			{
				ArborSettings.sidePanelWidth = num;
			}
		}
#endif
	}
}