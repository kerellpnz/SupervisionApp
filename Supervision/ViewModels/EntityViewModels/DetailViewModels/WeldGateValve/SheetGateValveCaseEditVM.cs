using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusinessLayer.Repository.Implementations.Entities;
using BusinessLayer.Repository.Implementations.Entities.Detailing;
using DataLayer;
using DataLayer.Entities.Detailing.SheetGateValveDetails;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using Microsoft.EntityFrameworkCore;
using Supervision.Views.EntityViews.DetailViews.WeldGateValve;
using DataLayer.Entities.Detailing;
using Supervision.Views.EntityViews.DetailViews;
using Supervision.Views.EntityViews;
using BusinessLayer.Repository.Implementations.Entities.Material;
using DataLayer.Entities.Materials;
using Supervision.Views.EntityViews.MaterialViews;
using Supervision.ViewModels.EntityViewModels.Materials;
using System.Text.RegularExpressions;
using Supervision.Commands;
using DataLayer.Entities.Periodical;
using System;
using DataLayer.Journals.Periodical;

namespace Supervision.ViewModels.EntityViewModels.DetailViewModels.WeldGateValve
{
    public class SheetGateValveCaseEditVM : ViewModelBase
    {
        private readonly DataContext db;
        private IEnumerable<string> journalNumbers;
        private IEnumerable<CoverFlange> coverFlanges;
        private IEnumerable<CaseBottom> caseBottoms;
        private IEnumerable<CoverSleeve008> coverSleeves008;
        private IEnumerable<ForgingMaterial> forgingMaterials;
        //private IEnumerable<Ring043> rings043;
        //private IEnumerable<Ring047> rings047;
        private IEnumerable<string> drawings;
        private IEnumerable<string> materials;
        private IEnumerable<SheetGateValveCaseTCP> points;
        private IEnumerable<Inspector> inspectors;
        private IList<SheetGateValveCaseJournal> assemblyJournal;
        private IList<SheetGateValveCaseJournal> mechanicalJournal;
        private IList<SheetGateValveCaseJournal> nDTJournal;
        private IList<SheetGateValveCaseJournal> inputControlJournal;
        private readonly BaseTable parentEntity;
        private readonly SheetGateValveCaseRepository repo;
        private readonly InspectorRepository inspectorRepo;
        private readonly JournalNumberRepository journalRepo;
        private readonly CoverFlangeRepository coverFlangeRepo;
        private readonly CaseBottomRepository caseBottomRepo;
        private readonly CoverSleeve008Repository coverSleeve008Repo;
        private readonly ForgingMaterialRepository forgingRepo;
        private readonly Ring043Repository ring043Repo;
        //private readonly Ring047Repository ring047Repo;
        private readonly WeldingPeriodicalRepository repoWeld;
        private IEnumerable<SheetGateValveCase> cases;
        private SheetGateValveCase selectedItem;
        private SheetGateValveCaseTCP selectedTCPPoint;
        private SheetGateValveCaseJournal operation;
        private IEnumerable<PID> pIDs;
        private IList<WeldingProcedures> Welding;
        private readonly PIDRepository pIDRepo;

        private CoverFlange AddedCoverFlange;
        private CaseBottom AddedCaseBotttom;
        private ForgingMaterial AddedForgingMaterial;
        private CoverSleeve008 AddedCoverSleeve008;
        //private Ring043 AddedRing043;
        //private Ring047 AddedRing047;

        private IEnumerable<Ring043> rings;
        private Ring043 selectedRing;
        private Ring043 selectedRingFromList;

        public Ring043 SelectedRing
        {
            get => selectedRing;
            set
            {
                selectedRing = value;
                RaisePropertyChanged();
            }
        }

        public Ring043 SelectedRingFromList
        {
            get => selectedRingFromList;
            set
            {
                selectedRingFromList = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<Ring043> Rings
        {
            get => rings;
            set
            {
                rings = value;
                RaisePropertyChanged();
            }
        }

        public Supervision.Commands.IAsyncCommand AddRingToCaseCommand { get; private set; }
        private async Task AddRingToCase()
        {
            try
            {
                IsBusy = true;
                if (SelectedItem.Rings?.Count() < 2 || SelectedItem.Rings == null)
                {
                    if (SelectedRing != null)
                    {
                        if (!await Task.Run(() => ring043Repo.IsAssembliedAsync(SelectedRing, SelectedItem)))
                        {
                            Ring043 AddedRing = SelectedRing;
                            AddedRing.BaseWeldValveId = SelectedItem.Id;
                            SelectedItem.Rings.Add(AddedRing);
                            int value = await Task.Run(() => ring043Repo.Update(AddedRing));
                            if (value == 0)
                            {
                                SelectedItem.Rings.Remove(AddedRing);
                                MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз, либо перезайдите в текущий корпус", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                SelectedRing = null;
                                Rings = ring043Repo.UpdateList();
                            }
                        }
                    }
                    else MessageBox.Show("Объект не выбран!", "Ошибка");
                }
                else MessageBox.Show("Невозможно привязать более 2 колец!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Supervision.Commands.IAsyncCommand DeleteRingFromCaseCommand { get; private set; }
        private async Task DeleteRingFromCase()
        {
            try
            {
                IsBusy = true;
                if (SelectedRingFromList != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        Ring043 DeletedRing = SelectedRingFromList;
                        DeletedRing.BaseWeldValveId = null;
                        SelectedItem.Rings.Remove(DeletedRing);
                        int value = await Task.Run(() => ring043Repo.Update(DeletedRing));
                        if (value == 0)
                        {
                            SelectedItem.Rings.Add(DeletedRing);
                            MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз, либо перезайдите в текущий корпус", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            Rings = ring043Repo.UpdateList();
                        }
                    }
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand EditRingCommand { get; private set; }
        private void EditRing()
        {
            if (SelectedRingFromList != null)
            {
                _ = new Ring043EditView
                {
                    DataContext = Ring043EditVM.LoadVM(SelectedRingFromList.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }

        public IEnumerable<SheetGateValveCase> Cases
        {
            get => cases;
            set
            {
                cases = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<PID> PIDs
        {
            get => pIDs;
            set
            {
                pIDs = value;
                RaisePropertyChanged();
            }
        }

        public ICommand EditPIDCommand { get; private set; }
        private void EditPID()
        {
            if (SelectedItem.PID != null)
            {
                _ = new PIDEditView
                {
                    DataContext = PIDEditVM.LoadPIDEditVM(SelectedItem.PID.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("PID не выбран", "Ошибка");
        }
        public SheetGateValveCaseJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }

        public SheetGateValveCase SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }

        public IList<SheetGateValveCaseJournal> AssemblyJournal
        {
            get => assemblyJournal;
            set
            {
                assemblyJournal = value;
                RaisePropertyChanged();
            }
        }
        public IList<SheetGateValveCaseJournal> MechanicalJournal
        {
            get => mechanicalJournal;
            set
            {
                mechanicalJournal = value;
                RaisePropertyChanged();
            }
        }
        public IList<SheetGateValveCaseJournal> NDTJournal
        {
            get => nDTJournal;
            set
            {
                nDTJournal = value;
                RaisePropertyChanged();
            }
        }
        public IList<SheetGateValveCaseJournal> InputControlJournal
        {
            get => inputControlJournal;
            set
            {
                inputControlJournal = value;
                RaisePropertyChanged();
            }
        }
        
        public IEnumerable<SheetGateValveCaseTCP> Points
        {
            get => points;
            set
            {
                points = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<Inspector> Inspectors
        {
            get => inspectors;
            set
            {
                inspectors = value;
                RaisePropertyChanged();
            }
        }

        

        /*public FrontWall SelectedFrontWall
        {
            get => selectedFrontWall;
            set
            {
                selectedFrontWall	= value;
                RaisePropertyChanged();
            }
        }

        public FrontWall SelectedFrontWallFromList
        {
            get => selectedFrontWallFromList;
            set
            {
                selectedFrontWallFromList = value;
                RaisePropertyChanged();
            }
        }*/

        
        public IEnumerable<CoverFlange> CoverFlanges
        {
            get => coverFlanges;
            set
            {
                coverFlanges = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<CoverSleeve008> CoverSleeves008
        {
            get => coverSleeves008;
            set
            {
                coverSleeves008 = value;
                RaisePropertyChanged();
            }
        }

        //public IEnumerable<Ring043> Rings043
        //{
        //    get => rings043;
        //    set
        //    {
        //        rings043 = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public IEnumerable<Ring047> Rings047
        //{
        //    get => rings047;
        //    set
        //    {
        //        rings047 = value;
        //        RaisePropertyChanged();
        //    }
        //}
        public IEnumerable<CaseBottom> CaseBottoms
        {
            get => caseBottoms;
            set
            {
                caseBottoms = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<ForgingMaterial> ForgingMaterials
        {
            get => forgingMaterials;
            set
            {
                forgingMaterials = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<string> Materials
        {
            get => materials;
            set
            {
                materials = value;
                RaisePropertyChanged();
            }
        }


        public IEnumerable<string> Drawings
        {
            get => drawings;
            set
            {
                drawings = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<string> JournalNumbers
        {
            get => journalNumbers;
            set
            {
                journalNumbers = value;
                RaisePropertyChanged();
            }
        }

        public SheetGateValveCaseTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }



        public Supervision.Commands.IAsyncCommand<object> SaveItemCommand { get; private set; }
        private async Task SaveItem(object obj)
        {
            try
            {

                IsBusy = true;
                int value = await Task.Run(() => repo.Update(SelectedItem));
                if (value != 0)
                {
                    Window w = obj as Window;
                    w?.Close();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public new IAsyncCommand<object> CloseWindowCommand { get; private set; }
        protected new async Task CloseWindow(object obj)
        {
            if (IsBusy)
            {
                MessageBoxResult result = MessageBox.Show("Процесс сохранения уже запущен, теперь все в \"руках\" сервера. Попробовать отправить запрос на сохранение повторно? (Возможен вылет программы и не сохранение результата)", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    await SaveItemCommand.ExecuteAsync(obj);
                }
            }
            else
            {
                bool flag = true;
                bool check = true;
                //bool castingCaseCheck = false;
                //bool weldingRing043Check = false;
                bool assemblyFlag1 = false;
                bool assemblyFlag2 = false;
                bool assemblyFlag3 = false;
                bool assemblyFlag4 = false;

                if (SelectedItem.ForgingMaterialId != null)
                {
                    if (AddedForgingMaterial == null || !SelectedItem.ForgingMaterial.Equals(AddedForgingMaterial))
                    {
                        if (await Task.Run(() => forgingRepo.IsAssembliedAsync(SelectedItem)))
                        {
                            SelectedItem.ForgingMaterial = null;
                            check = false;
                        }
                        else AddDataFromForgingMaterial();
                    }
                    else AddDataFromForgingMaterial();
                }

                if (SelectedItem.Number == null || SelectedItem.Number == "")
                {
                    MessageBox.Show("Не оставляйте номер пустым, либо напишите \"удалить\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    check = false;
                }
                else if (!SelectedItem.Number.ToLower().Contains("удалить"))
                {
                    /*if (SelectedItem.Number != null && SelectedItem.DN != null)
                    {
                        int count = 0;
                        foreach (SheetGateValveCase entity in Cases)
                        {
                            if (SelectedItem.Number.Equals(entity.Number) && SelectedItem.DN.Equals(entity.DN))
                            {
                                count++;
                                if (count > 1)
                                {
                                    MessageBox.Show("Корпус с таким номером уже существует! Действия сохранены не будут! В поле \"Номер\" напишите \"удалить\". Впредь будьте внимательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    count = 0;
                                    check = false;
                                }
                            }
                        }
                    }*/
                    if (SelectedItem.DN == null || SelectedItem.DN == "")
                    {
                        MessageBox.Show("Не выбран диаметр!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Material == null || SelectedItem.Material == "")
                    {
                        MessageBox.Show("Не введен материал!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Melt == null || SelectedItem.Melt == "")
                    {
                        MessageBox.Show("Не введена плавка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                }                

                if (SelectedItem.SheetGateValveCaseJournals != null)
                {
                    foreach (SheetGateValveCaseJournal journal in SelectedItem.SheetGateValveCaseJournals)
                    {
                        if (journal.InspectorId != null)
                        {
                            if (journal.Date == null)
                            {
                                check = false;
                                journal.Status = null;
                                MessageBox.Show("Не выбрана дата!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                break;
                            }
                            if (journal.JournalNumber == null)
                            {
                                check = false;
                                journal.Status = null;
                                MessageBox.Show("Не выбран номер журнала!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                break;
                            }
                        }
                        if (journal.Date != null)
                        {
                            if (journal.RemarkIssued != null && journal.RemarkIssued != "")
                            {
                                if (!Regex.IsMatch(journal.RemarkIssued, @"^\d+$"))
                                {
                                    check = false;
                                    MessageBox.Show("Введите только номер замечания! Без пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                                }
                            }
                            if (journal.RemarkClosed != null && journal.RemarkClosed != "")
                            {
                                if (!Regex.IsMatch(journal.RemarkClosed, @"^\d+$"))
                                {
                                    check = false;
                                    MessageBox.Show("Введите только номер замечания! Без пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                                }
                            }
                            if (journal.RemarkIssued != null && journal.RemarkIssued != "" && (journal.RemarkClosed == null || journal.RemarkClosed == ""))
                            {
                                journal.Status = "Не соответствует";
                                journal.DateOfRemark = journal.Date;
                                SelectedItem.Status = "НЕ СООТВ.";
                                flag = false;
                                if (journal.Inspector != null)
                                {
                                    journal.RemarkInspector = journal.Inspector.Name;
                                }
                            }
                            else
                            {
                                journal.Status = "Cоответствует";
                            }
                            if (journal.PointId == 53 && journal.InspectorId != null)
                            {
                                if (SelectedItem.CaseBottomId == null)
                                {
                                    check = false;
                                    MessageBox.Show("Не выбрано днище!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            if (journal.PointId == 52 && journal.InspectorId != null)
                            {
                                if (SelectedItem.CoverFlangeId == null)
                                {
                                    check = false;
                                    MessageBox.Show("Не выбран фланец!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            if (journal.PointId == 171 && journal.InspectorId != null)
                            {
                                if (SelectedItem.CoverSleeve008Id == null)
                                {
                                    check = false;
                                    MessageBox.Show("Не выбран штуцер!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            if (journal.PointId == 150 && journal.InspectorId != null)
                            {
                                if (SelectedItem.Rings == null || SelectedItem.Rings.Count == 0)
                                {
                                    check = false;
                                    MessageBox.Show("Кольца не выбраны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            if ((journal.PointId == 150 || journal.PointId == 151) && journal.InspectorId != null)
                            {
                                if (SelectedItem.Rings == null)
                                {
                                    check = false;
                                    MessageBox.Show("Кольцо не выбрано!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            //if (journal.PointId == 150 && journal.InspectorId != null)
                            //{
                            //    weldingRing043Check = true;
                            //    if (SelectedItem.Ring043Id == null)
                            //    {
                            //        check = false;
                            //        MessageBox.Show("Не выбрано кольцо-фланец", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            //    }
                            //}
                            //if (journal.PointId == 151 && journal.InspectorId != null)
                            //{
                            //    if (SelectedItem.Ring047Id == null)
                            //    {
                            //        check = false;
                            //        MessageBox.Show("Не выбрано кольцо-днище!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            //    }
                            //}
                            if (journal.PointId == 54 && journal.InspectorId != null && Welding[0].Id == 1 && Welding[0].LastControl != null)
                            {
                                if (journal.Date > Welding[0].LastControl)
                                {
                                    Welding[0] = await Task.Run(() => repoWeld.GetByIdIncludeAsync(1));
                                    Welding[0].WeldingProceduresJournals.Add(new WeldingProceduresJournal()
                                    {
                                        DetailId = 1,
                                        PointId = 120,
                                        Point = journal.Point,
                                        Description = journal.Description,
                                        JournalNumber = journal.JournalNumber,
                                        Date = journal.Date,
                                        Status = journal.Status,
                                        RemarkClosed = journal.RemarkClosed,
                                        RemarkIssued = journal.RemarkIssued,
                                        Comment = journal.Comment,
                                        InspectorId = journal.InspectorId,
                                        PlaceOfControl = journal.PlaceOfControl,
                                        Documents = journal.Documents,
                                        DateOfRemark = journal.DateOfRemark,
                                        RemarkInspector = journal.RemarkInspector
                                    });
                                    Welding[0].LastControl = journal.Date;
                                    Welding[0].NextControl = Convert.ToDateTime(journal.Date).AddDays(7);
                                    int value = await Task.Run(() => repoWeld.Update(Welding[0]));
                                    if (value == 0)
                                    {
                                        MessageBox.Show("Режимы АФ не были сохранены в периодический контроль из-за ошибки сервера!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            if (journal.PointId == 55 && journal.InspectorId != null && Welding[0].Id == 1 && Welding[0].LastControl != null)
                            {
                                if (journal.Date > Welding[0].LastControl)
                                {
                                    Welding[0] = await Task.Run(() => repoWeld.GetByIdIncludeAsync(1));
                                    Welding[0].WeldingProceduresJournals.Add(new WeldingProceduresJournal()
                                    {
                                        DetailId = 1,
                                        PointId = 120,
                                        Point = journal.Point,
                                        Description = journal.Description,
                                        JournalNumber = journal.JournalNumber,
                                        Date = journal.Date,
                                        Status = journal.Status,
                                        RemarkClosed = journal.RemarkClosed,
                                        RemarkIssued = journal.RemarkIssued,
                                        Comment = journal.Comment,
                                        InspectorId = journal.InspectorId,
                                        PlaceOfControl = journal.PlaceOfControl,
                                        Documents = journal.Documents,
                                        DateOfRemark = journal.DateOfRemark,
                                        RemarkInspector = journal.RemarkInspector
                                    });
                                    Welding[0].LastControl = journal.Date;
                                    Welding[0].NextControl = Convert.ToDateTime(journal.Date).AddDays(7);
                                    int value = await Task.Run(() => repoWeld.Update(Welding[0]));
                                    if (value == 0)
                                    {
                                        MessageBox.Show("Режимы АФ не были сохранены в периодический контроль из-за ошибки сервера!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            if (journal.PointId == 62 && journal.InspectorId != null)
                            {
                                if (SelectedItem.Drawing == null || SelectedItem.Drawing == "")
                                {
                                    check = false;
                                    MessageBox.Show("Не введен чертеж!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                if (SelectedItem.PN == null || SelectedItem.PN == "")
                                {
                                    MessageBox.Show("Не выбрано давление!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    check = false;
                                }
                                if (Welding[0].Id == 1 && Welding[0].NextControl != null)
                                {
                                    if (Welding[0].NextControl < DateTime.Now && journal.Date > Convert.ToDateTime(Welding[0].NextControl).AddDays(2))
                                    {
                                        journal.Status = "Не соответствует";
                                        SelectedItem.Status = "НЕ СООТВ.";
                                        flag = false;
                                        MessageBox.Show("Просрочен контроль режимов сварки АФ. Статус контроля документов установлен на \"Не соответствует\"." +
                                            "Обратитесь в службу ОТК завода для выяснения обстоятельств.", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);                                  
                                    }
                                }
                            }
                            //if (journal.PointId == 33 && journal.InspectorId != null)
                            //{
                            //    castingCaseCheck = true;
                            //}
                            if (journal.PointId == 161 && journal.InspectorId != null)
                            {
                                SelectedItem.DateOfWashing = journal.Date;
                            }
                            if (journal.PointId == 59 && journal.InspectorId != null && journal.Status == "Cоответствует")
                            {
                                assemblyFlag1 = true;
                            }
                            if (journal.PointId == 62 && journal.InspectorId != null && journal.Status == "Cоответствует")
                            {
                                assemblyFlag2 = true;
                            }
                            if (journal.PointId == 61 && journal.InspectorId != null && journal.Status == "Cоответствует")
                            {
                                assemblyFlag3 = true;
                            }
                            if (journal.PointId == 161 && journal.InspectorId != null && journal.Status == "Cоответствует")
                            {
                                assemblyFlag4 = true;
                            }
                        }
                    }
                }
                
                //if (!castingCaseCheck && weldingRing043Check && SelectedItem.ForgingMaterialId == null)
                //{
                //    check = false;
                //    MessageBox.Show("Не выбрана поковка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                //}

                void AddDataFromForgingMaterial()
                {
                    SelectedItem.Material = SelectedItem.ForgingMaterial.Material;
                    SelectedItem.Melt = SelectedItem.ForgingMaterial.Melt;
                    SelectedItem.Certificate = SelectedItem.ForgingMaterial.Certificate;
                    if (SelectedItem.ForgingMaterial.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранная поковка имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущему корпусу.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                void CheckCoverFlangeStatus()
                {
                    if (SelectedItem.CoverFlange.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранный фланец имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущему корпусу.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                void CheckCaseBottomStatus()
                {
                    if (SelectedItem.CaseBottom.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранное днище имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущему корпусу.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                void CheckCoverSleeve008Status()
                {
                    if (SelectedItem.CoverSleeve008.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранная втулка(008) имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей крышке", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                //void CheckRing043Status()
                //{
                //    if (SelectedItem.Ring043.Status == "НЕ СООТВ.")
                //    {
                //        SelectedItem.Status = "НЕ СООТВ.";
                //        flag = false;
                //        MessageBox.Show("Выбранное кольцо-фланец имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущему корпусу.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    }
                //}
                //void CheckRing047Status()
                //{
                //    if (SelectedItem.Ring047.Status == "НЕ СООТВ.")
                //    {
                //        SelectedItem.Status = "НЕ СООТВ.";
                //        flag = false;
                //        MessageBox.Show("Выбранное кольцо-днище имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущему корпусу.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    }
                //}


                if (SelectedItem.CoverFlangeId != null)
                {
                    foreach (SheetGateValveCaseJournal journal in AssemblyJournal)
                    {
                        if (journal.PointId == 52 && journal.InspectorId == null)
                        {
                            check = false;
                            MessageBox.Show("Не выбрана операция СпС (шов №1)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    if (AddedCoverFlange == null || !SelectedItem.CoverFlange.Equals(AddedCoverFlange))
                    {
                        if (await Task.Run(() => coverFlangeRepo.IsAssembliedAsync(SelectedItem)))
                        {
                            SelectedItem.CoverFlange = null;
                            check = false;
                        }
                        else CheckCoverFlangeStatus();
                    } 
                    else CheckCoverFlangeStatus();
                }
                if (SelectedItem.CaseBottomId != null)
                {
                    foreach (SheetGateValveCaseJournal journal in AssemblyJournal)
                    {
                        if (journal.PointId == 53 && journal.InspectorId == null)
                        {
                            check = false;
                            MessageBox.Show("Не выбрана операция СпС (шов №2)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    if (AddedCaseBotttom == null || !SelectedItem.CaseBottom.Equals(AddedCaseBotttom))
                    {
                        if (await Task.Run(() => caseBottomRepo.IsAssembliedAsync(SelectedItem)))
                        {
                            SelectedItem.CaseBottom = null;
                            check = false;
                        }
                        else CheckCaseBottomStatus();
                    }
                    else CheckCaseBottomStatus();
                }
                if (SelectedItem.CoverSleeve008Id != null)
                {
                    foreach (SheetGateValveCaseJournal journal in AssemblyJournal)
                    {
                        if (journal.PointId == 171 && journal.InspectorId == null)
                        {
                            check = false;
                            MessageBox.Show("Не выбрана операция СпС (шов №19)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    if (AddedCoverSleeve008 == null || !SelectedItem.CoverSleeve008.Equals(AddedCoverSleeve008))
                    {
                        if (await Task.Run(() => coverSleeve008Repo.IsAssembliedAsync(SelectedItem)))
                        {
                            SelectedItem.CoverSleeve008 = null;
                            check = false;
                        }
                        else CheckCoverSleeve008Status();
                    }
                    else CheckCoverSleeve008Status();
                }
                if (SelectedItem.Rings != null)
                {
                    if (SelectedItem.Rings.Count != 0)
                    {
                        foreach (SheetGateValveCaseJournal journal in AssemblyJournal)
                        {
                            if (journal.PointId == 150 && journal.InspectorId == null)
                            {
                                check = false;
                                MessageBox.Show("Не выбрана операция СпС - Кольца (шов №1/№2)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    foreach (Ring043 ring in SelectedItem.Rings)
                    {
                        if (ring.Status == "НЕ СООТВ.")
                        {
                            SelectedItem.Status = "НЕ СООТВ.";
                            flag = false;
                            MessageBox.Show("Выбранное кольцо имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей ЗШ.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        }
                    }
                }
                //if (SelectedItem.Ring047Id != null)
                //{
                //    if (AddedRing047 == null || !SelectedItem.Ring047.Equals(AddedRing047))
                //    {
                //        if (await Task.Run(() => ring047Repo.IsAssembliedAsync(SelectedItem)))
                //        {
                //            SelectedItem.Ring047 = null;
                //            check = false;
                //        }
                //        else CheckRing047Status();
                //    }
                //    else CheckRing047Status();
                //}

                if (flag)
                {
                    SelectedItem.Status = "Cоотв.";
                    if (assemblyFlag1 && assemblyFlag2 && assemblyFlag3 && assemblyFlag4) SelectedItem.Status = "Готово к сборке";
                }

                if (check)
                {
                    try
                    {
                        IsBusy = true;
                        int value = await Task.Run(() => repo.Update(SelectedItem));
                        if (value != 0)
                        {
                            Window w = obj as Window;
                            w?.Close();
                        }
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }
            }  
            //if (repo.HasChanges(SelectedItem) || repo.HasChanges(SelectedItem.SheetGateValveCaseJournals))
            //{
            //    MessageBoxResult result = MessageBox.Show("Закрыть без сохранения изменений?", "Выход", MessageBoxButton.YesNo);

            //    if (result == MessageBoxResult.Yes)
            //    {
            //        base.CloseWindow(obj);
            //    }
            //}
            //else
            //{
            //    base.CloseWindow(obj);
            //}
        }

        private bool CanExecute()
        {
            return true;
        }

        public Commands.IAsyncCommand<int> LoadItemCommand { get; private set; }
        public async Task Load(int id)
        {
            try
            {
                IsBusy = true;
                SelectedItem = await Task.Run(() => repo.GetByIdIncludeAsync(id));
                Cases = await Task.Run(() => repo.GetAllAsyncForCompare());
                Welding = await Task.Run(() => repoWeld.GetAllAsync());

                await Task.Run(() => forgingRepo.Load());
                ForgingMaterials = forgingRepo.GetByDetail("Крестовина");

                await Task.Run(() => coverFlangeRepo.Load());
                await Task.Run(() => ring043Repo.Load());
                //await Task.Run(() => ring047Repo.Load());
                await Task.Run(() => caseBottomRepo.Load());
                await Task.Run(() => coverSleeve008Repo.Load());

                CoverFlanges = coverFlangeRepo.SortList();
                Rings = ring043Repo.UpdateList();
                //Rings047 = ring047Repo.SortList();
                CaseBottoms = caseBottomRepo.SortList();
                CoverSleeves008 = coverSleeve008Repo.SortList();


                PIDs = await Task.Run(() => pIDRepo.GetAllAsync());
                //CoverFlanges = await Task.Run(() => coverFlangeRepo.GetAllAsync());
                //Rings043 = await Task.Run(() => ring043Repo.GetAllAsync());
                //Rings047 = await Task.Run(() => ring047Repo.GetAllAsync());
                //CaseBottoms = await Task.Run(() => caseBottomRepo.GetAllAsync());
                
                
                
                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
                Drawings = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Drawing));
                Materials = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Material));
                Points = await Task.Run(() => repo.GetTCPsAsync());
                JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());
                AssemblyJournal = SelectedItem.SheetGateValveCaseJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка/Сварка").OrderBy(x => x.PointId).ToList();
                MechanicalJournal = SelectedItem.SheetGateValveCaseJournals.Where(i => i.EntityTCP.OperationType.Name == "Механическая обработка").OrderBy(x => x.PointId).ToList();
                NDTJournal = SelectedItem.SheetGateValveCaseJournals.Where(i => i.EntityTCP.OperationType.Name == "Документация").OrderBy(x => x.PointId).ToList();
                InputControlJournal = SelectedItem.SheetGateValveCaseJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();

                if (SelectedItem.CoverFlangeId != null)
                {
                    AddedCoverFlange = SelectedItem.CoverFlange;
                }
                if (SelectedItem.CaseBottomId != null)
                {
                    AddedCaseBotttom = SelectedItem.CaseBottom;
                }
                if (SelectedItem.ForgingMaterialId != null)
                {
                    AddedForgingMaterial = SelectedItem.ForgingMaterial;
                }
                if (SelectedItem.CoverSleeve008Id != null)
                {
                    AddedCoverSleeve008 = SelectedItem.CoverSleeve008;
                }
                //if (SelectedItem.Ring043Id != null)
                //{
                //    AddedRing043 = SelectedItem.Ring043;
                //}
                //if (SelectedItem.Ring047Id != null)
                //{
                //    AddedRing047 = SelectedItem.Ring047;
                //}
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand EditCaseFlangeCommand { get; private set; }
        private void EditCaseFlange()
        {
            if (SelectedItem.CoverFlange != null)
            {
                _ = new CoverFlangeEditView
                {
                    DataContext = CoverFlangeEditVM.LoadVM(SelectedItem.CoverFlange.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите деталь", "Ошибка");
        }

        //public ICommand EditRing043Command { get; private set; }
        //private void EditRing043()
        //{
        //    if (SelectedItem.Ring043 != null)
        //    {
        //        _ = new Ring043EditView
        //        {
        //            DataContext = Ring043EditVM.LoadVM(SelectedItem.Ring043.Id, SelectedItem, db)
        //        };
        //    }
        //    else MessageBox.Show("Для просмотра привяжите деталь", "Ошибка");
        //}

        //public ICommand EditRing047Command { get; private set; }
        //private void EditRing047()
        //{
        //    if (SelectedItem.Ring047 != null)
        //    {
        //        _ = new Ring047EditView
        //        {
        //            DataContext = Ring047EditVM.LoadVM(SelectedItem.Ring047.Id, SelectedItem, db)
        //        };
        //    }
        //    else MessageBox.Show("Для просмотра привяжите деталь", "Ошибка");
        //}

        public ICommand EditCaseBottomCommand { get; private set; }
        private void EditCaseBottom()
        {
            if (SelectedItem.CaseBottom != null)
            {
                _ = new CaseBottomEditView
                {
                    DataContext = CaseBottomEditVM.LoadVM(SelectedItem.CaseBottom.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите деталь", "Ошибка");
        }

        public ICommand EditCoverSleeve008Command { get; private set; }
        private void EditCoverSleeve008()
        {
            if (SelectedItem.CoverSleeve008 != null)
            {
                _ = new CoverSleeve008EditView
                {
                    DataContext = CoverSleeve008EditVM.LoadVM(SelectedItem.CoverSleeve008.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите втулку(008)", "Ошибка");
        }

        public ICommand EditForgingMaterialCommand { get; private set; }
        private void EditForgingMaterial()
        {
            if (SelectedItem.ForgingMaterial != null)
            {
                _ = new ForgingMaterialEditView
                {
                    DataContext = ForgingMaterialEditVM.LoadVM(SelectedItem.ForgingMaterial.Id, SelectedItem, db)
                };
            }
        }

        public ICommand AddOperationCommand { get; private set; }
        public void AddJournalOperation()
        {
            if (SelectedTCPPoint == null) MessageBox.Show("Выберите пункт ПТК!", "Ошибка");
            else
            {
                SelectedItem.SheetGateValveCaseJournals.Add(new SheetGateValveCaseJournal(SelectedItem, SelectedTCPPoint));                
                AssemblyJournal = SelectedItem.SheetGateValveCaseJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка/Сварка").OrderBy(x => x.PointId).ToList();
                MechanicalJournal = SelectedItem.SheetGateValveCaseJournals.Where(i => i.EntityTCP.OperationType.Name == "Механическая обработка").OrderBy(x => x.PointId).ToList();
                NDTJournal = SelectedItem.SheetGateValveCaseJournals.Where(i => i.EntityTCP.OperationType.Name == "Документация").OrderBy(x => x.PointId).ToList();
                InputControlJournal = SelectedItem.SheetGateValveCaseJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();
                SelectedTCPPoint = null;
            }
        }

        public ICommand RemoveOperationCommand { get; private set; }
        private void RemoveOperation()
        {
            try
            {
                IsBusy = true;
                if (Operation != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        SelectedItem.SheetGateValveCaseJournals.Remove(Operation);                        
                        AssemblyJournal = SelectedItem.SheetGateValveCaseJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка/Сварка").OrderBy(x => x.PointId).ToList();
                        MechanicalJournal = SelectedItem.SheetGateValveCaseJournals.Where(i => i.EntityTCP.OperationType.Name == "Механическая обработка").OrderBy(x => x.PointId).ToList();
                        NDTJournal = SelectedItem.SheetGateValveCaseJournals.Where(i => i.EntityTCP.OperationType.Name == "Документация").OrderBy(x => x.PointId).ToList();
                        InputControlJournal = SelectedItem.SheetGateValveCaseJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();
                        Operation = null;
                    }
                }
                else MessageBox.Show("Выберите операцию!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        
        public static SheetGateValveCaseEditVM LoadVM(int id, BaseTable entity, DataContext context)
        {
            SheetGateValveCaseEditVM vm = new SheetGateValveCaseEditVM(entity, context);
            vm.LoadItemCommand.ExecuteAsync(id);
            return vm;
        }

        public SheetGateValveCaseEditVM(BaseTable entity, DataContext context)
        {
            db = context;
            parentEntity = entity;
            repo = new SheetGateValveCaseRepository(db);
            forgingRepo = new ForgingMaterialRepository(db);
            inspectorRepo = new InspectorRepository(db);
            journalRepo = new JournalNumberRepository(db);
            coverFlangeRepo = new CoverFlangeRepository(db);
            ring043Repo = new Ring043Repository(db);
            //ring047Repo = new Ring047Repository(db);
            caseBottomRepo = new CaseBottomRepository(db);
            coverSleeve008Repo = new CoverSleeve008Repository(db);
            repoWeld = new WeldingPeriodicalRepository(db);


            pIDRepo = new PIDRepository(db);
            EditPIDCommand = new Supervision.Commands.Command(o => EditPID());
            LoadItemCommand = new Supervision.Commands.AsyncCommand<int>(Load);
            SaveItemCommand = new Supervision.Commands.AsyncCommand<object>(SaveItem);
            CloseWindowCommand = new Supervision.Commands.AsyncCommand<object>(CloseWindow);
            AddOperationCommand = new Supervision.Commands.Command(o => AddJournalOperation());
            RemoveOperationCommand = new Supervision.Commands.Command(o => RemoveOperation());
            EditCaseBottomCommand = new Supervision.Commands.Command(o => EditCaseBottom());
            EditCaseFlangeCommand = new Supervision.Commands.Command(o => EditCaseFlange());
            EditCoverSleeve008Command = new Supervision.Commands.Command(o => EditCoverSleeve008());
            //EditRing043Command = new Supervision.Commands.Command(o => EditRing043());
            //EditRing047Command = new Supervision.Commands.Command(o => EditRing047());
            AddRingToCaseCommand = new Supervision.Commands.AsyncCommand(AddRingToCase);
            DeleteRingFromCaseCommand = new Supervision.Commands.AsyncCommand(DeleteRingFromCase);
            EditRingCommand = new Supervision.Commands.Command(o => EditRing());
            EditForgingMaterialCommand = new Supervision.Commands.Command(o => EditForgingMaterial());
        }
    }
}
