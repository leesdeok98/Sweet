using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player; // Player Ÿ������ ����

    void Awake()
    {
        // �̱��� �ʱ�ȭ
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // ������ Player ������Ʈ�� ã�� Player ������Ʈ �Ҵ�
        if (player == null)
            player = GameObject.Find("Player").GetComponent<Player>();
    }
}
