using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFDC.wef
{
	public class EditorAudio : MonoBehaviour
	{
		public AudioClip objectRemoved;
		public AudioClip objectPicked;
		public AudioClip objectPickedCancelled;
		public AudioClip objectDropped;
		public AudioClip objectRotated;

		private GvrAudioSource audioSource;

		public void playAt(Vector3 position, AudioClip clip)
		{
			if (audioSource == null)
				audioSource = createAudioSource();

			audioSource.transform.position = position;
			audioSource.clip = clip;
			audioSource.Play();
		}

		private GvrAudioSource createAudioSource()
		{
			GameObject gameObject = new GameObject("AudioSource");
			GvrAudioSource audioSource = gameObject.AddComponent<GvrAudioSource>();
			audioSource.playOnAwake = false;
			audioSource.loop = false;
			return audioSource;
		}
	}
}
