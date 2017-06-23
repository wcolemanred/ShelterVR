using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VR;

public class VRUtil : MonoBehaviour {

	private GameObject gvrController;
	private GameObject gvrControllerVisual;
	private string initialVrDevice = null;

	void Awake () {
		gvrController = FindObjectOfType<GvrController>().gameObject;
		gvrControllerVisual = FindObjectOfType<GvrControllerVisualManager>().gameObject;
		initialVrDevice = VRSettings.loadedDeviceName;
	}
	
	public IEnumerator enableVrMode(bool isVrEnabled)
	{
		string newDevice = (isVrEnabled) ? initialVrDevice : "";
		// Disable controller before disabling VR
		if (!isVrEnabled)
			enableGvrController(false);
		// Load VR device
		VRSettings.LoadDeviceByName(newDevice);
		if (!VRSettings.loadedDeviceName.Equals(newDevice))
			yield return null;
		// Toggle VR mode
		VRSettings.enabled = isVrEnabled;
		if (VRSettings.enabled != isVrEnabled)
			yield return null;
		Debug.Log("Switched to device [" + newDevice + "], vr=" + isVrEnabled);
		// Enable controller after enabling VR
		if (isVrEnabled)
		{
			StandaloneInputModule inputModule = FindObjectOfType<StandaloneInputModule>();
			if (inputModule != null)
				Destroy(inputModule);
			enableGvrController(true);
		}
	}

	public void enableGvrController(bool isActive)
	{
		gvrControllerVisual.SetActive(isActive);
		gvrController.SetActive(isActive);
	}
}
