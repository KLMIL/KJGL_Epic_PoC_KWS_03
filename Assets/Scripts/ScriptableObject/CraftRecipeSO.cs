/**********************************************************
 * Script Name: CraftRecipe
 * Author: 김우성
 * Date Created: 2025-05-14
 * Last Modified: 2025-05-14
 * Description: 
 * - 합성 레시피 데이터.
 * - 필요 원소와 결과 아이템 정의.
 *********************************************************/

using UnityEngine;

[CreateAssetMenu(fileName = "New Craft Recipe", menuName = "Inventory/CraftRecipe")]
public class CraftRecipeSO : ScriptableObject
{
    public string recipeName; // 레시피 이름
    public ItemDataSO resultItem; // 결과 아이템
    public ElementRequirement[] requiredElements; // 필요 원소

    [System.Serializable]
    public class ElementRequirement
    {
        public string elementID; // 원소 ID
        public int quantity; // 필요 수량
    }
}
