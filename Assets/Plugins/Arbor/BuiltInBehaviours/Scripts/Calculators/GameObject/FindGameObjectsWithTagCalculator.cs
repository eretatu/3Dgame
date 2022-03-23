//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arbor.Calculators
{
#if ARBOR_DOC_JA
	/// <summary>
	/// 指定タグがついたアクティブGameObjectを探して返す。
	/// </summary>
	/// <remarks>
	/// 詳しくは、<a href="https://docs.unity3d.com/ScriptReference/GameObject.FindGameObjectsWithTag.html">GameObject.FindGameObjectsWithTag</a>を参照してください。
	/// </remarks>
#else
	/// <summary>
	/// Finds and returns active GameObjects with the specified tag.
	/// </summary>
	/// <remarks>
	/// For more information, see <a href="https://docs.unity3d.com/ScriptReference/GameObject.FindGameObjectsWithTag.html">GameObject.FindGameObjectsWithTag</a>.
	/// </remarks>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("GameObject/FindGameObjectsWithTag")]
	[BehaviourTitle("FindGameObjectsWithTag")]
	[BuiltInBehaviour]
	public sealed class FindGameObjectsWithTagCalculator : Calculator
	{
#if ARBOR_DOC_JA
		/// <summary>
		/// タグ
		/// </summary>
#else
		/// <summary>
		/// Tag
		/// </summary>
#endif
		[SerializeField]
		[TagSelector]
		private FlexibleString _Tag = new FlexibleString("Untagged");

#if ARBOR_DOC_JA
		/// <summary>
		/// 見つかったGameObjectを出力する。
		/// </summary>
#else
		/// <summary>
		/// Output the found GameObjects.
		/// </summary>
#endif
		[SerializeField]
		[SlotType(typeof(IList<GameObject>))]
		private OutputSlotAny _Output = new OutputSlotAny();

		// Use this for calculate
		public override void OnCalculate()
		{
			var gameObjects = GameObject.FindGameObjectsWithTag(_Tag.value);
			_Output.SetValue<IList<GameObject>>(gameObjects);
		}
	}
}