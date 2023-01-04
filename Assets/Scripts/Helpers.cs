using UnityEngine;

public enum CustomColor
{
    Black,
    LightGray
}

public class Helpers
{
    public static Color32 GetCustomColor(CustomColor customColor, byte alpha = 255)
    {
        switch (customColor)
        {
            case CustomColor.LightGray:
                return new Color32(200, 200, 200, alpha);
            default:
                return new Color32(0, 0, 0, alpha);
        }
    }
}
