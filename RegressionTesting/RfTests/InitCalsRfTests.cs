#define WR_RES_TO_CONSOLE //Option to write out results to console
#define WR_RES_TO_PDF     //Option to Create PDF report
#define WR_RES_TO_TXT

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using niVisaWrapper;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;

using iTextSharp.text;
using iTextSharp.text.pdf;
using AdiCmdServerClient;
using DotNETRoutines;
using AdiMathLibrary;






namespace mykonosUnitTest
{
    
    /// <summary>
    /// Test Suite of Tx RF tests
    /// </summary>
    //[TestFixture("Tx 20/100MHz, IQrate  122.88MHz, Dec5", "3.5")]
    //[TestFixture("Tx 75/200MHz, IQrate 245.76MHz, Dec5", "3.5")]
    [TestFixture("Tx 20/100MHz, IQrate  122.88MHz, Dec5", "2.5")]
    //[TestFixture("Tx 75/200MHz, IQrate 245.76MHz, Dec5", "2.5")]
    //[TestFixture("Tx 20/100MHz, IQrate  122.88MHz, Dec5", "0.7")]
    //[TestFixture("Tx 75/200MHz, IQrate 245.76MHz, Dec5", "0.7")]
    [NUnit.Framework.Category("RF")]
    public class InitCalsRfTests
    {

        static MeasurementEquipment measEquipment = new MeasurementEquipment();
        public string TestBoard;
        private string TxProfile;
        private string TxProfileString;
        public static TestSetupConfig settings = new TestSetupConfig();
        public static string ResPath = @"..\..\..\TestResults\TxRfTests\";
        public InitCalsRfTests()
        {
            this.TxProfile = settings.mykSettings.txProfileName;
            this.TxProfileString = Helper.parseProfileName(TxProfile);
            ResPath = System.IO.Path.Combine(ResPath, TxProfileString);
            System.IO.Directory.CreateDirectory(ResPath);
        }

        public InitCalsRfTests(string TxProfile, string TestBoardFreq)
        {
            this.TxProfile = TxProfile;
            this.TestBoard = TestBoardFreq;
            this.TxProfileString = Helper.parseProfileName(TxProfile);
            ResPath = System.IO.Path.Combine(ResPath, TxProfileString);
            System.IO.Directory.CreateDirectory(ResPath);
        }

        /// <summary>
        /// Mykonos Test Setup Prior to Init Cal Testing
        /// Setup Parameters:  
        /// From Locally Stored ARM Firmware     @"..\..\..\resources\Profiles";
        /// From Locally Stored Default Profile  @"..\..\..\resources\ArmBinaries"
        /// Profiles as per test Case
        /// Default JESD settings check TestSettings.cs for more details
        /// </summary>
        [SetUp]
        public void InitCalsRfTestsInit()
        {
           
            if (TestBoard == "3.5")
            {
                settings.txPllLoFreq_Hz = 3500000000;
                settings.rxPllLoFreq_Hz = 3500000000;
            }
            else if (TestBoard == "2.5")
            {
                settings.txPllLoFreq_Hz = 2500000000;
                settings.rxPllLoFreq_Hz = 2500000000;
            }
            else if (TestBoard == "0.7")
            {
                settings.txPllLoFreq_Hz = 700000000;
                settings.rxPllLoFreq_Hz = 700000000;
            }
            else
            {
                settings.txPllLoFreq_Hz = 3500000000;
                settings.rxPllLoFreq_Hz = 3500000000;
            }
            settings.testBoard = TestBoard;
            settings.mykSettings.txProfileName = TxProfile;
            //Call Arm Test Setup- Does not run Init Cals
            //TestSetup.ArmTestSetupInit(settings);
 
        }

        ///<summary>
        /// RF Test Name: 
        ///      TxQECInitCalCheck
        /// RF Feature Under Test: 
        ///     QEC Initial Calibration	
        /// RF Test Procedure: 
        ///     Initialise Mykonos. Enable all Init Cals excepting TX QEC
        ///     Based on Profile Data Determine Profile BW & Sampling Freq
        ///     & LO Frequency.
        ///     Configure FPGA Tone Generator & Enable Mykonos Datapath
        ///     From PXA Analayser record
        ///         1. Fundamental amplitude (dBm)
        ///         2. Image Power(dBm)
        ///     Reset and Initialise Mykonos. Enable ALL Init-cals including TX QEC
        ///      From PXA Analayser record
        ///         1. Fundamental amplitude (dBm)
        ///         3. Image Power(dBm)
        /// RF Test Pass Criteria: 
        ///      Check Image power is less when Init-cals has been enabled.	
        ///</summary>
         [Test, Sequential]
         [NUnit.Framework.Category("TX")]
        public static void TxQECInitCalCheck([Values(Mykonos.TXCHANNEL.TX1, Mykonos.TXCHANNEL.TX2)]Mykonos.TXCHANNEL channel)

        {
            //Setup the calibrations
             settings.calMask = (UInt32)(Mykonos.CALMASK.TX_BB_FILTER) |
                   (UInt32)(Mykonos.CALMASK.ADC_TUNER) |
                   (UInt32)(Mykonos.CALMASK.TIA_3DB_CORNER) |
                   (UInt32)(Mykonos.CALMASK.DC_OFFSET) |
                   (UInt32)(Mykonos.CALMASK.TX_ATTENUATION_DELAY) |
                   (UInt32)(Mykonos.CALMASK.RX_GAIN_DELAY) |
                   (UInt32)(Mykonos.CALMASK.FLASH_CAL) |
                   (UInt32)(Mykonos.CALMASK.PATH_DELAY) |
                   (UInt32)(Mykonos.CALMASK.TX_LO_LEAKAGE_INTERNAL) |
                //(UInt32)(Mykonos.CALMASK.TX_QEC_INIT) |
                   (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_LO_DELAY) |
                   (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_RX_QEC_INIT) |
                   (UInt32)(Mykonos.CALMASK.RX_LO_DELAY) |
                   (UInt32)(Mykonos.CALMASK.RX_QEC_INIT);
            TestSetup.TestSetupInit(settings);
            //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
            double[] profileInfo = new double[3];
            int backoff = 0;
            string boardfreq = settings.testBoard;
            profileInfo[0] = settings.txProfileData.IqRate_kHz;
            profileInfo[1] = settings.txPllLoFreq_Hz;
            profileInfo[2] = settings.txProfileData.PrimarySigBw_Hz;
            
            double txIqDataRate_kHz = profileInfo[0]; 
            double profileBandwidth_MHz = profileInfo[2] / 1000000; 
            Console.WriteLine("IQ Data Rate (kHz): " + txIqDataRate_kHz);
            Console.WriteLine("Profile Bandwdith (MHz): " + profileBandwidth_MHz);
            double freqTxLo_MHz = profileInfo[1] / 1000000; 
            Console.WriteLine("Tx LO Frequency (MHz): " + freqTxLo_MHz);

            //Define variables for storing Fundamental Amplitue, LoLeakage and ImageAmplitude
            double [] fundAmp = new double[2];
            double [] imageAmp = new double[2];

            //Connect to Signal Analyser 
            //The span is fixed to 100MHz 
            //Note: this may need to be set depending on profile.
            NumberStyles style = NumberStyles.AllowExponent | NumberStyles.Number;
            SA_AgilentPXA pxa = new SA_AgilentPXA(measEquipment.PXAAddress);
            pxa.SetCenterSpan(freqTxLo_MHz, 100, 0);
            pxa.SetAtten(20);
            pxa.SetRefLevel(10);
            
            //Config and Enable Mykonos with Test Specific Settings
            //Use Default Test Constructor
            //Start Calibration

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.setTx1Attenuation(0);

            System.Threading.Thread.Sleep(500);
            Link.Mykonos.radioOn();
            Link.Disconnect();

            //Generate Tone and Measure Image 
            double offsetFreq_Mhz = 10;
            double testFreq_MHz = freqTxLo_MHz + offsetFreq_Mhz;
            double offsetFreq_Hz = offsetFreq_Mhz * 1000000;
            Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, profileInfo, offsetFreq_Hz, backoff);
            
             //Take Measurements from PXA
             pxa.SetMarker(3, freqTxLo_MHz - (offsetFreq_Hz / 1000000)); //Image marker
             pxa.SetMarker(1, testFreq_MHz); //Fundamental amplitue marker

             System.Threading.Thread.Sleep(500);
             fundAmp[0] = Double.Parse(pxa.GetMarker(1), style);  
             imageAmp[0] = Double.Parse(pxa.GetMarker(3), style);
             Console.WriteLine("No Init QEC" + ": FundAmp: " + fundAmp[0] + ", Image:   " + imageAmp[0] + ",   " + (fundAmp[0] - imageAmp[0]) + ",   ");

             //Turn Radio Off and Re-Initialise with TX QEC init Cal 
             settings.calMask = settings.calMask | (UInt32)(Mykonos.CALMASK.TX_QEC_INIT);
             TestSetup.TestSetupInit(settings);
             Link = AdiCommandServerClient.Instance;
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
             Link.Mykonos.setTx1Attenuation(0);
             Link.Mykonos.radioOff();
             System.Threading.Thread.Sleep(500);
           

             System.Threading.Thread.Sleep(500);
             Link.Disconnect();

             //Config and Enable Mykonos with Test Specific Settings
             Link = AdiCommandServerClient.Instance;
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
             Link.Mykonos.setTx1Attenuation(0);
             Link.Mykonos.radioOn();
             Link.Disconnect();

             //Generate Tone and Measure Image 
             Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, profileInfo, offsetFreq_Hz, backoff);

             //Take Measurements from PXA
             System.Threading.Thread.Sleep(500);
             fundAmp[1] = Double.Parse(pxa.GetMarker(1), style);
             imageAmp[1] = Double.Parse(pxa.GetMarker(3), style);
             Console.WriteLine("Init QEC" + ": FundAmp: " + fundAmp[1] + ", Image:   " + imageAmp[1] + ",   " + (fundAmp[1] - imageAmp[1]) + ",   ");
             Console.WriteLine("Image Diff:   " + (imageAmp[0] - imageAmp[1]));
             // Check that Image has reduced
             NUnit.Framework.Assert.Less(imageAmp[1],imageAmp[0]);

        }
         ///<summary>
         /// RF Test Name: 
         ///      TxLOLInitCalCheck
         /// RF Feature Under Test: 
         ///     QEC Initial Calibration	
         /// RF Test Procedure: 
         ///     Initialise Mykonos. Enable all Init Cals excepting TX QEC
         ///     Based on Profile Data Determine Profile BW & Sampling Freq
         ///     & LO Frequency.
         ///     Configure FPGA Tone Generator & Enable Mykonos Datapath
         ///     From PXA Analayser record
         ///         1. Fundamental amplitude (dBm)
         ///         2. LO leakage (dBm)
         ///     Reset and Initialise Mykonos. Enable ALL Init-cals including TX QEC
         ///      From PXA Analayser record
         ///         1. Fundamental amplitude (dBm)
         ///         3. LO leakage (dBm)
         /// RF Test Pass Criteria: 
         ///      Check Image power is less when Init-cals has been enabled.	
         ///</summary>
         [Test, Sequential]
         [NUnit.Framework.Category("TX")]
         public static void TxLOLInitCalCheck([Values(Mykonos.TXCHANNEL.TX1, Mykonos.TXCHANNEL.TX2)]Mykonos.TXCHANNEL channel)
         {
             //Setup the calibrations
             settings.calMask = (UInt32)(Mykonos.CALMASK.TX_BB_FILTER) |
                    (UInt32)(Mykonos.CALMASK.ADC_TUNER) |
                    (UInt32)(Mykonos.CALMASK.TIA_3DB_CORNER) |
                    (UInt32)(Mykonos.CALMASK.DC_OFFSET) |
                    (UInt32)(Mykonos.CALMASK.TX_ATTENUATION_DELAY) |
                    (UInt32)(Mykonos.CALMASK.RX_GAIN_DELAY) |
                    (UInt32)(Mykonos.CALMASK.FLASH_CAL) |
                    (UInt32)(Mykonos.CALMASK.PATH_DELAY) |
                 //(UInt32)(Mykonos.CALMASK.TX_LO_LEAKAGE_INTERNAL) |
                    (UInt32)(Mykonos.CALMASK.TX_QEC_INIT) |
                    (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_LO_DELAY) |
                    (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_RX_QEC_INIT) |
                    (UInt32)(Mykonos.CALMASK.RX_LO_DELAY) |
                    (UInt32)(Mykonos.CALMASK.RX_QEC_INIT);
             TestSetup.TestSetupInit(settings);

             //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
             double[] profileInfo = new double[3];
             int backoff = 0;
             string boardfreq = settings.testBoard;
             profileInfo[0] = settings.txProfileData.IqRate_kHz;
             profileInfo[1] = settings.txPllLoFreq_Hz;
             profileInfo[2] = settings.txProfileData.PrimarySigBw_Hz;

             double txIqDataRate_kHz = profileInfo[0];
             double profileBandwidth_MHz = profileInfo[2] / 1000000;
             Console.WriteLine("IQ Data Rate (kHz): " + txIqDataRate_kHz);
             Console.WriteLine("Profile Bandwdith (MHz): " + profileBandwidth_MHz);
             double freqTxLo_MHz = profileInfo[1] / 1000000;
             Console.WriteLine("Tx LO Frequency (MHz): " + freqTxLo_MHz);

             //Define variables for storing Fundamental Amplitue, LoLeakage and ImageAmplitude
             double[] fundAmp = new double[2];
             double[] LoLeakageAmp = new double[2];

             //Connect to Signal Analyser 
             //The span is fixed to 100MHz 
             //Set Marker 2 as LO leakage marker
             //Note: this may need to be set depending on profile.
             NumberStyles style = NumberStyles.AllowExponent | NumberStyles.Number;
             SA_AgilentPXA pxa = new SA_AgilentPXA(measEquipment.PXAAddress);
             pxa.SetCenterSpan(freqTxLo_MHz, 100, 0);
             pxa.SetAtten(20);
             pxa.SetRefLevel(10);
             pxa.SetMarker(2, freqTxLo_MHz);

             //Config and Enable Mykonos with Test Specific Settings
             //Use Default Test Constructor
             //Start Calibration

             AdiCommandServerClient Link = AdiCommandServerClient.Instance;
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
             Link.Mykonos.setTx1Attenuation(0);

             System.Threading.Thread.Sleep(500);
             Link.Mykonos.radioOn();
             Link.Disconnect();

             //Generate Tone and Measure Image 
             double offsetFreq_Mhz = 10;
             double testFreq_MHz = freqTxLo_MHz + offsetFreq_Mhz;
             double offsetFreq_Hz = offsetFreq_Mhz * 1000000;
             Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, profileInfo, offsetFreq_Hz, backoff);

             //Take Measurements from PXA
             pxa.SetMarker(1, testFreq_MHz); //Fundamental amplitue marker

             System.Threading.Thread.Sleep(500);
             fundAmp[0] = Double.Parse(pxa.GetMarker(1), style);
             LoLeakageAmp[0] = Double.Parse(pxa.GetMarker(2), style);
             Console.WriteLine("No Init LO Leakage" + ": FundAmp: " + fundAmp[0] + ", LO Leakage:   " + LoLeakageAmp[0]);


             //Re-Initialise with TX QEC init Cal 
             settings.calMask = settings.calMask | (UInt32)(Mykonos.CALMASK.TX_LO_LEAKAGE_INTERNAL);
             TestSetup.TestSetupInit(settings);
             //Turn Radio Off and 
             Link = AdiCommandServerClient.Instance;
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
             Link.Mykonos.setTx1Attenuation(0);
             Link.Mykonos.radioOff();
             System.Threading.Thread.Sleep(500);



             System.Threading.Thread.Sleep(500);
             Link.Disconnect();

             //Config and Enable Mykonos with Test Specific Settings
             Link = AdiCommandServerClient.Instance;
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
             Link.Mykonos.setTx1Attenuation(0);
             Link.Mykonos.radioOn();
             Link.Disconnect();

             //Generate Tone and Measure Image 
             Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, profileInfo, offsetFreq_Hz, backoff);

             //Take Measurements from PXA
             System.Threading.Thread.Sleep(500);
             fundAmp[1] = Double.Parse(pxa.GetMarker(1), style);
             LoLeakageAmp[1] = Double.Parse(pxa.GetMarker(2), style);
             Console.WriteLine("Init LO Leakage" + ": FundAmp: " + fundAmp[1] + ", LO Leakage:   " + LoLeakageAmp[1] + ",   " + (fundAmp[1] - LoLeakageAmp[1]) + ",   ");
             Console.WriteLine("Lo Leakage Diff:   " + (LoLeakageAmp[0] - LoLeakageAmp[1]));
             // Check that Image has reduced
             NUnit.Framework.Assert.Less(LoLeakageAmp[1], LoLeakageAmp[0]);

         }

    }
}
