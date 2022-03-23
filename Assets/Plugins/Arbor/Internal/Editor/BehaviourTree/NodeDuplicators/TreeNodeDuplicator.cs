//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEditor;
using System.Collections.Generic;

namespace ArborEditor.BehaviourTree
{
	using Arbor.BehaviourTree;

	internal abstract class TreeNodeDuplicator : NodeDuplicator
	{
		public TreeNodeBase sourceTreeNode
		{
			get
			{
				return sourceNode as TreeNodeBase;
			}
		}

		public TreeNodeBase destTreeNode
		{
			get
			{
				return destNode as TreeNodeBase;
			}
		}

		public BehaviourTreeInternal targetBehaviourTree
		{
			get
			{
				return targetGraph as BehaviourTreeInternal;
			}
		}

		public void ReconnectBranch(List<NodeDuplicator> duplicators)
		{
			if (!sourceTreeNode.HasParentLinkSlot())
			{
				return;
			}

			NodeLinkSlot parentLink = sourceTreeNode.GetParentLinkSlot();
			NodeBranch parentBranch = sourceTreeNode.behaviourTree.nodeBranchies.GetFromID(parentLink.branchID);
			if (parentBranch != null)
			{
				TreeNodeBase parentNode = sourceTreeNode.behaviourTree.GetNodeFromID(parentBranch.parentNodeID) as TreeNodeBase;
				if (parentNode != null)
				{
					CompositeNode parentCompositeNode = parentNode as CompositeNode;
					if (parentCompositeNode != null)
					{
						bool found = false;

						for (int i = 0; i < duplicators.Count; i++)
						{
							NodeDuplicator duplicator = duplicators[i];
							if (duplicator.sourceNode.nodeID == parentCompositeNode.nodeID)
							{
								if (isClip)
								{
									targetBehaviourTree.ConnectBranch(parentBranch.branchID, duplicator.destNode as TreeNodeBase, destTreeNode);
								}
								else
								{
									targetBehaviourTree.ConnectBranch(duplicator.destNode as TreeNodeBase, destTreeNode);
								}
								found = true;
								break;
							}
						}

						if (!found)
						{
							if (Clipboard.IsSameNodeGraph(sourceTreeNode.behaviourTree, targetBehaviourTree))
							{
								parentNode = targetBehaviourTree.GetNodeFromID(parentBranch.parentNodeID) as TreeNodeBase;
								if (parentNode != null)
								{
									targetBehaviourTree.ConnectBranch(parentNode, destTreeNode);
								}
							}
							else if (isClip)
							{
								NodeBranch branch = new NodeBranch();
								branch.branchID = parentBranch.branchID;
								branch.parentNodeID = parentNode.nodeID;
								branch.childNodeID = destTreeNode.nodeID;

								Undo.RecordObject(targetBehaviourTree, "Connect NodeBranch");

								targetBehaviourTree.nodeBranchies.Add(branch);

								destTreeNode.GetParentLinkSlot().branchID = branch.branchID;

								EditorUtility.SetDirty(targetBehaviourTree);
							}
						}
					}
				}
				else if (Clipboard.IsSameNodeGraph(sourceTreeNode.behaviourTree, targetBehaviourTree))
				{
					parentNode = targetBehaviourTree.GetNodeFromID(parentBranch.parentNodeID) as TreeNodeBase;
					if (parentNode != null)
					{
						targetBehaviourTree.ConnectBranch(parentNode, destTreeNode);
					}
				}
			}
		}
	}
}