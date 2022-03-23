//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	using Arbor;

	internal sealed class CalculatorEditorGUI : BehaviourEditorGUI
	{
		void SetRecalculateMode(object value)
		{
			Calculator calculator = this.behaviourObj as Calculator;
			if (calculator != null)
			{
				var recalculateMode = (RecalculateMode)value;
				Undo.RecordObject(calculator, "Set Recalculate Mode");
				calculator.recalculateMode = recalculateMode;
				EditorUtility.SetDirty(calculator);
			}
		}

		bool SetRecalculateModeMenu(GenericMenu menu, bool fullPath)
		{
			Calculator calculator = this.behaviourObj as Calculator;
			if (calculator == null)
			{
				return false;
			}

			var recalculateMode = calculator.recalculateMode;
			menu.AddItem(RecalculateModeContents.Get(RecalculateMode.Dirty, fullPath), recalculateMode == RecalculateMode.Dirty, SetRecalculateMode, RecalculateMode.Dirty);
			menu.AddItem(RecalculateModeContents.Get(RecalculateMode.Frame, fullPath), recalculateMode == RecalculateMode.Frame, SetRecalculateMode, RecalculateMode.Frame);
			menu.AddItem(RecalculateModeContents.Get(RecalculateMode.Scope, fullPath), recalculateMode == RecalculateMode.Scope, SetRecalculateMode, RecalculateMode.Scope);
			menu.AddItem(RecalculateModeContents.Get(RecalculateMode.Always, fullPath), recalculateMode == RecalculateMode.Always, SetRecalculateMode, RecalculateMode.Always);

			return true;
		}

		protected override void SetPopupMenu(GenericMenu menu)
		{
			SetRecalculateModeMenu(menu, true);
		}

		internal void RecalculateModeButton(Rect iconRect)
		{
			if (Event.current.type == EventType.Repaint)
			{
				GUIStyle.none.Draw(new Rect(iconRect.x - 2, iconRect.yMax - iconRect.height * 0.2f, 13, 8), RecalculateModeContents.iconDropDown, false, false, false, false);
			}

			if (EditorGUI.DropdownButton(iconRect, RecalculateModeContents.buttonTooltip, FocusType.Passive, GUIStyle.none))
			{
				GenericMenu menu = new GenericMenu();
				SetRecalculateModeMenu(menu, false);
				menu.DropDown(iconRect);
			}
		}

		static class RecalculateModeContents
		{
			public static GUIContent dirty;
			public static GUIContent frame;
			public static GUIContent scope;
			public static GUIContent always;

			public static GUIContent dirtyFull;
			public static GUIContent frameFull;
			public static GUIContent scopeFull;
			public static GUIContent alwaysFull;

			public static readonly GUIContent iconDropDown = null;
			public static GUIContent buttonTooltip;

			private static SystemLanguage _CurrentLanguage;

			static RecalculateModeContents()
			{
				iconDropDown = EditorGUIUtility.IconContent("Icon Dropdown");
				UpdateLocalization();

				ArborSettings.onChangedLanguage += OnChangedLanguage;
				Localization.onRebuild += UpdateLocalization;
			}

			static void UpdateLocalization()
			{
				_CurrentLanguage = ArborSettings.currentLanguage;

				dirty = Localization.GetTextContent("Dirty");
				frame = Localization.GetTextContent("Frame");
				scope = Localization.GetTextContent("Scope");
				always = Localization.GetTextContent("Always");

				string recalculateModeText = Localization.GetWord("Recalculate Mode");

				dirtyFull = new GUIContent(dirty);
				dirtyFull.text = recalculateModeText + "/" + dirtyFull.text;

				frameFull = new GUIContent(frame);
				frameFull.text = recalculateModeText + "/" + frameFull.text;

				scopeFull = new GUIContent(scope);
				scopeFull.text = recalculateModeText + "/" + scopeFull.text;

				alwaysFull = new GUIContent(always);
				alwaysFull.text = recalculateModeText + "/" + alwaysFull.text;

				buttonTooltip = new GUIContent("", recalculateModeText);
			}

			public static GUIContent Get(RecalculateMode mode, bool getFull)
			{
				switch (mode)
				{
					case RecalculateMode.Dirty:
						return getFull ? dirtyFull : dirty;
					case RecalculateMode.Frame:
						return getFull ? frameFull : frame;
					case RecalculateMode.Scope:
						return getFull ? scopeFull : scope;
					case RecalculateMode.Always:
						return getFull ? alwaysFull : always;
				}

				return getFull ? dirtyFull : dirty;
			}

			static void OnChangedLanguage()
			{
				if (_CurrentLanguage != ArborSettings.currentLanguage)
				{
					UpdateLocalization();
				}
			}
		}
	}
}