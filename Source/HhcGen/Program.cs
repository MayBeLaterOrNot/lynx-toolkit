// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Lynx">
//   The MIT License (MIT)
//   
//   Copyright (c) 2012 Oystein Bjorke
//   
//   Permission is hereby granted, free of charge, to any person obtaining a
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HhcGen
{
    using System;

    using LynxToolkit;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(Application.Header);
            if (args.Length == 0)
            {
                Console.WriteLine(Application.Description);
                Console.WriteLine();
                Console.WriteLine(Application.Comments);
                return;
            }

            string input = null;
            string output = null;
            foreach (var arg in args)
            {
                var kv = arg.Split('=');
                if (kv.Length > 0)
                {
                    switch (kv[0].ToLower())
                    {
                        case "/input":
                            input = kv[1];
                            break;
                        case "/output":
                            output = kv[1];
                            break;
                    }
                }
            }

            Console.WriteLine("Parsing " + input);
            var site = SiteMap.Parse(input);

            Console.WriteLine("Writing to " + output);
            site.WriteContentsFile(output);
        }
    }
}