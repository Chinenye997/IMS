using Application.DTOs;
using Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ICategoryInterface _categoryService;

    public CategoryController(ICategoryInterface categoryService)
    {
        _categoryService = categoryService;
    }


    [HttpGet]
    public async Task<ActionResult> Get()
    {
        var categories = await _categoryService.Get();
        if (categories.Any())
        {
            return StatusCode(200, categories);
        }
        return StatusCode(404, "Not found");
    }


    [HttpPost]
    public async Task<IActionResult> Create(CategoryRequest request)
    {
        var categories = await _categoryService.Create(request);
        return Ok(categories);
    }
}
