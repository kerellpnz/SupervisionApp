using DataLayer.Entities.AssemblyUnits;
using DataLayer.Files;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Materials.AnticorrosiveCoating
{
    public class BaseAnticorrosiveCoating : BaseTable
    {
        public string Name { get; set; }
        public string Factory { get; set; }
        public string Certificate { get; set; }
        public string Batch { get; set; }
        public string Amount { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
      
        public DateTime? ExpirationDate { get; set; }
        public DateTime? InputControlDate { get; set; }

        [NotMapped] 
        public string FullName => string.Format($"Партия: {Batch}| АКП: {Name} - {Status}");

        public ObservableCollection<BaseValveWithCoating> BaseValveWithCoatings { get; set; }
        
        public ObservableCollection<BaseAnticorrosiveCoatingWithFile> Files { get; set; }

        public BaseAnticorrosiveCoating()
        {
            Status = "Соотв.";
        }

        public BaseAnticorrosiveCoating(BaseAnticorrosiveCoating coating)
        {
            Name = coating.Name;
            Factory = coating.Factory;
            Status = coating.Status;
            Comment = coating.Comment;
        }
    }
}
