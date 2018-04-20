using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Main : MonoBehaviour {
    public GameObject filePathInputObject;
    public GameObject subsampleDropDownObject;
    public GameObject pointCloudContainer;
    public GameObject minutiaePrefab;
    private InputField inputField;
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
        inputField = filePathInputObject.GetComponent<InputField>();
        subsampleDropDown = subsampleDropDownObject.GetComponent<Dropdown>();
        features = new List<GameObject>();
        drawer.width = 10f;
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
            GameObject point = GetPositionOnSurface(ray, pointCloud, Camera.main.transform.position, out xi, out yi, out zi);

            if (tool == Tools.MINUTIAE_3D)
            {
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
        }
        else if (Input.GetMouseButton(0) && isEditing)
        {
            if (tool == Tools.MINUTIAE_3D)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float dist = Vector3.Distance(Camera.main.transform.position, currentFeature.transform.position);
                currentFeature.transform.LookAt(ray.GetPoint(dist));
            }
        }
        else if (Input.GetMouseButtonUp(0) && isEditing)
        {
            isEditing = false;
            currentFeature = null;
        }

    }

    private GameObject GetPositionOnSurface(Ray ray, PointCloud cloud, Vector3 cameraPos, out int xIndex, out int yIndex, out float zIndex)
    {
        xIndex = 0;
        yIndex = 0;
        zIndex = 0;
        RenderedCloudContainer container = pointCloudContainer.GetComponent<RenderedCloudContainer>();
        // TODO: Need a KD Tree or something to speed up this O(n^2) solution:
        Vector3 p00 = container.GetPointObject(0, 0).transform.position;
        Vector3 p11 = container.GetPointObject(1, 1).transform.position;
        p00.y = 0;
        p11.y = 0;
        float distThreshold = Vector3.Distance(p00, p11) / 2f;
        // Get points close to ray
        List<Point> points = new List<Point>();
        for (int y = 0; y < container.Height; y++)
        {
            for (int x = 0; x < container.Width; x++)
            {
                float dist = Vector3.Cross(ray.direction, container.GetPointObject(x, y).transform.position - ray.origin).magnitude;
                if (dist < distThreshold)
                    points.Add(container.GetPointCloudPosition(x, y));
            }
        }
        // Debug.Log("Selected Points:" + points.Count);
        // foreach (var p in points)
        // {
        //     Debug.Log(string.Format("({0}, {1})", p.X, p.Y));
        // }
        if (points.Count == 0)
            return null;

        // Order Points by closest to camera
        points = points.OrderBy(o => Vector3.Distance(container.GetPointObject(o.X, o.Y).transform.position, cameraPos)).ToList();
        xIndex = points[0].X;
        yIndex = points[0].Y;
        return container.GetPointObject(points[0].X, points[0].Y);
    }

    public void loadFile()
    {
        foreach (Transform child in pointCloudContainer.transform)
            Destroy(child.gameObject);
        pointCloud = new PointCloud(inputField.text);
        Debug.Log(String.Format("Read cloud: {0}x{1}", pointCloud.Width, pointCloud.Height));
        scalingFactor = AuxillaryFunctions.Pow(2, subsampleDropDown.value);
        drawer.DrawCloud(pointCloud, scalingFactor, pointCloudContainer.transform);
    }

    public void loadFeatureFile(GameObject inputField)
    {
        string filePath = inputField.GetComponent<InputField>().text;
        foreach (var featObj in features)
            Destroy(featObj);
        features = new List<GameObject>();
        using (System.IO.StreamReader file = new System.IO.StreamReader(filePath))
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
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath))
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
