using System.ComponentModel;
using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<SolutionType>))]
public class SolutionType : Enumeration
{
    public static SolutionType Unmanaged = new SolutionType { Value = nameof(Unmanaged) };
    public static SolutionType Managed = new SolutionType { Value = nameof(Managed) };

    public static implicit operator string(SolutionType solutionType)
    {
        return solutionType.Value;
    }
}
