using System;
using UnityEngine;


namespace SFDC.gvr
{
	public class GvrSwipeMonitor : MonoBehaviour
	{
		private Vector2 touchStartPos, touchEndPos;

		public event Action onSwipeUp;
		public event Action onSwipeDown;
		public event Action onSwipeLeft;
		public event Action onSwipeRight;

		private const float MIN_SWIPE_DISTANCE = 0.4f;

		public void Update()
		{
			if (GvrController.TouchDown)
			{
				touchStartPos = GvrController.TouchPos;
			}
			else if (GvrController.TouchUp)
			{
				touchEndPos = GvrController.TouchPos;
				processSwipe();
			}
		}

		private void processSwipe()
		{
			// Get swipe vector
			Vector2 swipe = touchEndPos - touchStartPos;

			// Avoid clicks
			float swipeDistance = Vector2.Distance(touchStartPos, touchEndPos);
			if (swipeDistance < MIN_SWIPE_DISTANCE)
				return;
			
			swipe.Normalize();

			// Swipe up
			if (swipe.y < 0 && swipe.x > -0.5f && swipe.x < 0.5f && onSwipeUp != null)
				onSwipeUp.Invoke();
			// Swipe down
			else if (swipe.y > 0 && swipe.x > -0.5f && swipe.x < 0.5f && onSwipeDown != null)
				onSwipeDown.Invoke();
			// Swipe left
			else if (swipe.x < 0 && swipe.y > -0.5f && swipe.y < 0.5f && onSwipeLeft != null)
				onSwipeLeft.Invoke();
			// Swipe right
			else if (swipe.x > 0 && swipe.y > -0.5f && swipe.y < 0.5f && onSwipeRight != null)
				onSwipeRight.Invoke();
		}
	}
}