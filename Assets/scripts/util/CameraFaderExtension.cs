using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public static class CameraFaderExt {

	public static IEnumerator fadeIn(this Camera camera, float duration)
	{
		GameObject fadeCanvas = camera.GetComponentInChildren<Canvas>(true).gameObject;
		fadeCanvas.SetActive(true);
		Image fadeImage = fadeCanvas.GetComponentInChildren<Image>();
		yield return fadeImage.fadeIn(duration);
		fadeCanvas.SetActive(false);
	}

	public static IEnumerator fadeOut(this Camera camera, float duration)
	{
		GameObject fadeCanvas = camera.GetComponentInChildren<Canvas>(true).gameObject;
		fadeCanvas.SetActive(true);
		Image fadeImage = fadeCanvas.GetComponentInChildren<Image>();
		yield return fadeImage.fadeOut(duration);
	}
}
