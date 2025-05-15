/**********************************************************
 * Script Name: TrashItem
 * Author: 김우성
 * Date Created: 2025-05-15
 * Last Modified: 2025-05-15
 * Description: 
 * - 쓰레기 오브젝트로, 플레이어가 상호작용 시 지정된 아이템을 획득.
 * - 프리팹으로 배치 가능, 인스펙터에서 아이템 설정.
 * - 기존 Trash의 기능 추가 및 변경을 위한 스크립트
 *********************************************************/

using UnityEngine;

public class TrashItem : MonoBehaviour
{
    [SerializeField] ItemDataSO _item;
    [SerializeField] SpriteRenderer _spriteRenderer;

    public ItemDataSO Item => _item;

    public void SetItem(ItemDataSO item)
    {
        _item = item;
        if (_spriteRenderer != null && item != null)
        {
            _spriteRenderer.sprite = item.icon;
        }
        Debug.Log($"TrashItem set to {item.displayName}");
    }
}
