using DynamicExpresso;
using Swifter.Json;
using Swifter.Readers;
using Swifter.RW;
using Swifter.NewScript;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace Swifter.Test
{
    class Program
    {
        public unsafe static void Main()
        {
            var exp = "-123+-123";

            Console.WriteLine("Swifter result : " + Engine.Compile(exp).Eval(new NewScript.Environment()).GetString());
        }
    }
}