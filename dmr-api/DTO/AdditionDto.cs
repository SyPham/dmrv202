using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class AdditionDto
    {
        public int ID { get; set; }
        public string WorkerID { get; set; }
        public int LineID { get; set; }
        public List<int> LineIDList { get; set; }
        public List<int> IDList { get; set; }

        public string LineName { get; set; }
        public bool IsDelete { get; set; }
        public string BPFCEstablishName { get; set; }
        public int ChemicalID { get; set; }
        public string ChemicalName { get; set; }
        public double Amount { get; set; }
        public int BPFCEstablishID { get; set; }
        public string Remark { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public int ModifiedBy { get; set; }
        public int DeletedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime DeletedTime { get; set; }
    }
  
}
