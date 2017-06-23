using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;
using System.Collections.Generic;

namespace SFDC.wef.data
{
	public class Configuration : DataWithId
	{
		public string name { get; set; }
		public string briefName { get; set; }
		public int availableSpace { get; set; }
		public string climate { get; set; }

		public Configuration(string id, string name, string briefName, string climate)
		{
			this.id = id;
			this.name = name;
			this.briefName = briefName;
			this.availableSpace = 0;
			this.climate = "Cold";
		}

		public Configuration(JSONValue record)
		{
			JSONObject recordObject = record.Obj;

			id = recordObject.GetString("Id");
			name = recordObject.GetString("Name");
			// Get brief info
			JSONObject briefObject = recordObject.GetObject("Brief__r");
			briefName = briefObject.GetString("Name");
			availableSpace = (int) briefObject.GetNumber("Available_Space__c");
			climate = briefObject.GetString("Climate__c");
		}

		public static string getSelectQueryBase()
		{
			return "SELECT Id, Name, Brief__r.Name, Brief__r.Available_Space__c, Brief__r.Climate__c FROM Configuration__c";
		}

		/**
		 * Loads objects from a JSON array 
		 **/
		public static List<Configuration> parseFromJsonArray(JSONArray array)
		{
			List<Configuration> items = new List<Configuration>();
			foreach (JSONValue item in array)
				items.Add(new Configuration(item));
			return items;
		}
	}
}
