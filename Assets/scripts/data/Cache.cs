using Boomlagoon.JSON;
using System.Collections.Generic;

namespace SFDC.wef.data
{
	public abstract class DataWithId
	{
		public string id { get; set; }
	}
	
	public class Cache<T> where T : DataWithId
	{
		private string cacheName;
		private Dictionary<string, T> cache;

		public Cache(string cacheName)
		{
			this.cacheName = cacheName;
			clear();
		}

		public void clear()
		{
			cache = new Dictionary<string, T>();
		}

		public void put(List<T> items)
		{
			foreach (T item in items)
				put(item);
		}

		public void put(T item)
		{
			cache.Add(item.id, item);
		}

		public T get(string id)
		{
			T item;
			if (cache.TryGetValue(id, out item))
				return item;
			throw new KeyNotFoundException("Could not retrieve object "+ id +" from cache "+ cacheName);
		}

		public void remove(string id)
		{
			cache.Remove(id);
		}

		public List<T> getAll()
		{
			return new List<T>(cache.Values);
		}
	}
}

