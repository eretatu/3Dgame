//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace ArborEditor
{
	using Arbor;
	using Arbor.DynamicReflection;
	using Arbor.Serialization;

	public static class EditorGUITools
	{
		private static GUIContent s_HelpButtonContent = new GUIContent(EditorGUIUtility.FindTexture("_Help"));

		private static Texture2D s_InfoIcon;
		private static Texture2D s_WarningIcon;
		private static Texture2D s_ErrorIcon;

		public const float kDropDownWidth = 13f;
		public const float kSubtractDropdownWidth = 14f;

		public const float kPopupWidth = 16f;
		public const float kSubtrackPopupWidth = 20f;

		public const float kWideModeMinWidth = 330f;

		public static GUIContent helpContent
		{
			get
			{
				return s_HelpButtonContent;
			}
		}

		private static GUIContent[] s_WaitSpins;

		private static Material s_HandleWireMaterial2D;

		public static Material handleWireMaterial2D
		{
			get
			{
				if (s_HandleWireMaterial2D == null)
				{
					s_HandleWireMaterial2D = (Material)EditorGUIUtility.LoadRequired("SceneView/2DHandleLines.mat");
				}
				return s_HandleWireMaterial2D;
			}
		}

		internal static Texture2D infoIcon
		{
			get
			{
				if (s_InfoIcon == null)
					s_InfoIcon = EditorGUIUtility.FindTexture("console.infoicon");
				return s_InfoIcon;
			}
		}



		internal static Texture2D warningIcon
		{
			get
			{
				if (s_WarningIcon == null)
					s_WarningIcon = EditorGUIUtility.FindTexture("console.warnicon");
				return s_WarningIcon;
			}
		}


		internal static Texture2D errorIcon
		{
			get
			{
				if (s_ErrorIcon == null)
					s_ErrorIcon = EditorGUIUtility.FindTexture("console.erroricon");
				return s_ErrorIcon;
			}
		}

		public static Texture2D GetHelpIcon(MessageType type)
		{
			switch (type)
			{
				case MessageType.Info:
					return infoIcon;
				case MessageType.Warning:
					return warningIcon;
				case MessageType.Error:
					return errorIcon;
			}
			return null;
		}

		public static GUIContent GetHelpBoxContent(string message, MessageType type)
		{
			GUIContent content = GetTextContent(message);
			content.image = GetHelpIcon(type);

			return content;
		}

		public static float GetHelpBoxHeight(string message, MessageType type, float width)
		{
			GUIContent content = GetHelpBoxContent(message, type);

			return EditorStyles.helpBox.CalcHeight(content, width);
		}

		public static float GetHelpBoxHeight(string message, MessageType type)
		{
			GUIContent content = GetHelpBoxContent(message, type);

			Vector2 contentSize = EditorStyles.helpBox.CalcSize(content);
			return EditorStyles.helpBox.CalcScreenSize(contentSize).y;
		}

		private delegate string DelegateSearchField(Rect position, string text);
		private delegate string DelegateToolbarSearchField(int id, Rect position, string[] searchModes, ref int searchMode, string text);
		private delegate string DelegateToolbarSearchField2(Rect position, string text, bool showWithPopupArrow);
		private delegate string DelegateToolbarSearchFieldLayout(string text, string[] searchModes, ref int searchMode, params GUILayoutOption[] options);

		private static readonly DelegateSearchField _SearchField;
		private static readonly DelegateToolbarSearchField _ToolbarSearchField;
		private static readonly DelegateToolbarSearchFieldLayout _ToolbarSearchFieldLayout;
		private static readonly DelegateToolbarSearchField2 _ToolbarSearchField2;

		public static readonly bool useNewSearchField;

		private delegate bool DelegateValidContextEditorMenu(MenuCommand command);
		private delegate void DelegateContextEditorMenu(MenuCommand command);

		private delegate Texture2D DelegateFindTextureByType(System.Type type);

		private static readonly DelegateFindTextureByType _FindTexture;

		public static readonly float toolbarHeight;

		private delegate string DelegateGetNiceHelpNameForObject(Object obj);

		private static readonly DelegateGetNiceHelpNameForObject _GetNiceHelpNameForObject;

		private static readonly DynamicMethod _DoTextFieldMethod;
		private static readonly DynamicField _RecycledEditorField;

		private delegate void DelegateClearGlobalCache();
		private static readonly DelegateClearGlobalCache _DelegateClearGlobalCache;

		private static Dictionary<System.Type, System.Type[]> s_DelegateArguments = new Dictionary<System.Type, System.Type[]>();

		private delegate Assembly[] DelegateGetLoadedAssemblies();

		private static DelegateGetLoadedAssemblies s_GetLoadedAssemblies;

		private delegate string DelegateGetActiveFolderPath();
		private static readonly DelegateGetActiveFolderPath _DelegateGetActiveFolderPath;

		private static readonly System.Func<float> _GetContextWidthMethod;

		private static readonly DynamicMethod _EditorIsEnabledMethod;

		public static Assembly[] loadedAssemblies
		{
			get
			{
				return s_GetLoadedAssemblies();
			}
		}

		public static float contextWidth
		{
			get
			{
				return _GetContextWidthMethod();
			}
		}

		static System.Type[] GetDelegateArguments(System.Type delegateType)
		{
			MethodInfo invokeMethod = delegateType.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);

			System.Type[] arg = null;
			if (!s_DelegateArguments.TryGetValue(delegateType, out arg))
			{
				List<System.Type> arguments = new List<System.Type>();
				var parameters = invokeMethod.GetParameters();
				for (int i = 0; i < parameters.Length; i++)
				{
					ParameterInfo parameterInfo = parameters[i];
					arguments.Add(parameterInfo.ParameterType);
				}

				arg = arguments.ToArray();

				s_DelegateArguments.Add(delegateType, arg);
			}

			return arg;
		}

		public static MethodInfo GetMethod(System.Type delegateType, System.Type classType, string name, BindingFlags bindingAttr)
		{
			return classType.GetMethod(name, bindingAttr, null, GetDelegateArguments(delegateType), null);
		}

		public static MethodInfo GetMethod<TDelegate>(System.Type classType, string name, BindingFlags bindingAttr)
		{
			System.Type delegateType = typeof(TDelegate);

			return GetMethod(delegateType, classType, name, bindingAttr);
		}

		public static TDelegate GetDelegate<TDelegate>(System.Type classType, string name, BindingFlags bindingAttr) where TDelegate : System.Delegate
		{
			System.Type delegateType = typeof(TDelegate);

			MethodInfo method = GetMethod(delegateType, classType, name, bindingAttr);

			if (method == null)
			{
				return null;
			}

			return System.Delegate.CreateDelegate(delegateType, method) as TDelegate;
		}

		static EditorGUITools()
		{
			System.Type editorGUIType = typeof(EditorGUI);
			System.Type editorGUILayoutType = typeof(EditorGUILayout);
			System.Type editorGUIUtilityType = typeof(EditorGUIUtility);

			_SearchField = GetDelegate<DelegateSearchField>(editorGUIType, "SearchField", BindingFlags.Static | BindingFlags.NonPublic);
			_ToolbarSearchField = GetDelegate<DelegateToolbarSearchField>(editorGUIType, "ToolbarSearchField", BindingFlags.Static | BindingFlags.NonPublic);
			_ToolbarSearchField2 = GetDelegate<DelegateToolbarSearchField2>(editorGUIType, "ToolbarSearchField", BindingFlags.Static | BindingFlags.NonPublic);
			_ToolbarSearchFieldLayout = GetDelegate<DelegateToolbarSearchFieldLayout>(editorGUILayoutType, "ToolbarSearchField", BindingFlags.Static | BindingFlags.NonPublic);

			_FindTexture = GetDelegate<DelegateFindTextureByType>(editorGUIUtilityType, "FindTexture", BindingFlags.Static | BindingFlags.NonPublic);

			System.Type recycledTextEdtiorType = editorGUIType.GetNestedType("RecycledTextEditor", BindingFlags.NonPublic);
			_DoTextFieldMethod = DynamicMethod.GetMethod(editorGUIType.GetMethod("DoTextField", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new System.Type[] { recycledTextEdtiorType, typeof(int), typeof(Rect), typeof(string), typeof(GUIStyle), typeof(string), typeof(bool).MakeByRefType(), typeof(bool), typeof(bool), typeof(bool) }, null));
			_RecycledEditorField = DynamicField.GetField(editorGUIType.GetField("s_RecycledEditor", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));

			Assembly unityEditorAssembly = Assembly.Load("UnityEditor.dll");
			System.Type scriptAttributeUtilityType = unityEditorAssembly.GetType("UnityEditor.ScriptAttributeUtility");
			_DelegateClearGlobalCache = GetDelegate<DelegateClearGlobalCache>(scriptAttributeUtilityType, "ClearGlobalCache", BindingFlags.Static | BindingFlags.NonPublic);

			System.Type editorAssembliesType = unityEditorAssembly.GetType("UnityEditor.EditorAssemblies");
			PropertyInfo loadedAssembliesProperty = editorAssembliesType.GetProperty("loadedAssemblies", BindingFlags.Static | BindingFlags.NonPublic);
			s_GetLoadedAssemblies = (DelegateGetLoadedAssemblies)System.Delegate.CreateDelegate(typeof(DelegateGetLoadedAssemblies), loadedAssembliesProperty.GetGetMethod(true));

			_DelegateGetActiveFolderPath = GetDelegate<DelegateGetActiveFolderPath>(typeof(ProjectWindowUtil), "GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

			PropertyInfo contextWidthProperty = typeof(EditorGUIUtility).GetProperty("contextWidth", BindingFlags.Static | BindingFlags.NonPublic);
			_GetContextWidthMethod = (System.Func<float>)System.Delegate.CreateDelegate(typeof(System.Func<float>), contextWidthProperty.GetGetMethod(true));

			s_WaitSpins = new GUIContent[12];
			for (int index = 0; index < 12; ++index)
			{
				s_WaitSpins[index] = new GUIContent(EditorGUIUtility.IconContent("WaitSpin" + index.ToString("00")));
			}

			MethodInfo editorIsEnabledMethod = typeof(Editor).GetMethod("IsEnabled", BindingFlags.Instance | BindingFlags.NonPublic);
			_EditorIsEnabledMethod = DynamicMethod.GetMethod(editorIsEnabledMethod);

			System.Type helpType = typeof(Help);
			_GetNiceHelpNameForObject = GetDelegate<DelegateGetNiceHelpNameForObject>(helpType, "GetNiceHelpNameForObject", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

			toolbarHeight = 18f;

			// Unity 2019.3 or newer
			var windowToolbarHeightField = editorGUIType.GetField("kWindowToolbarHeight", BindingFlags.Static | BindingFlags.NonPublic);
			if (windowToolbarHeightField != null)
			{
				object value = windowToolbarHeightField.GetValue(null);
				if (value != null)
				{
					var valueProperty = value.GetType().GetProperty("value", BindingFlags.Instance | BindingFlags.Public);
					if (valueProperty != null)
					{
						toolbarHeight = (float)valueProperty.GetValue(value, null);
					}
				}
			}

			System.Type typeAddComponentGUI = unityEditorAssembly.GetType("UnityEditor.AddComponent.AddComponentGUI", false);

			if (typeAddComponentGUI != null)
			{
				var method = typeAddComponentGUI.GetMethod("DrawSearchFieldControl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				useNewSearchField = method == null;
			}
			else
			{
				useNewSearchField = false;
			}
		}

		public static string SearchField(Rect position, string text)
		{
			if (_SearchField == null)
			{
				return text;
			}
			return _SearchField(position, text);
		}

		public static string DropdownSearchField(string searchWord)
		{
			Rect rect;
			string str;
			if (useNewSearchField)
			{
				const float kBorderWidth = 1f;
				rect = GUILayoutUtility.GetRect(0, Styles.toolbarSearchField.fixedHeight, Styles.toolbarSearchField);

				rect.height = Styles.toolbarSearchField.fixedHeight;
				rect.xMin += kBorderWidth;
				rect.xMax -= kBorderWidth;
				rect.yMin += kBorderWidth;

				str = ToolbarSearchField(rect, searchWord, false);
			}
			else
			{
				rect = GUILayoutUtility.GetRect(10f, 20f);
				rect.x += 8f;
				rect.width -= 16f;

				str = SearchField(rect, searchWord);
			}

			return str;
		}

		public static string ToolbarSearchField(int id, Rect position, string[] searchModes, ref int searchMode, string text)
		{
			if (_ToolbarSearchField == null)
			{
				return text;
			}

			return _ToolbarSearchField(id, position, searchModes, ref searchMode, text);
		}

		private static readonly int s_SearchFieldHash = "EditorSearchField".GetHashCode();

		public static string ToolbarSearchField(Rect position, string[] searchModes, ref int searchMode, string text)
		{
			int id = GUIUtility.GetControlID(s_SearchFieldHash, FocusType.Keyboard, position);
			return ToolbarSearchField(id, position, searchModes, ref searchMode, text);
		}

		public static string ToolbarSearchField(Rect position, string text, bool showWithPopupArrow)
		{
			if (_ToolbarSearchField2 == null)
			{
				return SearchField(position, text);
			}

			return _ToolbarSearchField2(position, text, showWithPopupArrow);
		}

		public static string ToolbarSearchField(string text, string[] searchModes, ref int searchMode, params GUILayoutOption[] options)
		{
			if (_ToolbarSearchFieldLayout == null)
			{
				return text;
			}

			return _ToolbarSearchFieldLayout(text, searchModes, ref searchMode, options);
		}

		public static Texture2D FindTexture(System.Type type)
		{
			if (_FindTexture != null)
			{
				Texture2D tex = _FindTexture(type);
				if (tex != null)
				{
					return tex;
				}
			}

			return AssetPreview.GetMiniTypeThumbnail(type);
		}

		private static System.Type GetScriptTypeFromProperty(SerializedProperty property)
		{
			SerializedProperty property1 = property.serializedObject.FindProperty("m_Script");
			if (property1 == null)
			{
				return null;
			}
			MonoScript monoScript = property1.objectReferenceValue as MonoScript;
			if (monoScript == null)
			{
				return null;
			}
			return monoScript.GetClass();
		}

		private sealed class FieldList
		{
			public System.Type type
			{
				get;
				private set;
			}

			internal sealed class Field
			{
				public System.Type fieldType
				{
					get;
					private set;
				}
				public FieldInfo fieldInfo
				{
					get;
					private set;
				}
				public DynamicField dynamicField
				{
					get;
					private set;
				}

				public Field(System.Type type, FieldInfo fieldInfo)
				{
					this.fieldType = type;
					this.fieldInfo = fieldInfo;
					this.dynamicField = DynamicField.GetField(fieldInfo);
				}
			}

			private Dictionary<string, Field> _Fields = new Dictionary<string, Field>();

			public FieldList(System.Type type)
			{
				this.type = type;
			}

			private static readonly System.Text.RegularExpressions.Regex s_ToFieldPathRegex = new System.Text.RegularExpressions.Regex(@"Array.data\[[0-9]*\]");
			private const string kArrayName = "[Array]";

			public static string ToFieldPath(string propertyPath)
			{
				return s_ToFieldPathRegex.Replace(propertyPath, kArrayName);
			}

			private static Field GetFieldInfoFromPropertyPath(System.Type typeFromProperty, string path)
			{
				FieldInfo fieldInfo1 = null;
				System.Type type = typeFromProperty;
				string[] strArray = path.Split('.');
				int length = strArray.Length;
				for (int index2 = 0; index2 < length; ++index2)
				{
					string name = strArray[index2];
					if (name == kArrayName)
					{
						type = SerializationUtility.ElementType(type);
					}
					else
					{
						FieldInfo fieldInfo2 = null;
						for (System.Type type1 = type; fieldInfo2 == null && type1 != null; type1 = type1.BaseType)
						{
							fieldInfo2 = type1.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						}
						if (fieldInfo2 == null)
						{
							type = null;
							return null;
						}
						fieldInfo1 = fieldInfo2;
						type = fieldInfo1.FieldType;
					}
				}

				if (fieldInfo1 == null)
				{
					return null;
				}

				return new Field(type, fieldInfo1);
			}

			public Field GetField(string path)
			{
				Field field = null;
				if (!_Fields.TryGetValue(path, out field))
				{
					field = GetFieldInfoFromPropertyPath(type, path);
					if (field != null)
					{
						_Fields.Add(path, field);
					}
				}

				return field;
			}

			public Field GetField(SerializedProperty property)
			{
				string path = ToFieldPath(property.propertyPath);
				return GetField(path);
			}
		}

		private static Dictionary<System.Type, FieldList> _FieldLists = new Dictionary<System.Type, FieldList>();

		private static FieldList GetFieldList(System.Type typeFromProperty)
		{
			FieldList fieldList = null;
			if (!_FieldLists.TryGetValue(typeFromProperty, out fieldList))
			{
				fieldList = new FieldList(typeFromProperty);
				_FieldLists.Add(typeFromProperty, fieldList);
			}
			return fieldList;
		}

		public static FieldInfo GetFieldInfoFromProperty(SerializedProperty property, out System.Type fieldType)
		{
			System.Type typeFromProperty = GetScriptTypeFromProperty(property);
			if (typeFromProperty != null)
			{
				FieldList fieldList = GetFieldList(typeFromProperty);

				FieldList.Field field = fieldList.GetField(property);
				if (field != null)
				{
					fieldType = field.fieldType;
					return field.fieldInfo;
				}
			}
			fieldType = null;
			return null;
		}

		private static Dictionary<string, GUIContent> _TextContents = new Dictionary<string, GUIContent>();
		public static GUIContent GetTextContent(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return GUIContent.none;
			}

			GUIContent content = null;
			if (!_TextContents.TryGetValue(key, out content))
			{
				content = new GUIContent(key);
				_TextContents.Add(key, content);
			}

			return content;
		}

		private static Dictionary<Object, GUIContent> _ThumbnailContents = new Dictionary<Object, GUIContent>();

#if UNITY_2019_3_OR_NEWER
		[InitializeOnEnterPlayMode]
		static void OnEnterPlayMode()
		{
			_ThumbnailContents.Clear();
		}
#endif

		public static GUIContent GetThumbnailContent(Object obj)
		{
			if (obj == null)
			{
				return GUIContent.none;
			}

			GUIContent content = null;
			if (!_ThumbnailContents.TryGetValue(obj, out content))
			{
				content = new GUIContent(AssetPreview.GetMiniThumbnail(obj));
				_ThumbnailContents.Add(obj, content);
			}

			return content;
		}

		private static Dictionary<string, string> _NicifyVariableNames = new Dictionary<string, string>();
		public static string NicifyVariableName(string name)
		{
			string value;
			if (!_NicifyVariableNames.TryGetValue(name, out value))
			{
				value = ObjectNames.NicifyVariableName(name);
				_NicifyVariableNames.Add(name, value);
			}

			return value;
		}

		public static Color GetColorOnGUI(Color color)
		{
			return color * GUI.color;
		}

		public static void DrawArrow(Vector2 position, Vector2 direction, Color color, float width)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			color = GetColorOnGUI(color);

			Vector2 cross = Vector3.Cross(direction, Vector3.forward).normalized;

			Vector3[] vector3Array = new Vector3[4];
			vector3Array[0] = position;
			vector3Array[1] = position - direction * width + cross * width * 0.5f;
			vector3Array[2] = position - direction * width - cross * width * 0.5f;
			vector3Array[3] = vector3Array[0];

			Shader.SetGlobalColor("_HandleColor", color);
			handleWireMaterial2D.SetPass(0);

			GL.PushMatrix();
			GL.MultMatrix(Handles.matrix);

			GL.Begin(GL.TRIANGLES);
			GL.Color(color);
			GL.Vertex(vector3Array[0]);
			GL.Vertex(vector3Array[1]);
			GL.Vertex(vector3Array[2]);
			GL.End();

			GL.PopMatrix();
		}

		public static void BezierArrow(Bezier2D bezier, Vector2 offset, Texture2D tex, Color color, float width, float arrowWidth)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			Vector2 startPosition = bezier.startPosition + offset;
			Vector2 startControl = bezier.startControl + offset;
			Vector2 endPosition = bezier.endPosition + offset;
			Vector2 endControl = bezier.endControl + offset;

			Vector2 v = (endPosition - endControl).normalized * arrowWidth;

			Handles.DrawBezier(startPosition, endPosition - v, startControl, endControl - v, GetColorOnGUI(color), tex, width);

			DrawArrow(endPosition, v.normalized, color, arrowWidth);
		}

		public static void DrawBezier(Bezier2D bezier, Vector2 offset, Texture2D tex, Color color, float width)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			Vector2 startPosition = bezier.startPosition + offset;
			Vector2 startControl = bezier.startControl + offset;
			Vector2 endPosition = bezier.endPosition + offset;
			Vector2 endControl = bezier.endControl + offset;

			Handles.DrawBezier(startPosition, endPosition, startControl, endControl, GetColorOnGUI(color), tex, width);
		}

		public static void DrawLines(Texture2D tex, Color color, float width, params Vector3[] points)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			Color tempColor = Handles.color;
			Handles.color = GetColorOnGUI(color);
			Handles.DrawAAPolyLine(tex, width, points);
			Handles.color = tempColor;
		}

		public const float kBranchArrowWidth = 16.0f;

		public static void DrawBranch(Bezier2D bezier, Color lineColor, bool shadow, Color shadowColor, float width, bool arrow, float arrowWidth, bool selected)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			Vector2 shadowPos = Vector2.one * 3;

			if (selected)
			{
				Vector2 v = Vector2.zero;
				if (arrow)
				{
					v = (bezier.endPosition - bezier.endControl).normalized * arrowWidth;
				}
				Handles.DrawBezier(bezier.startPosition, bezier.endPosition - v, bezier.startControl, bezier.endControl - v, GetColorOnGUI(Color.cyan), Styles.selectedConnectionTexture, width + 6f);
			}

			if (arrow)
			{
				if (shadow && !selected)
				{
					BezierArrow(bezier, shadowPos, Styles.connectionTexture, shadowColor, width, arrowWidth);
				}
				BezierArrow(bezier, Vector2.zero, Styles.connectionTexture, lineColor, width, arrowWidth);
			}
			else
			{
				if (shadow && !selected)
				{
					DrawBezier(bezier, shadowPos, Styles.connectionTexture, shadowColor, width);
				}
				DrawBezier(bezier, Vector2.zero, Styles.connectionTexture, lineColor, width);
			}
		}

		public static void DrawBranch(Bezier2D bezier, Color lineColor, Color shadowColor, float width, bool arrow, bool selected)
		{
			DrawBranch(bezier, lineColor, true, shadowColor, width, arrow, kBranchArrowWidth, selected);
		}

		const int s_QuadVertexNum = 4;

		public static void DrawSolidQuad(Vector3 center, float radius, float angle, Texture tex)
		{
			using (new ProfilerScope("DrawSolidQuad"))
			{
				Shader.SetGlobalColor("_HandleColor", GetColorOnGUI(Handles.color * new Color(1f, 1f, 1f, 0.5f)));
				Shader.SetGlobalFloat("_HandleSize", 1f);

				Texture oldTex = handleWireMaterial2D.GetTexture("_MainTex");
				handleWireMaterial2D.SetTexture("_MainTex", tex);
				handleWireMaterial2D.SetPass(0);

				GL.PushMatrix();
				GL.MultMatrix(Handles.matrix);
				GL.Begin(GL.QUADS);

				Quaternion rotate = Quaternion.Euler(0, 0, angle);

				GL.Color(GetColorOnGUI(Handles.color));

				GL.TexCoord(new Vector2(0.0f, 1.0f));
				GL.Vertex(center + (rotate * new Vector2(-0.5f, -0.5f)) * radius);

				GL.TexCoord(new Vector2(1.0f, 1.0f));
				GL.Vertex(center + (rotate * new Vector2(0.5f, -0.5f)) * radius);

				GL.TexCoord(new Vector2(1.0f, 0.0f));
				GL.Vertex(center + (rotate * new Vector2(0.5f, 0.5f)) * radius);

				GL.TexCoord(new Vector2(0.0f, 0.0f));
				GL.Vertex(center + (rotate * new Vector2(-0.5f, 0.5f)) * radius);

				GL.End();
				GL.PopMatrix();

				handleWireMaterial2D.SetTexture("_MainTex", oldTex);
			}
		}

		static void GenerateSolidQuad(Vector3 center, float width, float angle, Color color, List<Vector3> vertices, List<Color> colors, List<int> triangles, List<Vector2> texcoords)
		{
			Quaternion rotate = Quaternion.Euler(0, 0, angle);

			int vertCount = vertices.Count;

			vertices.Add(center + (rotate * new Vector2(-0.5f, -0.5f)) * width);
			vertices.Add(center + (rotate * new Vector2(0.5f, -0.5f)) * width);
			vertices.Add(center + (rotate * new Vector2(-0.5f, 0.5f)) * width);
			vertices.Add(center + (rotate * new Vector2(0.5f, 0.5f)) * width);

			texcoords.Add(new Vector2(0.0f, 1.0f));
			texcoords.Add(new Vector2(1.0f, 1.0f));
			texcoords.Add(new Vector2(0.0f, 0.0f));
			texcoords.Add(new Vector2(1.0f, 0.0f));

			colors.Add(color);
			colors.Add(color);
			colors.Add(color);
			colors.Add(color);

			triangles.Add(vertCount + 0);
			triangles.Add(vertCount + 1);
			triangles.Add(vertCount + 2);
			triangles.Add(vertCount + 2);
			triangles.Add(vertCount + 1);
			triangles.Add(vertCount + 3);
		}

		static readonly List<Vector3> s_GenerateMeshVertices = new List<Vector3>();
		static readonly List<Color> s_GenerateMeshColors = new List<Color>();
		static readonly List<int> s_GenerateMeshTriangles = new List<int>();
		static readonly List<Vector2> s_GenerateMeshTexcoords = new List<Vector2>();

		public static void GenerateBezierDottedQuadMesh(Bezier2D bezier, Color startColor, Color endColor, float edgeLength, float space, bool shadow, Vector2 shadowOffset, Color shadowColor, Mesh mesh)
		{
			using (new ProfilerScope("GenerateBezierDottedQuadMesh"))
			{
				mesh.Clear();

				int vertexPerQuad = s_QuadVertexNum;
				int maxVertex = 65000;
				int maxQuadCount = maxVertex / vertexPerQuad;

				float bezierLength = bezier.length;

				int dotCount = (int)(bezierLength / space);
				if (dotCount >= maxQuadCount)
				{
					space = bezierLength / maxQuadCount;
				}

				for (float l = 0.0f; l <= bezierLength; l += space)
				{
					float tl = l / bezierLength;
					float t = bezier.LinearToInterpolationParam(tl);
					Vector2 point = bezier.GetPoint(t);
					Vector2 tangent = bezier.GetTangent(t);

					float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

					if (shadow)
					{
						GenerateSolidQuad(point + shadowOffset, edgeLength, angle, shadowColor, s_GenerateMeshVertices, s_GenerateMeshColors, s_GenerateMeshTriangles, s_GenerateMeshTexcoords);
					}
					GenerateSolidQuad(point, edgeLength, angle, Color.Lerp(startColor, endColor, tl), s_GenerateMeshVertices, s_GenerateMeshColors, s_GenerateMeshTriangles, s_GenerateMeshTexcoords);
				}

				mesh.SetVertices(s_GenerateMeshVertices);
				mesh.SetColors(s_GenerateMeshColors);
				mesh.SetUVs(0, s_GenerateMeshTexcoords);
				mesh.SetTriangles(s_GenerateMeshTriangles, 0);

				s_GenerateMeshVertices.Clear();
				s_GenerateMeshColors.Clear();
				s_GenerateMeshTexcoords.Clear();
				s_GenerateMeshTriangles.Clear();
			}
		}

		static readonly Color s_IntColor = Color.cyan;
		static readonly Color s_BoolColor = Color.red;
		static readonly Color s_StringColor = Color.magenta;
		static readonly Color s_FloatColor = new Color(0.5f, 1, 0.5f);
		static readonly Color s_Vector2Color = new Color(1, 0.5f, 0);
		static readonly Color s_Vector3Color = Color.yellow;
		static readonly Color s_QuaternionColor = new Color(0.5f, 1.0f, 0);
		static readonly Color s_UnityObjectColor = new Color(0, 0.5f, 1);
		static readonly Color s_AnyColor = Color.white;
		static readonly Color s_OtherColor = new Color(0.7f, 0.5f, 1.0f);

		public static Color GetTypeColor(System.Type type)
		{
			if (type != null && TypeUtility.IsGeneric(type, typeof(System.Nullable<>)))
			{
				type = type.GetGenericArguments()[0];
			}

			if (type != null)
			{
				type = DataSlotGUIUtility.ElementType(type);
			}

			if (type == null || type == typeof(object))
			{
				return s_AnyColor;
			}
			else if (type == typeof(int) || type == typeof(long))
			{
				return s_IntColor;
			}
			else if (type == typeof(bool))
			{
				return s_BoolColor;
			}
			else if (type == typeof(string))
			{
				return s_StringColor;
			}
			else if (type == typeof(float))
			{
				return s_FloatColor;
			}
			else if (type == typeof(Vector2))
			{
				return s_Vector2Color;
			}
			else if (type == typeof(Vector3))
			{
				return s_Vector3Color;
			}
			else if (type == typeof(Quaternion))
			{
				return s_QuaternionColor;
			}
			else if (type == typeof(Vector2Int))
			{
				return s_Vector2Color;
			}
			else if (type == typeof(Vector3Int))
			{
				return s_Vector3Color;
			}
			else if (typeof(Object).IsAssignableFrom(type))
			{
				return s_UnityObjectColor;
			}

			return s_OtherColor;
		}

		static readonly Color s_SlotBackgroundDarkColor = new Color(0.25f, 0.25f, 0.25f, 1f);
		static readonly Color s_SlotBackgroundLightColor = Color.white;

		public static Color GetSlotBackgroundColor(Color slotColor, bool isActive, bool on)
		{
			slotColor.a = 1f;

			if (isActive)
			{
				return Color.Lerp(slotColor, s_SlotBackgroundLightColor, 0.8f);
			}

			if (!on)
			{
				return Color.Lerp(slotColor, s_SlotBackgroundDarkColor, 0.5f);
			}

			return slotColor;
		}

		public static void BezierQuad(Bezier2D bezier, Vector2 offset, Color color, float radius, float space, Texture tex)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			float bezierLength = bezier.length;

			Color cachedColor = Handles.color;
			Handles.color = GetColorOnGUI(color);

			for (float l = 0.0f; l <= bezierLength; l += space)
			{
				float tl = l / bezierLength;
				float t = bezier.LinearToInterpolationParam(tl);
				Vector2 pos = bezier.GetPoint(t);
				Vector2 tangent = bezier.GetTangent(t);

				float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

				DrawSolidQuad(pos + offset, radius, angle, tex);
			}

			Handles.color = cachedColor;
		}

		public static void DrawMesh(Mesh mesh, Texture tex)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			using (new ProfilerScope("EditorGUITools.DrawMesh"))
			{
				Shader.SetGlobalColor("_HandleColor", GetColorOnGUI(Handles.color * new Color(1f, 1f, 1f, 0.5f)));
				Shader.SetGlobalFloat("_HandleSize", 1f);

				Texture oldTex = handleWireMaterial2D.GetTexture("_MainTex");
				handleWireMaterial2D.SetTexture("_MainTex", tex);

				handleWireMaterial2D.SetPass(0);

				Graphics.DrawMeshNow(mesh, Handles.matrix);

				handleWireMaterial2D.SetTexture("_MainTex", oldTex);
			}
		}

		public static void DrawGridBackground(Rect position)
		{
			if (Event.current.type == EventType.Repaint)
			{
				Styles.graphBackground.Draw(position, false, false, false, false);
			}
		}

		static readonly Color kSplitColorNormal = new Color(0.6f, 0.6f, 0.6f, 1.333f);
		static readonly Color kSplitColorPro = new Color(0.12f, 0.12f, 0.12f, 1.333f);

		public static Color GetSplitColor(bool isDarkSkin)
		{
			return isDarkSkin ? kSplitColorPro : kSplitColorNormal;
		}

		public static void DrawSeparator(bool isDarkSkin)
		{
			Rect rect = GUILayoutUtility.GetRect(0.0f, 1.0f);

			if (Event.current.type == EventType.Repaint)
			{
				EditorGUI.DrawRect(rect, GetSplitColor(isDarkSkin));
			}
		}

		public static readonly Color gridMinorColor = new Color(0.5f, 0.5f, 0.5f, 0.18f);
		public static readonly Color gridMajorColor = new Color(0.5f, 0.5f, 0.5f, 0.28f);

		public static Rect GUIToScreenRect(Rect guiRect)
		{
			Vector2 vector2 = GUIUtility.GUIToScreenPoint(new Vector2(guiRect.x, guiRect.y));
			guiRect.x = vector2.x;
			guiRect.y = vector2.y;
			return guiRect;
		}

		struct ContextMenuElement
		{
			public string menuItem;
			public System.Action method;
			public int index;
			public int priority;
		}

		struct ContextEditorMenuElement
		{
			public string menuItem;
			public DelegateContextEditorMenu method;
			public DelegateValidContextEditorMenu validateMethod;
			public int index;
			public int priority;
		}

		private sealed class CompareMenuIndex : IComparer<ContextMenuElement>
		{
			public int Compare(ContextMenuElement element1, ContextMenuElement element2)
			{
				if (element1.priority != element2.priority)
					return element1.priority.CompareTo(element2.priority);
				return element1.index.CompareTo(element2.index);
			}
		}

		private sealed class CompareEditorMenuIndex : IComparer<ContextEditorMenuElement>
		{
			public int Compare(ContextEditorMenuElement element1, ContextEditorMenuElement element2)
			{
				if (element1.priority != element2.priority)
					return element1.priority.CompareTo(element2.priority);
				return element1.index.CompareTo(element2.index);
			}
		}

		static ContextEditorMenuElement[] ExtractEditorMenuItem(System.Type behaviourType)
		{
			Dictionary<string, ContextEditorMenuElement> dic = new Dictionary<string, ContextEditorMenuElement>();

			var menuElements = BehaviourMenuItemUtilitty.elements;
			for (int elementIndex = 0; elementIndex < menuElements.Length; elementIndex++)
			{
				BehaviourMenuItemUtilitty.Element element = menuElements[elementIndex];
				if (element.menuItem.type != behaviourType && !behaviourType.IsSubclassOf(element.menuItem.type))
				{
					continue;
				}

				ContextEditorMenuElement menuEelement = dic.ContainsKey(element.menuItem.menuItem) ? dic[element.menuItem.menuItem] : new ContextEditorMenuElement();
				if (element.menuItem.localization)
				{
					menuEelement.menuItem = Localization.GetWord(element.menuItem.menuItem);
				}
				else
				{
					menuEelement.menuItem = element.menuItem.menuItem;
				}
				if (element.menuItem.validate)
				{
					menuEelement.validateMethod = (DelegateValidContextEditorMenu)System.Delegate.CreateDelegate(typeof(DelegateValidContextEditorMenu), element.method, false);
				}
				else
				{
					menuEelement.method = (DelegateContextEditorMenu)System.Delegate.CreateDelegate(typeof(DelegateContextEditorMenu), element.method, false);
					menuEelement.index = element.index;
					menuEelement.priority = element.menuItem.priority;
				}

				dic[element.menuItem.menuItem] = menuEelement;
			}

			ContextEditorMenuElement[] elements = dic.Values.ToArray();
			System.Array.Sort(elements, new CompareEditorMenuIndex());

			return elements;
		}

		static ContextMenuElement[] ExtractContextMenu(Object obj)
		{
			System.Type type = obj.GetType();

			Dictionary<string, ContextMenuElement> dic = new Dictionary<string, ContextMenuElement>();

			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int index = 0; index < methods.Length; ++index)
			{
				MethodInfo method = methods[index];
				var contextMenus = AttributeHelper.GetAttributes<ContextMenu>(method);
				for (int i = 0; i < contextMenus.Length; i++)
				{
					ContextMenu contextMenu = contextMenus[i];
					ContextMenuElement element = dic.ContainsKey(contextMenu.menuItem) ? dic[contextMenu.menuItem] : new ContextMenuElement();
					element.menuItem = contextMenu.menuItem;
					element.method = (System.Action)System.Delegate.CreateDelegate(typeof(System.Action), obj, method, false);
					element.index = 0;
					element.priority = 0;
					dic[contextMenu.menuItem] = element;
				}
			}

			return dic.Values.ToArray();
		}

		static void ExecuteContextMenu(object obj)
		{
			ContextMenuElement contextMenu = (ContextMenuElement)obj;

			if (contextMenu.method != null)
			{
				contextMenu.method();
			}
		}

		static void ExecuteEditorContextMenu(object obj)
		{
			KeyValuePair<MenuCommand, ContextEditorMenuElement> pair = (KeyValuePair<MenuCommand, ContextEditorMenuElement>)obj;
			MenuCommand command = pair.Key;
			ContextEditorMenuElement contextMenu = pair.Value;

			if (contextMenu.method != null)
			{
				contextMenu.method(command);
			}
		}

		public static void AddContextMenu(GenericMenu menu, Object obj)
		{
			if (obj == null)
			{
				return;
			}

			ContextEditorMenuElement[] editorContextMenus = ExtractEditorMenuItem(obj.GetType());
			ContextMenuElement[] contextMenus = ExtractContextMenu(obj);

			if (editorContextMenus.Length > 0 || contextMenus.Length > 0)
			{
				menu.AddSeparator("");
				if (editorContextMenus.Length > 0)
				{
					MenuCommand command = new MenuCommand(obj);
					for (int elementIndex = 0; elementIndex < editorContextMenus.Length; elementIndex++)
					{
						ContextEditorMenuElement element = editorContextMenus[elementIndex];
						if (element.method == null)
						{
							continue;
						}

						bool enable = true;
						if (element.validateMethod != null)
						{
							enable = element.validateMethod(command);
						}
						if (enable)
						{
							menu.AddItem(EditorGUITools.GetTextContent(element.menuItem), false, ExecuteEditorContextMenu, new KeyValuePair<MenuCommand, ContextEditorMenuElement>(command, element));
						}
						else
						{
							menu.AddDisabledItem(EditorGUITools.GetTextContent(element.menuItem));
						}
					}
				}
				if (contextMenus.Length > 0)
				{
					for (int menuIndex = 0; menuIndex < contextMenus.Length; menuIndex++)
					{
						ContextMenuElement element = contextMenus[menuIndex];
						if (element.method != null)
						{
							menu.AddItem(GetTextContent(element.menuItem), false, ExecuteContextMenu, element);
						}
					}
				}
			}
		}

		public const float kStateBezierTargetOffsetY = 16.0f;
		public const float kBezierTangent = 50f;
		public static readonly Vector2 kBezierTangentOffset = new Vector2(kBezierTangent, 0.0f);

		public static void HelpButton(Rect position, string url, string tooltip, GUIStyle style)
		{
			s_HelpButtonContent.tooltip = tooltip;

			if (GUI.Button(position, s_HelpButtonContent, style))
			{
				Help.BrowseURL(url);
			}
		}

		public static void HelpButton(Rect position, string url, string tooltip)
		{
			HelpButton(position, url, tooltip, Styles.iconButton);
		}

		public static bool HasHelpButton(Object obj)
		{
			BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(obj);

			return !string.IsNullOrEmpty(behaviourInfo.helpUrl) || Help.HasHelpForObject(obj);
		}

		public static bool HelpButton(Rect position, Object obj, GUIStyle style)
		{
			BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(obj);

			string url = behaviourInfo.helpUrl;
			string tooltip = behaviourInfo.helpTooltip;

			if (string.IsNullOrEmpty(url) && Help.HasHelpForObject(obj))
			{
				url = Help.GetHelpURLForObject(obj);
				string helpTopic = _GetNiceHelpNameForObject != null ? _GetNiceHelpNameForObject(obj) : obj.GetType().Name;
				tooltip = string.Format("Open Reference for {0}.", helpTopic);
			}

			if (!string.IsNullOrEmpty(url))
			{
				HelpButton(position, url, tooltip, style);

				return true;
			}

			return false;
		}

		public static bool HelpButton(Rect position, Object obj)
		{
			return HelpButton(position, obj, Styles.iconButton);
		}

		public static bool HelpButton(Rect position, System.Type type, GUIStyle style)
		{
			BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(type);

			string url = behaviourInfo.helpUrl;
			string tooltip = behaviourInfo.helpTooltip;

			if (!string.IsNullOrEmpty(url))
			{
				HelpButton(position, url, tooltip, style);

				return true;
			}

			return false;
		}

		public static bool HelpButton(Rect position, System.Type type)
		{
			return HelpButton(position, type, Styles.iconButton);
		}

		public static Rect GetDropdownRect(Rect position)
		{
			position.xMin = position.xMax - kDropDownWidth;
			position.height = EditorGUIUtility.singleLineHeight;
			return position;
		}

		public static Rect SubtractDropdownWidth(Rect position)
		{
			position.width -= kSubtractDropdownWidth;
			return position;
		}

		public static Rect GetPopupRect(Rect position)
		{
			position.xMin = position.xMax - kPopupWidth;
			position.height = EditorGUIUtility.singleLineHeight;
			return position;
		}

		public static Rect SubtractPopupWidth(Rect position)
		{
			position.width -= kSubtrackPopupWidth;
			return position;
		}

		public static Rect PrefixLabel(Rect totalPosition, GUIContent label)
		{
			Rect labelPosition = EditorGUI.IndentedRect(new Rect(totalPosition.x, totalPosition.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight));
			Rect rect = new Rect(totalPosition.x + EditorGUIUtility.labelWidth, totalPosition.y, totalPosition.width - EditorGUIUtility.labelWidth, totalPosition.height);
			EditorGUI.HandlePrefixLabel(totalPosition, labelPosition, label, 0, EditorStyles.label);
			return rect;
		}

		static System.Text.StringBuilder s_ObjectPathBuilder = new System.Text.StringBuilder(1000);
		const string kPropertyArrayPrefix = ".Array.data[";

		static string GetPropertyPath(SerializedProperty property)
		{
			string path = property.propertyPath;
			if (path.Contains(kPropertyArrayPrefix))
			{
				s_ObjectPathBuilder.Length = 0;
				s_ObjectPathBuilder.Append(path);

				s_ObjectPathBuilder.Replace(kPropertyArrayPrefix, "[");
				path = s_ObjectPathBuilder.ToString();
			}
			return path;
		}

		private static object GetValue_Imp(object source, string name)
		{
			if (source == null)
			{
				return null;
			}

			System.Type type = source.GetType();

			FieldList fieldList = GetFieldList(type);

			FieldList.Field field = fieldList.GetField(name);
			if (field != null)
			{
				return field.dynamicField.GetValue(source);
			}

			return null;
		}

		private static object GetValue_Imp(object source, string name, int index)
		{
			IList list = GetValue_Imp(source, name) as IList;
			if (list == null)
			{
				return null;
			}

			return list[index];
		}

		public static object GetPropertyObject(SerializedProperty property)
		{
			object obj = property.serializedObject.targetObject;

			string path = GetPropertyPath(property);

			string[] elements = path.Split('.');

			int length = elements.Length;
			for (int i = 0; i < length; i++)
			{
				string element = elements[i];

				if (element.Contains('['))
				{
					var array = element.Split('[', ']');
					var elementName = array[0];
					var index = System.Convert.ToInt32(array[1]);
					obj = GetValue_Imp(obj, elementName, index);
				}
				else
				{
					obj = GetValue_Imp(obj, element);
				}
			}
			return obj;
		}

		public static T GetPropertyObject<T>(SerializedProperty property)
		{
			return (T)GetPropertyObject(property);
		}

		public static IEnumerable<object> GetPropertyObjects(SerializedProperty property)
		{
			string path = GetPropertyPath(property);

			string[] elements = path.Split('.');

			int length = elements.Length;

			var targetObjects = property.serializedObject.targetObjects;
			for (int targetObjectIndex = 0; targetObjectIndex < targetObjects.Length; targetObjectIndex++)
			{
				Object targetObj = targetObjects[targetObjectIndex];
				object obj = targetObj;

				for (int elementIndex = 0; elementIndex < length; elementIndex++)
				{
					string element = elements[elementIndex];

					if (element.Contains('['))
					{
						var array = element.Split('[', ']');
						var elementName = array[0];
						var index = System.Convert.ToInt32(array[1]);
						obj = GetValue_Imp(obj, elementName, index);
					}
					else
					{
						obj = GetValue_Imp(obj, element);
					}
				}
				yield return obj;
			}
		}

		public static Rect FromToRect(Vector2 start, Vector2 end)
		{
			Rect rect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
			if (rect.width < 0.0f)
			{
				rect.x += rect.width;
				rect.width = -rect.width;
			}
			if (rect.height < 0.0f)
			{
				rect.y += rect.height;
				rect.height = -rect.height;
			}
			return rect;
		}

		public static MonoScript GetMonoScript(Object obj)
		{
			MonoBehaviour monoBehaviour = obj as MonoBehaviour;
			if (monoBehaviour != null)
			{
				return MonoScript.FromMonoBehaviour(monoBehaviour);
			}
			ScriptableObject scriptableObject = obj as ScriptableObject;
			if (scriptableObject != null)
			{
				return MonoScript.FromScriptableObject(scriptableObject);
			}

			return null;
		}

		public static bool LabelHasContent(GUIContent label)
		{
			if (label == null || label.text != string.Empty)
				return true;
			return (UnityEngine.Object)label.image != (UnityEngine.Object)null;
		}

		static float kLabelFloatMinW
		{
			get
			{
				return EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth;
			}
		}

		internal static float kLabelFloatMaxW
		{
			get
			{
				return EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth;
			}
		}

		public static Rect GetControlRect(bool hasLabel, float height, GUIStyle style, params GUILayoutOption[] options)
		{
			return GUILayoutUtility.GetRect(!hasLabel ? EditorGUIUtility.fieldWidth : kLabelFloatMinW, kLabelFloatMaxW, height, height, style, options);
		}

		public static Rect MultiFieldPrefixLabel(Rect totalPosition, int id, GUIContent label, int columns, GUIStyle style, out Rect labelPosition)
		{
			if (columns == 0)
			{
				labelPosition = totalPosition;
				labelPosition.height = 16f;
				Rect rect = totalPosition;
				rect.xMin = totalPosition.xMax;
				Vector2 size = style.CalcSize(label);
				labelPosition.width = size.x;
				return rect;
			}
			if (columns == 1 || EditorGUIUtility.wideMode)
			{
				labelPosition = new Rect(totalPosition.x, totalPosition.y, EditorGUIUtility.labelWidth, 16f);
				Rect rect = totalPosition;
				rect.xMin += EditorGUIUtility.labelWidth;
				if (columns > 1)
				{
					--labelPosition.width;
					--rect.xMin;
				}
				if (columns == 2)
				{
					float num = (float)(((double)rect.width - 4.0) / 3.0);
					rect.xMax -= num + 2f;
				}
				return rect;
			}
			labelPosition = new Rect(totalPosition.x, totalPosition.y, totalPosition.width, 16f);
			Rect rect1 = EditorGUI.IndentedRect(totalPosition);
			rect1.xMin += 15f;
			rect1.yMin += 16f;
			return rect1;
		}

		public static void DrawIndicator(Rect position, string text)
		{
			int index = (int)Mathf.Repeat(Time.realtimeSinceStartup * 10f, 11.99f);
			GUIStyle style = EditorStyles.largeLabel;
			GUIContent content = s_WaitSpins[index];
			content.text = text;

			Vector2 size = style.CalcSize(content);

			Vector2 min = position.center - size * 0.5f;
			Vector2 max = position.center + size * 0.5f;
			position.min = min;
			position.max = max;

			GUI.Label(position, content, style);
		}

		public static string TextArea(Rect position, string text, int controlId, GUIStyle style)
		{
			bool changed = false;
			text = (string)_DoTextFieldMethod.Invoke(null, new object[] { _RecycledEditorField.GetValue(null), controlId, EditorGUI.IndentedRect(position), text, style, (string)null, changed, false, true, false });
			return text;
		}

		public static float SnapToGrid(float x)
		{
			if (ArborSettings.showGrid && ArborSettings.snapGrid)
			{
				float gridSizeMinor = ArborSettings.gridSize / (float)ArborSettings.gridSplitNum;
				int num1 = Mathf.FloorToInt(x / gridSizeMinor);
				x = num1 * gridSizeMinor;
			}
			return x;
		}

		public static Vector2 SnapToGrid(Vector2 position)
		{
			if (ArborSettings.showGrid && ArborSettings.snapGrid)
			{
				float gridSizeMinor = ArborSettings.gridSize / (float)ArborSettings.gridSplitNum;
				int num1 = Mathf.FloorToInt(position.x / gridSizeMinor);
				int num2 = Mathf.FloorToInt(position.y / gridSizeMinor);
				position.x = num1 * gridSizeMinor;
				position.y = num2 * gridSizeMinor;
			}
			return position;
		}

		public static float GetSnapSpace()
		{
			float space = 10f;
			if (ArborSettings.showGrid && ArborSettings.snapGrid)
			{
				space = ArborSettings.gridSize / (float)ArborSettings.gridSplitNum;
			}

			return space;
		}

		public static Rect SnapPositionToGrid(Rect position)
		{
			position.position = SnapToGrid(position.position);
			return position;
		}

		public static void ClearPropertyDrawerCache()
		{
			_DelegateClearGlobalCache();
		}

		public static bool ShouldRethrowException(System.Exception exception)
		{
			while (exception is TargetInvocationException && exception.InnerException != null)
				exception = exception.InnerException;
			return exception is ExitGUIException;
		}

		private static string StringPopupInternal(Rect position, GUIContent label, string selectedValue, GUIContent[] displayedOptions, string[] optionValues)
		{
			int selectedIndex = -1;
			if (optionValues != null)
			{
				selectedIndex = 0;
				while (selectedIndex < optionValues.Length && selectedValue != optionValues[selectedIndex])
					++selectedIndex;
			}
			int index = EditorGUI.Popup(position, label, selectedIndex, displayedOptions);
			if (optionValues == null)
				return string.Empty;
			if (index < 0 || index >= optionValues.Length)
				return selectedValue;
			return optionValues[index];
		}

		static void StringPopup(Rect position, SerializedProperty property, GUIContent[] displayedOptions, string[] optionValues, GUIContent label)
		{
			label = EditorGUI.BeginProperty(position, label, property);
			EditorGUI.BeginChangeCheck();
			string value = StringPopupInternal(position, label, property.stringValue, displayedOptions, optionValues);
			if (EditorGUI.EndChangeCheck())
			{
				property.stringValue = value;
			}
			EditorGUI.EndProperty();
		}

		static void StringPopup(SerializedProperty property, GUIContent[] displayedOptions, string[] optionValues, GUIContent label)
		{
			StringPopup(EditorGUILayout.GetControlRect(true, 16f, EditorStyles.popup), property, displayedOptions, optionValues, label);
		}

		static int GetLayerIndex(Animator animator, string layerName)
		{
			AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
			if (animatorController == null)
			{
				return -1;
			}
			AnimatorControllerLayer[] layers = animatorController.layers;

			int layerCount = layers.Length;

			for (int i = 0; i < layerCount; i++)
			{
				AnimatorControllerLayer layer = layers[i];

				if (layer.name == layerName)
				{
					return i;
				}
			}

			return -1;
		}

		public static AnimatorControllerLayer GetAnimatorLayer(Animator animator, string layerName)
		{
			if (animator == null)
			{
				return null;
			}

			AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
			if (animatorController == null)
			{
				return null;
			}
			AnimatorControllerLayer[] layers = animatorController.layers;

			int layerIndex = GetLayerIndex(animator, layerName);

			return (layerIndex >= 0) ? layers[layerIndex] : null;
		}

		public static AnimatorControllerLayer AnimatorLayerField(Rect position, Animator animator, SerializedProperty layerNameProperty, GUIContent label)
		{
			if (animator == null || animator.runtimeAnimatorController == null)
			{
				layerNameProperty.stringValue = string.Empty;
				return null;
			}

			AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
			AnimatorControllerLayer[] layers = animatorController.layers;

			int layerCount = layers.Length;

			string[] layerNames = new string[layerCount];
			GUIContent[] layerDisplayed = new GUIContent[layerCount];

			for (int i = 0; i < layerCount; i++)
			{
				AnimatorControllerLayer layer = layers[i];

				string name = layer.name;

				layerNames[i] = name;
				layerDisplayed[i] = new GUIContent(name);
			}

			StringPopup(position, layerNameProperty, layerDisplayed, layerNames, label);

			int layerIndex = GetLayerIndex(animator, layerNameProperty.stringValue);

			AnimatorControllerLayer selectedLayer = (layerIndex >= 0) ? layers[layerIndex] : null;

			if (selectedLayer != null)
			{
				layerNameProperty.stringValue = selectedLayer.name;
			}
			else
			{
				layerNameProperty.stringValue = string.Empty;
			}

			return selectedLayer;
		}

		public static AnimatorControllerLayer AnimatorLayerField(Animator animator, SerializedProperty layerNameProperty, GUIContent label)
		{
			if (animator == null || animator.runtimeAnimatorController == null)
			{
				layerNameProperty.stringValue = string.Empty;
				return null;
			}

			return AnimatorLayerField(EditorGUILayout.GetControlRect(true, 16f, EditorStyles.popup), animator, layerNameProperty, label);
		}

		public static void AnimatorLayerField(Animator animator, FlexibleFieldProperty layerNameProperty)
		{
			layerNameProperty.property.SetStateData<OnFlexibleConstantGUI>((fieldPosition, valueProperty, label) =>
			{
				if (animator == null)
				{
					return false;
				}
				AnimatorLayerField(fieldPosition, animator, valueProperty, label);
				return true;
			});
			EditorGUILayout.PropertyField(layerNameProperty.property);

			layerNameProperty.property.RemoveStateData<OnFlexibleConstantGUI>();
		}

		public static void AnimatorStateField(Rect position, Animator animator, AnimatorControllerLayer layer, SerializedProperty stateNameProperty, GUIContent label)
		{
			if (layer == null)
			{
				return;
			}

			ChildAnimatorState[] states = layer.stateMachine.states;

			int stateCount = states.Length;

			string[] stateNames = new string[stateCount];
			GUIContent[] stateDisplayed = new GUIContent[stateCount];

			for (int i = 0; i < stateCount; i++)
			{
				AnimatorState state = states[i].state;

				string stateName = state.name;

				stateNames[i] = stateName;
				stateDisplayed[i] = new GUIContent(stateName);
			}

			StringPopup(position, stateNameProperty, stateDisplayed, stateNames, label);
		}

		public static void AnimatorStateField(Animator animator, AnimatorControllerLayer layer, SerializedProperty stateNameProperty, GUIContent label)
		{
			if (layer == null)
			{
				return;
			}

			AnimatorStateField(EditorGUILayout.GetControlRect(true, 16f, EditorStyles.popup), animator, layer, stateNameProperty, label);
		}

		public static void AnimatorStateField(Animator animator, SerializedProperty layerNameProperty, SerializedProperty stateNameProperty)
		{
			AnimatorControllerLayer layer = AnimatorLayerField(animator, layerNameProperty, null);

			AnimatorStateField(animator, layer, stateNameProperty, null);
		}

		public static void AnimatorStateField(Animator animator, FlexibleFieldProperty layerNameProperty, FlexibleFieldProperty stateNameProperty)
		{
			AnimatorLayerField(animator, layerNameProperty);

			if (layerNameProperty.type == FlexibleType.Constant)
			{
				AnimatorControllerLayer layer = GetAnimatorLayer(animator, layerNameProperty.valueProperty.stringValue);

				stateNameProperty.property.SetStateData<OnFlexibleConstantGUI>((fieldPosition, valueProperty, label) =>
				{
					if (animator == null || layer == null)
					{
						return false;
					}
					AnimatorStateField(fieldPosition, animator, layer, valueProperty, label);
					return true;
				});
			}
			EditorGUILayout.PropertyField(stateNameProperty.property);

			stateNameProperty.property.RemoveStateData<OnFlexibleConstantGUI>();
		}

		private sealed class AnimatorParameters
		{
			public GUIContent[] displayNames;
			public string[] names;
			public AnimatorControllerParameterType[] types;
			public int selected;

			public Dictionary<AnimatorControllerParameterType, AnimatorParameters> parametersByType = new Dictionary<AnimatorControllerParameterType, AnimatorParameters>();

			public AnimatorParameters GetTypeParameter(AnimatorControllerParameterType type)
			{
				AnimatorParameters results = null;
				if (parametersByType.TryGetValue(type, out results))
				{
					return results;
				}
				return null;
			}

			public bool Update(AnimatorController animatorController, string name)
			{
				AnimatorControllerParameter[] animParames = animatorController.parameters;

				int parameterCount = (animParames != null) ? animParames.Length + 1 : 0;
				if (parameterCount > 0)
				{
					selected = -1;

					if (names == null || names.Length != parameterCount)
					{
						names = new string[parameterCount];
					}
					if (displayNames == null || displayNames.Length != parameterCount)
					{
						displayNames = new GUIContent[parameterCount];
					}
					if (this.types == null || this.types.Length != parameterCount)
					{
						this.types = new AnimatorControllerParameterType[parameterCount];
					}

					names[0] = string.Empty;
					displayNames[0] = EditorGUITools.GetTextContent("[None]");
					this.types[0] = AnimatorControllerParameterType.Float;

					Dictionary<AnimatorControllerParameterType, List<int>> typeIndexes = new Dictionary<AnimatorControllerParameterType, List<int>>();

					for (int paramIndex = 1; paramIndex < parameterCount; paramIndex++)
					{
						AnimatorControllerParameter parameter = animParames[paramIndex - 1];

						string parameterName = parameter.name;

						displayNames[paramIndex] = new GUIContent(parameterName);
						names[paramIndex] = parameterName;
						types[paramIndex] = parameter.type;

						List<int> indexes = null;
						if (!typeIndexes.TryGetValue(parameter.type, out indexes))
						{
							indexes = new List<int>();
							typeIndexes.Add(parameter.type, indexes);
						}
						indexes.Add(paramIndex);

						if (parameterName == name)
						{
							selected = paramIndex;
						}
					}

					parametersByType.Clear();

					foreach (KeyValuePair<AnimatorControllerParameterType, List<int>> pair in typeIndexes)
					{
						AnimatorParameters p = new AnimatorParameters();
						List<int> indexes = pair.Value;

						p.selected = 0;

						p.displayNames = new GUIContent[indexes.Count + 1];
						p.names = new string[indexes.Count + 1];
						p.types = new AnimatorControllerParameterType[indexes.Count + 1];

						p.displayNames[0] = EditorGUITools.GetTextContent("[None]");
						p.names[0] = string.Empty;
						p.types[0] = AnimatorControllerParameterType.Float;

						int count = 1;
						for (int i = 0; i < indexes.Count; i++)
						{
							int index = indexes[i];
							p.displayNames[count] = displayNames[index];
							p.names[count] = names[index];
							p.types[count] = types[index];
							if (index == selected)
							{
								p.selected = count;
							}
							count++;
						}

						parametersByType.Add(pair.Key, p);
					}

					return true;
				}

				return false;
			}

			public void Popup(Rect position, SerializedProperty nameProperty, SerializedProperty typeProperty, GUIContent label)
			{
				label = EditorGUI.BeginProperty(position, label, nameProperty);
				EditorGUI.BeginChangeCheck();
				selected = EditorGUI.Popup(position, label, selected, displayNames);
				if (EditorGUI.EndChangeCheck())
				{
					if (selected >= 0)
					{
						nameProperty.stringValue = names[selected];
						if (typeProperty != null)
						{
							typeProperty.enumValueIndex = EnumUtility.GetIndexFromValue(types[selected]);
						}
					}
				}
				EditorGUI.EndProperty();
			}
		}
		private static readonly Dictionary<AnimatorController, AnimatorParameters> _Parameters = new Dictionary<AnimatorController, AnimatorParameters>();

		private static AnimatorParameters GetAnimatorParameters(AnimatorController animatorController)
		{
			AnimatorParameters parameters;
			if (!_Parameters.TryGetValue(animatorController, out parameters))
			{
				parameters = new AnimatorParameters();
				_Parameters.Add(animatorController, parameters);
			}

			return parameters;
		}

		public static void AnimatorParameterField(Rect position, Animator animator, SerializedProperty nameProperty, SerializedProperty typeProperty, GUIContent label, bool hasType = false, AnimatorControllerParameterType parameterType = AnimatorControllerParameterType.Bool)
		{
			AnimatorParameters parameters = null;

			if (animator != null && animator.runtimeAnimatorController != null)
			{
				AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;

				string name = nameProperty.stringValue;

				parameters = GetAnimatorParameters(animatorController);

				if (parameters.Update(animatorController, name))
				{
					if (hasType)
					{
						parameters = parameters.GetTypeParameter(parameterType);
					}
				}
				else
				{
					parameters = null;
				}

				if (parameters != null)
				{
					parameters.Popup(position, nameProperty, typeProperty, label);
				}
				else
				{
					EditorGUI.BeginDisabledGroup(true);

					label = EditorGUI.BeginProperty(position, label, nameProperty);

					EditorGUI.Popup(position, label, -1, new GUIContent[] { GUIContent.none });

					EditorGUI.EndProperty();

					EditorGUI.EndDisabledGroup();
				}
			}
			else
			{
				position.height = EditorGUI.GetPropertyHeight(nameProperty);
				EditorGUI.PropertyField(position, nameProperty, true);
				position.y += position.height;

				if (typeProperty != null)
				{
					if (hasType)
					{
						typeProperty.enumValueIndex = EnumUtility.GetIndexFromValue(parameterType);
					}
					else
					{
						position.y += EditorGUIUtility.standardVerticalSpacing;
						position.height = EditorGUI.GetPropertyHeight(typeProperty);
						EditorGUI.PropertyField(position, typeProperty);
						position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
					}
				}
			}
		}

		public static float GetAnimatorParameterFieldHeight(Animator animator, SerializedProperty nameProperty, SerializedProperty typeProperty, bool hasType = false)
		{
			float height = 0.0f;

			if (animator != null && animator.runtimeAnimatorController != null)
			{
				height = EditorGUIUtility.singleLineHeight;
			}
			else
			{
				height += EditorGUI.GetPropertyHeight(nameProperty);

				if (typeProperty != null)
				{
					if (!hasType)
					{
						height += EditorGUIUtility.standardVerticalSpacing;

						height += EditorGUI.GetPropertyHeight(typeProperty);
					}
				}
			}

			return height;
		}

		private static Rect GetAnimatorParameterFieldLayoutRect(Animator animator, SerializedProperty nameProperty, SerializedProperty typeProperty, bool hasType = false)
		{
			float height = GetAnimatorParameterFieldHeight(animator, nameProperty, typeProperty, hasType);

			if (animator != null && animator.runtimeAnimatorController != null)
			{
				return EditorGUILayout.GetControlRect(true, height, EditorStyles.popup);
			}
			else
			{
				return GUILayoutUtility.GetRect(0, height);
			}
		}

		public static void AnimatorParameterField(Animator animator, SerializedProperty nameProperty, SerializedProperty typeProperty, GUIContent label)
		{
			Rect position = GetAnimatorParameterFieldLayoutRect(animator, nameProperty, typeProperty);

			AnimatorParameterField(position, animator, nameProperty, typeProperty, label);
		}

		public static void AnimatorParameterField(Animator animator, SerializedProperty nameProperty, SerializedProperty typeProperty, GUIContent label, AnimatorControllerParameterType parameterType)
		{
			Rect position = GetAnimatorParameterFieldLayoutRect(animator, nameProperty, typeProperty, true);

			AnimatorParameterField(position, animator, nameProperty, typeProperty, label, true, parameterType);
		}

		public static string GetActiveFolderPath()
		{
			return _DelegateGetActiveFolderPath();
		}

		private static void DrawLine(Vector2 p1, Vector2 p2)
		{
			GL.Vertex((Vector3)p1);
			GL.Vertex((Vector3)p2);
		}

		private static void DrawGridLines(Rect rect, float gridSize, Color gridColor)
		{
			GL.Color(gridColor);

			float x = rect.xMin - rect.xMin % gridSize;
			while (x < rect.xMax)
			{
				DrawLine(new Vector2(x, rect.yMin), new Vector2(x, rect.yMax));
				x += gridSize;
			}
			GL.Color(gridColor);
			float y = rect.yMin - rect.yMin % gridSize;
			while (y < rect.yMax)
			{
				DrawLine(new Vector2(rect.xMin, y), new Vector2(rect.xMax, y));
				y += gridSize;
			}
		}

		public static void DrawGrid(Rect rect, float zoomLevel, float gridSize, int gridSplitNum)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}

			using (new ProfilerScope("DrawGrid"))
			{
				handleWireMaterial2D.SetPass(0);
				GL.PushMatrix();
				GL.Begin(GL.LINES);

				float t = Mathf.InverseLerp(0.1f, 1, zoomLevel);

				if (gridSplitNum > 1 && t > 0f)
				{
					DrawGridLines(rect, gridSize / (float)gridSplitNum, Color.Lerp(Color.clear, gridMinorColor, t));
				}
				DrawGridLines(rect, gridSize, Color.Lerp(gridMinorColor, gridMajorColor, t));

				GL.End();
				GL.PopMatrix();
			}
		}

		public static void DrawGrid(Rect rect, float zoomLevel)
		{
			DrawGrid(rect, zoomLevel, ArborSettings.gridSize, ArborSettings.gridSplitNum);
		}

		public static void OpenAssetStore(string storeURL)
		{
			storeURL = storeURL.Substring(storeURL.IndexOf("content", System.StringComparison.Ordinal));
			UnityEditorInternal.AssetStore.Open(storeURL);
		}

		public static TEnum EnumPopup<TEnum>(Rect position, GUIContent label, TEnum selected, GUIStyle style) where TEnum : System.Enum
		{
			var enumType = typeof(TEnum);

			if (!enumType.IsEnum)
			{
				throw new System.ArgumentException("Parameter selected must be of type System.Enum", "selected");
			}

			TEnum[] values = EnumUtility.GetValues<TEnum>();
			int selectedIndex = EnumUtility.GetIndexFromValue<TEnum>(selected);
			GUIContent[] displayOptions = EnumUtility.GetContents<TEnum>();

			selectedIndex = EditorGUI.Popup(position, label, selectedIndex, displayOptions, style);
			if (0 <= selectedIndex && selectedIndex < values.Length)
			{
				selected = EnumUtility.GetValueFromIndex<TEnum>(selectedIndex);
			}

			return selected;
		}

		public static TEnum EnumPopupUnIndent<TEnum>(Rect position, GUIContent label, TEnum selected, GUIStyle style) where TEnum : System.Enum
		{
			int tempIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			selected = EnumPopup(position, label, selected, style);

			EditorGUI.indentLevel = tempIndentLevel;
			return selected;
		}

		public static GameObject SaveAsPrefabAsset(GameObject instanceRoot, string assetPath)
		{
			return PrefabUtility.SaveAsPrefabAsset(instanceRoot, assetPath);
		}

		private static readonly int s_CircleButtonHash = "s_CircleButtonHash".GetHashCode();

		public static bool CircleButton(GUIContent iconContent, params GUILayoutOption[] options)
		{
			bool buttonDown = false;
			GUIStyle style = Styles.circleButton;
			Rect position = GUILayoutUtility.GetRect(iconContent, style, options);

			float radius = position.width * 0.5f;

			int controlID = GUIUtility.GetControlID(s_CircleButtonHash, FocusType.Passive, position);

			Event evt = Event.current;

			bool isHover = Vector2.Distance(position.center, evt.mousePosition) <= radius;

			switch (evt.GetTypeForControl(controlID))
			{
				case EventType.MouseDown:
					if (evt.button == 0 && isHover)
					{
						GUIUtility.hotControl = controlID;
						evt.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						if (evt.button == 0)
						{
							if (isHover)
							{
								buttonDown = true;
							}

							GUIUtility.hotControl = 0;
						}
						evt.Use();
					}
					break;
				case EventType.Repaint:
					{
						style.Draw(position, iconContent, isHover || GUIUtility.hotControl == controlID, GUIUtility.hotControl == controlID, false, false);
					}
					break;
			}

			return buttonDown;
		}

		public static bool IconButton(GUIContent content, out Rect buttonPosition)
		{
			buttonPosition = GUILayoutUtility.GetRect(content, Styles.iconButton);

			bool button = GUI.Button(buttonPosition, content, Styles.iconButton);

			return button;
		}

		public static bool IconButton(GUIContent content)
		{
			Rect buttonPosition;
			return IconButton(content, out buttonPosition);
		}

		public static bool VisibilityToggle(Rect position, GUIContent label, bool toggle)
		{
			Rect toggleRect = position;
			toggleRect.width = Styles.visibilityToggle.fixedWidth;

			toggle = EditorGUI.Toggle(toggleRect, GUIContent.none, toggle, Styles.visibilityToggle);

			Rect setLabelRect = position;
			setLabelRect.xMin = toggleRect.xMax + 2f;

			EditorGUI.BeginDisabledGroup(!toggle);

			EditorGUI.LabelField(setLabelRect, label);

			EditorGUI.EndDisabledGroup();

			return toggle;
		}

		public static void TagField(SerializedProperty property, params GUILayoutOption[] options)
		{
			TagField(property, null, EditorStyles.popup, options);
		}

		public static void TagField(SerializedProperty property, GUIContent label, params GUILayoutOption[] options)
		{
			TagField(property, label, EditorStyles.popup, options);
		}

		public static void TagField(SerializedProperty property, GUIContent label, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, style, options);

			TagField(rect, property, label, style);
		}

		public static void TagField(Rect rect, SerializedProperty property)
		{
			TagField(rect, property, null, EditorStyles.popup);
		}

		public static void TagField(Rect rect, SerializedProperty property, GUIContent label)
		{
			TagField(rect, property, label, EditorStyles.popup);
		}

		public static void TagField(Rect rect, SerializedProperty property, GUIContent label, GUIStyle style)
		{
			label = EditorGUI.BeginProperty(rect, label, property);

			EditorGUI.BeginChangeCheck();
			string tag = EditorGUI.TagField(rect, label, property.stringValue, style);
			if (EditorGUI.EndChangeCheck())
			{
				property.stringValue = tag;
			}

			EditorGUI.EndProperty();
		}

		public static bool IsEditorEnabled(Editor editor)
		{
			return editor != null && (bool)_EditorIsEnabledMethod.Invoke(editor, null);
		}

		public static bool ButtonForceEnabled(string text, params GUILayoutOption[] options)
		{
			return ButtonForceEnabled(text, GUI.skin.button, options);
		}

		public static bool ButtonForceEnabled(string text, GUIStyle style, params GUILayoutOption[] options)
		{
			bool guiEnabled = GUI.enabled;
			GUI.enabled = true;

			bool button = GUILayout.Button(text, style, options);

			GUI.enabled = guiEnabled;

			return button;
		}

		static long RoundToLong(float f)
		{
			return (long)System.Math.Round(f);
		}

		public static long LongSlider(Rect position, long value, long leftValue, long rightValue, GUIContent label)
		{
			return RoundToLong(EditorGUI.Slider(position, label, value, leftValue, rightValue));
		}

		public static void LongSlider(Rect position, SerializedProperty property, long leftValue, long rightValue, GUIContent label)
		{
			label = EditorGUI.BeginProperty(position, label, property);

			EditorGUI.BeginChangeCheck();

			long newValue = LongSlider(position, property.longValue, leftValue, rightValue, label);

			if (EditorGUI.EndChangeCheck())
			{
				property.longValue = newValue;
			}

			EditorGUI.EndProperty();
		}

		public static float CalcLabelWidth(float contextWidth)
		{
			return Mathf.Max(contextWidth * 0.45f - 40, 120);
		}
	}
}
