using UnityEngine;
using System.Collections;

public static class GameObjectExtension
{
	public static Vector3 getCenter(this GameObject obj)
	{
		Vector3 center = Vector3.zero;
		Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer renderer in renderers)
			center += renderer.bounds.center;
		return (center / renderers.Length);
	}

	public static Bounds getBounds(this GameObject obj)
	{
		Quaternion currentRotation = obj.transform.rotation;
		obj.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
		foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
			bounds.Encapsulate(renderer.bounds);
		obj.transform.rotation = currentRotation;
		return bounds;
	}

	public static void disableAllColliders(this GameObject gameObject)
	{
		BoxCollider[] boxColliders = gameObject.GetComponentsInChildren<BoxCollider>();
		foreach (BoxCollider collider in boxColliders)
			collider.enabled = false;
		CapsuleCollider[] capsuleColliders = gameObject.GetComponentsInChildren<CapsuleCollider>();
		foreach (CapsuleCollider collider in capsuleColliders)
			collider.enabled = false;
		MeshCollider[] meshColliders = gameObject.GetComponentsInChildren<MeshCollider>();
		foreach (MeshCollider collider in meshColliders)
			collider.enabled = false;
	}

	public static void enableRenderers(this GameObject gameObject, bool isEnabled)
	{
		Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in renderers)
			renderer.enabled = isEnabled;
	}
}