using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Model
{
    internal interface ISave
    {
        string name { get; set; }
        string sourceFilePath { get; set; }
        string targetFilePath { get; set; }
    }
}
