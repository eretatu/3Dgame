//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using System.Reflection; // Needed to use Type.GetField with NETFX_CORE

namespace Arbor
{
#if ARBOR_DOC_JA
	/// <summary>
	/// DataBranchのリルートノード。
	/// </summary>
#else
	/// <summary>
	/// Reroute node of DataBranch.
	/// </summary>
#endif
	[System.Serializable]
	public sealed class DataBranchRerouteNode : Node, ISerializationCallbackReceiver
	{
#if ARBOR_DOC_JA
		/// <summary>
		/// リルートスロット
		/// </summary>
#else
		/// <summary>
		/// Reroute slot
		/// </summary>
#endif
		public RerouteSlot link = new RerouteSlot();

#if ARBOR_DOC_JA
		/// <summary>
		/// ラインの方向
		/// </summary>
#else
		/// <summary>
		/// Direction of line
		/// </summary>
#endif
		public Vector2 direction = Vector2.right;

#if ARBOR_DOC_JA
		/// <summary>
		/// linkのDataSlotField
		/// </summary>
#else
		/// <summary>
		/// link's DataSlotField
		/// </summary>
#endif
		[System.Obsolete("use link.slotField")]
		public DataSlotField slotField
		{
			get
			{
				return link.slotField;
			}
		}

		internal DataBranchRerouteNode(NodeGraph nodeGraph, int nodeID, System.Type type) : base(nodeGraph, nodeID)
		{
			link.type = type;

			SetupDataBranchSlotField();
		}

		[System.NonSerialized]
		private bool _DelayUpdateDataBranchSlotField = false;

		void SetupDataBranchSlotField()
		{
			FieldInfo linkFieldInfo = MemberCache.GetFieldInfo(this.GetType(), "link");
			link.SetupField(linkFieldInfo);
		}

		void RegisterUpdateSlotField()
		{
			if (_DelayUpdateDataBranchSlotField)
			{
				return;
			}

			_DelayUpdateDataBranchSlotField = true;
			if (nodeGraph != null)
			{
				if (nodeGraph.isDeserialized)
				{
					UpdateDataBranchSlotField();
				}
				else
				{
					nodeGraph.onAfterDeserialize += UpdateDataBranchSlotField;
				}
			}
		}

		void UpdateDataBranchSlotField()
		{
			_DelayUpdateDataBranchSlotField = false;

			link.ClearDataBranchSlot();
			link.SetupDataBranchSlot();
		}

		internal void RefreshDataSlots()
		{
			link.ClearDataBranchSlot();
			link.SetupDataBranchSlot();
			link.RefreshDataBranchSlot();
		}

		#region ISerializationCallbackReceiver

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			SetupDataBranchSlotField();
			RegisterUpdateSlotField();
		}

		#endregion // ISerializationCallbackReceiver
	}
}