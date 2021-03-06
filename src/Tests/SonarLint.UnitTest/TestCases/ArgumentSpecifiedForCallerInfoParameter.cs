﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tests.Diagnostics
{
    class ArgumentSpecifiedForCallerInfoParameter
    {
        void TraceMessage(string message,
          [CallerMemberName] string memberName = "",
          [CallerFilePath] string filePath = "",
          [CallerLineNumber] int lineNumber = 0)
        {
            /* ... */
        }

        void MyMethod()
        {
            TraceMessage("my message", "MyMethod"); // Noncompliant
            TraceMessage("my message");
            TraceMessage("my message", filePath: "aaaa"); // Noncompliant
        }
    }
}
