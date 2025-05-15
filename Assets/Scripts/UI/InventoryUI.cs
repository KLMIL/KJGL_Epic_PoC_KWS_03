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
        DontDestroyOnLoad(gameObject.transform.parent.gameObject);
        InitializeUI();
        _inventoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _activePanel != null && _activePanel.activeSelf)
        {
            CloseActivePanel();
        }
    }

    private void InitializeUI()
    {
        _gridButtons = new Button[4, 4];
        _gridIcons = new Image[4, 4];
        _gridTexts = new TextMeshProUGUI[4, 4];

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                int index = y * 4 + x;
                GameObject slot = _gridLayout.transform.GetChild(index).gameObject;
                _gridButtons[x, y] = slot.GetComponent<Button>();
                _gridIcons[x, y] = slot.GetComponent<Image>();
                _gridTexts[x, y] = slot.GetComponentInChildren<TextMeshProUGUI>();

                int slotX = x, slotY = y;
                _gridButtons[x, y].onClick.AddListener(() => OnSlotClicked(slotX, slotY));
                var slotDrag = slot.AddComponent<SlotDrag>();
                slotDrag.Setup(this, slotX, slotY);
                var slotDrop = slot.AddComponent<SlotDrop>();
                slotDrop.Setup(this, slotX, slotY);
            }
        }
    }

    public void ShowMergerPanel(GameObject panel)
    {
        _activePanel = panel;
        _activePanel.SetActive(true);
        _inventoryPanel.SetActive(true);
        InitializeMergerPanel();
        UpdateUI();
    }

    public void ShowCrafterPanel(GameObject panel)
    {
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
    }

    private void UpdateUI()
    {
        // 인벤토리 그리드 갱신
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                ItemDataSO item = _inventory.GetGridItem(x, y);
                _gridIcons[x, y].sprite = item?.icon;
                _gridIcons[x, y].color = item != null ? Color.white : Color.clear;
                _gridTexts[x, y].text = item?.displayName ?? "";
            }
        }

        // 병합기 패널 갱신
        if (_activePanel != null && _activePanel.activeSelf)
        {
            if (_mergerSlots != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    var item = _mergerSlotItems[i];
                    _mergerSlots[i].sprite = item?.icon;
                    _mergerSlots[i].color = item != null ? Color.white : Color.white; // : Color.clear;
                }
                var item1 = _mergerSlotItems[0];
                _mergerResultText.text = item1?.nextElementLevel != null ? $"Result: {item1.nextElementLevel.displayName}" : "";
            }
            else if (_crafterSlots != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    var item = _crafterSlotItems[i];
                    _crafterSlots[i].sprite = item?.icon;
                    _crafterSlots[i].color = item != null ? Color.white : Color.white; // : Color.clear;
                }
            }
        }
    }

    private void OnSlotClicked(int x, int y)
    {
        _inventory.DecomposeItem(x, y);
        UpdateUI();
    }


    public void ToggleInventory()
    {
        //if (_activePanel != null && _activePanel.activeSelf)
        //{
        //    _activePanel.SetActive(false);
        //}
        _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);
        if (_inventoryPanel.activeSelf)
        {
            UpdateUI();
        }
    }

    public void OnSlotDropped(int fromX, int fromY, int toX, int toY)
    {
        _inventory.SwapItems(fromX, fromY, toX, toY);
        UpdateUI();
    }

    public void OnItemDeleted(int x, int y)
    {
        _inventory.RemoveItem(x, y);
        UpdateUI();
        Debug.Log($"Deleted item at ({x}, {y})");
    }

    /*
    private void DEP_UpdateUI()
    {
        // 그리드 업데이트
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                ItemDataSO item = _inventory.GetGridItem(x, y);
                if (item != null)
                {
                    _gridIcons[x, y].sprite = item.icon;
                    _gridIcons[x, y].color = Color.white;
                    _gridTexts[x, y].text = item.displayName;
                }
                else
                {
                    _gridIcons[x, y].sprite = null;
                    _gridIcons[x, y].color = Color.clear;
                    _gridTexts[x, y].text = "";
                }
            }
        }

        // 원소 목록 업데이트
        var elements = _inventory.GetElements();
        int index = 0;
        foreach (var pair in elements)
        {
            if (index >= _elementTexts.Length)
            {
                Debug.LogWarning("Too many element types for UI display");
                break;
            }

            if (_elementTexts[index] == null)
            {
                GameObject textObj = new GameObject($"Element {index}");
                textObj.transform.SetParent(_elementLayout.transform);
                _elementTexts[index] = textObj.AddComponent<TextMeshProUGUI>();
                _elementTexts[index].fontSize = 20;
                _elementTexts[index].color = Color.white;
            }

            ItemDataSO elementData = _inventory.GetElementData(pair.Key);
            _elementTexts[index].text = elementData != null ? $"{elementData.displayName} x{pair.Value}" : "";
            index++;
        }

        for (; index < _elementTexts.Length; index++)
        {
            if (_elementTexts[index] != null)
            {
                _elementTexts[index].text = "";
            }
        }

        // 합성 레시피 업데이트
        var recipes = _inventory.GetCraftRecipes();
        index = 0;

        foreach (var recipe in recipes)
        {
            if (index >= _recipeButtons.Length)
            {
                Debug.LogWarning("Too many recipes for UI display");
                break;
            }

            if (_recipeButtons[index] == null)
            {
                // 버튼 프리펩 사용 고려 
                // 버튼 생성
                GameObject buttonObj = new GameObject($"Recipe {index}");
                buttonObj.transform.SetParent(_recipeLayout.transform, false);
                _recipeButtons[index] = buttonObj.AddComponent<Button>();
                Image buttonImage = buttonObj.AddComponent<Image>();
                buttonImage.color = Color.gray;

                // 텍스트를 자식 GameObject에 추가 
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform, false);
                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.fontSize = 18;
                text.color = Color.white;
                text.alignment = TextAlignmentOptions.Center;
                text.text = recipe.recipeName;

                // 텍스트 위치 조정
                RectTransform textRect = text.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;                                                                                                                                                   
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                // 버튼 크기 조정
                RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
                buttonRect.sizeDelta = new Vector2(180, 40);
            }
            else
            {
                // 기존 텍스트 업데이트
                var textComponent = _recipeButtons[index].GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent == null)
                {
                    Debug.LogError($"레시피 버튼 {index}에 TextMeshProUGUI가 없습니다!");
                    // 자식에 새 텍스트 추가
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(_recipeButtons[index].transform, false);
                    textComponent = textObj.AddComponent<TextMeshProUGUI>();
                    textComponent.fontSize = 18;
                    textComponent.color = Color.white;
                    textComponent.alignment = TextAlignmentOptions.Center;

                    RectTransform textRect = textComponent.GetComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = Vector2.zero;
                    textRect.offsetMax = Vector2.zero;
                }
                textComponent.text = recipe.recipeName;
                _recipeButtons[index].gameObject.SetActive(true);
            }

            int recipeIndex = index;
            _recipeButtons[index].onClick.RemoveAllListeners();
            _recipeButtons[index].onClick.AddListener(() => OnRecipeClicked(recipeIndex));
            index++;
        }

        for (; index < _recipeButtons.Length; index++)
        {
            if (_recipeButtons[index] != null)
            {
                _recipeButtons[index].gameObject.SetActive(false);
            }
        }

        _inventory.PrintInventory();
    }

    private void DEP_OnSlotClicked(int x, int y)
    {
        _inventory.DecomposeItem(x, y);
        UpdateUI();
    }

    private void DEP_OnMergeClicked()
    {
        _inventory.MergeElements();
        UpdateUI();
    }

    private void DEP_OnRecipeClicked(int recipeIndex)
    {
        var recipes = _inventory.GetCraftRecipes();
        if (recipeIndex < recipes.Length)
        {
            _inventory.CraftItem(recipes[recipeIndex]);
            UpdateUI();
        }
    }

    */
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