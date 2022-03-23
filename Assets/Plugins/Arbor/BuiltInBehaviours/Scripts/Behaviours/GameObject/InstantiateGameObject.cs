//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.Serialization;
using Arbor.ObjectPooling;

namespace Arbor.StateMachine.StateBehaviours
{
#if ARBOR_DOC_JA
	/// <summary>
	/// GameObjectを生成する。
	/// </summary>
#else
	/// <summary>
	/// GameObject the searches in the tag and then stored in the parameter.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("GameObject/InstantiateGameObject")]
	[BuiltInBehaviour]
	public sealed class InstantiateGameObject : StateBehaviour, INodeBehaviourSerializationCallbackReceiver
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// 生成するGameObject。
		/// </summary>
#else
		/// <summary>
		/// The Instantiated GameObject.
		/// </summary>
#endif
		[SerializeField] private FlexibleGameObject _Prefab = new FlexibleGameObject();

#if ARBOR_DOC_JA
		/// <summary>
		/// 親に指定するTransform。
		/// </summary>
#else
		/// <summary>
		/// Transform that specified in the parent.
		/// </summary>
#endif
		[SerializeField] private FlexibleTransform _Parent = new FlexibleTransform();

#if ARBOR_DOC_JA
		/// <summary>
		/// 姿勢タイプ
		/// </summary>
#else
		/// <summary>
		/// Posture type
		/// </summary>
#endif
		[SerializeField]
		[Internal.DocumentType(typeof(PostureType))]
		private FlexiblePostureType _PostureType = new FlexiblePostureType(PostureType.Transform);

#if ARBOR_DOC_JA
		/// <summary>
		/// 初期時に指定するTransform。Posture TypeがTransformの時に使用する。
		/// </summary>
#else
		/// <summary>
		/// Transform that you specify for the initial time. Used when Posture Type is Transform
		/// </summary>
#endif
		[SerializeField] private FlexibleTransform _InitTransform = new FlexibleTransform();

#if ARBOR_DOC_JA
		/// <summary>
		/// 初期時に指定する位置。Posture TypeがDirectlyの時に使用する。
		/// </summary>
#else
		/// <summary>
		/// The position specified at the beginning. Used when Posture Type is Directly.
		/// </summary>
#endif
		[SerializeField]
		private FlexibleVector3 _InitPosition = new FlexibleVector3();

#if ARBOR_DOC_JA
		/// <summary>
		/// 初期時に指定する回転。Posture TypeがDirectlyの時に使用する。
		/// </summary>
#else
		/// <summary>
		/// The rotation specified at the initial stage. Used when Posture Type is Directly.
		/// </summary>
#endif
		[SerializeField]
		private FlexibleQuaternion _InitRotation = new FlexibleQuaternion();

#if ARBOR_DOC_JA
		/// <summary>
		/// 初期時に指定する空間。Posture TypeがDirectlyの時に使用する。
		/// </summary>
#else
		/// <summary>
		/// The space specified at the beginning. Used when Posture Type is Directly.
		/// </summary>
#endif
		[SerializeField]
		private FlexibleSpace _InitSpace = new FlexibleSpace(Space.World);

#if ARBOR_DOC_JA
		/// <summary>
		/// ObjectPoolを使用してインスタンス化するフラグ。
		/// </summary>
#else
		/// <summary>
		/// Flag to instantiate using ObjectPool.
		/// </summary>
#endif
		[SerializeField]
		private FlexibleBool _UsePool = new FlexibleBool();

#if ARBOR_DOC_JA
		/// <summary>
		/// 格納先のパラメータ
		/// </summary>
#else
		/// <summary>
		/// Storage destination parameters
		/// </summary>
#endif
		[SerializeField] private GameObjectParameterReference _Parameter = new GameObjectParameterReference();

#if ARBOR_DOC_JA
		/// <summary>
		/// 演算ノードへの出力。
		/// </summary>
#else
		/// <summary>
		/// Output to the calculator node.
		/// </summary>
#endif
		[SerializeField] private OutputSlotGameObject _Output = new OutputSlotGameObject();

		[SerializeField]
		[HideInInspector]
		private int _SerializeVersion = 0;

		#region old

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("_Prefab")]
		private GameObject _OldPrefab = null;

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("_PostureType")]
		private PostureType _OldPostureType = PostureType.Transform;

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("_InitSpace")]
		private Space _OldInitSpace = Space.World;

		#endregion // old

		#endregion // Serialize fields

		private const int kCurrentSerializeVersion = 2;

		GameObject Internal_Instantiate(GameObject prefab)
		{
			if (_UsePool.value)
			{
				return ObjectPool.Instantiate(prefab) as GameObject;
			}
			else
			{
				return Object.Instantiate(prefab) as GameObject;
			}
		}

		GameObject Internal_Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			if (_UsePool.value)
			{
				return ObjectPool.Instantiate(prefab, position, rotation) as GameObject;
			}
			else
			{
				return Object.Instantiate(prefab, position, rotation) as GameObject;
			}
		}

		public override void OnStateBegin()
		{
			if (_Prefab != null)
			{
				bool worldPositionStays = false;

				GameObject obj = null;

				PostureType postureType = _PostureType.value;
				Space initSpace = _InitSpace.value;

				switch (postureType)
				{
					case PostureType.Transform:
						if (_InitTransform.value == null)
						{
							obj = Internal_Instantiate(_Prefab.value) as GameObject;
							worldPositionStays = false;
						}
						else
						{
							obj = Internal_Instantiate(_Prefab.value, _InitTransform.value.position, _InitTransform.value.rotation) as GameObject;
							worldPositionStays = true;
						}
						break;
					case PostureType.Directly:
						{
							switch (initSpace)
							{
								case Space.World:
									{
										obj = Internal_Instantiate(_Prefab.value, _InitPosition.value, _InitRotation.value) as GameObject;
										worldPositionStays = true;
									}
									break;
								case Space.Self:
									{
										obj = Internal_Instantiate(_Prefab.value) as GameObject;
										worldPositionStays = false;
									}
									break;
							}
						}
						break;
				}

				Transform objTransform = obj.transform;

				if (_Parent.value != null)
				{
					objTransform.SetParent(_Parent.value, worldPositionStays);
				}

				if (postureType == PostureType.Directly && initSpace == Space.Self)
				{
					objTransform.localPosition = _InitPosition.value;
					objTransform.localRotation = _InitRotation.value;
				}

				_Parameter.value = obj;

				_Output.SetValue(obj);
			}
		}

		void Reset()
		{
			_SerializeVersion = kCurrentSerializeVersion;
		}

		void SerializeVer1()
		{
			_Prefab = (FlexibleGameObject)_OldPrefab;
		}

		void SerializeVer2()
		{
			_PostureType = (FlexiblePostureType)_OldPostureType;
			_InitSpace = (FlexibleSpace)_OldInitSpace;
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
					case 1:
						SerializeVer2();
						_SerializeVersion++;
						break;
					default:
						_SerializeVersion = kCurrentSerializeVersion;
						break;
				}
			}
		}

		void INodeBehaviourSerializationCallbackReceiver.OnAfterDeserialize()
		{
			Serialize();
		}

		void INodeBehaviourSerializationCallbackReceiver.OnBeforeSerialize()
		{
			Serialize();
		}
	}
}
