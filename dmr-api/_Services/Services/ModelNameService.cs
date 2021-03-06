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
using Microsoft.Extensions.Configuration;
using DMR_API.Helpers.Enum;
using System.Transactions;

namespace DMR_API._Services.Services
{
    public class ModelNameService : IModelNameService
    {
        private readonly IModelNameRepository _repoModelName;
        private readonly IGlueIngredientRepository _repoGlueIngredient;
        private readonly IGlueRepository _repoGlue;
        private readonly IArticleNoRepository _repoArticleNo;
        private readonly IProcessRepository _repoProcess;
        private readonly IArtProcessRepository _repoArtProcess;
        private readonly IIngredientRepository _repoIngredient;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IBPFCEstablishRepository _repoBPFC;
        private readonly IPlanRepository _repoPlan;
        private readonly IModelNoRepository _repoModelNO;
        private readonly IMailExtension _mailExtension;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public ModelNameService(
            IModelNameRepository repoBrand,
            IGlueRepository repoGlue,
            IGlueIngredientRepository repoGlueIngredient,
            IMailExtension mailExtension,
            IArticleNoRepository repoArticleNo,
            IProcessRepository repoProcess,
            IArtProcessRepository repoArtProcess,
            IIngredientRepository repoIngredient,
            IBPFCEstablishRepository repoBPFC,
            IModelNoRepository repoModelNO,
            ISupplierRepository supplierRepository,
            IConfiguration configuration,
            IPlanRepository repoPlan,
            IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoModelName = repoBrand;
            _repoGlueIngredient = repoGlueIngredient;
            _repoGlue = repoGlue;
            _repoModelNO = repoModelNO;
            _repoArticleNo = repoArticleNo;
            _repoProcess = repoProcess;
            _repoArtProcess = repoArtProcess;
            _repoBPFC = repoBPFC;
            _repoIngredient = repoIngredient;
            _supplierRepository = supplierRepository;
            _configuration = configuration;
            _mailExtension = mailExtension;
            _repoPlan = repoPlan;
        }

        public async Task<ArticleNo> FindArticleNoByCloneDto(CloneDto clone)
        {
            var artNo = await _repoArticleNo.FindAll().FirstOrDefaultAsync(x => x.ModelNoID == clone.ModelNOID && x.Name.ToLower() == clone.Name.ToLower());
            if (artNo != null)
            {
                return artNo;
            }
            else
            {
                var artNoData = new ArticleNo();
                artNoData.ModelNoID = clone.ModelNOID;
                artNoData.Name = clone.Name;
                _repoArticleNo.Add(artNoData);
                await _repoArticleNo.SaveAll();
                return artNoData;
            }
        }
        // new version
        public async Task<object> CloneBPFC(CloneDto clone)
        {
            try
            {
                // Tim cai source

                // Kiem tra neu aricle trung thi k cho clone

                // tao moi BPFC

                // tao moi articleNO cha cua artprocess

                // tao artprocess

                // Clone Glue 

                var bpfc = await _repoBPFC.FindAll()
                .Include(x => x.ModelName)
                .Include(x => x.ModelNo)
                .Include(x => x.ArtProcess)
                .Include(x => x.ArticleNo)
                .Include(x => x.Glues).ThenInclude(x => x.GlueIngredients)
                .Include(x => x.Plans)
                .FirstOrDefaultAsync(x => x.ID == clone.BPFCID); // chua glue 

                var artNo = await FindArticleNoByCloneDto(clone);
                clone.ArticleNOID = artNo.ID;
                var artProcess = await FindArtProcessByCloneDto(clone);
                var bpfcForClone = await FindBPFCDestination(clone, artProcess);
                // Not exists bpfc -> add new -> clone 
                if (bpfcForClone == null)
                {
                    clone.ApprovalBy = bpfc.ApprovalBy;
                    var bpfcClone = await CreateNewBPFCDestination(clone, artProcess.ID);
                    await CloneNewGlueByCloneDto(clone, bpfc, bpfcClone);
                }
                else
                {
                    await CloneNewGlueByCloneDto(clone, bpfc, bpfcForClone);
                }
                return new
                {
                    message = "The BPFC has been cloned!",
                    status = true
                };
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Loi clone model name", ex);
                return new
                {
                    message = "",
                    status = false
                };
            }
        }
        //Th??m Brand m???i v??o b???ng ModelName
        public async Task<bool> Add(ModelNameDto model)
        {
            var ModelName = _mapper.Map<ModelName>(model);
            _repoModelName.Add(ModelName);
            return await _repoModelName.SaveAll();
        }

        //L???y danh s??ch Brand v?? ph??n trang
        public async Task<PagedList<ModelNameDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoModelName.FindAll().ProjectTo<ModelNameDto>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<ModelNameDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        //T??m ki???m ModelName
        public async Task<PagedList<ModelNameDto>> Search(PaginationParams param, object text)
        {
            var lists = _repoModelName.FindAll().ProjectTo<ModelNameDto>(_configMapper)
            .Where(x => x.Name.Contains(text.ToString()))
            .OrderByDescending(x => x.ID);
            return await PagedList<ModelNameDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        //X??a Brand
        public async Task<bool> Delete(object id)
        {
            var ModelName = _repoModelName.FindById(id);
            _repoModelName.Remove(ModelName);
            return await _repoModelName.SaveAll();
        }

        //C???p nh???t Brand
        public async Task<bool> Update(ModelNameDto model)
        {
            var ModelName = _mapper.Map<ModelName>(model);
            _repoModelName.Update(ModelName);
            return await _repoModelName.SaveAll();
        }

        //L???y to??n b??? danh s??ch Brand 
        public async Task<List<ModelNameDto>> GetAllAsync()
        {
            var lists = await _repoModelName.FindAll().ProjectTo<ModelNameDto>(_configMapper).OrderBy(x => x.Name).ToListAsync();
            return lists;
        }
        //L???y to??n b??? danh s??ch Brand 
        public async Task<List<ModelNameDto>> GetAllAsyncForAdmin()
        {
            var lists = await _repoModelName.FindAll().ProjectTo<ModelNameDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
            return lists;
        }

        //L???y Brand theo Brand_Id
        public ModelNameDto GetById(object id)
        {
            return _mapper.Map<ModelName, ModelNameDto>(_repoModelName.FindById(id));
        }

        public Task<int> GetArticleNameQuantityByModelName(int modelName)
        {

            throw new NotImplementedException();
        }

        public Task<bool> CloneModelName(int modelNameID, string modelName, string modelNo, int processID)
        {

            throw new System.NotImplementedException();
        }

        public Task<bool> CloneArticleModelname(int modelNameID, string modelName, string modelNo, string article, int processID)
        {

            throw new System.NotImplementedException();
        }
        public Task<object> Approval(int modelNameID, int userid)
        {

            throw new System.NotImplementedException();
        }

        public Task<object> Done(int modelNameID, int userid)
        {

            throw new System.NotImplementedException();
        }

        public Task<object> Release(int modelNameID, int userid)
        {

            throw new System.NotImplementedException();
        }

        public Task<object> Reject(int modelNameID, int userid)
        {

            throw new System.NotImplementedException();
        }

        public Task<List<ModelNameDto>> FilterByApprovedStatus()
        {

            throw new System.NotImplementedException();
        }

        public Task<List<ModelNameDto>> FilterByNotApprovedStatus()
        {

            throw new System.NotImplementedException();
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public Task<List<ModelNameDto>> FilterByFinishedStatus()
        {

            throw new System.NotImplementedException();
        }

        public async Task SendMailForPIC(string email)
        {
            string subject = "(SHC-902) DMR System - Notification";
            string url = _configuration["MailSettings:API_URL"].ToSafetyString();
            string message = @"
                Notification from Digital Mixing Room <br />
                <b>*PLEASE DO NOT REPLY*</b> this email was automatically sent from the Digital Mixing Room <br />
                The model name has been rejected by suppervisor!!!<br />";
            message += $"<a href='{url}'>Click here to go to the system</a>";
            var emails = new List<string> { email };

            await _mailExtension.SendEmailRangeAsync(emails, subject, message);
        }

        public Task<bool> ImportExcel(List<ModelNameForImportExcelDto> modelNameForImportExcelDtos)
        {

            throw new System.NotImplementedException();
        }

        public Task<List<ModelNameDto>> GetModelNameForBPFCRecord(Status status, string startBuildingDate, string endBuildingDate)
        {

            throw new System.NotImplementedException();


        }
        //L???y to??n b??? danh s??ch Brand 
        public Task<List<ModelNameDtoForBPFCSchedule>> GetAllModelNameForBPFCSchedule()
        {

            // return new List<ModelNameDtoForBPFCSchedule>();
            throw new System.NotImplementedException();

        }

        public async Task<bool> CloneModelName(int modelNameID, int modelNOID, int articleNOID, int processID)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required,
                  new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
                {
                    var bpfc = await _repoBPFC.FindAll()
                                        .Include(x => x.ModelName)
                                        .Include(x => x.ModelNo)
                                        .Include(x => x.ArtProcess)
                                        .Include(x => x.ArticleNo)
                                        .Include(x => x.Glues.Where(x => x.isShow == true))
                                        .Include(x => x.Plans)
                                        .FirstOrDefaultAsync(x => x.ModelNameID == modelNameID
                                    && x.ModelNoID == modelNOID
                                    && x.ArticleNoID == articleNOID
                                    && x.ArtProcessID == processID);
                    if (bpfc == null) return false;
                    var modelNameData = bpfc.ModelName;
                    modelNameData.ID = 0;
                    _repoModelName.Add(modelNameData);
                    await _repoModelName.SaveAll();

                    var modelNoData = bpfc.ModelNo;
                    modelNoData.ID = 0;
                    modelNoData.ModelNameID = modelNameData.ID;
                    _repoModelNO.Add(modelNoData);
                    await _repoModelNO.SaveAll();

                    var articleNOData = bpfc.ArticleNo;
                    articleNOData.ID = 0;
                    articleNOData.ModelNoID = modelNoData.ID;
                    _repoArticleNo.Add(articleNOData);
                    await _repoArticleNo.SaveAll();

                    var artProcessData = bpfc.ArtProcess;
                    artProcessData.ID = 0;
                    artProcessData.ArticleNoID = articleNOData.ID;
                    _repoArtProcess.Add(artProcessData);
                    await _repoArtProcess.SaveAll();

                    var bpfcData = bpfc;
                    bpfcData.ModelName = null;
                    bpfcData.ModelNo = null;
                    bpfcData.ArticleNo = null;
                    bpfcData.ArtProcess = null;
                    bpfcData.Glues = null;
                    bpfcData.Plans = null;
                    bpfcData.ID = 0;
                    bpfcData.ModelNameID = modelNameData.ID;
                    bpfcData.ModelNoID = modelNoData.ID;
                    bpfcData.ArticleNoID = articleNOData.ID;
                    bpfcData.ArtProcessID = artProcessData.ID;
                    _repoBPFC.Add(bpfcData);
                    await _repoBPFC.SaveAll();

                    var gluesData = bpfc.Glues.ToList();
                    gluesData.ForEach(item =>
                    {
                        item.ID = 0;
                        item.BPFCEstablishID = bpfcData.ID;
                    });

                    _repoGlue.AddRange(gluesData);
                    await _repoGlue.SaveAll();

                    var planData = bpfc.Plans.ToList();
                    planData.ForEach(item =>
                    {
                        item.ID = 0;
                        item.BPFCEstablishID = bpfcData.ID;
                    });

                    _repoPlan.AddRange(planData);
                    await _repoPlan.SaveAll();
                    scope.Complete();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi clone model name", ex);
                return false;
            }
        }
        private async Task<string> GenatateGlueCode(string code)
        {
            int lenght = 8;
            if (await _repoGlue.FindAll().AnyAsync(x => x.isShow == true && x.Code.Equals(code)) == true)
            {
                var newCode = RandomString(lenght);
                return await GenatateGlueCode(newCode);
            }
            return code;

        }
        private async Task<bool> CheckExistBPFC(CloneDto model)
        {
            var bpfc = await _repoBPFC.FindAll().AnyAsync(x => x.ModelNameID == model.ModelNameID
                                 && x.ModelNoID == model.ModelNOID
                                 && x.ArticleNoID == model.ArticleNOID
                                 && x.ArtProcessID == model.ArtProcessID);
            return bpfc;
        }
        public async Task<BPFCEstablish> CreateNewBPFCDestination(CloneDto model, int artProcessID)
        {

            var bpfcData = new BPFCEstablish();
            bpfcData.ModelNameID = model.ModelNameID;
            bpfcData.ModelNoID = model.ModelNOID;

            bpfcData.ArticleNoID = model.ArticleNOID;
            bpfcData.ArtProcessID = artProcessID;

            bpfcData.ApprovalStatus = false;
            bpfcData.FinishedStatus = false;
            bpfcData.ApprovalBy = model.ApprovalBy;
            bpfcData.CreatedBy = model.CloneBy;
            bpfcData.UpdateTime = DateTime.Now;
            bpfcData.BuildingDate = DateTime.Now;
            bpfcData.CreatedDate = DateTime.Now;
            _repoBPFC.Add(bpfcData);
            await _repoBPFC.SaveAll();
            return bpfcData;
        }
        /// <summary>
        /// Chuy???n keo t??? bpfcSourceClone -> bpfcDestination
        /// </summary>
        /// <param name="clone"></param>
        /// <param name="bpfcDestination"></param>
        /// <param name="bpfcSourceClone"></param>
        /// <returns></returns>
        public async Task ExportGlueFromBPFCSourceToBPFCDestination(int cloneBy, BPFCEstablish bpfcDestination, BPFCEstablish bpfcSource)
        {
            // T??m keo(Glue) trong BPFC ????ch (BPFC Destination)
            // N???u t???n t???i keo th?? x??a h???t.
            var checkExist = await _repoGlue.FindAll().Where(x => x.BPFCEstablishID == bpfcDestination.ID && x.isShow == true).ToListAsync();
            if (checkExist.Count > 0)
            {
                _repoGlue.RemoveMultiple(checkExist);
                await _repoGlue.SaveAll();
            }

            // L???y keo c???a BPFC ngu???n (BPFC source)
            // N???u ngu???n k c?? keo th?? th??ng b??o kh??ng clone dc
            var gluesData = new List<Glue>();
            gluesData = bpfcSource.Glues.Where(x => x.isShow == true).ToList();
            if (gluesData.Count == 0)
                return;

            foreach (var item in gluesData)
            {

                var glue = new Glue();
                glue.Code = await this.GenatateGlueCode(item.Code);
                glue.Name = item.Name;
                glue.isShow = true;
                glue.Consumption = item.Consumption;
                glue.CreatedBy = cloneBy;
                glue.MaterialID = item.MaterialID;
                glue.KindID = item.KindID;
                glue.PartID = item.PartID;
                glue.GlueNameID = item.GlueNameID;
                glue.ExpiredTime = item.ExpiredTime;
                glue.BPFCEstablishID = bpfcDestination.ID;
                _repoGlue.Add(glue);

                await _repoGlue.SaveAll();

                var glueIngredients = item.GlueIngredients.ToList();
                var glueIngredientData = glueIngredients.Select(x => new GlueIngredient
                {
                    GlueID = glue.ID,
                    IngredientID = x.IngredientID,
                    Allow = x.Allow,
                    Percentage = x.Percentage,
                    CreatedDate = DateTime.Now.ToString("MMMM dd, yyyy HH:mm:ss tt"),
                    Position = x.Position,
                }).ToList();
                _repoGlueIngredient.AddRange(glueIngredientData);
                await _repoGlueIngredient.SaveAll();
            }
        }
        public async Task CloneNewGlueByCloneDto(CloneDto clone, BPFCEstablish bpfc, BPFCEstablish bpfcClone)
        {

            var gluesData = new List<Glue>();
            if (bpfcClone.Glues == null)
            {
                gluesData = bpfc.Glues.Where(x => x.isShow == true).ToList();
            }
            else
            {
                var list2 = bpfcClone.Glues.Where(x => x.isShow == true).Select(x => x.Name);
                var list1 = bpfc.Glues.Where(x => x.isShow == true).Select(x => x.Name);
                var check = list1.Except(list2);
                gluesData = bpfc.Glues.Where(x => x.isShow == true && check.Contains(x.Name)).ToList();
            }
            if (gluesData.Count == 0)
                return;
            foreach (var item in gluesData)
            {
                var glue = new Glue();
                glue.Code = await this.GenatateGlueCode(item.Code);
                glue.Name = item.Name;
                glue.isShow = true;
                glue.Consumption = item.Consumption;
                glue.CreatedBy = clone.CloneBy;
                glue.MaterialID = item.MaterialID;
                glue.KindID = item.KindID;
                glue.PartID = item.PartID;
                glue.GlueNameID = item.GlueNameID;
                glue.ExpiredTime = item.ExpiredTime;
                glue.BPFCEstablishID = bpfcClone.ID;
                _repoGlue.Add(glue);
                await _repoGlue.SaveAll();

                var glueIngredients = item.GlueIngredients.ToList();
                var glueIngredientData = glueIngredients.Select(x => new GlueIngredient
                {
                    GlueID = glue.ID,
                    IngredientID = x.IngredientID,
                    Allow = x.Allow,
                    Percentage = x.Percentage,
                    CreatedDate = DateTime.Now.ToString("MMMM dd, yyyy HH:mm:ss tt"),
                    Position = x.Position,
                }).ToList();
                _repoGlueIngredient.AddRange(glueIngredientData);
                await _repoGlueIngredient.SaveAll();
            }
        }
        /// <summary>
        /// T??m artprocess theo articleNoID v?? processID (artProcessID) ??c g???i t??? client
        /// N???u ch??a c?? th?? th??m m???i
        /// </summary>
        /// <param name="clone"> clone.ArtProcess ????y l?? ProcessID hi???n t???i h??? th???ng ch??? c?? 2 process STF, ASY</param>
        /// <returns>Tr??? v??? ArtProcess</returns>
        public async Task<ArtProcess> FindArtProcessByCloneDto(CloneDto clone)
        {
            var artProcess = await _repoArtProcess.FindAll().FirstOrDefaultAsync(x => x.ArticleNoID == clone.ArticleNOID && x.ProcessID == clone.ArtProcessID);
            if (artProcess != null)
                return artProcess;
            else
            {
                var artProcessData = new ArtProcess();
                artProcessData.ArticleNoID = clone.ArticleNOID;
                artProcessData.ProcessID = clone.ArtProcessID;
                _repoArtProcess.Add(artProcessData);
                await _repoGlueIngredient.SaveAll();
                return artProcessData;
            }
        }
        /// <summary>
        /// T??m BPFC ????ch (BPFC Destination)
        /// </summary>
        /// <param name="clone"></param>
        /// <param name="artProcess"></param>
        /// <returns></returns>
        public async Task<BPFCEstablish> FindBPFCDestination(CloneDto clone, ArtProcess artProcess)
        {
            var bpfcForClone = await _repoBPFC.FindAll()
                                              .Include(x => x.ModelName)
                                              .Include(x => x.ModelNo)
                                              .Include(x => x.ArtProcess)
                                              .Include(x => x.ArticleNo)
                                              .Include(x => x.Glues)
                                              .ThenInclude(x => x.GlueIngredients)
                                              .Include(x => x.Plans)
                                              .FirstOrDefaultAsync(x => x.ModelNameID == clone.ModelNameID
                                                                                              && x.ModelNoID == clone.ModelNOID
                                                                                              && x.ArticleNoID == clone.ArticleNOID
                                                                                          && x.ArtProcessID == artProcess.ID);
            return bpfcForClone;
        }
        /// <summary>
        /// FlowChart
        /// B1: Ch???n BPFC ngu???n v?? ch???n ModelNameID, ModelNoID, ArticleNoID, Process
        /// B2: Ki???m tra BPFC ngu???n c?? trong db ch??a. N???u kh??ng t???n t???i -> K???t th??c
        /// B3: 
        ///     T??m ArtProcess theo ArticleNoID v?? ProcecssID -> Kh??ng t???n t???i -> th??m m???i -> Tr??? v??? ProcessID
        ///     Ki???m BPFC ????ch theo ModelNameID, ModelNoID, ArticleNoID, ArticleNoID, ProcessID c?? trong Db ch??a -> Kh??ng t???n t???i -> th??m m???i -> tr??? v??? BPFC ????ch
        ///     B?????c nh??n b???n: X??a t???t c??? keo c???a BPFC ????ch -> L???y t???t c??? keo c???a BPFC ngu???n th??m v??o BPFC ????ch
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<object> CloneModelName(CloneDto model)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required,
                  new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
                {
                    //B1: T??m bpfc ngu???n
                    var bpfcSource = await _repoBPFC.FindAll()
                    .Include(x => x.ModelName)
                    .Include(x => x.ModelNo)
                    .Include(x => x.ArtProcess)
                    .Include(x => x.ArticleNo)
                    .Include(x => x.Glues).ThenInclude(x => x.GlueIngredients)
                    .Include(x => x.Plans)
                    .FirstOrDefaultAsync(x => x.ID == model.BPFCID);

                    // B2: Ki???m tra ch??a c?? th?? th??ng b??o l???i
                    if (bpfcSource == null)
                        return new
                        {
                            message = "The BPFC is invalid!",
                            status = false
                        };
                    // N???u ch??a t???o process th?? t???o m???i
                    var artProcess = await FindArtProcessByCloneDto(model);

                    // T??m BPFC ????ch
                    var bpfcDestination = await FindBPFCDestination(model, artProcess);
                    // N???u ch??a c?? trong b???ng BPFCEstablish th?? th??m m???i
                    if (bpfcDestination == null)
                    {
                        bpfcDestination = await CreateNewBPFCDestination(model, artProcess.ID);
                    }
                    // Nh??n b???n
                    await ExportGlueFromBPFCSourceToBPFCDestination(model.CloneBy, bpfcDestination, bpfcSource);

                    scope.Complete();
                }
                return new
                {
                    message = "The BPFC has been cloned!",
                    status = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi clone model name", ex);
                return new
                {
                    message = "",
                    status = false
                };
            }
        }
    }
}