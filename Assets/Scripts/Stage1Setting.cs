// Stage1Setting.cs (최종 완성본: '확인 버튼' 관리 로직 제거 - InventoryInput이 담당함)
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
    
    [Header("Tetris Inventory Link")]
    [SerializeField] private GameObject tetrisInventoryPanel; 
    [SerializeField] private ItemSelectionUI itemSelectionUI; 

    // ★ '확인 버튼' 슬롯이 여기서 '삭제'되었습니다.

    // ('130줄' 이동 기능)
    [SerializeField] private Vector2 inventoryEscPosition = new Vector2(-500f, 0f);
    private RectTransform inventoryRect;
    private Vector2 inventoryOriginalPosition;
    
    private bool isPaused = false; 

    // '내가' (Stage1Setting) '시간'을 '직접' 멈췄는지 '기록'
    private bool didIPauseTime = false; 

    void Start()
    {
        pausePanel.SetActive(false); 
        
        // ('130줄' 이동 기능) 'UI 부품' (RectTransform)을 '찾아본다'
        if (tetrisInventoryPanel != null)
        {
            inventoryRect = tetrisInventoryPanel.GetComponent<RectTransform>(); 
            if (inventoryRect != null)
            {
                inventoryOriginalPosition = inventoryRect.anchoredPosition; 
            }
        }
        
        // '인벤토리 끄기' (Start)
        if (itemSelectionUI == null || !itemSelectionUI.IsWaitingForClose())
        {
             if (tetrisInventoryPanel != null) 
             {
                 tetrisInventoryPanel.SetActive(false);
             }
        }
        
        // (★ 1번(A) 기능 추가) 씬 시작 시 시간/오디오/입력 초기화
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

    // ★ 2번(B)의 'P키 대기' 이동 버그 '수정된' 함수
    public void TogglePause()
    {
        isPaused = !isPaused; // '설정창' 켜기/끄기

        if (isPaused) // '일시정지' (ESC 누름)
        {
            // ★ 1. '시간'이 '이미' 멈춰있는지 (1f인지) 확인
            if (Time.timeScale == 1f)
            {
                Time.timeScale = 0f;
                didIPauseTime = true; // '내가' 멈췄다고 '기록'
            }
            else
            {
                didIPauseTime = false; // '내가' 멈춘 게 '아님'
            }
            
            // ★ 2. '설정창' 켜기
            pausePanel.SetActive(true);

            // ★ 3. '인벤토리 켜기' (P키 대기 중이 '아닐' 때만)
            if (itemSelectionUI == null || !itemSelectionUI.IsWaitingForClose())
            {
                 if (tetrisInventoryPanel != null) 
                 {
                     tetrisInventoryPanel.SetActive(true); // '평소' 상태일 때만 '켠다'
                 }
            }

            // ★ 4. '인벤토리 이동' ('P키 대기'/'평소' '모두' '왼쪽'으로)
            if (tetrisInventoryPanel != null && inventoryRect != null) 
            {
                 inventoryRect.anchoredPosition = inventoryEscPosition; 
            }
            
            // ★ 5. [삭제] '확인 버튼' 숨기기 (InventoryInput이 담당)
        }
        else // '게임 재개' (ESC 다시 누름)
        {
            // ★ 1. '설정창' 끄기
            pausePanel.SetActive(false);

            // ★ 2. '시간' 및 '인벤토리 끄기' 복구 ('내가' 멈췄을 때만)
            if (didIPauseTime) 
            {
                Time.timeScale = 1f; // '게임 재개'
                didIPauseTime = false; // '기록' 삭제
                
                if (itemSelectionUI == null || !itemSelectionUI.IsWaitingForClose())
                {
                    if (tetrisInventoryPanel != null)
                    {
                        tetrisInventoryPanel.SetActive(false); // '평소' ESC는 '끈다'
                    }
                }
            }

            // ★ 3. '인벤토리 위치' 복구 ('P키 대기'/'평소' '모두' '가운데'로)
            if (tetrisInventoryPanel != null && inventoryRect != null)
            {
                inventoryRect.anchoredPosition = inventoryOriginalPosition;
            }

            // ★ 4. [삭제] '확인 버튼' 다시 켜기 (InventoryInput이 담당)
        }
    }

    // ★ 2번(B)의 'Resume' 버튼
    public void ResumeGame()
    {
        isPaused = false; // '설정창' 끄기
        pausePanel.SetActive(false);

        // ★ 2. '시간' 및 '인벤토리 끄기' 복구 ('내가' 멈췄을 때만)
        if (didIPauseTime) 
        {
            Time.timeScale = 1f; // '게임 재개'
            didIPauseTime = false; // '기록' 삭제

            if (itemSelectionUI == null || !itemSelectionUI.IsWaitingForClose())
            {
                if (tetrisInventoryPanel != null)
                {
                    tetrisInventoryPanel.SetActive(false); // '평소' ESC는 '끈다'
                }
            }
        }

        // ★ 3. '인벤토리 위치' 복구 ('P키 대기'/'평소' '모두' '가운데'로)
        if (tetrisInventoryPanel != null && inventoryRect != null)
        {
            inventoryRect.anchoredPosition = inventoryOriginalPosition;
        }

        // ★ 4. [삭제] '확인 버튼' 다시 켜기 (InventoryInput이 담당)
        
        // (★ 1번(A) 기능 추가) 입력 축 초기화
        Input.ResetInputAxes();
        var es = EventSystem.current; 
        if (es) es.SetSelectedGameObject(null);
    }

    // ─────────────────────────────────────────────────────────────
    // ▼▼▼ 1번(A)에서 가져온 '재시작', '메인으로' 기능 (수정 없음) ▼▼▼
    // ─────────────────────────────────────────────────────────────

    // ... (QuitToMain, GoMainRoutine, OnClickRetry, LoadRoutine 함수는 이전과 동일하게 유지) ...
    
    /// <summary>
    /// [1번(A) 기능] '메인으로' 버튼 클릭 시 (안전하게 씬 이동)
    /// </summary>
    public void QuitToMain()
    {
        StartCoroutine(GoMainRoutine());
    }

    /// <summary>
    /// [1번(A) 기능] 메인 씬 로드 코루틴
    /// </summary>
    private System.Collections.IEnumerator GoMainRoutine()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // 메인으로 가기 전에 모든 Player 인스턴스 파괴
        var players = FindObjectsOfType<Player>(true);
        foreach (var p in players)
            Destroy(p.gameObject);

        yield return null; // 파괴가 반영되도록 한 프레임 대기

        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    /// <summary>
    /// [1번(A) 기능] '재시작' 버튼 클릭 시
    /// </summary>
    public void OnClickRetry()
    {
        // 현재 씬의 이름을 가져와서 다시 로드
        StartCoroutine(LoadRoutine(SceneManager.GetActiveScene().name));
    }

    /// <summary>
    /// [1번(A) 기능] 씬 재시작 코루틴 (★ '좀비 플레이어' 버그 수정됨 ★)
    /// </summary>
    private IEnumerator LoadRoutine(string sceneName)
    {
        // 정지 해제 / 사운드 / 입력 축 초기화
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Input.ResetInputAxes();
        var es = EventSystem.current; 
        if (es) es.SetSelectedGameObject(null);
        
        // [버그 수정] '죽은 Player'를 확실히 파괴하고 씬을 로드
        var players = FindObjectsOfType<Player>(true);
        foreach (var p in players)
            Destroy(p.gameObject);

        yield return null; // 파괴가 반영되도록 한 프레임 대기
        
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}