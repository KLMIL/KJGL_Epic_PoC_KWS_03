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
    [SerializeField] VerticalLayoutGroup _recipeLayout;
    [SerializeField] Button _mergeButton;
    [SerializeField] Inventory _inventory;

    Button[,] _gridButtons;
    Image[,] _gridIcons;
    TextMeshProUGUI[,] _gridTexts;
    TextMeshProUGUI[] _elementTexts;
    Button[] _recipeButtons;

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
        _recipeButtons = new Button[10];

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
                /* 버튼 프리펩 사용 고려 */
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

    private void OnRecipeClicked(int recipeIndex)
    {
        var recipes = _inventory.GetCraftRecipes();
        if (recipeIndex < recipes.Length)
        {
            _inventory.CraftItem(recipes[recipeIndex]);
            UpdateUI();
        }
    }
}
