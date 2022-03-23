//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------

namespace ArborEditor
{
	public class CalculatorBehaviourEditor : NodeBehaviourEditor
	{
		public virtual bool IsResizableNode()
		{
			return true;
		}

		public virtual float GetNodeWidth()
		{
			return 300f;
		}
	}
}