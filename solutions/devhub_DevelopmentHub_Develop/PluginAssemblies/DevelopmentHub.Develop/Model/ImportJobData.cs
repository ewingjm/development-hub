namespace DevelopmentHub.Develop.Model
{
    using System;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// The result of a solution import job.
    /// </summary>
    public class ImportJobData
    {
        /// <summary>
        /// Gets the result of the import.
        /// </summary>
        public ImportResult ImportResult { get; private set; }

        /// <summary>
        /// Gets the error text.
        /// </summary>
        public string ErrorText { get; private set; }

        /// <summary>
        /// Gets the status of the import.
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// Parses the data string returned by and import job.
        /// </summary>
        /// <param name="dataXml">The data XML string.</param>
        /// <returns>An intance of <see cref="ImportJobData"/>.</returns>
        public static ImportJobData ParseXml(string dataXml)
        {
            var importJobData = new ImportJobData();
            var document = XDocument.Parse(dataXml);
            var result = document.Descendants().First(el => el.Name.LocalName == "solutionManifest").Element("result");

            importJobData.ImportResult = (ImportResult)Enum.Parse(typeof(ImportResult), result.Attribute("result").Value, true);
            importJobData.ErrorText = result.Attribute("errortext").Value;
            importJobData.Status = document.Root.Attribute("status")?.Value;

            return importJobData;
        }
    }
}
