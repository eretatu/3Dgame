//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Arbor.ObjectPooling
{
	using Arbor.Extensions;

	internal sealed class PoolQueue
	{
		public Queue<PoolObject> queue = new Queue<PoolObject>();

		public void CleanInvalidObjects()
		{
			Queue<PoolObject> newList = new Queue<PoolObject>();

			while (queue.Count > 0)
			{
				PoolObject poolObject = queue.Dequeue();
				if (poolObject != null && poolObject.isValid)
				{
					newList.Enqueue(poolObject);
				}
			}

			queue = newList;
		}
	}

#if ARBOR_DOC_JA
	/// <summary>
	/// ObjectPoolの管理クラス
	/// </summary>
#else
	/// <summary>
	/// ObjectPool management class
	/// </summary>
#endif
	public static class ObjectPool
	{
		private const int kDefaultAdvancedRatePerFrame = 10;
		private const int kDefaultAdvancedFrameRate = -1;

		private static int s_AdvancedRatePerFrame = kDefaultAdvancedRatePerFrame;
		private static int s_AdvancedFrameRate = kDefaultAdvancedFrameRate;

		private static Dictionary<Object, PoolQueue> s_Pool = new Dictionary<Object, PoolQueue>();

		private static GameObject s_GameObject = null;
		private static Transform s_Transform = null;
		private static AdvancedPoolingInternal s_AdvancedPooling = null;

#if ARBOR_DOC_JA
		/// <summary>
		/// 事前プールの処理フレームレート(画面のリフレッシュレートに対する倍率)
		/// </summary>
		/// <remarks>
		/// このフレームレートを超えて処理時間がかかった場合は次フレームまで待機する。<br/>
		/// デフォルト値は10。<br/>
		/// 0以下を指定した場合は<see cref="advancedFrameRate"/>を使用する。
		/// </remarks>
#else
		/// <summary>
		/// Advanced Pooling processing frame rate (multiplication factor relative to the screen refresh rate)
		/// </summary>
		/// <remarks>
		/// If processing time is exceeded beyond this frame rate, it waits until the next frame.<br/>
		/// The default value is 10.<br/>
		/// If 0 or less is specified, use <see cref="advancedFrameRate"/>.
		/// </remarks>
#endif
		public static int advancedRatePerFrame
		{
			get
			{
				return s_AdvancedRatePerFrame;
			}
			set
			{
				s_AdvancedRatePerFrame = value;
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 事前プールの処理フレームレート
		/// </summary>
		/// <remarks>
		/// このフレームレートを超えて処理時間がかかった場合は次フレームまで待機する。<br/>
		/// advancedFrameRateと<see cref="advancedRatePerFrame"/>の両方に0以下を指定した場合は、全てのプールが完了するまで待機しない<br/>
		/// デフォルト値は0。<br/>
		/// </remarks>
#else
		/// <summary>
		/// Advanced Pooling processing frame rate
		/// </summary>
		/// <remarks>
		/// If processing time is exceeded beyond this frame rate, it waits until the next frame.<br/>
		/// When 0 or less is specified for both advancedFrameRate and <see cref="advancedRatePerFrame"/>, do not wait until all pools are completed.<br/>
		/// The default value is 0.<br/>
		/// </remarks>
#endif
		public static int advancedFrameRate
		{
			get
			{
				return s_AdvancedFrameRate;
			}
			set
			{
				s_AdvancedFrameRate = value;
			}
		}

		internal static GameObject gameObject
		{
			get
			{
				if (s_GameObject == null)
				{
					s_GameObject = new GameObject("ObjectPool");
					s_GameObject.hideFlags |= HideFlags.NotEditable;
				}
				return s_GameObject;
			}
		}

		internal static Transform transform
		{
			get
			{
				if (s_Transform == null)
				{
					s_Transform = gameObject.transform;
				}
				return s_Transform;
			}
		}

		private static AdvancedPoolingInternal advancedPooling
		{
			get
			{
				if (s_AdvancedPooling == null)
				{
					s_AdvancedPooling = gameObject.AddComponent<AdvancedPoolingInternal>();
				}
				return s_AdvancedPooling;
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 事前プールが完了しているかを返す。
		/// </summary>
#else
		/// <summary>
		/// Returns whether Advanced pooling is completed.
		/// </summary>
#endif
		public static bool isReady
		{
			get
			{
				if (s_AdvancedPooling != null)
				{
					return s_AdvancedPooling.isReady;
				}
				return true;
			}
		}

#if UNITY_2019_3_OR_NEWER
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void OnSubsystemRegistration()
		{
			s_AdvancedRatePerFrame = kDefaultAdvancedRatePerFrame;
			s_AdvancedFrameRate = kDefaultAdvancedFrameRate;
			s_Pool.Clear();
			s_GameObject = null;
			s_Transform = null;
			s_AdvancedPooling = null;
		}
#endif

		private static void CheckOriginalArgument(Object original)
		{
			if (original == null)
				throw new System.ArgumentException("The Object you want to instantiate is null.");

			if (original is ScriptableObject)
				throw new System.ArgumentException("Cannot instantiate a ScriptableObject");
		}

		static ObjectPool()
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			if (loadSceneMode == LoadSceneMode.Single)
			{
				foreach (PoolQueue pool in s_Pool.Values)
				{
					pool.CleanInvalidObjects();
				}
			}
		}

		static PoolQueue GetPool(Object original)
		{
			PoolQueue pool = null;
			if (!s_Pool.TryGetValue(original, out pool))
			{
				pool = new PoolQueue();
				s_Pool.Add(original, pool);
			}

			return pool;
		}

		private static Transform GetTransform(Object original)
		{
			Transform transform = original as Transform;
			if (transform != null)
			{
				return transform;
			}

			Component component = original as Component;
			if (component != null)
			{
				return component.transform;
			}

			GameObject gameObject = original as GameObject;
			if (gameObject != null)
			{
				return gameObject.transform;
			}

			return null;
		}

		static PoolObject CreatePoolObject(Object original)
		{
			Object instance = Object.Instantiate(original);

			GameObject gameObject = instance as GameObject;
			if (gameObject == null)
			{
				Component component = instance as Component;
				if (component != null)
				{
					gameObject = component.gameObject;
				}
			}

			PoolObject poolObject = null;
			if (!gameObject.TryGetComponent<PoolObject>(out poolObject))
			{
				poolObject = gameObject.AddComponent<PoolObject>();
			}
			poolObject.Initialize(original, instance);

			return poolObject;
		}

		static Object CreateInstante(Object original)
		{
			PoolObject poolObject = CreatePoolObject(original);

			return poolObject.instance;
		}

		internal static void CreatePool(Object original)
		{
			PoolObject poolObject = CreatePoolObject(original);

			poolObject.OnPoolSleep();

			PoolQueue pool = GetPool(original);
			pool.queue.Enqueue(poolObject);
		}

		private static Object Internal_CloneSingle(Object original)
		{
			PoolQueue pool = GetPool(original);

			while (pool.queue.Count > 0)
			{
				PoolObject poolObject = pool.queue.Dequeue();
				if (poolObject == null || !poolObject.isValid)
				{
					continue;
				}

				poolObject.OnPoolResume();

				return poolObject.instance;
			}

			return CreateInstante(original);
		}

		private static Object Internal_CloneSingleWithParent(Object original, Transform parent, bool worldPositionStays)
		{
			Object instance = Internal_CloneSingle(original);

			Transform transform = GetTransform(instance);
			if (transform != null)
			{
				transform.SetParent(parent, worldPositionStays);
			}

			return instance;
		}

		private static Object Internal_InstantiateSingle(Object original, Vector3 pos, Quaternion rot)
		{
			Object instance = Internal_CloneSingle(original);

			Transform transform = GetTransform(instance);
			if (transform != null)
			{
				transform.position = pos;
				transform.rotation = rot;
			}

			return instance;
		}

		private static Object Internal_InstantiateSingleWithParent(Object original, Transform parent, Vector3 pos, Quaternion rot)
		{
			Object instance = Internal_CloneSingleWithParent(original, parent, false);

			Transform transform = GetTransform(instance);
			if (transform != null)
			{
				transform.position = pos;
				transform.rotation = rot;
			}

			return instance;
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 事前プールする。
		/// </summary>
		/// <param name="items">プールするオブジェクトリスト</param>
#else
		/// <summary>
		/// Pool in advance.
		/// </summary>
		/// <param name="items">List of objects to pool</param>
#endif
		public static void AdvancedPool(IList<PoolingItem> items)
		{
			advancedPooling.AddItems(items);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 事前プールする。
		/// </summary>
		/// <param name="items">プールするオブジェクトリスト</param>
#else
		/// <summary>
		/// Pool in advance.
		/// </summary>
		/// <param name="items">List of objects to pool</param>
#endif
		public static void AdvancedPool(IEnumerable<PoolingItem> items)
		{
			advancedPooling.AddItems(items);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// オブジェクトをインスタンス化する。
		/// </summary>
		/// <param name="original">オリジナルのオブジェクト</param>
		/// <param name="position">位置</param>
		/// <param name="rotation">回転</param>
		/// <returns>インスタンス化されたオブジェクト</returns>
		/// <remarks>
		/// プールされているオブジェクトがある場合はそのオブジェクトを再開させる。<br/>
		/// プールがない場合はObject.Instantiateにより新たにインスタンス化する。<br/>
		/// </remarks>
#else
		/// <summary>
		/// Instantiate an object.
		/// </summary>
		/// <param name="original">Original object</param>
		/// <param name="position">Position</param>
		/// <param name="rotation">Rotation</param>
		/// <returns>Instantiated object</returns>
		/// <remarks>
		/// If there is a pooled object, resume that object.<br/>
		/// If there is no pool, it is newly instantiated by Object.Instantiate.
		/// </remarks>
#endif
		public static Object Instantiate(Object original, Vector3 position, Quaternion rotation)
		{
			return Instantiate(original, position, rotation, null);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// オブジェクトをインスタンス化する。
		/// </summary>
		/// <param name="original">オリジナルのオブジェクト</param>
		/// <param name="position">位置</param>
		/// <param name="rotation">回転</param>
		/// <param name="parent">親トランスフォーム</param>
		/// <returns>インスタンス化されたオブジェクト</returns>
		/// <remarks>
		/// プールされているオブジェクトがある場合はそのオブジェクトを再開させる。<br/>
		/// プールがない場合はObject.Instantiateにより新たにインスタンス化する。<br/>
		/// </remarks>
#else
		/// <summary>
		/// Instantiate an object.
		/// </summary>
		/// <param name="original">Original object</param>
		/// <param name="position">Position</param>
		/// <param name="rotation">Rotation</param>
		/// <param name="parent">Parent Transform</param>
		/// <returns>Instantiated object</returns>
		/// <remarks>
		/// If there is a pooled object, resume that object.<br/>
		/// If there is no pool, it is newly instantiated by Object.Instantiate.
		/// </remarks>
#endif
		public static Object Instantiate(Object original, Vector3 position, Quaternion rotation, Transform parent)
		{
			CheckOriginalArgument(original);

			if (parent == null)
				return Internal_InstantiateSingle(original, position, rotation);

			return Internal_InstantiateSingleWithParent(original, parent, position, rotation);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// オブジェクトをインスタンス化する。
		/// </summary>
		/// <param name="original">オリジナルのオブジェクト</param>
		/// <returns>インスタンス化されたオブジェクト</returns>
		/// <remarks>
		/// プールされているオブジェクトがある場合はそのオブジェクトを再開させる。<br/>
		/// プールがない場合はObject.Instantiateにより新たにインスタンス化する。<br/>
		/// </remarks>
#else
		/// <summary>
		/// Instantiate an object.
		/// </summary>
		/// <param name="original">Original object</param>
		/// <returns>Instantiated object</returns>
		/// <remarks>
		/// If there is a pooled object, resume that object.<br/>
		/// If there is no pool, it is newly instantiated by Object.Instantiate.
		/// </remarks>
#endif
		public static Object Instantiate(Object original)
		{
			CheckOriginalArgument(original);

			Transform transform = GetTransform(original);
			return Internal_InstantiateSingle(original, transform.position, transform.rotation);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// オブジェクトをインスタンス化する。
		/// </summary>
		/// <param name="original">オリジナルのオブジェクト</param>
		/// <param name="parent">親トランスフォーム</param>
		/// <returns>インスタンス化されたオブジェクト</returns>
		/// <remarks>
		/// プールされているオブジェクトがある場合はそのオブジェクトを再開させる。<br/>
		/// プールがない場合はObject.Instantiateにより新たにインスタンス化する。<br/>
		/// </remarks>
#else
		/// <summary>
		/// Instantiate an object.
		/// </summary>
		/// <param name="original">Original object</param>
		/// <param name="parent">Parent Transform</param>
		/// <returns>Instantiated object</returns>
		/// <remarks>
		/// If there is a pooled object, resume that object.<br/>
		/// If there is no pool, it is newly instantiated by Object.Instantiate.
		/// </remarks>
#endif
		public static Object Instantiate(Object original, Transform parent)
		{
			return Instantiate(original, parent, false);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// オブジェクトをインスタンス化する。
		/// </summary>
		/// <param name="original">オリジナルのオブジェクト</param>
		/// <param name="parent">親トランスフォーム</param>
		/// <param name="instantiateInWorldSpace">parent を指定するときに、元のワールドの位置が維持されるかどうか</param>
		/// <returns>インスタンス化されたオブジェクト</returns>
		/// <remarks>
		/// プールされているオブジェクトがある場合はそのオブジェクトを再開させる。<br/>
		/// プールがない場合はObject.Instantiateにより新たにインスタンス化する。<br/>
		/// </remarks>
#else
		/// <summary>
		/// Instantiate an object.
		/// </summary>
		/// <param name="original">Original object</param>
		/// <param name="parent">Parent Transform</param>
		/// <param name="instantiateInWorldSpace">If when assigning the parent the original world position should be maintained.</param>
		/// <returns>Instantiated object</returns>
		/// <remarks>
		/// If there is a pooled object, resume that object.<br/>
		/// If there is no pool, it is newly instantiated by Object.Instantiate.
		/// </remarks>
#endif
		public static Object Instantiate(Object original, Transform parent, bool instantiateInWorldSpace)
		{
			CheckOriginalArgument(original);

			if (parent == null)
				return Internal_CloneSingle(original);

			return Internal_CloneSingleWithParent(original, parent, instantiateInWorldSpace);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// オブジェクトをインスタンス化する。
		/// </summary>
		/// <typeparam name="T">オブジェクトの型</typeparam>
		/// <param name="original">オリジナルのオブジェクト</param>
		/// <returns>インスタンス化されたオブジェクト</returns>
		/// <remarks>
		/// プールされているオブジェクトがある場合はそのオブジェクトを再開させる。<br/>
		/// プールがない場合はObject.Instantiateにより新たにインスタンス化する。<br/>
		/// </remarks>
#else
		/// <summary>
		/// Instantiate an object.
		/// </summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="original">Original object</param>
		/// <returns>Instantiated object</returns>
		/// <remarks>
		/// If there is a pooled object, resume that object.<br/>
		/// If there is no pool, it is newly instantiated by Object.Instantiate.
		/// </remarks>
#endif
		public static T Instantiate<T>(T original) where T : Object
		{
			return (T)Instantiate((Object)original);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// オブジェクトをインスタンス化する。
		/// </summary>
		/// <typeparam name="T">オブジェクトの型</typeparam>
		/// <param name="original">オリジナルのオブジェクト</param>
		/// <param name="position">位置</param>
		/// <param name="rotation">回転</param>
		/// <returns>インスタンス化されたオブジェクト</returns>
		/// <remarks>
		/// プールされているオブジェクトがある場合はそのオブジェクトを再開させる。<br/>
		/// プールがない場合はObject.Instantiateにより新たにインスタンス化する。<br/>
		/// </remarks>
#else
		/// <summary>
		/// Instantiate an object.
		/// </summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="original">Original object</param>
		/// <param name="position">Position</param>
		/// <param name="rotation">Rotation</param>
		/// <returns>Instantiated object</returns>
		/// <remarks>
		/// If there is a pooled object, resume that object.<br/>
		/// If there is no pool, it is newly instantiated by Object.Instantiate.
		/// </remarks>
#endif
		public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object
		{
			return Instantiate<T>(original, position, rotation, null);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// オブジェクトをインスタンス化する。
		/// </summary>
		/// <typeparam name="T">オブジェクトの型</typeparam>
		/// <param name="original">オリジナルのオブジェクト</param>
		/// <param name="position">位置</param>
		/// <param name="rotation">回転</param>
		/// <param name="parent">親トランスフォーム</param>
		/// <returns>インスタンス化されたオブジェクト</returns>
		/// <remarks>
		/// プールされているオブジェクトがある場合はそのオブジェクトを再開させる。<br/>
		/// プールがない場合はObject.Instantiateにより新たにインスタンス化する。<br/>
		/// </remarks>
#else
		/// <summary>
		/// Instantiate an object.
		/// </summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="original">Original object</param>
		/// <param name="position">Position</param>
		/// <param name="rotation">Rotation</param>
		/// <param name="parent">Parent Transform</param>
		/// <returns>Instantiated object</returns>
		/// <remarks>
		/// If there is a pooled object, resume that object.<br/>
		/// If there is no pool, it is newly instantiated by Object.Instantiate.
		/// </remarks>
#endif
		public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object
		{
			return (T)Instantiate((Object)original, position, rotation, parent);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// オブジェクトをインスタンス化する。
		/// </summary>
		/// <typeparam name="T">オブジェクトの型</typeparam>
		/// <param name="original">オリジナルのオブジェクト</param>
		/// <param name="parent">親トランスフォーム</param>
		/// <returns>インスタンス化されたオブジェクト</returns>
		/// <remarks>
		/// プールされているオブジェクトがある場合はそのオブジェクトを再開させる。<br/>
		/// プールがない場合はObject.Instantiateにより新たにインスタンス化する。<br/>
		/// </remarks>
#else
		/// <summary>
		/// Instantiate an object.
		/// </summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="original">Original object</param>
		/// <param name="parent">Parent Transform</param>
		/// <returns>Instantiated object</returns>
		/// <remarks>
		/// If there is a pooled object, resume that object.<br/>
		/// If there is no pool, it is newly instantiated by Object.Instantiate.
		/// </remarks>
#endif
		public static T Instantiate<T>(T original, Transform parent) where T : Object
		{
			return Instantiate<T>(original, parent, false);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// オブジェクトをインスタンス化する。
		/// </summary>
		/// <typeparam name="T">オブジェクトの型</typeparam>
		/// <param name="original">オリジナルのオブジェクト</param>
		/// <param name="parent">親トランスフォーム</param>
		/// <param name="instantiateInWorldSpace">parent を指定するときに、元のワールドの位置が維持されるかどうか</param>
		/// <returns>インスタンス化されたオブジェクト</returns>
		/// <remarks>
		/// プールされているオブジェクトがある場合はそのオブジェクトを再開させる。<br/>
		/// プールがない場合はObject.Instantiateにより新たにインスタンス化する。<br/>
		/// </remarks>
#else
		/// <summary>
		/// Instantiate an object.
		/// </summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="original">Original object</param>
		/// <param name="parent">Parent Transform</param>
		/// <param name="instantiateInWorldSpace">If when assigning the parent the original world position should be maintained.</param>
		/// <returns>Instantiated object</returns>
		/// <remarks>
		/// If there is a pooled object, resume that object.<br/>
		/// If there is no pool, it is newly instantiated by Object.Instantiate.
		/// </remarks>
#endif
		public static T Instantiate<T>(T original, Transform parent, bool instantiateInWorldSpace) where T : Object
		{
			return (T)Instantiate((Object)original, parent, instantiateInWorldSpace);
		}

		internal static void Destroy(PoolObject poolObject)
		{
			if (poolObject.isValid)
			{
				poolObject.OnPoolSleep();

				Object original = poolObject.original;
				PoolQueue pool = GetPool(original);
				pool.queue.Enqueue(poolObject);
			}
			else
			{
				Object.Destroy(poolObject);
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// GameObjectを破棄する。
		/// </summary>
		/// <param name="gameObject">破棄するGameObject</param>
		/// <remarks>
		/// プール管理下のGameObjectであれば、プールへ返却する。<br/>
		/// 管理下でなければObject.Destroyにより破棄する。<br/>
		/// </remarks>
#else
		/// <summary>
		/// Destroy GameObject.
		/// </summary>
		/// <param name="gameObject">GameObject to destroy</param>
		/// <remarks>
		/// If it is a GameObject under pool management, return it to the pool.<br/>
		/// If it is not under management, it is destroyed by Object.Destroy.<br/>
		/// </remarks>
#endif
		public static void Destroy(GameObject gameObject)
		{
			PoolObject poolObject = null;
			if (gameObject.TryGetComponent<PoolObject>(out poolObject))
			{
				Destroy(poolObject);
			}
			else
			{
				Object.Destroy(gameObject);
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// GameObjectを即座に破棄する。
		/// </summary>
		/// <param name="gameObject">破棄するGameObject</param>
		/// <remarks>
		/// プール管理下のGameObjectであれば、プールへ返却する。<br/>
		/// 管理下でなければObject.DestroyImmediateにより破棄する。<br/>
		/// </remarks>
#else
		/// <summary>
		/// Destroy GameObject immediately.
		/// </summary>
		/// <param name="gameObject">GameObject to destroy</param>
		/// <remarks>
		/// If it is a GameObject under pool management, return it to the pool.<br/>
		/// If it is not under management, it is destroyed by Object.DestroyImmediate.<br/>
		/// </remarks>
#endif
		public static void DestroyImmediate(GameObject gameObject)
		{
			PoolObject poolObject = null;
			if (gameObject.TryGetComponent<PoolObject>(out poolObject))
			{
				Destroy(poolObject);
			}
			else
			{
				Object.DestroyImmediate(gameObject);
			}
		}
	}
}