using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeroesBeware;

[System.Serializable]
public class CardComponentItem
{
    public string fieldName;
    public List<CardComponent> cardComponents = new List<CardComponent>();
}

[CreateAssetMenu(fileName = "NewCardDefinition", menuName = "Card Definition")]
public class HB_CardDefinition : ScriptableObject
{
    public string cardTypeName;
    public List<CardComponentItem> componentItems = new List<CardComponentItem>();

    public Dictionary<string, List<CardComponent>> GetComponents()
    {
        var componentMap = new Dictionary<string, List<CardComponent>>();
        foreach (var componentItem in componentItems) 
        {
            componentMap[componentItem.fieldName] = componentItem.cardComponents;
        }
        return componentMap;
    } 
}
