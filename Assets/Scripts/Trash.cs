/**********************************************************
 * Script Name: Trash
 * Author: 김우성
 * Date Created: 2025-05-14
 * Last Modified: 2025-05-14
 * Description: 
 * - 쓰레기 오브젝트로, 플레이어가 상호작용 시 지정된 아이템을 획득.
 * - 프리팹으로 배치 가능, 인스펙터에서 아이템 설정.
 *********************************************************/

using UnityEngine;

public class Trash : MonoBehaviour
{
    [Header("Trash Settings")]
    [SerializeField] string _itemID = "DefaultItem";
    /* 추후 ScriptableObject로 확장 */

    public string ItemID => _itemID;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.DrawWireSphere(transform.position, collider.bounds.extents.magnitude);
        }
    }
}
