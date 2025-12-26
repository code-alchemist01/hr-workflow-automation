using Microsoft.AspNetCore.Mvc;
using NoteApp.Business.Services;
using NoteApp.Business.DTOs;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace NoteApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AudioController : ControllerBase
    {
        private readonly IAudioService _audioService;
        private readonly IMapper _mapper;

        public AudioController(IAudioService audioService, IMapper mapper)
        {
            _audioService = audioService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<AudioResponseDTO>> CreateAudio([FromForm] AudioCreateDTO createDto)
        {
            try
            {
                // --- YENÝ GÜVENLÝK KONTROLÜ BURADA ---
                if (createDto.AudioFile == null)
                {
                    return BadRequest("Lütfen bir ses dosyasý yükleyin.");
                }
                // ------------------------------------
                var audio = await _audioService.SaveAudioAsync(createDto.NoteId, createDto.AudioFile);
                var responseDto = _mapper.Map<AudioResponseDTO>(audio);
                return Ok(responseDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Internal server error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nInner Exception: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"\nInner Inner Exception: {ex.InnerException.InnerException.Message}";
                    }
                }
                return StatusCode(500, errorMessage);
            }
        }

        [HttpPut("{audioId}")]
        public async Task<ActionResult<AudioResponseDTO>> UpdateAudio(int audioId, [FromForm] AudioUpdateDTO updateDto)
        {
            try
            {
                // --- YENÝ GÜVENLÝK KONTROLÜ BURADA ---
                if (updateDto.AudioFile == null)
                {
                    return BadRequest("Lütfen bir ses dosyasý yükleyin.");
                }
                // ------------------------------------
                var audio = await _audioService.UpdateAudioAsync(audioId, updateDto.AudioFile);
                var responseDto = _mapper.Map<AudioResponseDTO>(audio);
                return Ok(responseDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{audioId}")]
        public async Task<ActionResult<AudioResponseDTO>> GetAudio(int audioId)
        {
            try
            {
                var audio = await _audioService.GetAudioAsync(audioId);
                if (audio == null)
                {
                    return NotFound();
                }
                var responseDto = _mapper.Map<AudioResponseDTO>(audio);
                return Ok(responseDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{audioId}/file")]
        public async Task<IActionResult> GetAudioFile(int audioId)
        {
            try
            {
                var audio = await _audioService.GetAudioAsync(audioId);
                if (audio == null)
                {
                    return NotFound();
                }

                var fileStream = await _audioService.GetAudioFileAsync(audioId);

                // --- YENÝ GÜVENLÝK KONTROLÜ BURADA ---
                var contentType = audio.ContentType ?? "application/octet-stream";
                var fileName = audio.FileName ?? $"audio_{audioId}.tmp";
                // ------------------------------------

                return File(fileStream, contentType, fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (FileNotFoundException)
            {
                return NotFound("Audio file not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{audioId}")]
        public async Task<IActionResult> DeleteAudio(int audioId)
        {
            try
            {
                await _audioService.DeleteAudioAsync(audioId);
                return Ok(new { message = "Audio deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AudioResponseDTO>>> GetAllAudios()
        {
            try
            {
                var audios = await _audioService.GetAllAudiosAsync();
                var responseDtos = _mapper.Map<IEnumerable<AudioResponseDTO>>(audios);
                return Ok(responseDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 