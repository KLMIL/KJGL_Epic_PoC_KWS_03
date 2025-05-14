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
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] int _gridWidth = 8;
    [SerializeField] int _gridHeight = 8;
    [SerializeField] ItemData[] _decomposeResults;

    ItemData[,] _grid;
    List<ItemData> _elements;

    private void Awake()
    {
        _grid = new ItemData[_gridWidth, _gridHeight];
        _elements = new List<ItemData>();
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
        if (_grid[x, y] == null || _grid[x, y].IsElement)
        {
            Debug.LogWarning("Invalid item for decomposition");
            return;
        }

        ItemData item = _grid[x, y];
        _grid[x, y] = null;

        /* 임시로 랜덤 원소 추가. 추후 item에 따라 정의할 것 */
        ItemData element = _decomposeResults[Random.Range(0, _decomposeResults.Length)];
        _elements.Add(element);
        Debug.Log($"Decomposed {item.DisplayName} into {element.DisplayName}");
    }

    // 원소 병합
    public void MergeElements()
    {
        Debug.Log("MergeElements called");
        for (int i = _elements.Count - 1; i >= 0; i--)
        {
            ItemData element = _elements[i];
            if (element.NextElementLevel == null) continue;

            Debug.Log("MergeElements started");
            // 동일 원소 2개 찾아서 제거
            int count = _elements.FindAll(e => e.ID == element.ID).Count;
            if (count >= 2)
            {
                for (int j = 0; j < 2; j++)
                {
                    _elements.Remove(element);
                }

                // 상위 레벨 원소 추가
                _elements.Add(element.NextElementLevel);
                Debug.Log($"Merged {element.DisplayName} into {element.NextElementLevel.DisplayName}");
            }
        }
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
        output += "Elements: " + string.Join(", ", _elements.ConvertAll(e => e.DisplayName));
        Debug.Log(output);
    }
}
