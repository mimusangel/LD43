using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour {

	public float power = 0.0f;
	public float powerDamage = 0.0f;
	public Elemental elemental;
	ParticleSystem _ps;
	ParticleSystem.MainModule _main;
	public GameObject	source;
	// Use this for initialization
	void Start () {
		_ps = GetComponent<ParticleSystem>();
		_main = _ps.main;
		// _main.startSizeMultiplier = 0;
		_ps.Play();
	}
	
	// Update is called once per frame
	void Update () {
		_main.startSizeMultiplier = power;
	}

	void OnCollisionEnter(Collision other)
	{
		if (other.gameObject == source)
			return ;
		if (other.gameObject.tag == "Player")
		{ // Hit Player
			PlayerMove pm = other.gameObject.GetComponent<PlayerMove>();
			pm.TakeDamage(PlayerEffect(pm, elemental, powerDamage), elemental);
		}
		if (other.gameObject.tag == "Entity")
		{ // Hit Entity
			IA ia = other.gameObject.GetComponent<IA>();
			if (ia.TakeDamage(EntityEffect(ia, elemental, powerDamage), elemental))
			{
				PlayerMove pm = source.GetComponent<PlayerMove>();
				if (pm)
				{
					pm.AddExp(1);
					pm.AddKill();
					if (elemental == Elemental.Wood)
						pm.Life = Mathf.Min(pm.Life + pm.Power * 0.01f, pm.Power);
				}
			}
		}
		Destroy(gameObject);
	}

	float PlayerEffect(PlayerMove playerMove, Elemental elemental, float dmg)
	{
		if (elemental == Elemental.Lighning)
		{
			// Rotate View
			return dmg;
		}
		if (elemental == Elemental.Wood)
		{
			return (dmg * 0.5f);
		}
		if (elemental == Elemental.Fire)
		{
			playerMove.Effect(elemental, dmg * 0.75f, 2.5f);
			return (dmg * 0.75f);
		}
		if (elemental == Elemental.Freeze)
		{
			playerMove.Effect(elemental, 0.0f, 5f);
			return dmg * 0.75f;
		}
		if (elemental == Elemental.Mud)
		{
			playerMove.Effect(elemental, dmg * 0.5f, 5f);
			return (dmg * 0.5f);
		}
		if (elemental == Elemental.Ice)
		{
			playerMove.Effect(elemental, 0.0f, 2f);
			return dmg * 0.5f;
		}
		if (elemental == Elemental.Wind)
		{
			 // Knockback
			return dmg;
		}
		if (elemental == Elemental.Water)
		{
			playerMove.Effect(elemental, 0.0f, 2f);
			return dmg;
		}
		if (elemental == Elemental.Earth)
		{
			return dmg * 1.15f;
		}
		return dmg;
	}
	float EntityEffect(IA ia, Elemental elemental, float dmg)
	{
		if (elemental == Elemental.Lighning)
		{
			return dmg;
		}
		if (elemental == Elemental.Wood)
		{
			return (dmg * 0.5f);
		}
		if (elemental == Elemental.Fire)
		{
			ia.Effect(elemental, dmg * 0.75f, 2.5f);
			return (dmg * 0.75f);
		}
		if (elemental == Elemental.Freeze)
		{
			ia.Effect(elemental, 0.0f, 5f);
			return dmg * 0.75f;
		}
		if (elemental == Elemental.Mud)
		{
			ia.Effect(elemental, dmg * 0.5f, 5f);
			return (dmg * 0.5f);
		}
		if (elemental == Elemental.Ice)
		{
			ia.Effect(elemental, 0.0f, 2f);
			return dmg * 0.5f;
		}
		if (elemental == Elemental.Wind)
		{
			ia.GetComponent<Rigidbody>().AddForce((GetComponent<Rigidbody>().velocity + Vector3.up).normalized * 500.0f);
			 // Knockback
			return dmg;
		}
		if (elemental == Elemental.Water)
		{
			ia.Effect(elemental, 0.0f, 2f);
			return dmg;
		}
		if (elemental == Elemental.Earth)
		{
			return dmg * 1.15f;
		}
		return dmg;
	}
}
