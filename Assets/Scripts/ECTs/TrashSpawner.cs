/**********************************************************
 * Script Name: TrashSpawner
 * Author: 김우성
 * Date Created: 2025-05-15
 * Last Modified: 2025-05-15
 * Description: 
 * - 맵에 주기적으로 쓰레기를 스폰하는 스크립트
 *********************************************************/

using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    [SerializeField] List<ItemDataSO> _trashItems;
    [SerializeField] GameObject _trashPrefab;
    [SerializeField] Vector2 _spawnAreaMin;
    [SerializeField] Vector2 _spawnAreaMax;
    [SerializeField] float _spawnInterval = 10f;
    [SerializeField] int _maxTrashCount = 10;

    List<GameObject> _spawnedTrash = new List<GameObject>();
    float _timer;

    private void Start()
    {
        _timer = _spawnInterval;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0 && _spawnedTrash.Count < _maxTrashCount)
        {
            SpawnTrash();
            _timer = _spawnInterval;
        }
    }

    private void SpawnTrash()
    {
        // 랜덤 위치 계산
        float x = Random.Range(_spawnAreaMin.x, _spawnAreaMax.x);
        float y = Random.Range(_spawnAreaMin.y, _spawnAreaMax.y);
        Vector3 spawnPos = new Vector3(x, y, 0);

        // 랜덤 쓰레기 아이템 선택
        ItemDataSO item = _trashItems[Random.Range(0, _trashItems.Count)];

        // 쓰레기 인스턴스 생성
        GameObject trash = Instantiate(_trashPrefab, spawnPos, Quaternion.identity);
        TrashItem trashItem = trash.GetComponent<TrashItem>();
        if (trashItem != null)
        {
            trashItem.SetItem(item);
        }
        _spawnedTrash.Add(trash);

        Debug.Log($"Spawned {item.displayName} at {spawnPos}");
    }

    public void RemoveTrash(GameObject trash)
    {
        _spawnedTrash.Remove(trash);
        Destroy(trash);
    }
}
