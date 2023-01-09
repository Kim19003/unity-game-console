using System;

namespace Assets.Scripts.Attributes
{
    public enum RelationTargetType
    {
        Assembly,
        Module,
        Class,
        Struct,
        Enum,
        Constructor,
        Method,
        Property,
        Field,
        Event,
        Interface,
        Parameter,
        Delegate,
        ReturnValue,
        GenericParameter,
        All
    }

    /// <summary>
    /// Indicate where something is related to.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class RelatedToAttribute : Attribute
    {
        public string RelationTargetName { get; }
        public RelationTargetType RelationTargetType { get; }

        public RelatedToAttribute(string relationTargetName, RelationTargetType relationTargetType)
        {
            RelationTargetName = relationTargetName;
            RelationTargetType = relationTargetType;
        }
    }
}
