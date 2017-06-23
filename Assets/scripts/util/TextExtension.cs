using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public static class TextExtension {

	public static void setAlpha(this Text text, float alpha)
	{
		Color color = text.color;
		color.a = alpha;
		text.color = color;
	}
}
