using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.TechnicalControlPlans
{
    public class BaseTCP : BaseTable
    {
        public string Point { get; set; }
        public string OperationName { get; set; } //TODO: при ненадобности удалить
        public string Description { get; set; }

        public string PlaceOfControl { get; set; }
        public string Documents { get; set; }

        public int? OperationNameId { get; set; }
        [ForeignKey("OperationNameId")]
        public OperationType OperationType { get; set; }

        public int? ProductTypeId { get; set; }
        public ProductType ProductType { get; set; }

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