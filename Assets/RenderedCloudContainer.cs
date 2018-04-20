using UnityEngine;

/// <summary>
/// Contains the rendered cloud.
/// </summary>
public class RenderedCloudContainer : MonoBehaviour
{
    public int Width { get; set; }
    public int Height { get; set; }

    private GameObject[][] cloudObjects;
    private Point[][] cloudPoints;

    public void ResetCloud(int width, int height)
    {
        cloudObjects = new GameObject[width][];
        cloudPoints = new Point[width][];
        for (int x = 0; x < width; x++)
        {
            cloudObjects[x] = new GameObject[height];
            cloudPoints[x] = new Point[height];
        }
        Width = width;
        Height = height;
    }

    public GameObject GetPointObject(int x, int y)
    {
        return cloudObjects[x][y];
    }

    public Point GetPointCloudPosition(int x, int y)
    {
        return cloudPoints[x][y];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="point"></param>
    /// <param name="x">The x index of the rendered points.</param>
    /// <param name="y">The y index of the rendered points.</param>
    /// <param name="cloudX">The x value in the point cloud file.</param>
    /// <param name="cloudY">The y value in the point cloud file.</param>
    /// <param name="cloudZ">The z value in the point cloud file.</param>
    public void SetPoint(GameObject point, int x, int y, int cloudX, int cloudY, float cloudZ)
    {
        cloudObjects[x][y] = point;
        cloudPoints[x][y] = new Point(cloudX, cloudY, cloudZ);
    }
}
