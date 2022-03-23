//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ArborEditor
{
	public class PropertyEditor
	{
		public SerializedProperty property
		{
			get;
			private set;
		}
		public System.Reflection.FieldInfo fieldInfo
		{
			get;
			private set;
		}

		protected virtual void OnInitialize()
		{
		}

		protected virtual void OnDestroy()
		{
		}

		protected virtual void OnGUI(Rect position, GUIContent label)
		{
		}

		protected virtual float GetHeight(GUIContent label)
		{
			return 0;
		}

		private float _HeightCache;

		internal void DoInitialize(SerializedProperty property, System.Reflection.FieldInfo fieldInfo)
		{
			this.property = property;
			this.fieldInfo = fieldInfo;

			OnInitialize();
		}

		internal void DoDestroy()
		{
			OnDestroy();
		}

		public void DoOnGUI(Rect position, GUIContent label)
		{
			OnGUI(position, label);
		}

		public float DoGetHeight(GUIContent label)
		{
			if (Event.current.type == EventType.Layout)
			{
				_HeightCache = GetHeight(label);
			}

			return _HeightCache;
		}
	}

	public static class PropertyEditorUtility
	{
		private static Dictionary<SerializedPropertyKey, PropertyEditor> s_PropertyEditors = null;
		private static Dictionary<SerializedPropertyKey, PropertyEditor> s_NewEditors = null;
		private static bool s_DirtyPropertyEditors = true;

		public static T GetPropertyEditor<T>(SerializedProperty property, System.Reflection.FieldInfo fieldInfo) where T : PropertyEditor, new()
		{
			if (Event.current.type == EventType.Layout)
			{
				if (s_DirtyPropertyEditors)
				{
					if (s_NewEditors == null)
					{
						s_NewEditors = new Dictionary<SerializedPropertyKey, PropertyEditor>();
					}

					if (s_PropertyEditors != null)
					{
						foreach (var pair in s_PropertyEditors)
						{
							SerializedPropertyKey key = pair.Key;
							PropertyEditor propertyEditor = pair.Value;

							SerializedProperty prop = key.GetProperty();
							if (prop != null && SerializedPropertyUtility.EqualContents(prop, propertyEditor.property))
							{
								s_NewEditors.Add(key, propertyEditor);
							}
							else
							{
								pair.Value.DoDestroy();
							}
						}
					}

					Dictionary<SerializedPropertyKey, PropertyEditor> oldEditors = s_PropertyEditors;

					s_PropertyEditors = s_NewEditors;

					s_NewEditors = oldEditors;

					if (s_NewEditors != null)
					{
						s_NewEditors.Clear();
					}

					s_DirtyPropertyEditors = false;
				}
			}
			else
			{
				s_DirtyPropertyEditors = true;
			}

			if (s_PropertyEditors == null)
			{
				s_PropertyEditors = new Dictionary<SerializedPropertyKey, PropertyEditor>();
			}

			SerializedPropertyKey propertyKey = new SerializedPropertyKey(property);

			PropertyEditor editor = null;
			if (s_PropertyEditors.TryGetValue(propertyKey, out editor))
			{
				if (SerializedPropertyUtility.EqualContents(property, editor.property))
				{
					return editor as T;
				}

				editor.DoDestroy();
				s_PropertyEditors.Remove(propertyKey);
			}

			editor = new T();
			editor.DoInitialize(property.Copy(), fieldInfo);

			s_PropertyEditors.Add(propertyKey, editor);

			return editor as T;
		}
	}

	public class PropertyEditorDrawer<T> : PropertyDrawer where T : PropertyEditor, new()
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			try
			{
#if UNITY_2019_3_OR_NEWER
				if (property.IsInvalidManagedReference())
				{
					EditorGUI.PropertyField(position, property, label);
					return;
				}
#endif
				T editor = PropertyEditorUtility.GetPropertyEditor<T>(property, fieldInfo);

				editor.DoOnGUI(position, label);
			}
			catch (System.Exception ex)
			{
				if (EditorGUITools.ShouldRethrowException(ex))
				{
					throw;
				}
				else
				{
					Debug.LogException(ex);
				}
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			try
			{
#if UNITY_2019_3_OR_NEWER
				if (property.IsInvalidManagedReference())
				{
					return EditorGUI.GetPropertyHeight(property, label);
				}
#endif

				T editor = PropertyEditorUtility.GetPropertyEditor<T>(property, fieldInfo);

				return editor.DoGetHeight(label);
			}
			catch (System.Exception ex)
			{
				if (EditorGUITools.ShouldRethrowException(ex))
				{
					throw;
				}
				else
				{
					Debug.LogException(ex);
				}

				return 0f;
			}
		}
	}
}