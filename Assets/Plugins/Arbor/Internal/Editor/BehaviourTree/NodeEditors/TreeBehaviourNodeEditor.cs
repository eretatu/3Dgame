//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.BehaviourTree
{
	using Arbor;
	using Arbor.BehaviourTree;

	public abstract class TreeBehaviourNodeEditor : TreeNodeBaseEditor
	{
		public TreeBehaviourNode treeBehaviourNode
		{
			get
			{
				return node as TreeBehaviourNode;
			}
		}

		private DecoratorEditorList _DecoratorEditorList = new DecoratorEditorList();
		private ServiceEditorList _ServiceEditorList = new ServiceEditorList();

		private BehaviourEditorGUI _MainEditorGUI = null;

		protected override void OnInitialize()
		{
			_DecoratorEditorList.nodeEditor = this;
			_ServiceEditorList.nodeEditor = this;

			CreateEditors();
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		protected virtual void OnEnable()
		{
			_DecoratorEditorList.nodeEditor = this;
			_ServiceEditorList.nodeEditor = this;
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		protected virtual void OnDestroy()
		{
			DestroyEditors();
		}

		public override void Validate(Node node)
		{
			base.Validate(node);

			if (_MainEditorGUI != null)
			{
				_MainEditorGUI.Validate();
			}

			_DecoratorEditorList.nodeEditor = this;
			_DecoratorEditorList.Validate();

			_ServiceEditorList.nodeEditor = this;
			_ServiceEditorList.Validate();
		}

		void CreateEditors()
		{
			if (treeBehaviourNode != null)
			{
				Object mainBehaviour = treeBehaviourNode.GetBehaviourObject();
				_MainEditorGUI = new TreeNodeBehaviourEditorGUI();
				_MainEditorGUI.Initialize(this, mainBehaviour);

				_DecoratorEditorList.RebuildBehaviourEditors();

				_ServiceEditorList.RebuildBehaviourEditors();
			}
		}

		void DestroyEditors()
		{
			if (_MainEditorGUI != null)
			{
				_MainEditorGUI.DestroyEditor();
			}
			_MainEditorGUI = null;

			_DecoratorEditorList.DestroyEditors();
			_ServiceEditorList.DestroyEditors();
		}

		protected override void OnGUI()
		{
			using (new ProfilerScope("OnTreeBehaviourNodeGUI"))
			{
				{
					DecoratorList decoratorList = treeBehaviourNode.decoratorList;
					int decoratorCount = decoratorList.count;

					_DecoratorEditorList.OnGUI();

					if (decoratorCount == 0)
					{
						GUILayout.Space(-EditorGUIUtility.standardVerticalSpacing);
					}
				}

				BehaviourEditorGUI mainEditor = GetMainEditor();
				if (mainEditor != null)
				{
					mainEditor.OnGUI();
				}

				{
					_ServiceEditorList.OnGUI();
				}
			}
		}

		public void AddDecorator(System.Type classType)
		{
			Object decoratorObj = treeBehaviourNode.AddDecorator(classType);

			DecoratorList decoratorList = treeBehaviourNode.decoratorList;
			int decoratorCount = decoratorList.count;
			_DecoratorEditorList.InsertBehaviourEditor(decoratorCount - 1, decoratorObj);

			graphEditor.RaiseOnChangedNodes();
		}

		public void InsertDecorator(int index, System.Type classType)
		{
			Object decoratorObj = treeBehaviourNode.InsertDecorator(index, classType);

			_DecoratorEditorList.InsertBehaviourEditor(index, decoratorObj);

			graphEditor.RaiseOnChangedNodes();
		}

		public void MoveDecorator(int fromIndex, int toIndex)
		{
			NodeGraph nodeGraph = treeBehaviourNode.nodeGraph;

			Undo.IncrementCurrentGroup();

			Undo.RecordObject(nodeGraph, (fromIndex > toIndex) ? "MoveUp Behaviour" : "MoveDown Behaviour");

			treeBehaviourNode.decoratorList.Swap(fromIndex, toIndex);

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			EditorUtility.SetDirty(nodeGraph);

			_DecoratorEditorList.MoveBehaviourEditor(fromIndex, toIndex);
		}

		void AddDecoratorMenu(object obj)
		{
			Rect position = (Rect)obj;

			DecoratorMenuWindow.instance.Init(this, position, -1);
		}

		public void PasteDecorator(int index)
		{
			Undo.IncrementCurrentGroup();

			TreeBehaviourNode node = treeBehaviourNode;
			NodeGraph nodeGraph = node.nodeGraph;

			Undo.RecordObject(nodeGraph, "Paste Decorator");

			PasteDecoratorAsNew(node, index);

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			EditorUtility.SetDirty(nodeGraph);

			graphEditor.RaiseOnChangedNodes();
		}

		internal static void PasteDecoratorAsNew(TreeBehaviourNode behaviourNode, int index)
		{
			Decorator destDecorator = Clipboard.copyBehaviour as Decorator;
			if (destDecorator == null)
			{
				return;
			}

			PasteDecoratorAsNew(behaviourNode, index, destDecorator);
		}

		internal static void PasteDecoratorAsNew(TreeBehaviourNode behaviourNode, int index, Decorator destDecorator)
		{
			Decorator decorator = NodeBehaviour.CreateNodeBehaviour(behaviourNode, destDecorator.GetType(), true) as Decorator;

			if (index == -1)
			{
				behaviourNode.decoratorList.Add(decorator);
			}
			else
			{
				behaviourNode.decoratorList.Insert(index, decorator);
			}

			Clipboard.CopyNodeBehaviour(destDecorator, decorator, true);
		}

		void PasteDecoratorAsNewContextMenu()
		{
			PasteDecorator(-1);
		}

		public void AddService(System.Type classType)
		{
			Object serviceObj = treeBehaviourNode.AddService(classType);

			ServiceList serviceList = treeBehaviourNode.serviceList;
			int serviceCount = serviceList.count;
			_ServiceEditorList.InsertBehaviourEditor(serviceCount - 1, serviceObj);

			graphEditor.RaiseOnChangedNodes();
		}

		public void InsertService(int index, System.Type classType)
		{
			Object serviceObj = treeBehaviourNode.InsertService(index, classType);

			_ServiceEditorList.InsertBehaviourEditor(index, serviceObj);

			graphEditor.RaiseOnChangedNodes();
		}

		public void MoveService(int fromIndex, int toIndex)
		{
			NodeGraph nodeGraph = treeBehaviourNode.nodeGraph;

			Undo.IncrementCurrentGroup();

			Undo.RecordObject(nodeGraph, (fromIndex > toIndex) ? "MoveUp Behaviour" : "MoveDown Behaviour");

			treeBehaviourNode.serviceList.Swap(fromIndex, toIndex);

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			EditorUtility.SetDirty(nodeGraph);

			_ServiceEditorList.MoveBehaviourEditor(fromIndex, toIndex);
		}

		void AddServiceMenu(object obj)
		{
			Rect position = (Rect)obj;

			ServiceMenuWindow.instance.Init(this, position, -1);
		}

		public void PasteService(int index)
		{
			Undo.IncrementCurrentGroup();

			TreeBehaviourNode node = treeBehaviourNode;
			NodeGraph nodeGraph = node.nodeGraph;

			Undo.RecordObject(nodeGraph, "Paste Service");

			PasteServiceAsNew(node, index);

			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			EditorUtility.SetDirty(nodeGraph);

			graphEditor.RaiseOnChangedNodes();
		}

		internal static void PasteServiceAsNew(TreeBehaviourNode behaviourNode, int index)
		{
			Service destService = Clipboard.copyBehaviour as Service;
			if (destService == null)
			{
				return;
			}

			PasteServiceAsNew(behaviourNode, index, destService);
		}

		internal static void PasteServiceAsNew(TreeBehaviourNode behaviourNode, int index, Service destService)
		{
			Service service = NodeBehaviour.CreateNodeBehaviour(behaviourNode, destService.GetType(), true) as Service;

			if (index == -1)
			{
				behaviourNode.serviceList.Add(service);
			}
			else
			{
				behaviourNode.serviceList.Insert(index, service);
			}

			Clipboard.CopyNodeBehaviour(destService, service, true);
		}

		void PasteServiceAsNewContextMenu()
		{
			PasteService(-1);
		}

		public override void ExpandAll(bool expanded)
		{
			BehaviourEditorGUI mainEditor = GetMainEditor();
			if (mainEditor != null)
			{
				mainEditor.SetExpanded(expanded);
			}

			DecoratorList decoratorList = treeBehaviourNode.decoratorList;
			int decoratorCount = decoratorList.count;
			for (int decoratorIndex = 0; decoratorIndex < decoratorCount; decoratorIndex++)
			{
				BehaviourEditorGUI behaviourEditor = GetDecoratorEditor(decoratorIndex);
				if (behaviourEditor != null)
				{
					behaviourEditor.SetExpanded(expanded);
				}
			}

			ServiceList serviceList = treeBehaviourNode.serviceList;
			int serviceCount = serviceList.count;
			for (int serviceIndex = 0; serviceIndex < serviceCount; serviceIndex++)
			{
				BehaviourEditorGUI behaviourEditor = GetServiceEditor(serviceIndex);
				if (behaviourEditor != null)
				{
					behaviourEditor.SetExpanded(expanded);
				}
			}
		}

		void ExpandAll()
		{
			ExpandAll(true);
		}

		void FoldAll()
		{
			ExpandAll(false);
		}

		void FlipBreakPoint()
		{
			NodeGraph nodeGraph = treeBehaviourNode.nodeGraph;

			if (treeBehaviourNode.breakPoint)
			{
				Undo.RecordObject(nodeGraph, "Node BreakPoint Off");
			}
			else
			{
				Undo.RecordObject(nodeGraph, "Node BreakPoint On");
			}

			treeBehaviourNode.breakPoint = !treeBehaviourNode.breakPoint;

			EditorUtility.SetDirty(nodeGraph);
		}

		protected virtual void SetReplaceBehaviourMenu(GenericMenu menu, Rect headerPosition, bool editable)
		{
		}

		protected override void SetContextMenu(GenericMenu menu, Rect headerPosition, bool editable)
		{
			if (editable)
			{
				menu.AddItem(EditorContents.breakPoint, treeBehaviourNode.breakPoint, FlipBreakPoint);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.breakPoint);
			}

			menu.AddSeparator("");

			int itemCount = menu.GetItemCount();

			SetReplaceBehaviourMenu(menu, headerPosition, editable);

			if (menu.GetItemCount() > itemCount)
			{
				menu.AddSeparator("");
			}

			if (editable)
			{
				menu.AddItem(EditorContents.addDecorator, false, AddDecoratorMenu, EditorGUITools.GUIToScreenRect(headerPosition));
				menu.AddItem(EditorContents.addService, false, AddServiceMenu, EditorGUITools.GUIToScreenRect(headerPosition));
			}
			else
			{
				menu.AddDisabledItem(EditorContents.addDecorator);
				menu.AddDisabledItem(EditorContents.addService);
			}

			if (Clipboard.CompareBehaviourType(typeof(Decorator), true) && editable)
			{
				menu.AddItem(EditorContents.pasteDecoratorAsNew, false, PasteDecoratorAsNewContextMenu);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.pasteDecoratorAsNew);
			}

			if (Clipboard.CompareBehaviourType(typeof(Service), true) && editable)
			{
				menu.AddItem(EditorContents.pasteServiceAsNew, false, PasteServiceAsNewContextMenu);
			}
			else
			{
				menu.AddDisabledItem(EditorContents.pasteServiceAsNew);
			}

			menu.AddSeparator("");

			menu.AddItem(EditorContents.expandAll, false, ExpandAll);

			menu.AddItem(EditorContents.collapseAll, false, FoldAll);
		}

		public enum BehaviourType
		{
			Main,
			Decorator,
			Service,
		}

		public static BehaviourType GetBehaviourType(Object behaviourObj)
		{
			if (behaviourObj is Decorator)
			{
				return BehaviourType.Decorator;
			}
			else if (behaviourObj is Service)
			{
				return BehaviourType.Service;
			}

			return BehaviourType.Main;
		}

		public BehaviourEditorGUI GetBehaviourEditor(Object behaviourObj)
		{
			return GetBehaviourEditor(behaviourObj, GetBehaviourType(behaviourObj));
		}

		public BehaviourEditorGUI GetBehaviourEditor(Object behaviourObj, BehaviourType behaviourType)
		{
			switch (behaviourType)
			{
				case BehaviourType.Main:
					return GetMainEditor();
				case BehaviourType.Decorator:
					int decoratorIndex = treeBehaviourNode.decoratorList.IndexOf(behaviourObj);
					return GetDecoratorEditor(decoratorIndex);
				case BehaviourType.Service:
					int serviceIndex = treeBehaviourNode.serviceList.IndexOf(behaviourObj);
					return GetServiceEditor(serviceIndex);
			}

			return null;
		}

		public void DestroyDecoratorAt(int decoratorIndex)
		{
			_DecoratorEditorList.RemoveBehaviourEditor(decoratorIndex);

			Undo.IncrementCurrentGroup();
			int undoGruop = Undo.GetCurrentGroup();

			treeBehaviourNode.decoratorList.Destroy(treeBehaviourNode, decoratorIndex);

			Undo.CollapseUndoOperations(undoGruop);

			graphEditor.RaiseOnChangedNodes();
		}

		public void DestroyServiceAt(int serviceIndex)
		{
			_ServiceEditorList.RemoveBehaviourEditor(serviceIndex);

			Undo.IncrementCurrentGroup();
			int undoGruop = Undo.GetCurrentGroup();

			treeBehaviourNode.serviceList.Destroy(treeBehaviourNode, serviceIndex);

			Undo.CollapseUndoOperations(undoGruop);

			graphEditor.RaiseOnChangedNodes();
		}

		public BehaviourEditorGUI GetMainEditor()
		{
			BehaviourEditorGUI behaviourEditor = _MainEditorGUI;

			if (!ComponentUtility.IsValidObject(behaviourEditor.behaviourObj))
			{
				Object behaviourObj = treeBehaviourNode.GetBehaviourObject();
				behaviourEditor.Repair(behaviourObj);
			}

			return behaviourEditor;
		}

		internal DecoratorEditorGUI GetDecoratorEditor(int decoratorIndex)
		{
			return _DecoratorEditorList.GetBehaviourEditor(decoratorIndex);
		}

		internal ServiceEditorGUI GetServiceEditor(int serviceIndex)
		{
			return _ServiceEditorList.GetBehaviourEditor(serviceIndex);
		}

		protected virtual Texture2D GetDefaultIcon()
		{
			return null;
		}

		public override Texture2D GetIcon()
		{
			Texture icon = EditorGUITools.GetThumbnailContent(treeBehaviourNode.behaviour).image;
			if (icon != null && !DefaultScriptIcon.IsDefaultScriptIcon(icon))
			{
				return icon as Texture2D;
			}
			return GetDefaultIcon();
		}

		GUIStyle GetBreakPointStyle()
		{
			return (Application.isPlaying && EditorApplication.isPaused && treeBehaviourNode.isActive) ? Styles.breakpointOn : Styles.breakpoint;
		}

		public override void OnListElement(Rect rect)
		{
			base.OnListElement(rect);

			if (treeBehaviourNode.breakPoint)
			{
				GUIStyle style = GetBreakPointStyle();
				GUIContent content = EditorContents.breakPointTooltip;
				Vector2 size = style.CalcSize(content);
				Rect breakRect = new Rect(rect.x, rect.center.y - size.y * 0.5f, size.x, size.y);
				EditorGUI.LabelField(breakRect, content, style);
			}
		}

		protected override bool HasOutsideGUI()
		{
			return true;
		}

		bool IsVisibleDataLinkGUI()
		{
			if (_DecoratorEditorList.IsVisibleDataLinkGUI())
			{
				return true;
			}

			BehaviourEditorGUI mainEditor = GetMainEditor();
			if (mainEditor != null && mainEditor.IsVisibleDataLinkGUI())
			{
				return true;
			}

			if (_ServiceEditorList.IsVisibleDataLinkGUI())
			{
				return true;
			}

			return false;
		}

		public override bool IsCopyable()
		{
			return treeBehaviourNode.behaviour != null;
		}

		protected override RectOffset GetOutsideOffset()
		{
			RectOffset offset = IsVisibleDataLinkGUI() ? new RectOffset(DataSlotGUIUtility.kOutsideOffset, 0, 0, 0) : new RectOffset();

			if (base.HasOutsideGUI())
			{
				RectOffset baseOffset = base.GetOutsideOffset();

				offset.left = Mathf.Max(offset.left, baseOffset.left);
				offset.right = Mathf.Max(offset.right, baseOffset.right);
				offset.top = Mathf.Max(offset.top, baseOffset.top);
				offset.bottom = Mathf.Max(offset.bottom, baseOffset.bottom);
			}

			if (treeBehaviourNode.breakPoint)
			{
				GUIStyle style = GetBreakPointStyle();
				GUIContent content = EditorContents.breakPointTooltip;
				Vector2 size = style.CalcSize(content);

				offset.left = Mathf.Max(offset.left, (int)(size.x * 0.5f));
				offset.top = Mathf.Max(offset.top, (int)(size.y * 0.5f));
			}

			return offset;
		}

		protected override void OnOutsideGUI()
		{
			RectOffset overflowOffset = GetOverflowOffset();

			{
				_DecoratorEditorList.OnDataLinkGUI(overflowOffset);
			}

			BehaviourEditorGUI mainEditor = GetMainEditor();
			if (mainEditor != null)
			{
				mainEditor.DataLinkGUI(overflowOffset);
			}

			{
				_ServiceEditorList.OnDataLinkGUI(overflowOffset);
			}

			if (base.HasOutsideGUI())
			{
				base.OnOutsideGUI();

				if (treeBehaviourNode.breakPoint)
				{
					GUIStyle style = GetBreakPointStyle();
					GUIContent content = EditorContents.breakPointTooltip;
					Vector2 size = style.CalcSize(content);
					Rect breakRect = new Rect(-size.x * 0.5f, -size.y * 0.5f, size.x, size.y);
					breakRect.position += new Vector2(overflowOffset.left, overflowOffset.top);

					EditorGUI.LabelField(breakRect, content, style);
				}
			}
		}
	}
}