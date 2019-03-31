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

    public abstract bool IsConditionMet(Unit instigator, GambitRule currentRule);

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

    public override bool IsConditionMet(Unit instigator, GambitRule currentRule)
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

    public override bool IsConditionMet(Unit instigator, GambitRule currentRule)
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

public class TargetIsHealthAt : GambitTargetCondition
{
    int percent;
    public TargetIsHealthAt(string displayName, int percentage) : base(displayName, GambitTargetType.Enemy)
    {
        this.percent = percentage;
    }

    public override bool IsConditionMet(Unit instigator, GambitRule currentRule)
    {
        foreach (var unitGO in GameManager.Instance.CurrentPartyMembers)
        {
            Unit unit = unitGO.GetComponent<Unit>();
            if (unit.IsLeader && unit.CurrentValidatedRule != null && unit.CurrentValidatedRule.TargetCondition.TargetType == GambitTargetType.Enemy)
            {
                if(unit.CurrentValidatedRule.TargetCondition.ResultUnit.HealthPercentage == percent)
                {
                    ResultUnit = unit.CurrentValidatedRule.TargetCondition.ResultUnit;
                    return true;
                }
            }
        }
        return false;
    }
}


public class TargetNearestVisible : GambitTargetCondition
{
    public TargetNearestVisible() : base("Target: Nearest Visible", GambitTargetType.Enemy) { }

    public override bool IsConditionMet(Unit instigator, GambitRule currentRule)
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
    private UnitWeakness weaknessToCheck;
    public TargetWeakToElement(string displayName, UnitWeakness weakness) : base(displayName, GambitTargetType.Enemy)
    {
        weaknessToCheck = weakness;
    }

    public override bool IsConditionMet(Unit instigator, GambitRule currentRule)
    {
        var result = GameManager.Instance.GetNearestUnitsInRangeBasedOnSourceTag(instigator);

        if (result != null)
        {
            foreach (var unit in result)
            {
                if (unit.Weakness == weaknessToCheck)
                {
                    ResultUnit = unit;
                    return true;
                }
            }
        }

        return false;
    }
}

/// <summary>
/// Target any ally and make sure it isn't targeted a second time for that action
/// </summary>
public class TargetAny : GambitTargetCondition
{
    public TargetAny() : base("Target: Any", GambitTargetType.Player) { }

    private int currentIndex = 0;
    private GambitAction currentAction = null;

    public override bool IsConditionMet(Unit instigator, GambitRule currentRule)
    {
        int maxPartySize = GameManager.Instance.CurrentPartyMembers.Count;
        if (currentRule.GambitAction == currentAction && currentIndex < maxPartySize)
        {
            //same action, assign a target, if needed continuing where we left off the last time when this rule was used.
            ResultUnit = GameManager.Instance.GetPartyMembers()[currentIndex];
            currentIndex++;
            return true;
        }
        else if(currentAction != currentRule.GambitAction)
        {
            //different action (probably a buff or heal) so we save the action and start counting from the first party member in the list again
            currentAction = currentRule.GambitAction;
            currentIndex = 0;
            ResultUnit = GameManager.Instance.GetPartyMembers()[currentIndex];
            currentIndex++;
            return true;
        }

        return false;
    }
}

public class TargetSelf : GambitTargetCondition
{
    public TargetSelf() : base("Target: Self", GambitTargetType.Player) { }

    public override bool IsConditionMet(Unit instigator, GambitRule currentRule)
    {
        ResultUnit = instigator;
        return true;
    }
}


public class TargetPartyLeaderTarget : GambitTargetCondition
{
    public TargetPartyLeaderTarget() : base("Target: Party Leader Target", GambitTargetType.Enemy) { }

    public override bool IsConditionMet(Unit instigator, GambitRule currentRule)
    {
        foreach (var unitGO in GameManager.Instance.CurrentPartyMembers)
        {
            Unit unit = unitGO.GetComponent<Unit>();
            if (unit.IsLeader && unit.CurrentValidatedRule != null && unit.CurrentValidatedRule.TargetCondition.TargetType == GambitTargetType.Enemy)
            {
                ResultUnit = unit.CurrentValidatedRule.TargetCondition.ResultUnit;
                return true;
            }
        }
        return false;
    }
}

