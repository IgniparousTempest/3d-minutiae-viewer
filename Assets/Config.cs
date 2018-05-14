using System;

[Serializable]
public struct Config
{
    public string featureFilePath;
    public string pointCloudFilePath;
    public int scalingFactorPower;

    public Config(string featureFilePath, string pointCloudFilePath, int scalingFactorPower)
    {
        this.featureFilePath = featureFilePath;
        this.pointCloudFilePath = pointCloudFilePath;
        this.scalingFactorPower = scalingFactorPower;
    }
}
