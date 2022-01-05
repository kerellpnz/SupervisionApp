using System;
using System.Collections.Generic;
using System.Text;

namespace DataLayer
{
    public class VersionControl : BaseTable
    {
        public VersionControl() { }

        public int ControlValue { get; set; }

        public string AdministratorMessage { get; set; }
    }
}
