using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using Microsoft.AspNetCore.Mvc;

namespace DMR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AdditionController : ControllerBase
    {
        private readonly IAdditionService _additionService;
        public AdditionController(IAdditionService additionService)
        {
            _additionService = additionService;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetLines([FromQuery]PaginationParams param)
        //{
        //    var lines = await _lineService.GetWithPaginations(param);
        //    Response.AddPagination(lines.CurrentPage,lines.PageSize,lines.TotalCount,lines.TotalPages);
        //    return Ok(lines);
        //}

        [HttpGet]
        public async Task<IActionResult> GetBPFCSchedulesByApprovalStatus()
        {
            var data = await _additionService.GetBPFCSchedulesByApprovalStatus();
            return Ok(data);
        }

        [HttpGet("{buildingID}")]
        public async Task<IActionResult> GetLinesByBuildingID(int buildingID)
        {
            var data = await _additionService.GetLinesByBuildingID(buildingID);
            return Ok(data);
        }
        [HttpGet("{buildingID}")]
        public async Task<IActionResult> GetAllByBuildingID(int buildingID)
        {
            var data = await _additionService.GetAllByBuildingID(buildingID);
            return Ok(data);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllChemical()
        {
            var lines = await _additionService.GetAllChemical();
            return Ok(lines);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lines = await _additionService.GetAllAsync();
            return Ok(lines);
        }

        //[HttpGet("{text}")]
        //public async Task<IActionResult> Search([FromQuery]PaginationParams param, string text)
        //{
        //    var lists = await _lineService.Search(param, text);
        //    Response.AddPagination(lists.CurrentPage, lists.PageSize, lists.TotalCount, lists.TotalPages);
        //    return Ok(lists);
        //}
        [HttpPost]
        public async Task<IActionResult> CreateRange(List<AdditionDto> create)
        {
            if (await _additionService.AddRange(create))
            {
                return NoContent();
            }

            throw new Exception("Creating the addition failed on save");
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteRange([FromQuery]List<int> idList, [FromQuery] int deleteBy)
        {
            if (await _additionService.DeleteRange(idList, deleteBy))
            {
                return NoContent();
            }

            throw new Exception("Deleting the addition failed on save");
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRange(AdditionDto update)
        {

            if (await _additionService.UpdateRange(update))
                return NoContent();
            return BadRequest($"Updating Addition failed on save");
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdditionDto create)
        {

            if (_additionService.GetById(create.ID) != null)
                return BadRequest("Addition ID already exists!");
            //create.CreatedDate = DateTime.Now;
            if (await _additionService.Add(create))
            {
                return NoContent();
            }

            throw new Exception("Creating the addition failed on save");
        }

        [HttpPut]
        public async Task<IActionResult> Update(AdditionDto update)
        {
            if (await _additionService.Update(update))
                return NoContent();
            return BadRequest($"Updating Addition {update.ID} failed on save");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _additionService.Delete(id))
                return NoContent();
            throw new Exception("Error deleting the Addition");
        }
    }
}