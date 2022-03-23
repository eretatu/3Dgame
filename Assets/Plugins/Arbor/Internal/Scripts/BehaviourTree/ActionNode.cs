//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.BehaviourTree
{
#if ARBOR_DOC_JA
	/// <summary>
	/// アクションを実行するノード
	/// </summary>
#else
	/// <summary>
	/// The node that executes the action
	/// </summary>
#endif
	[System.Serializable]
	public sealed class ActionNode : TreeBehaviourNode
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// 親ノードへのリンク
		/// </summary>
#else
		/// <summary>
		/// Link to parent node
		/// </summary>
#endif
		public NodeLinkSlot parentLink = new NodeLinkSlot();

#if ARBOR_DOC_JA
		/// <summary>
		/// ノード名。
		/// </summary>
#else
		/// <summary>
		/// Node name.
		/// </summary>
#endif
		public string name = "New Action";

		#endregion // Serialize fields

		internal ActionNode(NodeGraph nodeGraph, int nodeID, System.Type classType)
			: base(nodeGraph, nodeID)
		{
			CreateActionBehaviour(classType);
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// ActionBehaviourを作成する。エディタで使用する。
		/// </summary>
		/// <param name="classType">ActionBehaviourの型</param>
		/// <returns>作成したActionBehaviourを返す。</returns>
#else
		/// <summary>
		/// Create a ActionBehaviour. Use it in the editor.
		/// </summary>
		/// <param name="classType">ActionBehaviour type</param>
		/// <returns>Returns the created ActionBehaviour.</returns>
#endif
		public ActionBehaviour CreateActionBehaviour(System.Type classType)
		{
			ActionBehaviour behaviour = ActionBehaviour.Create(this, classType);
			SetBehaviour(behaviour);
			return behaviour;
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
			return true;
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 親へのNodeLinkSlotを取得。
		/// </summary>
		/// <returns>親へのNodeLinkSlot</returns>
#else
		/// <summary>
		/// Get NodeLinkSlot to parent.
		/// </summary>
		/// <returns>NodeLinkSlot to parent</returns>
#endif
		public override NodeLinkSlot GetParentLinkSlot()
		{
			return parentLink;
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
			return false;
		}

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
			ActionBehaviour actionBehaviour = behaviour as ActionBehaviour;
			if (actionBehaviour == null)
			{
				FinishExecute(false);
				return;
			}

			actionBehaviour.CallExecuteInternal();
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
			return name;
		}
	}
}