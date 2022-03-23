//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arbor.Playables
{
#if ARBOR_DOC_JA
	/// <summary>
	/// プレイ可能な挙動のコールバックを定義するインターフェイス
	/// </summary>
#else
	/// <summary>
	/// Interface that defines callbacks for playable behavior
	/// </summary>
#endif
	public interface IPlayableBehaviourCallbackReceiver
	{
#if ARBOR_DOC_JA
		/// <summary>
		/// 挙動が初めてアクティブになったときに呼ばれる。
		/// </summary>
#else
		/// <summary>
		/// Called when the behavior is first activated.
		/// </summary>
#endif
		void OnAwake();

#if ARBOR_DOC_JA
		/// <summary>
		/// 挙動がアクティブになったときに呼ばれる。
		/// </summary>
#else
		/// <summary>
		/// Called when the behavior becomes active.
		/// </summary>
#endif
		void OnStart();

#if ARBOR_DOC_JA
		/// <summary>
		/// 挙動がアクティブの間、毎フレーム更新する際に呼ばれる。
		/// </summary>
#else
		/// <summary>
		/// Called every frame while the behavior is active.
		/// </summary>
#endif
		void OnUpdate();

#if ARBOR_DOC_JA
		/// <summary>
		/// 挙動がアクティブの間、LateUpdateで呼ばれる。
		/// </summary>
#else
		/// <summary>
		/// Called by LateUpdate while the behavior is active.
		/// </summary>
#endif
		void OnLateUpdate();

#if ARBOR_DOC_JA
		/// <summary>
		/// 挙動がアクティブの間、FixedUpdateで呼ばれる。
		/// </summary>
#else
		/// <summary>
		/// Called by FixedUpdate while the behavior is active.
		/// </summary>
#endif
		void OnFixedUpdate();

#if ARBOR_DOC_JA
		/// <summary>
		/// 挙動が終了したときに呼ばれる。
		/// </summary>
#else
		/// <summary>
		/// Called when the behavior has ended.
		/// </summary>
#endif
		void OnEnd();
	}
}