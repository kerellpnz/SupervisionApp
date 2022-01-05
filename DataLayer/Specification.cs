﻿using DataLayer.Files;
using System.Collections.ObjectModel;

namespace DataLayer
{
    public class Specification : BaseTable
    {
        public string Number { get; set; }
        public string Program { get; set; }
        public string Consignee { get; set; }
        public string Facility { get; set; }
        public string Supplier { get; set; }
        public bool IsSupervisionRequest { get; set; } 

        public int? CustomerId { get; set; }
        public Customer Customer { get; set; }

        public ObservableCollection<SpecificationWithFile> Files { get; set; }
        public ObservableCollection<PID> PIDs { get; set; }
    }
}
