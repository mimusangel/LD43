using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public List<GameObject> NPCS = new List<GameObject>();

	// Use this for initialization
	void Start () {
		// 120 --- 904
		int count = 1000;
		while (count > 0)
		{
			// Debug.Log("Spawn : " + count);
			Vector3 pos = new Vector3(Random.Range(120.0f, 904.0f), 80.0f, Random.Range(120.0f, 904.0f));
			pos.y = Terrain.activeTerrain.SampleHeight(pos);
			if (!Physics.CheckSphere(pos + Vector3.up * 0.3f, 0.25f))
			{
				GameObject.Instantiate(NPCS[Random.Range(0, NPCS.Count)], pos, Quaternion.identity);
			}
			count--;
		}
	}
	static float _MaxDistance = 150.0f * 150.0f;
	void Update()
	{
		Vector3 pm = PlayerMove.Instance.transform.position;
		float dist;
		foreach (IA item in IA.IAList)
		{
			dist = Vector3.SqrMagnitude(item.transform.position - pm);
			item.gameObject.SetActive(dist < _MaxDistance);
		}
	}
}
