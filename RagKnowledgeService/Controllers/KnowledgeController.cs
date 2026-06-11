using Microsoft.AspNetCore.Mvc;
using RagKnowledgeService.Models;
using RagKnowledgeService.Services;

namespace RagKnowledgeService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KnowledgeController : ControllerBase
    {
        private readonly IRagService _ragService;

        // For Read write isolation, mediator pattern can be implemented here to decouple controller from service logic
        public KnowledgeController(IRagService ragService)
        {
            _ragService = ragService;
        }

        [HttpPost("ask")]
        public ActionResult<AskResponse> Ask([FromBody] AskRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new { Message = "Question payload cannot be empty." });
            }

            try
            {
                var response = _ragService.Ask(request.Question);
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                // Basic error safeguarding for reliable API contracts
                return StatusCode(500, new { Message = "An internal processing error occurred.", Details = ex.Message });
            }
        }
    }
}