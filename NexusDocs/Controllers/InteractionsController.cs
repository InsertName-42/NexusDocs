using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusDocs.Data;
using NexusDocs.Models;
using NexusDocs.Models.Dto;

namespace NexusDocs.Controllers
{
    [Route("Interactions")]
    public class InteractionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InteractionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Toggle")]
        public async Task<IActionResult> Toggle([FromBody] GiftToggleDto data)
        {
            if (data == null) return BadRequest();

            //Check for existing interaction
            var existingInteraction = await _context.PageInteractions
                .FirstOrDefaultAsync(i => i.PageId == data.PageId
                                       && i.ElementKey == data.ElementKey
                                       && i.InteractionType == InteractionType.Toggle);

            if (existingInteraction != null)
            {
                existingInteraction.Value = data.Status.ToString().ToLower();
                existingInteraction.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                //Create new record
                var newInteraction = new PageInteraction
                {
                    PageId = data.PageId,
                    ElementKey = data.ElementKey,
                    InteractionType = InteractionType.Toggle,
                    Value = data.Status.ToString().ToLower(),
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PageInteractions.Add(newInteraction);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}