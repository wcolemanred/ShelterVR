using UnityEngine;
using System.Collections;
using SFDC.wef;
using SFDC;
using SFDC.wef.data;
using System.Collections.Generic;

namespace SFDC.wef
{
	public class ShowroomController : MonoBehaviour
	{
		public Transform cameraContainer;
		public Transform modelOrigin;
		public Transform navigatioNodeContainer;

		private SalesforceController sfController;

		private static GameObject navNodePrefab;

		private const float NAV_NODE_SPACING = 11;
		private const float NAV_NODE_SPACING_Y = 4;
		private const float COORDS_SCALE = 20;

		IEnumerator Start()
		{
			StartCoroutine(cameraContainer.GetComponentInChildren<Camera>().fadeIn(3));
			StartCoroutine(spawnNavigationNodes());

			// Init Salesforce client & log in
			sfController = new SalesforceController(this, GetComponent<SalesforceClient>());
			Coroutine<bool> loginRoutine = this.StartCoroutine<bool>(sfController.login());
			yield return loginRoutine.coroutine;
			loginRoutine.getValue();

			// Load data from Salesforce
			yield return loadDataFromSalesforce();
		}

		private IEnumerator spawnNavigationNodes()
		{
			yield return new WaitForSeconds(1.5f);

			// Create node grid
			navNodePrefab = Resources.Load<GameObject>("Prefabs/NavigationNode");
			Vector3 startPos = cameraContainer.transform.position;
			for (float y = 0; y < 2; y++)
			{
				for (float x = 0; x < 3; x++)
				{
					spawnNavigationNode(startPos, new Vector3(x, y, 0));
					spawnNavigationNode(startPos, new Vector3(x, y, 2));
				}
				for (float z = 1; z < 2; z++)
				{
					spawnNavigationNode(startPos, new Vector3(0, y, z));
					spawnNavigationNode(startPos, new Vector3(2, y, z));
				}
			}
			// Set current node
			NavigationNode[] nodes = FindObjectsOfType<NavigationNode>();
			bool isInitialNodeFound = false;
			for (int i=0; !isInitialNodeFound && i < nodes.Length; i++)
			{
				Vector3 nodePos = nodes[i].transform.position;
				if (nodePos == startPos)
				{
					isInitialNodeFound = true;
					NavigationNode.initCurrentNode(nodes[i]);
				}
			}
			// Add middle node
			GameObject navNode = GameObject.Instantiate(navNodePrefab);
			navNode.name = "middleNode";
			navNode.transform.parent = navigatioNodeContainer;
			Vector3 position = modelOrigin.position;
			position.y += 1.5f;
			navNode.transform.position = position;
		}

		private void spawnNavigationNode(Vector3 startPos, Vector3 nodeCoords)
		{
			GameObject navNode = GameObject.Instantiate(navNodePrefab);
			navNode.name = nodeCoords.ToString();
			navNode.transform.parent = navigatioNodeContainer;
			navNode.transform.position = new Vector3(startPos.x - nodeCoords.x * NAV_NODE_SPACING, startPos.y + nodeCoords.y * NAV_NODE_SPACING_Y, startPos.z + nodeCoords.z * NAV_NODE_SPACING);
		}

		private IEnumerator loadDataFromSalesforce()
		{
			// TODO: remove this after tests
			if (ApplicationState.configuration == null)
				yield return ModelBuilder.loadDefaultTestConfiguration(this, sfController);

			GameObject container = ModelBuilder.initContainerModel(modelOrigin, false, 1);
			container.AddComponent<WallHack>();

			yield return ModelBuilder.loadDataFromSalesforce(this, sfController);
			List<ObjectInstance> objectInstances = CacheManager.getObjectInstances().getAll();

			// Restore object instances
			foreach (ObjectInstance instance in objectInstances)
			{
				if (instance.isPlaced)
					createPlacedGameObject(instance);
			}
		}

		private GameObject createPlacedGameObject(ObjectInstance objectInstance)
		{
			ObjectDefinition definition = objectInstance.definition;
			GameObject gameObject = GameObject.Instantiate(definition.getPrefab(ApplicationState.configuration));
			if (objectInstance.id != null)
				gameObject.name = objectInstance.id;
			gameObject.transform.SetParent(modelOrigin);
			gameObject.transform.localPosition = objectInstance.position * COORDS_SCALE;
			gameObject.transform.Rotate(0, objectInstance.yAngle, 0, Space.World);
			gameObject.disableAllColliders();
			return gameObject;
		}
	}
}