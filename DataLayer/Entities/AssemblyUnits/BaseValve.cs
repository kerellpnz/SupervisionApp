using DataLayer.Entities.Detailing;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DataLayer.Entities.AssemblyUnits
{
    public class BaseValve : BaseAssemblyUnit
    {
        public BaseValve()
        {
        }
        public BaseValve(BaseValve valve) : base(valve)
        {

        }        

        public int? GateId { get; set; }
        public Gate Gate { get; set; }

        public int? ShutterId { get; set; }
        

        public ObservableCollection<Saddle> Saddles { get; set; }
        
        public ObservableCollection<CounterFlange> CounterFlanges { get; set; }
        public ObservableCollection<Nozzle> Nozzles { get; set; }

        public ObservableCollection<BaseValveWithSealing> BaseValveWithSeals { get; set; }
        public ObservableCollection<BaseValveWithScrewNut> BaseValveWithScrewNuts { get; set; }
        public ObservableCollection<BaseValveWithScrewStud> BaseValveWithScrewStuds { get; set; }
        public ObservableCollection<BaseValveWithShearPin> BaseValveWithShearPins { get; set; }
        public ObservableCollection<BaseValveWithSpring> BaseValveWithSprings { get; set; }
        public ObservableCollection<BaseValveWithCoating> BaseValveWithCoatings { get; set; }
    }
}
