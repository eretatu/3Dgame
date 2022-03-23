//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
namespace ArborEditor
{
	using ArborEditor.IMGUI.Controls;

	public interface ITreeFilter
	{
		bool useFilter
		{
			get;
		}

		bool openFilter
		{
			get;
			set;
		}

		void OnFilterSettingsGUI();
		bool IsValid(TreeViewItem item);
	}
}