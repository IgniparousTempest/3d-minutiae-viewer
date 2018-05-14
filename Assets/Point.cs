public class Point
{
    public int X { get; set; }
    public int Y { get; set; }
    public float Z { get; set; }

    public Point(int x, int y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString()
    {
        return string.Format("Point({0}, {1}, {2})", X, Y, Z);
    }
}