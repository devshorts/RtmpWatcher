using System;

namespace com.TheSilentGroup.Fluorine
{
    /// <summary>
    /// The FluorineAccessAllowedAttribute is used to explicitly indicate to Fluorine
    /// that an assembly is for remote access. Fluorine will not resolve access to
    /// any class or method not in an assembly marked with this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public sealed class FluorineAccessAllowedAttribute : Attribute
    {
    }
}
