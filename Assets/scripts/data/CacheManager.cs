using System;

namespace SFDC.wef.data
{
	abstract class CacheManager
	{
		private static Cache<ObjectDefinition> objectDefinitions = new Cache<ObjectDefinition>("object definitions");
		private static Cache<ObjectInstance> objectInstances = new Cache<ObjectInstance>("object instances");

		public static Cache<ObjectDefinition> getObjectDefinitions()
		{
			return objectDefinitions;
		}

		public static Cache<ObjectInstance> getObjectInstances()
		{
			return objectInstances;
		}
	}
}

