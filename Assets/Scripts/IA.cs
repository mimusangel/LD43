using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IA : MonoBehaviour {
	public static List<IA> IAList = new List<IA>();

	// CharacterController cc;
	Animator			animator;
	NavMeshAgent		agent;
	// Rigidbody			rgb;

	public float Gravity = -9.81f;
	public float MoveSpeed = 3.0f;
	public GameObject MagicWand;
	public GameObject MagicWandCall;

	[Header("Magie Effect")]
	public GameObject EarthCall;
	public GameObject WindCall;
	public GameObject WaterCall;
	public GameObject LighningCall;
	public GameObject WoodCall;
	public GameObject FireCall;
	public GameObject FreezeCall;
	public GameObject MudCall;
	public GameObject IceCall;
	[Header("IA Stats")]
	public Elemental Elemental = Elemental.Earth;
	public float Life = 10.0f;
	public float Power = 10.0f;
	public float SpellRate = 2.0f;
	public float _spellLoad = 0.0f;


	GameObject focusObject = null;

	
	[Header("Effect")]
	Elemental	_effectElemental = Elemental.None;
	float		_effectValue;
	float		_effectTimer;
	GameObject	_effectSource;
	public GameObject EffectFire;
	public GameObject EffectFreezeWater;
	public GameObject EffectMud;
	public GameObject EffectIce;

	public float MovingTimer = 0.0f;
	

	void Start () {
		// cc = GetComponent<CharacterController>();
		agent = GetComponent<NavMeshAgent>();
		// agent.updatePosition = false;
		// agent.updateRotation = false;
		animator = GetComponent<Animator>();
		// rgb = GetComponent<Rigidbody>();

		Vector3 pos = transform.position;
		pos.y = Terrain.activeTerrain.SampleHeight(pos);
		transform.position = pos;

		float distToPlayer = Vector3.Distance(transform.position, PlayerMove.Instance.transform.position);
		Debug.Log (distToPlayer);
		Power = (distToPlayer / 4.5f);
		Power += Random.Range(-2.5f, 2.5f);
		if (Power < 4.0f)
			Power = 4.0f;
		Life = Power;
		SpellRate += Random.Range(-0.5f, 0.5f);
		do {
			Elemental = Utility.RandomEnumValue<Elemental>();
        } while(Elemental == Elemental.None);
		
		EffectFire.SetActive(false);
		EffectFreezeWater.SetActive(false);
		EffectMud.SetActive(false);
		EffectIce.SetActive(false);
		IAList.Add(this);
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.timeScale <= 0)
			return ;
		float moveSpd = MoveSpeed;
		if (_effectElemental == Elemental.Water)
			moveSpd *= 0.75f;
		else if (_effectElemental == Elemental.Freeze)
			moveSpd *= 0.5f;
		else if (_effectElemental == Elemental.Ice)
			moveSpd *= 0.0f;
		if (transform.position.y <= 54.0f)
			moveSpd *= 0.75f;
		agent.speed = moveSpd;

		MovingTimer = Mathf.Max(MovingTimer - Time.deltaTime, 0.0f);
		if (focusObject)
		{
			Debug.Log (agent.speed);
			Debug.Log (agent.hasPath);
			// Vector3 pos = focusObject.transform.position;
			// pos.y = Terrain.activeTerrain.SampleHeight(pos);
			agent.SetDestination(focusObject.transform.position);
			transform.localRotation = Quaternion.LookRotation((focusObject.transform.position - transform.position).normalized, Vector3.up);
		}
		else if (!focusObject && MovingTimer <= 0.0f)
		{
			Vector3 rndPos = transform.position + new Vector3(Random.Range(-50.0f, 50.0f), 0.0f, Random.Range(-50.0f, 50.0f));
			rndPos.y = Terrain.activeTerrain.SampleHeight(rndPos);
			agent.ResetPath();
			agent.SetDestination(rndPos);
			MovingTimer = 15.0f;
		}
		// if (agent.hasPath)
		// {
		// 	Vector3 dirTarget = (transform.position + agent.velocity).normalized;
		// 	transform.localRotation = Quaternion.LookRotation(dirTarget.normalized, Vector3.up);
		// }
		animator.SetBool("Walk", (agent.velocity.sqrMagnitude > 0));
		
		// rgb.velocity = agent.velocity + new Vector3(0, rgb.velocity.y, 0);
		// transform.position = agent.nextPosition;

		
		if (focusObject)
		{
			float dist = Vector3.Distance(transform.position, focusObject.transform.position);
			if (dist < 10.0f)
			{
				// if (focusObject)
				animator.SetInteger("Action", 1);
				if (!MagicWandCall)
				{
					MagicWandCall = Instantiate(GetPrefabMagicCall(), Vector3.zero, Quaternion.identity, MagicWand.transform);
					MagicWandCall.transform.localPosition = Vector3.zero;
					MagicWandCall.transform.localRotation = Quaternion.identity;
					MagicWandCall.transform.localScale = Vector3.zero;
					MagicWandCall.GetComponent<SphereCollider>().radius = 0.5f;
					Spell sp = MagicWandCall.GetComponent<Spell>();
					sp.source = this.gameObject;
					sp.elemental = Elemental;
				}
			}
		}

		UpdateEffect();

		if (MagicWandCall)
		{
			_spellLoad += SpellRate * Time.deltaTime;
			Spell sp = MagicWandCall.GetComponent<Spell>();
			if (_spellLoad > Power)
			{
				sp.power =  1.0f;
				sp.powerDamage =  GetAttack();

				MagicWandCall.transform.SetParent(null);

				Rigidbody rg = MagicWandCall.AddComponent<Rigidbody>();
				if (focusObject)
					rg.velocity = (focusObject.transform.position - transform.position).normalized * 20.0f;
				else
					rg.velocity = transform.forward * 20.0f;
				rg.useGravity = false;
				rg.collisionDetectionMode = CollisionDetectionMode.Continuous;

				Destroy(MagicWandCall, 5.0f);

				animator.SetInteger("Action", 0);
				MagicWandCall = null;
				_spellLoad = 0.0f;
				GameObject prefab = Resources.Load<GameObject>("FireSpell");
				if (prefab)
				{
					GameObject sound = Instantiate(prefab, rg.transform.position, Quaternion.identity);
					Destroy(sound, 5.0f);
				}
			}
			else
			{
				sp.power =  _spellLoad / Power;
				sp.powerDamage =  _spellLoad;
			}
		}
	}
	public float GetDefense()
	{
		return Power * 0.25f;
	}

	public float GetAttack()
	{
		return (_spellLoad * 0.5f) + (Random.Range(-0.15f, 0.15f) * _spellLoad);
	}

	
	public bool TakeDamage(float dmg, Elemental dmgType, GameObject src)
	{
		dmg -= GetDefense();
		if (!focusObject)
		{
			focusObject = src;
			agent.ResetPath();
		}
		GameObject prefab = Resources.Load<GameObject>("Hit");
		if (prefab)
		{
			GameObject sound = Instantiate(prefab, transform.position + Vector3.up, Quaternion.identity);
			Destroy(sound, 5.0f);
		}
		if (dmg > 0.0f)
		{
			Life -= dmg;
			Debug.Log(gameObject.name + " " + Life);
			if (Life <= 0)
			{
				Destroy(gameObject);
				return true;
			}
		}
		return false;
	}

	public GameObject GetPrefabMagicCall()
	{
		if (Elemental == Elemental.Lighning)
			return LighningCall;
		if (Elemental == Elemental.Wood)
			return WoodCall;
		if (Elemental == Elemental.Fire)
			return FireCall;
		if (Elemental == Elemental.Freeze)
			return FreezeCall;
		if (Elemental == Elemental.Mud)
			return MudCall;
		if (Elemental == Elemental.Ice)
			return IceCall;
		if (Elemental == Elemental.Wind)
			return WindCall;
		if (Elemental == Elemental.Water)
			return WaterCall;
		if (Elemental == Elemental.Earth)
			return EarthCall;
		return null;
	}

	bool InView(Vector3 position)
	{
		position.y = 0.0f;
		Vector3 thisPos = transform.position;
		thisPos.y = 0.0f;
		Vector3 dir = (position - thisPos).normalized;
		if (Vector3.Dot(transform.forward, dir) > 0.52f)
		{
			// RaycastHit hit;
			// float dist = (position - transform.position).magnitude;
			
			// // if (Physics.Raycast(transform.position, dir, out hit, dist))
			// // {
			// // 	return (hit.distance >= dist - 0.5f);
			// // }
			// RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up * 1.0f + dir * 0.3f, dir, dist);
			// // Debug.Log(hits.Length);
			// foreach (RaycastHit hit in hits)
			// {
			// 	// Debug.Log(hit.distance + " // " + (dist - 0.5f));
			// 	// Debug.Log(hit.collider.gameObject.name);
			// 	if ((hit.distance < dist - 0.5f))
			// 		return false;
			// }
			return true;
		}
		return false; // 0.35 = 58.5
	}

	// Vector3 testTemp = Vector3.zero;

	void OnTriggerEnter(Collider other)
	{
		OnTriggerStay(other);
	}
	void OnTriggerStay(Collider other)
	{
		if (focusObject)
			return ;
		if (other.gameObject.tag == "Player")
		{
			// testTemp = other.gameObject.transform.position;
			if (InView(other.gameObject.transform.position))
			{
				focusObject = other.gameObject;
				agent.ResetPath();
			}
		}
	}

	void OnDrawGizmosSelected()
	{
		for(int d = 0; d < 360; d += 10)
		{
			float c = Mathf.Cos(Mathf.Deg2Rad * (float)d);
			float s = Mathf.Sin(Mathf.Deg2Rad * d);
			Vector3 dir = new Vector3(c, 0.0f, s);
			if (Vector3.Dot(transform.forward, dir) > 0.52f)
				Gizmos.color = Color.green;
			else
				Gizmos.color = Color.red;
			Vector3 pos = transform.position + Vector3.up * 1.5f;
			Gizmos.DrawLine(pos, pos + dir * 5.0f);
		}
		// if (testTemp != Vector3.zero)
		// {
		// 	Gizmos.color = Color.blue;
		// 	Gizmos.DrawLine(
		// 		transform.position + Vector3.up * 1.0f,
		// 		testTemp+ Vector3.up * 1.0f
		// 	);
		// }
	}

	void OnTriggerExit(Collider other)
	{
		if (other.gameObject == focusObject)
		{
			focusObject = null;
			agent.ResetPath();
		}
	}

	public void Effect(Elemental _effEle, float _effVal, float _effTimer, GameObject src)
	{
		if (_effectElemental != Elemental.None)
			return ;
		_effectElemental = _effEle;
		_effectValue = _effVal;
		_effectTimer = _effTimer;
		_effectSource = src;

		EffectFire.SetActive(_effectElemental == Elemental.Fire);
		EffectFreezeWater.SetActive(_effectElemental == Elemental.Freeze || _effectElemental == Elemental.Water);
		EffectMud.SetActive(_effectElemental == Elemental.Mud);
		EffectIce.SetActive(_effectElemental == Elemental.Ice);
	}

	public void UpdateEffect()
	{
		if (_effectElemental != Elemental.None)
		{
			if (_effectElemental == Elemental.Fire || _effectElemental == Elemental.Mud)
				Life -= _effectValue * Time.deltaTime;
			_effectTimer = Mathf.Max(_effectTimer - Time.deltaTime, 0.0f);
			if (_effectTimer <= 0.0f)
			{
				_effectElemental = Elemental.None;
				EffectFire.SetActive(false);
				EffectFreezeWater.SetActive(false);
				EffectMud.SetActive(false);
				EffectIce.SetActive(false);
			}
			if (Life <= 0)
			{
				PlayerMove pm = _effectSource.GetComponent<PlayerMove>();
				if (pm)
				{
					pm.AddExp(1);
					pm.AddKill();
				}
				Destroy(gameObject);
			}
		}
	}
	void OnDestroy()
	{
		IAList.Remove(this);
	}
}
