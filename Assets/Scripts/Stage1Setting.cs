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

    [Header("Clear Panel")]
    [SerializeField] private GameObject ClearPanel;
 
    [SerializeField] private Vector2 inventoryEscPosition = new Vector2(-300f, 0f);
    private RectTransform inventoryRect;
    private Vector2 inventoryOriginalPosition;
    
    private bool isPaused = false; 
    private bool didIPauseTime = false; 

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false); 
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (tetrisInventoryPanel != null)
        {
            inventoryRect = tetrisInventoryPanel.GetComponent<RectTransform>(); 
            if (inventoryRect != null)
            {
                inventoryOriginalPosition = inventoryRect.anchoredPosition; 
            }
            
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
        // 게임 오버 패널이 떠 있는 동안에는 ESC 입력 무시
        if ((gameOverPanel != null && gameOverPanel.activeSelf) ||
            (ClearPanel != null && ClearPanel.activeSelf))
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause(); 
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (itemSelectionUI != null)
        {
            itemSelectionUI.IsEscMenuOpen = isPaused;
        }

        if (isPaused) // [ESC 열림]
        {
            if (Time.timeScale == 1f)
            {
                Time.timeScale = 0f;
                didIPauseTime = true; 
            }
            else
            {
                didIPauseTime = false;
            }
            
            if (pausePanel != null) pausePanel.SetActive(true);

            if (tetrisInventoryPanel != null) 
            {
                tetrisInventoryPanel.SetActive(true);
                if (inventoryRect != null)
                    inventoryRect.anchoredPosition = inventoryEscPosition;
            }
        }
        else // [ESC 닫힘]
        {
            if (pausePanel != null) pausePanel.SetActive(false);

            if (didIPauseTime) 
            {
                Time.timeScale = 1f; 
                didIPauseTime = false; 

                // ★★★ [핵심 수정] '변경된 사항이 있을 때만(isDirty)' 스킬 갱신! ★★★
                // 이제 아이템을 건드리지 않았다면 스킬 초기화(깜빡임)가 발생하지 않습니다.
                if (InventoryManager.instance != null && InventoryManager.instance.isDirty)
                {
                    InventoryManager.instance.UpdateActiveSkills();
                    Debug.Log("[Stage1Setting] 인벤토리 변경됨 -> 스킬 상태 갱신");
                }
                
                if (tetrisInventoryPanel != null)
                {
                    tetrisInventoryPanel.SetActive(false);
                }
            }

            if (tetrisInventoryPanel != null && inventoryRect != null)
            {
                inventoryRect.anchoredPosition = inventoryOriginalPosition;
            }
        }
    }

    public void ResumeGame()
    {
        isPaused = false; 
        
        if (itemSelectionUI != null) itemSelectionUI.IsEscMenuOpen = false;
        if (pausePanel != null) pausePanel.SetActive(false);

        if (didIPauseTime) 
        {
            Time.timeScale = 1f; 
            didIPauseTime = false; 

            // ★★★ [핵심 수정] 여기도 동일하게 적용 ★★★
            if (InventoryManager.instance != null && InventoryManager.instance.isDirty)
            {
                InventoryManager.instance.UpdateActiveSkills();
                Debug.Log("[Stage1Setting] 인벤토리 변경됨 -> 스킬 상태 갱신");
            }

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

    // --- (이하 게임 오버 및 씬 이동 함수는 기존 유지) ---
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