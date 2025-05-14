/**********************************************************
 * Script Name: Inventory
 * Author: 김우성
 * Date Created: 2025-05-14
 * Last Modified: 2025-05-14
 * Description: 
 * - 그리드형 인벤토리 관리.
 * - 아이템 추가, 원소 분해, 원소 병합 기능.
 * - 씬 전환 시 유지.
 *********************************************************/

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] int _gridWidth = 8;
    [SerializeField] int _gridHeight = 8;
    [SerializeField] ItemDataSO[] _decomposeResults; // 분해 레시피
    [SerializeField] CraftRecipeSO[] _craftRecipes; // 합성 레시피

    ItemDataSO[,] _grid;
    Dictionary<string, int> _elements; // 원소 ID와 수량
    Dictionary<string, ItemDataSO> _elementDataCache; // ID로 ItemData 캐싱

    private void Awake()
    {
        _grid = new ItemDataSO[_gridWidth, _gridHeight];
        _elements = new Dictionary<string, int>();
        _elementDataCache = new Dictionary<string, ItemDataSO>();
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
        Debug.LogWarning("Inventory is full");
        return false;
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

        if (_decomposeResults.Length == 0)
        {
            Debug.LogWarning("No decompose results configured");
            return;
        }

        /* 임시로 랜덤 원소 추가. 추후 item에 따라 정의할 것 */
        ItemDataSO element = _decomposeResults[Random.Range(0, _decomposeResults.Length)];
        //_elements.Add(element);
        if (!_elements.ContainsKey(element.ID))
        {
            _elements[element.ID] = 0;
            _elementDataCache[element.ID] = element;
        }
        _elements[element.ID]++;
        Debug.Log($"Decomposed {item.displayName} into {element.displayName}");
    }

    // 원소 병합
    public void MergeElements()
    {
        if (_elements.Count < 2)
        {
            Debug.Log("Not enough elements to merge");
            return;
        }

        Debug.Log($"Elements before merge: {string.Join(", ", _elements.Select(e => $"{_elementDataCache[e.Key].displayName} (ID: {e.Value})"))}");

        bool merged = false;
        var keys = _elements.Keys.ToList();

        foreach (var id in keys)
        {
            ItemDataSO element = _elementDataCache[id];
            if (_elements[id] >= 2 && element.nextElementLevel != null)
            {
                int mergeCount = _elements[id] / 2;
                _elements[id] -= mergeCount * 2;

                string nextID = element.nextElementLevel.ID;
                if (!_elements.ContainsKey(nextID))
                {
                    _elements[nextID] = 0;
                    _elementDataCache[nextID] = element.nextElementLevel;
                }
                _elements[nextID] += mergeCount;

                Debug.Log($"Merged {mergeCount * 2}x {element.displayName} into {mergeCount}x {element.nextElementLevel.displayName}");
                merged = true;

                if (_elements[id] == 0)
                {
                    _elements.Remove(id);
                }
            }
        }

        if (!merged)
        {
            Debug.Log("No mergeable elements found");
        }
        Debug.Log($"Elements after merge: {string.Join(", ", _elements.Select(e => $"{_elementDataCache[e.Key].displayName} x{e.Value}"))}");
    }

    public bool CraftItem(CraftRecipeSO recipe)
    {
        // 필요 원소 체크
        foreach (var req in recipe.requiredElements)
        {
            if (!_elements.ContainsKey(req.elementID) || _elements[req.elementID] < req.quantity)
            {
                Debug.LogWarning($"Insufficient {req.elementID} for crafting {recipe.recipeName} (Need: {req.quantity}, Have: {_elements.GetValueOrDefault(req.elementID)})");
                return false;
            }
        }

        // 원소 소모
        foreach (var req in recipe.requiredElements)
        {
            _elements[req.elementID] -= req.quantity;
            if (_elements[req.elementID] == 0)
            {
                _elements.Remove(req.elementID);
            }
        }

        // 결과 아이템 추가
        if (AddItem(recipe.resultItem))
        {
            Debug.Log($"Crafted {recipe.resultItem.displayName} using {recipe.recipeName}");
            return true;
        }
        else
        {
            // 인벤토리 부족하면 원소 복구
            foreach (var req in recipe.requiredElements)
            {
                if (!_elements.ContainsKey(req.elementID))
                {
                    _elements[req.elementID] = 0;
                }
                _elements[req.elementID] += req.quantity;
            }
            Debug.LogWarning($"Failed to craft {recipe.recipeName}: Invnetory full");
            return false;
        }
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

    public Dictionary<string, int> GetElements()
    {
        return _elements;
    }

    public ItemDataSO GetElementData(string id)
    {
        return _elementDataCache.ContainsKey(id) ? _elementDataCache[id] : null;
    }

    public CraftRecipeSO[] GetCraftRecipes()
    {
        return _craftRecipes;
    }

    // 디버깅: 인벤토리 상태 출력
    public void PrintInventory()
    {
        string output = "Inventory:\n";
        for (int y = 0; y < _gridHeight; y++)
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                output += _grid[x, y] == null ? "[Empty] " : $"[{_grid[x, y].displayName}]";
            }
            output += "\n";
        }
        output += "Elements: " + string.Join(", ", _elements.Select(e => $"{_elementDataCache[e.Key].displayName} x{e.Value}"));
        Debug.Log(output);
    }
}
