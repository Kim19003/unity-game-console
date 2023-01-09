using System;
using UnityEngine;

namespace Assets.Scripts.Other
{
    public class GameConsoleConvert
    {
        public static GameConsoleType<int> ToInt(string value)
        {
            value = value.Trim();
            try
            {
                GameConsoleType<int> gameConsoleType = new GameConsoleType<int>(int.Parse(value));
                return gameConsoleType;
            }
            catch
            {
                return null;
            }
        }

        public static GameConsoleType<float> ToFloat(string value)
        {
            value = value.Trim();
            value = value.Replace(".", ",");
            value = value.Replace("f", string.Empty);
            try
            {
                GameConsoleType<float> gameConsoleType = new GameConsoleType<float>(float.Parse(value));
                return gameConsoleType;
            }
            catch
            {
                return null;
            }
        }

        public static GameConsoleType<bool> ToBool(string value)
        {
            value = value.Trim();
            try
            {
                GameConsoleType<bool> gameConsoleType = new GameConsoleType<bool>(bool.Parse(value));
                return gameConsoleType;
            }
            catch
            {
                return null;
            }
        }

        public static GameConsoleType<char> ToChar(string value)
        {
            value = value.Trim();
            try
            {
                GameConsoleType<char> gameConsoleType = new GameConsoleType<char>(char.Parse(value));
                return gameConsoleType;
            }
            catch
            {
                return null;
            }
        }

        public static GameConsoleType<Vector2> ToVector2(string value)
        {
            value = value.Replace("(", string.Empty);
            value = value.Replace(")", string.Empty);
            string[] values = value.Split(',');

            if (values.Length == 2)
            {
                GameConsoleType<float> firstType = ToFloat(values[0]);
                GameConsoleType<float> secondType = ToFloat(values[1]);

                if (firstType != null && secondType != null)
                {
                    GameConsoleType<Vector2> gameConsoleType = new GameConsoleType<Vector2>(new Vector2(firstType.Value, secondType.Value));
                    return gameConsoleType;
                }
            }
            
            return null;
        }

        public static GameConsoleType<Vector3> ToVector3(string value)
        {
            value = value.Replace("(", string.Empty);
            value = value.Replace(")", string.Empty);
            string[] values = value.Split(',');

            if (values.Length == 3)
            {
                GameConsoleType<float> firstType = ToFloat(values[0]);
                GameConsoleType<float> secondType = ToFloat(values[1]);
                GameConsoleType<float> thirdType = ToFloat(values[2]);
                
                if (firstType != null && secondType != null && thirdType != null)
                {
                    GameConsoleType<Vector3> gameConsoleType = new GameConsoleType<Vector3>(new Vector3(firstType.Value, secondType.Value, thirdType.Value));
                    return gameConsoleType;
                }
            }

            return null;
        }
    }
}
