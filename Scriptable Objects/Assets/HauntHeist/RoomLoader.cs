using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Debug = UnityEngine.Debug;
using UnityEditor;

public class RoomLoader : MonoBehaviour
{
    /// <summary>
    /// The csv file can be dragged throughthe inspector.
    /// </summary>
    public TextAsset csvFile;

    /// <summary>
    /// The grid in which the CSV File would be parsed.
    /// </summary>
    string[,] grid;

    List<Room> rooms = new List<Room>();


    public void Generate()
    {
        cam = Camera.main;
        grid = getCSVGrid(csvFile.text);
        Populate();
        Capture();
    }

    /// <summary>
    /// splits a CSV file into a 2D string array
    /// </summary>
    /// <returns> 2 day array of the csv file.</returns>
    /// <param name="csvText">the CSV data as string</param>
    public string[,] getCSVGrid(string csvText)
    {
        //split the data on split line character
        string[] lines = csvText.Split("\n"[0]);

        // find the max number of columns
        int totalColumns = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            string[] row = lines[i].Split('\t');
            totalColumns = Mathf.Max(totalColumns, row.Length);
        }

        // creates new 2D string grid to output to
        string[,] outputGrid = new string[totalColumns + 1, lines.Length + 1];
        for (int y = 0; y < lines.Length; y++)
        {
            string[] row = lines[y].Split('\t');
            if (row[1] == "")
                continue;
            for (int x = 0; x < row.Length; x++)
            {
                outputGrid[x, y] = row[x];
            }
        }
        return outputGrid;
    }


    int cardCount = 0;

    void Populate()
    {
        rooms = new List<Room>(GetComponentsInChildren<Room>());
        captureWidth = (int)transform.FindDeepChild("Grid").GetComponent<RectTransform>().rect.width;
        captureHeight = (int)transform.FindDeepChild("Grid").GetComponent<RectTransform>().rect.height;
        posX = (int)transform.FindDeepChild("Grid").GetComponent<RectTransform>().rect.position.x;
        posY = (int)transform.FindDeepChild("Grid").GetComponent<RectTransform>().rect.position.y;
        cardCount = 0;

        for (int y = 1; y < grid.GetLength(1); y++)
        {
            if (cardCount == rooms.Count)
            {
                cardCount = 0;
                Capture();
                foreach (var slot in rooms)
                    slot.Clear();
            }
            try
            {
                cardCount += rooms[cardCount].Fill(grid[0, y], grid[1, y], grid[2, y]);
            }
            catch
            {
                continue;
            }
        }
    }


    // 4k = 3840 x 2160   1080p = 1920 x 1080
    int captureWidth = 1680;
    int captureHeight = 1680;
    int posX, posY = 0;

    // optional game object to hide during screenshots (usually your scene canvas hud)
    public GameObject hideGameObject;

    Camera cam;

    // optimize for many screenshots will not destroy any objects so future screenshots will be fast
    public bool optimizeForManyScreenshots = true;

    // configure with raw, jpg, png, or ppm (simple raw format)
    public enum Format { RAW, JPG, PNG, PPM };
    public Format format = Format.PPM;

    // folder to write output (defaults to data path)
    public string folder;

    // private vars for screenshot
    private Rect rect;
    private RenderTexture renderTexture;
    private Texture2D screenShot;
    private int counter = 0; // image #

    // commands
    private bool captureScreenshot = false;
    private bool captureVideo = false;

    // create a unique filename using a one-up variable
    private string uniqueFilename(int width, int height)
    {
        // if folder not specified by now use a good default
        if (folder == null || folder.Length == 0)
        {
            folder = Application.dataPath;
            if (Application.isEditor)
            {
                // put screenshots in folder above asset path so unity doesn't index the files
                var stringPath = folder + "/..";
                folder = Path.GetFullPath(stringPath);
            }
            folder += "/screenshots";

            // make sure directoroy exists
            System.IO.Directory.CreateDirectory(folder);

            // count number of files of specified format in folder
            string mask = string.Format("screen_{0}x{1}*.{2}", width, height, format.ToString().ToLower());
            counter = Directory.GetFiles(folder, mask, SearchOption.TopDirectoryOnly).Length;
        }

        // use width, height, and counter for unique file name
        var filename = string.Format("{0}/screen_{1}x{2}_{3}.{4}", folder, width, height, counter, format.ToString().ToLower());

        // up counter for next call
        ++counter;

        // return unique filename
        return filename;
    }

    public void CaptureScreenshot()
    {
        captureScreenshot = true;
    }

    void Capture()
    {

        // hide optional game object if set
        if (hideGameObject != null) hideGameObject.SetActive(false);

        Debug.Log($"{captureWidth}, {captureHeight}");
        // creates off-screen render texture that can rendered into
        rect = new Rect(0, 0, captureWidth, captureHeight);
        renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
        screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);

        // get main camera and manually render scene into rt
        cam.targetTexture = renderTexture;
        cam.Render();

        // read pixels will read from the currently active render texture so make our offscreen 
        // render texture active and then read the pixels
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);

        // reset active camera texture and render texture
        cam.targetTexture = null;
        RenderTexture.active = null;
        // get our unique filename
        string filename = uniqueFilename((int)rect.width, (int)rect.height);

        // pull in our file header/data bytes for the specified image format (has to be done from main thread)
        byte[] fileHeader = null;
        byte[] fileData = null;
        if (format == Format.RAW)
        {
            fileData = screenShot.GetRawTextureData();
        }
        else if (format == Format.PNG)
        {
            fileData = screenShot.EncodeToPNG();
        }
        else if (format == Format.JPG)
        {
            fileData = screenShot.EncodeToJPG();
        }
        else // ppm
        {
            // create a file header for ppm formatted file
            string headerStr = string.Format("P6\n{0} {1}\n255\n", rect.width, rect.height);
            fileHeader = System.Text.Encoding.ASCII.GetBytes(headerStr);
            fileData = screenShot.GetRawTextureData();
        }

        // create new thread to save the image to file (only operation that can be done in background)
        new System.Threading.Thread(() =>
        {
            // create file and write optional header with image bytes
            var f = System.IO.File.Create(filename);
            if (fileHeader != null) f.Write(fileHeader, 0, fileHeader.Length);
            f.Write(fileData, 0, fileData.Length);
            f.Close();
            Debug.Log(string.Format("Wrote screenshot {0} of size {1}", filename, fileData.Length));
        }).Start();

        // unhide optional game object if set
        if (hideGameObject != null) hideGameObject.SetActive(true);

        // cleanup if needed
        if (optimizeForManyScreenshots == false)
        {
            Destroy(renderTexture);
            renderTexture = null;
            screenShot = null;
        }
    }
}

[CanEditMultipleObjects]
[CustomEditor(typeof(RoomLoader))]
public class RoomLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var scripts = targets.OfType<RoomLoader>();
        if (GUILayout.Button("GENERATE"))
            foreach (var script in scripts)
                script.Generate();
    }
}