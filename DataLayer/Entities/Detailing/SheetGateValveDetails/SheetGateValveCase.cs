using System.Collections.ObjectModel;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Detailing.WeldGateValveDetails;

namespace DataLayer.Entities.Detailing.SheetGateValveDetails
{
    public class SheetGateValveCase : WeldGateValveCase
    {
        public SheetGateValveCase()
        {
            Name = "Корпус ЗШ";
            Material = "20ГМЛ";
        }
        public SheetGateValveCase(SheetGateValveCase sheetCase) : base(sheetCase)
        {                      
            Number = Microsoft.VisualBasic.Interaction.InputBox("Введите номер:");
        }

        public ObservableCollection<SheetGateValveCaseJournal> SheetGateValveCaseJournals { get; set; }

    }
}
