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
	/// プレイ可能な挙動。PlayableGraphから参照する挙動に使用する。
	/// </summary>
#else
	/// <summary>
	/// Playable behavior, used for behavior that is referenced from a PlayableGraph.
	/// </summary>
#endif
	[AddComponentMenu("")]
	public abstract class PlayableBehaviour : NodeBehaviour
	{
		private bool _IsAwake = false;

		private bool _IsActive;

		internal bool isActive
		{
			get
			{
				return _IsActive;
			}
		}

		void CallActiveEvent()
		{
			if (_IsActive)
			{
				return;
			}
			_IsActive = true;

			IPlayableBehaviourCallbackReceiver receiver = this as IPlayableBehaviourCallbackReceiver;

			if (receiver != null)
			{
				UpdateDataLink(DataLinkUpdateTiming.Enter);
			}

			if (!_IsAwake)
			{
				_IsAwake = true;

				if (receiver != null)
				{
					try
					{
#if ARBOR_PROFILER && (DEVELOPMENT_BUILD || UNITY_EDITOR)
						using (new ProfilerScope(GetProfilerName("OnAwake()")))
#endif
						{
							receiver.OnAwake();
						}
					}
					catch (System.Exception ex)
					{
						Debug.LogException(ex, this);
					}
				}
			}

			if (receiver != null)
			{
				try
				{
#if ARBOR_PROFILER && (DEVELOPMENT_BUILD || UNITY_EDITOR)
					using (new ProfilerScope(GetProfilerName("OnStart()")))
#endif
					{
						receiver.OnStart();
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogException(ex, this);
				}
			}
		}

		void CallInactiveEvent()
		{
			if (!_IsActive)
			{
				return;
			}
			_IsActive = false;

			IPlayableBehaviourCallbackReceiver receiver = this as IPlayableBehaviourCallbackReceiver;
			if (receiver != null)
			{
				UpdateDataLink(DataLinkUpdateTiming.Execute);

				try
				{
#if ARBOR_PROFILER && (DEVELOPMENT_BUILD || UNITY_EDITOR)
					using (new ProfilerScope(GetProfilerName("OnEnd()")))
#endif
					{
						receiver.OnEnd();
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogException(ex, this);
				}
			}
		}

		void CallUpdateInternal()
		{
			if (!_IsActive)
			{
				return;
			}

			IPlayableBehaviourCallbackReceiver receiver = this as IPlayableBehaviourCallbackReceiver;
			if (receiver != null)
			{
				UpdateDataLink(DataLinkUpdateTiming.Execute);

				try
				{
#if ARBOR_PROFILER && (DEVELOPMENT_BUILD || UNITY_EDITOR)
					using (new ProfilerScope(GetProfilerName("OnStateUpdate()")))
#endif
					{
						receiver.OnUpdate();
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogException(ex, this);
				}
			}
		}

		void CallLateUpdateInternal()
		{
			if (!_IsActive)
			{
				return;
			}

			IPlayableBehaviourCallbackReceiver receiver = this as IPlayableBehaviourCallbackReceiver;
			if (receiver != null)
			{
				UpdateDataLink(DataLinkUpdateTiming.Execute);

				try
				{
#if ARBOR_PROFILER && (DEVELOPMENT_BUILD || UNITY_EDITOR)
					using (new ProfilerScope(GetProfilerName("OnStateUpdate()")))
#endif
					{
						receiver.OnLateUpdate();
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogException(ex, this);
				}
			}
		}

		void CallFixedUpdateInternal()
		{
			if (!_IsActive)
			{
				return;
			}

			IPlayableBehaviourCallbackReceiver receiver = this as IPlayableBehaviourCallbackReceiver;
			if (receiver != null)
			{
				UpdateDataLink(DataLinkUpdateTiming.Execute);

				try
				{
#if ARBOR_PROFILER && (DEVELOPMENT_BUILD || UNITY_EDITOR)
					using (new ProfilerScope(GetProfilerName("OnStateUpdate()")))
#endif
					{
						receiver.OnFixedUpdate();
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogException(ex, this);
				}
			}
		}

		internal void ActivateInternal(bool active, bool changeState)
		{
			if (active)
			{
				if (!enabled)
				{
					enabled = true;
					if (changeState)
					{
						CallActiveEvent();
					}
				}
			}
			else
			{
				if (enabled)
				{
					if (changeState)
					{
						CallInactiveEvent();
					}

					enabled = false;
				}
			}
		}

		internal void PauseInternal()
		{
			if (_IsActive)
			{
				CallPauseEvent();
			}
			enabled = false;
		}

		internal void ResumeInternal()
		{
			enabled = true;
			if (_IsActive)
			{
				CallResumeEvent();
			}
		}

		internal void StopInternal()
		{
			if (_IsActive)
			{
				CallStopEvent();
				CallInactiveEvent();
			}

			enabled = false;
		}

		internal void UpdateInternal()
		{
			CallUpdateInternal();
		}

		internal void LateUpdateInternal()
		{
			CallLateUpdateInternal();
		}

		internal void FixedUpdateInternal()
		{
			CallFixedUpdateInternal();
		}
	}
}