﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ZadanieRekrutacyjne.Interfaces;
using ZadanieRekrutacyjne.Models;

namespace ZadanieRekrutacyjne.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StackOverflowAPIController : ControllerBase
    {
        private readonly IStackOverflowAPIService _stackOverflowAPIService;

        public StackOverflowAPIController(IStackOverflowAPIService stackOverflowAPIService)
        {
            _stackOverflowAPIService = stackOverflowAPIService;
        }


        [HttpGet]
        public async Task<IActionResult> FetchTags()
        {
            try
            {
                await _stackOverflowAPIService.FetchAndSaveTagsAsync();
                return Ok("Tags retrived and saved to data base successfully");
            }
            catch(Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retriving data from SO API. Exception message: {e}");
            }
        }

        [HttpGet("paginated-result")]
        public async Task<ActionResult<IEnumerable<Root>>> GetPaginatedTags(int pageSize, int pageNum, char ordering, string orderBy)
        {
            // to do: implelement exception handling when user tries to take index out of range or when result is empty

            var result = await _stackOverflowAPIService.GetPaginatedResultAsync(pageSize, pageNum, ordering, orderBy);

            if(result.IsNullOrEmpty())
            {
                return NotFound();
            }

            return Ok(result);
            
            
        }


    }
}
