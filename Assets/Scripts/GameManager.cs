using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// Core game manager containing references to party members, enemies and containing handy utility methods related to them. Called as a singleton from other classes.
/// </summary>
public class GameManager : MonoBehaviour {

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("Trying to access static instance while it has not been assigned yet");
                Debug.Break();
            }

            return instance;
        }
    }

    public List<GameObject> CurrentPartyMembers = new List<GameObject>();
    public List<GameObject> CurrentEnemies;

    public GameObject GambitPanelHolder;
    public GambitVisualController GambitController;

    public Text LogText;
    public float LogDisplayTime = 3f;

    [Header("ParticleSystem and Damage Prefabs")]
    public GameObject AttackParticleSystem;
    public GameObject WhiteMagickParticleSystem;
    public GameObject DamageTextPrefab;

    public bool IsGameplayFrozen = false;

    void Awake()
    {
        // First we check if there are any other instances conflicting
        if (instance != null && instance != this)
        {
            Debug.Log("There's already a Game Manager in the scene, destroying this one.");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start ()
    { 
        SetupParty();
        SetupEnemies();

        //Log
        if (LogText != null)
            LogText.gameObject.SetActive(false);

    }

    public void SetupParty()
    {
        /* //Do this in editor to ensure the order is right when there are multiple party members.
        var players = GameObject.FindGameObjectsWithTag("Player");
        CurrentPartyMembers = new List<GameObject>();

        foreach (var player in players)
        {
            CurrentPartyMembers.Add(player);
        }
        */

        //Setup Gambits for party members
        for (int i = 0; i < CurrentPartyMembers.Count; i++)
        {
            SetDefaultGambitsForPlayer(CurrentPartyMembers[i].GetComponent<Unit>(), i);
        }


        Debug.Log("Added " + CurrentPartyMembers.Count + " party members");
    }

    private void SetDefaultGambitsForEnemies()
    {
        GambitRule attackRule = new GambitRule();
        attackRule.TargetCondition = new TargetNearestVisible();
        attackRule.GambitAction = new AttackAction();
        attackRule.IsEnabled = true;

        foreach (var enemy in CurrentEnemies)
        {
            enemy.GetComponent<Unit>().GambitRules.Add(attackRule);
        }
    }

    private void SetDefaultGambitsForPlayer(Unit u, int index)
    {
        GambitRule attackRule = new GambitRule();
        attackRule.TargetCondition = new TargetNearestVisible();
        attackRule.GambitAction = new AttackAction();
        attackRule.IsEnabled = true;

        GambitRule attackPartyLeaderTargetRule = new GambitRule();
        attackPartyLeaderTargetRule.TargetCondition = new TargetPartyLeaderTarget();
        attackPartyLeaderTargetRule.GambitAction = new AttackAction();
        attackPartyLeaderTargetRule.IsEnabled = true;

        GambitRule cureRule = new GambitRule();
        cureRule.TargetCondition = new AllyHPBelowValuePercentage("Ally: HP <= 50%", 50);
        cureRule.GambitAction = new CureAction();
        cureRule.IsEnabled = true;

        GambitRule emptyRule = new GambitRule();
        GambitRule emptyRule2 = new GambitRule();

        if (index == 0)
        {
            u.GambitRules.Add(cureRule);
            u.GambitRules.Add(attackRule);
            u.IsLeader = true; //make this one the leader
        }

        if(index > 0)
        {
            u.GambitRules.Add(attackPartyLeaderTargetRule);
        }

        u.GambitRules.Add(emptyRule);
        u.GambitRules.Add(emptyRule2);
    }

    public void SetupEnemies()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        CurrentEnemies = new List<GameObject>();

        foreach (var enemy in enemies)
        {
            CurrentEnemies.Add(enemy);
        }

        SetDefaultGambitsForEnemies();

        Debug.Log("Added " + CurrentEnemies.Count + " enemies");
    }

    public void ToggleGambitsView()
    {
        bool isActive = GambitPanelHolder.gameObject.activeInHierarchy;
        GambitPanelHolder.SetActive(!isActive);

        IsGameplayFrozen = !isActive;

        if (!isActive)
            Time.timeScale = 0.0f;
        else
            Time.timeScale = 1.0f;
    }
	
	// Update is called once per frame
	void Update () {

        if(Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleGambitsView();
        }

        //update Log Display
        if(LogText.gameObject.activeInHierarchy)
        {
            LogDisplayTime -= Time.deltaTime;
            if (LogDisplayTime <= 0)
                LogText.gameObject.SetActive(false);
        }

    }

    public List<Unit> GetPartyMembers()
    {
        List<Unit> units = new List<Unit>();

        foreach (var pMember in CurrentPartyMembers)
        {
            units.Add(pMember.GetComponent<Unit>());
        }

        return units;
    }

    public List<Unit> GetEnemies()
    {
        List<Unit> units = new List<Unit>();

        foreach (var enemy in CurrentEnemies)
        {
            units.Add(enemy.GetComponent<Unit>());
        }

        return units;
    }

    public List<Unit> GetNearestUnitsInRangeBasedOnSourceTag(Unit source)
    {
        List<Unit> result = null;

        //when the player uses this action, target all enemies
        if(source.gameObject.tag == "Player")
            result = GetNearestInRange(source, CurrentEnemies, 10.0f);

        //when the enemy uses this action, target all players
        if (source.gameObject.tag == "Enemy")
            result = GetNearestInRange(source, CurrentPartyMembers, 10.0f);

        return result;
    }

    public List<Unit> GetNearestInRange(Unit source, List<GameObject> list, float range)
    {
        List<Unit> units = new List<Unit>();

        foreach (var unit in list)
        {
            //units.OrderBy(u => Vector3.Distance(source.transform.position, u.transform.position) < range).ToList();
            if (Vector3.Distance(source.transform.position, unit.transform.position) < range)
                units.Add(unit.GetComponent<Unit>());
        }

        if (units.Count > 0)
            return units;
        else
            return null;
    }

    public void RemoveUnitFromList(Unit u)
    {
        if(u.gameObject.tag == "Enemy")
        {
            CurrentEnemies.Remove(u.gameObject);
        }

        if (u.gameObject.tag == "Player")
        {
            CurrentPartyMembers.Remove(u.gameObject);
        }
    }

    public int ResolveDamageUnitAGivesUnitB(Unit Instigator, Unit receiver)
    {
        int weaponAttack = 12;
        //todo change by weapon attack
        float baseDamage = weaponAttack - receiver.Defense;

        if (baseDamage <= 1.0)
            baseDamage = 1.0f;

        //multiplier
        float lowerLimit = Instigator.Strength;
        float upperLimit = Instigator.Strength + (Instigator.Strength + Instigator.level) / 4;
        float multiplier = UnityEngine.Random.Range(lowerLimit, upperLimit) / 2;

        //level
        float levelTakenInto = Mathf.Pow(Instigator.level, 0.5f);
        float levelMultiplier = 1 + (levelTakenInto * 0.6f);

        //general scaler
        float generalScaler = 1.0f;

        float totalDamage = baseDamage * multiplier * levelMultiplier * generalScaler;

        return (int)totalDamage;
    }

    public void SpawnDamageText(string text, Vector3 position)
    {
        if (DamageTextPrefab != null)
        {
            float randomY = UnityEngine.Random.Range(0.8f, 1.8f);
            float randomZ = UnityEngine.Random.Range(-0.5f, 0.5f);
            Vector3 randomizedPosition = position + new Vector3(0, randomY, randomZ);

            GameObject dNumberObject = (GameObject)GameObject.Instantiate(DamageTextPrefab, randomizedPosition, Quaternion.identity);
            dNumberObject.GetComponent<DamageNumber>().SetDamageText(text);
        }
    }

    public void AddToCombatLog(string instigator, string action)
    {
        AddToCombatLog(instigator + " used " + action + ".", 2f);
    }

    public void AddToCombatLog(string text, float logDisplayTime)
    {
        if (LogText != null)
        {
            LogText.gameObject.SetActive(true);
            LogText.text = text;
            LogDisplayTime = logDisplayTime;
        }
    }

    public void SpawnActionEffect(EffectType effectType, Unit unit)
    {
        GameObject particleObject = null;

        switch (effectType)
        {
            case EffectType.BasicAttack:
                particleObject = AttackParticleSystem;
                break;
            case EffectType.WhiteMagick:
                particleObject = WhiteMagickParticleSystem;
                break;
            case EffectType.FireMagick: //TODO
                particleObject = AttackParticleSystem;
                break;
            default:
                break;
        }

        if (particleObject != null)
            GameObject.Instantiate(particleObject, unit.transform.position, Quaternion.identity);
    }

    public void AddExperienceToPartyMembers(int exp)
    {
        foreach (var pMember in CurrentPartyMembers)
        {
            pMember.GetComponent<Unit>().IncreaseExperience(exp);
        }
    }
}
