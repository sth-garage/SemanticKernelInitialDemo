// FilesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelWebClient.DAL.Context;
using SemanticKernelWebClient.Shared.Models;
using System.Collections;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    ConfigurationValues _configValues = null;
    IChatCompletionService _chatCompletionService = null;
    Kernel _kernel = null;
    SemanticKernelWebClientDBContext _context = null;

    public FileController(ConfigurationValues configValues, IChatCompletionService chatCompletionService, Kernel kernel, SemanticKernelWebClientDBContext context)
    {
        _chatCompletionService = chatCompletionService;
        _kernel = kernel;
        _configValues = configValues;
        _context = context;

    }

}