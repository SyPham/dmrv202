using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMR_API.Helpers;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;

namespace DMR_API._Services.Services
{
    public class AdditionService : IAdditionService
    {
        private readonly IAdditionRepository _repoAddition;
        private readonly IBPFCEstablishRepository _repoBPFCEstablish;
        private readonly IBuildingRepository _repoBuilding;
        private readonly IIngredientRepository _repoIngredient;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public AdditionService(
            IAdditionRepository repoAddition,
            IBPFCEstablishRepository repoBPFCEstablish,
            IBuildingRepository repoBuilding,
            IIngredientRepository repoIngredient,
            IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoAddition = repoAddition;
            _repoBPFCEstablish = repoBPFCEstablish;
            _repoBuilding = repoBuilding;
            _repoIngredient = repoIngredient;
        }

        //Thêm Addition mới vào bảng addition
        public async Task<bool> Add(AdditionDto model)
        {
            var addition = _mapper.Map<Addition>(model);
            addition.CreatedTime = DateTime.Now;
            _repoAddition.Add(addition);
            return await _repoAddition.SaveAll();
        }

        public async Task<List<BPFCStatusDto>> GetBPFCSchedulesByApprovalStatus ()
        {
            var lists = await _repoBPFCEstablish.FindAll()
                .Include(x => x.Glues)
                .ThenInclude(x => x.GlueName)
                .Include(x => x.Glues)
                .Where(x => x.ApprovalStatus == true && x.FinishedStatus == true && !x.IsDelete).ProjectTo<BPFCStatusDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
            return lists;
        }

        public async Task<List<Building>> GetLinesByBuildingID(int buildingID)
        {
            var lists = await _repoBuilding.FindAll(x=> x.ParentID == buildingID) .ToListAsync();
            return lists;
        }

        //Lấy danh sách Addition và phân trang
        public async Task<PagedList<AdditionDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoAddition.FindAll().ProjectTo<AdditionDto>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<AdditionDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        //Tìm kiếm addition
        public async Task<PagedList<AdditionDto>> Search(PaginationParams param, object text)
        {
            var lists = _repoAddition.FindAll().ProjectTo<AdditionDto>(_configMapper)
            .Where(x => x.Remark.Contains(text.ToString()))
            .OrderByDescending(x => x.ID);
            return await PagedList<AdditionDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        //Xóa Addition
        public async Task<bool> Delete(object id)
        {
            var addition = _repoAddition.FindById(id);
            addition.DeletedBy = addition.CreatedBy;
            addition.IsDelete = true;
            addition.DeletedTime = DateTime.Now;
            _repoAddition.Update(addition);
            return await _repoAddition.SaveAll();
        }

        //Cập nhật Addition
        public async Task<bool> Update(AdditionDto model)
        {
            var addition = _mapper.Map<Addition>(model);
            addition.ModifiedBy = addition.CreatedBy;
            addition.ModifiedTime = DateTime.Now;
            _repoAddition.Update(addition);
            return await _repoAddition.SaveAll();
        }

        //Lấy toàn bộ danh sách Addition 
        public async Task<List<AdditionDto>> GetAllAsync()
        {
            return await _repoAddition.FindAll(x=> !x.IsDelete)
                .Include(x => x.Building)
                .Include(x => x.Ingredient)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                 .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelNo)
                 .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArticleNo)
                 .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArtProcess)
                    .ThenInclude(x => x.Process)
                .ProjectTo<AdditionDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        public async Task<object> GetAllByBuildingID(int buildingID)
        {
            var curentDate = DateTime.Now.Date;
            var lines = await _repoBuilding.FindAll(x => x.ParentID == buildingID).Select(x=>x.ID).ToListAsync();
            if (lines.Count == 0)
                return new List<AdditionDto>();
            var result = new List<AdditionDto>();

            var model = await _repoAddition.FindAll(x => x.CreatedTime.Date == curentDate && lines.Contains(x.LineID) && !x.IsDelete)
                .Include(x => x.Building)
                .Include(x => x.Ingredient)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                 .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelNo)
                 .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArticleNo)
                 .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArtProcess)
                    .ThenInclude(x => x.Process)
                .ProjectTo<AdditionDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
            var groupBy = model.GroupBy(x => new
            {
                x.ChemicalID,
                x.BPFCEstablishID,
                x.Amount,
                x.CreatedTime
            }).ToList();
       
            foreach (var item in groupBy)
            {
                var addition = item.FirstOrDefault();
                var linesList = item.Select(x => x.LineID).ToList();
                var lineNameList = item.OrderBy(x=>x.LineName).Select(x => x.LineName).Distinct().ToList();
                var itemData = new AdditionDto
                {
                    ID = addition.ID,
                    WorkerID = addition.WorkerID,
                    IsDelete = addition.IsDelete,
                    LineName = linesList.Count > 0 ? string.Join(",", lineNameList) : "N/A",
                    LineIDList = linesList.Distinct().ToList(),
                    IDList = item.Select(x=>x.ID).Distinct().ToList(),
                    BPFCEstablishName = addition.BPFCEstablishName,
                    BPFCEstablishID = addition.BPFCEstablishID,
                    ChemicalID = addition.ChemicalID,
                    ChemicalName = addition.ChemicalName,
                    Amount = addition.Amount,
                    Remark = addition.Remark,
                    CreatedBy = addition.CreatedBy,
                    CreatedTime = addition.CreatedTime,
                    DeletedBy = addition.DeletedBy,
                    ModifiedTime = addition.ModifiedTime,
                    DeletedTime = addition.DeletedTime,
                };
                result.Add(itemData);
            }
            return result;
        }

        //Lấy Addition theo Addition_Id
        public AdditionDto GetById(object id)
        {
            return _mapper.Map<Addition, AdditionDto>(_repoAddition.FindById(id));
        }

        public async Task<List<IngredientDto>> GetAllChemical()
        => await _repoIngredient.FindAll()
                .Where(x => x.isShow == true)
                .Include(x => x.Supplier)
                .Include(x => x.GlueType)
                .ProjectTo<IngredientDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();

        public async Task<bool> AddRange(List<AdditionDto> model)
        {
            var addition = _mapper.Map<List<Addition>>(model);
            addition.ForEach(item =>
            {
                item.CreatedTime = DateTime.Now;
            });
            _repoAddition.AddRange(addition);
            return await _repoAddition.SaveAll();
        }

        public async Task<bool> UpdateRange(AdditionDto model)
        {
            var old = await _repoAddition.FindAll(x => model.IDList.Contains(x.ID)).ToListAsync();
            if (old.Count == 0) return false;
            var createdTime = old.FirstOrDefault().CreatedTime;
            var createdBy = old.FirstOrDefault().CreatedBy;
            var result = new List<Addition>();
            foreach (var item in model.LineIDList)
            {
                var itemData = new Addition
                {
                    WorkerID = model.WorkerID,
                    LineID = item,
                    ChemicalID = model.ChemicalID,
                    BPFCEstablishID = model.BPFCEstablishID,
                    Amount = model.Amount,
                    Remark = model.Remark,
                    ModifiedBy = model.ModifiedBy,
                    ModifiedTime = DateTime.Now.ToRemoveTicks(),
                    CreatedBy = createdBy,
                    CreatedTime = createdTime
                };
                result.Add(itemData);
            }
            _repoAddition.RemoveMultiple(old);
            _repoAddition.AddRange(result);
            return await _repoAddition.SaveAll();
        }

        public async Task<bool> DeleteRange(List<int> model, int deleteBy)
        {
           
           var delete = await _repoAddition.FindAll(x=> model.Contains(x.ID)).ToListAsync();
            delete.ForEach(item =>
            {
                item.DeletedBy = deleteBy;
                item.IsDelete = true;
                item.DeletedTime = DateTime.Now;
            });
            _repoAddition.UpdateRange(delete);
            return await _repoAddition.SaveAll();
        }

    }
}