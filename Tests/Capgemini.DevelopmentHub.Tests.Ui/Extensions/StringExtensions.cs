namespace Capgemini.DevelopmentHub.Tests.Ui.Extensions
{
    using System.Security;

    public static class StringExtensions
    {
        public static SecureString ToSecureString(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
            else
            {
                SecureString result = new SecureString();

                foreach (char c in input.ToCharArray())
                {
                    result.AppendChar(c);
                }

                return result;
            }
        }
    }
}
