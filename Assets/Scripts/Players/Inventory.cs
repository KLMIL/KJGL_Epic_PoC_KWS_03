/**********************************************************
 * Script Name: Inventory
 * Author: 김우성
 * Date Created: 2025-05-14
 * Last Modified: 2025-05-15
 * Description: 
 * - 그리드형 인벤토리 관리.
 * - 아이템 추가, 원소 분해, 원소 병합 기능. -> 분리예정
 * - 씬 전환 시 유지.
 *********************************************************/

using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] int _gridWidth = 4;
    [SerializeField] int _gridHeight = 4;
    ItemDataSO[,] _grid;


    // DEP 예정
    //[SerializeField] ItemDataSO[] _decomposeResults; // 분해 레시피
    [SerializeField] CraftRecipeSO[] _craftRecipes; // 합성 레시피

    //Dictionary<string, int> _elements; // 원소 ID와 수량
    //Dictionary<string, ItemDataSO> _elementDataCache; // ID로 ItemData 캐싱

    private void Awake()
    {
        _grid = new ItemDataSO[_gridWidth, _gridHeight];
        //_elements = new Dictionary<string, int>();
        //_elementDataCache = new Dictionary<string, ItemDataSO>();
        DontDestroyOnLoad(gameObject); // 씬 변경시 아이템 유지
    }

    // 아이템 추가
    public bool AddItem(ItemDataSO item)
    {
        for (int y = 0; y  < _gridHeight; y++)
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                if (_grid[x, y] == null)
                {
                    _grid[x, y] = item;
                    Debug.Log($"Added {item.displayName} to inventory at ({x}, {y})");
                    return true;
                }
            }
        }
        Debug.LogWarning($"Inventory is full. cannot add {item.displayName}");
        return false;
    }

    public void RemoveItem(int x, int y)
    {
        if (x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight)
        {
            Debug.LogWarning($"Invalid grid position: ({x}, {y})");
            return;
        }
        if (_grid[x, y] != null)
        {
            Debug.Log($"Removed item: {_grid[x, y].displayName} at ({x}, {y})");
            _grid[x, y] = null;
        }
    }

    public void SwapItems(int fromX, int fromY, int toX, int toY)
    {
        if (fromX < 0 || fromX >= _gridWidth || fromY < 0 || fromY >= _gridHeight ||
            toX < 0 || toX >= _gridWidth || toY < 0 || toY >= _gridHeight)
        {
            Debug.LogWarning($"Invalid grid positions: ({fromX},{fromY}) to ({toX},{toY})");
            return;
        }
        (_grid[fromX, fromY], _grid[toX, toY]) = (_grid[toX, toY], _grid[fromX, fromY]);
        Debug.Log($"Swapped items from ({fromX},{fromY}) to ({toX},{toY})");
    }

    // 원소 분해
    public void DecomposeItem(int x, int y)
    {
        if (x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight)
        {
            Debug.LogWarning($"Invalid grid position: ({x}, {y})");
            return;
        }
        if (_grid[x, y] == null)
        {
            Debug.LogWarning($"No item at grid position ({x}, {y})");
            return;
        }
        if (_grid[x, y].isElement)
        {
            Debug.LogWarning($"Item at ({x}, {y}) is already an element: {_grid[x, y].displayName}");
            return;
        }

        ItemDataSO item = _grid[x, y];
        _grid[x, y] = null;

        if (item.decomposeResults == null || item.decomposeResults.Length == 0)
        {
            Debug.LogWarning($"No decompose results defined for {item.displayName}");
            return;
        }

        ///* 임시로 랜덤 원소 추가. 추후 item에 따라 정의할 것 */
        ItemDataSO element = item.decomposeResults[Random.Range(0, item.decomposeResults.Length)];
        if (element == null)
        {
            Debug.LogError($"Selected decompose reuslt is null for {item.displayName}.");
            return;
        }

        if (!AddItem(element))
        {
            Debug.LogWarning($"Failed to add decomposed element {element.displayName}: Inventory full");
            return;
        }

        Debug.Log($"Decomposed {item.displayName} into {element.displayName}");
    }

    // 원소 병합
    public bool MergeItems(ItemDataSO item1, ItemDataSO item2)
    {
        if (item1 == null || item2 == null || item1 != item2 || item1.nextElementLevel == null)
        {
            Debug.LogWarning("Cannot merge: Invalid or different items, or no next level");
            return false;
        }

        if (AddItem(item1.nextElementLevel))
        {
            Debug.Log($"Merged {item1.displayName} x2 into {item1.nextElementLevel.displayName}");
            return true;
        }
        Debug.LogWarning($"Failed to merge: Inventory full");
        return false;
    }

    // 아이템 제작
    public bool CraftItem(CraftRecipeSO recipe, ItemDataSO item1, ItemDataSO item2)
    {
        if (item1 == null || item2 == null)
        {
            Debug.LogWarning("Empty items selected");
            return false;
        }

        bool match = recipe.requiredElements.Length == 2 &&
                     ((recipe.requiredElements[0].elementID == item1.ID && recipe.requiredElements[1].elementID == item2.ID) ||
                      (recipe.requiredElements[0].elementID == item2.ID && recipe.requiredElements[1].elementID == item1.ID));

        if (!match)
        {
            Debug.Log($"Items do not match recipe: {recipe.recipeName}");
            return false;
        }

        if (AddItem(recipe.resultItem))
        {
            Debug.Log($"Crafted {recipe.resultItem.displayName} using {recipe.recipeName}");
            return true;
        }
        Debug.LogWarning($"Failed to craft: Inventory full");
        return false;
    }

    // UI용 데이터 접근 함수
    public ItemDataSO GetGridItem(int x, int y)
    {
        if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight)
        {
            return _grid[x, y];
        }
        return null;
    }


    public CraftRecipeSO[] GetCraftRecipes()
    {
        return _craftRecipes;
    }

    // 디버깅: 인벤토리 상태 출력
    //public void PrintInventory()
    //{
    //    string output = "Inventory:\n";
    //    for (int y = 0; y < _gridHeight; y++)
    //    {
    //        for (int x = 0; x < _gridWidth; x++)
    //        {
    //            output += _grid[x, y] == null ? "[Empty] " : $"[{_grid[x, y].displayName}]";
    //        }
    //        output += "\n";
    //    }
    //    output += "Elements: " + string.Join(", ", _elements.Select(e => $"{_elementDataCache[e.Key].displayName} x{e.Value}"));
    //    Debug.Log(output);
    //}
}
