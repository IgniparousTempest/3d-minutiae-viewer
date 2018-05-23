﻿using UnityEngine;

public class Minutiae3D : MonoBehaviour
{
    /// <summary>
    /// The x pos in [0, 512]
    /// </summary>
    public int X { get; set; }
    public int Y { get; set; }
    public float Z { get; set; }
    public Quaternion Rotation { get { return transform.rotation; } }

    public void Draw()
    {
        transform.position = new Vector3(X, Z, Y);
        transform.rotation = Rotation;
    }
}
