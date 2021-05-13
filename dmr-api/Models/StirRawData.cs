using System;

namespace DMR_API.Models
{
    public class StirRawData
    {
        public int ID { get; set; }
        public int BuildingID { get; set; }
        public int MachineID { get; set; }
        public double RPM { get; set; }
        public string Building { get; set; }
        public int Duration { get; set; }
        public int Sequence { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}