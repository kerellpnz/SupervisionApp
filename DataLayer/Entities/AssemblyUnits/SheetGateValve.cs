using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Journals.AssemblyUnits;

namespace DataLayer.Entities.AssemblyUnits
{
    public class SheetGateValve : BaseWeldValve
    {
        /*public string NumberTemp { get; set; }

        public new string Number => string.Format($"О{NumberTemp}");*/


        public string GatePlace { get; set; }
        public string Moment { get; set; }

        public string Time { get; set; }

        public string AutomaticReset { get; set; }

        public System.DateTime? EarTest { get; set; }

        public System.DateTime? ZIP { get; set; }



        public SheetGateValve()
        {
            Name = "ЗШ";            
        }
        public SheetGateValve(SheetGateValve valve) :base(valve)
        {
            Number = Microsoft.VisualBasic.Interaction.InputBox("Введите номер:");
        }

        [NotMapped]
        public new string FullName => string.Format($"№О{Number}");

        public ObservableCollection<SheetGateValveJournal> SheetGateValveJournals { get; set; }
    }
}
