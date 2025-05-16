/**********************************************************
 * Script Name: InventoryUI
 * Author: 김우성
 * Date Created: 2025-05-14
 * Last Modified: 2025-05-14
 * Description: 
 * - 그리드형 인벤토리 UI 관리.
 * - 4x4 슬롯, 원소 목록, 분해/병합 버튼.
 * - Inventory와 동기화, 씬 전환 시 유지.
 *********************************************************/

using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{
    static InventoryUI _instance;
    public static InventoryUI Instance => _instance;

    [Header("UI Components")]
    [SerializeField] GameObject _inventoryPanel;
    [SerializeField] GridLayoutGroup _gridLayout;
    [SerializeField] Inventory _inventory;
    Button[,] _gridButtons;
    Image[,] _gridIcons;
    TextMeshProUGUI[,] _gridTexts;
    GameObject _activePanel;

    [Header("Merger UI")]
    Image[] _mergerSlots;
    Button _mergerButton;
    TextMeshProUGUI _mergerResultText;
    ItemDataSO[] _mergerSlotItems = new ItemDataSO[2];

    [Header("Crafter UI")]
    Image[] _crafterSlots;
    Button _crafterButton;
    TMP_Dropdown _recipeDropdown;
    ItemDataSO[] _crafterSlotItems = new ItemDataSO[2];


    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject.transform.parent.gameObject);
            return;
        }
        _instance = this;

        DontDestroyOnLoad(gameObject.transform.parent.gameObject);
        InitializeUI();
        _inventoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _activePanel != null && _activePanel.activeSelf)
        {
            CloseActivePanel();
            if (IsInventoryOpen())
            {
                ToggleInventory();
            }
        }
    }

    private void InitializeUI()
    {
        _gridButtons = new Button[_inventory.GridWidth, _inventory.MaxGridHeight]; // 5x5
        _gridIcons = new Image[_inventory.GridWidth, _inventory.MaxGridHeight];
        _gridTexts = new TextMeshProUGUI[_inventory.GridWidth, _inventory.MaxGridHeight];

        int expectedSlots = _inventory.GridWidth * _inventory.MaxGridHeight; // 25
        if (_gridLayout.transform.childCount != expectedSlots)
        {
            Debug.LogError($"Expected {expectedSlots} slots in GridLayout, found {_gridLayout.transform.childCount}");
            return;
        }

        for (int y = 0; y < _inventory.MaxGridHeight; y++)
        {
            for (int x = 0; x < _inventory.GridWidth; x++)
            {
                int index = y * _inventory.GridWidth + x; // 5열 기준
                GameObject slot = _gridLayout.transform.GetChild(index).gameObject;
                _gridButtons[x, y] = slot.GetComponent<Button>();
                _gridIcons[x, y] = slot.GetComponent<Image>();
                _gridTexts[x, y] = slot.GetComponentInChildren<TextMeshProUGUI>();

                // 비활성 슬롯 초기 설정
                bool isLocked = y >= _inventory.ActiveGridHeight;
                _gridButtons[x, y].interactable = !isLocked;
                _gridIcons[x, y].color = isLocked ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : Color.white;

                int slotX = x, slotY = y;
                _gridButtons[x, y].onClick.RemoveAllListeners();
                _gridButtons[x, y].onClick.AddListener(() => OnSlotClicked(slotX, slotY));
                var slotDrag = slot.GetComponent<SlotDrag>();
                if (slotDrag == null) slotDrag = slot.AddComponent<SlotDrag>();
                slotDrag.Setup(this, slotX, slotY);
                var slotDrop = slot.GetComponent<SlotDrop>();
                if (slotDrop == null) slotDrop = slot.AddComponent<SlotDrop>();
                slotDrop.Setup(this, slotX, slotY);
            }
        }
        UpdateUI();
    }

    public void ShowMergerPanel(GameObject panel)
    {
        if (_activePanel == panel) return;

        _activePanel = panel;
        _activePanel.SetActive(true);
        _inventoryPanel.SetActive(true);
        InitializeMergerPanel();
        UpdateUI();
    }

    public void ShowCrafterPanel(GameObject panel)
    {
        if (_activePanel == panel) return;

        _activePanel = panel;
        _activePanel.SetActive(true);
        _inventoryPanel.SetActive(true);
        InitializeCrafterPanel();
        UpdateUI();
    }

    private void InitializeMergerPanel()
    {
        _mergerSlots = new Image[2];
        _mergerSlotItems = new ItemDataSO[2];
        for (int i = 0; i < 2; i++)
        {
            int index = i;
            _mergerSlots[i] = _activePanel.transform.Find($"Slot{index + 1}").GetComponent<Image>();
            var dropHandler = _mergerSlots[i].gameObject.AddComponent<SlotDrop>();
            if (dropHandler == null) dropHandler = _mergerSlots[i].gameObject.AddComponent<SlotDrop>();
            dropHandler.Setup(this, -1, -1);
            dropHandler.OnDropCallback = () => OnMergerSlotDropped(index);
        }
        _mergerButton = _activePanel.transform.Find("MergeButton").GetComponent<Button>();
        _mergerResultText = _activePanel.transform.Find("ResultText").GetComponent<TextMeshProUGUI>();
        _mergerButton.onClick.AddListener(OnMergeClicked);

        var closeButton = _activePanel.transform.Find("CloseButton")?.GetComponent<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseActivePanel);
        }
    }

    private void InitializeCrafterPanel()
    {
        _crafterSlots = new Image[2];
        _crafterSlotItems = new ItemDataSO[2];
        for (int i = 0; i < 2; i++)
        {
            int index = i;
            _crafterSlots[i] = _activePanel.transform.Find($"Slot{index + 1}").GetComponent<Image>();
            var dropHandler = _crafterSlots[i].gameObject.AddComponent<SlotDrop>();
            if (dropHandler == null) dropHandler = _crafterSlots[i].gameObject.AddComponent<SlotDrop>();
            dropHandler.Setup(this, -1, -1);
            dropHandler.OnDropCallback = () => OnCrafterSlotDropped(index);
        }
        _crafterButton = _activePanel.transform.Find("CraftButton").GetComponent<Button>();
        _recipeDropdown = _activePanel.transform.Find("RecipeDropdown").GetComponent<TMP_Dropdown>();
        _crafterButton.onClick.AddListener(OnCraftClicked);

        // 드롭다운에 레시피 추가
        _recipeDropdown.ClearOptions();
        var recipes = _inventory.GetCraftRecipes();
        _recipeDropdown.AddOptions(recipes.Select(r => r.recipeName).ToList());

        var closeButton = _activePanel.transform.Find("CloseButton")?.GetComponent<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseActivePanel);
        }
    }

    private void OnMergerSlotDropped(int slotIndex)
    {
        var slotDrag = EventSystem.current.currentSelectedGameObject?.GetComponent<SlotDrag>();
        if (slotDrag != null)
        {
            int x = slotDrag.SlotX, y = slotDrag.SlotY;
            var item = _inventory.GetGridItem(x, y);
            if (item != null)
            {
                _mergerSlotItems[slotIndex] = item;
                _inventory.RemoveItem(x, y);
                UpdateUI();
            }
        }
    }

    private void OnCrafterSlotDropped(int slotIndex)
    {
        var slotDrag = EventSystem.current.currentSelectedGameObject?.GetComponent<SlotDrag>();
        if (slotDrag != null)
        {
            int x = slotDrag.SlotX, y = slotDrag.SlotY;
            var item = _inventory.GetGridItem(x, y);
            if (item != null)
            {
                _crafterSlotItems[slotIndex] = item;
                _inventory.RemoveItem(x, y);
                UpdateUI();
            }
        }
    }

    private void OnMergeClicked()
    {
        if (_mergerSlotItems[0] == null || _mergerSlotItems[1] == null) return;

        if (_inventory.MergeItems(_mergerSlotItems[0], _mergerSlotItems[1]))
        {
            _activePanel.SetActive(false);
            _activePanel = null;
            _mergerSlotItems = new ItemDataSO[2];
            UpdateUI();
        }
        else
        {
            RestoreMergerItems();
            UpdateUI();
        }
    }

    private void OnCraftClicked()
    {
        if (_crafterSlotItems[0] == null || _crafterSlotItems[1] == null) return;

        var recipes = _inventory.GetCraftRecipes();
        if (_recipeDropdown.value < recipes.Length)
        {
            if (_inventory.CraftItem(recipes[_recipeDropdown.value], _crafterSlotItems[0], _crafterSlotItems[1]))
            {
                _activePanel.SetActive(false);
                _activePanel = null;
                _crafterSlotItems = new ItemDataSO[2];
                UpdateUI();
            }
            else
            {
                RestoreCrafterItems();
                UpdateUI();
            }
        }
    }

    private void RestoreMergerItems()
    {
        for (int i = 0; i < _mergerSlotItems.Length; i++)
        {
            if (_mergerSlotItems[i] != null)
            {
                if (_inventory.AddItem(_mergerSlotItems[i]))
                {
                    Debug.Log($"Resotred {_mergerSlotItems[i].displayName} to inventory");
                }
                else
                {
                    Debug.LogWarning($"Failed to resotre {_mergerSlotItems[i].displayName}");
                    // 필요 시 추가 처리 (예: UI 알림)
                }
            }
            _mergerSlotItems[i] = null;
        }
    }

    private void RestoreCrafterItems()
    {
        for (int i = 0; i < _crafterSlotItems.Length; i++)
        {
            if (_crafterSlotItems[i] != null)
            {
                if (_inventory.AddItem(_crafterSlotItems[i]))
                {
                    Debug.Log($"Restored {_crafterSlotItems[i].displayName} to inventory");
                }
                else
                {
                    Debug.LogWarning($"Failed to restore {_crafterSlotItems[i].displayName}: Inventory full");
                    // 필요 시 추가 처리 (예: UI 알림)
                }
                _crafterSlotItems[i] = null;
            }
        }
    }

    private void CloseActivePanel()
    {
        if (_activePanel == null) return;

        // 패널 닫기 전 아이템 복원
        if (_mergerSlots != null)
        {
            RestoreMergerItems();
        }
        else if (_crafterSlots != null)
        {
            RestoreCrafterItems();
        }

        _activePanel.SetActive(false);
        _activePanel = null;
        _mergerSlots = null;
        _crafterSlots = null;
        UpdateUI();
        Debug.Log("Closed active panel");

        if (IsInventoryOpen())
        {
            ToggleInventory();
        }
    }

    private void UpdateUI()
    {
        // 인벤토리 그리드 갱신
        for (int y = 0; y < _inventory.MaxGridHeight; y++)
        {
            for (int x = 0; x < _inventory.GridWidth; x++)
            {
                bool isLocked = y >= _inventory.ActiveGridHeight;
                _gridButtons[x, y].interactable = !isLocked;
                ItemDataSO item = _inventory.GetGridItem(x, y);
                _gridIcons[x, y].sprite = item?.icon;
                _gridIcons[x, y].color = isLocked ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : (item != null ? Color.white : Color.clear);
                _gridTexts[x, y].text = item?.displayName ?? "";
            }
        }

        // 병합기 패널 갱신
        if (_activePanel != null && _activePanel.activeSelf && _mergerSlots != null)
        {
            for (int i = 0; i < 2; i++)
            {
                var item = _mergerSlotItems[i];
                _mergerSlots[i].sprite = item?.icon;
                //_mergerSlots[i].color = item != null ? Color.white : Color.clear;
                _mergerSlots[i].color = Color.white;
            }
            var item1 = _mergerSlotItems[0];
            _mergerResultText.text = item1?.nextElementLevel != null ? $"Result: {item1.nextElementLevel.displayName}" : "";
        }

        // 조합기 패널 갱신
        if (_activePanel != null && _activePanel.activeSelf && _crafterSlots != null)
        {
            for (int i = 0; i < 2; i++)
            {
                var item = _crafterSlotItems[i];
                _crafterSlots[i].sprite = item?.icon;
                //_crafterSlots[i].color = item != null ? Color.white : Color.clear;
                _crafterSlots[i].color = Color.white;
            }
        }
    }

    private void OnSlotClicked(int x, int y)
    {
        if (y >= _inventory.ActiveGridHeight) return; // 비활성 슬롯 클릭 무시
        _inventory.DecomposeItem(x, y);
        UpdateUI();
    }


    public void ToggleInventory()
    {
        _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);
        if (_inventoryPanel.activeSelf)
        {
            UpdateUI();
        }
    }

    public bool IsInventoryOpen()
    {
        return _inventoryPanel.activeSelf;
    }

    public void OnSlotDropped(int fromX, int fromY, int toX, int toY)
    {
        if (fromY >= _inventory.ActiveGridHeight || toY >= _inventory.ActiveGridHeight) return; // 비활성 슬롯 드롭 무시
        _inventory.SwapItems(fromX, fromY, toX, toY);
        UpdateUI();
    }

    public void OnItemDeleted(int x, int y)
    {
        if (y >= _inventory.ActiveGridHeight) return;
        _inventory.RemoveItem(x, y);
        UpdateUI();
        Debug.Log($"Deleted item at ({x}, {y})");
    }
}

public class SlotDrag: MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    InventoryUI _inventoryUI;
    int _slotX, _slotY;
    RectTransform _rectTransform;
    CanvasGroup _canvasGroup;
    //Canvas _canvas;

    public int SlotX => _slotX;
    public int SlotY => _slotY;

    public void Setup(InventoryUI inventoryUI, int x, int y)
    {
        _inventoryUI = inventoryUI;
        _slotX = x;
        _slotY = y;
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1f;
        _rectTransform.anchoredPosition = Vector2.zero; // 프리뷰 정리

        var dropTarget = eventData.pointerEnter?.GetComponent<SlotDrop>();

        if (dropTarget != null && dropTarget.SlotX >= 0 && dropTarget.SlotY >= 0)
        {
            _inventoryUI.OnSlotDropped(_slotX, _slotY, dropTarget.SlotX, dropTarget.SlotY);
        }
        // Crafter/Merger 드롭은 SlotDrop의 OnDropCallback에서 처리
    }
}


public class SlotDrop: MonoBehaviour, IDropHandler
{
    InventoryUI _inventoryUI;
    int _slotX, _slotY;

    public System.Action OnDropCallback;
    public int SlotX => _slotX;
    public int SlotY => _slotY;

    public void Setup(InventoryUI inventoryUI, int x, int y)
    {
        _inventoryUI = inventoryUI;
        _slotX = x;
        _slotY = y;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var slotDrag = eventData.pointerDrag?.GetComponent<SlotDrag>();
        if (slotDrag == null) return;

        if (OnDropCallback != null)
        {
            OnDropCallback.Invoke(); // Crafter/Merger용 커스텀 처리
        }
        else if (_slotX >= 0 && _slotY >= 0)
        {
            _inventoryUI.OnSlotDropped(slotDrag.SlotX, slotDrag.SlotY, _slotX, _slotY);
        }
    }
}