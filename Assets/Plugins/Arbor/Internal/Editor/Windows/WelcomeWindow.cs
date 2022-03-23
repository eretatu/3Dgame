//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
#if !ARBOR_DLL
#define ARBOR_EDITOR_EXTENSIBLE
#endif

using UnityEngine;
using UnityEditor;
using ArborEditor.UpdateCheck;
using System.Collections.Generic;

namespace ArborEditor
{
	internal sealed class WelcomeWindow : EditorWindow
	{
		[MenuItem("Window/Arbor/Welcome")]
		static void Open()
		{
			ArborEditorTemp.instance.openedWelcomeWindow = true;
			ArborEditorCache.welcomeWindowOpenedVersion = ArborVersion.fullVersion;

			var windows = Resources.FindObjectsOfTypeAll<WelcomeWindow>();
			if (windows != null && windows.Length > 0)
			{
				windows[0].Focus();
				return;
			}
			else
			{
				var window = CreateInstance<WelcomeWindow>();
				window.ShowUtility();
			}
		}

		[InitializeOnLoadMethod]
		static void OnLoadMethod()
		{
			EditorApplication.delayCall += OnAutoOpen;
		}

		static void OnAutoOpen()
		{
			if (Application.isBatchMode ||
				(ArborEditorCache.welcomeWindowOpenedVersion == ArborVersion.fullVersion) &&
				(ArborEditorTemp.instance.openedWelcomeWindow || ArborEditorCache.welcomeWindowDontDisplayNextVersion))
			{
				return;
			}

			ArborEditorCache.welcomeWindowDontDisplayNextVersion = false;
			Open();
		}

		private Vector2 _ScrollPos = new Vector2();

		private class ElementGUI
		{
			public virtual void DoGUI(GUIStyle style)
			{
			}
		}

		private class ButtonGUI : ElementGUI
		{
			private GUIContent _IconContent;
			private string _Title;
			private string _Description;

			public ButtonGUI(Texture2D icon, string title, string description)
			{
				_IconContent = new GUIContent(icon);
				_Title = title;
				_Description = description;
			}

			protected virtual void OnButtonDown()
			{
			}

			protected virtual void OnCustomGUI()
			{
			}

			public override void DoGUI(GUIStyle style)
			{
				using (new EditorGUILayout.HorizontalScope(style))
				{
					using (new EditorGUILayout.VerticalScope(Defaults.noMargin, GUILayout.ExpandHeight(false)))
					{
						GUILayout.Space(10f);

						if (EditorGUITools.CircleButton(_IconContent, GUILayout.Width(48f), GUILayout.Height(48f)))
						{
							OnButtonDown();
						}

						GUILayout.Space(10f);
					}

					GUILayout.Space(5f);

					using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(false)))
					{
						GUILayout.Space(10f);

						GUILayout.Label(Localization.GetTextContent(_Title), Styles.largeBoldLabel);
						GUILayout.Label(Localization.GetTextContent(_Description), EditorStyles.wordWrappedLabel);

						OnCustomGUI();

						GUILayout.Space(10f);
					}
				}
			}
		}

		private class BrowseButtonGUI : ButtonGUI
		{
			private string _URL = null;

			public BrowseButtonGUI(Texture2D icon, string title, string description, string url) : base(icon, title, description)
			{
				_URL = url;
			}

			protected override void OnButtonDown()
			{
				Help.BrowseURL(Localization.GetWord(_URL));
			}
		}

		private sealed class DownloadDocumentGUI : BrowseButtonGUI
		{
			private bool _IsDownloading = false;

			public DownloadDocumentGUI(Texture2D icon, string title, string description, string url) : base(icon, title, description, url)
			{
			}

			private static readonly string[] s_Unit = new[] { "B", "KB", "MB", "GB" };

			public static string ToReadableSize(double size, int scale = 0, int standard = 1024)
			{
				if (scale == s_Unit.Length - 1 || size <= standard)
				{
					return $"{size.ToString(".##")} {s_Unit[scale]}";
				}
				return ToReadableSize(size / standard, scale + 1, standard);
			}

			protected override void OnCustomGUI()
			{
				EditorGUI.BeginDisabledGroup(_IsDownloading);
				if (GUILayout.Button("Download Zip"))
				{
					string fileName = "ArborDocumentation_";
					if (ArborSettings.currentLanguage == SystemLanguage.Japanese)
					{
						fileName += "ja_";
					}
					else
					{
						fileName += "en_";
					}
					fileName += ArborVersion.documentVersion + ".zip";

					string url = "https://caitsithware.com/assets/arbor/download/docs/" + fileName;

					string directory = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);

					string path = EditorUtility.SaveFilePanel("Download ArborDocumentation", directory, fileName, "zip");
					if (!string.IsNullOrEmpty(path))
					{
						string tempPath = FileUtil.GetUniqueTempPathInProject();
						try
						{
							System.Net.WebClient client = new System.Net.WebClient();
							client.DownloadProgressChanged += (sender, e) => {
								bool isCancel = EditorUtility.DisplayCancelableProgressBar("Download ArborDocumentation", string.Format("{1} of {2} bytes. {3} % complete...", fileName, ToReadableSize(e.BytesReceived), ToReadableSize(e.TotalBytesToReceive), e.ProgressPercentage), (float)e.BytesReceived / e.TotalBytesToReceive);
								if (isCancel)
								{
									client.CancelAsync();
								}
							};
							client.DownloadFileCompleted += (sender, e) =>
							{
								_IsDownloading = false;
								EditorUtility.ClearProgressBar();
								if (e.Cancelled)
								{
									if (System.IO.File.Exists(tempPath))
									{
										System.IO.File.Delete(tempPath);
									}
									return;
								}
								if (e.Error != null)
								{
									Debug.LogException(e.Error);
								}
								else
								{
									if (System.IO.File.Exists(path))
									{
										System.IO.File.Delete(path);
									}
									FileUtil.MoveFileOrDirectory(tempPath, path);
									EditorUtility.RevealInFinder(path);
								}

								client.Dispose();
								client = null;
							};
							_IsDownloading = true;
							client.DownloadFileAsync(new System.Uri(url), tempPath);
						}
						catch
						{
							Debug.LogError("Download failed : " + url);
						}
					}

					GUIUtility.ExitGUI();
				}
				EditorGUI.EndDisabledGroup();
			}
		}

		private sealed class OpenAssetButtonGUI : ButtonGUI
		{
			private string _PathInArbor = "";

			public OpenAssetButtonGUI(Texture2D icon, string title, string description, string path) : base(icon, title, description)
			{
				_PathInArbor = path;
			}

			protected override void OnButtonDown()
			{
				string path = PathUtility.Combine(EditorResources.arborRootDirectory, Localization.GetWord(_PathInArbor));

				Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
				if (asset == null)
				{
					return;
				}

				if (AssetDatabase.OpenAsset(asset))
				{
					GUIUtility.ExitGUI();
				}
			}
		}

		private List<ElementGUI> _ElementGUIs = new List<ElementGUI>();

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnEnable()
		{
			Vector2 windowSize = new Vector2(400, 600f);

			this.minSize = windowSize;
			this.maxSize = windowSize;

			titleContent = Defaults.defaultTitleContent;

			this.wantsMouseMove = true;

			_ElementGUIs.Add(new BrowseButtonGUI(Icons.homeIcon, "WelcomeWindow.OfficialSite.Title", "WelcomeWindow.OfficialSite.Description", "SiteURL"));
			_ElementGUIs.Add(new DownloadDocumentGUI(Icons.documentationIcon, "WelcomeWindow.Documentation.Title", "WelcomeWindow.Documentation.Description", "DocumentURL"));
			_ElementGUIs.Add(new BrowseButtonGUI(Icons.forumIcon, "WelcomeWindow.SupportForum.Title", "WelcomeWindow.SupportForum.Description", "ForumURL"));
			_ElementGUIs.Add(new BrowseButtonGUI(Icons.reviewIcon, "WelcomeWindow.Review.Title", "WelcomeWindow.Review.Description", "ReviewURL"));
			_ElementGUIs.Add(new BrowseButtonGUI(Icons.tutorialIcon, "WelcomeWindow.Tutorial.Title", "WelcomeWindow.Tutorial.Description", "TutorialURL"));
			_ElementGUIs.Add(new OpenAssetButtonGUI(Icons.documentationIcon, "WelcomeWindow.ReadMe.Title", "WelcomeWindow.ReadMe.Description", "readme-en.txt"));
			_ElementGUIs.Add(new OpenAssetButtonGUI(Icons.documentationIcon, "WelcomeWindow.CHANGELOG.Title", "WelcomeWindow.CHANGELOG.Description", "CHANGELOG-en.md"));

			Repaint();
		}

#if ARBOR_EDITOR_EXTENSIBLE
		private bool _IsSkinChanged = false;

		void BeginSkin()
		{
			if (ArborEditorWindow.skin == null || _IsSkinChanged)
			{
				return;
			}

			ArborEditorWindow.skin.Begin();

			_IsSkinChanged = true;
		}

		void EndSkin()
		{
			if (ArborEditorWindow.skin == null || !_IsSkinChanged)
			{
				return;
			}

			ArborEditorWindow.skin.End();

			_IsSkinChanged = false;
		}
#endif

		[System.Reflection.Obfuscation(Exclude = true)]
		void OnGUI()
		{
#if ARBOR_EDITOR_EXTENSIBLE
			BeginSkin();
#endif

			Rect hostPosition = position;
			hostPosition.position = Vector2.zero;
			if (Event.current.type == EventType.Repaint)
			{
				Styles.tabWindowBackground.Draw(hostPosition, GUIContent.none, false, false, false, false);
			}

			using (var scope = new EditorGUILayout.VerticalScope(Defaults.noMargin))
			{
				EditorGUITools.DrawGridBackground(scope.rect);
				EditorGUITools.DrawGrid(scope.rect, 1f, ArborSettings.kDefaultGridSize, ArborSettings.kDefaultGridSplitNum);

				using (new EditorGUILayout.HorizontalScope(Defaults.noMargin))
				{
					GUILayout.FlexibleSpace();

					Texture2D logoTex = Icons.logo;
					float width = position.width;
					float scale = width / logoTex.width;
					float height = logoTex.height * scale;

					Rect logoRect = GUILayoutUtility.GetRect(width, height);
					GUI.DrawTexture(logoRect, logoTex, ScaleMode.ScaleToFit);

					GUILayout.FlexibleSpace();
				}

				EditorGUILayout.LabelField(EditorContents.version.text + " : " + ArborVersion.fullVersion, Defaults.versionLabel);
			}

			using (new EditorGUILayout.HorizontalScope(Styles.toolbar))
			{
				Localization.LanguagePopupLayout(GUIContent.none, EditorStyles.toolbarDropDown, GUILayout.MaxWidth(120f));
				GUILayout.FlexibleSpace();

				ArborEditorCache.welcomeWindowDontDisplayNextVersion = GUILayout.Toggle(ArborEditorCache.welcomeWindowDontDisplayNextVersion, Localization.GetTextContent("WelcomeWindow.DontDisplayNextVersion"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
			}

			using (new EditorGUILayout.VerticalScope(Styles.entryBox))
			{
				using (var scope = new EditorGUILayout.ScrollViewScope(_ScrollPos))
				{
					_ScrollPos = scope.scrollPosition;

					for (int index = 0; index < _ElementGUIs.Count; index++)
					{
						_ElementGUIs[index].DoGUI((index + 1) % 2 == 0 ? Styles.entryBackEven : Styles.entryBackOdd);
					}
				}
			}

			if (GUILayout.Button(Localization.GetTextContent("OpenArborEditor"), Styles.largeButton))
			{
				ArborEditorWindow.OpenFromMenu();
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField(Localization.GetTextContent("WelcomeWindow.Copyright"), EditorStyles.miniLabel);
			}

			switch (Event.current.type)
			{
				case EventType.MouseDown:
					{
						GUIUtility.keyboardControl = 0;
						Event.current.Use();
					}
					break;
				case EventType.MouseMove:
					{
						Event.current.Use();
					}
					break;
			}
#if ARBOR_EDITOR_EXTENSIBLE
			EndSkin();
#endif
		}

		private static class Defaults
		{
			public static readonly GUIContent defaultTitleContent;

			private static GUIStyle s_VersionLabel = null;
			public static GUIStyle versionLabel
			{
				get
				{
					if (s_VersionLabel == null)
					{
						s_VersionLabel = new GUIStyle(EditorStyles.whiteMiniLabel);
						s_VersionLabel.alignment = TextAnchor.MiddleRight;
					}

					return s_VersionLabel;
				}
			}

			private static GUIStyle s_NoMargin = null;
			public static GUIStyle noMargin
			{
				get
				{
					if (s_NoMargin == null)
					{
						s_NoMargin = new GUIStyle();
					}
					return s_NoMargin;
				}
			}

			static Defaults()
			{
				defaultTitleContent = new GUIContent("Welcome to Arbor " + ArborVersion.version);
			}
		}
	}
}