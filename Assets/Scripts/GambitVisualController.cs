using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Controls the on-screen display, displaying all available gambit rules for a unit and allowing to adjust them or switch them around.
/// </summary>
public class GambitVisualController : MonoBehaviour {

    public Unit UnitToShow;
    private int showIndex = 0;
    private int switchIndex = 0;
    private bool isSwitching = false;
    public Text UnitNameTextLabel;
    public GameObject GambitRuleLinePrefab;

    public Color TargetColor;
    public Color AllyColor;
    public Color DisabledColor;

    public GameObject AllTargetConditionsPanel;
    public GameObject AllActionsPanel;
    public GameObject AvailableOptionButtonPrefab;

    //
    private List<GambitTargetCondition> AllTargetConditions;
    private List<GambitAction> AllActions;
    private int GambitRuleIndexToModify;

    private bool IsOverlayWindowOpen = false;

    private bool isConstructed = false;

	void Start () {
        AllTargetConditions = new List<GambitTargetCondition>();
        AllTargetConditions.Add(new TargetNearestVisible());
        AllTargetConditions.Add(new TargetPartyLeaderTarget());
        AllTargetConditions.Add(new TargetSelf());
        AllTargetConditions.Add(new TargetAny());

        AllTargetConditions.Add(new TargetWeakToElement("Target: Weak to Fire", UnitWeakness.Fire));
        AllTargetConditions.Add(new TargetWeakToElement("Target: Weak to Electricity", UnitWeakness.Electricity));
        AllTargetConditions.Add(new TargetWeakToElement("Target: Weak to Water", UnitWeakness.Water));
        AllTargetConditions.Add(new TargetWeakToElement("Target: Weak to Wind", UnitWeakness.Wind));

        AllTargetConditions.Add(new AllyHPBelowValuePercentage("Ally: HP <= 70%", 70));
        AllTargetConditions.Add(new AllyHPBelowValuePercentage("Ally: HP <= 50%", 50));
        AllTargetConditions.Add(new AllyHPBelowValuePercentage("Ally: HP <= 20%", 20));
        //
        AllActions = new List<GambitAction>();
        AllActions.Add(new AttackAction());
        AllActions.Add(new StealAction());
        AllActions.Add(new CureAction());
        AllActions.Add(new FireAction());

        if (GameManager.Instance.CurrentPartyMembers.Count > showIndex)
            UnitToShow = GameManager.Instance.CurrentPartyMembers[showIndex].GetComponent<Unit>();
    }
	
	void Update () {
        if (!isConstructed)
        {
            if (UnitToShow != null)
            {
                if (UnitNameTextLabel != null)
                    UnitNameTextLabel.text = UnitToShow.Name + "'s Gambits";
                ConstructGambitRuleUI();
                ConstructAvailableTargetConditionsUI();
                ConstructAvailableActionsUI();
                isConstructed = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && IsOverlayWindowOpen)
            CloseFlyouts();
    }

    /*********** UI Construction functions *********/

    public void ConstructGambitRuleUI()
    {
        foreach (Transform item in this.transform)
        {
            GameObject.Destroy(item.gameObject);
        }

        int order = 0;
        foreach (var rule in UnitToShow.GambitRules)
        {
            GameObject gLine = GameObject.Instantiate(GambitRuleLinePrefab);
            gLine.transform.SetParent(this.transform);

            foreach (Transform t in gLine.transform)
            {
                if (t.name == "RuleOrder")
                    t.gameObject.GetComponent<Text>().text = order.ToString();

                if(t.name == "OnOffButton")
                {
                    int index = order;
                    
                    string txt = rule.IsEnabled ? "ON" : "OFF";

                    if (rule.TargetCondition != null && rule.GambitAction != null)
                        t.gameObject.GetComponentInChildren<Text>().text = txt;
                    else
                        t.gameObject.GetComponentInChildren<Text>().text = "";

                    //Add button click to toggle on and off if both a target and action are assigned.
                    if (rule.TargetCondition != null && rule.GambitAction != null)
                        t.GetComponent<Button>().onClick.AddListener(delegate () { ToggleOnOff(index); });

                    if(!rule.IsEnabled & rule.TargetCondition != null && rule.GambitAction != null)
                        t.gameObject.GetComponentInChildren<Text>().color = Color.grey;
                }

                if(t.name == "RuleTarget")
                {
                    int index = order;
                    t.gameObject.GetComponent<Button>().onClick.AddListener(delegate () { OpenAvailableTargetFlyout(index); });

                    if (rule.IsEnabled)
                    {
                        t.gameObject.GetComponentInChildren<Text>().text = rule.TargetCondition.Name;
                    }
                    else if(rule.TargetCondition != null)
                    {
                        t.gameObject.GetComponentInChildren<Text>().text = rule.TargetCondition.Name;
                    }
                    else
                    {
                        t.gameObject.GetComponentInChildren<Text>().text = " - ";
                    }

                    //set background color
                    if (rule.IsEnabled)
                        ChangeBackgroundBasedOnTargetType(rule.TargetCondition.TargetType, gLine);
                    else
                        ChangeBackgroundBasedOnTargetType(GambitTargetType.Disabled, gLine);
                }

                if (t.name == "RuleAction")
                {
                    int index = order;
                    t.gameObject.GetComponent<Button>().onClick.AddListener(delegate () { OpenAvailableActionsFlyout(index); });

                    if (rule.IsEnabled)
                    {
                        t.gameObject.GetComponentInChildren<Text>().text = rule.GambitAction.Name;
                    }
                    else if (rule.GambitAction != null)
                    {
                        t.gameObject.GetComponentInChildren<Text>().text = rule.GambitAction.Name;
                    }
                    else
                    {
                        t.gameObject.GetComponentInChildren<Text>().text = "-";
                    }
                }

                if (t.name == "SwitchButton")
                {
                    int index = order;
                    t.gameObject.GetComponent<Button>().onClick.AddListener(delegate () { SwitchGambits(index); });

                    //zoom the rule for nice effect because
                    if(isSwitching && switchIndex == index)
                        gLine.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                    else
                        gLine.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                }
              }
            order++;
        }
    }

    public void ConstructAvailableTargetConditionsUI()
    {
        foreach (Transform item in AllTargetConditionsPanel.transform)
        {
            GameObject.Destroy(item.gameObject);
        }

        int order = 0;
        foreach (var rule in AllTargetConditions)
        {
            GameObject targetButton = GameObject.Instantiate(AvailableOptionButtonPrefab);
            targetButton.transform.SetParent(AllTargetConditionsPanel.transform);
            targetButton.transform.GetChild(0).GetComponent<Text>().text = rule.Name;

            int index = order;
            targetButton.GetComponent<Button>().onClick.AddListener(delegate () { AssignCurrentTargetCondition(index); });
            order++;
        }
    }

    public void ConstructAvailableActionsUI()
    {
        foreach (Transform item in AllActionsPanel.transform)
        {
            GameObject.Destroy(item.gameObject);
        }

        int order = 0;
        foreach (var rule in AllActions)
        {
            GameObject actionButton = GameObject.Instantiate(AvailableOptionButtonPrefab);
            actionButton.transform.SetParent(AllActionsPanel.transform);
            actionButton.transform.GetChild(0).GetComponent<Text>().text = rule.Name;

            int index = order;
            actionButton.GetComponent<Button>().onClick.AddListener(delegate () { AssignCurrentAction(index); });
            order++;
        }
    }

    /************* EVENT LISTENERS **************/

    public void ToggleOnOff(int gambitIndex)
    {
        UnitToShow.GambitRules[gambitIndex].IsEnabled = !UnitToShow.GambitRules[gambitIndex].IsEnabled;
        UnitToShow.GambitsChanged();
        ConstructGambitRuleUI();
    }

    public void AssignCurrentTargetCondition(int allTargetIndex)
    {
        GambitTargetCondition option = AllTargetConditions[allTargetIndex];
        UnitToShow.GambitRules[GambitRuleIndexToModify].TargetCondition = option;
        UnitToShow.GambitsChanged();
        ConstructGambitRuleUI();
        
        //
        CloseFlyouts();
    }

    public void AssignCurrentAction(int allActionIndex)
    {
        GambitAction option = AllActions[allActionIndex];
        UnitToShow.GambitRules[GambitRuleIndexToModify].GambitAction = option;
        UnitToShow.GambitsChanged();
        ConstructGambitRuleUI();

        //
        CloseFlyouts();
    }

    public void OpenAvailableTargetFlyout(int index)
    {
        CloseFlyouts();
        AllTargetConditionsPanel.SetActive(true);
        GambitRuleIndexToModify = index;
        IsOverlayWindowOpen = true;
    }

    public void SwitchGambits(int index)
    {
        //we already clicked it before and thus selected a rule to switch
        if (isSwitching)
        {
            GambitRule back = UnitToShow.GambitRules[switchIndex];
            UnitToShow.GambitRules[switchIndex] = UnitToShow.GambitRules[index];
            UnitToShow.GambitRules[index] = back;

            UnitToShow.GambitsChanged();
            isSwitching = false;
            ConstructGambitRuleUI();
        }
        else //we clicked the switch button for the first time, mark this rule to be switched
        {
            switchIndex = index;
            isSwitching = true;
            ConstructGambitRuleUI();
        }
    }

    public void OpenAvailableActionsFlyout(int index)
    {
        CloseFlyouts();
        GambitRuleIndexToModify = index;
        AllActionsPanel.SetActive(true);
        IsOverlayWindowOpen = true;
    }

    public void CloseFlyouts()
    {
        AllTargetConditionsPanel.SetActive(false);
        AllActionsPanel.SetActive(false);
        IsOverlayWindowOpen = false;
    }

    public void NextPartyMember()
    {
        showIndex++;

        if(showIndex >= GameManager.Instance.CurrentPartyMembers.Count)
            showIndex = 0;

        UnitToShow = GameManager.Instance.CurrentPartyMembers[showIndex].GetComponent<Unit>();
        isConstructed = false;
    }

    public void PreviousPartyMember()
    {
        showIndex--;

        if (showIndex < 0)
            showIndex = GameManager.Instance.CurrentPartyMembers.Count - 1;

        UnitToShow = GameManager.Instance.CurrentPartyMembers[showIndex].GetComponent<Unit>();
        isConstructed = false;
    }

    public void ChangeBackgroundBasedOnTargetType(GambitTargetType targetType, GameObject gLine)
    {
        switch(targetType)
        {
            case GambitTargetType.Player:
                gLine.GetComponent<Image>().color = AllyColor;
                break;
            case GambitTargetType.Enemy:
                gLine.GetComponent<Image>().color = TargetColor;
                break;
            case GambitTargetType.Disabled:
                gLine.GetComponent<Image>().color = DisabledColor;
                break;
        } 
    }
}
