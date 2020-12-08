/// <summary>
/// Configuration for a Dataverse solution.
/// </summary>
public class SolutionConfiguration
{
    private string masterProfile;

    /// <summary>
    /// The PAC profile to use for the development environment
    /// </summary>
    public string DevelopmentProfile { get; set; }

    /// <summary>
    /// The PAC profile to use for the master environment
    /// </summary>
    public string MasterProfile
    {
        get
        {
            return string.IsNullOrEmpty(this.masterProfile) ? this.DevelopmentProfile : this.masterProfile;
        }
        set
        {
            this.masterProfile = value;
        }
    }

    /// <summary>
    /// Dependencies configuration
    /// </summary>
    public SolutionDependencyConfiguration Dependencies { get; set; }

    /// <summary>
    /// Configuration relevant to a solution's dependencies.
    /// </summary>
    public class SolutionDependencyConfiguration
    {
        /// <summary>
        /// Solutions to not attempt to resolve from local solution projects when building (usually out-of-the-box solutions).
        /// </summary>
        public string[] NoResolve { get; set; }
    }
}