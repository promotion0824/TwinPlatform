namespace Willow.ExceptionHandling.Exceptions;

/// <summary>
/// FileParseException custom exception class for import file parsing errors.
/// </summary>
public class FileParseException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileParseException"/> class.
    /// Default parameterless constructor.
    /// </summary>
    public FileParseException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileParseException"/> class.
    /// </summary>
    /// <param name="rowNumber">Row number of the error line.</param>
    /// <param name="columnName">Column name where the error occured while parsing.</param>
    /// <param name="message">Parse error message.</param>
    /// <param name="innerException">Exception instance.</param>
    public FileParseException(int rowNumber, string columnName, string message, Exception innerException)
        : base(FormatErrorMessage(rowNumber, columnName, message), innerException)
    {
    }

    private static string FormatErrorMessage(int rowNumber, string columnName, string message)
    {
        return string.Format("Error parsing Line {0}, Column : {1} \n {2} \n", rowNumber, columnName, message);
    }
}
