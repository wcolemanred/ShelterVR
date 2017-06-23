using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MobileElement : MonoBehaviour
{
	private Vector3 start;
	private Vector3 destination;
	private float speed = 0;
	private float totalDistance;
	private float distanceCovered ;

	public void Update()
	{
		if (speed == 0)
			return;
		// Check progress
		distanceCovered += Time.deltaTime * speed;
		float percentCompleted = distanceCovered / totalDistance;
		// Avoid precision related bug
		if (percentCompleted > 0.999f)
		{
			speed = 0;
			return;
		}
		// Calculate new position
		Vector3 nextPosition = Vector3.Lerp(start, destination, percentCompleted);
		// Move object
		transform.position = nextPosition;
	}

	public IEnumerator speedModifierCoroutine(float delay, float targetSpeed, float duration)
	{
		yield return new WaitForSeconds(delay);

		float initialSpeed = speed;
		bool isAcceleration = (targetSpeed > initialSpeed);

		float speedDelta = Mathf.Abs(targetSpeed - initialSpeed);
		float counter = 0f;
		while (counter < duration)
		{
			if (isAcceleration)
				speed = initialSpeed + speedDelta * counter / duration;
			else
				speed = initialSpeed - speedDelta * counter / duration;
			counter += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		speed = targetSpeed;
	}

	public void setDesination(Vector3 destination)
	{
		start = transform.position;
		this.destination = destination;
		totalDistance = Vector3.Distance(start, destination);
		distanceCovered = 0;
		speed = 0;
	}
}

