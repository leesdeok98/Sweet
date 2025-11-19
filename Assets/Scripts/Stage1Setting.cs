using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Stage1Setting : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject pausePanel; 
    
    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel; 
    
    [Header("Tetris Inventory Link")]
    [SerializeField] private GameObject tetrisInventoryPanel; 
    [SerializeField] private ItemSelectionUI itemSelectionUI; 

    // ESC 눌렀을 때 인벤토리를 왼쪽으로 이동시키는 좌표
    [SerializeField] private Vector2 inventoryEscPosition = new Vector2(-300f, 0f);
    private RectTransform inventoryRect;
    private Vector2 inventoryOriginalPosition;
    
    private bool isPaused = false; 
    
    // '내가' 시간을 멈췄는지 기록하는 변수
    private bool didIPauseTime = false; 

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false); 
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // 1. 인벤토리 UI 위치 및 초기 상태 설정
        if (tetrisInventoryPanel != null)
        {
            inventoryRect = tetrisInventoryPanel.GetComponent<RectTransform>(); 
            if (inventoryRect != null)
            {
                inventoryOriginalPosition = inventoryRect.anchoredPosition; 
            }
            
            // 시작 시 아이템 선택 중이 아니라면 인벤토리 끄기
            if (itemSelectionUI == null || !itemSelectionUI.IsWaitingForClose())
            {
                tetrisInventoryPanel.SetActive(false);
            }
        }
        
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Input.ResetInputAxes();
        var es = EventSystem.current; 
        if (es) es.SetSelectedGameObject(null);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause(); 
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        // ItemSelectionUI에게 ESC 상태 알림 (확인 버튼 숨김용)
        if (itemSelectionUI != null)
        {
            itemSelectionUI.IsEscMenuOpen = isPaused;
        }

        if (isPaused) // [ESC 메뉴 열림]
        {
            // 1. 시간 정지 체크
            // (이미 시간이 0이면 아이템 선택 중이라는 뜻 -> 내가 멈춘 게 아님)
            if (Time.timeScale == 1f)
            {
                Time.timeScale = 0f;
                didIPauseTime = true; // "내가 멈췄음"
            }
            else
            {
                didIPauseTime = false;
            }
            
            // 2. 설정창 켜기
            if (pausePanel != null) pausePanel.SetActive(true);

            // 3. ★★★ [복구됨] 인벤토리 보여주기 ★★★
            // "ESC 때는 보이는 게 당연해" -> 강제로 켭니다.
            if (tetrisInventoryPanel != null) 
            {
                tetrisInventoryPanel.SetActive(true);
                
                // (선택) 위치 이동
                if (inventoryRect != null)
                    inventoryRect.anchoredPosition = inventoryEscPosition;
            }
        }
        else // [ESC 메뉴 닫힘]
        {
            // 1. 설정창 끄기
            if (pausePanel != null) pausePanel.SetActive(false);

            // 2. 시간 복구 및 인벤토리 닫기
            // ★★★ [핵심 수정] "내가 멈췄던 거라면(=원래 게임 중이었다면)" 무조건 닫습니다. ★★★
            if (didIPauseTime) 
            {
                Time.timeScale = 1f; 
                didIPauseTime = false; 

                // 원래 게임 중이었으니 인벤토리는 닫혀있는 게 맞음 -> 강제 종료
                if (tetrisInventoryPanel != null)
                {
                    tetrisInventoryPanel.SetActive(false);
                }
            }
            
            // (참고) 만약 didIPauseTime이 false라면? 
            // -> 아이템 선택 중이었다는 뜻이므로, 인벤토리를 끄지 않고 그대로 둡니다.

            // 3. 인벤토리 위치 원복
            if (tetrisInventoryPanel != null && inventoryRect != null)
            {
                inventoryRect.anchoredPosition = inventoryOriginalPosition;
            }
        }
    }

    // 'Resume' 버튼용 함수
    public void ResumeGame()
    {
        isPaused = false; 
        
        if (itemSelectionUI != null) itemSelectionUI.IsEscMenuOpen = false;
        if (pausePanel != null) pausePanel.SetActive(false);

        // ★★★ [핵심 수정] 여기도 동일하게 적용 ★★★
        if (didIPauseTime) 
        {
            Time.timeScale = 1f; 
            didIPauseTime = false; 

            if (tetrisInventoryPanel != null)
            {
                tetrisInventoryPanel.SetActive(false);
            }
        }

        if (tetrisInventoryPanel != null && inventoryRect != null)
        {
            inventoryRect.anchoredPosition = inventoryOriginalPosition;
        }
        
        Input.ResetInputAxes();
        var es = EventSystem.current; 
        if (es) es.SetSelectedGameObject(null);
    }

    // --- 게임 오버 및 씬 이동 (기존 유지) ---
    public void ShowGameOver()
    {
        Debug.Log("Stage1Setting: 게임 오버 UI 활성화");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling(); 
        }
        Time.timeScale = 0f;
        if (tetrisInventoryPanel != null) tetrisInventoryPanel.SetActive(false);
    }

    public void QuitToMain() { StartCoroutine(GoMainRoutine()); }
    private IEnumerator GoMainRoutine() { 
        Time.timeScale = 1f; 
        AudioListener.pause = false;
        var players = FindObjectsOfType<Player>(true);
        foreach (var p in players) Destroy(p.gameObject);
        yield return null; 
        SceneManager.LoadScene("Main", LoadSceneMode.Single); 
    }
    public void OnClickRetry() { StartCoroutine(LoadRoutine(SceneManager.GetActiveScene().name)); }
    private IEnumerator LoadRoutine(string sceneName) { 
        Time.timeScale = 1f; 
        AudioListener.pause = false;
        Input.ResetInputAxes();
        var es = EventSystem.current; if (es) es.SetSelectedGameObject(null);
        var players = FindObjectsOfType<Player>(true);
        foreach (var p in players) Destroy(p.gameObject);
        yield return null; 
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single); 
    }
}