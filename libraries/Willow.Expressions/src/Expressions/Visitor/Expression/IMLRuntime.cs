using System;
using System.Collections.Generic;

namespace Willow.Expressions;

public interface IMLRuntime
{
    /// <summary>
    /// Executes an ML model
    /// </summary>
    IConvertible Run(IConvertible[][] input);
}
