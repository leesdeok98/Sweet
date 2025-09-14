using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player; // Player 타입으로 선언

    void Awake()
    {
        // 싱글톤 초기화
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // 씬에서 Player 오브젝트를 찾아 Player 컴포넌트 할당
        if (player == null)
            player = GameObject.Find("Player").GetComponent<Player>();
    }
}
