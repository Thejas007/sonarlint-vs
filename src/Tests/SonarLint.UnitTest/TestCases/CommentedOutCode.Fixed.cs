// Copyright © 2011 - Present RealDimensions Software, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");

using System;







// foo ; {} bar

// ; {} foo

















// &&
// ||
// && &&
// && ||












//




namespace Tests.Diagnostics
{


    /// <summary>
    /// ...
    /// </summary>
    /// <code>
    /// Console.WriteLine("Hello, world!");
    /// </code>
    public class CommentedOutCode
    {
        public void M()
        {
            /* foo */
            M();
            M(); /* foo */
        }


        int a;
        int b;

        // this should be compliant:
        // does *not* overwrite file if (still) exists
    }
}
