using UnityEngine;

public class IgfWidth : MonoBehaviour
{
    /// <summary>
    /// The x pos in [0, 512]
    /// </summary>
    public int startX { get; set; }
    public int startY { get; set; }
    public float startZ { get; set; }
    public int endX { get; set; }
    public int endY { get; set; }
    public float endZ { get; set; }
    public Quaternion Rotation { get { return transform.rotation; } }

    public void Draw()
    {
        transform.position = new Vector3(startX, startZ, startY);
        transform.rotation = Rotation;
    }
}
