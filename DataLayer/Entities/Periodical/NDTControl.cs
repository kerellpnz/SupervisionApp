﻿using DataLayer.Journals.Periodical;
using System.Collections.ObjectModel;

namespace DataLayer.Entities.Periodical
{
    //Контроль ТО
    public class NDTControl : PeriodicalControl
    {
        public ObservableCollection<NDTControlJournal> NDTControlJournals { get; set; }
    }
}
