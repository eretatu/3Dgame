//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

using Arbor;

namespace ArborEditor
{
	[CustomNodeEditor(typeof(CalculatorNode))]
	public sealed class CalculatorEditor : NodeEditor
	{
		public CalculatorNode calculatorNode
		{
			get
			{
				return node as CalculatorNode;
			}
		}

		BehaviourEditorGUI _BehaviourEditorGUI;

		public Editor editor
		{
			get
			{
				BehaviourEditorGUI behaviourEditor = GetBehaviourEditor();
				if (behaviourEditor != null)
				{
					return behaviourEditor.editor;
				}
				return null;
			}
		}

		public BehaviourEditorGUI GetBehaviourEditor()
		{
			if (node == null)
			{
				return null;
			}

			Object behaviourObj = calculatorNode.GetObject();

			if ((object)behaviourObj != null && _BehaviourEditorGUI != null)
			{
				Editor editor = _BehaviourEditorGUI.editor;
				if (editor == null)
				{
					_BehaviourEditorGUI = null;
				}
				else if (behaviourObj != editor.target)
				{
					_BehaviourEditorGUI.DestroyEditor();

					_BehaviourEditorGUI = null;
				}
			}

			if (_BehaviourEditorGUI == null)
			{
				_BehaviourEditorGUI = new CalculatorEditorGUI();
				_BehaviourEditorGUI.Initialize(this, behaviourObj);

				OnInitializeEditor(_BehaviourEditorGUI);
			}
			else if (!ComponentUtility.IsValidObject(_BehaviourEditorGUI.behaviourObj))
			{
				_BehaviourEditorGUI.Repair(behaviourObj);
			}

			return _BehaviourEditorGUI;
		}

		public override void Validate(Node node)
		{
			base.Validate(node);

			if (_BehaviourEditorGUI != null)
			{
				_BehaviourEditorGUI.Validate();
			}
		}

		void OnInitializeEditor(BehaviourEditorGUI behaviourEditor)
		{
			behaviourEditor.marginType = BehaviourEditorGUI.MarginType.ForceFull;
		}

		public override Texture2D GetIcon()
		{
			Texture icon = EditorGUITools.GetThumbnailContent(calculatorNode.GetObject()).image;
			if (icon != null && !DefaultScriptIcon.IsDefaultScriptIcon(icon))
			{
				return icon as Texture2D;
			}
			return Icons.calculatorNodeIcon;
		}

		public override Styles.Color GetStyleColor()
		{
			return Styles.Color.Blue;
		}

		protected override void DrawOverlayIcon(Rect iconRect)
		{
			iconRect.y += 2;

			Calculator calculator = calculatorNode.calculator;
			if (calculator != null)
			{
				switch (calculator.recalculateMode)
				{
					case RecalculateMode.Dirty:
						break;
					case RecalculateMode.Frame:
						GUIStyle.none.Draw(iconRect, Icons.frameCalculateIcon, false, false, false, false);
						break;
					case RecalculateMode.Scope:
						GUIStyle.none.Draw(iconRect, Icons.scopeCalculateIcon, false, false, false, false);
						break;
					case RecalculateMode.Always:
						GUIStyle.none.Draw(iconRect, Icons.alwaysCalculateIcon, false, false, false, false);
						break;
				}
			}
		}

		protected override void HeaderIconGUI(Rect iconRect, Texture icon)
		{
			base.HeaderIconGUI(iconRect, icon);

			CalculatorEditorGUI behaviourEditor = GetBehaviourEditor() as CalculatorEditorGUI;
			if (behaviourEditor == null)
			{
				return;
			}

			behaviourEditor.RecalculateModeButton(iconRect);
		}

		void CreateEditor()
		{
			SetupResizable();
		}

		void DestroyEditor()
		{
			if (_BehaviourEditorGUI != null)
			{
				_BehaviourEditorGUI.DestroyEditor();
				_BehaviourEditorGUI = null;
			}
		}

		protected override void OnInitialize()
		{
			CreateEditor();
		}

		internal void SetupResizable()
		{
			CalculatorBehaviourEditor editor = this.editor as CalculatorBehaviourEditor;
			if (editor != null)
			{
				isResizable = editor.IsResizableNode();
			}
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnEnable()
		{
			isShowableComment = true;

			if (editor != null)
			{
				SetupResizable();
			}
		}

		protected override float GetWidth()
		{
			CalculatorBehaviourEditor editor = this.editor as CalculatorBehaviourEditor;
			return (editor != null) ? editor.GetNodeWidth() : base.GetWidth();
		}

		protected override bool HasHelpButton()
		{
			return EditorGUITools.HasHelpButton(calculatorNode.GetObject());
		}

		protected override void OnHelpButton(Rect position, GUIStyle style)
		{
			EditorGUITools.HelpButton(position, calculatorNode.GetObject(), style);
		}

		protected override bool HasPresetButton()
		{
			return Presets.PresetContextMenu.HasPresetButton(calculatorNode.GetObject());
		}

		void OnPresetChanged()
		{
			NodeBehaviourEditor editor = this.editor as NodeBehaviourEditor;
			if (editor != null)
			{
				editor.OnPresetApplied();
			}

			Repaint();
		}

		protected override void OnPresetButton(Rect position, GUIStyle style)
		{
			Presets.PresetContextMenu.PresetButton(position, calculatorNode.GetObject(), OnPresetChanged, style);
		}

		public void OnEditorGUI()
		{
			bool hierarchyMode = EditorGUIUtility.hierarchyMode;
			EditorGUIUtility.hierarchyMode = false;

			EditorGUIUtility.labelWidth = EditorGUITools.CalcLabelWidth(EditorGUITools.contextWidth);

			BehaviourEditorGUI behaviourEditor = GetBehaviourEditor();
			if (behaviourEditor != null)
			{
				behaviourEditor.OnGUI();
			}

			EditorGUIUtility.labelWidth = 0; // Use default labelWidth
			EditorGUIUtility.hierarchyMode = hierarchyMode;
		}

		protected override void OnGUI()
		{
			using (new ProfilerScope("OnCalculatorGUI"))
			{
				EditorGUITools.DrawSeparator(ArborEditorWindow.isDarkSkin);

				OnEditorGUI();
			}
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnDestroy()
		{
			DestroyEditor();
		}

		public override string GetTitle()
		{
			BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(calculatorNode.GetObject());
			return behaviourInfo.titleContent.text;
		}

		static void EditScriptContextMenu(object obj)
		{
			MonoScript script = obj as MonoScript;

			AssetDatabase.OpenAsset(script);
		}

		public override bool IsCopyable()
		{
			return calculatorNode.calculator != null;
		}

		protected override void SetContextMenu(GenericMenu menu, Rect headerPosition, bool editable)
		{
			BehaviourEditorGUI behaviourEditor = GetBehaviourEditor();
			if (behaviourEditor != null)
			{
				behaviourEditor.SetContextMenu(menu, headerPosition);
			}
		}

		protected override bool HasOutsideGUI()
		{
			return true;
		}

		bool IsVisibleDataLinkGUI()
		{
			BehaviourEditorGUI behaviourEditor = GetBehaviourEditor();
			return behaviourEditor != null && behaviourEditor.IsVisibleDataLinkGUI();
		}

		protected override RectOffset GetOutsideOffset()
		{
			return IsVisibleDataLinkGUI() ? new RectOffset(DataSlotGUIUtility.kOutsideOffset, 0, 0, 0) : new RectOffset();
		}

		protected override void OnOutsideGUI()
		{
			RectOffset overflowOffset = GetOverflowOffset();

			BehaviourEditorGUI behaviourEditor = GetBehaviourEditor();
			if (behaviourEditor != null)
			{
				behaviourEditor.DataLinkGUI(overflowOffset);
			}
		}
	}
}
