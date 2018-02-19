using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media.Imaging;
using DPFP;
using DPFP.Capture;
using DPFP.Processing;
using DPFP.Verification;
using Models;

namespace NORSU.BioPay.ViewModels
{
    static class ScannerExtensions
    {
        private static readonly Verification Verifier = new Verification(2147);

        public static byte[] ToBytes(this DPFP.Sample Sample)
        {
            DPFP.Capture.SampleConversion Convertor = new DPFP.Capture.SampleConversion();
            Bitmap bitmap = null;
            
            Convertor.ConvertToPicture(Sample, ref bitmap);

            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
            
        }

        public static DPFP.FeatureSet ExtractFeatures(this DPFP.Sample Sample, DPFP.Processing.DataPurpose Purpose)
        {
            var Extractor = new DPFP.Processing.FeatureExtraction();
            var feedback = DPFP.Capture.CaptureFeedback.None;
            var features = new DPFP.FeatureSet();
            Extractor.CreateFeatureSet(Sample, Purpose, ref feedback, ref features);
            if (feedback == DPFP.Capture.CaptureFeedback.Good)
                return features;
            else
                return null;
        }

        public static FingerPrint ToFingerPrint(Sample sample)
        {
            var features = sample.ExtractFeatures(DataPurpose.Verification);

            try
            {
                var list = Models.FingerPrint.Cache.ToList();
                foreach (var finger in list)
                {
                    using (var ms = new MemoryStream(finger.Template))
                    {
                        var template = new Template(ms);
                        var result = new Verification.Result();
                        Verifier.Verify(features, template, ref result);
                        if (result.Verified)
                        {
                            return finger;
                        }
                    }
                }
            }
            catch (Exception)
            {
                //throw;
            }
            return null;
        }
    }
    
    class Scanner : ViewModelBase, DPFP.Capture.EventHandler
    {
        private static Capture Capturer;
        private static Enrollment Enroller;
        private static Verification Verifier;

        private Scanner()
        {
            
        }
        private static Scanner _instance;
        public static Scanner Instance => _instance ?? (_instance = new Scanner());

        private bool _IsConnected;

        public bool IsConnected
        {
            get => _IsConnected;
            set
            {
                if(value == _IsConnected)
                    return;
                _IsConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        private string _Status;

        public string Status
        {
            get => _Status;
            set
            {
                if(value == _Status)
                    return;
                _Status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        
        public static void Start()
        {
            if (Capturer != null) return;
            try
            {
                Capturer = new Capture(Priority.Normal);
                if (Capturer == null)
                {
                }
                else
                {
                    Capturer.EventHandler = Instance;
                    Capturer.StartCapture();
                    Enroller = new Enrollment();
                    Verifier = new Verification(2147);
                }
            }
            catch (Exception e)
            {
                Stop();
            }

        }
        
        public static void Stop()
        {
            try
            {
                Capturer.StopCapture();
                Enroller = null;
                Verifier = null;
                Capturer.Dispose();
                Capturer = null;
                GC.Collect();
            }
            catch (Exception e)
            {
                //
            }
        }
        
        public void OnComplete(object Capture, string ReaderSerialNumber, Sample Sample)
        {
            Messenger.Default.Broadcast(Messages.Scan, Sample);
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber)
        {
        }

        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
        {
            
        }

        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            Instance.IsConnected = true;
        }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            Instance.IsConnected = false;
        }

        public void OnSampleQuality(object Capture, string ReaderSerialNumber, CaptureFeedback CaptureFeedback)
        {
            switch(CaptureFeedback)
            {
                case CaptureFeedback.Good:
                    break;
                case CaptureFeedback.None:
                    break;
                case CaptureFeedback.TooLight:
                    Instance.Status = ("Too light!");
                    break;
                case CaptureFeedback.TooDark:
                    Instance.Status = ("Too dark!");
                    break;
                case CaptureFeedback.TooNoisy:
                    Instance.Status = ("Too noisy!");
                    break;
                case CaptureFeedback.LowContrast:
                    Instance.Status = ("Low contrast!");
                    break;
                case CaptureFeedback.NotEnoughFeatures:
                    Instance.Status = ("Not enough features!");
                    break;
                case CaptureFeedback.NoCentralRegion:
                    Instance.Status = ("No central region!");
                    break;
                case CaptureFeedback.NoFinger:
                    Instance.Status = ("No finger!");
                    break;
                case CaptureFeedback.TooHigh:
                    Instance.Status = ("Too High!");
                    break;
                case CaptureFeedback.TooLow:
                    Instance.Status = ("Too low!");
                    break;
                case CaptureFeedback.TooLeft:
                    Instance.Status = ("Too left!");
                    break;
                case CaptureFeedback.TooRight:
                    Instance.Status = ("Too right!");
                    break;
                case CaptureFeedback.TooStrange:
                    Instance.Status = ("Too strange!");
                    break;
                case CaptureFeedback.TooFast:
                    Instance.Status = ("Too fast!");
                    break;
                case CaptureFeedback.TooSkewed:
                    Instance.Status = ("Too skewed!");
                    break;
                case CaptureFeedback.TooShort:
                    Instance.Status = ("Too short!");
                    break;
                case CaptureFeedback.TooSlow:
                    Instance.Status = ("Too slow!");
                    break;
                case CaptureFeedback.TooSmall:
                    Instance.Status = ("Too small!");
                    break;
                    //   default:
                    //  throw new ArgumentOutOfRangeException(nameof(CaptureFeedback), CaptureFeedback, null);
            }
        }
    }
}
