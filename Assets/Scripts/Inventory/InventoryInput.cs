using UnityEngine;

public class InventoryInput : MonoBehaviour
{
    // ★ '꺼지는' panelRoot에 붙어있는 ItemSelectionUI 스크립트를 연결할 슬롯
    public ItemSelectionUI itemSelectionUI;

    // ▼▼▼ '확인' 버튼 GameObject 연결 필수 ▼▼▼
    public GameObject confirmButton;
    
    void Update()
    {
        if (itemSelectionUI == null) return;
        
        // 1. ESC 메뉴가 열려있는지 상태 확인
        bool isEscOpen = itemSelectionUI.IsEscMenuOpen;
        
        // 2. '확인' 대기 상태인지 확인 (아이템 선택 후 인벤토리 정리 중)
        bool isWaitingForConfirmation = itemSelectionUI.IsWaitingForClose();

        // ★ [핵심 로직]: 확인 대기 상태이고 && ESC 메뉴가 닫혀있을 때만 버튼을 켠다
        bool shouldShowConfirmButton = isWaitingForConfirmation && !isEscOpen; 

        if (confirmButton != null)
        {
            // 버튼 활성/비활성 상태 동기화
            if (confirmButton.activeSelf != shouldShowConfirmButton)
            {
                confirmButton.SetActive(shouldShowConfirmButton);
            }
        }

        // ★★★ [수정됨] P키 입력 감지 로직 삭제됨 ★★★
        // 이제 키보드 P를 눌러도 아무 반응이 없으며, 오직 화면의 버튼을 눌러야 합니다.
    }

    /// <summary>
    /// (UI 버튼 클릭 이벤트로 연결하세요) 
    /// UI를 닫고, 0.5초 뒤에 스킬을 발동시킵니다.
    /// </summary>
    public void ConfirmSelection()
    {
        if (itemSelectionUI == null) return;

        bool isWaitingForClose = itemSelectionUI.IsWaitingForClose();
        bool isEscOpen = itemSelectionUI.IsEscMenuOpen;

        // ESC 메뉴가 닫혀 있을 때만 실행을 허용합니다.
        if (isWaitingForClose && !isEscOpen)
        {
            Debug.Log("[InventoryInput] Confirm button clicked. Closing panel.");
            
            // 1. UI 닫기 및 게임 재개 (Time.timeScale = 1)
            itemSelectionUI.ClosePanelAndResume(); 

            // 2. ★★★ [추가됨] 0.5초 뒤에 스킬 발동 요청 ★★★
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.ApplySkillsWithDelay(0.5f);
            }
        }
        else if (isEscOpen)
        {
            Debug.LogWarning("[InventoryInput] Confirm button press ignored because ESC Menu is open.");
        }
    }
}