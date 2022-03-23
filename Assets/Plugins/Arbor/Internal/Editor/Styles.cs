//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ArborEditor
{
	public static class Styles
	{
		public static readonly GUIContent addIconContent;
		public static readonly GUIContent removeIconContent;
		public static readonly Texture2D connectionTexture;
		public static readonly Texture2D selectedConnectionTexture;
		public static readonly Texture2D outlineConnectionTexture;
		public static readonly Texture2D dataConnectionTexture;
		public static readonly Texture2D visibilityToggleOnTexture;
		public static readonly Texture2D visibilityToggleOffTexture;

		public static readonly GUIStyle graphBackground;
		public static readonly GUIStyle graphHighlight;
		public static readonly GUIStyle overlayBox;
		public static readonly GUIStyle addBehaviourButton;
		public static readonly GUIStyle insertion;
		public static readonly GUIStyle insertionAbove;
		public static readonly GUIStyle breakpoint;
		public static readonly GUIStyle breakpointOn;
		public static readonly GUIStyle nodeCommentField;
		public static readonly GUIStyle sidePanelTitlebar;
		public static readonly GUIStyle dropField;
		public static readonly GUIStyle nodeLinkSlot;
		public static readonly GUIStyle nodeLinkSlotActive;
		public static readonly GUIStyle nodeLinkPin;
		public static readonly GUIStyle nodeElementSelect;
		public static readonly GUIStyle nodeDataOutPin;
		public static readonly GUIStyle nodeDataInPin;
		public static readonly GUIStyle nodeDataArrayOutPin;
		public static readonly GUIStyle nodeDataArrayInPin;
		public static readonly GUIStyle nodeWindow;
		public static readonly GUIStyle graphLabel;
		public static readonly GUIStyle playStateLabel;
		public static readonly GUIStyle reroutePin;
		public static readonly GUIStyle reroutePinFrame;
		public static readonly GUIStyle dataArrayReroutePin;
		public static readonly GUIStyle iconButton;
		public static readonly GUIStyle popupIconButton;
		public static readonly GUIStyle visibilityToggle;
		public static readonly GUIStyle dataLinkSlot;
		public static readonly GUIStyle dataLinkSlotActive;
		public static readonly GUIStyle stateLinkHeaderLight;
		public static readonly GUIStyle stateLinkHeaderDark;
		public static readonly GUIStyle stateLinkRightPin;
		public static readonly GUIStyle stateLinkLeftPin;
		public static readonly GUIStyle stateLinkMargin;
		public static readonly GUIStyle circleButton;
		public static readonly GUIStyle whiteBorder;
		public static readonly GUIStyle whiteBorderBold;
		public static readonly GUIStyle whiteCross;
		public static readonly GUIStyle nodeHeaderTitle;

		public static GUIStyle stateLinkHeader
		{
			get
			{
				return ArborEditorWindow.isDarkSkin ? stateLinkHeaderDark : stateLinkHeaderLight;
			}
		}

		private static GUIStyle _InvisibleButton;
		private static GUIStyle _RLHeader;
		private static GUIStyle _RLBackground;
		private static GUIStyle _Separator;
		private static GUIStyle _Background;
		private static GUIStyle _Header;
		private static GUIStyle _ToolbarSearchField;
		private static GUIStyle _ToolbarSearchFieldRaw;
		private static GUIStyle _ComponentButton;
		private static GUIStyle _GroupButton;
		private static GUIStyle _PreviewBackground;
		private static GUIStyle _PreviewHeader;
		private static GUIStyle _PreviewText;
		private static GUIStyle _RightArrow;
		private static GUIStyle _LeftArrow;
		private static GUIStyle _ShurikenDropDown;
		private static GUIStyle _Titlebar;
		private static GUIStyle _TitlebarText;
		private static GUIStyle _Foldout;
		private static GUIStyle _Hostview;
		private static GUIStyle _TabWindowBackground;
		private static GUIStyle _TreeBehaviourBackground;
		private static GUIStyle _SelectionRect;
		private static GUIStyle _VarPinIn;
		private static GUIStyle _VarPinOut;
		private static GUIStyle _PreSlider;
		private static GUIStyle _PreSliderThumb;
		private static GUIStyle _TreeLineStyle;
		private static GUIStyle _TreeSelectionStyle;
		private static GUIStyle _DisabledTreeStyle;
		private static GUIStyle _PrefabTreeStyle;
		private static GUIStyle _DisabledPrefabTreeStyle;
		private static GUIStyle _LargeButton;
		private static GUIStyle _LockButton;
		private static GUIStyle _CountBadge;
		private static GUIStyle _CountBadgeLarge;
		private static GUIStyle _ColorPickerBox;
		private static GUIStyle _BreadcrumbLeft;
		private static GUIStyle _BreadcrumbLeftPrefab;
		private static GUIStyle _BreadcrumbLeftBg;
		private static GUIStyle _BreadcrumbMid;
		private static GUIStyle _BreadcrumbMidPrefab;
		private static GUIStyle _BreadcrumbMidBg;
		private static GUIStyle _WordWrappedTextArea;
		private static GUIStyle _LargeButtonLeft;
		private static GUIStyle _LargeButtonRight;
		private static GUIStyle _StateLinkSlot;
		private static GUIStyle _StateLinkPin;
		private static GUIStyle _NodeHeaderButton;
		private static GUIStyle _NodeHeaderButtonLeft;
		private static GUIStyle _NodeHeaderButtonMid;
		private static GUIStyle _NodeHeaderButtonRight;
		private static GUIStyle _RenameTextField;
		private static GUIStyle _GroupFoldout;
		private static GUIStyle _ParameterListHeader;
		private static GUIStyle _EntryBackEven;
		private static GUIStyle _EntryBackOdd;
		private static GUIStyle _EntryBox;
		private static GUIStyle _PopupWindowToolbar;
		private static GUIStyle _Toolbar;
		private static GUIStyle _LargeBoldLabel;
		private static GUIStyle _Highlight;
		private static GUIStyle _PreToolbar;
		private static GUIStyle _PreToolbar2;
		private static GUIStyle _PreToolbarLabel;
		private static GUIStyle _DragHandle;
		private static GUIStyle _LargeDropDown;

		public static GUIStyle invisibleButton
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _InvisibleButton;
			}
		}

		public static GUIStyle RLHeader
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _RLHeader;
			}
		}

		public static GUIStyle RLBackground
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _RLBackground;
			}
		}

		public static GUIStyle separator
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _Separator;
			}
		}

		public static GUIStyle background
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _Background;
			}
		}

		public static GUIStyle header
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _Header;
			}
		}

		public static GUIStyle toolbarSearchFieldRaw
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _ToolbarSearchFieldRaw;
			}
		}

		public static GUIStyle toolbarSearchField
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _ToolbarSearchField;
			}
		}

		public static GUIStyle componentButton
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _ComponentButton;
			}
		}

		public static GUIStyle groupButton
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _GroupButton;
			}
		}

		public static GUIStyle previewBackground
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _PreviewBackground;
			}
		}

		public static GUIStyle previewHeader
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _PreviewHeader;
			}
		}

		public static GUIStyle previewText
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _PreviewText;
			}
		}

		public static GUIStyle rightArrow
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _RightArrow;
			}
		}

		public static GUIStyle leftArrow
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _LeftArrow;
			}
		}

		public static GUIStyle shurikenDropDown
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _ShurikenDropDown;
			}
		}

		public static GUIStyle titlebar
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _Titlebar;
			}
		}

		public static GUIStyle titlebarText
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _TitlebarText;
			}
		}

		public static GUIStyle foldout
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _Foldout;
			}
		}

		public static GUIStyle hostview
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _Hostview;
			}
		}

		public static GUIStyle tabWindowBackground
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _TabWindowBackground;
			}
		}

		public static GUIStyle treeBehaviourBackground
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _TreeBehaviourBackground;
			}
		}

		public static GUIStyle selectionRect
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _SelectionRect;
			}
		}

		public static GUIStyle varPinIn
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _VarPinIn;
			}
		}

		public static GUIStyle varPinOut
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _VarPinOut;
			}
		}

		public static GUIStyle preSlider
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _PreSlider;
			}
		}

		public static GUIStyle preSliderThumb
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _PreSliderThumb;
			}
		}

		public static GUIStyle treeLineStyle
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _TreeLineStyle;
			}
		}

		public static GUIStyle treeSelectionStyle
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _TreeSelectionStyle;
			}
		}

		public static GUIStyle disabledTreeStyle
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _DisabledTreeStyle;
			}
		}

		public static GUIStyle prefabTreeStyle
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _PrefabTreeStyle;
			}
		}

		public static GUIStyle disabledPrefabTreeStyle
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _DisabledPrefabTreeStyle;
			}
		}

		public static GUIStyle largeButton
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _LargeButton;
			}
		}

		public static GUIStyle lockButton
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _LockButton;
			}
		}

		public static GUIStyle countBadge
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _CountBadge;
			}
		}

		public static GUIStyle countBadgeLarge
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _CountBadgeLarge;
			}
		}

		public static GUIStyle colorPickerBox
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _ColorPickerBox;
			}
		}

		public static GUIStyle breadcrumbLeft
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _BreadcrumbLeft;
			}
		}

		public static GUIStyle breadcrumbLeftPrefab
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _BreadcrumbLeftPrefab;
			}
		}

		public static GUIStyle breadcrumbLeftBg
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _BreadcrumbLeftBg;
			}
		}

		public static GUIStyle breadcrumbMid
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _BreadcrumbMid;
			}
		}

		public static GUIStyle breadcrumbMidPrefab
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _BreadcrumbMidPrefab;
			}
		}

		public static GUIStyle breadcrumbMidBg
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _BreadcrumbMidBg;
			}
		}

		public static GUIStyle wordWrappedTextArea
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _WordWrappedTextArea;
			}
		}

		public static GUIStyle largeButtonLeft
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _LargeButtonLeft;
			}
		}

		public static GUIStyle largeButtonRight
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _LargeButtonRight;
			}
		}

		public static GUIStyle stateLinkSlot
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _StateLinkSlot;
			}
		}

		public static GUIStyle stateLinkPin
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _StateLinkPin;
			}
		}

		public static GUIStyle nodeHeaderButton
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _NodeHeaderButton;
			}
		}

		public static GUIStyle nodeHeaderButtonLeft
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _NodeHeaderButtonLeft;
			}
		}

		public static GUIStyle nodeHeaderButtonMid
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _NodeHeaderButtonMid;
			}
		}

		public static GUIStyle nodeHeaderButtonRight
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _NodeHeaderButtonRight;
			}
		}

		public static GUIStyle renameTextField
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _RenameTextField;
			}
		}

		public static GUIStyle groupFoldout
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _GroupFoldout;
			}
		}

		public static GUIStyle parameterListHeader
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _ParameterListHeader;
			}
		}

		public static GUIStyle entryBackEven
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _EntryBackEven;
			}
		}

		public static GUIStyle entryBackOdd
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _EntryBackOdd;
			}
		}

		public static GUIStyle entryBox
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _EntryBox;
			}
		}

		public static GUIStyle popupWindowToolbar
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _PopupWindowToolbar;
			}
		}

		public static GUIStyle toolbar
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _Toolbar;
			}
		}

		public static GUIStyle largeBoldLabel
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _LargeBoldLabel;
			}
		}

		public static GUIStyle highlight
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _Highlight;
			}
		}

		public static GUIStyle preToolbar
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _PreToolbar;
			}
		}

		public static GUIStyle preToolbar2
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _PreToolbar2;
			}
		}

		public static GUIStyle preToolbarLabel
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _PreToolbarLabel;
			}
		}

		public static GUIStyle dragHandle
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _DragHandle;
			}
		}

		public static GUIStyle largeDropDown
		{
			get
			{
				UpdateBuiltInGUIStyles();
				return _LargeDropDown;
			}
		}

		public const float kInsertionHeight = 16f;

		private static GUISkin _Skin = null;
		private static readonly Dictionary<string, GUIStyle> _StyleCache;

		private static GUISkin _LastBuiltinGUISkin = null;

		private static GUISkin skin
		{
			get
			{
				if (_Skin == null)
				{
					_Skin = EditorResources.Load<GUISkin>("EditorSkin", ".guiskin");
				}
				return _Skin;
			}
		}

		static RectOffset CopyOffset(RectOffset offset)
		{
			return new RectOffset(offset.left, offset.right, offset.top, offset.bottom);
		}

		static Styles()
		{
			_StyleCache = new Dictionary<string, GUIStyle>();

			addIconContent = new GUIContent(EditorGUIUtility.FindTexture("Toolbar Plus"));
			removeIconContent = new GUIContent(EditorGUIUtility.FindTexture("Toolbar Minus"));
			connectionTexture = UnityEditor.Graphs.Styles.connectionTexture.image as Texture2D;
			selectedConnectionTexture = EditorResources.LoadTexture("Textures/selected connection line");
			outlineConnectionTexture = EditorResources.LoadTexture("Textures/outline connection line");
			dataConnectionTexture = EditorResources.LoadTexture("Textures/data connection line");

			graphBackground = GetStyle("graph background");
			graphHighlight = GetStyle("graph highlight");
			overlayBox = GetStyle("pre drop glow");
			addBehaviourButton = GetStyle("add behaviour button");
			insertion = GetStyle("drop insertion");
			insertionAbove = GetStyle("drop insertion above");
			breakpoint = GetStyle("breakpoint");
			breakpointOn = GetStyle("breakpoint on");
			nodeCommentField = GetStyle("node comment");
			sidePanelTitlebar = GetStyle("panel header");
			dropField = GetStyle("drop field");
			nodeLinkSlot = GetStyle("node link slot");
			nodeLinkSlotActive = GetStyle("node link slot active");
			nodeLinkPin = GetStyle("node link pin");
			nodeElementSelect = GetStyle("node element select");
			nodeDataOutPin = GetStyle("node data out pin");
			nodeDataInPin = GetStyle("node data in pin");
			nodeDataArrayOutPin = GetStyle("node data array out pin");
			nodeDataArrayInPin = GetStyle("node data array in pin");
			nodeWindow = GetStyle("node window");
			graphLabel = GetStyle("graph label");
			playStateLabel = GetStyle("play state label");
			reroutePin = GetStyle("reroute pin");
			reroutePinFrame = GetStyle("reroute pin frame");
			dataArrayReroutePin = GetStyle("data array reroute pin");
			dataLinkSlot = GetStyle("data link slot");
			dataLinkSlotActive = GetStyle("data link slot active");
			stateLinkHeaderLight = GetStyle("state link header@Light");
			stateLinkHeaderDark = GetStyle("state link header@Dark");
			stateLinkRightPin = GetStyle("state link right pin");
			stateLinkLeftPin = GetStyle("state link left pin");

			stateLinkMargin = new GUIStyle(GUIStyle.none);
			stateLinkMargin.margin = new RectOffset(4, 4, 0, 0);

			circleButton = GetStyle("circle button");

			whiteBorder = GetStyle("white border");
			whiteBorderBold = GetStyle("white border bold");
			whiteCross = GetStyle("white cross");

			nodeHeaderTitle = GetStyle("node header title");

			// In Unity 2019.1.0a5 the built-in IconButton and the same name style do not reflect GUI.contentColor.
			//iconButton = GUI.skin.FindStyle("IconButton") ?? GetStyle("icon button");
			iconButton = GetStyle("icon button");

			popupIconButton = new GUIStyle(GUIStyle.none);
			popupIconButton.alignment = TextAnchor.MiddleCenter;
			popupIconButton.fixedHeight = 16f;
			popupIconButton.fixedWidth = 16f;

			visibilityToggleOffTexture = EditorGUIUtility.FindTexture("animationvisibilitytoggleoff");
			visibilityToggleOnTexture = EditorGUIUtility.FindTexture("animationvisibilitytoggleon");

			visibilityToggle = new GUIStyle();
			visibilityToggle.normal.background = visibilityToggleOffTexture;
			visibilityToggle.onNormal.background = visibilityToggleOnTexture;

			visibilityToggle.fixedHeight = visibilityToggleOffTexture.height;
			visibilityToggle.fixedWidth = visibilityToggleOffTexture.width;
			visibilityToggle.border = new RectOffset(2, 2, 2, 2);
			visibilityToggle.padding = new RectOffset(3, 3, 3, 3);
			visibilityToggle.overflow = new RectOffset(-1, 1, -2, 2);

			SetupBuiltinGUIStyles();
		}

		static void CopyBreadcrumbTextColor(GUIStyle dest, GUIStyle sorce)
		{
			dest.normal.textColor = sorce.normal.textColor;
			dest.hover.textColor = sorce.hover.textColor;
			dest.active.textColor = sorce.active.textColor;
			dest.focused.textColor = sorce.focused.textColor;

			// ignore on color
			dest.onNormal.textColor = sorce.normal.textColor;
			dest.onHover.textColor = sorce.hover.textColor;
			dest.onActive.textColor = sorce.active.textColor;
			dest.onFocused.textColor = sorce.focused.textColor;
		}

		static void SetupBuiltinGUIStyles()
		{
			_LastBuiltinGUISkin = GUI.skin;

			_InvisibleButton = (GUIStyle)"InvisibleButton";
			_RLHeader = (GUIStyle)"RL Header";
			_Separator = (GUIStyle)"sv_iconselector_sep";
			_Background = (GUIStyle)"grey_border";
			_PreviewBackground = (GUIStyle)"PopupCurveSwatchBackground";
			_RightArrow = (GUIStyle)"ArrowNavigationRight";
			_LeftArrow = (GUIStyle)"ArrowNavigationLeft";
			_ShurikenDropDown = (GUIStyle)"ShurikenDropDown";
			_Titlebar = new GUIStyle((GUIStyle)"IN Title");
			_Titlebar.margin.left += 1;
			_Titlebar.margin.right += 1;
			_TitlebarText = (GUIStyle)"IN TitleText";
			_Foldout = (GUIStyle)"Foldout";
			_LockButton = (GUIStyle)"IN LockButton";
			_CountBadge = (GUIStyle)"CN CountBadge";
			_ColorPickerBox = (GUIStyle)"ColorPickerBox";
			_BreadcrumbLeft = (GUIStyle)"GUIEditor.BreadcrumbLeft";
			_BreadcrumbLeftBg = GUI.skin.FindStyle("GUIEditor.BreadcrumbLeftBackground"); // Unity 2019 or later
			_BreadcrumbMid = (GUIStyle)"GUIEditor.BreadcrumbMid";
			_BreadcrumbMidBg = GUI.skin.FindStyle("GUIEditor.BreadcrumbMidBackground"); // Unity 2019 or later
			_SelectionRect = (GUIStyle)"SelectionRect";
			_VarPinIn = (GUIStyle)"flow varPin in";
			_VarPinOut = (GUIStyle)"flow varPin out";
			_PreSlider = (GUIStyle)"preSlider";
			_PreSliderThumb = (GUIStyle)"preSliderThumb";
			_NodeHeaderButtonLeft = (GUIStyle)"ButtonLeft";
			_NodeHeaderButtonMid = (GUIStyle)"ButtonMid";
			_NodeHeaderButtonRight = (GUIStyle)"ButtonRight";
			_RenameTextField = (GUIStyle)"PR TextField";
			_GroupFoldout = (GUIStyle)"IN Foldout";
			_EntryBox = new GUIStyle((GUIStyle)"CN Box");
			_EntryBox.padding = new RectOffset(-1, 0, 0, 1);
			_EntryBox.overflow = new RectOffset(1, 1, 0, 0);
			_EntryBackEven = new GUIStyle((GUIStyle)"CN EntryBackEven");
			_EntryBackEven.margin = new RectOffset();
			_EntryBackOdd = new GUIStyle((GUIStyle)"CN EntryBackOdd");
			_EntryBackOdd.margin = new RectOffset();

			_RLBackground = new GUIStyle((GUIStyle)"RL Background")
			{
				stretchHeight = false,
			};

			_Header = new GUIStyle((GUIStyle)"DD HeaderStyle")
			{
				font = EditorStyles.boldLabel.font,
			};

			GUIStyle toolbarSearchField = (GUIStyle)"ToolbarSeachTextField";
			_ToolbarSearchFieldRaw = toolbarSearchField;
			if (toolbarSearchField != null && toolbarSearchField != GUIStyle.none)
			{
				_ToolbarSearchField = new GUIStyle(toolbarSearchField)
				{
					margin = new RectOffset(5, 4, 4, 5)
				};
			}

			_PreviewHeader = new GUIStyle(EditorStyles.label);
			_PreviewHeader.padding.left += 1;
			_PreviewHeader.padding.right += 3;
			_PreviewHeader.padding.top += 3;
			_PreviewHeader.padding.bottom += 2;

			_PreviewText = new GUIStyle(EditorStyles.wordWrappedLabel);
			_PreviewText.padding.left += 3;
			_PreviewText.padding.right += 3;

			_Hostview = new GUIStyle((GUIStyle)"hostview")
			{
				stretchHeight = false,
				padding = new RectOffset(),
			};

			_TabWindowBackground = ((GUIStyle)"TabWindowBackground");

			_TreeBehaviourBackground = new GUIStyle((GUIStyle)"sv_iconselector_back")
			{
				stretchHeight = false,
				padding = new RectOffset(0, 0, 0, (int)EditorGUIUtility.standardVerticalSpacing),
				margin = new RectOffset(2, 2, 2, 2),
			};

			_ComponentButton = new GUIStyle((GUIStyle)"PR Label")
			{
				alignment = TextAnchor.MiddleLeft,
				fixedHeight = 20f,
			};
			_ComponentButton.padding.left -= 15;

			_GroupButton = new GUIStyle(_ComponentButton);
			_GroupButton.padding.left += 17;

			_CountBadgeLarge = new GUIStyle(_CountBadge)
			{
				fixedHeight = 0f,
			};

			_WordWrappedTextArea = new GUIStyle(EditorStyles.textArea)
			{
				wordWrap = true,
			};

			_TreeLineStyle = new GUIStyle((GUIStyle)"TV Line");
			_TreeLineStyle.padding.left = 2;

			_TreeSelectionStyle = new GUIStyle((GUIStyle)"TV Selection");
			_TreeSelectionStyle.padding.left = 2;

			_DisabledTreeStyle = new GUIStyle((GUIStyle)"PR DisabledLabel");
			_DisabledTreeStyle.padding.left = 2;

			_PrefabTreeStyle = new GUIStyle((GUIStyle)"PR PrefabLabel");
			_PrefabTreeStyle.padding.left = 2;

			_DisabledPrefabTreeStyle = new GUIStyle((GUIStyle)"PR DisabledPrefabLabel");
			_DisabledPrefabTreeStyle.padding.left = 2;

			_BreadcrumbLeftPrefab = new GUIStyle(_BreadcrumbLeft);
			CopyBreadcrumbTextColor(_BreadcrumbLeftPrefab, _PrefabTreeStyle);

			_BreadcrumbMidPrefab = new GUIStyle(_BreadcrumbMid);
			CopyBreadcrumbTextColor(_BreadcrumbMidPrefab, _PrefabTreeStyle);

			// Create LargeButton based on Button
			// (LargeButton became fixed height at Unity 2019.1.0a5)
			GUIStyle largeButton_ = (GUIStyle)"LargeButton";
			GUIStyle largeButtonLeft_ = (GUIStyle)"LargeButtonLeft";
			GUIStyle largeButtonRight_ = (GUIStyle)"LargeButtonRight";

			_LargeButton = new GUIStyle("Button")
			{
				font = largeButton_.font,
				fontSize = largeButton_.fontSize,
				padding = CopyOffset(largeButton_.padding),
				margin = CopyOffset(largeButton_.margin),
			};

			_LargeButtonLeft = new GUIStyle("ButtonLeft")
			{
				font = largeButtonLeft_.font,
				fontSize = largeButtonLeft_.fontSize,
				padding = CopyOffset(largeButtonLeft_.padding),
				margin = CopyOffset(largeButtonLeft_.margin),
			};

			_LargeButtonRight = new GUIStyle("ButtonRight")
			{
				font = largeButtonRight_.font,
				fontSize = largeButtonRight_.fontSize,
				padding = CopyOffset(largeButtonRight_.padding),
				margin = CopyOffset(largeButtonRight_.margin),
			};

			GUIStyle miniButton_ = EditorStyles.miniButton;
			_StateLinkSlot = new GUIStyle(GUI.skin.button)
			{
				margin = new RectOffset(),
				border = CopyOffset(miniButton_.border),
				padding = CopyOffset(miniButton_.padding),
				font = miniButton_.font,
				fontSize = miniButton_.fontSize,
			};

			_StateLinkPin = new GUIStyle(EditorStyles.radioButton)
			{
				fixedHeight = 12f,
			};
			_StateLinkPin.margin.top = 0;
			_StateLinkPin.margin.bottom = 0;
			_StateLinkPin.padding.top = 0;
			_StateLinkPin.padding.bottom = 0;
			_StateLinkPin.overflow.top = 0;
			_StateLinkPin.overflow.bottom = 3;

			_NodeHeaderButton = new GUIStyle(GUI.skin.button)
			{
				padding = _NodeHeaderButtonLeft.padding,
			};

			_ParameterListHeader = new GUIStyle((GUIStyle)"RL Header");
			_ParameterListHeader.fixedHeight = 0f;

			_PopupWindowToolbar = new GUIStyle(EditorStyles.toolbar);
			_PopupWindowToolbar.margin = new RectOffset(1, 1, 1, 0);

			_Toolbar = new GUIStyle(EditorStyles.toolbar);
			_Toolbar.padding.left = 6;
			_Toolbar.padding.right = 6;

			_LargeBoldLabel = new GUIStyle(EditorStyles.largeLabel);
			_LargeBoldLabel.fontStyle = FontStyle.Bold;

			_Highlight = (GUIStyle)"LightmapEditorSelectedHighlight";
			_PreToolbar = (GUIStyle)"preToolbar";
			_PreToolbar2 = (GUIStyle)"preToolbar2";
			_PreToolbarLabel = GUI.skin.FindStyle("ToolbarBoldLabel"); // Unity2019 or later
			if (_PreToolbarLabel == null)
			{
				_PreToolbarLabel = _PreToolbar2;
			}
			_DragHandle = (GUIStyle)"RL DragHandle";

			_LargeDropDown = new GUIStyle((GUIStyle)"DropDownButton");
			_LargeDropDown.padding.left = Mathf.Max(_LargeDropDown.padding.left, _LargeDropDown.border.left + 2);
			_LargeDropDown.padding.right = Mathf.Max(_LargeDropDown.padding.right, _LargeDropDown.border.right + 1);
			_LargeDropDown.fixedHeight = Mathf.Max(_LargeDropDown.fixedHeight, 24);
			_LargeDropDown.fontSize = Mathf.Max(_LargeDropDown.fontSize, 12);
			_LargeDropDown.alignment = TextAnchor.MiddleCenter;
		}

		static void UpdateBuiltInGUIStyles()
		{
			if (_LastBuiltinGUISkin != GUI.skin)
			{
#if ARBOR_DEBUG
				Debug.Log("SkinChanged");
#endif
				SetupBuiltinGUIStyles();
			}
		}

		public static GUIStyle GetStyle(string name)
		{
			GUIStyle style = null;
			if (!_StyleCache.TryGetValue(name, out style))
			{
				style = skin.FindStyle(name);
				if (style != null)
				{
					_StyleCache.Add(name, style);
				}
			}
			return style;
		}

		public static GUIStyle GetNodeFrameStyle(bool on, bool active)
		{
			string name = string.Format("node frame{0}{1}", (!on) ? string.Empty : " on", (!active) ? string.Empty : " active");
			return GetStyle(name);
		}

		public static GUIStyle GetNodeBaseStyle(Styles.BaseColor color)
		{
			string name = string.Format("node base {0}", (int)color);
			return GetStyle(name);
		}

		public static GUIStyle GetNodeHeaderStyle(Styles.Color color)
		{
			string name = string.Format("node header {0}", (int)color);
			return GetStyle(name);
		}

		public enum BaseColor
		{
			Gray = 0,
			Grey = 0,
			White = 1,
		}

		public enum Color
		{
			Gray = 0,
			Grey = 0,
			Blue = 1,
			Aqua = 2,
			Green = 3,
			Yellow = 4,
			Orange = 5,
			Red = 6,
			Purple = 7,
			White = 8,
		}

		private static readonly Dictionary<Styles.Color, UnityEngine.Color> s_ColorDictionary = new Dictionary<Color, UnityEngine.Color>()
		{
			{ Styles.Color.Gray,new UnityEngine.Color(0.8f, 0.8f, 0.8f) },
			{ Styles.Color.Blue,new UnityEngine.Color(0.2f, 0.7f, 1.0f) },
			{ Styles.Color.Aqua,new UnityEngine.Color(0.5f, 1.0f, 1.0f) },
			{ Styles.Color.Green,UnityEngine.Color.green },
			{ Styles.Color.Yellow,UnityEngine.Color.yellow },
			{ Styles.Color.Orange,new UnityEngine.Color(1.0f, 0.5f, 0.0f) },
			{ Styles.Color.Red,UnityEngine.Color.red },
			{ Styles.Color.Purple,new UnityEngine.Color(0.9f,0.5f, 1.0f) },
			{ Styles.Color.White,UnityEngine.Color.white },
		};

		public static UnityEngine.Color GetColor(Styles.Color styleColor)
		{
			UnityEngine.Color color = UnityEngine.Color.white;
			if (s_ColorDictionary.TryGetValue(styleColor, out color))
			{
				return color;
			}
			return UnityEngine.Color.white;
		}

		public static GUIStyle GetDataInPin(System.Type type)
		{
			if (type != null && DataSlotGUIUtility.IsList(type))
			{
				return nodeDataArrayInPin;
			}
			return nodeDataInPin;
		}

		public static GUIStyle GetDataOutPin(System.Type type)
		{
			if (type != null && DataSlotGUIUtility.IsList(type))
			{
				return nodeDataArrayOutPin;
			}
			return nodeDataOutPin;
		}

		public static GUIStyle GetDataReroutePin(System.Type type)
		{
			if (type != null && DataSlotGUIUtility.IsList(type))
			{
				return dataArrayReroutePin;
			}
			return reroutePin;
		}
	}
}
