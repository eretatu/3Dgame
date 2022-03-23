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
	/// GameObjectをタグで検索してパラメータに格納する。
	/// </summary>
#else
	/// <summary>
	/// GameObject the searches in the tag and then stored in the parameter.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("GameObject/FindWithTagGameObject")]
	[BuiltInBehaviour]
	public sealed class FindWithTagGameObject : StateBehaviour, INodeBehaviourSerializationCallbackReceiver
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
		/// 検索するタグ
		/// </summary>
#else
		/// <summary>
		/// Search tag
		/// </summary>
#endif
		[SerializeField]
		[TagSelector]
		private FlexibleString _Tag = new FlexibleString("Untagged");

		[SerializeField]
		[HideInInspector]
		private int _SerializeVersion = 0;

		#region old

		[SerializeField]
		[FormerlySerializedAs("_Tag")]
		[HideInInspector]
		private string _OldTag = "Untagged";

		#endregion // old

		#endregion // Serialize fields

		private const int kCurrentSerializeVersion = 1;

		public override void OnStateBegin()
		{
			GameObject findObject = GameObject.FindWithTag(_Tag.value);

			_Output.SetValue(findObject);

			_Reference.value = findObject;
		}

		void Reset()
		{
			_SerializeVersion = kCurrentSerializeVersion;
		}

		void SerializeVer1()
		{
			_Tag = (FlexibleString)_OldTag;
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

		void INodeBehaviourSerializationCallbackReceiver.OnBeforeSerialize()
		{
			Serialize();
		}

		void INodeBehaviourSerializationCallbackReceiver.OnAfterDeserialize()
		{
			Serialize();
		}
	}
}
