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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] GameObject _inventoryPanel;
    [SerializeField] GridLayoutGroup _gridLayout;
    [SerializeField] VerticalLayoutGroup _elementLayout;
    [SerializeField] Button _mergeButton;
    [SerializeField] Inventory _inventory;

    Button[,] _gridButtons;
    Image[,] _gridIcons;
    TextMeshProUGUI[,] _gridTexts;
    TextMeshProUGUI[] _elementTexts;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitializeUI();
        _inventoryPanel.SetActive(false);
    }

    private void InitializeUI()
    {
        _gridButtons = new Button[4, 4];
        _gridIcons = new Image[4, 4];
        _gridTexts = new TextMeshProUGUI[4, 4];
        _elementTexts = new TextMeshProUGUI[20];

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
            }
        }

        _mergeButton.onClick.AddListener(OnMergeClicked);
    }

    public void ToggleInventory()
    {
        _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);
        if (_inventoryPanel.activeSelf)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        // 그리드 업데이트
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                ItemData item = _inventory.GetGridItem(x, y);
                if (item != null)
                {
                    _gridIcons[x, y].sprite = item.Icon;
                    _gridIcons[x, y].color = Color.white;
                    _gridTexts[x, y].text = item.DisplayName;
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

            ItemData elementData = _inventory.GetElementData(pair.Key);
            _elementTexts[index].text = elementData != null ? $"{elementData.DisplayName} x{pair.Value}" : "";
            index++;
        }

        for (; index < _elementTexts.Length; index++)
        {
            if (_elementTexts[index] != null)
            {
                _elementTexts[index].text = "";
            }
        }

        _inventory.PrintInventory();
    }

    private void OnSlotClicked(int x, int y)
    {
        _inventory.DecomposeItem(x, y);
        UpdateUI();
    }

    private void OnMergeClicked()
    {
        _inventory.MergeElements();
        UpdateUI();
    }
}
