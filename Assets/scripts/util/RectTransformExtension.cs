using UnityEngine;
using System.Collections;

public static class RectTransformExtension {

	public static void setTopLeftPosition(this RectTransform trans, Vector2 newPos)
	{
		trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
	}

	public static Vector2 getSize(this RectTransform trans)
	{
		return trans.rect.size;
	}

	public static void setSize(this RectTransform trans, Vector2 newSize)
	{
		Vector2 oldSize = trans.rect.size;
		Vector2 deltaSize = newSize - oldSize;
		trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
		trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
	}
}
