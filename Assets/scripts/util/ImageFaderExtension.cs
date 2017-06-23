using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public static class ImageFaderExt {

	public static IEnumerator fadeIn(this Image image, float duration)
	{
		yield return image.fade(Color.black, new Color(0,0,0,0), duration);
	}

	public static IEnumerator fadeOut(this Image image, float duration)
	{
		yield return image.fade(image.color, Color.black, duration);
	}

	public static IEnumerator fade(this Image image, Color startCol, Color endCol, float duration)
	{
		float timer = 0f;
		while (timer <= duration)
		{
			image.color = Color.Lerp(startCol, endCol, timer / duration);
			timer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		image.color = endCol;
	}
}
