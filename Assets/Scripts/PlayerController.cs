using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject particle, adBtn;
    [SerializeField] private TextMeshProUGUI moneyText, baristaText, baristaBtn, upgradeBtn, restartBtn, unitsBtn, rewardTimerText;
    [SerializeField] private List<TextMeshProUGUI> titleText, costText, prodText;
    [SerializeField] private List<Image> fillImage;

    private GameController controller;
    private float[] timers, timecaps, multis, powers = new float[] { 1.07f, 1.15f, 1.14f, 1.13f, 1.12f, 1.11f, 1.10f, 1.09f, 1.08f, 1.07f },
        baristaCosts = new float[] { 1000, 15000, 100000, 500000, 1200000, 10000000, 111111111, 555555555, 10000000000, 100000000000 },
        costs = new float[] { 3.738f, 60, 720, 8640, 103680, 1244160, 14929920, 179159040, 2149908480, 25798901760 },
        revenues = new float[] { 1, 60, 540, 4320, 51840, 622080, 7464960, 89579520, 1074954240, 29668737024 };
    private string[] names = new string[] { "Espresso", "Cappuccino", "Cortado", "Mocha", "Frappe", "Latte", "Americano", "Macchiato", "Flat White", "Lungo" };
    private int[] levels;
    private bool[] autos;
    private bool single, saving;
    private float money, lifeMoney, power, rewardTimer;
    private int baristas, upgrades, upgradeCost;
    private bool reward;
    private Vector2 mousePos;

    public float Money { get { return money; } }
    public float LifeMoney { get { return lifeMoney; } }
    public float Power { get { return power; } }
    public int Baristas { get { return baristas; } }
    public int Upgrades { get { return upgrades; } }
    public int UpgradeCost { get { return upgradeCost; } }
    public float[] Timers { get { return timers; } }
    public float[] Multis { get { return multis; } }
    public int[] Levels { get { return levels; } }
    public bool[] Autos { get { return autos; } }
    public bool Reward { get { return reward; } }

    private void Start()
    {
        controller = FindObjectOfType<GameController>();
        FullReset(true);
        LoadPlayer();
        baristaText.text = baristas.ToString();
        if (baristas < 10) {
            baristaBtn.gameObject.SetActive(true);
            baristaBtn.text = Notation(baristaCosts[baristas]);
        }
        else
            baristaBtn.gameObject.SetActive(false);
        unitsBtn.text = "X1";
        single = true;
        upgradeBtn.text = names[upgrades % 10] + "\n" + Notation(upgradeCost);
        reward = false;
        rewardTimer = 500;
    }

    private void FullReset(bool newGame)
    {
        money = 5;
        if (newGame) power = 1;
        lifeMoney = 5;
        baristas = 0;
        upgrades = 0;
        upgradeCost = 250000;
        timers = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        timecaps = new float[] { 1.2f, 6, 12, 24, 48, 96, 384, 1536, 6114, 36864 };
        multis = new float[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        levels = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        autos = new bool[] { false, false, false, false, false, false, false, false, false, false };
    }

    private void ProduceAll()
    {
        for (int i = 0; i < 10; i++)
        {
            Produce(i, names[i]);
        }
    }

    private void Produce(int i, string title)
    {
        titleText[i].text = title + " (" + levels[i] + ")";
        costText[i].text = Notation(costs[i] * (float)Math.Pow(powers[i], levels[i]));
        float revenue = revenues[i] * multis[i] * levels[i] * power;
        if (timecaps[i] / multis[i] < 0.076)
        {
            fillImage[i].fillAmount = 1;
            prodText[i].text = Notation(reward ? revenue * 2 / (timecaps[i] / multis[i]) : revenue / (timecaps[i] / multis[i])) + "/sec";
        }
        else
        {
            fillImage[i].fillAmount = timers[i] / timecaps[i];
            prodText[i].text = Notation(reward ? revenue * 2 : revenue);
        }
        if (levels[i] != 0)
        {
            if (timers[i] < timecaps[i]) timers[i] += Time.deltaTime * multis[i];
            else if (autos[i])
                Collect(i);
            else timers[i] = timecaps[i];
        }
    }

    public void Collect(int i)
    {
        if (timers[i] >= timecaps[i])
        {
            float revenue = revenues[i] * multis[i] * levels[i] * power;
            money += reward ? revenue * 2 : revenue;
            lifeMoney += reward ? revenue * 2 : revenue;
            timers[i] = 0;
            if (!autos[i])
            {
                //FindObjectOfType<AudioManager>().Play("ProduceSound");
                CreateObject(particle, new Vector2(mousePos.x, mousePos.y), true);
            }
        }
    }

    public void Buy(int i)
    {
        if (money >= costs[i] * Math.Pow(powers[i], levels[i]))
        {
            //FindObjectOfType<AudioManager>().Play("BuySound");
            CreateObject(particle, new Vector2(mousePos.x, mousePos.y), true);
            money -= costs[i] * (float)Math.Pow(powers[i], levels[i]);
            levels[i]++;
            if (levels[i] == 10)
                multis[i] *= 2;
            if (levels[i] == 25)
                multis[i] *= 2;
            else if (levels[i] % 50 == 0)
                multis[i] *= 2;
            if (!single) Buy(i);
        }
    }

    public void Automate()
    {
        if (money >= baristaCosts[baristas])
        {
            //FindObjectOfType<AudioManager>().Play("UpgradeSound");
            money -= baristaCosts[baristas];
            autos[baristas] = true;
            baristas++;
            baristaText.text = baristas.ToString();
            if (baristas < 10)
            {
                baristaBtn.gameObject.SetActive(true);
                baristaBtn.text = Notation(baristaCosts[baristas]);
            }
            else
                baristaBtn.gameObject.SetActive(false);
        }
    }

    public void Upgrade()
    {
        if (money >= upgradeCost)
        {
            //FindObjectOfType<AudioManager>().Play("UpgradeSound");
            money -= upgradeCost;
            multis[upgrades % 10] *= 3;
            upgradeCost *= 2;
            upgrades++;
            upgradeBtn.text = names[upgrades % 10] + "\n" + Notation(upgradeCost);
        }
    }

    public void Multiply()
    {
        single = !single;
        unitsBtn.text = single ? "X1" : "MAX";
    }

    public void Restart()
    {
        power += 0.1f * (float)Math.Pow(30, Math.Log10(lifeMoney) / 3 - 4);
        FullReset(false);
        if (baristas < 10)
        {
            baristaBtn.gameObject.SetActive(true);
            baristaBtn.text = Notation(baristaCosts[baristas]);
        }
        else
            baristaBtn.gameObject.SetActive(false);
        upgradeBtn.text = names[upgrades % 10] + "\n" + Notation(upgradeCost);
    }

    private string Notation(float x)
    {
        string result;
        if (x >= 1000)
        {
            int exponent = (int)Math.Floor(Math.Log10(Math.Abs(x)));
            int suffix = exponent / 3;
            var mantissa = x / Math.Pow(10, 3 * suffix);
            string suffixes;
            if (suffix < 5) suffixes = new List<string> { "", "K", "M", "B", "T" }[suffix];
            else
            {
                suffixes = new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" }[(suffix - 5) / 26];
                suffixes += new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" }[(suffix - 5) % 26];
            }
            string[] sigFigs = { "F2", "F1", "F0" };
            result = "$" + mantissa.ToString(sigFigs[exponent % 3]) + suffixes;
        }
        else
            result = "$" + x.ToString("F2");
        return result;
    }

    private IEnumerator SavePlayer()
    {
        saving = true;
        if (reward) power /= 2;
        PlayerPrefs.SetFloat("Money", money);
        PlayerPrefs.SetFloat("LifeMoney", lifeMoney);
        PlayerPrefs.SetFloat("Power", power);
        PlayerPrefs.SetInt("Baristas", baristas);
        PlayerPrefs.SetInt("Upgrades", upgrades);
        PlayerPrefs.SetInt("UpgradeCost", upgradeCost);
        for (int i = 0; i < 10; i++)
        {
            PlayerPrefs.SetInt($"Level{i + 1}", levels[i]);
            PlayerPrefs.SetFloat($"Multi{i + 1}", multis[i]);
            PlayerPrefs.SetInt($"Auto{i + 1}", (autos[i] ? 1 : 0));
            PlayerPrefs.SetFloat($"Timer{i + 1}", timers[i]);
        }
        if (reward) power *= 2;
        yield return new WaitForSeconds(10);
        saving = false;
    }

    private void LoadPlayer()
    {
        money = PlayerPrefs.GetFloat("Money", 5f);
        lifeMoney = PlayerPrefs.GetFloat("LifeMoney", 5f);
        power = PlayerPrefs.GetFloat("Power", 1f);
        baristas = PlayerPrefs.GetInt("Baristas", 0);
        upgrades = PlayerPrefs.GetInt("Upgrades", 0);
        upgradeCost = PlayerPrefs.GetInt("UpgradeCost", 250000);
        for (int i = 0; i < 10; i++)
        {
            levels[i] = PlayerPrefs.GetInt($"Level{i + 1}", 0);
            multis[i] = PlayerPrefs.GetFloat($"Multi{i + 1}", 0);
            autos[i] = PlayerPrefs.GetInt($"Auto{i + 1}", 0) == 1;
            timers[i] = PlayerPrefs.GetFloat($"Timer{i + 1}", 0);
        }
    }

    private void FixedUpdate()
    {
        moneyText.text = Notation(money);
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        restartBtn.text = "Reset +" + Notation(10 * (float)Math.Pow(30, Math.Log10(lifeMoney) / 3 - 4))[1..] + "%";
        ProduceAll();
        if (reward)
        {
            if (rewardTimer <= 300)
            {
                rewardTimer -= Time.deltaTime;
                rewardTimerText.text = (rewardTimer).ToString("F0") + "s";
                if (rewardTimer <= 0)
                {
                    reward = false;
                    rewardTimer = 500;
                    adBtn.SetActive(true);
                }
            }
            else
            {
                adBtn.SetActive(false);
                rewardTimer = 300;
            }
        }
        if (!saving) _ = StartCoroutine(SavePlayer());
    }

    public void ExitGame()
    {
        controller.LoadLevel(0);
    }


    private void CreateObject(GameObject o, Vector2 pos, bool particle)
    {
        GameObject g = Instantiate(o, pos, Quaternion.identity);
        g.transform.localScale = new Vector2(0.02f, 0.02f);
        StartCoroutine(DestroyParticles(g));
    }

    private IEnumerator DestroyParticles(GameObject p)
    {
        yield return new WaitForSeconds(0.3f);
        Destroy(p);
    }
}
