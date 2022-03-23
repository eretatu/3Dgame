//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.BehaviourTree
{
#if ARBOR_DOC_JA
	/// <summary>
	/// アクションの挙動を定義するクラス。継承して利用する。
	/// </summary>
	/// <remarks>
	/// 使用可能な属性 : <br/>
	/// <list type="bullet">
	/// <item><description><see cref="AddBehaviourMenu" /></description></item>
	/// <item><description><see cref="HideBehaviour" /></description></item>
	/// <item><description><see cref="BehaviourTitle" /></description></item>
	/// <item><description><see cref="BehaviourHelp" /></description></item>
	/// </list>
	/// </remarks>
#else
	/// <summary>
	/// Class that defines the behavior of the action. Inherited and to use.
	/// </summary>
	/// <remarks>
	/// Available Attributes : <br/>
	/// <list type="bullet">
	/// <item><description><see cref="AddBehaviourMenu" /></description></item>
	/// <item><description><see cref="HideBehaviour" /></description></item>
	/// <item><description><see cref="BehaviourTitle" /></description></item>
	/// <item><description><see cref="BehaviourHelp" /></description></item>
	/// </list>
	/// </remarks>
#endif
	[AddComponentMenu("")]
	[Internal.DocumentManual("/manual/scripting/behaviourtree/actionbehaviour.md")]
	public class ActionBehaviour : TreeNodeBehaviour
	{
#if ARBOR_DOC_JA
		/// <summary>
		/// ActionNodeを取得。
		/// </summary>
#else
		/// <summary>
		/// Get the ActionNode.
		/// </summary>
#endif
		public ActionNode actionNode
		{
			get
			{
				return node as ActionNode;
			}
		}

		internal static ActionBehaviour Create(Node node, System.Type type)
		{
			System.Type classType = typeof(ActionBehaviour);
			if (type != classType && !TypeUtility.IsSubclassOf(type, classType))
			{
				throw new System.ArgumentException("The type `" + type.Name + "' must be convertible to `ActionBehaviour' in order to use it as parameter `type'", "type");
			}

			return CreateNodeBehaviour(node, type) as ActionBehaviour;
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 実行を終了する。
		/// </summary>
		/// <param name="result">実行結果</param>
#else
		/// <summary>
		/// Finish Execute.
		/// </summary>
		/// <param name="result">Execute result.</param>
#endif
		protected void FinishExecute(bool result)
		{
			actionNode.FinishExecute(result);
		}

		internal void CallExecuteInternal()
		{
			UpdateDataLink(DataLinkUpdateTiming.Execute);

			try
			{
#if ARBOR_PROFILER && (DEVELOPMENT_BUILD || UNITY_EDITOR)
				using (new ProfilerScope(GetProfilerName("OnExecute()")))
#endif
				{
					OnExecute();
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogException(ex, this);
			}
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
		protected virtual void OnExecute()
		{
			FinishExecute(false);
		}
	}
}