using UnityEngine;

[CreateAssetMenu(fileName = "HealingPotion", menuName = "Item/HealingPotion")]
public class HealingPotion : Item
{
    public int healAmount = 50;
    
    public override bool Use()
    {
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.Heal(healAmount);
            Debug.Log($"Used healing potion, healed {healAmount} HP!");
            return true; // Item was consumed
        }
        return false;
    }
}