using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }

    public UIManager uIManager;
    public ScoreManager scoreManager;

    [Header("Game settings")]
    public GameObject player;
    public GameObject gameOverlay;
    [Space(5)]
    public GameObject smasherPrefab;
    [Space(5)]
    public int minNumberOfSmashers = 5;
    public int maxNumberOfSmashers = 10;//can be higher value, but then the player ball needs to be smaller (depends on screen width)
    [Space(5)]
    public List<GameObject> upSmashers, bottomSmashers;
    [Space(5)]
    public float moveDistance; //distance between upper and bottom smashers when they are max opened
    [Space(5)]
    public float smasherMoveSpeed;
    public bool canMove;

    public List<float> smashersStartPositions, smashersTargetPositions;
    GameObject lastSmasher;
    Vector3 screenSize;
    int indexShorterSmasher, currentPosition, numOfSmashers;
    bool position; //false - down, true - up

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Physics2D.gravity = new Vector2(0, 0f);

        Application.targetFrameRate = 30;

        screenSize = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        StartCoroutine(CreateScene());
    }

    void Update()
    {
        if (uIManager.gameState == GameState.PLAYING && Input.GetMouseButtonDown(0) && canMove)
        {
            if (uIManager.IsButton())
                return;

            if (Input.mousePosition.x > Screen.width * 0.5f)
            {
                if (currentPosition < numOfSmashers)
                {
                    currentPosition++;
                    AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
                }
            }
            else
            {
                if (currentPosition > 0)
                {
                    currentPosition--;
                    AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
                }
            }

            if (currentPosition >= numOfSmashers)
                currentPosition = numOfSmashers - 1;

            canMove = false;
            player.transform.position = new Vector2(bottomSmashers[currentPosition].transform.position.x, bottomSmashers[currentPosition].transform.position.y + bottomSmashers[currentPosition].transform.localScale.y / 2 + player.GetComponent<SpriteRenderer>().size.y / 2);
        }

        if (uIManager.gameState == GameState.MOVINGSMASHERS)
        {

            for (int i = 0; i < upSmashers.Count; i++)
            {
                upSmashers[i].transform.position = Vector2.Lerp(upSmashers[i].transform.position, new Vector2(upSmashers[i].transform.position.x, smashersTargetPositions[i]), smasherMoveSpeed);
            }

            //check if smashers reach position
            if (Mathf.Abs(upSmashers[0].transform.position.y - smashersTargetPositions[0]) < .001f)
            {
                for (int i = 0; i < upSmashers.Count; i++)
                {
                    upSmashers[i].transform.position = new Vector2(upSmashers[i].transform.position.x, smashersTargetPositions[i]);
                }

                position = !position;

                if (!position)
                {
                    StartCoroutine(NewScene());
                    return;
                }
                else
                {
                    currentPosition = Random.Range(0, numOfSmashers);
                    player.transform.position = new Vector2(bottomSmashers[currentPosition].transform.position.x, bottomSmashers[currentPosition].transform.position.y + bottomSmashers[currentPosition].transform.localScale.y / 2 + player.GetComponent<SpriteRenderer>().size.y / 2);
                    ShowPlayer();
                }

                if (uIManager.mainMenuGui.activeInHierarchy) //if main menu is active
                    uIManager.gameState = GameState.MENU;
                else
                {
                    StartCoroutine(SlideDown());
                    uIManager.gameState = GameState.PLAYING;
                }
            }
        }

        if (uIManager.gameState == GameState.PLAYING && Input.GetMouseButtonUp(0))
        {
            canMove = true;
        }
    }

    public void OnHomeClicked()
    {
        StartCoroutine(Restart(false));
    }

    //create new scene
    IEnumerator CreateScene()
    {
        //Debug.Log("Creating new scene...");

        HidePlayer();

        position = false;

        uIManager.gameState = GameState.CREATINGSCENE;

        numOfSmashers = Random.Range(minNumberOfSmashers, maxNumberOfSmashers + 1);

        float smasherWidth = (screenSize.x * 2) / numOfSmashers; // calculate smasher width
        float smasherHeight;
        //Debug.Log(obstacleWidth);

        indexShorterSmasher = Random.Range(0, numOfSmashers); //choose index for shorter smasher

        //create bottom smashers
        for (int i = 0; i < numOfSmashers; i++)
        {
            smasherHeight = Random.Range(screenSize.y - .6f * screenSize.y, screenSize.y - .2f * screenSize.y); //random height depends on screen height and percent of screen height - in this case .4f of screen height
            lastSmasher = Instantiate(smasherPrefab);
            lastSmasher.transform.localScale = new Vector2(smasherWidth, smasherHeight);
            lastSmasher.transform.position = new Vector2(-screenSize.x + ((i + .5f) * smasherWidth), -screenSize.y + smasherHeight / 2);
            bottomSmashers.Add(lastSmasher);

        }


        //create top smashers
        for (int i = 0; i < numOfSmashers; i++)
        {
            smasherHeight = 2 * screenSize.y - bottomSmashers[i].gameObject.transform.localScale.y;

            lastSmasher = Instantiate(smasherPrefab);
            lastSmasher.transform.localScale = new Vector2(smasherWidth, smasherHeight);

            if (i == indexShorterSmasher) //one of the smashers needs to be put a little higher than others shorter
            {
                lastSmasher.transform.position = new Vector2(-screenSize.x + ((i + .5f) * smasherWidth), bottomSmashers[i].transform.position.y + bottomSmashers[i].transform.localScale.y / 2 + lastSmasher.transform.localScale.y / 2 + 1f);
            }
            else
                lastSmasher.transform.position = new Vector2(-screenSize.x + ((i + .5f) * smasherWidth), bottomSmashers[i].transform.position.y + bottomSmashers[i].transform.localScale.y / 2 + lastSmasher.transform.localScale.y / 2);

            smashersStartPositions.Add(lastSmasher.transform.position.y); //save last smasher position
            smashersTargetPositions.Add(lastSmasher.transform.position.y + moveDistance);

            upSmashers.Add(lastSmasher);

        }

        AudioManager.Instance.PlayEffects(AudioManager.Instance.slide);
        uIManager.gameState = GameState.MOVINGSMASHERS;

        yield return new WaitForSeconds(.1f);

        gameOverlay.GetComponent<Animator>().Play("GameOverlayHide");

    }

    public void Slide()
    {
        StartCoroutine(SlideDown());
    }

    IEnumerator NewScene()
    {
        uIManager.gameState = GameState.CREATINGSCENE;
        ScoreManager.Instance.UpdateScore(1);

        yield return new WaitForSeconds(.6f);


        ClearScene();
        StartCoroutine(CreateScene());
    }

    IEnumerator SlideDown()
    {
        AudioManager.Instance.PlayEffects(AudioManager.Instance.counter);

        for (int i = 0; i < numOfSmashers; i++)
        {
            smashersTargetPositions[i] = smashersStartPositions[i];
        }

        yield return new WaitForSeconds(3.5f);

        AudioManager.Instance.PlayEffects(AudioManager.Instance.slide);
        uIManager.gameState = GameState.MOVINGSMASHERS;
    }

    public void ShowPlayer()
    {
        player.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        player.gameObject.GetComponent<CircleCollider2D>().enabled = true;
    }

    public void HidePlayer()
    {
        player.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        player.gameObject.GetComponent<CircleCollider2D>().enabled = false;
    }

    //restart game, reset score, update platform position
    public void RestartGame()
    {
        StartCoroutine(Restart(true));
    }

    IEnumerator Restart(bool startGame)
    {
        gameOverlay.GetComponent<Animator>().Play("GameOverlayShow");

        yield return new WaitForSeconds(.6f);

        if (startGame)
            uIManager.ShowGameplay();

        ClearScene();
        scoreManager.ResetCurrentScore();
        StartCoroutine(CreateScene());
    }


    //clear all scene elements
    public void ClearScene()
    {
        smashersStartPositions.Clear();
        smashersTargetPositions.Clear();
        upSmashers.Clear();
        bottomSmashers.Clear();

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (GameObject item in obstacles)
        {
            Destroy(item);
        }
    }

    //show game over gui
    public void GameOver()
    {
        if (uIManager.gameState == GameState.PLAYING || uIManager.gameState == GameState.MOVINGSMASHERS)
        {
            StopAllCoroutines();
            AudioManager.Instance.PlayEffects(AudioManager.Instance.gameOver);
            AudioManager.Instance.PlayEffects(AudioManager.Instance.smash);
            AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
            uIManager.ShowGameOver();
            player.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            scoreManager.UpdateScoreGameover();
        }
    }
}
