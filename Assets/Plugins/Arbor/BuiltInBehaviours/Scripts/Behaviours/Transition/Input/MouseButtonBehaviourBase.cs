//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.Serialization;

namespace Arbor.StateMachine.StateBehaviours
{
#if ARBOR_DOC_JA
	/// <summary>
	/// マウスボタンを判定する基本クラス
	/// </summary>
#else
	/// <summary>
	/// Base class for determining mouse buttons
	/// </summary>
#endif
	public abstract class MouseButtonBehaviourBase : InputBehaviourBase, INodeBehaviourSerializationCallbackReceiver
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// マウスボタンの指定。
		/// </summary>
#else
		/// <summary>
		/// Specified mouse button.
		/// </summary>
#endif
		[SerializeField]
		private FlexibleInt _Button = new FlexibleInt(0);

		[SerializeField]
		[HideInInspector]
		private int _SerializeVersion = 0;

		#region old

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("_Button")]
		private int _OldButton = 0;

		#endregion // old

		#endregion // Serialize fields

		public int button
		{
			get
			{
				return _Button.value;
			}
		}

		private const int kCurrentSerializeVersion = 1;

		protected override void Reset()
		{
			base.Reset();

			_SerializeVersion = kCurrentSerializeVersion;
		}

		void SerializeVer1()
		{
			_Button = (FlexibleInt)_OldButton;
		}

		void Serialize()
		{
			while (_SerializeVersion != kCurrentSerializeVersion)
			{
				switch (_SerializeVersion)
				{
					case 0:
						SerializeVer1();
						_SerializeVersion++;
						break;
					default:
						_SerializeVersion = kCurrentSerializeVersion;
						break;
				}
			}
		}

		public virtual void OnBeforeSerialize()
		{
			Serialize();
		}

		public virtual void OnAfterDeserialize()
		{
			Serialize();
		}
	}
}
