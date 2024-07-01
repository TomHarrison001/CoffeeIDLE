using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private AudioManager audioManager;
    private GameController controller;

    [SerializeField] private GameObject particle, baristaBtn, adBtn;
    [SerializeField] private TextMeshProUGUI moneyText, baristaText, baristaBtnText, upgradeBtn, restartBtn, unitsBtn, rewardTimerText;
    [SerializeField] private List<TextMeshProUGUI> titleText, costText, prodText;
    [SerializeField] private List<Image> fillImage;

    private static double[] costs = new double[] { 3.738d, 60, 720, 8640, 103680, 1244160, 14929920, 179159040, 2149908480, 25798901760 };
    private static double[] powers = new double[] { 1.07d, 1.15d, 1.14d, 1.13d, 1.12d, 1.11d, 1.10d, 1.09d, 1.08d, 1.07d };
    private static double[] timecaps = new double[] { 1.2d, 6, 12, 24, 48, 96, 384, 1536, 6114, 36864 };
    private static long[] revenues = new long[] { 1, 60, 540, 4320, 51840, 622080, 7464960, 89579520, 1074954240, 29668737024 };
    private static long[] baristaCosts = new long[] { 1000, 15000, 100000, 500000, 1200000, 10000000, 111111111, 555555555, 10000000000, 100000000000 };
    private static string[] names = new string[] { "Espresso", "Cappuccino", "Cortado", "Mocha", "Frappe", "Latte", "Americano", "Macchiato", "Flat White", "Lungo" };

    private double[] timers, multis,  levels;
    private bool single, saving;
    private double money, lifeMoney, power, rewardTimer, upgrades;
    private int baristas;
    private bool reward;
    private Vector2 mousePos;

    public double Money { get { return money; } }
    public double LifeMoney { get { return lifeMoney; } }
    public double Power { get { return power; } }
    public double Upgrades { get { return upgrades; } }
    public int Baristas { get { return baristas; } }
    public double[] Timers { get { return timers; } }
    public double[] Multis { get { return multis; } }
    public double[] Levels { get { return levels; } }
    public bool Reward { set { reward = value; } }

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        controller = FindObjectOfType<GameController>();
        power = 1;
        Reset();
        if (!controller.DeleteSave) LoadPlayer();
        else
        {
            SavePlayer();
            controller.DeleteSave = false;
        }
        baristaText.text = baristas.ToString();
        if (baristas == 10)
            baristaBtn.gameObject.SetActive(false);
        else {
            baristaBtn.gameObject.SetActive(true);
            baristaBtnText.text = "$" + Notation(baristaCosts[baristas]);
        }
        unitsBtn.text = "X1";
        single = true;
        upgradeBtn.text = names[(int)Math.Floor(upgrades % 10)] + "\n" + "$" + Notation(250000 * Math.Pow(2, upgrades));
    }

    private void Reset()
    {
        money = 5;
        lifeMoney = 5;
        baristas = 0;
        upgrades = 0;
        timers = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        multis = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        levels = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        for (int i = 0; i < 10; i++) {
            titleText[i].text = names[i] + " (" + Notation(levels[i]) + ")";
            costText[i].text = "$" + Notation(costs[i] * Math.Pow(powers[i], levels[i]));
        }
        reward = false;
        rewardTimer = 500;
    }

    private void FixedUpdate()
    {
        moneyText.text = "$" + Notation(money);
        restartBtn.text = "Reset +" + Notation(10 * Math.Pow(30, Math.Log10(lifeMoney) / 3 - 4)) + "%";
        ProduceAll();
        if (reward)
        {
            if (rewardTimer == 500)
            {
                adBtn.SetActive(false);
                rewardTimer = 150;
            }
            rewardTimer -= Time.deltaTime;
            rewardTimerText.text = (rewardTimer).ToString("F0") + "s";
            if (rewardTimer <= 0)
            {
                reward = false;
                rewardTimer = 500;
                adBtn.SetActive(true);
            }
        }
        if (!saving) StartCoroutine(SavePlayer());
    }

    private void ProduceAll()
    {
        for (int i = 0; i < 10; i++)
        {
            Produce(i);
        }
    }

    private void Produce(int i)
    {
        double revenue = revenues[i] * Math.Pow(2, multis[i] - 1) * levels[i] * power;
        if (levels[i] > 0 && baristas > i && timecaps[i] / multis[i] < 0.2)
        {
            fillImage[i].fillAmount = 1;
            prodText[i].text = "$" + Notation((reward ? 2 : 1) * revenue / (timecaps[i] / multis[i])) + "/sec";
        }
        else
        {
            fillImage[i].fillAmount = Math.Min(1.0f, (float)(timers[i] / timecaps[i]));
            prodText[i].text = "$" + Notation((reward ? 2 : 1) * revenue);
        }
        if (levels[i] != 0)
        {
            if (timers[i] < timecaps[i] || baristas > i)
                timers[i] += Time.deltaTime * multis[i];
            else
                timers[i] = timecaps[i];
            if (timers[i] >= timecaps[i] && baristas > i) Collect(i);
        }
    }

    public void Collect(int i)
    {
        if (timers[i] >= timecaps[i])
        {
            if (baristas <= i)
            {
                CreateParticle();
                PlayButtonAudio();
            }
            double revenue = revenues[i] * Math.Pow(2, multis[i] - 1) * levels[i] * power;
            if (timecaps[i] / multis[i] < 0.2) revenue *= timers[i] / timecaps[i];
            money += (reward ? 2 : 1) * revenue;
            lifeMoney += (reward ? 2 : 1) * revenue;
            timers[i] = 0;
        }
    }

    public void Buy(int i)
    {
        if (money >= costs[i] * Math.Pow(powers[i], levels[i]))
        {
            CreateParticle();
            PlayButtonAudio();
            do
            {
                money -= costs[i] * Math.Pow(powers[i], levels[i]);
                levels[i]++;
                titleText[i].text = names[i] + " (" + Notation(levels[i]) + ")";
                costText[i].text = "$" + Notation(costs[i] * Math.Pow(powers[i], levels[i]));
                if (levels[i] == 10 ||
                    levels[i] == 25 ||
                    levels[i] % 50 == 0) multis[i]++;
            } while (!single && money >= costs[i] * Math.Pow(powers[i], levels[i]));
        }
    }

    public void Automate()
    {
        if (money >= baristaCosts[baristas])
        {
            CreateParticle();
            PlayButtonAudio();
            money -= baristaCosts[baristas];
            baristas++;
            baristaText.text = baristas.ToString();
            if (baristas < 10)
            {
                baristaBtn.gameObject.SetActive(true);
                baristaBtnText.text = "$" + Notation(baristaCosts[baristas]);
            }
            else
                baristaBtn.gameObject.SetActive(false);
        }
    }

    public void Upgrade()
    {
        if (money >= 250000 * Math.Pow(2, upgrades))
        {
            CreateParticle();
            PlayButtonAudio();
            money -= 250000 * Math.Pow(2, upgrades);
            multis[(int)Math.Floor(upgrades % 10)]++;
            upgrades++;
            upgradeBtn.text = names[(int)Math.Floor(upgrades % 10)] + "\n" + "$" + Notation(250000 * Math.Pow(2, upgrades));
        }
    }

    public void Multiply()
    {
        single = !single;
        unitsBtn.text = single ? "X1" : "MAX";
        PlayButtonAudio();
    }

    public void Restart()
    {
        CreateParticle();
        PlayButtonAudio();
        power += 0.1f * Math.Pow(30, Math.Log10(lifeMoney) / 3 - 4);
        Reset();
        baristaText.text = baristas.ToString();
        if (baristas < 10)
        {
            baristaBtn.gameObject.SetActive(true);
            baristaBtnText.text = "$" + Notation(baristaCosts[baristas]);
        }
        else
            baristaBtn.gameObject.SetActive(false);
        upgradeBtn.text = names[(int)Math.Floor(upgrades % 10)] + "\n" + "$" + Notation(250000 * Math.Pow(2, upgrades));
    }

    private string Notation(double x)
    {
        if (x >= 1000)
        {
            int exponent = (int)Math.Floor(Math.Log10(Math.Abs(x)));
            int suffix = exponent / 3;
            var mantissa = x / Math.Pow(10, 3 * suffix);
            string suffixes;
            if (suffix < 5) suffixes = new List<string> { "", "K", "M", "B", "T" }[suffix];
            else
            {
                suffixes = new List<string> { "", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" }[(suffix - 5) / 676];
                suffix -= (suffix - 5) / 676 * 676;
                suffixes += new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" }[(suffix - 5) / 26];
                suffixes += new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" }[(suffix - 5) % 26];
            }
            string[] sigFigs = { "F2", "F1", "F0" };
            return mantissa.ToString(sigFigs[exponent % 3]) + suffixes;
        }
        if (x >= 100) return x.ToString("F0");
        if (x >= 10) return x.ToString("F1");
        return x.ToString("F2");
    }

    private IEnumerator SavePlayer()
    {
        saving = true;
        yield return new WaitForSeconds(10);
        SaveSystem.SavePlayer(this);
        saving = false;
    }

    private void LoadPlayer()
    {
        SaveData save = SaveSystem.LoadPlayer();
        if (save == null) return;
        money = save.money;
        lifeMoney = save.lifeMoney;
        power = save.power;
        baristas = save.baristas;
        upgrades = save.upgrades;
        levels = save.levels;
        multis = save.multis;
        timers = save.timers;
        for (int i = 0; i < 10; i++)
        {
            titleText[i].text = names[i] + " (" + Notation(levels[i]) + ")";
            costText[i].text = "$" + Notation(costs[i] * Math.Pow(powers[i], levels[i]));
        }
    }

    public void ExitGame()
    {
        controller.LoadLevel(0);
    }

    private void CreateParticle()
    {
        GameObject g = Instantiate(particle, mousePos, Quaternion.identity);
        g.transform.localScale = new Vector2(0.02f, 0.02f);
        StartCoroutine(DestroyParticles(g));
    }

    private IEnumerator DestroyParticles(GameObject p)
    {
        yield return new WaitForSeconds(0.3f);
        Destroy(p);
    }

    private void PlayButtonAudio()
    {
        audioManager.Play("select");
        if (controller.Vibrate) Vibration.Vibrate(30);
    }

    public void SetMousePos(Transform t)
    {
        mousePos = t.position;
    }
}
