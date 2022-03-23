//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------

namespace Arbor
{
#if ARBOR_DOC_JA
	/// <summary>
	/// コメントを表すクラス
	/// </summary>
#else
	/// <summary>
	/// Class that represents a comment
	/// </summary>
#endif
	[System.Serializable]
	public sealed class CommentNode : Node
	{
#if ARBOR_DOC_JA
		/// <summary>
		/// コメントIDを取得。
		/// </summary>
#else
		/// <summary>
		/// Gets the comment identifier.
		/// </summary>
#endif
		[System.Obsolete("use Node.nodeID")]
		public int commentID
		{
			get
			{
				return nodeID;
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// コメント文字列。
		/// </summary>
#else
		/// <summary>
		/// Comment string.
		/// </summary>
#endif
		public string comment = string.Empty;

		internal CommentNode(NodeGraph nodeGraph, int nodeID) : base(nodeGraph, nodeID)
		{
		}
	}
}
