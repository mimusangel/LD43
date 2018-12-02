using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerMove : MonoBehaviour {

	CharacterController cc;
	Animator			animator;
	public GameObject PlayerCamera;

	public float Gravity = -9.81f;
	private float _headRotateX = 0.0f;
	public float MoveSpeed = 4.0f;
	public float RotateSpeed = 3.0f;

	public GameObject MagicWand0;
	public GameObject MagicWand1;

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

	[Header("UI")]
	public Image Spell0Border;
	public Image Spell0Icon;
	public Image Spell1Border;
	public Image Spell1Icon;
	public Image Spell2Border;
	public Image Spell2Icon;
	int _spellFirstSelect = 0;
	int _spellSecondSelect = 0;
	public Text  SpellName;
	public Image PowerLoad;
	public Image LifeBar;
	public Text LifeText;
	public Text InfoText;
	public Image StaminaBar;
	public Text StaminaText;
	public Image ExpBar;
	public Text ExpText;

	[Header("UI Menu")]
	public GameObject PanelBonus;
	public GameObject PanelMenu;
	public Text BonusTitle;
	public Text PowerUpText;
	public Text PowerUpButtonText;
	public Text PowerRateText;
	public Text PowerRateButtonText;
	public Text LifeRecoveryText;
	public Text LifeRecoveryButtonText;
	public Text StaminaRecoveryText;
	public Text StaminaRecoveryButtonText;
	public GameObject PanelTopPlayer;
	public Text TopPlayerText;

	[Header("Player Stats")]
	Elemental	_elemental = Elemental.None;
	Elemental	_prepareSpell = Elemental.None;
	public float Life = 10.0f;
	public float Power = 10.0f;
	public float SpellRate = 2.0f;
	float _prepareSpellPower = 0.0f;
	int Exp = 0;
	int Level = 1;
	int KillNumber = 0;
	int Bonus = 0;
	float Stamina = 2.0f;
	float StaminaRate = 0.2f;
	float LifeRecoverie = 0.5f;
	bool StaminaRecovery = false;

	[Header("Player Effect")]
	Elemental	_effectElemental = Elemental.None;
	float		_effectValue;
	float		_effectTimer;
	public GameObject EffectFire;
	public GameObject EffectFreezeWater;
	public GameObject EffectMud;
	public GameObject EffectIce;


	void Start () {
		cc = GetComponent<CharacterController>();
		animator = GetComponent<Animator>();
		Spell0Border.color = Color.black;
		Spell0Icon.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
		Spell1Border.color = Color.black;
		Spell1Icon.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
		Spell2Border.color = Color.black;
		Spell2Icon.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
		_spellFirstSelect = 0;
		_spellSecondSelect = 0;
		SpellName.text = "None";

		Power = 10.0f;
		Life = Power;
		SpellRate = 2.0f;
		Exp = 0;
		Level = 1;
		KillNumber = 0;
		Bonus = 0;
		Stamina = Power * 0.2f;
		StaminaRate = 0.2f;
		LifeRecoverie = 0.5f;
		
		UpdateBonusUI();

		EffectFire.SetActive(false);
		EffectFreezeWater.SetActive(false);
		EffectMud.SetActive(false);
		EffectIce.SetActive(false);
	}

	void FireSpell()
	{
		if (MagicWandCall)
		{
			MagicWandCall.transform.SetParent(null);

			Rigidbody rg = MagicWandCall.AddComponent<Rigidbody>();
			rg.velocity = PlayerCamera.transform.forward * 20.0f;
			rg.useGravity = false;

			Destroy(MagicWandCall, 5.0f);
			PowerLoad.fillAmount = _prepareSpellPower;
			_prepareSpellPower = 0.0f;
			MagicWandCall = null;
			_prepareSpell = Elemental.None;
		}
	}
	void UpdateMouse() {
		if (Input.GetButton("Fire1") && Stamina > 0.0f)
		{ // Charge Attack
			animator.SetInteger("Action", 1);
			_prepareSpellPower += SpellRate * Time.deltaTime;
			_prepareSpellPower = Mathf.Min(_prepareSpellPower, Power);
			Stamina -= 0.25f * Time.deltaTime;
			Stamina = Mathf.Max(Stamina, 0.0f);
			if (MagicWandCall == null)
			{
				_prepareSpell = _elemental;
				if (_prepareSpell != Elemental.None)
				{
					MagicWandCall = Instantiate(GetPrefabMagicCall(), Vector3.zero, Quaternion.identity, MagicWand0.transform);
					MagicWandCall.transform.localPosition = Vector3.zero;
					MagicWandCall.transform.localRotation = Quaternion.identity;
					MagicWandCall.transform.localScale = Vector3.zero;
					Spell sp = MagicWandCall.GetComponent<Spell>();
					sp.source = this.gameObject;
					sp.elemental = _prepareSpell;
					Life -= GetAttackLifeCost();
					Life = Mathf.Max(Life, 0.0f);
				}
			}
			else
			{
				Spell sp = MagicWandCall.GetComponent<Spell>();
				sp.power =  _prepareSpellPower / Power;
				sp.powerDamage =  GetAttack();
				PowerLoad.fillAmount = _prepareSpellPower / Power;
			}
			StaminaRecovery = Stamina <= 0.0f;
			if (StaminaRecovery)
			{
				if (MagicWandCall)
				{
					FireSpell();
				}
				animator.SetInteger("Action", 0);
			}
		}
		else if (Input.GetButtonUp("Fire1") || (Stamina <= 0.0f && _prepareSpell != Elemental.None))
		{ // Release Attack
			FireSpell();
		}
		else if (Input.GetButton("Fire2"))
		{ // Defense
			animator.SetInteger("Action", 2);
			if (MagicWandCall == null)
			{
				
			}
			Stamina -= 1.0f * Time.deltaTime;
			Stamina = Mathf.Max(Stamina, 0.0f);
			StaminaRecovery = Stamina <= 0.0f;
			if (StaminaRecovery)
			{
				if (MagicWandCall)
				{
					Destroy(MagicWandCall);
					MagicWandCall = null;
				}
				animator.SetInteger("Action", 0);
			}
		}
		else if (Input.GetButtonUp("Fire2"))
		{ // Defense
			if (MagicWandCall)
			{
				Destroy(MagicWandCall);
				MagicWandCall = null;
			}
		}
		else
		{
			animator.SetInteger("Action", 0);
			Stamina += StaminaRate * Time.deltaTime;
			Stamina = Mathf.Min(Stamina, Power * 0.2f);
		}
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 move = new Vector3(0, Gravity, 0);

		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
		{
			Cursor.lockState =  CursorLockMode.None;
			Cursor.visible = true;
			PanelBonus.SetActive(true);
			PanelMenu.SetActive(true);
			PanelTopPlayer.SetActive(true);
		}
		if (Cursor.lockState == CursorLockMode.Locked)
		{
			transform.localRotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * RotateSpeed, Vector3.up);
			Vector3 m = transform.forward * Input.GetAxisRaw("Vertical") + transform.right * Input.GetAxisRaw("Horizontal");
			m.Normalize();
			float moveSpd = MoveSpeed;
			if (_effectElemental == Elemental.Water)
				moveSpd *= 0.75f;
			else if (_effectElemental == Elemental.Freeze)
				moveSpd *= 0.5f;
			else if (_effectElemental == Elemental.Ice)
				moveSpd *= 0.0f;
			move += (m * moveSpd * Time.deltaTime);
			if (m.sqrMagnitude > 0)
				Walk();
			else
				IDLE();
			_headRotateX = Mathf.Clamp(_headRotateX - Input.GetAxis("Mouse Y") * RotateSpeed, -52.0f, 52.0f);
			PlayerCamera.transform.localRotation = Quaternion.AngleAxis(_headRotateX, Vector3.right);
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				SelectSpell(1);
				_elemental = GetElemental();
				SpellName.text = _elemental.ToString();
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				SelectSpell(2);
				_elemental = GetElemental();
				SpellName.text = _elemental.ToString();
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				SelectSpell(3);
				_elemental = GetElemental();
				SpellName.text = _elemental.ToString();
			}
			if (StaminaRecovery)
			{
				Stamina += StaminaRate * Time.deltaTime;
				if (Stamina >= Power * 0.2f)
				{
					Stamina = Power * 0.2f;
					StaminaRecovery = false;
				}
			}
			else
				UpdateMouse();
			
		}
		else
			IDLE();
		cc.Move(move);	

		UpdateEffect();

		// UI
		Life = Mathf.Max(Life, 0.0f);
		LifeBar.fillAmount = Life / Power;
		LifeText.text = "Life: " + Mathf.RoundToInt(LifeBar.fillAmount * 100.0f) + "%";

		StaminaBar.fillAmount  = Stamina / (Power * 0.2f);
		StaminaText.text = "Stamina: " + Mathf.RoundToInt(StaminaBar.fillAmount * 100.0f) + "%";

		ExpBar.fillAmount  = (float)Exp / (float)(Level * 2.0f + (Level - 1.0f));
		ExpText.text = "XP: " + Mathf.RoundToInt(ExpBar.fillAmount * 100.0f) + "%";
		

		int score = KillNumber * 100 + (Level - 1) * 50 + Mathf.RoundToInt(Life) + Exp * 2;
		InfoText.text = KillNumber + " Sacrificed\n" + 0 + " Alive\n" + score + " Scores";
		
		if (Life <= 0)
		{ // Death You Lost!

		}
	}

	public void AddExp(int xp)
	{
		Exp += xp;
		if (Exp >= Level * 2 + (Level - 1))
		{
			Exp -= Level * 2 + (Level - 1);
			Level += 1;
			Bonus += 2;
			Life += Power * LifeRecoverie;
			Life = Mathf.Min(Life, Power);
			UpdateBonusUI();
		}
	}
	public void AddKill()
	{		
		KillNumber++;
	}

	public void AddBonusPowerUp()
	{
		if (Bonus <= 0)
			return ;
		Bonus -= 1;
		Power += 10.0f;
		Life += 10.0f;
		UpdateBonusUI();
	}
	public void AddBonusPowerRate()
	{
		if (Bonus <= 0)
			return ;
		Bonus -= 1;
		SpellRate += 0.5f;
		UpdateBonusUI();
	}
	public void AddBonusLifeRecovery()
	{
		if (Bonus <= 0)
			return ;
		Bonus -= 1;
		LifeRecoverie += (1.0f - LifeRecoverie) * 0.1f;
		UpdateBonusUI();
	}
	public void AddBonusStaminaRecovery()
	{
		if (Bonus <= 0)
			return ;
		Bonus -= 1;
		StaminaRate += StaminaRate * 0.1f;
		UpdateBonusUI();
	}

	public void UpdateBonusUI()
	{
		BonusTitle.text = "Bonus (" + Bonus + ")";
		PowerUpText.text = "Power Up: " + Power;
		PowerUpButtonText.text = "1 Bonus For 10 Power";
		PowerRateText.text = "Power Rate: " + SpellRate;
		PowerRateButtonText.text = "1 Bonus For 0.5 Rate";
		LifeRecoveryText.text = "Life Recovery: " + Mathf.RoundToInt(LifeRecoverie * 100.0f) + " %";
		LifeRecoveryButtonText.text = "1 Bonus For " + ((1.0f - LifeRecoverie) * 0.1f) + " Life Recovery";
		StaminaRecoveryText.text = "Stamina Recovery: " + Mathf.RoundToInt(StaminaRate * 100.0f) + " %"; 
		StaminaRecoveryButtonText.text = "1 Bonus For " + (StaminaRate * 0.1f) + " Stamina Recovery";

		TopPlayerText.text = "";
	}

	public float GetDefense()
	{
		return Power * 0.5f;
	}

	public float GetAttack()
	{
		float mul = 0.5f;
		float mul2 = 0.15f;
		if (_spellFirstSelect != 0 && _spellSecondSelect != 0)
		{
			mul = 0.75f;
			mul2 = 0.1f;
		}
		return (_prepareSpellPower * mul) + (Random.Range(-mul2, mul2) * _prepareSpellPower);
	}

	public bool TakeDamage(float dmg, Elemental dmgType)
	{
		if (animator.GetInteger("Action") == 2)
			dmg -= GetDefense();
		else
			dmg -= GetDefense() * 0.25f;
		if (dmg > 0.0f)
		{
			Life -= dmg;
		}
		return false;
	}

	public float GetAttackLifeCost()
	{
		float cost = 0.0f;
		if (_spellFirstSelect != 0)
			cost += 1.0f;
		if (_spellSecondSelect != 0)
			cost += 0.5f;
		return cost;
	}

	void SelectSpell(int id)
	{
		if (_spellFirstSelect == id)
		{
			SetGraphSpell(_spellFirstSelect, 0);
			_spellFirstSelect = _spellSecondSelect;
			SetGraphSpell(_spellFirstSelect, 2);
			_spellSecondSelect = 0;

		}
		else if(_spellSecondSelect == id)
		{
			SetGraphSpell(_spellSecondSelect, 0);
			_spellSecondSelect = 0;
		}
		else
		{
			if (_spellFirstSelect == 0)
			{
				_spellFirstSelect = id;
				SetGraphSpell(_spellFirstSelect, 2);
			}
			else if (_spellSecondSelect == 0)
			{
				_spellSecondSelect = id;
				SetGraphSpell(_spellSecondSelect, 1);
			}
		}
	}

	void SetGraphSpell(int id, int status)
	{
		if (id == 1)
		{
			Spell0Border.color = status > 0 
				? ((status == 2) 
					? (Color)(new Color32((byte)255, (byte)215, (byte)0, (byte)255))
					: (Color)(new Color32((byte)206, (byte)206, (byte)206, (byte)255)))
				: Color.black;
			Spell0Icon.color = status > 0 ? Color.white : new Color(1.0f, 1.0f, 1.0f, 0.5f);
		}
		if (id == 2)
		{
			Spell1Border.color = status > 0 
				? ((status == 2) 
					? (Color)(new Color32((byte)255, (byte)215, (byte)0, (byte)255))
					: (Color)(new Color32((byte)206, (byte)206, (byte)206, (byte)255)))
				: Color.black;
			Spell1Icon.color = status > 0 ? Color.white : new Color(1.0f, 1.0f, 1.0f, 0.5f);
		}
		if (id == 3)
		{
			Spell2Border.color = status > 0 
				? ((status == 2) 
					? (Color)(new Color32((byte)255, (byte)215, (byte)0, (byte)255))
					: (Color)(new Color32((byte)206, (byte)206, (byte)206, (byte)255)))
				: Color.black;
			Spell2Icon.color = status > 0 ? Color.white : new Color(1.0f, 1.0f, 1.0f, 0.5f);
		}
	}

	static int _Wind = 1;
	static int _Water = 2;
	static int _Earth = 3;
	public GameObject GetPrefabMagicCall()
	{
		if (_elemental == Elemental.Lighning)
			return LighningCall;
		if (_elemental == Elemental.Wood)
			return WoodCall;
		if (_elemental == Elemental.Fire)
			return FireCall;
		if (_elemental == Elemental.Freeze)
			return FreezeCall;
		if (_elemental == Elemental.Mud)
			return MudCall;
		if (_elemental == Elemental.Ice)
			return IceCall;
		if (_elemental == Elemental.Wind)
			return WindCall;
		if (_elemental == Elemental.Water)
			return WaterCall;
		if (_elemental == Elemental.Earth)
			return EarthCall;
		return null;
	}

	public Elemental GetElemental()
	{
		if (_spellFirstSelect == _Earth && _spellSecondSelect == _Wind)
			return Elemental.Lighning;
		if (_spellFirstSelect == _Earth && _spellSecondSelect == _Water)
			return Elemental.Wood;
		if (_spellFirstSelect == _Wind && _spellSecondSelect == _Earth)
			return Elemental.Fire;
		if (_spellFirstSelect == _Wind && _spellSecondSelect == _Water)
			return Elemental.Freeze;
		if (_spellFirstSelect == _Water && _spellSecondSelect == _Earth)
			return Elemental.Mud;
		if (_spellFirstSelect == _Water && _spellSecondSelect == _Wind)
			return Elemental.Ice;
		if (_spellFirstSelect == _Wind)
			return Elemental.Wind;
		if (_spellFirstSelect == _Water)
			return Elemental.Water;
		if (_spellFirstSelect == _Earth)
			return Elemental.Earth;
		return Elemental.None;
	}

	void Walk()
	{
		animator.SetBool("Walk", true);
		cc.center = new Vector3(0, 0.95f, 0);
	}

	void IDLE()
	{
		animator.SetBool("Walk", false);
		cc.center = new Vector3(0, 0.90f, 0);
	}

	public void MenuGameResume()
	{
		Cursor.lockState =  CursorLockMode.Locked;
		Cursor.visible = false;
		PanelBonus.SetActive(false);
		PanelMenu.SetActive(false);
		PanelTopPlayer.SetActive(false);
	}
	public void MenuGameReset()
	{
		SceneManager.LoadScene(0);
	}
	public void MenuGameExit()
	{
		Application.Quit();
	}

	public void Effect(Elemental _effEle, float _effVal, float _effTimer)
	{
		if (_effectElemental != Elemental.None)
			return ;
		_effectElemental = _effEle;
		_effectValue = _effVal;
		_effectTimer = _effTimer;
		if (_effectElemental == Elemental.Lighning)
			transform.localRotation = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), Vector3.up);
		
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
			Life = Mathf.Max(Life, 0.0f);
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
