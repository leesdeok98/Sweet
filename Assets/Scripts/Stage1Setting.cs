using System.Collections;
using System.Collections.Generic;
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

    // ESC 눌렀을 때 인벤토리 이동 좌표
    [SerializeField] private Vector2 inventoryEscPosition = new Vector2(-500f, 0f);
    private RectTransform inventoryRect;
    private Vector2 inventoryOriginalPosition;
    
    private bool isPaused = false; 

    // '내가' 시간을 멈췄는지 기록
    private bool didIPauseTime = false; 

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false); 
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        // 인벤토리 UI 위치 및 초기화
        if (tetrisInventoryPanel != null)
        {
            inventoryRect = tetrisInventoryPanel.GetComponent<RectTransform>(); 
            if (inventoryRect != null)
            {
                inventoryOriginalPosition = inventoryRect.anchoredPosition; 
            }
            
            // 시작 시 인벤토리 끄기 (아이템 선택 중이 아니라면)
            if (itemSelectionUI == null || !itemSelectionUI.IsWaitingForClose())
            {
                 if (tetrisInventoryPanel != null) 
                 {
                     tetrisInventoryPanel.SetActive(false);
                 }
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
        isPaused = !isPaused; // 설정창 켜기/끄기 토글

        // ItemSelectionUI에게 알림 (확인 버튼 제어용)
        if (itemSelectionUI != null)
        {
            itemSelectionUI.IsEscMenuOpen = isPaused;
        }

        if (isPaused) // [ESC 메뉴 열림]
        {
            // 1. 시간 정지 체크
            if (Time.timeScale == 1f)
            {
                Time.timeScale = 0f;
                didIPauseTime = true; // "내가 멈췄음"
            }
            else
            {
                didIPauseTime = false; // "원래 멈춰있었음 (아이템 선택 중)"
            }
            
            // 2. 설정창 켜기
            if (pausePanel != null) pausePanel.SetActive(true);

            // 3. 인벤토리 보여주기 (설정창에서는 항상 보여줌)
            if (tetrisInventoryPanel != null) 
            {
                 tetrisInventoryPanel.SetActive(true);
            }

            // 4. 인벤토리 위치 이동
            if (tetrisInventoryPanel != null && inventoryRect != null) 
            {
                 inventoryRect.anchoredPosition = inventoryEscPosition; 
            }
        }
        else // [ESC 메뉴 닫힘 - 게임 복귀]
        {
            // 1. 설정창 끄기
            if (pausePanel != null) pausePanel.SetActive(false);

            // 2. 시간 복구 및 인벤토리 정리
            if (didIPauseTime) 
            {
                Time.timeScale = 1f; // 게임 재개
                didIPauseTime = false; 

                // ★★★ [핵심 수정] 설정창에서 바꾼 배치를 즉시 적용! ★★★
                // 확인 버튼 없이도, 설정창을 나가는 순간 변경된 스킬 상태를 반영합니다.
                if (InventoryManager.instance != null)
                {
                    InventoryManager.instance.UpdateActiveSkills();
                    Debug.Log("[Stage1Setting] ESC 메뉴 닫힘: 스킬 상태 갱신 완료");
                }
                
                // 원래 게임 중이었으니 인벤토리는 닫기
                if (tetrisInventoryPanel != null)
                {
                    tetrisInventoryPanel.SetActive(false);
                }
            }

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
        isPaused = false; // 설정창 끄기
        
        if (itemSelectionUI != null) itemSelectionUI.IsEscMenuOpen = false;
        if (pausePanel != null) pausePanel.SetActive(false);

        if (didIPauseTime) 
        {
            Time.timeScale = 1f; // 게임 재개
            didIPauseTime = false; 

            // ★★★ [핵심 수정] 버튼으로 나갈 때도 스킬 갱신! ★★★
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.UpdateActiveSkills();
                Debug.Log("[Stage1Setting] Resume 버튼 클릭: 스킬 상태 갱신 완료");
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

    // --- 게임 오버 및 씬 이동 함수들 ---
    public void ShowGameOver()
    {
        Debug.Log("Stage1Setting: 게임 오버 UI 활성화");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
            // (오디오 매니저가 있다면 사용)
            // AudioManager.instance.PlaySfx(AudioManager.Sfx.GameOver_SFX);
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