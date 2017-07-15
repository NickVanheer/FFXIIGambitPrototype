using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DamageNumber : MonoBehaviour {

    public float TimeToLive = 0.5f;
    private Text damageText;
    private string textToShow;

    void Start()
    {
        damageText = transform.GetChild(0).GetComponent<Text>();
        Debug.Assert(damageText != null);
    }

    void Update()
    {
        TimeToLive -= Time.deltaTime;
        updateFont();

        if (TimeToLive <= 0)
            GameObject.Destroy(this.gameObject);
    }

    void updateFont()
    {
        damageText.text = textToShow;
        transform.Translate(new Vector3(0, 0.01f, 0));
    }

    public void SetDamageText(string text, float timeToLive = 0.4f)
    {
        textToShow = text;
        TimeToLive = timeToLive;
    }
}
