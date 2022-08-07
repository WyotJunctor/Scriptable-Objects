using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;


namespace HeroesBeware
{
    //public class column, assign, default,
    public enum CardComponentType { Text, Image, Color, Symbol, Map };

    [System.Serializable]
    public class CardComponent
    {
        public static Regex rgx = new Regex("[^a-zA-Z0-9 -|_*,.:><()/;\n]");

        public string componentName;
        public CardComponentType cardComponentType;
        protected Transform holder;

        public static Color ArchToColor(string architect)
        {
            Dictionary<string, Color> colorMap = new Dictionary<string, Color>() 
            {
                { "limbsnatcher", new Color32(180,95,6,255)},
                { "thralltaker", new Color32(19,79,92,255)},
                { "willbreaker", new Color32(133,32,12,255)},
                { "planewarper", new Color32(67,67,67,255)},
                { "barony", new Color32(17, 85, 204, 255)},
                { "council", new Color32(153, 153, 153, 255)},
                { "order", new Color32(204, 0, 0, 255)},
                { "guild", new Color32(191, 144, 0, 255)},
                { "default", Color.gray},
            };
            return colorMap[architect];
        }

        public CardComponent(string componentName, CardComponentType cardComponentType)
        {
            this.componentName = componentName;
            this.cardComponentType = cardComponentType;
        }

        string Emojify(string unfortunate_soul)
        {
            string transcended_entity = "";
            int emoji_zone = 0;
            for (int i = 0; i < unfortunate_soul.Length; i++)
            {
                if (unfortunate_soul[i] > 0x00BF || unfortunate_soul[i] == '~')
                {
                    //Debug.Log($"ERMM... STINKY DETECTED?? {unfortunate_soul[i].ToString()}");
                    transcended_entity +=
                        CardComponent.rgx.Replace(
                            unfortunate_soul.Substring(emoji_zone, i - emoji_zone), "");
                    if (unfortunate_soul[i] == '~')
                    {
                        transcended_entity += $"<color=#7777><font=\"emoji_font\">{unfortunate_soul[i+1]}</font></color>";
                        i++;
                    }
                    else
                    {

                        transcended_entity += $"<font=\"emoji_font\">0x1F32B</font>";
                    }
                    emoji_zone = i + 1;
                }
            }
            if (emoji_zone < unfortunate_soul.Length)
            {
                transcended_entity +=
                    CardComponent.rgx.Replace(
                        unfortunate_soul.Substring(emoji_zone), "");
            }
            return transcended_entity;
        }

        public virtual bool Fill(string value, Transform holder)
        {
            if (this.holder == null) this.holder = holder;
            value = Emojify(value); //rgx.Replace(value, "");
            switch (cardComponentType)
            {
                case CardComponentType.Image:
                    Image image = holder.
                        FindDeepChild(componentName).
                        GetComponent<Image>();
                    try {
                        string path = $"HEROES_BEWARE/card_images/{value}";
                        image.sprite = Resources.Load<Sprite>(path);
                    }
                    catch { image.sprite = Resources.Load<Sprite>("HEROES_BEWARE/card_images/default"); }
                    break;
                case CardComponentType.Text:
                    try { holder.FindDeepChild(componentName).GetComponent<TextMeshProUGUI>().text = value; }
                    catch { Debug.Log(componentName); }
                    break;
                case CardComponentType.Color:
                    foreach (var child in holder.FindDeepChildren(componentName))
                    {
                        Color color = ArchToColor(value.ToLower());
                        color.a = child.GetComponent<Image>().color.a;
                        child.GetComponent<Image>().color = color;
                    }
                    break;
                case CardComponentType.Symbol:
                    holder.FindDeepChild(componentName).gameObject.SetActive(value == "TRUE");
                    break;
            }
            return true;
        }

        public virtual void Clear()
        {
            string value = "default";
            switch (cardComponentType)
            {
                case CardComponentType.Text:
                    value = componentName;
                    break;
            }
            Fill(value, holder);
        }
    }

    public class FixedTextCardComponent : CardComponent
    {
        public string textValue;

        public FixedTextCardComponent(string componentName, string textValue) : base(componentName, CardComponentType.Text)
        {
            this.textValue = rgx.Replace(textValue, "");
        }

        public override bool Fill(string value, Transform holder)
        {
            holder.FindDeepChild(componentName).GetComponent<TextMeshProUGUI>().text = textValue;
            return true;
        }
    }

    public class FixedColorCardComponent : CardComponent
    {
        public Color32 colorValue;
        Color originalColor;

        public FixedColorCardComponent(string componentName, Color32 colorValue) : base(componentName, CardComponentType.Color)
        {
            this.colorValue = colorValue;
        }

        public override bool Fill(string value, Transform holder)
        {
            Transform child = holder.FindDeepChild(componentName);
            if (child.GetComponent<TextMeshProUGUI>())
            {
                originalColor = child.GetComponent<TextMeshProUGUI>().color;
                child.GetComponent<TextMeshProUGUI>().color = colorValue;
            }
            else if (child.GetComponent<Image>())
            {
                originalColor = child.GetComponent<Image>().color;
                child.GetComponent<Image>().color = colorValue;
            }
            return true;
        }

        public override void Clear()
        {
            Transform child = holder.FindDeepChild(componentName);
            if (child.GetComponent<TextMeshProUGUI>())
            {
                child.GetComponent<TextMeshProUGUI>().color = originalColor;
            }
            else if (child.GetComponent<Image>())
            {
                child.GetComponent<Image>().color = originalColor;
            }
        }
    }

    public class Card
    {
        Dictionary<string, List<CardComponent>> cardComponentMap;

        public Card(Dictionary<string, List<CardComponent>> cardComponentMap)
        {
            this.cardComponentMap = cardComponentMap;
        }

        public string Fill(Row row, Transform holder)
        {
            string group = "NONE";
            foreach (var key in row.values.Keys)
            {
                if (cardComponentMap.ContainsKey(key))
                {
                    foreach (var cardComponent in cardComponentMap[key])
                    {
                        if (cardComponent.cardComponentType == CardComponentType.Color)
                        {
                            group = row.values[key];
                        }
                        cardComponent.Fill(row.values[key], holder);
                    }
                }
            }
            if (cardComponentMap.ContainsKey("*"))
            {
                foreach (var cardComponent in cardComponentMap["*"])
                    cardComponent.Fill("", holder);
            }
            return group;
        }

        public void Clear()
        {
            foreach (var componentList in cardComponentMap.Values)
                foreach (var c in componentList)
                    c.Clear();
        }
    }


    public class HB_CardSlot : MonoBehaviour
    {

        /*
        Dictionary<CardType, Card> cards { get { return new Dictionary<CardType, Card>()
        {
            { CardType.ASSET, new Card(new Dictionary<string, List<CardComponent>>()
                {
                    {"name", new List<CardComponent>() {new CardComponent("title", CardComponentType.Text, transform)} },
                    {"auras", new List<CardComponent>() {new CardComponent("auras", CardComponentType.Text, transform)} },
                    {"constant effect", new List<CardComponent>() {new CardComponent("c_effect", CardComponentType.Text, transform)} },
                    {"fate", new List<CardComponent>() {new CardComponent("fate_1", CardComponentType.Text, transform)} },
                    {"activation", new List<CardComponent>() {new CardComponent("activation_1", CardComponentType.Text, transform)} },
                    {"resistance", new List<CardComponent>() {new CardComponent("resistance_1", CardComponentType.Text, transform)} },
                    {"effect", new List<CardComponent>() {new CardComponent("effect_1", CardComponentType.Text, transform)} },
                    {"flavor", new List<CardComponent>() {new CardComponent("flavor_1", CardComponentType.Text, transform)} },
                    {"tags", new List<CardComponent>() {new CardComponent("tags", CardComponentType.Text, transform)} },
                    {"architect", new List<CardComponent>() {new CardComponent("card_color", CardComponentType.Color, transform)} },
                    {"cost", new List<CardComponent>() {new CardComponent("cost", CardComponentType.Text, transform)} },
                    {"image", new List<CardComponent>() {new CardComponent("image", CardComponentType.Image, transform)} },
                    {"fate^1", new List<CardComponent>() {new CardComponent("fate_2", CardComponentType.Text, transform)} },
                    {"activation^1", new List<CardComponent>() {new CardComponent("activation_2", CardComponentType.Text, transform)} },
                    {"resistance^1", new List<CardComponent>() {new CardComponent("resistance_2", CardComponentType.Text, transform)} },
                    {"effect^1", new List<CardComponent>() {new CardComponent("effect_2", CardComponentType.Text, transform)} },
                    {"flavor^1", new List<CardComponent>() {new CardComponent("flavor_2", CardComponentType.Text, transform)} },
                }
            )},
            { CardType.ROOM, new Card(new Dictionary<string, List<CardComponent>>()
                {
                    {"name", new List<CardComponent>() {new CardComponent("title", CardComponentType.Text, transform)} },
                    {"phase", new List<CardComponent>() {new CardComponent("phase", CardComponentType.Text, transform)} },
                    {"auras", new List<CardComponent>() {new CardComponent("auras", CardComponentType.Text, transform)} },
                    {"slots", new List<CardComponent>() {new CardComponent("slots", CardComponentType.Text, transform)} },
                    {"effect", new List<CardComponent>() {new CardComponent("effect", CardComponentType.Text, transform)} },
                    {"flavor", new List<CardComponent>() {new CardComponent("flavor", CardComponentType.Text, transform)} },
                    {"architect", new List<CardComponent>() {new CardComponent("card_color", CardComponentType.Color, transform)} },
                    {"image", new List<CardComponent>() {new CardComponent("image", CardComponentType.Image, transform)} },
                }
            )},
            { CardType.ACTION, new Card(new Dictionary<string, List<CardComponent>>()
                {
                    {"name", new List<CardComponent>() {new CardComponent("title", CardComponentType.Text, transform)} },
                    {"phase", new List<CardComponent>() {new CardComponent("phase", CardComponentType.Text, transform)} },
                    {"auras", new List<CardComponent>() {new CardComponent("auras", CardComponentType.Text, transform)} },
                    {"effect", new List<CardComponent>() {new CardComponent("effect", CardComponentType.Text, transform)} },
                    {"flavor", new List<CardComponent>() {new CardComponent("flavor", CardComponentType.Text, transform)} },
                    {"architect", new List<CardComponent>() {new CardComponent("card_color", CardComponentType.Color, transform)} },
                    {"starter", new List<CardComponent>() {new CardComponent("starter", CardComponentType.Symbol, transform)} },
                    {"image", new List<CardComponent>() {new CardComponent("image", CardComponentType.Image, transform)} },
                }
            )},
            { CardType.STRATEGY, new Card(new Dictionary<string, List<CardComponent>>()
                {
                    {"name", new List<CardComponent>() {new CardComponent("title", CardComponentType.Text, transform)} },
                    {"phase", new List<CardComponent>() {new CardComponent("phase", CardComponentType.Text, transform)} },
                    {"fate1", new List<CardComponent>() {new CardComponent("fate_1", CardComponentType.Text, transform)} },
                    {"flavor1", new List<CardComponent>() {new CardComponent("flavor_1", CardComponentType.Text, transform)} },
                    {"fate2", new List<CardComponent>() {new CardComponent("fate_2", CardComponentType.Text, transform)} },
                    {"flavor2", new List<CardComponent>() {new CardComponent("flavor_2", CardComponentType.Text, transform)} },
                    {"bonuses", new List<CardComponent>() {new CardComponent("c_effect", CardComponentType.Text, transform)} },
                    {"faction", new List<CardComponent>() {new CardComponent("card_color", CardComponentType.Color, transform)} },
                    {"image", new List<CardComponent>() {new CardComponent("image", CardComponentType.Image, transform)} },
                }
            )},
            { CardType.FRONT_HERO, new Card(new Dictionary<string, List<CardComponent>>()
                {
                    {"name", new List<CardComponent>() {new CardComponent("title", CardComponentType.Text, transform)} },
                    {"*", new List<CardComponent>() {
                        new FixedTextCardComponent("row", transform, "Front"), 
                        new FixedColorCardComponent("Top", transform, new Color(1, 1, 1, 0.25f)),
                        new FixedColorCardComponent("top", transform, new Color(1, 1, 1, 0.25f))
                        }
                    },
                    {"resolve", new List<CardComponent>() {new CardComponent("resolve", CardComponentType.Text, transform)} },
                    {"vitality", new List<CardComponent>() {new CardComponent("vitality", CardComponentType.Text, transform)} },
                    {"front", new List<CardComponent>() {new CardComponent("bottom", CardComponentType.Text, transform)} },
                    {"back", new List<CardComponent>() {new CardComponent("top", CardComponentType.Text, transform)} },
                    {"faction", new List<CardComponent>() {new CardComponent("card_color", CardComponentType.Color, transform)} },
                    {"front_image", new List<CardComponent>() {new CardComponent("image", CardComponentType.Image, transform)} },
                }
            )},
            { CardType.BACK_HERO, new Card(new Dictionary<string, List<CardComponent>>()
                {
                    {"name", new List<CardComponent>() {new CardComponent("title", CardComponentType.Text, transform)} },
                    {"*", new List<CardComponent>() {
                        new FixedTextCardComponent("row", transform, "Back"),
                        new FixedColorCardComponent("Bottom", transform, new Color(1, 1, 1, 0.25f)),
                        new FixedColorCardComponent("bottom", transform, new Color(1, 1, 1, 0.25f))
                        }
                    },
                    {"resolve", new List<CardComponent>() {new CardComponent("resolve", CardComponentType.Text, transform)} },
                    {"vitality", new List<CardComponent>() {new CardComponent("vitality", CardComponentType.Text, transform)} },
                    {"back", new List<CardComponent>() {new CardComponent("bottom", CardComponentType.Text, transform)} },
                    {"front", new List<CardComponent>() {new CardComponent("top", CardComponentType.Text, transform)} },
                    {"faction", new List<CardComponent>() {new CardComponent("card_color", CardComponentType.Color, transform)} },
                    {"back_image", new List<CardComponent>() {new CardComponent("image", CardComponentType.Image, transform)} },
                }
            )},
            { CardType.BACK, new Card(new Dictionary<string, List<CardComponent>>()
                {
                    {"group", new List<CardComponent>() {new CardComponent("card_color", CardComponentType.Color, transform)} },
                    {"symbol", new List<CardComponent>() {new CardComponent("symbol", CardComponentType.Image, transform)} },
                }
            )},
        };
        }}
        */

        Card currentCard;

        public void Clear()
        {
            if (currentCard != null)
                currentCard.Clear();
        }

        public string Fill(Row row, HB_CardDefinition cardDefinition)
        {
            currentCard = new Card(cardDefinition.GetComponents());
            return currentCard.Fill(row, transform);
        }
    }
}
