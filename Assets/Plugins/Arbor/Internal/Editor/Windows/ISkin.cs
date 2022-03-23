//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
#if !ARBOR_DLL
#define ARBOR_EDITOR_EXTENSIBLE
#endif

#if ARBOR_EDITOR_EXTENSIBLE
namespace ArborEditor
{
	public interface ISkin
	{
		bool isDarkSkin
		{
			get;
		}

		void Begin();
		void End();
	}
}

#endif