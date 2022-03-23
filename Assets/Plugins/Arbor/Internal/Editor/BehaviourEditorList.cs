//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ArborEditor
{
	using Arbor;

	[System.Serializable]
	public abstract class BehaviourEditorList<TEditor,TBehaviour> where TEditor : BehaviourEditorGUI, new() where TBehaviour : NodeBehaviour
	{
		public NodeEditor nodeEditor = null;
		public NodeGraphEditor graphEditor
		{
			get
			{
				if (nodeEditor == null)
				{
					return null;
				}
				return nodeEditor.graphEditor;
			}
		}
		public Node node
		{
			get
			{
				if (nodeEditor == null)
				{
					return null;
				}
				return nodeEditor.node;
			}
		}

		public virtual Color backgroundColor
		{
			get
			{
				return Color.white;
			}
		}
		public virtual GUIStyle backgroundStyle
		{
			get
			{
				return GUIStyle.none;
			}
		}

		public abstract System.Type targetType
		{
			get;
		}

		public virtual bool isDroppableParameter
		{
			get
			{
				return false;
			}
		}

		private List<TEditor> _BehaviourEditors = null;
		private List<TEditor> _NewBehaviourEditors = null;
		private List<DropBehaviourGUI> _DropBehaviours = new List<DropBehaviourGUI>();

		private List<TEditor> behaviourEditors
		{
			get
			{
				if (_BehaviourEditors == null || GetCount() != _BehaviourEditors.Count)
				{
					RebuildBehaviourEditors();
				}

				return _BehaviourEditors;
			}
		}

		public abstract int GetCount();
		public abstract Object GetObject(int behaviourIndex);
		public abstract void InsertBehaviour(int index, System.Type classType);
		public abstract void MoveBehaviour(Node fromNode, int fromIndex, Node toNode, int toIndex, bool isCopy);
		public abstract void OpenBehaviourMenu(Rect buttonRect, int index);
		public abstract void PasteBehaviour(int index);
		public abstract GUIContent GetInsertButtonContent();
		public abstract GUIContent GetAddBehaviourContent();
		public abstract GUIContent GetPasteBehaviourContent();

		public void RebuildBehaviourEditors()
		{
			if (_NewBehaviourEditors == null)
			{
				_NewBehaviourEditors = new List<TEditor>();
			}
			List<TEditor> newBehaviourEditors = _NewBehaviourEditors;

			int behaviourCount = GetCount();
			for (int i = 0, count = behaviourCount; i < count; i++)
			{
				Object behaviourObj = GetObject(i);

				TEditor behaviourEditor = null;

				if (_BehaviourEditors != null)
				{
					for (int editorIndex = 0; editorIndex < _BehaviourEditors.Count; editorIndex++)
					{
						TEditor e = _BehaviourEditors[editorIndex];
						if (e.behaviourObj == behaviourObj)
						{
							behaviourEditor = e;
							_BehaviourEditors.Remove(e);
							break;
						}
					}
				}

				if (behaviourEditor == null)
				{
					behaviourEditor = new TEditor();
					behaviourEditor.Initialize(nodeEditor, behaviourObj);
				}

				behaviourEditor.behaviourIndex = i;
				newBehaviourEditors.Add(behaviourEditor);
			}

			if (_BehaviourEditors != null)
			{
				for (int i = 0; i < _BehaviourEditors.Count; i++)
				{
					TEditor e = _BehaviourEditors[i];
					e.DestroyEditor();
				}
				_BehaviourEditors.Clear();
			}

			_NewBehaviourEditors = _BehaviourEditors;

			_BehaviourEditors = newBehaviourEditors;

			int handlerCount = behaviourCount + 1;
			int currentCount = _DropBehaviours.Count;
			if (currentCount > handlerCount)
			{
				_DropBehaviours.RemoveRange(handlerCount, currentCount - handlerCount);
			}
			else
			{
				for (int i = currentCount; i < handlerCount; i++)
				{
					_DropBehaviours.Add(new DropBehaviourGUI(this, i));
				}
			}
		}

		public TEditor GetBehaviourEditor(int behaviourIndex)
		{
			using (new ProfilerScope("GetBehaviourEditor"))
			{
				TEditor behaviourEditor = behaviourEditors[behaviourIndex];

				if (!ComponentUtility.IsValidObject(behaviourEditor.behaviourObj))
				{
					Object behaviourObj = GetObject(behaviourIndex);
					behaviourEditor.Repair(behaviourObj);
				}

				if (behaviourEditor != null)
				{
#if ARBOR_DEBUG
					if (behaviourEditor.behaviourIndex != behaviourIndex)
					{
						Debug.Log(behaviourEditor.behaviourIndex + " -> " + behaviourIndex);
					}
#endif
					behaviourEditor.behaviourIndex = behaviourIndex;
				}

				return behaviourEditor;
			}
		}

		public void Validate()
		{
			RebuildBehaviourEditors();

			for (int i = 0, count = _BehaviourEditors.Count; i < count; i++)
			{
				TEditor behaviourEditor = _BehaviourEditors[i];
				behaviourEditor.Validate();
			}
		}

		public void MoveBehaviourEditor(int fromIndex, int toIndex)
		{
			List<TEditor> behaviourEditors = this.behaviourEditors;

			TEditor tempEditor = behaviourEditors[toIndex];
			behaviourEditors[toIndex] = behaviourEditors[fromIndex];
			behaviourEditors[fromIndex] = tempEditor;

			behaviourEditors[fromIndex].behaviourIndex = fromIndex;
			behaviourEditors[toIndex].behaviourIndex = toIndex;
		}

		public void RemoveBehaviourEditor(int behaviourIndex)
		{
			List<TEditor> behaviourEditors = this.behaviourEditors;

			TEditor behaviourEditor = behaviourEditors[behaviourIndex];

			if (behaviourEditor != null)
			{
				behaviourEditor.DestroyEditor();
			}

			behaviourEditors.RemoveAt(behaviourIndex);

			for (int i = behaviourIndex, count = behaviourEditors.Count; i < count; i++)
			{
				TEditor e = behaviourEditors[i];
				e.behaviourIndex = i;
			}
		}

		public void DestroyEditors()
		{
			if (_BehaviourEditors == null)
			{
				return;
			}

			int editorCount = _BehaviourEditors.Count;
			for (int editorIndex = 0; editorIndex < editorCount; editorIndex++)
			{
				_BehaviourEditors[editorIndex].DestroyEditor();
			}
			_BehaviourEditors.Clear();
		}

		public TEditor InsertBehaviourEditor(int behaviourIndex, Object behaviourObj)
		{
			TEditor behaviourEditor = new TEditor();
			behaviourEditor.Initialize(nodeEditor, behaviourObj);
			behaviourEditor.behaviourIndex = behaviourIndex;

			List<TEditor> behaviourEditors = this.behaviourEditors;

			behaviourEditors.Insert(behaviourIndex, behaviourEditor);

			for (int i = behaviourIndex + 1, count = behaviourEditors.Count; i < count; i++)
			{
				TEditor e = behaviourEditors[i];
				e.behaviourIndex = i;
			}

			return behaviourEditor;
		}

		public virtual void InsertSetParameter(int index, Parameter parameter)
		{
		}

		public void OnGUI()
		{
			int behaviourCount = GetCount();
			Color savedBackgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = backgroundColor;
			GUIStyle style = behaviourCount > 0 ? backgroundStyle : GUIStyle.none;
			using (EditorGUILayout.VerticalScope listScope = new EditorGUILayout.VerticalScope(style))
			{
				GUI.backgroundColor = savedBackgroundColor;

				GUILayout.Space(0);

				for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
				{
					using (EditorGUILayout.VerticalScope behavioiurScope = new EditorGUILayout.VerticalScope())
					{
						Rect dropRect = behavioiurScope.rect;
						dropRect.height = 0f;

						_DropBehaviours[behaviourIndex].DoGUI(dropRect, false);

						TEditor behaviourEditor = GetBehaviourEditor(behaviourIndex);
						if (behaviourEditor != null)
						{
							behaviourEditor.backgroundColor = backgroundColor;
							BehaviourInfo behaviourInfo = BehaviourInfoUtility.GetBehaviourInfo(behaviourEditor.behaviourObj);
							GUIContent titleContent = behaviourInfo.titleContent;

							using (new ProfilerScope(titleContent.text))
							{
								behaviourEditor.OnGUI();
							}
						}
					}
				}

				if (behaviourCount > 0)
				{
					GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
				}

				Rect lastRect = listScope.rect;
				lastRect.yMin = lastRect.yMax;
				_DropBehaviours[behaviourCount].DoGUI(lastRect, true);
			}
		}

		public bool IsVisibleDataLinkGUI()
		{
			int behaviourCount = GetCount();
			for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
			{
				TEditor behaviourEditor = GetBehaviourEditor(behaviourIndex);
				if (behaviourEditor != null && behaviourEditor.IsVisibleDataLinkGUI())
				{
					return true;
				}
			}

			return false;
		}

		public void OnDataLinkGUI(RectOffset outsideOffset)
		{
			int behaviourCount = GetCount();
			for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
			{
				TEditor behaviourEditor = GetBehaviourEditor(behaviourIndex);
				if (behaviourEditor != null)
				{
					behaviourEditor.DataLinkGUI(outsideOffset);
				}
			}
		}

		sealed class DropBehaviourGUI
		{
			private static readonly int s_DoDragBehaviourHash = "DoDragBehaviour".GetHashCode();

			private BehaviourEditorList<TEditor, TBehaviour> _Owner;
			private int _Index;
			private DragEventHandler _DragEventHandler;

			private bool _IsDragging = false;

			public DropBehaviourGUI(BehaviourEditorList<TEditor, TBehaviour> owner, int index)
			{
				_Owner = owner;
				_Index = index;
				_DragEventHandler = new DragEventHandler()
				{
					onUpdated = OnDragUpdated,
					onPerform = OnDragPerform,
					onLeave = OnDragLeave,
				};
			}

			void DragUpdated(bool perform)
			{
				BehaviourDragInfo behaviourDragInfo = BehaviourDragInfo.GetBehaviourDragInfo();
				bool isDroppableParameter = _Owner.isDroppableParameter;

				int insertIndex = _Index;
				bool isAccepted = false;
				_IsDragging = false;

				var objectReferences = DragAndDrop.objectReferences;
				for (int i = 0; i < objectReferences.Length; i++)
				{
					Object draggedObject = objectReferences[i];
					MonoScript script = draggedObject as MonoScript;
					if (script != null)
					{
						System.Type classType = script.GetClass();

						if (classType != null && classType.IsSubclassOf(_Owner.targetType))
						{
							_IsDragging = true;
							DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

							if (perform)
							{
								_Owner.InsertBehaviour(insertIndex, classType);
								insertIndex++;

								isAccepted = true;
							}
						}
					}

					if (isDroppableParameter)
					{
						ParameterDraggingObject parameterDraggingObject = draggedObject as ParameterDraggingObject;
						if (parameterDraggingObject != null)
						{
							_IsDragging = true;
							DragAndDrop.visualMode = DragAndDropVisualMode.Link;

							if (perform)
							{
								_Owner.InsertSetParameter(insertIndex, parameterDraggingObject.parameter);
								insertIndex++;

								isAccepted = true;
							}
						}
					}

					if (behaviourDragInfo != null && behaviourDragInfo.behaviourEditor != null && behaviourDragInfo.dragging && behaviourDragInfo.behaviourEditor.behaviourObj == draggedObject)
					{
						BehaviourEditorGUI behaviourEditor = behaviourDragInfo.behaviourEditor;

						if (typeof(TEditor).IsAssignableFrom(behaviourEditor.GetType()))
						{
							Node fromNode = null;
							int fromIndex = -1;

							bool isCopy = (Application.platform == RuntimePlatform.OSXEditor) ? Event.current.alt : Event.current.control;

							if (behaviourEditor.nodeEditor.graphEditor.nodeGraph == _Owner.node.nodeGraph)
							{
								fromNode = behaviourEditor.nodeEditor.node;
							}
							else
							{
								fromNode = _Owner.node;
							}

							fromIndex = behaviourEditor.behaviourIndex;
							if (fromIndex >= 0)
							{
								if (!isCopy && fromNode == _Owner.node)
								{
									if (fromIndex < insertIndex)
									{
										insertIndex--;
									}
								}

								if (fromNode == _Owner.node && !isCopy)
								{
									_IsDragging = fromIndex != insertIndex;
								}
								else if (behaviourEditor.behaviourObj != null)
								{
									System.Type classType = behaviourEditor.behaviourObj.GetType();
									_IsDragging = classType.IsSubclassOf(_Owner.targetType);
								}

								if (_IsDragging)
								{
									if (isCopy)
									{
										DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
									}
									else
									{
										DragAndDrop.visualMode = DragAndDropVisualMode.Move;
									}

									if (perform)
									{
										_Owner.MoveBehaviour(fromNode, fromIndex, _Owner.node, insertIndex, isCopy);

										isAccepted = true;
									}
								}
							}
						}
					}
				}

				if (isAccepted)
				{
					DragAndDrop.AcceptDrag();
					DragAndDrop.activeControlID = 0;

					EditorGUIUtility.ExitGUI();
				}
			}

			void OnDragUpdated()
			{
				DragUpdated(false);
			}

			void OnDragPerform(Vector2 mousePosition)
			{
				DragUpdated(true);
			}

			void OnDragLeave()
			{
				_IsDragging = false;
			}

			public void DoGUI(Rect position, bool last)
			{
				bool editable = _Owner.graphEditor.editable;
				if (!editable)
				{
					return;
				}

				Rect insertBarRect = new Rect(position);
				insertBarRect.height = EditorGUIUtility.singleLineHeight * 0.5f;
				insertBarRect.xMin = position.center.x - 16f;
				insertBarRect.xMax = position.center.x + 16f;
				if (last)
				{
					insertBarRect.yMin = position.yMax - insertBarRect.height * 0.5f;
					insertBarRect.yMax = position.yMax;
				}
				else
				{
					insertBarRect.y -= insertBarRect.height * 0.5f;
				}

				GUIStyle insertionStyle = last ? Styles.insertion : Styles.insertionAbove;

				float styleHeight = Styles.kInsertionHeight;

				if (last)
				{
					position.yMin -= styleHeight;
				}
				position.height = styleHeight;

				Rect dropRect = new Rect(position);
				dropRect.height = EditorGUIUtility.singleLineHeight;
				if (last)
				{
					dropRect.yMin = dropRect.yMax - dropRect.height * 0.5f;
				}
				else if (_Index == 0)
				{
					dropRect.yMax = dropRect.yMin + dropRect.height * 0.5f;
				}
				else
				{
					dropRect.y -= dropRect.height * 0.5f;
				}

				Rect insertButtonRect = new Rect(position);
				insertButtonRect.height = EditorGUIUtility.singleLineHeight;
				insertButtonRect.width = 16.0f;

				Vector2 center = insertButtonRect.center;
				center.x = Mathf.Clamp(Event.current.mousePosition.x, insertBarRect.xMin, insertBarRect.xMax);
				insertButtonRect.center = center;

				if (last)
				{
					insertButtonRect.y += insertButtonRect.height * 0.5f;
				}
				else
				{
					insertButtonRect.y -= insertButtonRect.height * 0.5f;
				}

				int controlId = GUIUtility.GetControlID(s_DoDragBehaviourHash, FocusType.Passive, position);

				Event current = Event.current;

				_DragEventHandler.Handle(controlId, dropRect);

				EventType typeForControl = current.GetTypeForControl(controlId);
				switch (typeForControl)
				{
					case EventType.MouseMove:
						if (!(_DragEventHandler.isEntered && _IsDragging) && insertBarRect.Contains(current.mousePosition))
						{
							GUIStyle style = Styles.addBehaviourButton;
							GUIContent content = _Owner.GetInsertButtonContent();

							Rect popupButtonRect = _Owner.nodeEditor.NodeToGraphRect(insertButtonRect);
							popupButtonRect.size = style.CalcSize(content);

							Rect buttonRect = EditorGUITools.GUIToScreenRect(position);
							if (!last)
							{
								buttonRect.height = 0f;
							}

							_Owner.graphEditor.ShowPopupButtonControl(popupButtonRect, content, controlId, style, (Rect rect) =>
							{
								GenericMenu menu = new GenericMenu();
								menu.AddItem(_Owner.GetAddBehaviourContent(), false, () =>
								{
									_Owner.OpenBehaviourMenu(buttonRect, _Index);
								});

								GUIContent pasteContent = _Owner.GetPasteBehaviourContent();

								if (Clipboard.CompareBehaviourType(typeof(TBehaviour), true))
								{
									menu.AddItem(pasteContent, false, () =>
									{
										_Owner.PasteBehaviour(_Index);
									});
								}
								else
								{
									menu.AddDisabledItem(pasteContent);
								}

								menu.DropDown(rect);
							});
						}
						break;
					case EventType.Repaint:
						{
							bool draw = false;
							if (_DragEventHandler.isEntered && _IsDragging)
							{
								if (dropRect.Contains(current.mousePosition))
								{
									draw = true;
								}
							}
							else if (_Owner.graphEditor.GetPopupButtonActiveControlID() == controlId)
							{
								draw = true;
							}

							if (draw)
							{
								insertionStyle.Draw(position, true, true, true, false);
							}

							//EditorGUI.DrawRect(position, new Color(0f, 1f, 0f, 0.5f));
							//EditorGUI.DrawRect(insertBarRect, new Color(1f, 0f, 0f, 0.5f));
						}
						break;
				}
			}
		}
	}
}