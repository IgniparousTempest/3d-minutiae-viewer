using System.Collections.Generic;
using System.Globalization;

public class PointCloud {
	public int Width { get; private set; }
	public int Height { get; private set; }
	private List<List<float>> data;

	public PointCloud(string filePath) {
		data = new List<List<float>>();
		// Read the file and display it line by line.
		using (System.IO.StreamReader file = new System.IO.StreamReader(filePath)) {
			string line;
			while((line = file.ReadLine()) != null) {
				string[] lineArray = line.Split(',');
				var newLine = new List<float>(lineArray.Length);
				for (int i = 0; i < lineArray.Length; i++) {
					newLine.Add(float.Parse(lineArray[i], CultureInfo.InvariantCulture.NumberFormat));
				}
				data.Add(newLine);
			}

			Width = data[0].Count;
			Height = data.Count;

			for (int i = 0; i < data.Count; i++) {
				if (data[i].Count != Width)
					throw new System.ArgumentException("Inconsistent width");
			}
		}
	}

	public float Get(int x, int y) {
		return data[y][x];
	}
}
