using UnityEngine;
using System.Collections;
using SFDC.wef;
using SFDC;
using SFDC.wef.data;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SFDC.wef.ui;
using SFDC.gvr;
using UnityEngine.EventSystems;

namespace SFDC.wef
{
	[RequireComponent(typeof(SalesforceClient))]
	[RequireComponent(typeof(VRUtil))]
	public class MenuSceneController : MonoBehaviour
	{
		public GvrAudioSource ambientAudio;
		public GameObject laser;
		[Header("UI")]
		public Canvas nonVrUiCanvas;
		public Canvas vrUiCanvas;
		public RectTransform configurationSelectionContainer;
		public RectTransform navigationSelectionContainer;
		[Header("Scenes")]
		public string editorScene;
		public string showroomSceen;

		private SalesforceController sfController;
		private LoginUI loginUI;

		private const float UI_FADE_DURATION = 0.5f;

		public IEnumerator Start()
		{
			loginUI = FindObjectOfType<LoginUI>();

			// Init SF controller
			SalesforceClient client = GetComponent<SalesforceClient>();
			if (ApplicationState.sfConnection != null)
				client.setConnection(ApplicationState.sfConnection);
			sfController = new SalesforceController(this, client);

			// Is this the first run?
			if (ApplicationState.sfConnection == null)
			{
				// Show login screen
				yield return FindObjectOfType<VRUtil>().enableVrMode(false);
				loginUI.setSalesforceController(sfController);
				StartCoroutine(loginUI.fadeIn());
			}
			else // Returning to menu, display config list
			{
				// Destroy non-VR UI
				Destroy(loginUI.gameObject);
				Destroy(FindObjectOfType<StandaloneInputModule>());
				// Enable VR UI
				StartCoroutine(Camera.main.fadeIn(3));
				setVrUiScreen(configurationSelectionContainer.name);
				yield return initConfigSelectionView();
				yield return transitionUi(true);
			}
		}

		public IEnumerator displayConfigurationList()
		{
			yield return Camera.main.fadeOut(0.01f);
			StartCoroutine(loginUI.fadeOutAndDestroy());

			setVrUiScreen(configurationSelectionContainer.name);
			StartCoroutine(initConfigSelectionView());

			ambientAudio.Play();
			yield return FindObjectOfType<VRUtil>().enableVrMode(true);
			yield return Camera.main.fadeIn(2f);
		}

		private IEnumerator initConfigSelectionView()
		{
			// Fetch configurations
			Coroutine<List<Configuration>> configRoutine = this.StartCoroutine<List<Configuration>>(sfController.getConfigurations());
			yield return configRoutine.coroutine;
			List<Configuration> configurations = configRoutine.getValue();
			Debug.Log("Loaded " + configurations.Count + " configurations");
			// Configure screen
			ConfigurationSelectionScreen screen = configurationSelectionContainer.GetComponent<ConfigurationSelectionScreen>();
			screen.init(configurations);
			screen.onSelectConfiguration += (configuration) => StartCoroutine(onSelectConfiguration(configuration));
		}

		private IEnumerator onSelectConfiguration(Configuration selectedConfiguration)
		{
			// Disable all buttons
			UiHelper.setAllButtonEnabled(configurationSelectionContainer, false);
			// Process action
			ApplicationState.configuration = selectedConfiguration;
			// Transition to navigation selection screen
			yield return transitionUi(false);
			setVrUiScreen(navigationSelectionContainer.name);
			yield return transitionUi(true);
		}

		public void onGoToEditor()
		{
			StartCoroutine(transitionToScene(editorScene));
		}

		public void onGoToShowroom()
		{
			StartCoroutine(transitionToScene(showroomSceen));
		}

		private IEnumerator transitionToScene(string sceneName)
		{
			UiHelper.setAllButtonEnabled(navigationSelectionContainer, false);
			// Start to load next scene
			AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
			ao.allowSceneActivation = false;
			// Transition UI out
			StartCoroutine(ambientAudio.fadeOut(1));
			StartCoroutine(transitionUi(false));
			// Fade out
			yield return StartCoroutine(Camera.main.fadeOut(2));
			// Switch to next scene
			ao.allowSceneActivation = true;
		}

		private IEnumerator transitionUi(bool isFadeIn)
		{
			laser.SetActive(false);
			//yield return null;
			float startAlpha, endAlpha;
			if (isFadeIn)
			{
				startAlpha = 0f;
				endAlpha = 1f;
			}
			else
			{
				startAlpha = 1f;
				endAlpha = 0f;
			}
			// Set initial alpha
			Text[] texts = vrUiCanvas.GetComponentsInChildren<Text>();
			foreach (Text text in texts)
				text.setAlpha(startAlpha);
			List<CanvasRenderer> imageRenderers = new List<CanvasRenderer>();
			Image[] images = vrUiCanvas.GetComponentsInChildren<Image>();
			foreach (Image image in images)
			{
				CanvasRenderer renderer = image.GetComponent<CanvasRenderer>();
				if (image.GetComponent<Mask>() == null)
				{
					renderer.SetAlpha(startAlpha);
					imageRenderers.Add(renderer);
				}
			}
			// Fade alpha
			float timer = 0f;
			while (timer <= UI_FADE_DURATION)
			{
				float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / UI_FADE_DURATION);
				foreach (Text text in texts)
					text.setAlpha(alpha);
				foreach (CanvasRenderer imageRenderer in imageRenderers)
					imageRenderer.SetAlpha(alpha);

				timer += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			if (isFadeIn)
				laser.SetActive(true);
		}

		private void setVrUiScreen(string name)
		{
			foreach (Transform child in vrUiCanvas.transform)
			{
				child.gameObject.SetActive(child.name == name);
				// Fade texts & images
				if (child.name == name)
				{
					Text[] texts = child.GetComponentsInChildren<Text>();
					foreach (Text text in texts)
						text.setAlpha(0f);
					Image[] images = child.GetComponentsInChildren<Image>();
					foreach (Image image in images)
					{
						CanvasRenderer renderer = image.GetComponent<CanvasRenderer>();
						if (image.GetComponent<Mask>() == null)
							renderer.SetAlpha(0f);
					}
				}
			}
		}
	}
}