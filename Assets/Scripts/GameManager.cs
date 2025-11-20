using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float enmiesClearTime = 10f;

    [HideInInspector] public bool hasEnemiesCleared = false;
    [HideInInspector] public bool isGameWon = false;
    [HideInInspector] public bool isGameOver = false;

    public float bossSpawnTIme = 300;

    public static GameManager instance;
    public float gameTime;
    public float maxGameTime = 5 * 60f;
    public PoolManager pool;
    public Player player; // Player 타입으로 참조

    void Awake()
    {
        // 싱글톤 초기화
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 게임 상태 초기화
        ResetState();

        AudioManager.instance.PlayBgm(AudioManager.Bgm.Battle_BGM);

        // 플레이어 오브젝트 자동 할당 보강
        if (player == null)
        {
            GameObject pObj = GameObject.Find("Player");
            if (pObj != null)
                player = pObj.GetComponent<Player>();
        }
    }

    void Update()
    {
        // 적 전부 제거 이미 했으면 더 이상 시간 계산 안 함
        if (hasEnemiesCleared)
            return;

        gameTime += Time.deltaTime;

        if (gameTime >= enmiesClearTime && !hasEnemiesCleared)
        {
            if (pool != null)
            {
                pool.ClearAllEnemies();
                hasEnemiesCleared = true;
                Debug.Log("제발 사라져라");
            }
        }
    }

    // 🔹 씬 재시작/새 판 시작 시 상태 초기화용
    public void ResetState()
    {
        gameTime = 0f;
        hasEnemiesCleared = false;
        isGameWon = false;
        isGameOver = false;
    }

    public void GameClear()
    {
        if (isGameWon || isGameOver) return;
        isGameWon = true;
        Debug.Log("게임 승리");
        // 게임 승리 UI 띄우기 등
    }

    public void GameOver()
    {
        if (isGameWon || isGameOver) return;
        isGameOver = true;
        Debug.Log("게임 패배");
        // 실제 패널 활성화는 Player.Die()에서 처리 중
    }
}
