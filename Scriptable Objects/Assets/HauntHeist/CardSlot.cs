using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace HH
{
    class Card
    {
        TextMeshProUGUI title, description, value, type;
        Image image;

        public Card(Transform card_root)
        {
            title = card_root.FindDeepChild("card_title").GetComponent<TextMeshProUGUI>();
            description = card_root.FindDeepChild("card_desc").GetComponent<TextMeshProUGUI>();
            value = card_root.FindDeepChild("card_value").GetComponent<TextMeshProUGUI>();
            image = (card_root.FindDeepChild("card_image")) ? card_root.FindDeepChild("card_image").GetComponent<Image>() : null;
            type = (card_root.FindDeepChild("card_type")) ? card_root.FindDeepChild("card_type").GetComponent<TextMeshProUGUI>() : null;
        }

        public void Clear()
        {
            title.text = "Default";
            description.text = "Default";
            value.text = "-";
            if (type) type.text = "-";
            if (image) image.sprite = Resources.Load<Sprite>("card_images/default}");
        }

        public int Fill(string card_title, string card_value, string card_image, string card_text, string card_type)
        {
            try
            {
                title.text = card_title;
                description.text = card_text;
                value.text = card_value;
                if (type) type.text = card_type;
                if (image) image.sprite = Resources.Load<Sprite>($"card_images/{card_image}");
                return 1;
            }
            catch
            {
                Debug.Log("GOD DAMN IT");
                Clear();
                return 0;
            }
        }
    }

    public class CardSlot : MonoBehaviour
    {
        Card standard, haunted, unhaunted;
        GameObject hauntedCard;

        public void Clear()
        {
            standard.Clear();
            haunted.Clear();
            unhaunted.Clear();
            hauntedCard.SetActive(false);
        }

        public int Fill(CardType cardType, string card_title, string card_value, string card_image, string card_text, string card_type)
        {
            if (!hauntedCard) hauntedCard = transform.FindDeepChild("HauntedCard").gameObject;
            if (standard == null) standard = new Card(transform.FindDeepChild("StandardCard"));
            if (haunted == null) haunted = new Card(transform.FindDeepChild("Haunted"));
            if (unhaunted == null) unhaunted = new Card(transform.FindDeepChild("Unhaunted"));

            if (cardType == CardType.DEFAULT)
            {
                hauntedCard.SetActive(false);
                haunted.Clear();
                unhaunted.Clear();
                return standard.Fill(card_title, card_value, card_image, card_text, card_type);
            }
            else if (cardType == CardType.HAUNTED)
            {
                standard.Clear();
                hauntedCard.SetActive(true);
                haunted.Fill(card_title, card_value, card_image, card_text, card_type);
                return 0;
            }
            else if (cardType == CardType.UNHAUNTED)
            {
                standard.Clear();
                if (hauntedCard.activeSelf)
                    return unhaunted.Fill(card_title, card_value, card_image, card_text, card_type);
                else return 0;
            }

            return 0;
        }
    }
}