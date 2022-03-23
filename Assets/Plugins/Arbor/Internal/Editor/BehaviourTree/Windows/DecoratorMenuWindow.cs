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
	[InitializeOnLoad]
	internal sealed class DecoratorMenuWindow : SelectScriptWindow
	{
		private static DecoratorMenuWindow _Instance;

		public static DecoratorMenuWindow instance
		{
			get
			{
				if (_Instance == null)
				{
					DecoratorMenuWindow[] objects = Resources.FindObjectsOfTypeAll<DecoratorMenuWindow>();
					if (objects.Length > 0)
					{
						_Instance = objects[0];
					}
				}
				if (_Instance == null)
				{
					_Instance = ScriptableObject.CreateInstance<DecoratorMenuWindow>();
				}
				return _Instance;
			}
		}

		private TreeBehaviourNodeEditor _TreeBehaviourNodeEditor;
		private int _Index = -1;

		protected override string searchWord
		{
			get
			{
				return ArborEditorCache.decoratorSearch;
			}

			set
			{
				ArborEditorCache.decoratorSearch = value;
			}
		}

		protected override System.Type GetClassType()
		{
			return typeof(Decorator);
		}

		protected override string GetRootElementName()
		{
			return "Decorators";
		}

		protected override void OnSelect(System.Type classType)
		{
			if (_Index == -1)
			{
				_TreeBehaviourNodeEditor.AddDecorator(classType);
			}
			else
			{
				_TreeBehaviourNodeEditor.InsertDecorator(_Index, classType);
			}
		}

		public void Init(TreeBehaviourNodeEditor nodeEditor, Rect buttonRect, int index)
		{
			_TreeBehaviourNodeEditor = nodeEditor;
			_Index = index;

			Open(nodeEditor.graphEditor.hostWindow, buttonRect);
		}
	}
}