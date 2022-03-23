//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using System.Collections;

namespace ArborEditor
{
	using Arbor;
	using ArborEditor.IMGUI.Controls;

	public class GraphTreeViewItem : TreeViewItem
	{
		public virtual NodeGraph nodeGraph
		{
			get;
			private set;
		}

		public virtual bool isExternal
		{
			get
			{
				return false;
			}
		}

		public override string displayName
		{
			get
			{
				return nodeGraph.graphName;
			}
			set
			{
				nodeGraph.graphName = value;
			}
		}

		public GraphTreeViewItem(int id) : base(id, "", null)
		{
		}

		public GraphTreeViewItem(NodeGraph nodeGraph) : this(nodeGraph.GetInstanceID())
		{
			this.nodeGraph = nodeGraph;
		}
	}
}