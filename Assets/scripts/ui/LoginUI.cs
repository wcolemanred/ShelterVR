using SFDC;
using SFDC.wef;
using SFDC.wef.data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class LoginUI : MonoBehaviour {

	public InputField usernameField;
	public InputField passwordField;
	public Button loginButton;
	public Text errorMessage;
	public GameObject spinner;
	public Image uiFader;

	private SalesforceController sfController;
	private bool isLoading;

	void Start () {
		usernameField.onValueChanged.AddListener((username) => toggleLoginButton(username, passwordField.text));
		passwordField.onValueChanged.AddListener((password) => toggleLoginButton(usernameField.text, password));
		loginButton.onClick.AddListener(() => StartCoroutine(loginCoroutine()));

		usernameField.text = SalesforceAuthConfig.DEFAULT_USERNAME;
		passwordField.text = SalesforceAuthConfig.DEFAULT_PASSWORD;

		toggleLoginButton(usernameField.text, passwordField.text);
	}

	public void setSalesforceController(SalesforceController sfController)
	{
		this.sfController = sfController;
	}

	public IEnumerator fadeIn()
	{
		GetComponent<Canvas>().enabled = true;
		uiFader.gameObject.SetActive(true);
		yield return uiFader.fadeIn(2f);
		uiFader.gameObject.SetActive(false);
	}

	public IEnumerator fadeOutAndDestroy()
	{
		uiFader.gameObject.SetActive(true);
		yield return uiFader.fadeOut(2f);
		Destroy(gameObject);
	}

	private IEnumerator loginCoroutine()
	{
		setLoadingMode(true);
		// Try to login on selected org
		Coroutine<bool> loginRoutine = this.StartCoroutine<bool>(sfController.login(usernameField.text, passwordField.text));
		yield return loginRoutine.coroutine;
		try
		{
			loginRoutine.getValue();
			onLoginSucceeded();
		}
		catch (SalesforceAuthenticationException) // Login failed due to bad password
		{
			onLoginFailed("Invalid username or password");
		}
		catch (Exception e) // Login failed due to other technical error
		{
			onLoginFailed(e.Message);
		}
	}
	
	private void setLoadingMode(bool isLoading)
	{
		usernameField.interactable = !isLoading;
		passwordField.interactable = !isLoading;
		errorMessage.gameObject.SetActive(!isLoading);
		loginButton.gameObject.SetActive(!isLoading);
		spinner.SetActive(isLoading);
	}

	private void onLoginSucceeded()
	{
		ApplicationState.sfConnection = FindObjectOfType<SalesforceClient>().getConnection();
		StartCoroutine(FindObjectOfType<MenuSceneController>().displayConfigurationList());
	}

	private void onLoginFailed(string error)
	{
		setLoadingMode(false);
		errorMessage.text = error;
		passwordField.text = "";
		loginButton.interactable = false;
	}

	private void toggleLoginButton(string username, string password)
	{
		loginButton.interactable = (!"".Equals(username) && !"".Equals(password));
	}
}
