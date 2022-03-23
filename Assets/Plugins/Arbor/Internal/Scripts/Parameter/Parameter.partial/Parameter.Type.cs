//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace Arbor
{
	public sealed partial class Parameter
	{
#if ARBOR_DOC_JA
		/// <summary>
		/// パラメータの型。
		/// </summary>
#else
		/// <summary>
		/// Parameter type.
		/// </summary>
#endif
		public enum Type
		{
#if ARBOR_DOC_JA
			/// <summary>
			/// Int型。
			/// </summary>
#else
			/// <summary>
			/// Int type.
			/// </summary>
#endif
			[ParameterValueType(typeof(int))]
			Int,

#if ARBOR_DOC_JA
			/// <summary>
			/// Float型。
			/// </summary>
#else
			/// <summary>
			/// Float type.
			/// </summary>
#endif
			[ParameterValueType(typeof(float))]
			Float,

#if ARBOR_DOC_JA
			/// <summary>
			/// Bool型。
			/// </summary>
#else
			/// <summary>
			/// Bool type.
			/// </summary>
#endif
			[ParameterValueType(typeof(bool))]
			Bool,

#if ARBOR_DOC_JA
			/// <summary>
			/// GameObject型。
			/// </summary>
#else
			/// <summary>
			/// GameObject type.
			/// </summary>
#endif
			[ParameterValueType(typeof(GameObject))]
			GameObject,

#if ARBOR_DOC_JA
			/// <summary>
			/// String型。
			/// </summary>
#else
			/// <summary>
			/// String type.
			/// </summary>
#endif
			[ParameterValueType(typeof(string))]
			String,

#if ARBOR_DOC_JA
			/// <summary>
			/// Enum型。
			/// </summary>
#else
			/// <summary>
			/// Enum type.
			/// </summary>
#endif
			[ParameterValueType(typeof(System.Enum), useReferenceType = true)]
			Enum,

#if ARBOR_DOC_JA
			/// <summary>
			/// Vector2型。
			/// </summary>
#else
			/// <summary>
			/// Vector2 type.
			/// </summary>
#endif
			[ParameterValueType(typeof(Vector2))]
			Vector2 = 1000,

#if ARBOR_DOC_JA
			/// <summary>
			/// Vector3型。
			/// </summary>
#else
			/// <summary>
			/// Vector3 type.
			/// </summary>
#endif
			[ParameterValueType(typeof(Vector3))]
			Vector3,

#if ARBOR_DOC_JA
			/// <summary>
			/// Quaternion型。
			/// </summary>
#else
			/// <summary>
			/// Quaternion type.
			/// </summary>
#endif
			[ParameterValueType(typeof(Quaternion))]
			Quaternion,

#if ARBOR_DOC_JA
			/// <summary>
			/// Rect型。
			/// </summary>
#else
			/// <summary>
			/// Rect type.
			/// </summary>
#endif
			[ParameterValueType(typeof(Rect))]
			Rect,

#if ARBOR_DOC_JA
			/// <summary>
			/// Bounds型。
			/// </summary>
#else
			/// <summary>
			/// Bounds type.
			/// </summary>
#endif
			[ParameterValueType(typeof(Bounds))]
			Bounds,

#if ARBOR_DOC_JA
			/// <summary>
			/// Color型。
			/// </summary>
#else
			/// <summary>
			/// Color type.
			/// </summary>
#endif
			[ParameterValueType(typeof(Color))]
			Color,

#if ARBOR_DOC_JA
			/// <summary>
			/// Vector4型。
			/// </summary>
#else
			/// <summary>
			/// Vector4 type.
			/// </summary>
#endif
			[ParameterValueType(typeof(Vector4))]
			Vector4,

#if ARBOR_DOC_JA
			/// <summary>
			/// Vector2Int型。
			/// </summary>
#else
			/// <summary>
			/// Vector2Int type.
			/// </summary>
#endif
			[ParameterValueType(typeof(Vector2Int))]
			Vector2Int = 1100,

#if ARBOR_DOC_JA
			/// <summary>
			/// Vector3Int型。
			/// </summary>
#else
			/// <summary>
			/// Vector3Int type.
			/// </summary>
#endif
			[ParameterValueType(typeof(Vector3Int))]
			Vector3Int,

#if ARBOR_DOC_JA
			/// <summary>
			/// RectInt型。
			/// </summary>
#else
			/// <summary>
			/// RectInt type.
			/// </summary>
#endif
			[ParameterValueType(typeof(RectInt))]
			RectInt,

#if ARBOR_DOC_JA
			/// <summary>
			/// BoundsInt型。
			/// </summary>
#else
			/// <summary>
			/// BoundsInt type.
			/// </summary>
#endif
			[ParameterValueType(typeof(BoundsInt))]
			BoundsInt,

#if ARBOR_DOC_JA
			/// <summary>
			/// Transform型。
			/// </summary>
#else
			/// <summary>
			/// Transform type.
			/// </summary>
#endif
			[ParameterValueType(typeof(Transform))]
			Transform = 2000,

#if ARBOR_DOC_JA
			/// <summary>
			/// RectTransform型。
			/// </summary>
#else
			/// <summary>
			/// RectTransform type.
			/// </summary>
#endif
			[ParameterValueType(typeof(RectTransform))]
			RectTransform,

#if ARBOR_DOC_JA
			/// <summary>
			/// Rigidbody型。
			/// </summary>
#else
			/// <summary>
			/// Rigidbody type.
			/// </summary>
#endif
			[ParameterValueType(typeof(Rigidbody))]
			Rigidbody,

#if ARBOR_DOC_JA
			/// <summary>
			/// Rigidbody2D型。
			/// </summary>
#else
			/// <summary>
			/// Rigidbody2D type.
			/// </summary>
#endif
			[ParameterValueType(typeof(Rigidbody2D))]
			Rigidbody2D,

#if ARBOR_DOC_JA
			/// <summary>
			/// Component型。
			/// </summary>
#else
			/// <summary>
			/// Component type.
			/// </summary>
#endif
			[ParameterValueType(typeof(Component), useReferenceType = true)]
			Component,

#if ARBOR_DOC_JA
			/// <summary>
			/// Long型。
			/// </summary>
#else
			/// <summary>
			/// Long type.
			/// </summary>
#endif
			[ParameterValueType(typeof(long))]
			Long,

#if ARBOR_DOC_JA
			/// <summary>
			/// Object型(Asset)。
			/// </summary>
#else
			/// <summary>
			/// Object type(Asset).
			/// </summary>
#endif
			[ParameterValueType(typeof(Object), useReferenceType = true)]
			AssetObject,

#if ARBOR_DOC_JA
			/// <summary>
			/// Variable型。
			/// </summary>
#else
			/// <summary>
			/// Variable type.
			/// </summary>
#endif
			[ParameterValueType(null, useReferenceType = true)]
			Variable = 3000,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;int&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;int&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<int>))]
			IntList = 4000,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;long&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;long&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<long>))]
			LongList,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;float&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;float&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<float>))]
			FloatList,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;bool&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;bool&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<bool>))]
			BoolList,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;string&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;string&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<string>))]
			StringList,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;Enum&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;Enum&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<System.Enum>), useReferenceType = true, toList = true)]
			EnumList,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;Vector2&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;Vector2&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<Vector2>))]
			Vector2List = 5000,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;Vector3&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;Vector3&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<Vector3>))]
			Vector3List,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;Quaternion&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;Quaternion&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<Quaternion>))]
			QuaternionList,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;Rect&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;Rect&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<Rect>))]
			RectList,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;Bounds&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;Bounds&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<Bounds>))]
			BoundsList,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;Color&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;Color&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<Color>))]
			ColorList,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;Vector4&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;Vector4&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<Vector4>))]
			Vector4List,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;Vector2Int&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;Vector2Int&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<Vector2Int>))]
			Vector2IntList = 5100,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;Vector3Int&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;Vector3Int&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<Vector3Int>))]
			Vector3IntList,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;RectInt&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;RectInt&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<RectInt>))]
			RectIntList,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;BoundsInt&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;BoundsInt&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<BoundsInt>))]
			BoundsIntList,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;GameObject&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;GameObject&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<GameObject>))]
			GameObjectList = 6000,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;Component&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;Component&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<Component>), useReferenceType = true, toList = true)]
			ComponentList,

#if ARBOR_DOC_JA
			/// <summary>
			/// List&lt;Object(Asset)&gt;型。
			/// </summary>
#else
			/// <summary>
			/// List&lt;Object(Asset)&gt; type.
			/// </summary>
#endif
			[ParameterValueType(typeof(IList<Object>), useReferenceType = true, toList = true)]
			AssetObjectList,

#if ARBOR_DOC_JA
			/// <summary>
			/// VariableList型。
			/// </summary>
#else
			/// <summary>
			/// VariableList type.
			/// </summary>
#endif
			[ParameterValueType(null, useReferenceType = true, toList = true)]
			VariableList = 7000,
		}
	}
}