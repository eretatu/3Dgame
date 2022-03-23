using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	internal sealed class DragEventHandler
	{
		public System.Action onEnter = null;
		public System.Action onUpdated = null;
		public System.Action<Vector2> onPerform = null;
		public System.Action onLeave = null;
		public System.Action onExited = null;

		public bool isEntered
		{
			get;
			private set;
		}

		void OnDragEnter()
		{
			if(onEnter != null)
			{
				onEnter();
			}
		}

		void OnDragUpdated()
		{
			if(onUpdated != null)
			{
				onUpdated();
			}
		}

		void OnDragPerform(Vector2 mousePosition)
		{
			if(onPerform != null)
			{
				onPerform(mousePosition);
			}
		}

		void OnDragLeave()
		{
			if(onLeave != null)
			{
				onLeave();
			}
		}

		void OnDragExited()
		{
			if(onExited!=null)
			{
				onExited();
			}
		}

		public void Handle(int controlId, Rect position)
		{
			Event current = Event.current;

			EventType typeForControl = current.GetTypeForControl(controlId);

			switch (typeForControl)
			{
				case EventType.DragUpdated:
					if (position.Contains(current.mousePosition))
					{
						if (!isEntered)
						{
							isEntered = true;
							OnDragEnter();
						}

						OnDragUpdated();
						if( DragAndDrop.visualMode != DragAndDropVisualMode.None && DragAndDrop.visualMode != DragAndDropVisualMode.Rejected )
						{
							current.Use();
						}
					}
					else if (isEntered)
					{
						isEntered = false;
						OnDragLeave();
					}
					break;
				case EventType.DragPerform:
					if (isEntered)
					{
						OnDragPerform(current.mousePosition);
					}
					break;
				case EventType.DragExited:
					if (isEntered)
					{
						isEntered = false;
						OnDragLeave();
					}
					OnDragExited();
					break;
				default:
					if (typeForControl != EventType.Layout && typeForControl != EventType.Used)
					{
						if (isEntered && !position.Contains(current.mousePosition))
						{
							isEntered = false;
							OnDragLeave();
						}
					}
					break;
			}
		}
	}
}