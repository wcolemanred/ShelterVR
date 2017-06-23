using UnityEngine;
using System.Collections;
using SFDC.wef.data;
using System.Collections.Generic;

namespace SFDC.wef
{
	public class ModelBuilder
	{
		public static IEnumerator loadDefaultTestConfiguration(MonoBehaviour owner, SalesforceController sfController)
		{
			Coroutine<Configuration> configRoutine = owner.StartCoroutine<Configuration>(sfController.getConfiguration("a020Y000001AyEKQA0"));
			yield return configRoutine.coroutine;
			ApplicationState.configuration = configRoutine.getValue();
			Debug.LogWarning("Testing with configuration " + ApplicationState.configuration.name);
		}

		public static IEnumerator loadDataFromSalesforce(MonoBehaviour owner, SalesforceController sfController)
		{
			// Fetch object definitions
			CacheManager.getObjectDefinitions().clear();
			Coroutine<List<ObjectDefinition>> defRoutine = owner.StartCoroutine<List<ObjectDefinition>>(sfController.getObjectDefinitions());
			yield return defRoutine.coroutine;
			List<ObjectDefinition> objectDefinitions = defRoutine.getValue();
			CacheManager.getObjectDefinitions().put(objectDefinitions);
			Debug.Log("Loaded " + objectDefinitions.Count + " object definitions");
			
			// Fetch object instances
			CacheManager.getObjectInstances().clear();
			Coroutine<List<ObjectInstance>> instRoutine = owner.StartCoroutine<List<ObjectInstance>>(sfController.getObjectInstancesFromConfiguration(ApplicationState.configuration.id));
			yield return instRoutine.coroutine;
			List<ObjectInstance> objectInstances = instRoutine.getValue();
			CacheManager.getObjectInstances().put(objectInstances);
			Debug.Log("Loaded " + objectInstances.Count + " object instances from configuration " + ApplicationState.configuration.name);

			// Only preload remote object definitions that are in use
			foreach (ObjectDefinition definition in objectDefinitions)
			{
				if (definition.isRemotePrefab())
				{
					bool isDefinitionUsed = false;
					for (int i=0; !isDefinitionUsed && i <objectInstances.Count; i++)
					{
						if (objectInstances[i].definition.id == definition.id)
							isDefinitionUsed = true;
					}
					if (isDefinitionUsed)
						yield return definition.preloadRemotePrefab();
				}
				
			}
		}

		public static GameObject initContainerModel(Transform modelOrigin, bool isOpen, float scale)
		{
			Configuration configuration = ApplicationState.configuration;
			string prefabPath = ObjectDefinition.PREFAB_BASE_PATH + "Containers/12m/Container_"
				+ configuration.availableSpace + "_12m_"
				+ (isOpen ? "Open_" : "")
				+ configuration.climate;
			GameObject containerPrefab = Resources.Load<GameObject>(prefabPath);
			if (containerPrefab == null)
				Debug.LogError("Missing prefab: " + prefabPath);
			GameObject container = GameObject.Instantiate(containerPrefab);
			container.transform.SetParent(modelOrigin);
			container.transform.localPosition = Vector3.zero;
			container.transform.localScale = new Vector3(scale, scale, scale);
			return container;
		}
	}
}
