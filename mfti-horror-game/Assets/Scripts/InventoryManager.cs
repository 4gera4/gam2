using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [Header("Inventory")]
    public List<string> keys = new List<string>();
    public List<string> items = new List<string>();
    
    [Header("Special Items")]
    public bool hasCalculator = false;
    public bool hasCheatSheet = false;
    public bool hasEnergyDrink = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AddKey(string keyId)
    {
        if (!keys.Contains(keyId))
        {
            keys.Add(keyId);
            Debug.Log($"Ключ получен: {keyId}");
        }
    }
    
    public bool HasKey(string keyId)
    {
        return keys.Contains(keyId);
    }
    
    public void AddItem(string itemId)
    {
        if (!items.Contains(itemId))
        {
            items.Add(itemId);
            Debug.Log($"Предмет получен: {itemId}");
        }
    }
    
    public bool HasItem(string itemId)
    {
        return items.Contains(itemId);
    }
    
    public void UseCalculator()
    {
        if (hasCalculator)
        {
            // Упрощает решение интегралов
            Debug.Log("Калькулятор использован!");
        }
    }
    
    public void UseCheatSheet()
    {
        if (hasCheatSheet)
        {
            // Показывает подсказки
            Debug.Log("Шпаргалка использована!");
        }
    }
    
    public void UseEnergyDrink()
    {
        if (hasEnergyDrink)
        {
            // Восстанавливает выносливость
            PlayerController player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
            if (player != null)
            {
                // Восстановление выносливости
                Debug.Log("Энергетик использован! Выносливость восстановлена!");
            }
            hasEnergyDrink = false;
        }
    }
    
    public void ClearInventory()
    {
        keys.Clear();
        items.Clear();
        hasCalculator = false;
        hasCheatSheet = false;
        hasEnergyDrink = false;
    }
}
