using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Controls and updates the party HUD. Redrawn each frame
/// </summary>
public class PartyVisualController : MonoBehaviour {

    public GameObject UnitBarUIPrefab;
    private bool isConstructed = false;

    // Update is called once per frame
    void Update()
    {
        if (!isConstructed && UnitBarUIPrefab != null)
        {
            ConstructUI();
            isConstructed = false; //continuous loop
        }
    }

    private void UpdateActionBar(Unit unit, GameObject actionBarPanel)
    {
        if (unit.CurrentValidatedRule == null)
            actionBarPanel.SetActive(false);
        else
            actionBarPanel.SetActive(true);

        GameObject ActionBar = actionBarPanel.transform.GetChild(0).GetChild(0).gameObject;
        Image actionBarImage = ActionBar.GetComponent<Image>();
        //change action bar
        if (actionBarImage != null)
        {
            actionBarImage.fillAmount = 1.0f - (float)unit.TimeToAttack / (float)unit.MaxTimeToAttack;
        }

        Text actionText = actionBarPanel.transform.GetChild(1).gameObject.GetComponent<Text>();

        if (actionText != null && unit.CurrentValidatedRule != null)
            actionText.text = unit.CurrentValidatedRule.GambitAction.Name;
    }

    public void ConstructUI()
    {
        foreach (Transform item in this.transform)
        {
            GameObject.Destroy(item.gameObject);
        }

        foreach (var unitGO in GameManager.Instance.CurrentPartyMembers)
        {
            if(unitGO != null)
            {
                Unit unit = unitGO.GetComponent<Unit>();

                GameObject line = GameObject.Instantiate(UnitBarUIPrefab);
                line.transform.SetParent(this.transform);

                foreach (Transform t in line.transform)
                {
                    if (t.name == "ActionPanel")
                        UpdateActionBar(unit, t.gameObject);

                    if (t.name == "Name")
                        t.gameObject.GetComponent<Text>().text = unit.Name;

                    if (t.name == "hpVal")
                        t.gameObject.GetComponent<Text>().text = unit.Health.ToString();

                    if (t.name == "hpMax")
                        t.gameObject.GetComponent<Text>().text = unit.MaximumHealth.ToString();

                    if (t.name == "mpVal")
                        t.gameObject.GetComponent<Text>().text = unit.Mana.ToString();
                }
             }
        }
    }
}
