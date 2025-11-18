using UnityEngine;

public class InventoryInput : MonoBehaviour
{
    // ★ '꺼지는' panelRoot에 붙어있는 ItemSelectionUI 스크립트를 연결할 슬롯
    public ItemSelectionUI itemSelectionUI;

    // ▼▼▼ P키 대용 '확인' 버튼 GameObject 연결 필수 ▼▼▼
    public GameObject confirmButton;
    
    void Update()
    {
        if (itemSelectionUI == null) return;
        
        // 1. ESC 메뉴가 열려있는지 상태 확인 (이 값이 true여야 버튼이 숨겨집니다)
        bool isEscOpen = itemSelectionUI.IsEscMenuOpen;
        
        // 2. 'P' 키를 기다리는 상태인지 확인
        bool isWaitingForConfirmation = itemSelectionUI.IsWaitingForClose();

        // ★ [핵심 로직]: P키 대기 상태이고 && ESC 메뉴가 닫혀있을 때만 버튼을 켠다
        bool shouldShowConfirmButton = isWaitingForConfirmation && !isEscOpen; 

        if (confirmButton != null)
        {
            // ESC 메뉴가 열리면 (isEscOpen=true), shouldShowConfirmButton은 false가 되어 버튼이 비활성화됩니다.
            if (confirmButton.activeSelf != shouldShowConfirmButton)
            {
                confirmButton.SetActive(shouldShowConfirmButton);
            }
        }

        // 'P키 대기' 상태이고 ESC 메뉴가 닫혀 있을 때만 'P'키 입력을 받음
        if (isWaitingForConfirmation && !isEscOpen)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("[InventoryInput] 'P' key detected. Calling ConfirmSelection().");
                ConfirmSelection();
            }
        }
    }

    /// <summary>
    /// (P키 또는 UI 버튼이 호출) 게임을 재개합니다.
    /// </summary>
    public void ConfirmSelection()
    {
        bool isWaitingForClose = (itemSelectionUI != null && itemSelectionUI.IsWaitingForClose());
        bool isEscOpen = (itemSelectionUI != null && itemSelectionUI.IsEscMenuOpen);

        // ESC 메뉴가 닫혀 있을 때만 실행을 허용합니다.
        if (isWaitingForClose && !isEscOpen)
        {
            Debug.Log("[InventoryInput] ConfirmSelection activated. Calling Close().");
            itemSelectionUI.Close();
        }
        else if (isEscOpen)
        {
            Debug.LogWarning("[InventoryInput] Confirm button press ignored because ESC Menu is open.");
        }
    }
}