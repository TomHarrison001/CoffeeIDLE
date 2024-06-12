using UnityEngine;

public class ActivateMenu : MonoBehaviour
{
    public bool enable;

    private void Start()
    {
        if (enable) EnableMenu();
    }

    public void EnableMenu()
    {
        GetComponent<Animator>().ResetTrigger("Disable");
        GetComponent<Animator>().SetTrigger("Enable");
    }

    public void DisableMenu()
    {
        GetComponent<Animator>().ResetTrigger("Enable");
        GetComponent<Animator>().SetTrigger("Disable");
    }
}
