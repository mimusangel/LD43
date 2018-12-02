using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IA : MonoBehaviour {

	// CharacterController cc;
	
	Animator			animator;
	NavMeshAgent		agent;

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


	PlayerMove focusPlayer = null;

	
	[Header("Effect")]
	Elemental	_effectElemental = Elemental.None;
	float		_effectValue;
	float		_effectTimer;
	public GameObject EffectFire;
	public GameObject EffectFreezeWater;
	public GameObject EffectMud;
	public GameObject EffectIce;
	

	void Start () {
		// cc = GetComponent<CharacterController>();
		agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();
		Power += Random.Range(-2.5f, 2.5f);
		Life = Power;
		SpellRate += Random.Range(-0.5f, 0.5f);
		
		EffectFire.SetActive(false);
		EffectFreezeWater.SetActive(false);
		EffectMud.SetActive(false);
		EffectIce.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		float moveSpd = MoveSpeed;
		if (_effectElemental == Elemental.Water)
			moveSpd *= 0.75f;
		else if (_effectElemental == Elemental.Freeze)
			moveSpd *= 0.5f;
		else if (_effectElemental == Elemental.Ice)
			moveSpd *= 0.0f;
		agent.speed = moveSpd;

		if (focusPlayer)
		{
			agent.SetDestination(focusPlayer.transform.position);
		}
		if (focusPlayer)
			transform.localRotation = Quaternion.LookRotation((focusPlayer.transform.position - transform.position).normalized, Vector3.up);
		animator.SetBool("Walk", (agent.velocity.sqrMagnitude > 0));

		float dist = Vector3.Distance(transform.position, agent.destination);
		if (dist < 10.0f && focusPlayer)
		{
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
				if (focusPlayer)
					rg.velocity = (focusPlayer.transform.position - transform.position).normalized * 20.0f;
				else
					rg.velocity = transform.forward * 20.0f;
				rg.useGravity = false;

				Destroy(MagicWandCall, 5.0f);

				animator.SetInteger("Action", 0);
				MagicWandCall = null;
				_spellLoad = 0.0f;
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

	
	public bool TakeDamage(float dmg, Elemental dmgType)
	{
		dmg -= GetDefense();
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
	
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
			focusPlayer = other.gameObject.GetComponent<PlayerMove>();
	}
	void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			focusPlayer = null;
		}
	}

	public void Effect(Elemental _effEle, float _effVal, float _effTimer)
	{
		if (_effectElemental != Elemental.None)
			return ;
		_effectElemental = _effEle;
		_effectValue = _effVal;
		_effectTimer = _effTimer;
		
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
		}
	}
}
