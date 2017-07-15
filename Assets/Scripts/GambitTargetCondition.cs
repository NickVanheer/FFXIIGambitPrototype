using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public enum GambitTargetType
{
    Player, Enemy, Disabled
}

/// <summary>
/// Base class for the "if" clause of the gambit. The condition which when met will result in a target for the player (see ResultUnit variable)
/// </summary>
public abstract class GambitTargetCondition
{
    public GambitTargetType TargetType;
    public string Name;
    public Unit ResultUnit;
    public abstract bool IsConditionMet(Unit instigator = null);

    public GambitTargetCondition(string name, GambitTargetType type)
    {
        this.Name = name;
        this.TargetType = type;
    }
}

/********* RULES ***************/

public class AllyHPBelow70 : GambitTargetCondition
{
    public AllyHPBelow70() : base("Ally: HP < 70%", GambitTargetType.Player) { }

    public override bool IsConditionMet(Unit instigator = null)
    {
        var result = GameManager.Instance.GetPartyMembers().Where(p => p.HealthPercentage <= 70);

        if (result.Count() > 0)
        {
            ResultUnit = result.First();
            return true;
        }
        return false;
    }
}

public class AllyHPBelowValuePercentage : GambitTargetCondition
{
    int percent;
    public AllyHPBelowValuePercentage(string displayName, int percentage) : base(displayName, GambitTargetType.Player)
    {
        this.percent = percentage;
    }

    public override bool IsConditionMet(Unit instigator = null)
    {
        var result = GameManager.Instance.GetPartyMembers().Where(p => p.HealthPercentage <= percent);

        if (result.Count() > 0)
        {
            ResultUnit = result.First();
            return true;
        }
        return false;
    }
}

public class TargetNearestVisible : GambitTargetCondition
{
    public TargetNearestVisible() : base("Target: Nearest Visible", GambitTargetType.Enemy) { }

    public override bool IsConditionMet(Unit instigator = null)
    {
        var result = GameManager.Instance.GetNearestUnitsInRangeBasedOnSourceTag(instigator);

        if (result != null && result.Count > 0)
        {
            ResultUnit = result.First();
            return true;
        }
        return false;
    }
}

public class TargetWeakToElement : GambitTargetCondition
{
    public UnitWeakness WeaknessToCheck;
    public TargetWeakToElement(string displayName, UnitWeakness weakness) : base(displayName, GambitTargetType.Enemy)
    {
        WeaknessToCheck = weakness;
    }

    public override bool IsConditionMet(Unit instigator = null)
    {
        var result = GameManager.Instance.GetNearestUnitsInRangeBasedOnSourceTag(instigator);

        if (result != null)
        {
            foreach (var unit in result)
            {
                if (unit.Weakness == WeaknessToCheck)
                {
                    ResultUnit = unit;
                    return true;
                }
            }
        }

        return false;
    }
}

public class TargetSelf : GambitTargetCondition
{
    public TargetSelf() : base("Target: Self", GambitTargetType.Player) { }

    public override bool IsConditionMet(Unit instigator = null)
    {
        if(instigator != null)
        {
            ResultUnit = instigator;
            return true;
        }
        else
        {
            return false;
        }
    }
}


public class TargetPartyLeaderTarget : GambitTargetCondition
{
    public TargetPartyLeaderTarget() : base("Target: Party Leader Target", GambitTargetType.Enemy) { }

    public override bool IsConditionMet(Unit instigator = null)
    {
        foreach (var unitGO in GameManager.Instance.CurrentPartyMembers)
        {
            Unit unit = unitGO.GetComponent<Unit>();
            if (unit.IsLeader && unit.CurrentValidatedRule != null)
            {
                ResultUnit = unit.CurrentValidatedRule.TargetCondition.ResultUnit;
                return true;
            }
        }
        return false;
    }
}

