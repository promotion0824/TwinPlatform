namespace ConnectorCore.Infrastructure.Exceptions
{
    using System;

    internal class NotFoundException : Exception
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }
    }
}
