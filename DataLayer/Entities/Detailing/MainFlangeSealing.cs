using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataLayer.Entities.AssemblyUnits;
using DataLayer.Journals.Detailing;

namespace DataLayer.Entities.Detailing
{
    public class MainFlangeSealing : BaseSealing
    {
        public MainFlangeSealing()
        {
        }
        public MainFlangeSealing(MainFlangeSealing sealing) : base(sealing)
        {

        }

        public bool isWatchingLab { get; set; }

        public ObservableCollection<BaseValveWithSealing> BaseValveWithSeals { get; set; }

        public ObservableCollection<MainFlangeSealingJournal> MainFlangeSealingJournals { get; set; }
    }
}
