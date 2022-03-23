//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.BehaviourTree
{
	using Arbor.BehaviourTree;

	[System.Reflection.Obfuscation(Exclude = true)]
	[System.Serializable]
	internal sealed class DecoratorEditorGUI : TreeNodeBehaviourEditorGUI
	{
		protected override bool HasBehaviourEnable()
		{
			return true;
		}

		protected override bool GetBehaviourEnable()
		{
			Decorator decorator = behaviourObj as Decorator;
			return decorator.behaviourEnabled;
		}

		protected override void SetBehaviourEnable(bool enable)
		{
			Decorator decorator = behaviourObj as Decorator;
			decorator.behaviourEnabled = enable;
		}

		static Color GetConditionColor(Decorator.Condition condition)
		{
			switch (condition)
			{
				case Decorator.Condition.None:
					return Color.gray;
				case Decorator.Condition.Success:
					return Color.green;
				case Decorator.Condition.Failure:
					return Color.red;
			}

			return Color.clear;
		}

		void MoveUpContextMenu()
		{
			if (treeBehaviourEditor != null)
			{
				treeBehaviourEditor.MoveDecorator(behaviourIndex, behaviourIndex - 1);
			}
		}

		void MoveDownContextMenu()
		{
			if (treeBehaviourEditor != null)
			{
				treeBehaviourEditor.MoveDecorator(behaviourIndex, behaviourIndex + 1);
			}
		}

		void DeleteContextMenu()
		{
			if (nodeEditor != null)
			{
				TreeBehaviourNodeEditor behaviourNodeEditor = nodeEditor as TreeBehaviourNodeEditor;
				behaviourNodeEditor.DestroyDecoratorAt(behaviourIndex);
			}
		}

		protected override void SetPopupMenu(GenericMenu menu)
		{
			bool editable = nodeEditor.graphEditor.editable;

			DecoratorList decoratorList = treeBehaviourNode.decoratorList;
			int decoratorCount = decoratorList.count;

			if (behaviourIndex >= 1 && editable)
			{
				menu.AddItem(EditorContents.moveUp, false, MoveUpContextMenu);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.moveUp);
			}

			if (behaviourIndex < decoratorCount - 1 && editable)
			{
				menu.AddItem(EditorContents.moveDown, false, MoveDownContextMenu);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.moveDown);
			}

			base.SetPopupMenu(menu);

			if (editable)
			{
				menu.AddItem(EditorContents.delete, false, DeleteContextMenu);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.delete);
			}
		}

		protected override void OnUnderlayGUI(Rect rect)
		{
			Event currentEvent = Event.current;
			if (currentEvent.type != EventType.Repaint || !Application.isPlaying)
			{
				return;
			}

			Decorator decorator = behaviourObj as Decorator;
			if (!decorator.HasConditionCheck())
			{
				return;
			}

			Decorator.Condition condition = decorator.currentCondition;
			Color conditionColor = GetConditionColor(condition);

			rect.width = 5f;
			EditorGUI.DrawRect(rect, conditionColor);
		}
	}
}