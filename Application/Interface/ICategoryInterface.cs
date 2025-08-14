
using Application.DTOs;

namespace Application.Interface;

public interface ICategoryInterface
{
    Task<List<CategoryResponse>> Get();
    Task<CategoryResponse> GetById(string id);
    Task<CategoryResponse> Create(CategoryRequest request);
    Task<CategoryResponse> Update(UpdateCategoryRequest request);
    Task<bool> Delete(string id);
    //Task<CategoryResponse> Toggle(string id, bool isActive);
    Task<CategoryResponse> Toggle(string id);
}
