//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	internal struct SerializedPropertyKey : System.IEquatable<SerializedPropertyKey>
	{
		private SerializedObject _SerializedObject;
		private string _PropertyPath;

		private int _HashCode;

		public SerializedPropertyKey(SerializedProperty property)
		{
			_SerializedObject = property.serializedObject;
			_PropertyPath = property.propertyPath;

			_HashCode = _PropertyPath.GetHashCode();
			var targetObjects = _SerializedObject.targetObjects;
			for (int objIndex = 0; objIndex < targetObjects.Length; objIndex++)
			{
				Object obj = targetObjects[objIndex];
				_HashCode ^= obj.GetHashCode();
			}
		}

		public override int GetHashCode()
		{
			return _HashCode;
		}

		public SerializedProperty GetProperty()
		{
			try
			{
				return _SerializedObject.FindProperty(_PropertyPath);
			}
			catch
			{
				return null;
			}
		}

		bool System.IEquatable<SerializedPropertyKey>.Equals(SerializedPropertyKey other)
		{
			try
			{
				Object[] targetObjects = _SerializedObject.targetObjects;
				Object[] otherTargetObjects = other._SerializedObject.targetObjects;

				if (_PropertyPath != other._PropertyPath ||
					targetObjects == null || otherTargetObjects == null ||
					targetObjects.Length != otherTargetObjects.Length)
				{
					return false;
				}

				for (int i = 0; i < targetObjects.Length; ++i)
				{
					if (targetObjects[i] != otherTargetObjects[i])
					{
						return false;
					}
				}

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}