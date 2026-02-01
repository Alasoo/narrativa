using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SliderGame : Game
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image background;
    [SerializeField] private Image areaToStop;
    [Space]
    [SerializeField] private Button stopButton;
    [Space]
    public float velocidad = 20f; // Variable para controlar la velocidad


    private float currentSpeed;
    private int currentGame;
    private int totalGames;

    private bool stoped = false;



    void Start()
    {
        slider.minValue = 0;
        slider.maxValue = background.rectTransform.rect.width;
        currentSpeed = velocidad;
        stopButton.onClick.AddListener(() => StopSlider());
        //StartGame(5);
        gameObject.SetActive(false);
    }


    void OnEnable()
    {
        StartCoroutine(MoveHandleSlider());
        StartGame(4);
    }

    void OnDisable()
    {
        TurnOff();
    }


    public void StartGame(int totalGames, int current = -1)
    {
        this.totalGames = totalGames;
        if (current == -1)
            currentGame = totalGames;

        float multi = (float)totalGames / (float)currentGame;
        currentSpeed = (float)velocidad * (float)multi;

        stoped = false;
        gameObject.SetActive(true);

        areaToStop.rectTransform.sizeDelta = new Vector2(Mathf.Max(areaToStop.rectTransform.sizeDelta.x / 2f, slider.handleRect.sizeDelta.x), areaToStop.rectTransform.sizeDelta.y);
        SetRandomPosition();
    }

    private void SetRandomPosition()
    {
        float min = -((background.rectTransform.sizeDelta.x / 2f) - (areaToStop.rectTransform.sizeDelta.x / 2f));
        float max = (background.rectTransform.sizeDelta.x / 2f) - (areaToStop.rectTransform.sizeDelta.x / 2f);
        float x = Random.Range(min, max);
        areaToStop.rectTransform.anchoredPosition = new Vector2(x, areaToStop.rectTransform.anchoredPosition.y);
    }


    protected override void EndGame()
    {
        gameObject.SetActive(false);
        areaToStop.rectTransform.sizeDelta = background.rectTransform.sizeDelta;
        base.EndGame();
    }

    private void StopSlider()
    {
        if (stoped) return;
        stoped = true;

        float bgWidth = background.rectTransform.rect.width;
        float centerOffset = bgWidth / 2f;

        float areaCenterInSliderSpace = areaToStop.rectTransform.anchoredPosition.x + centerOffset;

        float halfAreaWidth = areaToStop.rectTransform.rect.width / 2f;
        float minWin = areaCenterInSliderSpace - halfAreaWidth;
        float maxWin = areaCenterInSliderSpace + halfAreaWidth;

        bool isWin = slider.value >= minWin && slider.value <= maxWin;

        Debug.Log($"isWin: {isWin}");

        if (!isWin)
            Reset();
        else
        {
            Check();
        }
    }


    private void Reset()
    {
        areaToStop.rectTransform.sizeDelta = background.rectTransform.sizeDelta;
        currentSpeed = velocidad;
        StartGame(totalGames);
    }

    private void TurnOff()
    {
        areaToStop.rectTransform.sizeDelta = background.rectTransform.sizeDelta;
        currentSpeed = velocidad;
        gameObject.SetActive(false);
    }


    private void Check()
    {
        currentGame--;

        if (currentGame > 0)
            StartGame(totalGames, currentGame);
        else
            EndGame();
    }


    IEnumerator MoveHandleSlider()
    {
        float valorActual = 0f;
        while (true)
        {
            while (valorActual < slider.maxValue)
            {
                valorActual += currentSpeed * Time.deltaTime;
                if (valorActual > slider.maxValue) valorActual = slider.maxValue;
                slider.value = valorActual;
                yield return null;
            }

            while (valorActual > slider.minValue)
            {
                valorActual -= currentSpeed * Time.deltaTime;
                if (valorActual < slider.minValue) valorActual = slider.minValue;
                slider.value = valorActual;

                yield return null;
            }
        }
    }


}
