/**********************************************************
 * Script Name: ItemData
 * Author: 김우성
 * Date Created: 2025-05-14
 * Last Modified: 2025-05-14
 * Description: 
 * - 쓰레기와 원소의 데이터를 정의하는 ScriptableObject.
 * - 인벤토리와 상호작용에서 사용.
 *********************************************************/

using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/ItemData")]
public class ItemDataSO : ScriptableObject
{
    public string ID; // 고유 ID (ex, PlasticBottle)
    public string displayName; // 표시 이름 (ex, Plastic Bottle
    public Sprite icon; // UI용 아이콘
    public bool isElement; // 원소 여부
    public int elementLevel; // 원소 레벨. 0: 일반 아이템, 1+: 원소
    public ItemDataSO nextElementLevel; // 병합 시 다음 레벨 원소
}
