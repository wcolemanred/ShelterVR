using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;
using System.Collections.Generic;
using System;

namespace SFDC.wef.data
{
	public class ObjectDefinition : DataWithId
	{
		public string name { get; set; }
		public string prefabPath { get; set; }
		public string category { get; set; }
		public bool isClimateSpecific { get; set; }
		public int modelVariations { get; set; }

		// Unity prefab for this object (not saved in Salesforce)
		private GameObject unityPrefab;
		public const string PREFAB_BASE_PATH = "Prefabs/Objects/";

		public ObjectDefinition(string id, string name, string prefabPath, string category, bool isClimateSpecific, int modelVariations)
		{
			this.id = id;
			this.name = name;
			this.prefabPath = prefabPath;
			this.category = category;
			this.isClimateSpecific = isClimateSpecific;
			this.modelVariations = modelVariations;
		}

		public ObjectDefinition(JSONValue record)
		{
			JSONObject recordObject = record.Obj;

			id = recordObject.GetString("Id");
			name = recordObject.GetString("Name");
			prefabPath = recordObject.GetString("Prefab_path__c");
			category = recordObject.GetString("Category__c");
			isClimateSpecific = recordObject.GetBoolean("Climate_Specific__c");
			modelVariations = (int) recordObject.GetNumber("Model_Variations__c");
		}

		public static string getSelectQueryBase()
		{
			return "SELECT Id, Name, Prefab_path__c, Category__c, Climate_Specific__c, Model_Variations__c FROM Object_Definition__c";
		}

		/**
		 * Loads objects from a JSON array 
		 **/
		public static List<ObjectDefinition> parseFromJsonArray(JSONArray array)
		{
			List<ObjectDefinition> items = new List<ObjectDefinition>();
			foreach (JSONValue item in array)
				items.Add(new ObjectDefinition(item));
			return items;
		}

		public GameObject getPrefab(Configuration configuration)
		{
			// Load local prefab on first use
			if (!isRemotePrefab())
				loadLocalPrefab(configuration);
			return unityPrefab;
		}

		public bool isRemotePrefab()
		{
			return prefabPath.StartsWith("http");
		}

		public IEnumerator preloadRemotePrefab()
		{
			Debug.Log("Downloading remote asset bundle: " + prefabPath);
			using (WWW www = new WWW(prefabPath))
			{
				yield return www;
				if (www.error != null)
					throw new Exception("Download of remote asset "+ prefabPath + " failed: " + www.error);
				AssetBundle bundle = www.assetBundle;
				if (bundle == null)
					throw new Exception("Downloaded file is not a Unity asset bundle.");
				unityPrefab = bundle.LoadAsset<GameObject>("tinker-prefab");
				if (unityPrefab == null)
					throw new Exception("Missing 'tinker-prefab' game object in remote bundle");
				bundle.Unload(false);
			}
		}

		private void loadLocalPrefab(Configuration configuration)
		{
			// Do not cache prefab if there are variations
			if (modelVariations > 1 || unityPrefab == null)
			{
				string fullPrefabPath = PREFAB_BASE_PATH + prefabPath;
				if (modelVariations > 1)
					fullPrefabPath += "_" + UnityEngine.Random.Range(0, modelVariations - 1);
				if (isClimateSpecific)
					fullPrefabPath += "_" + configuration.climate;

				unityPrefab = Resources.Load<GameObject>(fullPrefabPath);
				if (unityPrefab == null)
					Debug.LogError("Missing local prefab: " + fullPrefabPath);
			}
		}
	}
}
