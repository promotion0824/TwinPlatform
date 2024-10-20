namespace Connector.XL.Common.Models;

internal class ConnectorExportImportOptions
{
    public string StorageAccountName { get; set; }

    public string StorageKey { get; set; }

    public string ExportFunctionKey { get; set; }

    public string ImportFunctionKey { get; set; }

    public AsyncImportFunctionOptions AsyncImportFunction { get; set; }

    public class AsyncImportFunctionOptions
    {
        public string ImportStartKey { get; set; }

        public string DurableTaskKey { get; set; }

        public string DurableTaskHub { get; set; }

        public string DurableTaskConnection { get; set; }
    }
}
