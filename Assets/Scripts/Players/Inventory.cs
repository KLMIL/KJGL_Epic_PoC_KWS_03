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
    [SerializeField] ItemData[] _decomposeResults;

    ItemData[,] _grid;
    //List<ItemData> _elements;
    Dictionary<string, int> _elements; // 원소 ID와 수량
    Dictionary<string, ItemData> _elementDataCache; // ID로 ItemData 캐싱

    private void Awake()
    {
        _grid = new ItemData[_gridWidth, _gridHeight];
        //_elements = new List<ItemData>();
        _elements = new Dictionary<string, int>();
        _elementDataCache = new Dictionary<string, ItemData>();
        DontDestroyOnLoad(gameObject); // 씬 변경시 아이템 유지
    }

    // 아이템 추가
    public bool AddItem(ItemData item)
    {
        for (int y = 0; y  < _gridHeight; y++)
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                if (_grid[x, y] == null)
                {
                    _grid[x, y] = item;
                    Debug.Log($"Added {item.DisplayName} to inventory at ({x}, {y})");
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
        if (_grid[x, y].IsElement)
        {
            Debug.LogWarning($"Item at ({x}, {y}) is already an element: {_grid[x, y].DisplayName}");
            return;
        }

        ItemData item = _grid[x, y];
        _grid[x, y] = null;

        if (_decomposeResults.Length == 0)
        {
            Debug.LogWarning("No decompose results configured");
            return;
        }

        /* 임시로 랜덤 원소 추가. 추후 item에 따라 정의할 것 */
        ItemData element = _decomposeResults[Random.Range(0, _decomposeResults.Length)];
        //_elements.Add(element);
        if (!_elements.ContainsKey(element.ID))
        {
            _elements[element.ID] = 0;
            _elementDataCache[element.ID] = element;
        }
        _elements[element.ID]++;
        Debug.Log($"Decomposed {item.DisplayName} into {element.DisplayName}");
    }

    // 원소 병합
    public void MergeElements()
    {
        if (_elements.Count < 2)
        {
            Debug.Log("Not enough elements to merge");
            return;
        }

        Debug.Log($"Elements before merge: {string.Join(", ", _elements.Select(e => $"{_elementDataCache[e.Key].DisplayName} (ID: {e.Value})"))}");

        bool merged = false;
        var keys = _elements.Keys.ToList();

        foreach (var id in keys)
        {
            ItemData element = _elementDataCache[id];
            if (_elements[id] >= 2 && element.NextElementLevel != null)
            {
                int mergeCount = _elements[id] / 2;
                _elements[id] -= mergeCount * 2;

                string nextID = element.NextElementLevel.ID;
                if (!_elements.ContainsKey(nextID))
                {
                    _elements[nextID] = 0;
                    _elementDataCache[nextID] = element.NextElementLevel;
                }
                _elements[nextID] += mergeCount;

                Debug.Log($"Merged {mergeCount * 2}x {element.DisplayName} into {mergeCount}x {element.NextElementLevel.DisplayName}");
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
        Debug.Log($"Elements after merge: {string.Join(", ", _elements.Select(e => $"{_elementDataCache[e.Key].DisplayName} x{e.Value}"))}");



        //var groups = _elements
        //    .GroupBy(e => e.ID)
        //    .Where(g => g.Count() >= 2 && g.First().NextElementLevel != null)
        //    .ToList();

        //if (groups.Count == 0)
        //{
        //    Debug.Log("No mergeable elements found (check ID matches and nextElementLevel).");
        //    return;
        //}


        //foreach (var group in groups)
        //{
        //    ItemData element = group.First();
        //    ItemData nextLevel = element.NextElementLevel;
        //    int mergeCount = group.Count() / 2;

        //    for (int i = 0; i < mergeCount; i++)
        //    {
        //        _elements.Remove(element);
        //        _elements.Remove(element);
        //        _elements.Add(nextLevel);
        //        Debug.Log($"Merged 2x {element.DisplayName} into {nextLevel.DisplayName}");
        //    }
        //}

        //Debug.Log($"Elements after merge: {string.Join(", ", _elements.Select(e => $"{e.DisplayName} (ID: {e.ID})"))}");
    }

    // UI용 데이터 접근 함수
    public ItemData GetGridItem(int x, int y)
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

    public ItemData GetElementData(string id)
    {
        return _elementDataCache.ContainsKey(id) ? _elementDataCache[id] : null;
    }

    // 디버깅: 인벤토리 상태 출력
    public void PrintInventory()
    {
        string output = "Inventory:\n";
        for (int y = 0; y < _gridHeight; y++)
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                output += _grid[x, y] == null ? "[Empty] " : $"[{_grid[x, y].DisplayName}]";
            }
            output += "\n";
        }
        output += "Elements: " + string.Join(", ", _elements.Select(e => $"{_elementDataCache[e.Key].DisplayName} x{e.Value}"));
        Debug.Log(output);
    }
}
