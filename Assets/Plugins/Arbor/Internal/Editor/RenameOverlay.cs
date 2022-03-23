//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using System;
using System.Reflection;

namespace ArborEditor
{
	[System.Serializable]
	public sealed class RenameOverlay
	{
		private static readonly Type _RenameOverlayType;

		private static readonly PropertyInfo _NameProperty;
		private static readonly PropertyInfo _OriginalNameProperty;
		private static readonly PropertyInfo _UserAcceptedRenameNameProperty;
		private static readonly PropertyInfo _UserDataProperty;
		private static readonly PropertyInfo _IsWaitingForDelayProperty;
		private static readonly PropertyInfo _EditFieldRectProperty;
		private static readonly PropertyInfo _IsRenamingFilenameProperty;

		private static readonly MethodInfo _BeginRenameMethod;
		private static readonly MethodInfo _EndRenameMethod;
		private static readonly MethodInfo _ClearMethod;
		private static readonly MethodInfo _HasKeyboardFocusMethod;
		private static readonly MethodInfo _IsRenamingMethod;
		private static readonly MethodInfo _OnEventMethod;
		private static readonly MethodInfo _OnGUI1Method;
		private static readonly MethodInfo _OnGUI2Method;

		static Type GetType(string typeName, string assemblyName)
		{
			try
			{
				return Assembly.Load(assemblyName).GetType(typeName);
			}
			catch
			{
				return null;
			}
		}

		static RenameOverlay()
		{
			_RenameOverlayType = GetType("UnityEditor.RenameOverlay", "UnityEditor.dll");

			_NameProperty = _RenameOverlayType.GetProperty("name");
			_OriginalNameProperty = _RenameOverlayType.GetProperty("originalName");
			_UserAcceptedRenameNameProperty = _RenameOverlayType.GetProperty("userAcceptedRename");
			_UserDataProperty = _RenameOverlayType.GetProperty("userData");
			_IsWaitingForDelayProperty = _RenameOverlayType.GetProperty("isWaitingForDelay");
			_EditFieldRectProperty = _RenameOverlayType.GetProperty("editFieldRect");
			_IsRenamingFilenameProperty = _RenameOverlayType.GetProperty("isRenamingFilename");

			_BeginRenameMethod = _RenameOverlayType.GetMethod("BeginRename");
			_EndRenameMethod = _RenameOverlayType.GetMethod("EndRename");
			_ClearMethod = _RenameOverlayType.GetMethod("Clear");
			_HasKeyboardFocusMethod = _RenameOverlayType.GetMethod("HasKeyboardFocus");
			_IsRenamingMethod = _RenameOverlayType.GetMethod("IsRenaming");
			_OnEventMethod = _RenameOverlayType.GetMethod("OnEvent");
			_OnGUI1Method = _RenameOverlayType.GetMethod("OnGUI", new Type[] { }, null);
			_OnGUI2Method = _RenameOverlayType.GetMethod("OnGUI", new Type[] { typeof(GUIStyle) }, null);
		}

		private System.Object _Instance;

		private delegate string DelegateGetString();
		private DelegateGetString _DelegateGetName;
		private DelegateGetString _DelegateGetOriginalName;

		private delegate bool DelegateGetBool();
		private DelegateGetBool _DelegateGetUserAcceptedRename;
		private DelegateGetBool _DelegateGetIsWaitingForDelay;
		private DelegateGetBool _DelegateGetIsRenamingFilename;

		private delegate void DelegateSetBool(bool value);
		private DelegateSetBool _DelegateSetIsRenamingFilename;

		private delegate int DelegateGetInt();
		private DelegateGetInt _DelegateGetUserData;

		private delegate Rect DelegateGetRect();
		private DelegateGetRect _DelegateGetRditFieldRect;

		private delegate void DelegateSetRect(Rect value);
		private DelegateSetRect _DelegateSetRditFieldRect;

		private delegate bool DelegateBeginRename(string name, int userData, float delay);
		private DelegateBeginRename _DelegateBeginRename;

		private delegate void DelegateEndRename(bool acceptChanges);
		private DelegateEndRename _DelegateEndRename;

		private delegate void DelegateClear();
		private DelegateClear _DelegateClear;

		private delegate bool DelegateHasKeyboardFocus();
		private DelegateHasKeyboardFocus _DelegateHasKeyboardFocus;

		private delegate bool DelegateIsRenaming();
		private DelegateIsRenaming _DelegateIsRenaming;

		private delegate bool DelegateOnEvent();
		private DelegateOnEvent _DelegateOnEvent;

		private delegate bool DelegateOnGUI1();
		private DelegateOnGUI1 _DelegateOnGUI1;

		private delegate bool DelegateOnGUI2(GUIStyle textFieldStyle);
		private DelegateOnGUI2 _DelegateOnGUI2;

		public RenameOverlay()
		{
			_Instance = Activator.CreateInstance(_RenameOverlayType);

			_DelegateGetName = (DelegateGetString)Delegate.CreateDelegate(typeof(DelegateGetString), _Instance, _NameProperty.GetGetMethod());
			_DelegateGetOriginalName = (DelegateGetString)Delegate.CreateDelegate(typeof(DelegateGetString), _Instance, _OriginalNameProperty.GetGetMethod());

			_DelegateGetUserAcceptedRename = (DelegateGetBool)Delegate.CreateDelegate(typeof(DelegateGetBool), _Instance, _UserAcceptedRenameNameProperty.GetGetMethod());
			_DelegateGetIsWaitingForDelay = (DelegateGetBool)Delegate.CreateDelegate(typeof(DelegateGetBool), _Instance, _IsWaitingForDelayProperty.GetGetMethod());

			_DelegateGetIsRenamingFilename = (DelegateGetBool)Delegate.CreateDelegate(typeof(DelegateGetBool), _Instance, _IsRenamingFilenameProperty.GetGetMethod());
			_DelegateSetIsRenamingFilename = (DelegateSetBool)Delegate.CreateDelegate(typeof(DelegateSetBool), _Instance, _IsRenamingFilenameProperty.GetSetMethod());

			_DelegateGetUserData = (DelegateGetInt)Delegate.CreateDelegate(typeof(DelegateGetInt), _Instance, _UserDataProperty.GetGetMethod());

			_DelegateGetRditFieldRect = (DelegateGetRect)Delegate.CreateDelegate(typeof(DelegateGetRect), _Instance, _EditFieldRectProperty.GetGetMethod());
			_DelegateSetRditFieldRect = (DelegateSetRect)Delegate.CreateDelegate(typeof(DelegateSetRect), _Instance, _EditFieldRectProperty.GetSetMethod());

			_DelegateBeginRename = (DelegateBeginRename)Delegate.CreateDelegate(typeof(DelegateBeginRename), _Instance, _BeginRenameMethod);
			_DelegateEndRename = (DelegateEndRename)Delegate.CreateDelegate(typeof(DelegateEndRename), _Instance, _EndRenameMethod);
			_DelegateClear = (DelegateClear)Delegate.CreateDelegate(typeof(DelegateClear), _Instance, _ClearMethod);
			_DelegateHasKeyboardFocus = (DelegateHasKeyboardFocus)Delegate.CreateDelegate(typeof(DelegateHasKeyboardFocus), _Instance, _HasKeyboardFocusMethod);
			_DelegateIsRenaming = (DelegateIsRenaming)Delegate.CreateDelegate(typeof(DelegateIsRenaming), _Instance, _IsRenamingMethod);
			_DelegateOnEvent = (DelegateOnEvent)Delegate.CreateDelegate(typeof(DelegateOnEvent), _Instance, _OnEventMethod);
			_DelegateOnGUI1 = (DelegateOnGUI1)Delegate.CreateDelegate(typeof(DelegateOnGUI1), _Instance, _OnGUI1Method);
			_DelegateOnGUI2 = (DelegateOnGUI2)Delegate.CreateDelegate(typeof(DelegateOnGUI2), _Instance, _OnGUI2Method);
		}

		public string name
		{
			get
			{
				return _DelegateGetName();
			}
		}

		public string originalName
		{
			get
			{
				return _DelegateGetOriginalName();
			}
		}

		public bool userAcceptedRename
		{
			get
			{
				return _DelegateGetUserAcceptedRename();
			}
		}

		public int userData
		{
			get
			{
				return _DelegateGetUserData();
			}
		}

		public bool isWaitingForDelay
		{
			get
			{
				return _DelegateGetIsWaitingForDelay();
			}
		}

		public Rect editFieldRect
		{
			get
			{
				return _DelegateGetRditFieldRect();
			}
			set
			{
				_DelegateSetRditFieldRect(value);
			}
		}

		public bool isRenamingFilename
		{
			get
			{
				return _DelegateGetIsRenamingFilename();
			}
			set
			{
				_DelegateSetIsRenamingFilename(value);
			}
		}

		public bool BeginRename(string name, int userData, float delay)
		{
			return _DelegateBeginRename(name, userData, delay);
		}

		public void EndRename(bool acceptChanges)
		{
			_DelegateEndRename(acceptChanges);
		}

		public void Clear()
		{
			_DelegateClear();
		}

		public bool HasKeyboardFocus()
		{
			return _DelegateHasKeyboardFocus();
		}

		public bool IsRenaming()
		{
			return _DelegateIsRenaming();
		}

		public bool OnEvent()
		{
			return _DelegateOnEvent();
		}

		public bool OnGUI()
		{
			return _DelegateOnGUI1();
		}

		public bool OnGUI(GUIStyle textFieldStyle)
		{
			return _DelegateOnGUI2(textFieldStyle);
		}
	}
}