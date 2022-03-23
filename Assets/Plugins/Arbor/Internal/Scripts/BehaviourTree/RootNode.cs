//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------

namespace Arbor.BehaviourTree
{
#if ARBOR_DOC_JA
	/// <summary>
	/// ルートノード
	/// </summary>
#else
	/// <summary>
	/// Root Node
	/// </summary>
#endif
	[System.Serializable]
	public sealed class RootNode : TreeNodeBase
	{
		#region Serialize field

#if ARBOR_DOC_JA
		/// <summary>
		/// 子ノードへのリンク
		/// </summary>
#else
		/// <summary>
		/// Link to child nodes.
		/// </summary>
#endif
		public NodeLinkSlot childNodeLink = new NodeLinkSlot();

		#endregion // Serialize field

#if ARBOR_DOC_JA
		/// <summary>
		/// 子ノードを取得。
		/// </summary>
#else
		/// <summary>
		/// Get child node.
		/// </summary>
#endif
		public TreeNodeBase childNode
		{
			get
			{
				NodeBranch branch = behaviourTree.nodeBranchies.GetFromID(childNodeLink.branchID);
				if (branch != null)
				{
					return behaviourTree.GetNodeFromID(branch.childNodeID) as TreeNodeBase;
				}
				return null;
			}
		}

		internal RootNode(NodeGraph nodeGraph, int nodeID) : base(nodeGraph, nodeID)
		{
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 親へのNodeLinkSlotを持っているかどうか
		/// </summary>
		/// <returns>持っている場合はtrue、なければfalse。</returns>
#else
		/// <summary>
		/// Whether this node has a NodeLinkSlot to parent.
		/// </summary>
		/// <returns>True if it has a NodeLinkSlot to parent, false otherwise.</returns>
#endif
		public override bool HasParentLinkSlot()
		{
			return false;
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 子へのNodeLinkSlotを持っているかどうか
		/// </summary>
		/// <returns>持っている場合はtrue、なければfalse。</returns>
#else
		/// <summary>
		/// Whether this node has a NodeLinkSlot to child.
		/// </summary>
		/// <returns>True if it has a NodeLinkSlot to child, false otherwise.</returns>
#endif
		public override bool HasChildLinkSlot()
		{
			return true;
		}

		internal override void ConnectChildLinkSlot(int branchID)
		{
			NodeBranch oldBranch = behaviourTree.nodeBranchies.GetFromID(childNodeLink.branchID);
			if (oldBranch != null)
			{
				behaviourTree.DisconnectBranch(oldBranch);

				oldBranch = null;
				childNodeLink.branchID = 0;
			}
			childNodeLink.branchID = branchID;
		}

		internal override void DisconnectChildLinkSlot(int branchID)
		{
			childNodeLink.branchID = 0;
		}

		internal override int OnCalculateChildPriority(int order)
		{
			TreeNodeBase childNode = this.childNode;
			if (childNode != null)
			{
				order = childNode.CalculatePriority(order);
			}
			return order;
		}

		private bool _IsChildExecute = false;

		internal override bool OnActivate(bool active, bool interrupt, bool isRevaluator)
		{
			if (!base.OnActivate(active, interrupt, isRevaluator))
			{
				return false;
			}

			if (active)
			{
				_IsChildExecute = false;
			}

			return true;
		}

		private NodeStatus _ChildNodeStatus = NodeStatus.Running;

#if ARBOR_DOC_JA
		/// <summary>
		/// 実行する際に呼ばれる。
		/// </summary>
#else
		/// <summary>
		/// Called when executing.
		/// </summary>
#endif
		protected override void OnExecute()
		{
			if (_IsChildExecute)
			{
				FinishExecute(_ChildNodeStatus == NodeStatus.Success);
				return;
			}
			else
			{
				_IsChildExecute = true;

				TreeNodeBase childNode = behaviourTree.Push(childNodeLink);
				if (childNode == null)
				{
					FinishExecute(false);
					return;
				}
			}
		}

		internal override void OnChildExecuted(NodeStatus status)
		{
			_ChildNodeStatus = status;
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 削除できるかどうかを返す。
		/// </summary>
		/// <returns>削除できる場合にtrueを返す。</returns>
#else
		/// <summary>
		/// Returns whether or not it can be deleted.
		/// </summary>
		/// <returns>Returns true if it can be deleted.</returns>
#endif
		public override bool IsDeletable()
		{
			return false;
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// ノードの名前を取得
		/// </summary>
		/// <returns>ノードの名前</returns>
#else
		/// <summary>
		/// Get node name.
		/// </summary>
		/// <returns>Node name</returns>
#endif
		public override string GetName()
		{
			return "Root";
		}
	}
}