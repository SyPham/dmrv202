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
using DMR_API.SignalrHub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;

namespace DMR_API._Services.Services
{
    public class GlueService : IGlueService
    {
        private readonly IGlueIngredientRepository _repoGlueIngredient;
        private readonly IGlueRepository _repoGlue;
        private readonly IGlueNameRepository _repoGlueName;
        private readonly IPartRepository _repoPart;
        private readonly IKindRepository _repoKind;
        private readonly IMaterialRepository _repoMaterial;
        private readonly IHubContext<ECHub> _hubContext;
        private readonly IModelNameRepository _repoModelName;
        private readonly IBPFCEstablishRepository _repoBPFC;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public GlueService(
            IHttpContextAccessor accessor,
            IGlueRepository repoBrand,
            IGlueNameRepository repoGlueName,
            IModelNameRepository repoModelName,
            IGlueIngredientRepository repoGlueIngredient,
            IPartRepository repoPart,
            IKindRepository repoKind,
            IMaterialRepository repoMaterial,
            IBPFCEstablishRepository repoBPFC,
            IHubContext<ECHub> hubContext,
            IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _accessor = accessor;
            _repoGlue = repoBrand;
            _repoGlueName = repoGlueName;
            _repoPart = repoPart;
            _repoKind = repoKind;
            _repoMaterial = repoMaterial;
            _hubContext = hubContext;
            _repoBPFC = repoBPFC;
            _repoModelName = repoModelName;
            _repoGlueIngredient = repoGlueIngredient;
        }


        private async Task<string> GenatateGlueCode(string code)
        {
            int lenght = 8;
            if (await _repoGlue.FindAll().AnyAsync(x => x.Code.Equals(code)) == true)
            {
                var newCode = CodeUtility.RandomString(lenght);
                return await GenatateGlueCode(newCode);
            }
            return code;

        }
        //Thêm Brand mới vào bảng Glue
        public async Task<bool> Add(GlueCreateDto model)
        {
            using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var glue = _mapper.Map<Glue>(model);
                    // Neu chua co GlueName trong bang GlueName thi them moi va cap nhat ID vao Glues
                    // nguoc lai thi update
                    var glueNameModal = _repoGlueName.FindAll(x => x.Name == model.Name).FirstOrDefault();
                    if (glueNameModal is null)
                    {
                        var glueNameItem = new GlueName { Name = model.Name };
                        _repoGlueName.Add(glueNameItem);
                        await _repoGlueName.SaveAll();

                        glue.GlueNameID = glueNameItem.ID;
                        glue.Name = model.Name;
                    }
                    else
                    {
                        glue.Name = model.Name;
                        glue.GlueNameID = glueNameModal.ID;
                    }
                    glue.isShow = true;
                    glue.Code = await GenatateGlueCode(glue.Code);
                    glue.CreatedDate = DateTime.Now.ToString("MMMM dd, yyyy HH:mm:ss tt");

                    var nameList = new List<int>();
                    foreach (var item in _repoGlue.FindAll().Include(x => x.GlueName).Where(x => x.isShow == true && x.BPFCEstablishID == model.BPFCEstablishID))
                    {
                        if (item.Name.ToInt() > 0)
                        {
                            nameList.Add(item.Name.ToInt());
                        }
                    }
                    if (nameList.Count > 0)
                    {
                        var name = nameList.OrderByDescending(x => x).FirstOrDefault();
                        var nameTemp = name + "";
                        var glueNameModalTemp = _repoGlueName.FindAll(x => x.Name == nameTemp).FirstOrDefault();
                        if (glueNameModal is null)
                        {
                            var glueNameItem = new GlueName { Name = model.Name };
                            _repoGlueName.Add(glueNameItem);
                            _repoGlueName.Save();

                            glue.GlueNameID = glueNameItem.ID;
                            glue.Name = (name + 1).ToString();
                            _repoGlue.Save();

                        }
                        else
                        {
                            glue.Name = model.Name;
                            glue.GlueNameID = glueNameModal.ID;
                            glue.Name = glueNameModalTemp.Name;
                            glue.isShow = true;
                            _repoGlue.Add(glue);
                            _repoGlue.Save();
                        }
                    }


                    _repoGlue.Add(glue);
                    await _repoGlue.SaveAll();
                    transaction.Complete();
                    return true;
                }
                catch 
                {
                    transaction.Dispose();
                    return false;
                }
            }

        }

        //Lấy danh sách Brand và phân trang
        public async Task<PagedList<GlueCreateDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoGlue.FindAll().Where(x => x.isShow == true).ProjectTo<GlueCreateDto>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<GlueCreateDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        //public async Task<object> GetIngredientOfGlue(int glueid)
        //{
        //    return await _repoGlue.GetIngredientOfGlue(glueid);

        //    throw new System.NotImplementedException();
        //}

        //Tìm kiếm glue
        public async Task<PagedList<GlueCreateDto>> Search(PaginationParams param, object text)
        {
            var lists = _repoGlue.FindAll().ProjectTo<GlueCreateDto>(_configMapper)
            .Where(x => x.Code.Contains(text.ToString()))
            .OrderByDescending(x => x.ID);
            return await PagedList<GlueCreateDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        public async Task<bool> CheckExists(int id)
        {
            return await _repoGlue.CheckExists(id);
        }

        //Xóa Brand
        public async Task<bool> Delete(object id)
        {
            var glue = _repoGlue.FindById(id);
            string token = _accessor.HttpContext.Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            // _repoGlue.Remove(glue);
            glue.isShow = false;
            glue.ModifiedBy = userID;
            glue.ModifiedDate = DateTime.Now;
            return await _repoGlue.SaveAll();
        }

        //Cập nhật Brand
        public async Task<bool> Update(GlueCreateDto model)
        {
             using var transaction = new TransactionScopeAsync().Create();
            {
                try
                {
                    var glue = _mapper.Map<Glue>(model);
                    glue.isShow = true;
                    var glueNameModalTemp = _repoGlueName.FindAll(x => x.Name == glue.Name).FirstOrDefault();
                    if (glueNameModalTemp is null) // add GlueLib va update GlueName
                    {
                        var glueNameItem = new GlueName { Name = model.Name };
                        _repoGlueName.Add(glueNameItem);
                        _repoGlueName.Save();
                        await _repoGlueName.SaveAll();

                        glue.GlueNameID = glueNameItem.ID;
                        glue.Name = glueNameItem.Name;
                        await _repoGlue.SaveAll();

                    }
                    else // update GlueName
                    {
                        glue.Name = glueNameModalTemp.Name;
                        glue.GlueNameID = glueNameModalTemp.ID;
                        glue.isShow = true;
                        _repoGlue.Update(glue);
                        await _repoGlue.SaveAll();
                    }
                    await _hubContext.Clients.All.SendAsync("summaryRecieve", "ok");
                    transaction.Complete();
                    return true;

                }
                catch 
                {
                    transaction.Dispose();
                    return false;
                }
            };
        }

        //Cập nhật Brand
        public async Task<bool> UpdateChemical(GlueCreateDto model)
        {
            var glue = _mapper.Map<Glue>(model);
            _repoGlue.Update(glue);
            var item = _repoGlueIngredient.FindAll().FirstOrDefault(x => x.GlueID.Equals(model.ID) && x.Percentage == 100);
            if (item != null)
            {
                item.IngredientID = model.IngredientID;
                return await _repoGlue.SaveAll();
            }
            else
                return false;
        }

        //Lấy toàn bộ danh sách Brand 
        public async Task<List<GlueCreateDto>> GetAllAsync()
        {
            return await _repoGlue.FindAll().ProjectTo<GlueCreateDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        //Lấy Brand theo Brand_Id
        public GlueCreateDto GetById(object id)
        {
            return _mapper.Map<Glue, GlueCreateDto>(_repoGlue.FindById(id));
        }

        public async Task<bool> CheckBarCodeExists(string code)
        {
            return await _repoGlue.CheckBarCodeExists(code);

        }

        public async Task<List<GlueCreateDto1>> GetAllGluesByBPFCID(int BPFCID)
        {
            var lists = await _repoGlue.FindAll(x => x.BPFCEstablishID == BPFCID && x.isShow == true)
                .Include(x=> x.GlueName)
                .Include(x => x.Kind)
                .Include(x => x.Part)
                .Include(x => x.Material)
                .Include(x => x.GlueIngredients)
                .ThenInclude(x => x.Ingredient)
                .ProjectTo<GlueCreateDto1>(_configMapper)
                .OrderByDescending(x => x.ID).ToListAsync();
            return lists;
        }
    }
}