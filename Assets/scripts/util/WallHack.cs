using UnityEngine;
using System.Collections.Generic;

public class WallHack : MonoBehaviour {

	private Transform player;
	private List<Transform> hiddenObjects;
	private List<Vector3> detectionPoints;

	private const int TOGGLEABLE_VOLUMES_LAYER = 10;
	private const float DETECTION_POINT_EXPANSION_RATIO = 0.75f;

	private void Start()
	{
		player = Camera.main.transform;
		hiddenObjects = new List<Transform>();
		detectionPoints = getDetectionPoints();
	}

	void Update()
	{
		// Get objects with colliders in the way
		List<RaycastHit> hits = new List<RaycastHit>();
		foreach (Vector3 point in detectionPoints)
			hits.AddRange(getObjectsBetweenPlayerAndPoint(point));

		// Hide hit objects
		foreach (RaycastHit hit in hits)
		{
			Transform currentHit = hit.transform;
			if (!hiddenObjects.Contains(currentHit))
			{
				hiddenObjects.Add(currentHit);
				currentHit.gameObject.enableRenderers(false);
			}
		}

		// Show objects that are no longer hit
		for (int i=0; i<hiddenObjects.Count; i++)
		{
			Transform curObjectTransform = hiddenObjects[i];
			// Check if object is hit
			bool isHit = false;
			for (int j = 0; !isHit && j < hits.Count; j++)
			{
				if (hits[j].transform == curObjectTransform)
					isHit = true;
			}
			// Show object if it is not hit
			if (!isHit)
			{
				curObjectTransform.gameObject.enableRenderers(true);
				hiddenObjects.RemoveAt(i);
				i--;
			}
		}
	}
	
	private RaycastHit[] getObjectsBetweenPlayerAndPoint(Vector3 target)
	{
		Vector3 direction = target - player.position;
		return Physics.RaycastAll(player.position, direction, direction.magnitude, 1 << TOGGLEABLE_VOLUMES_LAYER);
	}

	private List<Vector3> getDetectionPoints()
	{
		List<Vector3> detectionPoints = new List<Vector3>();

		Bounds bounds = gameObject.getBounds();
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;

		Vector3 marker = center;
		marker.x -= extents.x * DETECTION_POINT_EXPANSION_RATIO;
		detectionPoints.Add(marker);
		marker = center;
		marker.x += extents.x * DETECTION_POINT_EXPANSION_RATIO;
		detectionPoints.Add(marker);

		marker = center;
		marker.y -= extents.y * DETECTION_POINT_EXPANSION_RATIO;
		detectionPoints.Add(marker);
		marker = center;
		marker.y += extents.y * DETECTION_POINT_EXPANSION_RATIO;
		detectionPoints.Add(marker);

		marker = center;
		marker.z -= extents.z * DETECTION_POINT_EXPANSION_RATIO;
		detectionPoints.Add(marker);
		marker = center;
		marker.z += extents.z * DETECTION_POINT_EXPANSION_RATIO;
		detectionPoints.Add(marker);

		return detectionPoints;
	}
}
