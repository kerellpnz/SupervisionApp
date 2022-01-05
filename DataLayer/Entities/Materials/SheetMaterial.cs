using DataLayer.Journals.Materials;
using System.Collections.ObjectModel;

namespace DataLayer.Entities.Materials
{
    public class SheetMaterial : MetalMaterial
    {
        public SheetMaterial()
        {
            Name = "Лист";
        }

        public SheetMaterial(SheetMaterial material) : base(material)
        {
            Number = Microsoft.VisualBasic.Interaction.InputBox("Введите номер:");
        }

        public ObservableCollection<SheetMaterialJournal> SheetMaterialJournals { get; set; }
    }
}
