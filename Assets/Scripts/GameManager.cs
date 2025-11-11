using UnityEngine;

public class GameManager : MonoBehaviour
{

    public float enmiesClearTime = 10f;

    [HideInInspector]
    public bool hasEnemiesCleared = false;

    [HideInInspector]
    public bool isGameWon = false;
    [HideInInspector]
    public bool isGameOver = false;

    public float bossSpawnTIme = 300;

    public static GameManager instance;
    public float gameTime;
    public float maxGameTime = 5 * 60f;
    public PoolManager pool;
    public Player player; // Player Ÿ������ ����

    void Awake()
    {
        // �̱��� �ʱ�ȭ
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    void Update()
    {
        if (GameManager.instance.hasEnemiesCleared)
        {
            return;
        }

        float currentGameTime = GameManager.instance.gameTime;

        if (hasEnemiesCleared)
        {
            return;
        }

        gameTime += Time.deltaTime;

        if (gameTime >= enmiesClearTime && !hasEnemiesCleared)
        {
            pool.ClearAllEnemies();
            hasEnemiesCleared = true;
            Debug.Log("제발 사라져라");
        }

        //gameTime += Time.deltaTime;

        //if (gameTime > maxGameTime) {
        //    gameTime = maxGameTime;
        //}

        //if ( gameTime >= enmiesClearTime && !hasEnemiesCleared)
        //{
        //    pool.ClearAllEnemies();
        //    hasEnemiesCleared = true;
        //}
    }

    void Start()
    {
        // ������ Player ������Ʈ�� ã�� Player ������Ʈ �Ҵ�
        if (player == null)
            player = GameObject.Find("Player").GetComponent<Player>();
    }

    public void GameClear()
    {
        if (isGameWon || isGameOver) return;
        isGameWon = true;
        Debug.Log("게임 승리");
        // 여기에 게임 승리 UI 띄우는 코드 넣으셈
    }

    public void GameOver()
    {
        if (isGameWon || isGameOver) return;
        isGameOver = true;
        Debug.Log("게임 패배");
        // 여기에 게임 오버 UI 띄우는 코드 넣으셈 ㅋ
    }
}
