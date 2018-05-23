using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public GameObject cloudFilePathInputObject;
    public GameObject featureFilePathInputObject;
    public GameObject subsampleDropDownObject;
    public GameObject pointCloudContainer;
    public GameObject minutiaePrefab;
    public GameObject igfWidthPrefab;
    public GameObject igfHeightPrefab;
    private InputField cloudPathField;
    private InputField featureFilePathField;
    private Dropdown subsampleDropDown;
    private Drawer drawer;
    private RenderedCloudContainer cloudContainer;
    private List<GameObject> features;
	private PointCloud pointCloud = null;
    private Tools tool = Tools.NONE;
    private Image activeTool = null;
    private GameObject currentFeature = null;
    private bool isEditing = true;
    private int scalingFactor = 1;

    // Use this for initialization
    void Start ()
    {
        cloudContainer = pointCloudContainer.GetComponent<RenderedCloudContainer>();
        drawer = GetComponent<Drawer>();
        cloudPathField = cloudFilePathInputObject.GetComponent<InputField>();
        featureFilePathField = featureFilePathInputObject.GetComponent<InputField>();
        subsampleDropDown = subsampleDropDownObject.GetComponent<Dropdown>();
        features = new List<GameObject>();
        drawer.width = 10f;

        // Load previous session's editor settings
        string configPath = Application.dataPath + Path.DirectorySeparatorChar + "config.dat";
        if (File.Exists(configPath))
        {
            try
            {
                using (FileStream stream = new FileStream(configPath, FileMode.Open, FileAccess.Read))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    Config config = (Config)bf.Deserialize(stream);
                    subsampleDropDown.value = config.scalingFactorPower;
                    cloudPathField.text = config.pointCloudFilePath;
                    featureFilePathField.text = config.featureFilePath;
                }
            }
            catch (SerializationException) { }
        }
    }

	// Update is called once per frame
	void Update () {
        if (pointCloud == null)
            return;
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            isEditing = true;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int xi, yi;
            float zi;

            if (tool == Tools.MINUTIAE_3D)
            {
                GameObject point = GetPositionOnSurface(ray, pointCloud, Camera.main.transform.position, out xi, out yi, out zi);
                if (point == null)
                {
                    Debug.LogWarning("Couldn't place minutiae.");
                    isEditing = false;
                    return;
                }
                currentFeature = Instantiate(minutiaePrefab, point.transform.position, Quaternion.identity);
                currentFeature.transform.localScale = point.transform.localScale;
                Minutiae3D minutiae = currentFeature.GetComponent<Minutiae3D>();
                minutiae.X = xi;
                minutiae.Y = yi;
                minutiae.Z = zi;
                features.Add(currentFeature);
            }
            else if (tool == Tools.DELETE)
            {
                int best = -1;
                float bestDist = float.PositiveInfinity;
                for (int i = 0; i < features.Count; i++)
                {
                    float dist = Vector3.Cross(ray.direction, features[i].transform.position - ray.origin).magnitude;
                    if (dist < bestDist)
                    {
                        best = i;
                        bestDist = dist;
                    }
                }
                Vector3 p00 = cloudContainer.GetPointObject(0, 0).transform.position;
                Vector3 p11 = cloudContainer.GetPointObject(1, 1).transform.position;
                p00.y = 0;
                p11.y = 0;
                float distThreshold = Vector3.Distance(p00, p11) / 2f;
                if (bestDist < distThreshold)
                {
                    Destroy(features[best]);
                    features.RemoveAt(best);
                }
            }
            else if (tool == Tools.IGF_WIDTH)
            {
                GameObject point = GetPositionOnSurface(ray, pointCloud, Camera.main.transform.position, out xi, out yi, out zi);
                if (point == null)
                {
                    Debug.LogWarning("Couldn't place IGF Width.");
                    isEditing = false;
                    return;
                }
                currentFeature = Instantiate(igfWidthPrefab, point.transform.position, Quaternion.identity);
                currentFeature.transform.localScale = point.transform.localScale;
                IgfWidth igfWidth = currentFeature.GetComponent<IgfWidth>();
                igfWidth.startX = xi;
                igfWidth.startY = yi;
                igfWidth.startZ = zi;
                features.Add(currentFeature);
            }
            else if (tool == Tools.IGF_HEIGHT)
            {
                GameObject point = GetPositionOnSurface(ray, pointCloud, Camera.main.transform.position, out xi, out yi, out zi);
                if (point == null)
                {
                    Debug.LogWarning("Couldn't place IGF Height.");
                    isEditing = false;
                    return;
                }
                currentFeature = Instantiate(igfHeightPrefab, point.transform.position, Quaternion.identity);
                currentFeature.transform.localScale = point.transform.localScale;
                IgfHeight igfHeight = currentFeature.GetComponent<IgfHeight>();
                igfHeight.startX = xi;
                igfHeight.startY = yi;
                igfHeight.startZ = zi;
                features.Add(currentFeature);
            }
        }
        else if (Input.GetMouseButton(0) && isEditing)
        {
            if (tool == Tools.MINUTIAE_3D)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float dist = Vector3.Distance(Camera.main.transform.position, currentFeature.transform.position);
                currentFeature.transform.LookAt(ray.GetPoint(dist));
            }
            else if (tool == Tools.IGF_WIDTH)
            {
                int xi, yi;
                float zi;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                GameObject point = GetPositionOnSurface(ray, pointCloud, Camera.main.transform.position, out xi, out yi, out zi);
                currentFeature.transform.LookAt(point.transform.position);
                float scaleZ = Vector3.Distance(currentFeature.transform.position, point.transform.position) / 2.0f;
                var scale = currentFeature.transform.localScale;
                scale.z = scaleZ;
                currentFeature.transform.localScale = scale;
            }
            else if (tool == Tools.IGF_HEIGHT)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float dist = Vector3.Distance(Camera.main.transform.position, currentFeature.transform.position);
                currentFeature.transform.LookAt(ray.GetPoint(dist));

                float scaleZ = Vector3.Distance(currentFeature.transform.position, ray.GetPoint(dist)) / 2.0f;
                var scale = currentFeature.transform.localScale;
                scale.z = scaleZ;
                currentFeature.transform.localScale = scale;
            }
        }
        else if (Input.GetMouseButtonUp(0) && isEditing)
        {
            if (tool == Tools.IGF_WIDTH)
            {
                int xi, yi;
                float zi;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                GameObject point = GetPositionOnSurface(ray, pointCloud, Camera.main.transform.position, out xi, out yi, out zi);
                if (point == null)
                {
                    Debug.LogWarning("Couldn't end IGF Width.");
                    isEditing = false;
                    return;
                }
                IgfWidth igfWidth = currentFeature.GetComponent<IgfWidth>();
                igfWidth.endX = xi;
                igfWidth.endY = yi;
                igfWidth.endZ = zi;
                features.Add(currentFeature);
                tool = Tools.IGF_HEIGHT;
            }
            else if (tool == Tools.IGF_HEIGHT)
            {
                tool = Tools.IGF_WIDTH;
                isEditing = false;
            }
            else
            {
                isEditing = false;
            }

            currentFeature = null;
        }
    }

    private GameObject GetPositionOnSurface(Ray ray, PointCloud cloud, Vector3 cameraPos, out int xIndex, out int yIndex, out float zIndex)
    {
        xIndex = 0;
        yIndex = 0;
        zIndex = 0;
        // TODO: Need a KD Tree or something to speed up this O(n^2) solution:
        Vector3 p00 = cloudContainer.GetPointObject(0, 0).transform.position;
        Vector3 p11 = cloudContainer.GetPointObject(1, 1).transform.position;
        p00.y = 0;
        p11.y = 0;
        // Get points close to ray
        List<Point> points = AuxillaryFunctions.a(ray, cloudContainer, Vector3.Distance(p00, p11) / 2f);
        if (points.Count == 0)
            points = AuxillaryFunctions.a(ray, cloudContainer, Vector3.Distance(p00, p11));
        if (points.Count == 0)
            return null;

        // Order Points by closest to camera
        points = points.OrderBy(o => Vector3.Distance(cloudContainer.GetPointObject(o.X / scalingFactor, o.Y / scalingFactor).transform.position, cameraPos)).ToList();
        xIndex = points[0].X;
        yIndex = points[0].Y;
        return cloudContainer.GetPointObject(points[0].X / scalingFactor, points[0].Y / scalingFactor);
    }

    public void LoadFile()
    {
        foreach (Transform child in pointCloudContainer.transform)
            Destroy(child.gameObject);
        foreach (var featObj in features)
            Destroy(featObj);
        features = new List<GameObject>();
        pointCloud = new PointCloud(cloudPathField.text);
        Debug.Log(String.Format("Read cloud: {0}x{1}", pointCloud.Width, pointCloud.Height));
        scalingFactor = AuxillaryFunctions.Pow(2, subsampleDropDown.value);
        drawer.DrawCloud(pointCloud, scalingFactor, pointCloudContainer.transform);

        // Save session's editor settings for next time
        using (FileStream stream = new FileStream(Application.dataPath + Path.DirectorySeparatorChar + "config.dat", FileMode.OpenOrCreate, FileAccess.Write))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(stream, new Config(featureFilePathField.text, cloudPathField.text, subsampleDropDown.value));
        }
    }

    public void loadFeatureFile(GameObject[] inputField)
    {
        string filePath = inputField[0].GetComponent<InputField>().text;
        foreach (var featObj in features)
            Destroy(featObj);
        features = new List<GameObject>();
        using (StreamReader file = new StreamReader(filePath))
        {
            string line;
            while ((line = file.ReadLine()) != null)
            {
                string[] lineArray = line.Split(',');
                if (lineArray.Length == 0)
                    continue;
                if (lineArray[0] == "minutia")
                {
                    int x = int.Parse(lineArray[1]);
                    int y = int.Parse(lineArray[2]);
                    int z = int.Parse(lineArray[3]);
                    float α = float.Parse(lineArray[4]);
                    float β = float.Parse(lineArray[5]);
                    float γ = float.Parse(lineArray[6]);
                    features.Add(Instantiate(minutiaePrefab, drawer.IndexPointToWorldPoint(pointCloud, new Point(x, y, z), scalingFactor), Quaternion.Euler(α, γ, β)));
                    features[features.Count - 1].transform.localScale = cloudContainer.GetPointObject(0, 0).transform.localScale;
                }
            }
        }
        Debug.Log(string.Format("Read feature file: {0} Feature(s)", features.Count));
    }

    public void saveFeatureFile(GameObject inputField)
    {
        string filePath = inputField.GetComponent<InputField>().text;
        using (StreamWriter file = new StreamWriter(filePath))
        {
            foreach (var feature in features)
            {
                Minutiae3D minutiae = feature.GetComponent<Minutiae3D>();
                Vector3 angles = minutiae.Rotation.eulerAngles;
                file.WriteLine(string.Format("minutia,{0},{1},{2},{3},{4},{5}", minutiae.X, minutiae.Y, minutiae.Z, angles.x, angles.z, angles.y));
            }
        }
        Debug.Log("Saved feature file");
    }

    public void setTool(GameObject toolObject)
    {
        switch (toolObject.name)
        {
            case "Delete":
                tool = (tool == Tools.DELETE) ? Tools.NONE : Tools.DELETE;
                break;
            case "3D Minutiae":
                tool = (tool == Tools.MINUTIAE_3D) ? Tools.NONE : Tools.MINUTIAE_3D;
                break;
            case "IGF":
                tool = (tool == Tools.IGF_WIDTH || tool == Tools.IGF_HEIGHT) ? Tools.NONE : Tools.IGF_WIDTH;
                break;
            default:
                throw new ArgumentException("Unknown tool: " + toolObject.name);
        }
        if (activeTool != null)
            activeTool.color = Color.white;
        Image buttonImg = toolObject.GetComponent<Image>();
        if (tool == Tools.NONE)
        {
            buttonImg.color = Color.white;
            activeTool = null;
        }
        else
        {
            buttonImg.color = Color.green;
            activeTool = buttonImg;
        }
    }
}
