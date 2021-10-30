using UnityEngine;
using UnityEditor;
using System.IO;

public class AnimExtractor
{

	static string tempExportedClip = "Assets/tempClip.anim";
	static AnimationEvent[] emptyAnimationEventArray = new AnimationEvent[0];

	[MenuItem("Assets/AnimExtractor")]
	static void AssetCopy()
	{


		Object[] selectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
		foreach (var go in selectedAsset)
		{
			ExtractFunc(go);
		}

		AssetDatabase.Refresh();
	}

	private static void ExtractFunc(Object obj)
	{
		string path = AssetDatabase.GetAssetPath(obj);
		string currentFolder = Path.GetDirectoryName(path);

		var animations = AssetDatabase.LoadAllAssetsAtPath(path);
		var originalClips = System.Array.FindAll<Object>(animations, item =>
			  item is AnimationClip
		);

		foreach (var clip in originalClips)
		{
			copyClip(clip, currentFolder);
		}
	}

	private static void copyClip(Object clip, string folder)
	{
		if (clip.name.StartsWith("__preview__"))
			return;

		var instance = Object.Instantiate(clip);
		AnimationClip copiedAnim = instance as AnimationClip;
		AnimationUtility.SetAnimationEvents(copiedAnim, emptyAnimationEventArray);  //AnimationEventÇçÌèúÇµÇ»Ç≠ÇƒÇ¢Ç¢Ç»ÇÁÇ±ÇÃçsÇçÌèú

		AssetDatabase.CreateAsset(copiedAnim, tempExportedClip);
		string exportPath = folder + "/" + clip.name + ".anim";

		if (File.Exists(exportPath))
			exportPath = AssetDatabase.GenerateUniqueAssetPath(exportPath);

		File.Copy(tempExportedClip, exportPath, true);
		File.Delete(tempExportedClip);
	}
}