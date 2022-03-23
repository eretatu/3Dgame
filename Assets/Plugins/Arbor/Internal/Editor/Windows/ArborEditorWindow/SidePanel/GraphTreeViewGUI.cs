//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ArborEditor
{
	using ArborEditor.IMGUI.Controls;

	internal sealed class GraphTreeViewGUI : TreeViewGUI
	{
		public GraphTreeViewGUI(EditorWindow window, TreeView treeView, TreeViewState state) : base(window, treeView, state)
		{
		}

		protected override void OnContentGUI(Rect rect, TreeViewItem item, string label, bool selected, bool focused)
		{
			var graphItem = item as GraphTreeViewItem;
			var window = hostWindow as ArborEditorWindow;
			if (graphItem == null || window == null)
			{
				base.OnContentGUI(rect, item, label, selected, focused);
				return;
			}

			bool isExternalGraph = graphItem.isExternal;

			if (Event.current.rawType == EventType.Repaint)
			{
				float contentIndent = GetContentIndent(item);
				rect.xMin += contentIndent;

				GUIStyle treeStyle = Styles.treeLineStyle;

				if (isExternalGraph)
				{
					if (item.disable)
					{
						treeStyle = Styles.disabledPrefabTreeStyle;
					}
					else
					{
						treeStyle = Styles.prefabTreeStyle;
					}
				}
				else
				{
					if (item.disable)
					{
						treeStyle = Styles.disabledTreeStyle;
					}
				}

				Texture icon = GetIconForItem(item);
				if (icon != null)
				{
					Rect iconRect = rect;
					iconRect.width = k_IconWidth;

					GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

					rect.xMin += k_IconWidth + k_SpaceBetweenIconAndText;
				}

				treeStyle.Draw(rect, label, false, false, selected, focused);
			}

			if (isExternalGraph)
			{
				GUIStyle rightArrowStyle = Styles.rightArrow;
				Vector2 size = new Vector2(rightArrowStyle.fixedWidth, rightArrowStyle.fixedHeight);
				Rect rightArrowRect = new Rect(rect.xMax - size.x, rect.y + (rect.height - size.y) * 0.5f, size.x, size.y);

				if (GUI.Button(rightArrowRect, GUIContent.none, rightArrowStyle))
				{
					window.SelectExternalGraph(graphItem);
				}
			}
		}
	}
}