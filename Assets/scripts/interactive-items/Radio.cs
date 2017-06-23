using UnityEngine;
using System.Collections;
using SFDC.gvr;

[RequireComponent(typeof(GvrAudioSource))]
public class Radio: MonoBehaviour {

	public AudioClip[] radioTracks;

	private int trackIndex = 0;
	private GvrAudioSource audioSource;

	void Start () {
		audioSource = GetComponent<GvrAudioSource>();
		trackIndex = Random.Range(0, radioTracks.Length -1);
	}

	public void togglePlay() {
		onHoverEnd();

		if (audioSource.isPlaying)
		{
			audioSource.Stop();
		}
		else
		{
			audioSource.clip = radioTracks[trackIndex];
			audioSource.Play();

			trackIndex++;
			if (trackIndex == radioTracks.Length)
				trackIndex = 0;
		}
	}

	public void onHoverStart()
	{
		GetComponent<Renderer>().material.color = Color.yellow;
	}

	public void onHoverEnd()
	{
		GetComponent<Renderer>().material.color = Color.white;
	}
}
