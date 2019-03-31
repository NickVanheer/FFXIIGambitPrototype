using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Represents an action that can be performed, along with its name, cast time and mp cost. Used in Unit and GambitRule classes.
/// </summary>
public abstract class GambitAction {

    public string Name;
    public float CastTime;
    public int MPCost = 0;

    public abstract void DoAction(Unit source, Unit target);

    public GambitAction(string name, float castTime)
    {
        this.Name = name;
        this.CastTime = castTime;
    }

    public GambitAction(string name, float castTime, int mpCost)
    {
        this.Name = name;
        this.CastTime = castTime;
        this.MPCost = mpCost;
    }
}

public class AttackAction : GambitAction
{
    public AttackAction() : base("Attack", 2.3f) { }

    public override void DoAction(Unit source, Unit target)
    {
        //float value = 0.3f * Mathf.Pow(source.Strength, 2.0f);
        float val = GameManager.Instance.ResolveDamageUnitAGivesUnitB(source, target);
        target.ChangeHealth(-(int)val);
        GameManager.Instance.SpawnActionEffect(EffectType.BasicAttack, target);

        GameManager.Instance.SpawnDamageText(((int)val).ToString(), target.transform.position);
    }
}

public class StealAction : GambitAction
{
    public StealAction() : base("Steal", 3.0f) { }

    public override void DoAction(Unit source, Unit target)
    {
        //steal gil from enemy
        GameManager.Instance.SpawnDamageText("Stole: Potion", target.transform.position);
        GameManager.Instance.SpawnActionEffect(EffectType.BasicAttack, target);
    }
}

public class CureAction : GambitAction
{
    public CureAction() : base("Cure", 4, 8) { }

    public override void DoAction(Unit source, Unit target)
    {
        int value = 40;
        target.ChangeHealthRelative(value);
        GameManager.Instance.SpawnActionEffect(EffectType.WhiteMagick, target);
        GameManager.Instance.SpawnDamageText(value.ToString(), target.transform.position);

        GameManager.Instance.AddToCombatLog(source.Name, Name);
    }
}

public class FireAction : GambitAction
{
    public FireAction() : base("Fire", 4, 6) { }

    public override void DoAction(Unit source, Unit target)
    {
        float value = 0.3f * Mathf.Pow(source.Magic, 2.0f);

        if (target.Weakness == UnitWeakness.Fire)
        {
            value = 130; //decimate it
            GameManager.Instance.SpawnDamageText("WEAK", target.transform.position);
        }

        target.ChangeHealth(-(int)value);

        GameManager.Instance.SpawnActionEffect(EffectType.FireMagick, target);
        GameManager.Instance.SpawnDamageText(value.ToString(), target.transform.position);

        GameManager.Instance.AddToCombatLog(source.Name, Name);
    }
}

public enum EffectType
{
    BasicAttack, WhiteMagick, FireMagick
}