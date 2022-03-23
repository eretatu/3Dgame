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
	public sealed class NodeListGUI : System.IDisposable, IPropertyChanged
	{
		enum SearchMode
		{
			All,
			Name,
			Type
		};

		private string _SearchText;
		private SearchMode _SearchMode;
		private Vector2 _ScrollPos = Vector2.zero;

		public delegate bool IsShowCallback(NodeEditor nodeEditor);
		public delegate void OnSelectCallback(NodeEditor nodeEditor);
		public delegate bool IsSelectionCallback(NodeEditor nodeEditor);

		public IsShowCallback isShowCallback;
		public System.Comparison<NodeEditor> sortComparisonCallback;
		public OnSelectCallback onSelectCallback;
		public IsSelectionCallback isSelectionCallback;

		private NodeGraphEditor _GraphEditor;

		public NodeGraphEditor graphEditor
		{
			get
			{
				return _GraphEditor;
			}
		}

		public NodeGraph nodeGraph
		{
			get
			{
				return _GraphEditor != null ? _GraphEditor.nodeGraph : null;
			}
		}

		public List<NodeEditor> viewNodes
		{
			get;
			private set;
		}

		void OnChangedGraph()
		{
			RebuildViewNodes();
		}

		void OnChangedNodes()
		{
			RebuildViewNodes();
		}

		public void Initialize(NodeGraphEditor graphEditor)
		{
			if (_GraphEditor == graphEditor)
			{
				return;
			}

			Dispose();

			_GraphEditor = graphEditor;

			if (_GraphEditor != null)
			{
				_GraphEditor.onChangedGraph += OnChangedGraph;
				_GraphEditor.onChangedNodes += OnChangedNodes;

				EditorCallbackUtility.RegisterPropertyChanged(this);
			}

			RebuildViewNodes();
		}

		public void Dispose()
		{
			if (_GraphEditor != null)
			{
				EditorCallbackUtility.UnregisterPropertyChanged(this);

				_GraphEditor.onChangedGraph -= OnChangedGraph;
				_GraphEditor.onChangedNodes -= OnChangedNodes;

				_GraphEditor = null;
			}
		}

		void IPropertyChanged.OnPropertyChanged(PropertyChangedType propertyChangedType)
		{
			if (propertyChangedType != PropertyChangedType.UndoRedoPerformed)
			{
				return;
			}

			RebuildViewNodes();
		}

		public NodeListGUI()
		{
			viewNodes = new List<NodeEditor>();
		}

		public static class Defaults
		{
			public static int SortComparison(NodeEditor a, NodeEditor b)
			{
				return a.GetTitle().CompareTo(b.GetTitle());
			}

			public static bool IsSelection(NodeEditor a)
			{
				return a.isSelection;
			}
		}

		void RebuildViewNodes()
		{
			int nodeCount = 0;

			if (nodeGraph != null)
			{
				nodeCount = nodeGraph.nodeCount;
			}

			viewNodes.Clear();

			for (int i = 0; i < nodeCount; i++)
			{
				Node node = nodeGraph.GetNodeFromIndex(i);
				NodeEditor nodeEditor = graphEditor.GetNodeEditor(node);

				if (nodeEditor == null || !nodeEditor.IsShowNodeList())
				{
					continue;
				}

				if (isShowCallback != null && !isShowCallback(nodeEditor))
				{
					continue;
				}

				if (!string.IsNullOrEmpty(_SearchText))
				{
					string nodeName = nodeEditor.GetTitle();

					switch (_SearchMode)
					{
						case SearchMode.All:
							if (nodeName.IndexOf(_SearchText, System.StringComparison.OrdinalIgnoreCase) >= 0)
							{
								viewNodes.Add(nodeEditor);
							}
							else
							{
								INodeBehaviourContainer nodeBehaviours = node as INodeBehaviourContainer;
								if (nodeBehaviours != null)
								{
									int behaviourCount = nodeBehaviours.GetNodeBehaviourCount();
									for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
									{
										NodeBehaviour behaviour = nodeBehaviours.GetNodeBehaviour<NodeBehaviour>(behaviourIndex);

										if (behaviour.GetType().Name.Equals(_SearchText, System.StringComparison.OrdinalIgnoreCase))
										{
											viewNodes.Add(nodeEditor);
											break;
										}
									}
								}
							}
							break;
						case SearchMode.Name:
							if (nodeName.IndexOf(_SearchText, System.StringComparison.OrdinalIgnoreCase) >= 0)
							{
								viewNodes.Add(nodeEditor);
							}
							break;
						case SearchMode.Type:
							{
								INodeBehaviourContainer nodeBehaviours = node as INodeBehaviourContainer;
								if (nodeBehaviours != null)
								{
									int behaviourCount = nodeBehaviours.GetNodeBehaviourCount();
									for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
									{
										NodeBehaviour behaviour = nodeBehaviours.GetNodeBehaviour<NodeBehaviour>(behaviourIndex);

										if (behaviour.GetType().Name.Equals(_SearchText, System.StringComparison.OrdinalIgnoreCase))
										{
											viewNodes.Add(nodeEditor);
											break;
										}
									}
								}
							}
							break;
					}
				}
				else
				{
					viewNodes.Add(nodeEditor);
				}
			}

			if (sortComparisonCallback != null)
			{
				viewNodes.Sort(sortComparisonCallback);
			}
			else
			{
				viewNodes.Sort(Defaults.SortComparison);
			}
		}

		public void OnGUI()
		{
			int nodeCount = 0;

			if (nodeGraph != null)
			{
				nodeCount = nodeGraph.nodeCount;
			}
			else
			{
				EditorGUI.BeginDisabledGroup(true);
			}

			Rect searchRect = GUILayoutUtility.GetRect(0.0f, 20.0f);
			searchRect.y += 4f;
			searchRect.x += 8f;
			searchRect.width -= 16f;

			string[] names = System.Enum.GetNames(typeof(SearchMode));
			int searchMode = (int)_SearchMode;
			EditorGUI.BeginChangeCheck();
			_SearchText = EditorGUITools.ToolbarSearchField(searchRect, names, ref searchMode, _SearchText);
			_SearchMode = (SearchMode)searchMode;
			if (EditorGUI.EndChangeCheck())
			{
				RebuildViewNodes();
			}

			if (nodeCount > 0)
			{
				//RebuildViewNodes();

				_ScrollPos = EditorGUILayout.BeginScrollView(_ScrollPos);

				GUILayout.Space(3.0f);

				Vector2 iconSize = EditorGUIUtility.GetIconSize();
				EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));

				int viewNodeCount = viewNodes.Count;
				for (int stateIndex = 0; stateIndex < viewNodeCount; stateIndex++)
				{
					NodeEditor nodeEditor = viewNodes[stateIndex];

					GUIStyle nodeStyle = nodeEditor.GetListStyle();
					Color color = nodeEditor.GetListColor();

					Color tempBackgroundColor = GUI.backgroundColor;
					GUI.backgroundColor = color;

					Rect rect = GUILayoutUtility.GetRect(0.0f, 25.0f);

					GUIContent nodeContent = nodeEditor.GetListElementContent();

					if (EditorGUI.DropdownButton(rect, nodeContent, FocusType.Passive, nodeStyle))
					{
						// Unity 2021.2 or later: Addressing a bug where hotControl remains non-zero
						GUIUtility.hotControl = 0;

						if (onSelectCallback != null)
						{
							onSelectCallback(nodeEditor);
						}
					}

					GUI.backgroundColor = tempBackgroundColor;

					nodeEditor.OnListElement(rect);

					bool isSelection = (isSelectionCallback != null) ? isSelectionCallback(nodeEditor) : Defaults.IsSelection(nodeEditor);
					if (isSelection)
					{
						if (Event.current.type == EventType.Repaint)
						{
							Styles.nodeElementSelect.Draw(rect, GUIContent.none, false, false, false, false);
						}
					}

					GUILayout.Space(1);
				}

				EditorGUIUtility.SetIconSize(iconSize);

				EditorGUILayout.EndScrollView();
			}

			if (nodeGraph == null)
			{
				EditorGUI.EndDisabledGroup();
			}
		}
	}
}