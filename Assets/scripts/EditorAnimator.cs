using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFDC.wef
{
	[RequireComponent(typeof(EditorAudio))]
	public class EditorAnimator : MonoBehaviour
	{
		public GameObject smoke;

		private EditorAudio editorAudio;
		private Vector3 pickedObjectOriginalPosition;

		private const float PICKED_OBJECT_ELEVATION	= 0.2f;


		public void Start()
		{
			editorAudio = GetComponent<EditorAudio>();
		}

		public IEnumerator playDropAnimation(Transform droppedObject, Vector3 targetPosition, LineRenderer objectDropBeam)
		{
			editorAudio.playAt(targetPosition, editorAudio.objectDropped);

			Vector3 originalPosition = droppedObject.position;
			float duration = 1;
			float timeSpent = 0;
			while (timeSpent < duration)
			{
				timeSpent += Time.deltaTime;
				droppedObject.position = Vector3.Lerp(originalPosition, targetPosition, timeSpent / duration);
				objectDropBeam.SetPosition(0, droppedObject.position);
				yield return new WaitForEndOfFrame();
			}
			droppedObject.position = targetPosition;
			objectDropBeam.enabled = false;
		}

		public IEnumerator playResetPickedObjectAnimation(Transform droppeObject)
		{
			Vector3 originalPosition = droppeObject.position;
			editorAudio.playAt(originalPosition, editorAudio.objectPickedCancelled);
			float duration = 1;
			float timeSpent = 0;
			while (timeSpent < duration)
			{
				timeSpent += Time.deltaTime;
				droppeObject.position = Vector3.Lerp(originalPosition, pickedObjectOriginalPosition, timeSpent / duration);
				yield return new WaitForEndOfFrame();
			}
			droppeObject.position = pickedObjectOriginalPosition;
		}

		public IEnumerator playPickupAnimation(Transform pickedObject)
		{
			pickedObjectOriginalPosition = pickedObject.position;
			editorAudio.playAt(pickedObjectOriginalPosition, editorAudio.objectPicked);
			Vector3 targetPosition = pickedObjectOriginalPosition;
			targetPosition.y += PICKED_OBJECT_ELEVATION;

			float duration = 1;
			float timeSpent = 0;
			while (timeSpent < duration)
			{
				timeSpent += Time.deltaTime;
				pickedObject.position = Vector3.Lerp(pickedObjectOriginalPosition, targetPosition, timeSpent / duration);
				yield return new WaitForEndOfFrame();
			}
			pickedObject.position = targetPosition;
		}

		public IEnumerator playRotationAnimation(Transform targetedObject, int rotationAngle)
		{
			editorAudio.playAt(targetedObject.position, editorAudio.objectRotated);
			int angleDelta = (rotationAngle > 0) ? 1 : -1;
			for (int i = 0; i < Math.Abs(rotationAngle) + 1; i++)
			{
				targetedObject.Rotate(0, angleDelta, 0, Space.World);
				yield return new WaitForEndOfFrame();
			}
		}

		public IEnumerator playRemoveObjectAnimation(GameObject placedObject)
		{
			editorAudio.playAt(placedObject.transform.position, editorAudio.objectRemoved);

			smoke.transform.position = placedObject.transform.position;
			smoke.GetComponent<ParticleSystem>().Play();
			yield return new WaitForSeconds(0.1f);
			Destroy(placedObject);
		}
	}
}
