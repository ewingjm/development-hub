namespace Capgemini.DevelopmentHub.BusinessLogic.Extensions
{
    using System;
    using System.Activities;

    /// <summary>
    /// Extensions to the <see cref="InArgument"/> class.
    /// </summary>
    public static class InArgumentExtensions
    {
        /// <summary>
        /// Throws an exception if the value is null.
        /// </summary>
        /// <param name="inArgument">The argument.</param>
        /// <param name="context">The context.</param>
        /// <param name="argumentName">The name of the argument.</param>
        /// <typeparam name="T">The argument type.</typeparam>
        /// <returns>The argument value.</returns>
        public static T GetRequired<T>(this InArgument<T> inArgument, ActivityContext context, string argumentName)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var value = inArgument.Get<T>(context);

            if ((value is string && string.IsNullOrEmpty(value as string)) || value == null)
            {
                throw new ArgumentNullException(argumentName);
            }

            return value;
        }
    }
}
