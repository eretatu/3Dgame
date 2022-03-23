//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ArborEditor
{
	using Arbor;

	public struct ParameterTypeMenuItem
	{
		public readonly static ParameterTypeMenuItem[] menuItems;
		private readonly static GUIContent[] s_DisplayOptions;

		static void Move(Parameter.Type type, Parameter.Type to, ref Parameter.Type[] parameterTypes, ref GUIContent[] contents)
		{
			int fromIndex = ArrayUtility.IndexOf(parameterTypes, type);
			int toIndex = ArrayUtility.IndexOf(parameterTypes, to);
			if (fromIndex < toIndex)
			{
				--toIndex;
			}
			GUIContent content = contents[fromIndex];

			ArrayUtility.RemoveAt(ref parameterTypes, fromIndex);
			ArrayUtility.RemoveAt(ref contents, fromIndex);

			ArrayUtility.Insert(ref parameterTypes, toIndex, type);
			ArrayUtility.Insert(ref contents, toIndex, content);
		}

		static ParameterTypeMenuItem()
		{
			Parameter.Type[] rawParameterTypes = EnumUtility.GetValues<Parameter.Type>();
			GUIContent[] rawContents = EnumUtility.GetContents<Parameter.Type>();

			Parameter.Type[] parameterTypes = new Parameter.Type[rawParameterTypes.Length];
			System.Array.Copy(rawParameterTypes, parameterTypes, rawParameterTypes.Length);
			GUIContent[] contents = new GUIContent[rawContents.Length];
			System.Array.Copy(rawContents, contents, rawContents.Length);

			Move(Parameter.Type.Long, Parameter.Type.Float, ref parameterTypes, ref contents);
			Move(Parameter.Type.Vector4, Parameter.Type.Quaternion, ref parameterTypes, ref contents);
			Move(Parameter.Type.GameObject, Parameter.Type.Transform, ref parameterTypes, ref contents);
			Move(Parameter.Type.Vector4List, Parameter.Type.QuaternionList, ref parameterTypes, ref contents);

			List<ParameterTypeMenuItem> menuItems = new List<ParameterTypeMenuItem>();
			for (int i = 0, count = parameterTypes.Length; i < count; ++i)
			{
				Parameter.Type parameterType = parameterTypes[i];

				if (parameterType == Parameter.Type.Vector2 || parameterType == Parameter.Type.GameObject || parameterType == Parameter.Type.Variable ||
					parameterType == Parameter.Type.IntList || parameterType == Parameter.Type.Vector2List || parameterType == Parameter.Type.GameObjectList || parameterType == Parameter.Type.VariableList)
				{
					menuItems.Add(new ParameterTypeMenuItem() { content = GUIContent.none, isSeparator = true });
				}

				menuItems.Add(new ParameterTypeMenuItem() { content = contents[i], type = parameterType });
			}

			s_DisplayOptions = new GUIContent[menuItems.Count];
			for (int i = 0; i < menuItems.Count; i++)
			{
				s_DisplayOptions[i] = menuItems[i].content;
			}

			ParameterTypeMenuItem.menuItems = menuItems.ToArray();
		}

		public static int GetIndex(Parameter.Type parameterType)
		{
			int selectedIndex = -1;
			for (int i = 0; i < menuItems.Length; i++)
			{
				var menuItem = menuItems[i];
				if (!menuItem.isSeparator && menuItem.type == parameterType)
				{
					selectedIndex = i;
					break;
				}
			}

			return selectedIndex;
		}

		public static Parameter.Type Popup(Rect rect, GUIContent label, Parameter.Type parameterType)
		{
			int selectedIndex = GetIndex(parameterType);

			EditorGUI.BeginChangeCheck();
			selectedIndex = EditorGUI.Popup(rect, label, selectedIndex, s_DisplayOptions);
			if (EditorGUI.EndChangeCheck() && selectedIndex >= 0)
			{
				parameterType = menuItems[selectedIndex].type;
			}

			return parameterType;
		}

		public Parameter.Type type;
		public GUIContent content;
		public bool isSeparator;
	}
}