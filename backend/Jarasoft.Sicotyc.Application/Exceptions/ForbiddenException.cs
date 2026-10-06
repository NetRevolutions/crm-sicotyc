using System;
using System.Collections.Generic;
using System.Text;

namespace Jarasoft.Sicotyc.Application.Exceptions
{
    public sealed class ForbiddenException(
        string message)
        : Exception(message)
    {
    }
}
