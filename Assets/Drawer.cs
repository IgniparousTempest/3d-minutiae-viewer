using UnityEngine;

public class Drawer : MonoBehaviour {
	public GameObject pointPrefab;
    public float width { get; set; }

    public void DrawCloud(PointCloud cloud, int scalingFactor, Transform parent)
    {
        RenderedCloudContainer container = parent.gameObject.GetComponent<RenderedCloudContainer>();
        container.ResetCloud(cloud.Width / scalingFactor, cloud.Height / scalingFactor);

        float offset = width / (cloud.Width - 1);
        float scale = 0.25f * 32f / cloud.Width * scalingFactor;
        Vector3 scaleVec = new Vector3(scale, scale, scale);
        for (int yi = 0; yi < cloud.Height; yi += scalingFactor) {
			for (int xi = 0; xi < cloud.Width; xi += scalingFactor) {
            	GameObject point = Instantiate(pointPrefab, new Vector3(xi * offset, cloud.Get(xi, yi) * offset, yi * offset), Quaternion.identity);
				point.name = string.Format("point ({0}, {1})", xi, yi);
				point.transform.parent = parent;
                point.transform.localScale = scaleVec;
                container.SetPoint(point, xi / scalingFactor, yi / scalingFactor, xi, yi, cloud.Get(xi, yi));
            }
		}
	}

    public Vector3 IndexPointToWorldPoint(PointCloud cloud, Point point, int scalingFactor)
    {
        float offset = width / (cloud.Width - 1);
        return new Vector3(point.X * offset, cloud.Get(point.X, point.Y) * offset, point.Y * offset);
    }
}
