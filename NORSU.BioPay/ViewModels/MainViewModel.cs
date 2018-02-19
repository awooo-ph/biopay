using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using DPFP;

namespace NORSU.BioPay.ViewModels
{
    class MainViewModel:ViewModelBase
    {
        private MainViewModel()
        {
            
            Messenger.Default.AddListener<Sample>(Messages.Scan, sample =>
            {
                MessageBox.Show("asdf");
            });
        }

        private static MainViewModel _instance;
        public static MainViewModel Instance => _instance ?? (_instance = new MainViewModel());
        
        
    }
}
