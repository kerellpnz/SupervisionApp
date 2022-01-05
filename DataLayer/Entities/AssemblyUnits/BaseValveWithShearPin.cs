using DataLayer.Entities.Detailing;

namespace DataLayer.Entities.AssemblyUnits
{
    public class BaseValveWithShearPin
    {
        public int Id { get; set; }

        public int BaseValveId { get; set; }
        public BaseValve BaseValve { get; set; }

        public int ShearPinId { get; set; }
        public ShearPin ShearPin { get; set; }

        public int ShearPinAmount { get; set; }
    }
}
