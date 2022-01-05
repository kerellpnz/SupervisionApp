using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace DataLayer
{
    public class BasePropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        
        
        
        [NotMapped]
        public IEnumerable<string> statuses = new List<string> { "Соотв.", "Не соотв." };
        
        [NotMapped]
        public IEnumerable<string> Statuses
        {
            get => statuses;
            set
            {
                statuses = value;
                RaisePropertyChanged();
            }
        }
       
        [NotMapped]
        public IEnumerable<string> dns = new List<string> {
                        "150",
                        "200",
                        "250",
                        "300",
                        "350",
                        "400",
                        "500",
                        "600",
                        "700",
                        "800",
                        "1000",
                        "1050",
                        "1200" };
        
        [NotMapped]
        public IEnumerable<string> DNs
        {
            get => dns;
            set
            {
                dns = value;
                RaisePropertyChanged();
            }
        }
        
        [NotMapped]
        public IEnumerable<string> pns = new List<string> {
                        "1,6",
                        "2,5",
                        "4,0",
                        "6,3",
                        "8,0",
                        "10,0",
                        "12,5"};
        [NotMapped]
        public IEnumerable<string> PNs
        {
            get => pns;
            set
            {
                pns = value;
                RaisePropertyChanged();
            }
        }

        [NotMapped]
        public IEnumerable<string> targets = new List<string> {
                        "Втулка",
                        "Обойма",
                        "Катушка",
                        "Крестовина",
                        "Кольцо",
                        "Крышка",
                        "Ответный фланец" };
        [NotMapped]
        public IEnumerable<string> Targets
        {
            get => targets;
            set
            {
                targets = value;
                RaisePropertyChanged();
            }
        }

        [NotMapped]
        public IEnumerable<string> statusesGateValve = new List<string> { "Соотв.", "Не соотв.", "Отгружен" };
        
        [NotMapped]
        public IEnumerable<string> StatusesGateValve
        {
            get => statusesGateValve;
            set
            {
                statusesGateValve = value;
                RaisePropertyChanged();
            }
        }

    }
}
