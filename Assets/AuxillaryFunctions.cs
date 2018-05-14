using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AuxillaryFunctions
{
    public static int Pow(int bas, int exp)
    {
        return Enumerable
              .Repeat(bas, exp)
              .Aggregate(1, (a, b) => a * b);
    }

    /// <summary>
    /// Gets all points within a distance limit to a ray.
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="container">The container that contains all the points being rendered.</param>
    /// <param name="threshold">The distance threshold.</param>
    /// <returns></returns>
    public static List<Point> a(Ray ray, RenderedCloudContainer container, float threshold)
    {
        List<Point> points = new List<Point>();
        for (int y = 0; y < container.Height; y++)
        {
            for (int x = 0; x < container.Width; x++)
            {
                float dist = Vector3.Cross(ray.direction, container.GetPointObject(x, y).transform.position - ray.origin).magnitude;
                if (dist < threshold)
                    points.Add(container.GetPointCloudPosition(x, y));
            }
        }

        return points;
    }
}
