using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using DPFP;
using DPFP.Processing;
using DPFP.Verification;
using MaterialDesignThemes.Wpf;
using Models;

namespace NORSU.BioPay.ViewModels
{
    class AddFingerViewModel : ViewModelBase
    {
        public AddFingerViewModel(Employee employee)
        {
            Fingers.Add(CurrentSample);
            Employee = employee;
        }

        private Employee Employee { get; set; }
        
        private FingerSample CurrentSample = new FingerSample();
        
        private bool _IsValid;

        public bool IsValid
        {
            get => _IsValid;
            set
            {
                if(value == _IsValid)
                    return;
                _IsValid = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private ObservableCollection<FingerSample> _fingers;

        public ObservableCollection<FingerSample> Fingers =>
            _fingers ?? (_fingers = new ObservableCollection<FingerSample>());

        private Enrollment Enroller = new Enrollment();

        private Template _ResultTemplate;

        public Template ResultTemplate
        {
            get => _ResultTemplate;
            set
            {
                if(value == _ResultTemplate)
                    return;
                _ResultTemplate = value;
                OnPropertyChanged(nameof(ResultTemplate));
            }
        }

        private byte[] _ResultThumbnail;

        public byte[] ResultThumbnail
        {
            get => _ResultThumbnail;
            set
            {
                if(value == _ResultThumbnail)
                    return;
                _ResultThumbnail = value;
                OnPropertyChanged(nameof(ResultThumbnail));
            }
        }

        private Verification Verifier = new Verification(2174);

        public void Scan(Sample sample)
        {
            CurrentSample.Picture = sample.ToImageBytes();
            CurrentSample.IsWaiting = false;
            CurrentSample.IsValid = true;

            var finger = sample.ToFingerPrint();
            if (finger != null)
            {
                if (finger.Employee.IsDeleted)
                {
                    finger.Update(nameof(Employee.EmployeeId),Employee.Id);
                    DialogHost.CloseDialogCommand.Execute(false,Application.Current.MainWindow);
                    return;
                }
                
                CurrentSample.IsValid = false;
                IsValid = false;
                HasError = true;
                ErrorMessage = $"Finger already registered to {finger.Employee.Fullname}.";
                return;
            }
            
            ResultThumbnail = CurrentSample.Picture;
            
            var features = sample.ExtractFeatures(DataPurpose.Enrollment);
            Enroller.AddFeatures(features);

            if (Enroller.TemplateStatus == Enrollment.Status.Ready)
            {
                ResultTemplate = Enroller.Template;
                IsValid = true;
                return;
            }
            
            CurrentSample = new FingerSample();
            awooo.Context.Post(d =>
            {
                Fingers.Add(CurrentSample);
            },null);
            
        }

        private bool _HasError;

        public bool HasError
        {
            get => _HasError;
            set
            {
                if(value == _HasError)
                    return;
                _HasError = value;
                OnPropertyChanged(nameof(HasError));
            }
        }

        private string _ErrorMessage;

        public string ErrorMessage
        {
            get => _ErrorMessage;
            set
            {
                if(value == _ErrorMessage)
                    return;
                _ErrorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        
    }
}
