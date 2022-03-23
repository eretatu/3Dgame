//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;

namespace ArborEditor
{
	public static class SerializedPropertyUtility
	{
		public static bool EqualContents(SerializedProperty x, SerializedProperty y)
		{
			try
			{
				return SerializedProperty.EqualContents(x, y);
			}
			catch
			{
				return false;
			}
		}
	}
}