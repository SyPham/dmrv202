using DMR_API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
   public interface IIngredientInfoReportService : IECService<IngredientInfoReportDto>
    {
        Task<bool> CheckExists(int id); 
        Task<bool> CheckBarCodeExists(string code);
        Task<bool> Add1(IngredientDto1 ingredientIngredientDto);
        Task<bool> AddRangeAsync(List<IngredientForImportExcelDto> model);
        Task<bool> UpdatePrint(QrPrintDto entity);
        Task<IngredientDto> ScanQRCode(string qrCode);

        Task<object> ScanQRCodeFromChemialWareHouse(string qrCode);
    }
}
