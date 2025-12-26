using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NoteApp.Business.DTOs.Tag;
using NoteApp.Business.Services;
using NoteApp.DataAccess.Entities;
using System.Security.Claims;

namespace NoteApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TagController : ControllerBase
    {
        private readonly TagService _tagService;

        public TagController(TagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<List<TagResponseDTO>>> GetMyTags()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var tags = await _tagService.GetAllTagsAsync(userId);
            return Ok(tags);
        }

        [HttpGet]
        public async Task<ActionResult<List<TagResponseDTO>>> GetAllTags()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var tags = await _tagService.GetAllTagsAsync(userId);
            return Ok(tags);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TagResponseDTO>> GetTagById(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var tag = await _tagService.GetTagByIdAsync(id, userId);
            if (tag == null)
                return NotFound();

            return Ok(tag);
        }

        [HttpPost]
        public async Task<ActionResult<TagResponseDTO>> CreateTag(TagCreateDto tagDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var tag = await _tagService.CreateTagAsync(tagDto, userId);
            return CreatedAtAction(nameof(GetTagById), new { id = tag.Id }, tag);
        }

        [HttpPut]
        public async Task<ActionResult<TagResponseDTO>> UpdateTag(TagUpdateDto tagDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var tag = await _tagService.UpdateTagAsync(tagDto, userId);
            if (tag == null)
                return NotFound();

            return Ok(tag);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTag(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _tagService.DeleteTagAsync(id, userId);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
} 