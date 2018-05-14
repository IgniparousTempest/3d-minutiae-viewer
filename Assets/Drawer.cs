using UnityEngine;

public class Drawer : MonoBehaviour {
	public GameObject pointPrefab;
    public float width { get; set; }
    private Color[] scaleColours = { new Color(52f / 255f, 0f / 255f, 66f / 255f), new Color(31f / 255f, 121f / 255f, 122f / 255f), new Color(252f / 255f, 229f / 255f, 30f / 255f) };

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

        ColourCloud(container);
	}

    public void ColourCloud(RenderedCloudContainer container)
    {
        float lowest = float.PositiveInfinity;
        float highest = float.NegativeInfinity;
        for (int yi = 0; yi < container.Height; yi++)
            for (int xi = 0; xi < container.Width; xi++)
                if (container.GetPointCloudPosition(xi, yi).Z > highest)
                    highest = container.GetPointCloudPosition(xi, yi).Z;
                else if (container.GetPointCloudPosition(xi, yi).Z < lowest)
                    lowest = container.GetPointCloudPosition(xi, yi).Z;

        highest -= lowest;
        for (int yi = 0; yi < container.Height; yi++)
        {
            for (int xi = 0; xi < container.Width; xi++)
            {
                float percent = (container.GetPointCloudPosition(xi, yi).Z - lowest) / highest;
                float r, g, b;
                if (percent > 0.5f)
                {
                    r = Mathf.Lerp(scaleColours[1].r, scaleColours[2].r, (percent - 0.5f) * 2f);
                    g = Mathf.Lerp(scaleColours[1].g, scaleColours[2].g, (percent - 0.5f) * 2f);
                    b = Mathf.Lerp(scaleColours[1].b, scaleColours[2].b, (percent - 0.5f) * 2f);
                }
                else
                {
                    r = Mathf.Lerp(scaleColours[0].r, scaleColours[1].r, percent * 2f);
                    g = Mathf.Lerp(scaleColours[0].g, scaleColours[1].g, percent * 2f);
                    b = Mathf.Lerp(scaleColours[0].b, scaleColours[1].b, percent * 2f);
                }
                GameObject point = container.GetPointObject(xi, yi);
                point.GetComponent<Renderer>().material.color = new Color(r, g, b);
            }
        }
    }

    public Vector3 IndexPointToWorldPoint(PointCloud cloud, Point point, int scalingFactor)
    {
        float offset = width / (cloud.Width - 1);
        return new Vector3(point.X * offset, cloud.Get(point.X, point.Y) * offset, point.Y * offset);
    }
}
