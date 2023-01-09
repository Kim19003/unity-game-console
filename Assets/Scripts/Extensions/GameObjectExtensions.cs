using Assets.Scripts.Other;
using System;
using UnityEngine;
using GameConsoleConvert = Assets.Scripts.Other.GameConsoleConvert;

namespace Assets.Scripts.Extensions
{
    public static class GameObjectExtensions
    {
        public static readonly string[] AttributeNames = new string[]
        {
            "activate",
            "name",
            "tag",
            "layer",
            "position",
            "rotation",
            "scale",
            "activeInHierarchy",
            "activeSelf",
            "isStatic",
            "hideFlags"
        };

        public static SetterResult SetAttributeValue(this GameObject gameObject, string attributeName, string attributeValue)
        {
            switch (attributeName)
            {
                case "activate":
                    object gameConsoleType = GameConsoleConvert.ToBool(attributeValue);
                    if (gameConsoleType != null)
                    {
                        gameObject.SetActive(((GameConsoleType<bool>)gameConsoleType).Value);
                        return SetterResult.Successful;
                    }
                    return SetterResult.ValueNotAllowed;
                case "name":
                    gameObject.name = attributeValue;
                    return SetterResult.Successful;
                case "tag":
                    gameObject.tag = attributeValue;
                    return SetterResult.Successful;
                case "layer":
                    gameConsoleType = GameConsoleConvert.ToInt(attributeValue);
                    if (gameConsoleType != null)
                    {
                        gameObject.layer = ((GameConsoleType<int>)gameConsoleType).Value;
                        return SetterResult.Successful;
                    }
                    return SetterResult.ValueNotAllowed;
                case "position":
                    gameConsoleType = GameConsoleConvert.ToVector3(attributeValue);
                    if (gameConsoleType != null)
                    {
                        gameObject.transform.position = ((GameConsoleType<Vector3>)gameConsoleType).Value;
                        return SetterResult.Successful;
                    }
                    return SetterResult.ValueNotAllowed;
                case "rotation":
                    gameConsoleType = GameConsoleConvert.ToVector3(attributeValue);
                    if (gameConsoleType != null)
                    {
                        gameObject.transform.rotation = Quaternion.Euler(((GameConsoleType<Vector3>)gameConsoleType).Value);
                        return SetterResult.Successful;
                    }
                    return SetterResult.ValueNotAllowed;
                case "scale":
                    gameConsoleType = GameConsoleConvert.ToVector3(attributeValue);
                    if (gameConsoleType != null)
                    {
                        gameObject.transform.localScale = ((GameConsoleType<Vector3>)gameConsoleType).Value;
                        return SetterResult.Successful;
                    }
                    return SetterResult.ValueNotAllowed;
                case "activeInHierarchy":
                case "activeSelf":
                    gameConsoleType = GameConsoleConvert.ToBool(attributeValue);
                    if (gameConsoleType != null)
                    {
                        gameObject.SetActive(((GameConsoleType<bool>)gameConsoleType).Value);
                        return SetterResult.Successful;
                    }
                    return SetterResult.ValueNotAllowed;
                case "isStatic":
                    gameConsoleType = GameConsoleConvert.ToBool(attributeValue);
                    if (gameConsoleType != null)
                    {
                        gameObject.isStatic = ((GameConsoleType<bool>)gameConsoleType).Value;
                        return SetterResult.Successful;
                    }
                    return SetterResult.ValueNotAllowed;
                case "hideFlags":
                    gameConsoleType = GameConsoleConvert.ToInt(attributeValue);
                    if (gameConsoleType != null)
                    {
                        gameObject.hideFlags = (HideFlags)((GameConsoleType<int>)gameConsoleType).Value;
                        return SetterResult.Successful;
                    }
                    return SetterResult.ValueNotAllowed;
                default:
                    return SetterResult.TargetNotFound;
            }
        }

        public static (GetterResult Result, string Value) GetAttributeValue(this GameObject gameObject, string attributeName)
        {
            switch (attributeName)
            {
                case "activate":
                    return (GetterResult.Successful, gameObject.activeSelf.ToString());
                case "name":
                    return (GetterResult.Successful, gameObject.name);
                case "tag":
                    return (GetterResult.Successful, gameObject.tag);
                case "layer":
                    return (GetterResult.Successful, gameObject.layer.ToString());
                case "position":
                    return (GetterResult.Successful, gameObject.transform.position.ToString("F3"));
                case "rotation":
                    return (GetterResult.Successful, gameObject.transform.rotation.eulerAngles.ToString("F3"));
                case "scale":
                    return (GetterResult.Successful, gameObject.transform.localScale.ToString("F3"));
                case "activeInHierarchy":
                    return (GetterResult.Successful, gameObject.activeInHierarchy.ToString());
                case "activeSelf":
                    return (GetterResult.Successful, gameObject.activeSelf.ToString());
                case "isStatic":
                    return (GetterResult.Successful, gameObject.isStatic.ToString());
                case "hideFlags":
                    return (GetterResult.Successful, ((int)gameObject.hideFlags).ToString());
                default:
                    return (GetterResult.TargetNotFound, string.Empty);
            }
        }
    }
}
