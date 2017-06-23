using UnityEngine;

public static class Vector3Extension
{
	public static string getAsString(this Vector3 v)
	{
		return "("+ v.x +", "+ v.y +", "+ v.z +")";
	}
}
