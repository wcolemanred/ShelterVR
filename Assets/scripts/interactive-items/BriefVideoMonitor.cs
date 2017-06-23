using UnityEngine;
using System.Collections;
using SFDC.gvr;
using System;
using UnityEngine.UI;
using SFDC.wef.data;

[RequireComponent(typeof(GvrVideoPlayerTexture))]
public class BriefVideoMonitor: MonoBehaviour {

	public Material videoMaterial;

	private GvrVideoPlayerTexture videoPlayer;
	private bool wasPlayTriggered = false;
	private Material defaultMaterial;

	void Start()
	{
		videoPlayer = GetComponent<GvrVideoPlayerTexture>();
	}

	public void Update()
	{
		if (wasPlayTriggered && videoPlayer.PlayerState == GvrVideoPlayerTexture.VideoPlayerState.Ended)
		{
			videoPlayer.CleanupVideo();
			GetComponent<Renderer>().material = defaultMaterial;
		}
	}

	public void loadBrief(Configuration configuration)
	{
		videoPlayer.videoURL = "jar:file://${Application.dataPath}!/assets/brief"+ configuration.climate + ".mp4";
		videoPlayer.SetOnExceptionCallback((type, message) => {
			Debug.LogError("GvrVideoPlayerTexture threw Exception of type: " + type + ": " + message);
		});
		if (!videoPlayer.Init())
			Debug.LogError("Failed to init GvrVideoPlayerTexture with video: "+ videoPlayer.videoURL);
	}

	public void playBrief()
	{
		if (videoPlayer.PlayerState == GvrVideoPlayerTexture.VideoPlayerState.Ready)
		{
			onHoverEnd();
			wasPlayTriggered = true;

			Renderer renderer = GetComponent<Renderer>();
			defaultMaterial = renderer.material;
			renderer.material = videoMaterial;

			videoPlayer.Play();
		}
	}

	public void onHoverStart()
	{
		if (!wasPlayTriggered)
			GetComponent<Renderer>().material.color = Color.yellow;
	}

	public void onHoverEnd()
	{
		if (!wasPlayTriggered)
			GetComponent<Renderer>().material.color = Color.white;
	}
}
