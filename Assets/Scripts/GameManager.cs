using UnityEngine;

public class GameManager : MonoBehaviour
{
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
        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime) {
            gameTime = maxGameTime;
        }
    }

    void Start()
    {
        // ������ Player ������Ʈ�� ã�� Player ������Ʈ �Ҵ�
        if (player == null)
            player = GameObject.Find("Player").GetComponent<Player>();
    }
}
