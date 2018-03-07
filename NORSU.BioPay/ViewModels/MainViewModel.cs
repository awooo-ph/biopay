using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DPFP;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Models;
using NORSU.BioPay.Views;
using Xceed.Words.NET;
using Settings = NORSU.BioPay.Properties.Settings;

namespace NORSU.BioPay.ViewModels
{
    class MainViewModel:ViewModelBase
    {
        public const long ClockIndex = 0, AdminIndex = 1, PunchIndex=2;
        private MainViewModel()
        {
            
            Messenger.Default.AddListener<Sample>(Messages.Scan, async sample =>
            {
                if (ScreenIndex == AdminIndex) return;

                var finger = sample.ToFingerPrint();
                if (finger != null)
                {
                    finger.Usage++;
                    finger.LastUsed = DateTime.Now;
                    finger.Save();
                    
                    if (finger.Login && finger.Employee.IsAdmin)
                    {
                        Punch = finger.Employee;
                        ScreenIndex = AdminIndex;
                        return;
                    }
                    
                    var prevDtr = DailyTimeRecord.GetTimeIn(finger.EmployeeId);
                    if (prevDtr != null)
                    {
                        if ((DateTime.Now - prevDtr.TimeIn).TotalSeconds < 60) return;
                        prevDtr.TimeOut = DateTime.Now;
                        prevDtr.Save();
                    }
                    else
                    {
                        var dtr = new DailyTimeRecord(finger.EmployeeId);
                        dtr.Save();
                    }

                    PunchAM = DailyTimeRecord.Cache
                        .OrderByDescending(x=>x.TimeIn)
                        .FirstOrDefault(x => x.EmployeeId == finger.EmployeeId
                                             && x.TimeIn.Date == DateTime.Now.Date
                                             && x.TimeIn.Hour < 12);

                    PunchPM = DailyTimeRecord.Cache
                        .OrderByDescending(x => x.TimeIn)
                        .FirstOrDefault(x => x.EmployeeId == finger.EmployeeId
                                             && x.TimeIn.Date == DateTime.Now.Date
                                             && x.TimeIn.Hour >= 12);

                    Punch = finger.Employee;
                    ShowPunch();
                }
            });

            if (Models.User.Cache.Count == 0 && Models.Employee.Cache.Count==0)
            {
                ScreenIndex = AdminIndex;
            }
        }

        private string _SearchKeyword;

        public string SearchKeyword
        {
            get => _SearchKeyword;
            set
            {
                if(value == _SearchKeyword)
                    return;
                _SearchKeyword = value;
                OnPropertyChanged(nameof(SearchKeyword));
                Employees.Refresh();
            }
        }

        private ICommand _deleteSelectedCommand;

        public ICommand DeleteSelectedCommand =>
            _deleteSelectedCommand ?? (_deleteSelectedCommand = new DelegateCommand(
                d =>
                {
                    var list = new List<Employee>();
                    
                    foreach (Employee employee in Employees)
                    {
                        if (employee.IsSelected)
                        {
                            list.Add(employee);
                        }
                    }
                    
                    foreach (var employee in list)
                    {
                        employee.Delete(false);
                    }
                },d=>HasSelected));
        
        private DateTime _lastPunch;
        private async void ShowPunch()
        {
            _lastPunch = DateTime.Now;
            if (ScreenIndex == PunchIndex) return;
            ScreenIndex = PunchIndex;

            while ((DateTime.Now - _lastPunch).TotalMilliseconds < 7777)
                await TaskEx.Delay(111);
            if(ScreenIndex!=AdminIndex)
                ScreenIndex = ClockIndex;
        }
        
        

        private static MainViewModel _instance;
        public static MainViewModel Instance => _instance ?? (_instance = new MainViewModel());

        private long _ScreenIndex = ClockIndex;

        public long ScreenIndex
        {
            get => _ScreenIndex;
            set
            {
                if(value == _ScreenIndex)
                    return;
                _ScreenIndex = value;
                OnPropertyChanged(nameof(ScreenIndex));
            }
        }

        private Employee _Punch;

        public Employee Punch
        {
            get => _Punch;
            set
            {
                _Punch = value;
                OnPropertyChanged(nameof(Punch));
            }
        }

        private DailyTimeRecord _PunchAM;

        public DailyTimeRecord PunchAM
        {
            get => _PunchAM;
            set
            {
                _PunchAM = value;
                OnPropertyChanged(nameof(PunchAM));
            }
        }

        private DailyTimeRecord _PunchPM;

        public DailyTimeRecord PunchPM
        {
            get => _PunchPM;
            set
            {
                _PunchPM = value;
                OnPropertyChanged(nameof(PunchPM));
            }
        }
        
        private int _SelectedAuthenticationIndex;

        public int SelectedAuthenticationIndex
        {
            get => _SelectedAuthenticationIndex;
            set
            {
                if(value == _SelectedAuthenticationIndex)
                    return;
                _SelectedAuthenticationIndex = value;
                OnPropertyChanged(nameof(SelectedAuthenticationIndex));
            }
        }

        private ListCollectionView _admins;

        public ListCollectionView Admins
        {
            get
            {
                if (_admins != null) return _admins;
                _admins = new ListCollectionView(Employee.Cache);
                _admins.Filter = FilterAdmin;
                return _admins;
            }
        }

        private bool FilterAdmin(object o)
        {
            if (!(o is Employee e)) return false;
            return e.IsAdmin;
        }

        private ICommand _showAdminCommand;

        public ICommand ShowAdminCommand => _showAdminCommand ?? (_showAdminCommand = new DelegateCommand(d =>
        {
            if (ScreenIndex != AdminIndex)
            {
                if(Employee.Cache.All(x=>!x.IsAdmin) || (Punch?.IsAdmin??false))
                    ScreenIndex = AdminIndex;
            }
            else
                ScreenIndex = ClockIndex;
        }));

        private ListCollectionView _employees;

        public ListCollectionView Employees
        {
            get
            {
                if (_employees != null) return _employees;
                _employees = new ListCollectionView(Employee.Cache);
                _employees.CurrentChanged += (sender, args) =>
                {
                    if (_employees.IsAddingNew) return;
                    Employee.SelectedItem = _employees.CurrentItem as Employee;
                };
                _employees.Filter = FilterEmployee;
                return _employees;
            }
        }

        private bool FilterEmployee(object o)
        {
            if (!(o is Employee emp)) return false;
            if (string.IsNullOrWhiteSpace(SearchKeyword)) return true;
            return emp.Fullname.ToLower().Contains(SearchKeyword.ToLower());
        }

        private ListCollectionView _dtr;

        public ListCollectionView DTR
        {
            get
            {
                if (_dtr != null) return _dtr;
                _dtr = new ListCollectionView(DailyTimeRecord.Cache);
                _dtr.GroupDescriptions.Add(new PropertyGroupDescription("Date"));
                Employees.CurrentChanged += (sender, args) =>
                {
                    if (Employees.IsAddingNew) return;
                    _dtr.Filter = FilterDTR;
                };
                _dtr.Filter = FilterDTR;
                return _dtr;
            }
        }

        private bool FilterDTR(object o)
        {
            if (!(o is DailyTimeRecord dtr)) return false;
            if (!(Employees.CurrentItem is Employee emp)) return false;
            return dtr.EmployeeId == emp.Id;
        }

        private int _SelectedInfoIndex = 0;

        public int SelectedInfoIndex
        {
            get => _SelectedInfoIndex;
            set
            {
                if(value == _SelectedInfoIndex)
                    return;
                _SelectedInfoIndex = value;
                OnPropertyChanged(nameof(SelectedInfoIndex));
            }
        }

        private ICommand _showSettingsCommand;

        public ICommand ToggleSettingsCommand => _showSettingsCommand ?? (_showSettingsCommand = new DelegateCommand(
            d =>
            {
                IsSettingsShown = !IsSettingsShown;
                Scanner.OnScan = null;
            }));

        private bool _IsSettingsShown;

        public bool IsSettingsShown
        {
            get => _IsSettingsShown;
            set
            {
                if(value == _IsSettingsShown)
                    return;
                _IsSettingsShown = value;
                OnPropertyChanged(nameof(IsSettingsShown));
            }
        }

        private ListCollectionView _jobs;

        public ListCollectionView Jobs
        {
            get
            {
                if (_jobs != null) return _jobs;
                _jobs = new ListCollectionView(Job.Cache);
                return _jobs;
            }
        }

        private ListCollectionView _currentEmployeeFingers;

        public ListCollectionView CurrentEmployeeFingers
        {
            get
            {
                if (_currentEmployeeFingers != null) return _currentEmployeeFingers;
                _currentEmployeeFingers = new ListCollectionView(FingerPrint.Cache);
                _currentEmployeeFingers.Filter = FilterFinger;
                Employees.CurrentChanged += (sender, args) =>
                {
                    if (Employees.IsAddingNew) return;
                    _currentEmployeeFingers.Filter = FilterFinger;
                };
                return _currentEmployeeFingers;
            }
        }

        private bool FilterFinger(object o)
        {
            if (!(Employees.CurrentItem is Employee e)) return false;
            if (!(o is FingerPrint f)) return false;
            return f.EmployeeId == e.Id;
        }

        private ICommand _addFingerCommand;

        public ICommand AddFingerCommand => _addFingerCommand ?? (_addFingerCommand = new DelegateCommand(async d =>
        {
            if (!(Employees.CurrentItem is Employee employee)) return;
            
            var dlg = new AddFingerViewModel(employee);
            Scanner.OnScan = dlg.Scan;

            var view = new PrintEnrollment() {DataContext = dlg};

            await DialogHost.Show(view, "Admin", (sender, args) => { }, (sender, args) =>
            {
                if (args.IsCancelled) return;
                if ((args.Parameter as bool?) ?? false)
                {
                    Scanner.OnScan = null;
                    new FingerPrint()
                    {
                        EmployeeId = employee.Id,
                        EditMode = true,
                        Description = "Tudlo",
                        Template = dlg.ResultTemplate.Bytes,
                        Picture = dlg.ResultThumbnail,
                    }.Save();
                    CurrentEmployeeFingers.Refresh();
                }
            });
        }));


        private bool _IsAddingAdmin;

        public bool IsAddingAdmin
        {
            get => _IsAddingAdmin;
            set
            {
                if(value == _IsAddingAdmin)
                    return;
                _IsAddingAdmin = value;
                OnPropertyChanged(nameof(IsAddingAdmin));
            }
        }

        private bool _IsWaitingForAdminScan;

        public bool IsWaitingForAdminScan
        {
            get => _IsWaitingForAdminScan;
            set
            {
                if(value == _IsWaitingForAdminScan)
                    return;
                _IsWaitingForAdminScan = value;
                OnPropertyChanged(nameof(IsWaitingForAdminScan));
            }
        }

        private bool _IsAddingAdminError;

        public bool IsAddingAdminError
        {
            get => _IsAddingAdminError;
            set
            {
                if(value == _IsAddingAdminError)
                    return;
                _IsAddingAdminError = value;
                OnPropertyChanged(nameof(IsAddingAdminError));
            }
        }

        private string _AddingAdminStatus;

        public string AddingAdminStatus
        {
            get => _AddingAdminStatus;
            set
            {
                if(value == _AddingAdminStatus)
                    return;
                _AddingAdminStatus = value;
                OnPropertyChanged(nameof(AddingAdminStatus));
            }
        }

        
        private ICommand _addAdminCommand;

        public ICommand AddAdminCommand => _addAdminCommand ?? (_addAdminCommand = new DelegateCommand(d =>
        {
            IsAddingAdmin = true;
            IsAddingAdminError = true;
            IsWaitingForAdminScan = true;
            AddingAdminStatus = "Waiting for Finger Scan...";

            Scanner.OnScan = async sample =>
            {
                Scanner.OnScan = null;
                IsWaitingForAdminScan = false;
                var finger = sample.ToFingerPrint();
                var emp = Employee.Cache.FirstOrDefault(x => x.Id == finger.EmployeeId);
                
                if (finger==null || emp==null)
                {
                    IsAddingAdminError = true;
                    AddingAdminStatus = "Invalid Finger";
                }
                else
                {
                    finger.Update(nameof(FingerPrint.Login),true);
                    emp.Update(nameof(Employee.IsAdmin),true);
                    awooo.Context.Post(dd=>Admins.Filter = FilterAdmin,null);
                    IsAddingAdminError = false;
                    AddingAdminStatus = "Success!";
                }

                await TaskEx.Delay(1111);
                IsAddingAdmin = false;
            };
        }));

        private ICommand _cancelAddAdminCommand;

        public ICommand CancelAddAdminCommand =>
            _cancelAddAdminCommand ?? (_cancelAddAdminCommand = new DelegateCommand(
                d =>
                {
                    Scanner.OnScan = null;
                    IsAddingAdmin = false;
                }));

        private ICommand _RemoveAdminCommand;

        public ICommand RemoveAdminCommand => _RemoveAdminCommand ?? (_RemoveAdminCommand = new DelegateCommand<Employee>(d =>
        {
            d.Update(nameof(Employee.IsAdmin),false);
            Admins.Refresh();
        }));

        private ICommand _changePictureCommand;

        public ICommand ChangePictureCommand => _changePictureCommand ?? (_changePictureCommand = new DelegateCommand(
            d =>
            {
                if (!(Employees.CurrentItem is Employee emp)) return;
                
                try
                {
                    var dialog = new OpenFileDialog
                    {
                        Multiselect = false,
                        Filter = @"All Images|*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG|
                        BMP Files|*.BMP;*.DIB;*.RLE|
                        JPEG Files|*.JPG;*.JPEG;*.JPE;*.JFIF|
                        GIF Files|*.GIF|
                        PNG Files|*.PNG",
                        Title = "Select Picture",
                    };
                    if (!(dialog.ShowDialog() ?? false))
                        return;
                        
                    emp.Update(nameof(Employee.Picture),File.ReadAllBytes(dialog.FileName));
                }
                catch (Exception e)
                {
                    //
                }
                
            },d=>Employees.CurrentItem!=null));

        internal static void Print(string path)
        {
            var info = new ProcessStartInfo(path);
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = true;
            info.Verb = "PrintTo";
            try
            {
                Process.Start(info);
            } catch(Exception e)
            {
                //
            }
        }

        private bool _IsPrinting;

        public bool IsPrinting
        {
            get => _IsPrinting;
            set
            {
                if(value == _IsPrinting)
                    return;
                _IsPrinting = value;
                OnPropertyChanged(nameof(IsPrinting));
            }
        }

        private ICommand _printSuggestionsCommand;

        public ICommand PrintDtrCommand =>
            _printSuggestionsCommand ?? (_printSuggestionsCommand = new DelegateCommand(
            d =>
            {
                if (IsPrinting) return;
                IsPrinting = true;
                Task.Factory.StartNew(() =>
                {
                    if (!(Employees.CurrentItem is Employee employee)) return;
                    
                    if(!Directory.Exists("Temp"))
                        Directory.CreateDirectory("Temp");

                    var temp = Path.Combine("Temp", $"DTR [{DateTime.Now:yy-MMM-dd}].docx");
                    var template = $@"Templates\DTR.docx";
                    
                    using(var doc = DocX.Load(template))
                    {
                        var tbl = doc.Tables.First();
                        
                        for (var day = 1; day < 32; day++)
                        {
                            var day1 = day;
                            
                            if(!DailyTimeRecord.Cache.Any(x=> x.TimeIn.Day == day1 && x.EmployeeId == employee.Id)) continue;
                            
                            var records = DailyTimeRecord.Cache.Where(x =>
                                x.TimeIn.Day == day1 && x.EmployeeId == employee.Id).ToList();
                                
                            var r = tbl.Rows[day+1];
                            
                            var timedIn = false;
                            var timeOut = DateTime.MinValue;

                            foreach (var record in records)
                            {
                                var column = 3;
                                if (record.TimeIn.TimeOfDay.Hours < 12)
                                    column = 1;

                                if (!timedIn)
                                {
                                    timedIn = true;
                                    var p = r.Cells[column].Paragraphs.First()
                                        .Append(record.TimeIn.ToString("hh:mm tt"));
                                    p.LineSpacingAfter = 0;
                                    p.Alignment = Alignment.center;
                                }

                                if (timeOut < record.TimeOut)
                                {
                                    timeOut = record.TimeOut.Value;
                                    var p2 = r.Cells[column + 1].Paragraphs.First();
                                    p2.ReplaceText(p2.Text,"");
                                    p2.Append(record.TimeOut?.ToString("hh:mm tt"));
                                    p2.LineSpacingAfter = 0;
                                    p2.Alignment = Alignment.center;
                                }
                            }

                            var total = TimeSpan.FromSeconds(records.Sum(x => x.TimeSpan?.TotalSeconds)??0);
                            
                            var p3 = r.Cells[7].Paragraphs.First().Append($"{total.Hours:00}:{total.Minutes:00}:{total.Seconds:00}");
                            p3.LineSpacingAfter = 0;
                            p3.Alignment = Alignment.center;
                        }

                        var totalTime = TimeSpan.FromSeconds(DailyTimeRecord.Cache.Where(x=>x.EmployeeId==employee.Id)
                                                                 .Sum(x => x.TimeSpan?.TotalSeconds) ?? 0);

                        var p4 = tbl.Rows[33].Cells[3].Paragraphs.First()
                            .Append($"{totalTime.Hours:00}:{totalTime.Minutes:00}:{totalTime.Seconds:00}");
                        p4.LineSpacingAfter = 0;
                        p4.Alignment = Alignment.center;
                        
                        doc.ReplaceText("[NAME]", employee.Fullname);
                        doc.ReplaceText("[DESIGNATION]", employee.Job?.Name??"N/A");
                        
                        try
                        {
                            File.Delete(temp);
                        } catch(Exception e)
                        {
                            //
                        }

                        doc.SaveAs(temp);
                    }
                    
                    Print(temp);
                    IsPrinting = false;
                });
            }));
        
        private ICommand _showPrintPayrollCommand;

        public ICommand ShowPrintPayrollCommand => _showPrintPayrollCommand ?? (_showPrintPayrollCommand = new DelegateCommand(d =>
        {
            ShowPrintDialog = true;
        },d=>!IsPrinting));

        private ICommand _printThisMonthCommand;
        public ICommand PrintThisMonthCommand => _printThisMonthCommand ?? (_printThisMonthCommand = new DelegateCommand(d =>
        {
            PrintMonth = (Months) DateTime.Now.Month;
            PrintYear = DateTime.Now.Year;
        }));

        private ICommand _printLastMonthCommand;
        public ICommand PrintLastMonthCommand => _printLastMonthCommand ?? (_printLastMonthCommand = new DelegateCommand(d =>
        {
            PrintMonth = (Months) DateTime.Now.AddMonths(-1).Month;
            PrintYear = DateTime.Now.AddMonths(-1).Year;
        }));

        private Months _PrintMonth = (Months) DateTime.Now.Month;

        public Months PrintMonth
        {
            get => _PrintMonth;
            set
            {
                if (value == _PrintMonth) return;
                _PrintMonth = value;
                OnPropertyChanged(nameof(PrintMonth));
            }
        }

        private int _PrintYear = DateTime.Now.Year;

        public int PrintYear
        {
            get => _PrintYear;
            set
            {
                if (value == _PrintYear) return;
                _PrintYear = value;
                OnPropertyChanged(nameof(PrintYear));
            }
        }

        private ICommand _printPayrollCommand;

        public ICommand PrintPayrollCommand => _printPayrollCommand ?? (_printPayrollCommand = new DelegateCommand(d =>
        {
            var date = DateTime.Parse($"{PrintMonth} {PrintYear}");
            PrintPayroll(date);
            ShowPrintDialog = false;
        }));
        
        private void PrintPayroll(DateTime month)
        {
            if (IsPrinting)
                return;
            IsPrinting = true;
            Task.Factory.StartNew(() =>
            {
                
                if (!Directory.Exists("Temp"))
                    Directory.CreateDirectory("Temp");

                var number = 0;
                var page = 1;


                var template = $@"Templates\Payroll.docx";

                var doc = DocX.Load(template);
                doc.ReplaceText("[MONTH]", month.ToString("MMMM yyyy"));
                var hasPage = false;
                foreach (var employee in Employee.Cache)
                {
                    hasPage = true;
                    var tbl = doc.Tables.First();

                    number++;

                    var row = tbl.Rows[number];

                    var p = row.Cells[0].Paragraphs.First()
                        .Append(number.ToString());
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;

                    p = row.Cells[1].Paragraphs.First()
                        .Append(employee.Fullname);
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;

                    p = row.Cells[2].Paragraphs.First().Append(employee.Job?.Name ?? "N/A");
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;

                    p = row.Cells[3].Paragraphs.First().Append(employee.Job?.Rate.ToString("#,##0.00") ?? "N/A");
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.right;

                    var hours = GetHours(employee, month).Add(TimeSpan.FromHours(employee.BonusHours));

                    p = row.Cells[4].Paragraphs.First().Append($"{hours.TotalHours:0.00}");
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.right;

                    var salary = hours.TotalHours * (employee.Job?.Rate ?? 0);
                    p = row.Cells[5].Paragraphs.First().Append($"{salary:0.00}");
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.right;

                    var deduction = 0d;
                    if (employee.Deduction?.EndsWith("%") ?? false)
                    {
                        double.TryParse(employee.Deduction.Replace("%", ""), out deduction);
                        deduction = salary * deduction;
                    }
                    else
                    {
                        double.TryParse(employee.Deduction, out deduction);
                    }

                    p = row.Cells[6].Paragraphs.First().Append($"{employee.Deduction}");
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.right;

                    p = row.Cells[7].Paragraphs.First().Append($"{salary - deduction:0.00}");
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.right;

                    if (number % 15 == 0)
                    {

                        var temp = Path.Combine("Temp",
                            $"Payroll [{DateTime.Now:MMM-yyyy}] Page-{(int)(number / 15)}.docx");
                        try
                        {
                            File.Delete(temp);
                        }
                        catch (Exception e)
                        {
                            //
                        }

                        doc.SaveAs(temp);

                        Print(temp);

                        doc.Dispose();

                        doc = DocX.Load(template);
                        doc.ReplaceText("[MONTH]", month.ToString("MMMM yyyy"));
                        hasPage = false;
                    }
                }

                if (hasPage)
                {
                    var temp = Path.Combine("Temp",
                        $"Payroll [{DateTime.Now:MMM-yyyy}] Page-{(int)(number / 15) + 1}.docx");
                    try
                    {
                        File.Delete(temp);
                    }
                    catch (Exception e)
                    {
                        //
                    }

                    doc.SaveAs(temp);

                    Print(temp);

                    doc.Dispose();

                }

                IsPrinting = false;
            });
        }
        
        private TimeSpan GetHours(Employee employee,DateTime month)
        {
            if (!employee.Job.Teaching)
                return GetNonTeaching(employee,month);
            
            var timeSpan = new TimeSpan();
            
            var schedules = Schedule.Cache.Where(x => x.EmployeeId == employee.Id).ToList();
            foreach (var schedule in schedules)
            {
                var schedItems = ScheduleItem.Cache.Where(x => x.ScheduleId == schedule.Id).ToList();
                foreach (var item in schedItems)
                {
                    var start = DateTime.Parse(item.StartTime);
                    var endTime = DateTime.Parse(item.EndTime);

                    var records = DailyTimeRecord.Cache.Where(
                        x => x.HasTimeOut && x.EmployeeId == employee.Id && x.TimeIn.Month == month.Month &&
                             x.TimeIn.Year == month.Year
                             && x.TimeIn.DayOfWeek == item.Day &&
                             (x.TimeIn.Hour >= start.Hour && x.TimeIn.Hour < endTime.Hour
                              || x.TimeOut?.Hour <= endTime.Hour && x.TimeOut?.Hour > start.Hour)).ToList();
                    
                    foreach (var dtr in records)
                    {
                        var dtrIn = DateTime.Parse(dtr.TimeIn.ToString("d") + $" {item.StartTime}");
                        var dtrOut= DateTime.Parse(dtr.TimeIn.ToString("d") + $" {item.EndTime}");
                        timeSpan = timeSpan.Add(GetTrimmedTime(dtrIn, dtrOut, dtr.TimeIn,
                            dtr.TimeOut ?? DateTime.MaxValue));
                    }
                }
            }
            
            return timeSpan;
        }

        private TimeSpan GetTrimmedTime(DateTime sIn, DateTime sOut, DateTime rIn, DateTime rOut)
        {
            var timeIn = sIn < rIn ? rIn : sIn;
            var timeOut = sOut > rOut ? rOut : sOut;
            return timeOut - timeIn;
        }

        private TimeSpan GetNonTeaching(Employee employee,DateTime month)
        {
            var records = DailyTimeRecord.Cache.Where(x => x.EmployeeId == employee.Id && x.TimeIn.Month == month.Month
                                             && x.TimeIn.Year == month.Year && x.HasTimeOut)?.ToList();
            var time = new TimeSpan();
            
            foreach (var dtr in records)
            {
                if (dtr.TimeIn.Hour < 12)
                {
                    var timeInAm = DateTime.MinValue;
                    DateTime.TryParse(dtr.TimeIn.ToString("d") + " " + Settings.Default.TimeInAM, out timeInAm);

                    var timeIn = timeInAm < dtr.TimeIn ? dtr.TimeIn : timeInAm;

                    var timeOutAm = DateTime.MinValue;
                    DateTime.TryParse(dtr.TimeIn.ToString("d") + " " + Settings.Default.TimeOutAM, out timeOutAm);

                    var timeOut = timeOutAm > dtr.TimeOut ? dtr.TimeOut : timeOutAm;
                    var span = (timeOut - timeIn) ?? TimeSpan.Zero;
                    if (span.TotalSeconds < 0) span = TimeSpan.Zero;
                    time = time.Add(span);
                }
                else
                {
                    var timeInAm = DateTime.MinValue;
                    DateTime.TryParse(dtr.TimeIn.ToString("d") + " " + Settings.Default.TimeInPM, out timeInAm);

                    var timeIn = timeInAm < dtr.TimeIn ? dtr.TimeIn : timeInAm;

                    var timeOutAm = DateTime.MinValue;
                    DateTime.TryParse(dtr.TimeIn.ToString("d") + " " + Settings.Default.TimeOutPM, out timeOutAm);

                    var timeOut = timeOutAm > dtr.TimeOut ? dtr.TimeOut : timeOutAm;
                    
                    var span = (timeOut - timeIn) ?? TimeSpan.Zero;
                    if (span.TotalSeconds < 0)
                        span = TimeSpan.Zero;
                    time = time.Add(span);
                }
            }
            
            return time;
        }

        private ListCollectionView _EmployeeSchedules;
        public ListCollectionView EmployeeSchedules
        {
            get
            {
                if (_EmployeeSchedules != null) return _EmployeeSchedules;
                _EmployeeSchedules = new ListCollectionView(Schedule.Cache);
                _EmployeeSchedules.Filter = FilterSchedule;
                Employees.CurrentChanged += (sender, args) =>
                {
                    if (Employees.IsAddingNew)
                        return;
                    _EmployeeSchedules.Filter = FilterSchedule;
                };
                _EmployeeSchedules.CurrentChanged += (sender, args) =>
                {
                    Schedule.SelectedItem = _EmployeeSchedules.CurrentItem as Schedule;
                };
                return _EmployeeSchedules;
            }
        }

        private bool FilterSchedule(object o)
        {
            if (!(o is Schedule sched)) return false;
            if (!(Employees.CurrentItem is Employee emp)) return false;
            return sched.EmployeeId == emp.Id;
        }

        private bool? _SelectionState = false;

        public bool? SelectionState
        {
            get => _SelectionState;
            set
            {
                if (value == _SelectionState)
                    return;
                _SelectionState = value;
                OnPropertyChanged(nameof(SelectionState));

                var students = Employee.Cache.ToList();
                foreach (var student in students)
                {
                    student.Select(_SelectionState ?? false);
                }
                OnPropertyChanged(nameof(HasSelected));
            }
        }

        private bool _HasSelected;

        public bool HasSelected
        {
            get
            {
                var students = Employee.Cache;
                return students.Any(x => x.IsSelected);
            }
            set
            {
                _HasSelected = value;
                OnPropertyChanged(nameof(HasSelected));
            }
        }

        private ICommand _toggleDeveloperCommand;

        public ICommand ToggleDeveloperCommand =>
            _toggleDeveloperCommand ?? (_toggleDeveloperCommand = new DelegateCommand(
                d =>
                {
                    ShowDev = !ShowDev;
                }));
        private bool _ShowDev;

        public bool ShowDev
        {
            get => _ShowDev;
            set
            {
                if(value == _ShowDev)
                    return;
                _ShowDev = value;
                OnPropertyChanged(nameof(ShowDev));
            }
        }

        private ICommand _runExternalCommand;

        public ICommand RunExternalCommand => _runExternalCommand ?? (_runExternalCommand = new DelegateCommand<string>(d =>
        {
            Process.Start(d);
        }));

        private bool _ShowPrintDialog;

        public bool ShowPrintDialog
        {
            get => _ShowPrintDialog;
            set
            {
                if (value == _ShowPrintDialog) return;
                _ShowPrintDialog = value;
                OnPropertyChanged(nameof(ShowPrintDialog));
            }
        }

        
    }
}