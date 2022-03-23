//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Arbor
{
#if ARBOR_DOC_JA
	/// <summary>
	/// Rendererへ割り当てられているMaterialPropertyBlockのラッパークラス。
	/// </summary>
#else
	/// <summary>
	/// A wrapper class for the MaterialPropertyBlock assigned to the Renderer.
	/// </summary>
#endif
	public sealed class RendererPropertyBlock
	{
		private static Dictionary<Renderer, RendererPropertyBlock> s_Blocks = new Dictionary<Renderer, RendererPropertyBlock>();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void OnBeforeSceneLoad()
		{
			SceneManager.sceneUnloaded += OnSceneUnloaded;
		}

		static void OnSceneUnloaded(Scene scene)
		{
			List<Renderer> removeRenderers = new List<Renderer>();

			foreach (var pair in s_Blocks)
			{
				var renderer = pair.Key;
				if (renderer == null)
				{
					removeRenderers.Add(renderer);
				}
			}

			for (int rendererIndex = 0; rendererIndex < removeRenderers.Count; rendererIndex++)
			{
				Renderer renderer = removeRenderers[rendererIndex];
				s_Blocks.Remove(renderer);
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Rendererへ割り当てられているRendererPropertyBlockを取得する。
		/// </summary>
		/// <param name="renderer">Renderer</param>
		/// <returns>RendererPropertyBlock</returns>
#else
		/// <summary>
		/// Get the RendererPropertyBlock assigned to the Renderer.
		/// </summary>
		/// <param name="renderer">Renderer</param>
		/// <returns>RendererPropertyBlock</returns>
#endif
		public static RendererPropertyBlock Get(Renderer renderer)
		{
			if (renderer == null)
			{
				return null;
			}

			RendererPropertyBlock block = null;
			if (!s_Blocks.TryGetValue(renderer, out block))
			{
				block = new RendererPropertyBlock(renderer);
				s_Blocks.Add(renderer, block);
			}

			return block;
		}

		private Renderer _Renderer;
		private HashSet<int> _HasProperties = new HashSet<int>();
		private MaterialPropertyBlock _Block;

		private RendererPropertyBlock(Renderer renderer)
		{
			_Renderer = renderer;
			_Block = new MaterialPropertyBlock();
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// RendererからMaterialPropertyBlockを更新する。
		/// </summary>
#else
		/// <summary>
		/// Update MaterialPropertyBlock from Renderer.
		/// </summary>
#endif
		public void Update()
		{
			_Renderer.GetPropertyBlock(_Block);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// RendererへMaterialPropertyBlockを適用する。
		/// </summary>
#else
		/// <summary>
		/// Apply MaterialPropertyBlock to Renderer.
		/// </summary>
#endif
		public void Apply()
		{
			_Renderer.SetPropertyBlock(_Block);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Materialのプロパティ値をクリアする。
		/// </summary>
#else
		/// <summary>
		/// Clear material property values.
		/// </summary>
#endif
		public void Clear()
		{
			_Block.Clear();
			_HasProperties.Clear();
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// PropertyBlockがプロパティ値を持っているかどうかを返す。
		/// </summary>
		/// <param name="nameID">プロパティ名のID</param>
		/// <returns>プロパティ値が設定されている場合にtrueを返す。</returns>
		/// <remarks>MaterialPropertyBlockを直接変更した場合は反映されません。必ずRendererPropertyBlockのSetメソッドを使用して下さい。</remarks>
#else
		/// <summary>
		/// Returns whether the PropertyBlock has a property value.
		/// </summary>
		/// <param name="nameID">Property name ID</param>
		/// <returns>Returns true if the property value is set.</returns>
		/// <remarks>If you change the MaterialPropertyBlock directly, it will not be reflected. Be sure to use the Set method of RendererPropertyBlock.</remarks>
#endif
		public bool HasProperty(int nameID)
		{
			return _HasProperties.Contains(nameID);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// PropertyBlockがプロパティ値を持っているかどうかを返す。
		/// </summary>
		/// <param name="name">プロパティ名</param>
		/// <returns>プロパティ値が設定されている場合にtrueを返す。</returns>
		/// <remarks>MaterialPropertyBlockを直接変更した場合は反映されません。必ずRendererPropertyBlockのSetメソッドを使用して下さい。</remarks>
#else
		/// <summary>
		/// Returns whether the PropertyBlock has a property value.
		/// </summary>
		/// <param name="name">Property name</param>
		/// <returns>Returns true if the property value is set.</returns>
		/// <remarks>If you change the MaterialPropertyBlock directly, it will not be reflected. Be sure to use the Set method of RendererPropertyBlock.</remarks>
#endif
		public bool HasProperty(string name)
		{
			return HasProperty(Shader.PropertyToID(name));
		}

		#region Float

#if ARBOR_DOC_JA
		/// <summary>
		/// float値をゲットする。
		/// </summary>
		/// <param name="nameID">プロパティ名のID</param>
		/// <returns>float値</returns>
#else
		/// <summary>
		/// Get the float value.
		/// </summary>
		/// <param name="nameID">Property name ID</param>
		/// <returns>float value</returns>
#endif
		public float GetFloat(int nameID)
		{
			return _Block.GetFloat(nameID);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// float値をゲットする。
		/// </summary>
		/// <param name="name">プロパティ名</param>
		/// <returns>float値</returns>
#else
		/// <summary>
		/// Get the float value.
		/// </summary>
		/// <param name="name">Property name</param>
		/// <returns>float value</returns>
#endif
		public float GetFloat(string name)
		{
			return GetFloat(Shader.PropertyToID(name));
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// float値をセットする。
		/// </summary>
		/// <param name="nameID">プロパティ名のID</param>
		/// <param name="value">値</param>
#else
		/// <summary>
		/// Set the float value.
		/// </summary>
		/// <param name="nameID">Property name ID</param>
		/// <param name="value">Value</param>
#endif
		public void SetFloat(int nameID, float value)
		{
			_Block.SetFloat(nameID, value);
			_HasProperties.Add(nameID);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// float値をセットする。
		/// </summary>
		/// <param name="name">プロパティ名</param>
		/// <param name="value">値</param>
#else
		/// <summary>
		/// Set the float value.
		/// </summary>
		/// <param name="name">Property name</param>
		/// <param name="value">Value</param>
#endif
		public void SetFloat(string name, float value)
		{
			SetFloat(Shader.PropertyToID(name), value);
		}

		#endregion // Float

		#region Color

#if ARBOR_DOC_JA
		/// <summary>
		/// Color値をゲットする。
		/// </summary>
		/// <param name="nameID">プロパティ名のID</param>
		/// <returns>Color値</returns>
#else
		/// <summary>
		/// Get the Color value.
		/// </summary>
		/// <param name="nameID">Property name ID</param>
		/// <returns>Color value</returns>
#endif
		public Color GetColor(int nameID)
		{
			return _Block.GetColor(nameID);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Color値をゲットする。
		/// </summary>
		/// <param name="name">プロパティ名</param>
		/// <returns>Color値</returns>
#else
		/// <summary>
		/// Get the Color value.
		/// </summary>
		/// <param name="name">Property name</param>
		/// <returns>Color value</returns>
#endif
		public Color GetColor(string name)
		{
			return GetColor(Shader.PropertyToID(name));
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Color値をセットする。
		/// </summary>
		/// <param name="nameID">プロパティ名のID</param>
		/// <param name="value">値</param>
#else
		/// <summary>
		/// Set the Color value.
		/// </summary>
		/// <param name="nameID">Property name ID</param>
		/// <param name="value">Value</param>
#endif
		public void SetColor(int nameID, Color value)
		{
			_Block.SetColor(nameID, value);
			_HasProperties.Add(nameID);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Color値をセットする。
		/// </summary>
		/// <param name="name">プロパティ名</param>
		/// <param name="value">値</param>
#else
		/// <summary>
		/// Set the Color value.
		/// </summary>
		/// <param name="name">Property name</param>
		/// <param name="value">Value</param>
#endif
		public void SetColor(string name, Color value)
		{
			SetColor(Shader.PropertyToID(name), value);
		}

		#endregion // Color

		#region Matrix

#if ARBOR_DOC_JA
		/// <summary>
		/// Matrix4x4値をゲットする。
		/// </summary>
		/// <param name="nameID">プロパティ名のID</param>
		/// <returns>Matrix4x4値</returns>
#else
		/// <summary>
		/// Get the Matrix4x4 value.
		/// </summary>
		/// <param name="nameID">Property name ID</param>
		/// <returns>Matrix4x4 value</returns>
#endif
		public Matrix4x4 GetMatrix(int nameID)
		{
			return _Block.GetMatrix(nameID);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Matrix4x4値をゲットする。
		/// </summary>
		/// <param name="name">プロパティ名</param>
		/// <returns>Matrix4x4値</returns>
#else
		/// <summary>
		/// Get the Matrix4x4 value.
		/// </summary>
		/// <param name="name">Property name</param>
		/// <returns>Matrix4x4 value</returns>
#endif
		public Matrix4x4 GetMatrix(string name)
		{
			return GetMatrix(Shader.PropertyToID(name));
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Matrix4x4値をセットする。
		/// </summary>
		/// <param name="nameID">プロパティ名のID</param>
		/// <param name="value">値</param>
#else
		/// <summary>
		/// Set the Matrix4x4 value.
		/// </summary>
		/// <param name="nameID">Property name ID</param>
		/// <param name="value">Value</param>
#endif
		public void SetMatrix(int nameID, Matrix4x4 value)
		{
			_Block.SetMatrix(nameID, value);
			_HasProperties.Add(nameID);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Matrix4x4値をセットする。
		/// </summary>
		/// <param name="name">プロパティ名</param>
		/// <param name="value">値</param>
#else
		/// <summary>
		/// Set the Matrix4x4 value.
		/// </summary>
		/// <param name="name">Property name</param>
		/// <param name="value">Value</param>
#endif
		public void SetMatrix(string name, Matrix4x4 value)
		{
			SetMatrix(Shader.PropertyToID(name), value);
		}

		#endregion // Matrix

		#region Texture

#if ARBOR_DOC_JA
		/// <summary>
		/// Texture値をゲットする。
		/// </summary>
		/// <param name="nameID">プロパティ名のID</param>
		/// <returns>Texture値</returns>
#else
		/// <summary>
		/// Get the Texture value.
		/// </summary>
		/// <param name="nameID">Property name ID</param>
		/// <returns>Texture value</returns>
#endif
		public Texture GetTexture(int nameID)
		{
			return _Block.GetTexture(nameID);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Texture値をゲットする。
		/// </summary>
		/// <param name="name">プロパティ名</param>
		/// <returns>Texture値</returns>
#else
		/// <summary>
		/// Get the Texture value.
		/// </summary>
		/// <param name="name">Property name</param>
		/// <returns>Texture value</returns>
#endif
		public Texture GetTexture(string name)
		{
			return GetTexture(Shader.PropertyToID(name));
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Texture値をセットする。
		/// </summary>
		/// <param name="nameID">プロパティ名のID</param>
		/// <param name="value">値</param>
#else
		/// <summary>
		/// Set the Texture value.
		/// </summary>
		/// <param name="nameID">Property name ID</param>
		/// <param name="value">Value</param>
#endif
		public void SetTexture(int nameID, Texture value)
		{
			_Block.SetTexture(nameID, value);
			_HasProperties.Add(nameID);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Texture値をセットする。
		/// </summary>
		/// <param name="name">プロパティ名</param>
		/// <param name="value">値</param>
#else
		/// <summary>
		/// Set the Texture value.
		/// </summary>
		/// <param name="name">Property name</param>
		/// <param name="value">Value</param>
#endif
		public void SetTexture(string name, Texture value)
		{
			SetTexture(Shader.PropertyToID(name), value);
		}

		#endregion // Texture

		#region Vector

#if ARBOR_DOC_JA
		/// <summary>
		/// Vector4値をゲットする。
		/// </summary>
		/// <param name="nameID">プロパティ名のID</param>
		/// <returns>Vector4値</returns>
#else
		/// <summary>
		/// Get the Vector4 value.
		/// </summary>
		/// <param name="nameID">Property name ID</param>
		/// <returns>Vector4 value</returns>
#endif
		public Vector4 GetVector(int nameID)
		{
			return _Block.GetVector(nameID);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Vector4値をゲットする。
		/// </summary>
		/// <param name="name">プロパティ名</param>
		/// <returns>Vector4値</returns>
#else
		/// <summary>
		/// Get the Vector4 value.
		/// </summary>
		/// <param name="name">Property name</param>
		/// <returns>Vector4 value</returns>
#endif
		public Vector4 GetVector(string name)
		{
			return GetVector(Shader.PropertyToID(name));
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Vector4値をセットする。
		/// </summary>
		/// <param name="nameID">プロパティ名のID</param>
		/// <param name="value">値</param>
#else
		/// <summary>
		/// Set the Vector4 value.
		/// </summary>
		/// <param name="nameID">Property name ID</param>
		/// <param name="value">Value</param>
#endif
		public void SetVector(int nameID, Vector4 value)
		{
			_Block.SetVector(nameID, value);
			_HasProperties.Add(nameID);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// Vector4値をセットする。
		/// </summary>
		/// <param name="name">プロパティ名</param>
		/// <param name="value">値</param>
#else
		/// <summary>
		/// Set the Vector4 value.
		/// </summary>
		/// <param name="name">Property name</param>
		/// <param name="value">Value</param>
#endif
		public void SetVector(string name, Vector4 value)
		{
			SetVector(Shader.PropertyToID(name), value);
		}

		#endregion // Vector
	}
}