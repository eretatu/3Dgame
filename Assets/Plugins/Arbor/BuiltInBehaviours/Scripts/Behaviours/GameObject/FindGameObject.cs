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
	/// GameObjectを名前で検索してパラメータに格納する。
	/// </summary>
#else
	/// <summary>
	/// GameObject search by the name is then stored in the parameter.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("GameObject/FindGameObject")]
	[BuiltInBehaviour]
	public sealed class FindGameObject : StateBehaviour, INodeBehaviourSerializationCallbackReceiver
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// 格納先のパラメータ
		/// </summary>
#else
		/// <summary>
		/// Storage destination parameters
		/// </summary>
#endif
		[SerializeField]
		private GameObjectParameterReference _Reference = new GameObjectParameterReference();

#if ARBOR_DOC_JA
		/// <summary>
		/// GameObjectを出力。
		/// </summary>
#else
		/// <summary>
		/// Output GameObject
		/// </summary>
#endif
		[SerializeField]
		private OutputSlotGameObject _Output = new OutputSlotGameObject();

#if ARBOR_DOC_JA
		/// <summary>
		/// 検索する名前
		/// </summary>
#else
		/// <summary>
		/// Search for name
		/// </summary>
#endif
		[SerializeField]
		private FlexibleString _Name = new FlexibleString(string.Empty);

		[SerializeField]
		[HideInInspector]
		private int _SerializeVersion = 0;

		#region old

		[SerializeField]
		[FormerlySerializedAs("_Name")]
		[HideInInspector]
		private string _OldName = string.Empty;

		#endregion // old

		#endregion // Serialize fields

		private const int kCurrentSerializeVersion = 1;

		public override void OnStateBegin()
		{
			GameObject findObject = GameObject.Find(_Name.value);

			_Output.SetValue(findObject);

			_Reference.value = findObject;
		}

		void Reset()
		{
			_SerializeVersion = kCurrentSerializeVersion;
		}

		void SerializeVer1()
		{
			_Name = (FlexibleString)_OldName;
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
