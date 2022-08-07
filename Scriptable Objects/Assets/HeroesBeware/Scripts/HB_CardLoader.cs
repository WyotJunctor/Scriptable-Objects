using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Debug = UnityEngine.Debug;
using UnityEditor;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;

namespace HeroesBeware
{
    public enum CardType { ASSET, ACTION, ROOM, STRATEGY, FRONT_HERO, BACK_HERO, BACK }

    public class Row
    {
        public Dictionary<string, string> values;
        public int copies = 1;
        public bool added = false;

        public override string ToString()
        {
            return string.Join(", ", values);
        }
    }

    public class HB_CardLoader : MonoBehaviour
    {
        /// <summary>
        /// The csv file can be dragged throughthe inspector.
        /// </summary>
        public TextAsset csvFile;
        public bool capture = true;
        public HB_CardDefinition cardDefinition;
        // folder to write output (defaults to data path)
        string folder;
        // private int counter = 0; // image #
        private Dictionary<string, int> groupCounter = new Dictionary<string, int>();

        List<HB_CardSlot> cardSlots = new List<HB_CardSlot>();

        private void Start()
        {
            Generate();
        }

        public void Generate()
        {
            // counter = 0;
            List<Row> rows = getCSVGrid(csvFile.text);
            StartCoroutine(Populate(rows));
            // Capture(group);
        }

        /// <summary>
        /// splits a CSV file into a 2D string array
        /// </summary>
        /// <returns> 2 day array of the csv file.</returns>
        /// <param name="csvText">the CSV data as string</param>
        public List<Row> getCSVGrid(string csvText)
        {
            //split the data on split line character
            string[] lines = csvText.Split("\n"[0]);
            Row lastRow = new Row();
            List<Row> rows = new List<Row>();
            string[] keys = lines[0].Split('\t');
            int lineCounter = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                string[] row = lines[i].Split('\t');

                if (row[0].Length > 0 && row[0][0] == '^')
                {
                    lineCounter++;
                    for (int j = 0; j < keys.Length; j++)
                    {
                        string k = CardComponent.rgx.Replace(keys[j], "");
                        if (lastRow.values.ContainsKey(k))
                            lastRow.values[k + $"^{lineCounter}"] = row[j];
                        else
                            lastRow.values[k] = row[j];
                    }
                }
                else
                {
                    lineCounter = 0;
                    if (lastRow.added)
                        rows.Add(lastRow);
                    lastRow = new Row();
                    lastRow.values = new Dictionary<string, string>();
                    for (int j = 0; j < keys.Length; j++)
                    {
                        string k = CardComponent.rgx.Replace(keys[j], "");
                        lastRow.values[k] = row[j];
                    }
                    lastRow.added = true;
                }
            }
            rows.Add(lastRow);
            return rows;
        }

        IEnumerator Populate(List<Row> rows)
        {
            cardSlots = new List<HB_CardSlot>(GetComponentsInChildren<HB_CardSlot>());
            //captureWidth = (int)transform.FindDeepChild("Grid").GetComponent<RectTransform>().rect.width;
            //captureHeight = (int)transform.FindDeepChild("Grid").GetComponent<RectTransform>().rect.height;
            int cardCount = 0, captureCount = 1;

            //print(string.Join("| ", rows));

            foreach (Row row in rows)
            {
                int copies = 1;
                int.TryParse(row.values.SetDefault("copies", "1"), out copies);
                copies = Mathf.Max(1, copies);
                for (int i = 0; i < copies; i++) {
                    string group = cardSlots[cardCount].Fill(row, cardDefinition);
                    yield return new WaitForSeconds(1);
                    if (++cardCount == captureCount)
                    {
                        Capture(group);
                        foreach (var slot in cardSlots)
                            slot.Clear();
                        cardCount = 0;
                    }
                }
            }
            foreach (var slot in cardSlots)
                slot.Clear();
        }


        // create a unique filename using a one-up variable
        private string uniqueFilename(string group)
        {
            group = group.ToUpper();

            folder = Application.dataPath;
            if (Application.isEditor)
            {
                // put screenshots in folder above asset path so unity doesn't index the files
                var stringPath = folder + "/..";
                folder = Path.GetFullPath(stringPath);
            }
            folder += $"/screenshots/{cardDefinition.cardTypeName}/{group}/";

            if (!System.IO.Directory.Exists(folder))
                // make sure directory exists
                System.IO.Directory.CreateDirectory(folder);

            // string mask = $"capture_";
            // counter = Directory.GetFiles(folder, mask, SearchOption.TopDirectoryOnly).Length;
            // print(Directory.GetFiles(folder, "*.png"));

            groupCounter[group] = groupCounter.SetDefault(group, -1) + 1;

            // use width, height, and counter for unique file name
            var filename = $"{folder}capture_{cardDefinition.cardTypeName}_{groupCounter[group]}.png"; //string.Format("{0}/screen_{1}x{2}_{3}.{4}", folder, width, height, counter, format.ToString().ToLower());

            // up counter for next call
            // ++counter;

            // return unique filename
            return filename;
        }

        public void Capture(string group)
        {

            if (!capture) return;

            ScreenCapture.CaptureScreenshot(uniqueFilename(group));
            return;

        }
    }

    /*
    [CanEditMultipleObjects]
    [CustomEditor(typeof(HB_CardLoader))]
    public class CardLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var scripts = targets.OfType<HB_CardLoader>();
            if (GUILayout.Button("CAPTURE"))
                foreach (var script in scripts)
                    script.Capture("Manual");
        }
    }
    */

}

public static class DictionaryExtension
{
    public static V SetDefault<K, V>(this IDictionary<K, V> dict, K key, V @default)
    {
        V value;
        if (!dict.TryGetValue(key, out value))
        {
            dict.Add(key, @default);
            return @default;
        }
        else
        {
            return dict[key];
        }
    }
}