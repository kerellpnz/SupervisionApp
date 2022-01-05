using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Detailing.WeldGateValveDetails;

namespace DataLayer.Entities.Detailing.SheetGateValveDetails
{
    public class SheetGateValveCover : WeldGateValveCover
    {
        public SheetGateValveCover()
        {
            Name = "Крышка ЗШ";
        }
        public SheetGateValveCover(SheetGateValveCover sheetCover) : base(sheetCover)
        {
            Number = Microsoft.VisualBasic.Interaction.InputBox("Введите номер:");
        }

        

        public ObservableCollection<SheetGateValveCoverJournal> SheetGateValveCoverJournals { get; set; }
    }
}
