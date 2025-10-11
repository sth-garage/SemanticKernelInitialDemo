using System;
using System.Collections.Generic;

namespace SemanticKernelInitialDemo.DAL;

public partial class CustomRecipe
{
    public int Id { get; set; }

    public DateTime CreatedOn { get; set; }

    public string FilePath { get; set; } = null!;
}
