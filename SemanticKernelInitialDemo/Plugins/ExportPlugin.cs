using Google.Protobuf.Collections;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticKernelInitialDemo.Plugins
{
    public class ExportPlugin
    {
        [KernelFunction("export_text")]
        public async void ExportText(string textToExport, string filePath)
        {
            File.WriteAllText(filePath, textToExport);
        }
    }
}
