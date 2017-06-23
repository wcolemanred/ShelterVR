using UnityEngine;
using Boomlagoon.JSON;
using SFDC.wef.data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SFDC.wef
{
	public class SalesforceController
	{
		private MonoBehaviour owner;
		private SalesforceClient sfClient;

		public SalesforceController(MonoBehaviour owner, SalesforceClient sfClient) {
			this.owner = owner;
			this.sfClient = sfClient;
		}

		public SalesforceClient getClient()
		{
			return sfClient;
		}

		// Resumes connection or connects with default user credentials
		public IEnumerator login()
		{
			yield return login(SalesforceAuthConfig.DEFAULT_USERNAME, SalesforceAuthConfig.DEFAULT_PASSWORD);
		}

		public IEnumerator login(string username, string password)
		{
			// Check for existing connection
			SalesforceConnection connection = ApplicationState.sfConnection;
			if (connection != null)
			{
				sfClient.setConnection(connection);
				Debug.Log("Salesforce connection resumed.");
				yield return true;
			}
			else // Establish a new connection
			{
				// Set OAuth settings
				sfClient.consumerKey = SalesforceAuthConfig.OAUTH_CONSUMER_KEY;
				sfClient.consumerSecret = SalesforceAuthConfig.OAUTH_CONSUMER_SECRET;
				// Attemp login
				bool isUserLogged = false;
				bool shouldRetry = true;
				while (!isUserLogged && shouldRetry)
				{
					Coroutine<bool> routine = owner.StartCoroutine<bool>(
						sfClient.login(username, password)
					);
					yield return routine.coroutine;
					try
					{
						isUserLogged = routine.getValue();
						Debug.Log("Salesforce login successful.");
						// Store connection for later use
						ApplicationState.sfConnection = sfClient.getConnection();
					}
					catch (SalesforceConfigurationException e)
					{
						throw e;
					}
					catch (SalesforceAuthenticationException e)
					{
						throw e;
					}
					catch (SalesforceApiException)
					{
						Debug.Log("Salesforce login failed, retrying...");
					}
				}
				yield return isUserLogged;
			}
		}

		public IEnumerator getConfigurations()
		{
			Coroutine<JSONArray> routine = owner.StartCoroutine<JSONArray>(
				runSelectQuery(Configuration.getSelectQueryBase() +" ORDER BY Name")
			);
			yield return routine.coroutine;
			JSONArray json = routine.getValue();
			yield return Configuration.parseFromJsonArray(json);
		}

		public IEnumerator getConfiguration(string configurationId)
		{
			Coroutine<JSONArray> routine = owner.StartCoroutine<JSONArray>(
				runSelectQuery(Configuration.getSelectQueryBase() +" WHERE Configuration__c.Id='" + configurationId + "'")
			);
			yield return routine.coroutine;
			JSONArray json = routine.getValue();
			List<Configuration> configurations = Configuration.parseFromJsonArray(json);
			if (configurations.Count == 0)
				throw new Exception("Could not load configuration "+ configurationId);
			yield return configurations[0];
		}

		public IEnumerator getObjectDefinitions()
		{
			Coroutine<JSONArray> routine = owner.StartCoroutine<JSONArray>(
				runSelectQuery(ObjectDefinition.getSelectQueryBase())
			);
			yield return routine.coroutine;
			JSONArray json = routine.getValue();
			yield return ObjectDefinition.parseFromJsonArray(json);
		}

		public IEnumerator getObjectInstancesFromConfiguration(string configurationId)
		{
			Coroutine<JSONArray> routine = owner.StartCoroutine<JSONArray>(
				runSelectQuery(ObjectInstance.getSelectQueryBase() + " WHERE Configuration__c='" + configurationId +"'")
			);
			yield return routine.coroutine;
			JSONArray json = routine.getValue();
			yield return ObjectInstance.parseFromJsonArray(json);
		}

		public IEnumerator upsertObjectInstance(ObjectInstance objectInstance)
		{
			// Assemble payload
			JSONObject jsonBody = new JSONObject();
			jsonBody.Add("instance", objectInstance.toJson());
			// Send request
			Coroutine<string> routine = owner.StartCoroutine<string>(
				sfClient.runApex("POST", "ObjectInstance", jsonBody.ToString(), null)
			);
			yield return routine.coroutine;
			// Parse JSON response
			JSONObject jsonResponse = JSONObject.Parse(routine.getValue());
			yield return new ObjectInstance(jsonResponse);
		}

		public IEnumerator deleteObjectInstance(ObjectInstance objectInstance)
		{
			Coroutine<bool> routine = owner.StartCoroutine<bool>(
				sfClient.delete(objectInstance.id, "Object_Instance__c")
			);
			yield return routine.coroutine;
			yield return routine.getValue();
		}

		private IEnumerator runSelectQuery(string query) {
			// Run query to retrieve records
			Coroutine<string> routine = owner.StartCoroutine<string>(
				sfClient.query(query)
			);
			yield return routine.coroutine;
			// Parse JSON response
			JSONObject json = JSONObject.Parse(routine.getValue());
			yield return json.GetArray("records");
		}
	}
}

