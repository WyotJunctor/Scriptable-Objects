using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Room : MonoBehaviour
{

    TextMeshProUGUI title, text;
    GameObject T, B, L, R;

    public void Clear()
    {
        Set();
        title.text = "Empty Room";
        text.text = "It's an empty room.";
        T.SetActive(false);
        B.SetActive(false);
        L.SetActive(false);
        R.SetActive(false);
        print("HAHA");
    }

    void Set()
    {
        if (!title) title = transform.FindDeepChild("title").GetComponent<TextMeshProUGUI>();
        if (!text) text = transform.FindDeepChild("text").GetComponent<TextMeshProUGUI>();
        if (!T) T = transform.FindDeepChild("T").gameObject;
        if (!B) B = transform.FindDeepChild("B").gameObject;
        if (!L) L = transform.FindDeepChild("L").gameObject;
        if (!R) R = transform.FindDeepChild("R").gameObject;
        print(T);
    }

    public int Fill(string room_title, string room_text, string room_doors)
    {
        try
        {
            Clear();
            title.text = room_title;
            text.text = room_text;
            //T.SetActive(room_doors.Contains("T"));
            //B.SetActive(room_doors.Contains("B"));
            //L.SetActive(room_doors.Contains("L"));
            //R.SetActive(room_doors.Contains("R"));
        }
        catch
        {
            return 0;
        }
        return 1;
    }
}
