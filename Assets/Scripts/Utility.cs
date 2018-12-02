using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
	public static T RandomEnumValue<T>()
	{
		var values = System.Enum.GetValues(typeof(T));
		int random = UnityEngine.Random.Range(0, values.Length);
		return (T)values.GetValue(random);
	}
}
