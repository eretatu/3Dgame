//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace Arbor.Extensions
{
#if ARBOR_DOC_JA
	/// <summary>
	/// GameObjectの拡張クラス
	/// </summary>
#else
	/// <summary>
	/// GameObject extension class
	/// </summary>
#endif
	public static class GameObjectExtensions
	{
		private static class ComponentTemp<T>
		{
			private static List<T> s_List = new List<T>();

			public static List<T> GetComponents(GameObject gameObject)
			{
				s_List.Clear();
				gameObject.GetComponents(s_List);
				return s_List;
			}

			public static List<T> GetComponents(Component component)
			{
				s_List.Clear();
				component.GetComponents(s_List);
				return s_List;
			}

			public static List<T> GetComponentsInChildren(GameObject gameObject)
			{
				s_List.Clear();
				gameObject.GetComponentsInChildren(s_List);
				return s_List;
			}

			public static List<T> GetComponentsInChildren(Component component)
			{
				s_List.Clear();
				component.GetComponentsInChildren(s_List);
				return s_List;
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// コンポーネントのListを取得する。取得したListインスタンスは共有されるため一時的にのみ利用できる。
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="gameObject">GameObject</param>
		/// <returns>コンポーネントのリスト</returns>
#else
		/// <summary>
		/// Get a List of components. Since the acquired List instance is shared, it can be used only temporarily.
		/// </summary>
		/// <typeparam name="T">Component type</typeparam>
		/// <param name="gameObject">GameObject</param>
		/// <returns>List of components</returns>
#endif
		public static List<T> GetComponentsTemp<T>(this GameObject gameObject)
		{
			return ComponentTemp<T>.GetComponents(gameObject);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// コンポーネントのListを取得する。取得したListインスタンスは共有されるため一時的にのみ利用できる。
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="component">Component</param>
		/// <returns>コンポーネントのリスト</returns>
#else
		/// <summary>
		/// Get a List of components. Since the acquired List instance is shared, it can be used only temporarily.
		/// </summary>
		/// <typeparam name="T">Component type</typeparam>
		/// <param name="component">Component</param>
		/// <returns>List of components</returns>
#endif
		public static List<T> GetComponentsTemp<T>(this Component component)
		{
			return ComponentTemp<T>.GetComponents(component);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 子のゲームオブジェクトのコンポーネントも含めてコンポーネントのListを取得する。取得したListインスタンスは共有されるため一時的にのみ利用できる。
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="gameObject">GameObject</param>
		/// <returns>コンポーネントのリスト</returns>
#else
		/// <summary>
		/// Get a List of components, including the components of the child GameObject. Since the acquired List instance is shared, it can be used only temporarily.
		/// </summary>
		/// <typeparam name="T">Component type</typeparam>
		/// <param name="gameObject">GameObject</param>
		/// <returns>List of components</returns>
#endif
		public static List<T> GetComponentsInChildrenTemp<T>(this GameObject gameObject)
		{
			return ComponentTemp<T>.GetComponentsInChildren(gameObject);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 子のゲームオブジェクトのコンポーネントも含めてコンポーネントのListを取得する。取得したListインスタンスは共有されるため一時的にのみ利用できる。
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="component">Component</param>
		/// <returns>コンポーネントのリスト</returns>
#else
		/// <summary>
		/// Get a List of components, including the components of the child GameObject. Since the acquired List instance is shared, it can be used only temporarily.
		/// </summary>
		/// <typeparam name="T">Component type</typeparam>
		/// <param name="component">Component</param>
		/// <returns>List of components</returns>
#endif
		public static List<T> GetComponentsInChildrenTemp<T>(this Component component)
		{
			return ComponentTemp<T>.GetComponentsInChildren(component);
		}

#if !UNITY_2019_2_OR_NEWER
#if ARBOR_DOC_JA
		/// <summary>
		/// 存在している場合にコンポーネントを取得する。
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="gameObject">GameObject</param>
		/// <param name="outComponent">見つかったコンポーネントの出力引数</param>
		/// <returns>コンポーネントが見つかった場合にtrueを返す。</returns>
#else
		/// <summary>
		/// Gets the component if it exists.
		/// </summary>
		/// <typeparam name="T">Component type</typeparam>
		/// <param name="gameObject">GameObject</param>
		/// <param name="outComponent">Output arguments of the found component</param>
		/// <returns>Returns true if the component is found.</returns>
#endif
		public static bool TryGetComponent<T>(this GameObject gameObject, out T outComponent)
		{
			outComponent = gameObject.GetComponent<T>();
			return outComponent != null;
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 存在している場合にコンポーネントを取得する。
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="component">Component</param>
		/// <param name="outComponent">見つかったコンポーネントの出力引数</param>
		/// <returns>コンポーネントが見つかった場合にtrueを返す。</returns>
#else
		/// <summary>
		/// Gets the component if it exists.
		/// </summary>
		/// <typeparam name="T">Component type</typeparam>
		/// <param name="component">Component</param>
		/// <param name="outComponent">Output arguments of the found component</param>
		/// <returns>Returns true if the component is found.</returns>
#endif
		public static bool TryGetComponent<T>(this Component component, out T outComponent)
		{
			outComponent = component.GetComponent<T>();
			return outComponent != null;
		}
#endif
	}
}