using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Api.Exceptions
{
    public class ErrorResponse
    {
        public int		StatusCode { get; set; }
        public string	Message    { get; set; }
        public object	Data       { get; set; }
        public string[] CallStack  { get; set; }
    }
}
