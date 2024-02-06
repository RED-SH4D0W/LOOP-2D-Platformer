using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public UI_Main ui;
    public Player player;
    public bool colorEntirePlatform;

    [Header("Skybox Materials")]
    [SerializeField] private Material[] skyBoxMat;

    [Header("Color Info")]
    public Color platformColor;
    public Color playerColor = Color.white;

    [Header("Score Info")]
    public int coins;
    public float distance;

    public void Awake()
    {
        instance = this;
        Time.timeScale = 1;
        LoadColor();
        LoadColorPlatform();

        SetupSkyBox(PlayerPrefs.GetInt("SkyBoxSetting"));
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }

    public void SetupSkyBox(int i)
    {
        if(i <= 1)
            RenderSettings.skybox = skyBoxMat[i];
        else
            RenderSettings.skybox = skyBoxMat[Random.Range(0, skyBoxMat.Length)];

        PlayerPrefs.SetInt("SkyBoxSetting", i);
    }

    public void SaveColor(float r, float g, float b)
    {
        PlayerPrefs.SetFloat("ColorR", r);
        PlayerPrefs.SetFloat("ColorG", g);
        PlayerPrefs.SetFloat("ColorB", b);
    }

    private void LoadColor()
    {
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();

        Color newColor = new Color  (PlayerPrefs.GetFloat("ColorR",1),
                                    PlayerPrefs.GetFloat("ColorG",1),
                                    PlayerPrefs.GetFloat("ColorB",1),
                                    PlayerPrefs.GetFloat("ColorA", 1));

        sr.color = newColor;
    }

    public void SaveColorPlatform(float rP, float gP, float bP)
    {
        PlayerPrefs.SetFloat("ColorRa", rP);
        PlayerPrefs.SetFloat("ColorGa", gP);
        PlayerPrefs.SetFloat("ColorBa", bP);
    }

    private void LoadColorPlatform()
    {
        Color newPlatformColor = new Color(PlayerPrefs.GetFloat("ColorRa",1),
                                    PlayerPrefs.GetFloat("ColorGa",1),
                                    PlayerPrefs.GetFloat("ColorBa",1),
                                    PlayerPrefs.GetFloat("ColorA", 1));

        platformColor = newPlatformColor;
    }

    private void Update()
    {
        if(player.transform.position.x > distance)
            distance = player.transform.position.x;
    }

    public void UnlockPlayer() => player.playerUnlocked = true;

    public void RestartLevel()
    {

        SceneManager.LoadScene(0);

    }

    public void SaveInfo()
    {
        int savedCoins = PlayerPrefs.GetInt("Coins");
        PlayerPrefs.SetInt("Coins", savedCoins + coins);

        PlayerPrefs.SetFloat("LastScore", distance);

        if(PlayerPrefs.GetFloat("HighScore") < distance)
            PlayerPrefs.SetFloat("HighScore", distance);
    }

    public void GameEnded()
    {
        SaveInfo();
        ui.OpenEndGameUI();
    }
}
