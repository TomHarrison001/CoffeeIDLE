using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public double money;
    public double lifeMoney;
    public double power;
    public int baristas;
    public double upgrades;

    public double[] levels;
    public double[] multis;
    public double[] timers;

    public SaveData(PlayerController controller)
    {
        money = controller.Money;
        lifeMoney = controller.LifeMoney;
        power = controller.Power;
        baristas = controller.Baristas;
        upgrades = controller.Upgrades;

        levels = controller.Levels;
        multis = controller.Multis;
        timers = controller.Timers;
    }
}
