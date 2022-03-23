//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.ObjectPooling
{
	using Arbor.Extensions;

	internal sealed class PoolObject : MonoBehaviour
	{
		public Object original
		{
			get;
			set;
		}
		public Object instance
		{
			get;
			set;
		}

		private const HideFlags kPoolHideFlags = HideFlags.NotEditable;

		public bool isUsing
		{
			get;
			private set;
		}

		internal bool isValid
		{
			get
			{
				return gameObject != null && instance != null;
			}
		}

		private Transform _Transform;

#if !NETFX_CORE
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		void Awake()
		{
			_Transform = transform;
		}

		internal void OnPoolResume()
		{
			_Transform.SetParent(null, true);
			gameObject.SetActive(true);
			gameObject.hideFlags &= ~kPoolHideFlags;

			hideFlags |= HideFlags.HideAndDontSave | HideFlags.HideInInspector;

			if (!isUsing)
			{
				var receivers = gameObject.GetComponentsInChildrenTemp<IPoolCallbackReceiver>();
				for (int receiverIndex = 0; receiverIndex < receivers.Count; receiverIndex++)
				{
					IPoolCallbackReceiver receiver = receivers[receiverIndex];
					if (receiver != null)
					{
						receiver.OnPoolResume();
					}
				}
			}

			isUsing = true;
		}

		internal void OnPoolSleep()
		{
			if (isUsing)
			{
				var receivers = gameObject.GetComponentsInChildrenTemp<IPoolCallbackReceiver>();
				for (int receiverIndex = 0; receiverIndex < receivers.Count; receiverIndex++)
				{
					IPoolCallbackReceiver receiver = receivers[receiverIndex];
					if (receiver != null)
					{
						receiver.OnPoolSleep();
					}
				}
			}

			_Transform.SetParent(ObjectPool.transform, true);
			gameObject.hideFlags |= kPoolHideFlags;
			gameObject.SetActive(false);

			hideFlags |= HideFlags.HideAndDontSave | HideFlags.HideInInspector;

			isUsing = false;
		}

		internal void Initialize(Object original, Object instance)
		{
			this.original = original;
			this.instance = instance;
		}
	}
}