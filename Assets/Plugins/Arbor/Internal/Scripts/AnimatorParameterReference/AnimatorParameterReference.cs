//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.Serialization;

namespace Arbor
{
#if ARBOR_DOC_JA
	/// <summary>
	/// Animatorパラメータの参照。
	/// </summary>
#else
	/// <summary>
	/// Reference Animator parameters.
	/// </summary>
#endif
	[System.Serializable]
	public class AnimatorParameterReference : ISerializationCallbackReceiver, ISerializeVersionCallbackReceiver
	{
		#region Serialize Fields

#if ARBOR_DOC_JA
		/// <summary>
		/// パラメータが格納されているAnimator
		/// </summary>
#else
		/// <summary>
		/// Animator parameters are stored.
		/// </summary>
#endif
		[SerializeField, SlotType(typeof(Animator))]
		private FlexibleComponent _Animator = new FlexibleComponent(FlexibleHierarchyType.Self);

#if ARBOR_DOC_JA
		/// <summary>
		/// パラメータの名前
		/// </summary>
#else
		/// <summary>
		/// Parameter name.
		/// </summary>
#endif
		public string name;

#if ARBOR_DOC_JA
		/// <summary>
		/// パラメータのタイプ
		/// </summary>
#else
		/// <summary>
		/// Parameter type.
		/// </summary>
#endif
		public AnimatorControllerParameterType type = AnimatorControllerParameterType.Float;

		[SerializeField, HideInInspector]
		private SerializeVersion _SerializeVersion = new SerializeVersion();

		#region old

		[SerializeField, HideInInspector, FormerlySerializedAs("animator")]
		private Animator _OldAnimator = null;

		#endregion // old

		#endregion // Serialize Fields

		private const int kCurrentSerializeVersion = 1;

#if ARBOR_DOC_JA
		/// <summary>
		/// パラメータが格納されているAnimator
		/// </summary>
#else
		/// <summary>
		/// Animator parameters are stored.
		/// </summary>
#endif
		public Animator animator
		{
			get
			{
				return _Animator.value as Animator;
			}
			set
			{
				_Animator.SetConstant(animator);
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// AnimatorParameterReferenceのコンストラクタ
		/// </summary>
#else
		/// <summary>
		/// AnimatorParameterReference constructor
		/// </summary>
#endif
		public AnimatorParameterReference()
		{
			// Initialize when calling from script.
			_SerializeVersion.Initialize(this);
		}

		#region ISerializeVersionCallbackReceiver

		int ISerializeVersionCallbackReceiver.newestVersion
		{
			get
			{
				return kCurrentSerializeVersion;
			}
		}

		void ISerializeVersionCallbackReceiver.OnInitialize()
		{
			_SerializeVersion.version = kCurrentSerializeVersion;
		}

		void SerializeVer1()
		{
			_Animator.SetConstant(_OldAnimator);
		}

		void ISerializeVersionCallbackReceiver.OnSerialize(int version)
		{
			switch (version)
			{
				case 0:
					SerializeVer1();
					break;
			}
		}

		void ISerializeVersionCallbackReceiver.OnVersioning()
		{
		}

		#endregion // ISerializeVersionCallbackReceiver

		#region ISerializationCallbackReceiver

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			_SerializeVersion.BeforeDeserialize();
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			_SerializeVersion.AfterDeserialize();
		}

		#endregion // ISerializationCallbackReceiver
	}
}
