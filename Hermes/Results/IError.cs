using System;
using System.Collections.Generic;
using System.Text;

namespace Hermes.Results;

public interface IError
{
    public string Code { get; init; }
    public string Message { get; init; }
    public Dictionary<string, string?>? Metadata { get; init; }
}
