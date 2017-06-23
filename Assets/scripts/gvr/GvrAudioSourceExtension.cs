using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFDC.gvr
{
	public static class GvrAudioSourceExtension
	{
		public static IEnumerator fadeOut(this GvrAudioSource audioSource, float duration)
		{
			float startVolume = audioSource.volume;
			float counter = 0f;
			while (counter < duration)
			{
				audioSource.volume = Mathf.Lerp(startVolume, 0, counter / duration);
				counter += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			audioSource.volume = 0;
			audioSource.Stop();
		}
	}
}
