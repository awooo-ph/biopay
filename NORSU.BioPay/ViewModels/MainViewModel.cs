using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using DPFP;

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
    }
}
