using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;
using System.Collections.Generic;

namespace SFDC.wef.data
{
	public class ObjectInstance : DataWithId
	{
		public string configurationId { get; set; }
		public ObjectDefinition definition { get; set; }
		public Vector3 position { get; set; }
		public int yAngle { get; set; }
		public bool isPlaced { get; set; }
		public bool isVisible { get; set; }

		public ObjectInstance(string id, string configurationId, ObjectDefinition definition, Vector3 position, int yAngle, bool isPlaced)
		{
			this.id = id;
			this.configurationId = configurationId;
			this.definition = definition;
			this.position = position;
			this.yAngle = yAngle;
			this.isPlaced = isPlaced;
			this.isVisible = false;
		}

		public ObjectInstance(JSONValue record)
		{
			JSONObject recordObject = record.Obj;

			id = recordObject.GetString("Id");
			float x = (float)recordObject.GetNumber("x__c");
			float y = (float)recordObject.GetNumber("y__c");
			float z = (float)recordObject.GetNumber("z__c");
			position = new Vector3(x, y, z);
			yAngle = (int)recordObject.GetNumber("y_Angle__c");
			isPlaced = recordObject.GetBoolean("Is_Placed__c");

			// Get configuration id
			if (recordObject.ContainsKey("Configuration__c"))
				configurationId = recordObject.GetString("Configuration__c");
			else
			{
				JSONObject configurationObject = recordObject.GetObject("Configuration__r");
				configurationId = configurationObject.GetValue("Id").Str;
			}
			// Get object definition id
			string definitionId;
			if (recordObject.ContainsKey("Object_Definition__c"))
			{
				definitionId = recordObject.GetString("Object_Definition__c");
			}
			else
			{
				JSONObject definitionObject = recordObject.GetObject("Object_Definition__r");
				definitionId = definitionObject.GetValue("Id").Str;
			}
			// Load object definition from cache
			definition = CacheManager.getObjectDefinitions().get(definitionId);
			if (definition == null)
				throw new System.Exception("Could not find definition "+ definitionId + " of object instance "+ id +" in cache");
		}

		public JSONObject toJson()
		{
			// Ensure object definition is set
			if (definition == null)
				throw new System.Exception("Cannot serialize object instance " + id + " to JSON: missing definition");

			// Serialize object instance
			JSONObject instanceObject = new JSONObject();
			if (id != null)	// It could be a new object
				instanceObject.Add("Id", new JSONValue(id));
			instanceObject.Add("Configuration__c", configurationId);
			instanceObject.Add("Object_Definition__c", definition.id);
			instanceObject.Add("x__c", new JSONValue(position.x));
			instanceObject.Add("y__c", new JSONValue(position.y));
			instanceObject.Add("z__c", new JSONValue(position.z));
			instanceObject.Add("y_Angle__c", new JSONValue(yAngle));
			instanceObject.Add("Is_Placed__c", new JSONValue(isPlaced));
			return instanceObject;
		}

		public static string getSelectQueryBase()
		{
			return "SELECT Id, Configuration__r.Id, Object_Definition__r.Id, x__c, y__c, z__c, y_Angle__c, Is_Placed__c FROM Object_Instance__c";
		}

		/**
		 * Loads objects from a JSON array 
		 **/
		public static List<ObjectInstance> parseFromJsonArray(JSONArray array)
		{
			List<ObjectInstance> items = new List<ObjectInstance>();
			foreach (JSONValue item in array)
				items.Add(new ObjectInstance(item));
			return items;
		}

		public static JSONArray toJsonArray(List<ObjectInstance> instances)
		{
			JSONArray array = new JSONArray();
			foreach (ObjectInstance instance in instances)
				array.Add(instance.toJson());
			return array;
		}
	}
}
