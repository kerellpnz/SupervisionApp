using DataLayer.Entities;
using DataLayer.TechnicalControlPlans;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Journals
{
    public class BaseJournal<TEntityTCP> : BaseTable
        where TEntityTCP : BaseTCP
    {
        public string Point { get; set; }
        public string Description { get; set; }
        public string JournalNumber { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }
        public string RemarkIssued { get; set; }
        public string RemarkClosed { get; set; }
        public string Comment { get; set; }
        public string PlaceOfControl { get; set; }
        public string Documents { get; set; }
        public DateTime? DateOfRemark { get; set; }
        public string RemarkInspector { get; set; }

        public int? InspectorId { get; set; }
        public Inspector Inspector { get; set; }

        [NotMapped]
        public string FullNameRemarkIssued => string.Format($"Замечание №{RemarkIssued}\nот {Convert.ToDateTime(DateOfRemark).ToString("dd.MM.yyyy")}\n{RemarkInspector}");

        [NotMapped]
        public string FullNameRemarkClosed => string.Format($"Замечание №{RemarkIssued}\nснято от\n{Convert.ToDateTime(Date).ToString("dd.MM.yyyy")}");

        public int? PointId { get; set; }
        [ForeignKey("PointId")]
        public TEntityTCP EntityTCP { get; set; }

        public BaseJournal()
        {
        }

        public BaseJournal(TEntityTCP tCP)
        {
            PointId = tCP.Id;
            Point = tCP.Point;
            Description = tCP.Description;
            PlaceOfControl = tCP.PlaceOfControl;
            Documents = tCP.Documents;
        }

        [NotMapped]
        public IEnumerable<string> documentsView = new List<string> {
                        "НТД",
                        "Сертификат",
                        "ПМИ",
                        "Заключения",
                        "КД",
                        "ТП",
                        "КД, ТП",
                        "ТП, Диаграммы",
                        "КД, Заключения",
                        "КД, Сертификат",
                        "Сертификат, Заключения",
                        "Паспорт",
                        "Паспорт, КД",
                        "Схема увязки, ТН",
                        "Спецификация"
        };

        [NotMapped]
        public IEnumerable<string> DocumentsView
        {
            get => documentsView;
            set
            {
                documentsView = value;
                RaisePropertyChanged();
            }
        }


        [NotMapped]
        public IEnumerable<string> placeOfControlView = new List<string> {
                        "Цех",
                        "Склад материалов",
                        "Малярный участок",
                        "Испытательный стенд",
                        "ЦЗЛ",
                        "Офис ТН",
                        "Участок отгрузки",
                        "Цех, Офис ТН"

        };

        [NotMapped]
        public IEnumerable<string> PlaceOfControlView
        {
            get => placeOfControlView;
            set
            {
                placeOfControlView = value;
                RaisePropertyChanged();
            }
        }
    }
}
