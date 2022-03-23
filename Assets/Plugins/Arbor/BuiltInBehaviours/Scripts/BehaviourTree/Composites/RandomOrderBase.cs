//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace Arbor.BehaviourTree.Composites
{
	public abstract class RandomOrderBase : CompositeBehaviour
	{
		private sealed class Order : System.IComparable<Order>
		{
			public int branchID;
			public float weight;

			public int CompareTo(Order other)
			{
				if (other == null)
				{
					return 1;
				}

				return weight.CompareTo(other.weight);
			}
		}

		private List<Order> _ChildNodeOrders = new List<Order>();

		private int _OrderIndex = 0;

		int GetSlotIndex(int orderIndex)
		{
			List<NodeLinkSlot> childrenLink = compositeNode.childrenLink;

			if (0 <= orderIndex && orderIndex < _ChildNodeOrders.Count)
			{
				int branchID = _ChildNodeOrders[orderIndex].branchID;
				for (int i = 0, count = childrenLink.Count; i < count; i++)
				{
					NodeLinkSlot slot = childrenLink[i];
					if (slot.branchID == branchID)
					{
						return i;
					}
				}
			}

			return -1;
		}

		public override int GetBeginIndex()
		{
			_ChildNodeOrders.Clear();

			var childrenLink = compositeNode.childrenLink;
			for (int slotIndex = 0; slotIndex < childrenLink.Count; slotIndex++)
			{
				NodeLinkSlot slot = childrenLink[slotIndex];
				Order order = new Order()
				{
					branchID = slot.branchID,
					weight = Random.value,
				};
				_ChildNodeOrders.Add(order);
			}

			_ChildNodeOrders.Sort();

			_OrderIndex = 0;

			return GetSlotIndex(_OrderIndex);
		}

		public override int GetNextIndex(int index)
		{
			_OrderIndex++;

			return GetSlotIndex(_OrderIndex);
		}

		public override int GetInterruptIndex(TreeNodeBase node)
		{
			_ChildNodeOrders.Clear();

			List<NodeLinkSlot> childrenLink = compositeNode.childrenLink;

			int childCount = childrenLink.Count;
			for (int childIndex = 0; childIndex < childCount; ++childIndex)
			{
				NodeLinkSlot slot = childrenLink[childIndex];
				float weight = 0.0f;

				NodeBranch branch = behaviourTree.nodeBranchies.GetFromID(slot.branchID);
				if (branch.childNodeID == node.nodeID)
				{
					weight = -1f;
					branch.isActive = true;
				}
				else
				{
					weight = Random.value;
				}

				Order order = new Order()
				{
					branchID = slot.branchID,
					weight = weight,
				};

				_ChildNodeOrders.Add(order);
			}

			_ChildNodeOrders.Sort();

			_OrderIndex = 0;

			return GetSlotIndex(_OrderIndex);
		}
	}
}
