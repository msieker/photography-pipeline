using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoPipeline.Commands
{
    internal class Add : Command
    {
        public Add() : base("add", "Adds photos")
        {
            var path = new Option<string>("--path");

            AddOption(path);

        }
    }
}
