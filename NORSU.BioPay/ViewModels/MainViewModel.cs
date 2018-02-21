using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DPFP;
using Models;

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

        private ICommand _showAdminCommand;

        public ICommand ShowAdminCommand => _showAdminCommand ?? (_showAdminCommand = new DelegateCommand(d =>
        {
            ScreenIndex = AdminIndex;
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

        
    }
}
