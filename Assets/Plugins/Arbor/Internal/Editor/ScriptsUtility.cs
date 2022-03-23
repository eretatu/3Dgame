//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ArborEditor
{
	internal static class ScriptsUtility
	{
		public static ReadOnlyCollection<MonoScript> scripts
		{
			get;
			private set;
		}

		public static ReadOnlyCollection<System.Type> scriptTypes
		{
			get;
			private set;
		}

		static ScriptsUtility()
		{
			List<MonoScript> _scripts = new List<MonoScript>();
			List<System.Type> _types = new List<System.Type>();

			var monoScripts = MonoImporter.GetAllRuntimeMonoScripts();
			for (int i = 0; i < monoScripts.Length; i++)
			{
				MonoScript script = monoScripts[i];
				if (script == null || script.hideFlags != 0)
				{
					continue;
				}

				System.Type classType = script.GetClass();
				if (classType == null)
				{
					continue;
				}

				if (!_scripts.Contains(script))
				{
					_scripts.Add(script);
				}

				if(!_types.Contains(classType))
				{
					_types.Add(classType);
				}
			}

			scripts = _scripts.AsReadOnly();
			scriptTypes = _types.AsReadOnly();
		}
	}
}