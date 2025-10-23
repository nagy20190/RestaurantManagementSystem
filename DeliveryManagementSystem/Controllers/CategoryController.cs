using AutoMapper;
using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryController> _logger;


        public CategoryController(IGenericRepository<Category> categoryRepository,
            IMapper mapper,
            ILogger<CategoryController> logger)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory(CreateCategoryDTO categoryDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (categoryDTO == null)
            {
                return BadRequest("Category data is null.");
            }
            try
            {
                // Check if category with same name already exists
                var existingCategory = await _categoryRepository
                    .FindByCondition(c => c.Name.ToLower() == categoryDTO.Name.ToLower())
                    .FirstOrDefaultAsync();

                if (existingCategory != null)
                {
                    return Conflict(new { Message = $"Category with name '{categoryDTO.Name}' already exists." });
                }
                var category = _mapper.Map<Category>(categoryDTO);
                await _categoryRepository.AddAsync(category);
                return CreatedAtAction(
                    nameof(GetCategoryById),
                    new { id = category.ID },
                    categoryDTO);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a category.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }

        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllCategories(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                // Validate pagination parameters
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Min(100, Math.Max(1, pageSize));

                var query = _categoryRepository.GetAll();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(c => c.Name.Contains(searchTerm) ||
                                           (c.Description != null && c.Description.Contains(searchTerm)));
                }
                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var categories = await query
                    .OrderBy(c => c.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var categoryDTOs = _mapper.Map<List<CategoryDTO>>(categories);

                var result = new PaginatedResult<CategoryDTO>
                {
                    Items = categoryDTOs,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, new { Message = "An error occurred while retrieving categories." });
            }
        }



        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);

                var categoryDTO = _mapper.Map<CategoryDTO>(category);
                return Ok(categoryDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID: {CategoryId}", id);
                return StatusCode(500, new { Message = "An error occurred while retrieving the category." });
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult>
            UpdateCategory([FromRoute] int id, [FromBody] UpdateCategoryDTO updateCategoryDTO)
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "Invalid category ID." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingCategory = await _categoryRepository.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound(new { Message = $"No category found with ID {id}." });
                }

                // Check for duplicate name (excluding current category)
                var duplicateCategory = await _categoryRepository
                    .FindByCondition(c => c.Name.ToLower() == updateCategoryDTO.Name.ToLower() && c.ID != id)
                    .FirstOrDefaultAsync();

                if (duplicateCategory != null)
                {
                    return Conflict(new { Message = $"Category with name '{updateCategoryDTO.Name}' already exists." });
                }

                // Map the updated properties
                _mapper.Map(updateCategoryDTO, existingCategory);

                await _categoryRepository.UpdateAsync(existingCategory);

                var updatedCategoryDTO = _mapper.Map<CategoryDTO>(existingCategory);

                _logger.LogInformation("Category updated successfully with ID: {CategoryId}", id);

                return Ok(updatedCategoryDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID: {CategoryId}", id);
                return StatusCode(500, new { Message = "An error occurred while updating the category." });
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> HardDeleteCategory([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "Invalid category ID." });
            }
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { Message = $"No category found with ID {id}." });
                }
                await _categoryRepository.DeleteAsync(category);
                _logger.LogWarning("Category hard deleted with ID: {CategoryId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hard deleting category with ID: {CategoryId}", id);
                return StatusCode(500, new { Message = "An error occurred while deleting the category." });
            }
        }



    }
}
