using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    private CanvasGroup sceneFade;
    private float musicVolume, sfxVolume;
    private bool vibrate = true;
    public float MusicVolume { get { return musicVolume; } set { musicVolume = value; SaveData(); } }
    public float SfxVolume { get { return sfxVolume; } set { sfxVolume = value; SaveData(); } }
    public bool Vibrate { get { return vibrate; } set { vibrate = value; SaveData(); } }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        LoadData();
        StartCoroutine(SceneFadeOut());
    }

    private void LoadData()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);
        vibrate = PlayerPrefs.GetFloat("Vibrate", 1f) == 1f;
    }

    private void SaveData()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
        PlayerPrefs.SetFloat("Vibrate", vibrate ? 1f : 0f);
        PlayerPrefs.Save();
    }

    private IEnumerator SceneFadeIn()
    {
        sceneFade = FindObjectOfType<CanvasGroup>();
        sceneFade.gameObject.SetActive(true);
        sceneFade.alpha = 0f;
        while (sceneFade.alpha < 1f)
        {
            if (Time.deltaTime * 2f <= 1f - sceneFade.alpha)
                sceneFade.alpha += Time.deltaTime * 2f;
            else
                sceneFade.alpha = 1f;
            yield return null;
        }
    }

    private IEnumerator SceneFadeOut()
    {
        sceneFade = FindObjectOfType<CanvasGroup>();
        sceneFade.gameObject.SetActive(true);
        sceneFade.alpha = 1f;
        while (sceneFade.alpha > 0f)
        {
            if (Time.deltaTime * 2f <= sceneFade.alpha)
                sceneFade.alpha -= Time.deltaTime * 2f;
            else
                sceneFade.alpha = 0f;
            yield return null;
        }
    }

    public IEnumerator Quit()
    {
        Debug.Log("Quitting...");
        StartCoroutine(SceneFadeIn());
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }

    public void LoadLevel(int index)
    {
        StartCoroutine(AsyncLoadLevel(index));
    }

    private IEnumerator AsyncLoadLevel(int index)
    {
        StartCoroutine(SceneFadeIn());
        yield return new WaitForSeconds(0.5f);
        var asyncLoadLevel = SceneManager.LoadSceneAsync(index);
        while (!asyncLoadLevel.isDone)
        {
            yield return null;
        }
        StartCoroutine(SceneFadeOut());
    }
}
