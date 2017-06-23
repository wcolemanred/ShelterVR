using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SFDC.gvr
{
	public class GvrExitToMenuControl : MonoBehaviour
	{
		public AudioClip sceneTransition;

		private float touchStart = 0f;
		private bool isTransitioning = false;

		private const float PRESS_DURATION = 2f;


		public void Update()
		{
			if (isTransitioning)
				return;

			if (GvrController.AppButtonDown)
				touchStart = Time.time;
			else if (GvrController.AppButtonUp)
				touchStart = 0;
			else if (touchStart != 0f)
			{
				float pressDuration = Time.time - touchStart;
				if (pressDuration >= PRESS_DURATION)
					StartCoroutine(transitionToMenu());
			}
		}

		private IEnumerator transitionToMenu()
		{
			isTransitioning = true;
			// Start to load next scene
			AsyncOperation ao = SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single);
			ao.allowSceneActivation = false;
			// Fade out
			playSceneTransitionAudio();
			yield return Camera.main.fadeOut(2);
			// Switch to next scene
			ao.allowSceneActivation = true;
		}

		private void playSceneTransitionAudio()
		{
			// Fade out all audio sources
			GvrAudioSource[] audioSources = FindObjectsOfType<GvrAudioSource>();
			foreach (GvrAudioSource audioSource in audioSources)
				StartCoroutine(audioSource.fadeOut(1));
			// Fade out all ambient sound fields in scene
			GvrAudioSoundfield[] soundfields = FindObjectsOfType<GvrAudioSoundfield>();
			foreach (GvrAudioSoundfield soundfield in soundfields)
				StartCoroutine(soundfield.fadeOut(1));
			// Play transition audio
			GvrAudioSource source = gameObject.AddComponent<GvrAudioSource>();
			source.loop = false;
			source.playOnAwake = false;
			source.clip = sceneTransition;
			source.gainDb = 20f;
			source.Play();
		}
	}
}