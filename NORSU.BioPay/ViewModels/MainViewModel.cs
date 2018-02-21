using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DPFP;
using MaterialDesignThemes.Wpf;
using Models;
using NORSU.BioPay.Views;

namespace NORSU.BioPay.ViewModels
{
    class MainViewModel:ViewModelBase
    {
        public const long ClockIndex = 0, AdminIndex = 1;
        private MainViewModel()
        {
            
            Messenger.Default.AddListener<Sample>(Messages.Scan, sample =>
            {
                MessageBox.Show("asdf");
            });

            if (Models.User.Cache.Count == 0 && Models.Employee.Cache.Count==0)
            {
                ScreenIndex = AdminIndex;
            }
        }

        private static MainViewModel _instance;
        public static MainViewModel Instance => _instance ?? (_instance = new MainViewModel());

        private long _ScreenIndex ;

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
                ScreenIndex = AdminIndex;
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
                return _employees;
            }
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
            
            var dlg = new AddFingerViewModel();
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
    }
}
