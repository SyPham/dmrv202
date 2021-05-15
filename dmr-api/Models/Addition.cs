using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Addition
    {
        public int ID { get; set; }
        public string WorkerID { get; set; }
        public int LineID { get; set; }
        public int BPFCEstablishID { get; set; }
        public int ChemicalID { get; set; }
        public bool IsDelete { get; set; }
        public string Remark { get; set; }
        public double Amount { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public int ModifiedBy { get; set; }
        public int DeletedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime DeletedTime { get; set; }
        [ForeignKey("LineID")]
        public virtual Building Building { get; set; }
        [ForeignKey("ChemicalID")]
        public virtual Ingredient Ingredient { get; set; }
        public virtual BPFCEstablish BPFCEstablish { get; set; }
    }
}
