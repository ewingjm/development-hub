namespace DevelopmentHub.Tests.Ui.Extensions
{
    using System.Security;

    /// <summary>
    /// Extensions to <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string to a <see cref="SecureString"/>.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The secure string.</returns>
        public static SecureString ToSecureString(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
            else
            {
                var result = new SecureString();

                foreach (char c in input.ToCharArray())
                {
                    result.AppendChar(c);
                }

                return result;
            }
        }
    }
}
