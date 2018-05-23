using UnityEngine;

public class IgfHeight : MonoBehaviour
{
    /// <summary>
    /// The x pos in [0, 512]
    /// </summary>
    public int startX { get; set; }
    public int startY { get; set; }
    public float startZ { get; set; }
    public Quaternion Rotation { get { return transform.rotation; } }
}
