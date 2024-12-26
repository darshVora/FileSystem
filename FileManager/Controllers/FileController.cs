using FileSystem.Core.IServices;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class FileController(IFileService fileService) : ControllerBase
{
    private readonly IFileService _fileService = fileService;

    [HttpPost("upload")]
    [RequestFormLimits(MultipartBodyLengthLimit = 500 *1024 * 1024)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        await _fileService.ProcessFileAsync(file);

        return Ok("File processed and data pushed to the queue.");
    }

    [HttpGet("data")]
    public IActionResult GetFileData(int pageno = 1, int pagesize = 10, string content = "")
    {
        if (pageno < 1)
        {
            return BadRequest("Page no should be greater than 0");
        }

        if(pagesize < 1 || pagesize >= 1000)
        {
            return BadRequest("Page size should be greater than 0 and less than 1001");
        }

        var data = _fileService.GetFileData(pageno, pagesize, content).ToList();

        return Ok(data);
    }
}
