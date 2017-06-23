using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SFDC.wef;
using SFDC.wef.data;
using SFDC;
using SFDC.gvr;
using System;

namespace SFDC.wef
{
	[RequireComponent(typeof(SalesforceClient))]
	[RequireComponent(typeof(EditorAnimator))]
	public class EditorSceneController : MonoBehaviour
	{
		[Header("Design space")]
		public Collider designPlane;
		public Transform placedObjects;
		public Transform availableObjects;
		public LineRenderer objectDropBeam;
		public Shader objectSelectionShader;
		
		private SalesforceController sfController;
		private EditorAnimator animator;
		private GvrPointerPhysicsRaycaster controllerRaycaster;

		private Transform pickedObject;

		private GameObject hoveredAvailableObject;
		private GameObject hoveredPlacedObject;
		private Shader lastShader;

		private bool isRotatingPlacedObject = false;
		private bool isDraggingPlacedObject = false;
		private List<Transform> hiddenObjects = new List<Transform>();

		private Color32 objectHoveredColor = new Color32(255, 204, 0, 255);
		private Color32 objectSelectedColor = new Color32(0, 255, 0, 255);

		private const int PLACED_OBJECTS_LAYER = 8;
		private const int AVAILABLE_OBJECTS_LAYER = 9;
		private const int TOGGLEABLE_VOLUME_LAYER = 10;
		
		private const float PICKED_OBJECT_DISTANCE = 0.5f;

		private const string OBJECT_CATEGORY_FURNITURE = "Furniture";
		private const float FURNITURE_OBJECT_ELEVATION = 0.009f;

		private const int AVAILABLE_OBJECT_PER_ROWS = 5;
		private const float AVAILABLE_OBJECT_SPACING = 0.2f;
		private const int MAX_AVAILABLE_OBJECT_DISPLAYED = 3 * AVAILABLE_OBJECT_PER_ROWS;
		private const float PREFAB_SCALE = 0.05f;

		public IEnumerator Start()
		{
			StartCoroutine(Camera.main.fadeIn(3));

			animator = GetComponent<EditorAnimator>();

			// Hook GVR controller ray caster
			controllerRaycaster = FindObjectOfType<GvrPointerPhysicsRaycaster>();
			// Initialize GVR controller swipe monitor
			GvrSwipeMonitor swipeMonitor = FindObjectOfType<GvrSwipeMonitor>();
			swipeMonitor.onSwipeRight += onSwipeRight;
			swipeMonitor.onSwipeLeft += onSwipeLeft;

			// Init Salesforce client & log in
			sfController = new SalesforceController(this, GetComponent<SalesforceClient>());
			Coroutine<bool> loginRoutine = this.StartCoroutine<bool>(sfController.login());
			yield return loginRoutine.coroutine;
			loginRoutine.getValue();

			// Load data from Salesforce
			yield return loadDataFromSalesforce();

			// Load brief movie based on configuration
			FindObjectOfType<BriefVideoMonitor>().loadBrief(ApplicationState.configuration);
		}

		private IEnumerator loadDataFromSalesforce()
		{
			// TODO: remove this after tests
			if (ApplicationState.configuration == null)
				yield return ModelBuilder.loadDefaultTestConfiguration(this, sfController);

			ModelBuilder.initContainerModel(placedObjects, true, PREFAB_SCALE);

			yield return ModelBuilder.loadDataFromSalesforce(this, sfController);
			List<ObjectInstance> objectInstances = CacheManager.getObjectInstances().getAll();

			// Restore object instances
			int availableObjectSlot = 0;
			foreach (ObjectInstance instance in objectInstances)
			{
				if (instance.isPlaced)
				{
					createPlacedGameObject(instance);
					instance.isVisible = true;
				}
				else if (availableObjectSlot < MAX_AVAILABLE_OBJECT_DISPLAYED)
				{
					Vector3 slotPosition = getAvailableObjectSlotPosition(availableObjectSlot);
					createAvailableGameObject(instance, slotPosition);
					instance.isVisible = true;
					availableObjectSlot ++;
				}
			}
		}

		public void Update()
		{
			if (pickedObject != null)
				movePickedObject();
			if (GvrController.State == GvrConnectionState.Connected)
				handleGvrControllerInput();
		}

		private void handleGvrControllerInput()
		{
			// Get controller ray
			Ray controllerRay = controllerRaycaster.GetLastRay();
			//Debug.DrawRay(controllerRay.origin, controllerRay.direction, Color.green, 0.1f, false);
			
			// Hide toggleable volumes hit by controller
			hideToggleableVolumesHitByController();

			// Have we picked an object?
			if (pickedObject != null)
			{
				if (GvrController.ClickButtonDown)
				{
					// Get ray position on design plane
					RaycastHit designPlaneHit;
					bool isPositionOnDesignPlane = designPlane.Raycast(controllerRay, out designPlaneHit, Mathf.Infinity);
					if (isPositionOnDesignPlane)
					{
						// Drop object on design plane
						Transform droppedObject = pickedObject;
						pickedObject = null;
						StartCoroutine(dropObject(droppedObject, designPlaneHit.point));
					}
					else
					{
						// Put back object to its original position
						StartCoroutine(animator.playResetPickedObjectAnimation(pickedObject));
						pickedObject = null;
					}
				}

			}
			// Are we dragging?
			else if (isDraggingPlacedObject)
			{
				// Get ray position on design plane
				RaycastHit designPlaneHit;
				bool isPositionOnDesignPlane = designPlane.Raycast(controllerRay, out designPlaneHit, Mathf.Infinity);
				if (isPositionOnDesignPlane)
				{
					// Move dragged object to mouse position on design plane
					hoveredPlacedObject.transform.position = designPlaneHit.point;
					// Check if clicked released (dragging stopped)
					if (GvrController.ClickButtonUp)
						StartCoroutine(stopDragging());
				}
				else // Stop dragging if we are no longer on the floor
					StartCoroutine(stopDragging());
			}
			else // Not dragging
			{
				// Get the placed object we are aiming at
				GameObject placedObject = getObjectTagettedByController(PLACED_OBJECTS_LAYER);
				bool isPlacedObjectHit = (placedObject != null);
				// Get the available object we are aiming at
				GameObject availableObject = null;
				if (!isPlacedObjectHit)
					availableObject = getObjectTagettedByController(AVAILABLE_OBJECTS_LAYER);
				bool isAvailableObjectHit = (availableObject != null);

				// Update hovered object
				setHoveredPlacedObject(placedObject);
				setHoveredAvailableObject(availableObject);

				// Did we hit a placed object?
				if (isPlacedObjectHit)
				{
					// Touchpad clicked while pointing at a placed object (start dragging)
					if (GvrController.ClickButtonDown)
					{
						isDraggingPlacedObject = true;
						markHoveredObjectAsSelected(true);
					}
					// App button clicked while pointing at a placed object (remove object)
					else if (GvrController.AppButtonDown)
					{
						StartCoroutine(removePlacedObject(placedObject));
						hoveredPlacedObject = null;
					}
				}
				else if (isAvailableObjectHit)
				{
					// Touchpad clicked while pointing at an available object (pick up object)
					if (GvrController.ClickButtonDown)
					{
						StartCoroutine(animator.playPickupAnimation(availableObject.transform));
						pickedObject = availableObject.transform;
						setHoveredAvailableObject(null);
					}
				}
			}
		}

		private void movePickedObject()
		{
			Ray controllerRay = controllerRaycaster.GetLastRay();
			// Displace object
			Vector3 targetPosition = controllerRay.origin + controllerRay.direction * PICKED_OBJECT_DISTANCE;
			targetPosition = Vector3.Lerp(pickedObject.position, targetPosition, 2 * Time.deltaTime);
			targetPosition.y = pickedObject.position.y;
			pickedObject.position = targetPosition;
			// Draw drop beam?
			RaycastHit designPlaneHit;
			bool isPositionOnDesignPlane = designPlane.Raycast(controllerRay, out designPlaneHit, Mathf.Infinity);
			if (isPositionOnDesignPlane)
			{
				objectDropBeam.enabled = true;
				objectDropBeam.SetPosition(0, pickedObject.position);
				objectDropBeam.SetPosition(1, designPlaneHit.point);
			}
			else
				objectDropBeam.enabled = false;
		}

		private IEnumerator dropObject(Transform droppedObject, Vector3 targetPosition)
		{
			yield return animator.playDropAnimation(droppedObject, targetPosition, objectDropBeam);
			yield return placeAvailableObject(droppedObject.gameObject);
		}

		private GameObject getObjectTagettedByController(int layer)
		{
			Ray controllerRay = controllerRaycaster.GetLastRay();
			RaycastHit hit;
			bool isObjectHit = Physics.Raycast(controllerRay, out hit, Mathf.Infinity, 1 << layer);
			return isObjectHit ? hit.collider.gameObject : null;
		}

		private void hideToggleableVolumesHitByController()
		{
			Ray controllerRay = controllerRaycaster.GetLastRay();
			RaycastHit[] hits = Physics.RaycastAll(controllerRay, Mathf.Infinity, 1 << TOGGLEABLE_VOLUME_LAYER);
			// Hide hit objects
			foreach (RaycastHit hit in hits)
			{
				Transform currentHit = hit.transform;
				if (!hiddenObjects.Contains(currentHit))
				{
					hiddenObjects.Add(currentHit);
					currentHit.gameObject.enableRenderers(false);
				}
			}
			// Show objects that are no longer hit
			for (int i = 0; i < hiddenObjects.Count; i++)
			{
				Transform curObjectTransform = hiddenObjects[i];
				// Check if object is hit
				bool isHit = false;
				for (int j = 0; !isHit && j < hits.Length; j++)
				{
					if (hits[j].transform == curObjectTransform)
						isHit = true;
				}
				// Show object if it is not hit
				if (!isHit)
				{
					curObjectTransform.gameObject.enableRenderers(true);
					hiddenObjects.RemoveAt(i);
					i--;
				}
			}
		}

		private void onSwipeRight()
		{
			tryToRotatePlacedObject(90);
		}

		private void onSwipeLeft()
		{
			tryToRotatePlacedObject(-90);
		}

		private void tryToRotatePlacedObject(int rotationAngle)
		{
			// Ensure only one rotation at a time
			if (isRotatingPlacedObject)
				return;
			// Rotate targeted placed object if any
			GameObject targetedObject = getObjectTagettedByController(PLACED_OBJECTS_LAYER);
			if (targetedObject != null)
				StartCoroutine(rotateObject(targetedObject, rotationAngle));
		}

		private IEnumerator rotateObject(GameObject targetedObject, int rotationAngle)
		{
			isRotatingPlacedObject = true;
			StartCoroutine(saveRotation(targetedObject.name, rotationAngle));
			yield return animator.playRotationAnimation(targetedObject.transform, rotationAngle);
			isRotatingPlacedObject = false;
		}

		private IEnumerator saveRotation(String objectId, int rotationAngle)
		{
			// Update local data
			ObjectInstance objectInstance = CacheManager.getObjectInstances().get(objectId);
			objectInstance.yAngle = (objectInstance.yAngle + rotationAngle) % 360;
			// Update object in Salesforce
			Coroutine<ObjectInstance> upsertRoutine = this.StartCoroutine<ObjectInstance>(sfController.upsertObjectInstance(objectInstance));
			yield return upsertRoutine.coroutine;
			upsertRoutine.getValue();
			Debug.Log("Rotated object " + objectId);
		}

		private IEnumerator stopDragging()
		{
			isDraggingPlacedObject = false;
			markHoveredObjectAsSelected(false);
			string objectId = hoveredPlacedObject.name;
			Vector3 objectPosition = hoveredPlacedObject.transform.localPosition;
			// Update object instance data
			ObjectInstance objectInstance = CacheManager.getObjectInstances().get(objectId);
			objectInstance.position = objectPosition;
			// Update object in Salesforce
			Coroutine<ObjectInstance> instRoutine = this.StartCoroutine<ObjectInstance>(sfController.upsertObjectInstance(objectInstance));
			yield return instRoutine.coroutine;
			instRoutine.getValue();
			Debug.Log("Moved object " + objectId + " to " + objectPosition.getAsString());
		}

		private void markHoveredObjectAsSelected(bool isSelected)
		{
			Renderer renderer = getRenderer(hoveredPlacedObject);
			if (isSelected)
				renderer.material.SetColor("_edgeColour", objectSelectedColor);
			else
				renderer.material.SetColor("_edgeColour", objectHoveredColor);
		}

		private void setHoveredPlacedObject(GameObject newHoveredObject)
		{
			if (hoveredPlacedObject != null)
			{
				Renderer renderer = getRenderer(hoveredPlacedObject);
				renderer.material.shader = lastShader;
				hoveredPlacedObject = null;
			}
			if (newHoveredObject != null)
			{
				Renderer renderer = getRenderer(newHoveredObject);
				lastShader = renderer.material.shader;
				renderer.material.shader = objectSelectionShader;
				hoveredPlacedObject = newHoveredObject;
			}
		}

		private void setHoveredAvailableObject(GameObject newHoveredObject)
		{
			if (hoveredAvailableObject != null)
			{
				Renderer renderer = getRenderer(hoveredAvailableObject);
				renderer.material.shader = lastShader;
				hoveredAvailableObject = null;
			}
			if (newHoveredObject != null)
			{
				Renderer renderer = getRenderer(newHoveredObject);
				lastShader = renderer.material.shader;
				renderer.material.shader = objectSelectionShader;
				hoveredAvailableObject = newHoveredObject;
			}
		}

		private Renderer getRenderer(GameObject gameObject)
		{
			Renderer renderer = gameObject.GetComponent<Renderer>();
			if (renderer == null)
				return gameObject.GetComponentInChildren<Renderer>();
			else
				return renderer;
		}

		private GameObject createPlacedGameObject(ObjectInstance objectInstance)
		{
			ObjectDefinition definition = objectInstance.definition;
			GameObject gameObject = GameObject.Instantiate(definition.getPrefab(ApplicationState.configuration));
			if (objectInstance.id != null)
				gameObject.name = objectInstance.id;
			gameObject.transform.localScale = new Vector3(PREFAB_SCALE, PREFAB_SCALE, PREFAB_SCALE);
			gameObject.transform.SetParent(placedObjects);
			gameObject.transform.localPosition = objectInstance.position;
			gameObject.transform.Rotate(0, objectInstance.yAngle, 0, Space.World);
			gameObject.layer = PLACED_OBJECTS_LAYER;
			return gameObject;
		}

		private GameObject createAvailableGameObject(ObjectInstance objectInstance, Vector3 position)
		{
			ObjectDefinition definition = objectInstance.definition;
			GameObject gameObject = GameObject.Instantiate(definition.getPrefab(ApplicationState.configuration));
			gameObject.name = objectInstance.id;
			gameObject.transform.localScale = new Vector3(PREFAB_SCALE, PREFAB_SCALE, PREFAB_SCALE);
			gameObject.transform.SetParent(availableObjects);
			gameObject.transform.localPosition = position;
			gameObject.transform.Rotate(0, 270, 0, Space.World);
			gameObject.layer = AVAILABLE_OBJECTS_LAYER;
			return gameObject;
		}

		private IEnumerator placeAvailableObject(GameObject availableObject)
		{
			Vector3 position = availableObject.transform.position;
			// Prevent object selection
			availableObject.layer = 0;
			// Update local object instance data
			string objectId = availableObject.name;
			ObjectInstance objectInstance = CacheManager.getObjectInstances().get(objectId);
			objectInstance.isPlaced = true;
			if (objectInstance.definition.category == OBJECT_CATEGORY_FURNITURE)
				position.y += FURNITURE_OBJECT_ELEVATION;
			objectInstance.position = placedObjects.InverseTransformPoint(position);

			// Update object in Salesforce
			Coroutine<ObjectInstance> upsertRoutine = this.StartCoroutine<ObjectInstance>(sfController.upsertObjectInstance(objectInstance));
			yield return upsertRoutine.coroutine;
			upsertRoutine.getValue();
			Debug.Log("Placed object " + objectId);
			// Complete object transition
			availableObject.layer = PLACED_OBJECTS_LAYER;
			availableObject.transform.position = position;
			availableObject.transform.SetParent(placedObjects);
			// Refresh available objects
			refreshAvailableObject();
		}

		private IEnumerator removePlacedObject(GameObject placedObject)
		{
			string objectId = placedObject.name;
			// Prevent object selection
			placedObject.layer = 0;
			// Remove game object
			StartCoroutine(animator.playRemoveObjectAnimation(placedObject));
			// Update object instance data
			ObjectInstance objectInstance = CacheManager.getObjectInstances().get(objectId);
			objectInstance.isPlaced = false;
			objectInstance.isVisible = false;
			// Update object in Salesforce
			Coroutine<ObjectInstance> upsertRoutine = this.StartCoroutine<ObjectInstance>(sfController.upsertObjectInstance(objectInstance));
			yield return upsertRoutine.coroutine;
			upsertRoutine.getValue();
			Debug.Log("Removed object " + objectId);
			// Refresh available objects
			refreshAvailableObject();
		}

		private void refreshAvailableObject()
		{
			// Check for empty object slots
			int displayedObjectCount = availableObjects.transform.childCount;
			if (displayedObjectCount == MAX_AVAILABLE_OBJECT_DISPLAYED)
				return;
			// Get available objects that are not yet displayed
			List<ObjectInstance> allObjectInstances = CacheManager.getObjectInstances().getAll();
			List<ObjectInstance> hiddenAvailableObjectInstances = new List<ObjectInstance>();
			foreach (ObjectInstance objectInstance in allObjectInstances)
			{
				if (!objectInstance.isPlaced && !objectInstance.isVisible)
					hiddenAvailableObjectInstances.Add(objectInstance);
			}
			// Fill empty slots
			for (int slotIndex = 0; hiddenAvailableObjectInstances.Count != 0 && slotIndex < MAX_AVAILABLE_OBJECT_DISPLAYED; slotIndex++)
			{
				Vector3 slotPosition = getAvailableObjectSlotPosition(slotIndex);
				// Check if slot is empty
				bool isSlotEmpty = true;
				for (int i = 0; isSlotEmpty && i < displayedObjectCount; i++)
				{
					Vector3 objectPosition = availableObjects.transform.GetChild(i).localPosition;
					if (objectPosition == slotPosition)
						isSlotEmpty = false;
				}
				// Fill slot if it is empty
				if (isSlotEmpty)
				{
					ObjectInstance availableObjectInstance = hiddenAvailableObjectInstances[0];
					hiddenAvailableObjectInstances.RemoveAt(0);
					createAvailableGameObject(availableObjectInstance, slotPosition);
					availableObjectInstance.isVisible = true;
					displayedObjectCount++;
				}
			}
		}

		private Vector3 getAvailableObjectSlotPosition(int slotIndex)
		{
			int row = slotIndex / AVAILABLE_OBJECT_PER_ROWS;
			int col = slotIndex % AVAILABLE_OBJECT_PER_ROWS;
			return new Vector3(0 - row * AVAILABLE_OBJECT_SPACING, 0, col * AVAILABLE_OBJECT_SPACING);
		}
	}
}