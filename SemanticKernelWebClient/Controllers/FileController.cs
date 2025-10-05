// FilesController.cs
using Memory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelInitialDemo.DAL;
using SemanticKernelWebClient.Models;
using SemanticKernelWebClient.SK.RAG;
using System.Collections;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    ConfigurationValues _configValues = null;
    IChatCompletionService _chatCompletionService = null;
    Kernel _kernel = null;
    CookingContext _cookingContext = null;

    public FileController(ConfigurationValues configValues, IChatCompletionService chatCompletionService, Kernel kernel, CookingContext cookingContext)
    {
        _chatCompletionService = chatCompletionService;
        _kernel = kernel;
        _configValues = configValues;
        _cookingContext = cookingContext;

    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file) 
    {
        var mimeType = "text/plain";

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Define the path where the file will be saved
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, file.FileName);

        var matchingRecord = _cookingContext.CustomRecipes.Where(x => x.FilePath == filePath).FirstOrDefault();
        CustomRecipe result = null;

        if (matchingRecord == null)
        {



            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var embeddingGenerator = _kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var vectorStoreFactory = new VectorStoreFactory(embeddingGenerator);
            var vectorStore = vectorStoreFactory.VectorStore;
            var processor = new VectorProcessor(vectorStore, embeddingGenerator);
            RagUploadManager ragUploadManager = new RagUploadManager(_configValues, _chatCompletionService, _kernel);

            var uploadEntry = await ragUploadManager.GetUploadEntryFromFilePathWithContentAsync(filePath);

            await _cookingContext.CustomRecipes.AddAsync(new CustomRecipe
            {
                FilePath = filePath,
            });
            await _cookingContext.SaveChangesAsync();
            result = _cookingContext.CustomRecipes.OrderByDescending(x => x.Id).FirstOrDefault();
        }
        else
        {
            result = matchingRecord;
        }

        var hadMatchingRecord = matchingRecord != null;

        var message = String.Format("{0} - {1} - {2} - {3}",
            hadMatchingRecord ? "That file was already uploaded!" : "Added the file!",
            result.Id,
            result.FilePath,
            result.CreatedOn.ToString("yyyyMMddHHmmss"));

        return Ok(new { Message = message, FileName = file.FileName });
    }

}