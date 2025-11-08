using UnityEngine;

public class GameManager : MonoBehaviour
{

    public float enmiesClearTime = 10f;

    [HideInInspector]
    public bool hasEnemiesCleared = false;

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

        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
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
}
