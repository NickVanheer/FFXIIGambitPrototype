using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public enum UnitWeakness
{
    None, Fire, Wind, Water, Electricity
}
public class Unit : MonoBehaviour
{
    [Header("Unit Level Properties")]
    public int level = 3;
    public int startExperience = 90;
    public int currentExperience = 90;
    public int nextLevelExperience = 160;

    [Header("Unit Core")]
    public bool IsGambitsEnabled = true;
    public string Name;
    public float Health;
    public float HealthPercentage;
    public float MaximumHealth;
    public float Mana;
    public float MaximumMana;

    [Header("Unit Properties")]
    public UnitWeakness Weakness;

    public int Strength; //physical offense power
    public int Magic; //magic offense power

    public int Skill; //hit-chance
    public int Speed; //
    public int Luck; //hit-crit rate

    public int Defense; //defense against physical
    public int Resistance; //defense against magic

    [Header("Other")]
    public bool IsLeader = false;
    public float StartDelay = 0.0f;
    public float startDelayCache = 0.0f;
    public bool IsReadyToAttack = false;

    [HideInInspector] //used from PartyVisualControl to build UI
    public float TimeToAttack = 0;

    [HideInInspector] //used from PartyVisualControl to build UI
    public float MaxTimeToAttack;

    [Header("Unit HUD")]
    public Image HealthBar;

    [Header("Unit Gambits")]
    public GambitRule CurrentValidatedRule;
    public List<GambitRule> GambitRules = new List<GambitRule>();

    public virtual void Start()
    {
        Health = MaximumHealth;
        Mana = MaximumMana;
        HealthPercentage = GetHealthPercentage();
        startDelayCache = StartDelay;
    }

    /*************** GAMBITS ***********/
    public void AddGambitRule(GambitRule rule)
    {
        if (GambitRules == null)
            GambitRules = new List<GambitRule>();

        GambitRules.Add(rule);
    }

    public void ValidateNextRuleFromGambits()
    {
        CurrentValidatedRule = null;

        foreach (var gambit in GambitRules)
        {
            //if it's enabled, the condition is met and we have enough MP
            if(gambit.IsEnabled && gambit.TargetCondition.IsConditionMet(this, gambit) && this.Mana >= gambit.GambitAction.MPCost)
            {
                CurrentValidatedRule = gambit;
                TimeToAttack = gambit.GambitAction.CastTime * Speed; //default speed modifier for player is 1
                MaxTimeToAttack = TimeToAttack;
                return;
            }
        }
    }

    public void GambitsChanged()
    {
        //triggers the system to go over the gambits again and pick a new rule.
        CurrentValidatedRule = null;
    }

    public void GambitLoop()
    {
        if (CurrentValidatedRule == null)
            ValidateNextRuleFromGambits();

        if (CurrentValidatedRule != null && CurrentValidatedRule.TargetCondition.ResultUnit != null)
        {
            StartDelay -= Time.deltaTime;

            if (StartDelay > 0.01)
                return;

            //Time to attack set in ValidateNextRuleFromGambits()
            TimeToAttack -= Time.deltaTime;

            if (TimeToAttack <= 0)
            {
                Debug.Log("Unit " + Name + " is using action: " + CurrentValidatedRule.GambitAction.Name);
                CurrentValidatedRule.GambitAction.DoAction(this, CurrentValidatedRule.TargetCondition.ResultUnit);
                ChangeMana(-CurrentValidatedRule.GambitAction.MPCost);

                CurrentValidatedRule = null;
                TimeToAttack = 0;
                StartDelay = startDelayCache;
            }
        }
        else
        {
            CurrentValidatedRule = null;
        }
    }

    /*************** HEALTH AND MANA ***********/
    public float GetHealthPercentage()
    {
        float v = Health / MaximumHealth;
        return v * 100;
    }

    private void UpdateHealthbar()
    {
        //change healthbar
        if (HealthBar != null)
        {
            HealthBar.fillAmount = (float)Health / (float)MaximumHealth;
        }
    }

    //increases or decreases health
    public void ChangeHealth(int value)
    {
        Health += value;

        UpdateHealthbar();
        HealthPercentage = GetHealthPercentage();

        Health = Mathf.Clamp(Health, 0, MaximumHealth);
    }

    //increases or decreases mana
    public void ChangeMana(int value)
    {
        Mana += value;
        Mana = Mathf.Clamp(Mana, 0, MaximumMana);
    }

    public void ChangeHealthRelative(int percentage)
    {
        float v = (float)percentage / (float)100;
        float fromMax = MaximumHealth * v;
        Health += fromMax;

        UpdateHealthbar();
        HealthPercentage = GetHealthPercentage();

        Health = Mathf.Clamp(Health, 0, MaximumHealth);
    }

    /*************** LEVEL AND EXPERIENCE ***********/
    public void ChangeLevel(int levelToBecome)
    {
        level = levelToBecome;
        startExperience = CalculateStartExperience(level); //lv1: 10
        currentExperience = CalculateStartExperience(level); //lv1: 10
        nextLevelExperience = CalculateStartExperience(level + 1); //lv2: 40

        //increase max HP
        int hpIncr = 8;
        int generalScaler = 1;
        //float res = BaseHealth + level * hpIncr + Mathf.Pow(level, 2) * 0.6f;
        float res = MaximumHealth + level * hpIncr + Mathf.Pow(level, 2) * 0.6f;

        MaximumHealth = (int)res * generalScaler;
    }

    public bool IncreaseExperience(int exp)
    {
        bool levelUp = false;
        currentExperience += exp;

        if (currentExperience > nextLevelExperience)
        {
            levelUp = true;
            //We can increase a level, increase exp too with remaining exp (we could potentially increase more than one level)
            IncreaseOneLevel();

            int remainder = currentExperience - nextLevelExperience;
            IncreaseExperience(remainder);
        }

        return levelUp;
    }

    public void IncreaseOneLevel()
    {
        ChangeLevel(level + 1);
    }

    private int CalculateStartExperience(int lv)
    {
        return (int)Mathf.Pow(lv, 2) * 10;
    }

    private void CalculateNextExperience()
    {
        nextLevelExperience = CalculateStartExperience(level + 1); //lv2: 40
    }

    public void Update()
    {
        if (GameManager.Instance.IsGameplayFrozen)
            return;

        if (Health <= 0)
        {
            HandleDeath();
            return;
        }

        HealthPercentage = GetHealthPercentage();

        if(IsGambitsEnabled)
            GambitLoop();
    }


    public void HandleDeath()
    {
        GameManager.Instance.SpawnDamageText("15 Exp.", this.gameObject.transform.position);
        GameManager.Instance.RemoveUnitFromList(this);
        GameObject.Destroy(this.gameObject);
    }
}
