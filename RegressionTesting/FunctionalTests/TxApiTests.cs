using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;


using NUnit.Framework;

using AdiCmdServerClient;

namespace mykonosUnitTest
{

    //TODO: For this Test Fixture
    [TestFixture]
    [Category("ApiFunctional")]
    public class TxApiFunctionalTests
    {
        public const byte MykonosSpi = 0x1;
        public const byte AD9528Spi = 0x2;
        public static TestSetupConfig settings = new TestSetupConfig();
        public static Int16[] TxFirCoeffs = new Int16[16] {  
                       -15,    33,  -92,  207, 
                      -457, 1094, -3342, 21908,
                     -4607,  2226,  -862,  376, 
                     -169,    72, -29,     19};
        /// <summary>
        /// ApiFunctionalTests Configure Test Setup Prior to QA Api Functional Tests
        /// Setup Parameters:  Refer to Test Settings
        /// From Locally Stored ARM Firmware     @"..\..\..\..\mykonos_resources\";
        /// Default Test Settings as per TestSettings.cs
        /// </summary>
        [SetUp] 
        public void TxApiTestInit()
        {
            //Set Calibration Mask
            UInt32 calMask = (UInt32)(Mykonos.CALMASK.TX_BB_FILTER) |
                           (UInt32)(Mykonos.CALMASK.ADC_TUNER) |
                           (UInt32)(Mykonos.CALMASK.TIA_3DB_CORNER) |
                           (UInt32)(Mykonos.CALMASK.DC_OFFSET) |
                           (UInt32)(Mykonos.CALMASK.FLASH_CAL) |
                           (UInt32)(Mykonos.CALMASK.PATH_DELAY) |
                           (UInt32)(Mykonos.CALMASK.TX_LO_LEAKAGE_INTERNAL) |
                           (UInt32)(Mykonos.CALMASK.TX_QEC_INIT) |
                           (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_LO_DELAY) |
                           (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_RX_QEC_INIT) |
                           (UInt32)(Mykonos.CALMASK.RX_LO_DELAY) |
                           (UInt32)(Mykonos.CALMASK.RX_QEC_INIT);
            settings.calMask = calMask;
            //Call Test Setup 
            TestSetup.TestSetupInit(settings);
            Console.WriteLine("TxApiFunctional Test Setup: Complete" );
        }

        ///<summary>
        /// API Test Name: 
        ///     SetTx1Atten
        /// API Under-Test: 
        ///     MYKONOS_setTx1Attenuation	
        /// API Test Description: 
        ///     Call API MYKONOS_setRfPllFrequency to set TX_PLL 
        ///     from  41.95            
        /// API Test Pass Criteria: 
        ///     Check Tx Attenuation Registers are updated. 
        ///     As Expected with the correct Step Size
        /// 
        ///</summary>
        [Test]
        public static void CheckSetTx1Atten41_95()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            double atten = 41.95;
            Link.Mykonos.setTx1Attenuation(atten);
            byte spiData1 = 0x0;
            double readback = 0;

            //Check Tx Attenuation Registers have been Updated
            Console.WriteLine("Test Setup Config Step Size:" + settings.txAttenStepSize.ToString("d"));

            Link.Mykonos.radioOn();
            
            spiData1 = Link.spiRead(0x96D); Console.WriteLine("Myk: Tx  1 Atten Step Size:" + spiData1.ToString("X"));
            Assert.AreEqual(settings.txAttenStepSize, ((spiData1 & 0x30) >> 5), "Myk: Tx 1 Atten StepSize not as expected");
            spiData1 = Link.spiRead(0x960); Console.WriteLine("Myk Addr: 0x960:" + spiData1.ToString("X"));
            Assert.AreEqual(0x47, spiData1 & 0xFF, "Myk: Tx  1Attenuation not as expected");
            spiData1 = Link.spiRead(0x961); Console.WriteLine("Myk Addr: 0x961:" + spiData1.ToString("X"));
            Assert.AreEqual(0x03, spiData1 & 0x03, "Myk: Tx  1Attenuation not as expected");
            Link.Mykonos.getTx1Attenuation(ref readback); Console.WriteLine("Myk: getTx1Attenuation return value:" + readback.ToString());
            Assert.AreEqual(atten, readback, "Myk: Tx Attenuation readback not as expected");

            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     SetTx1Atten
        /// API Under-Test: 
        ///     MYKONOS_setTx1Attenuation	
        /// API Test Description: 
        ///     Call API MYKONOS_setRfPllFrequency to set TX_PLL 
        ///     from  60.95            
        /// API Test Pass Criteria: 
        ///     Check Tx Attenuation Registers are updated. 
        ///     As Expected with the correct Step Size
        /// 
        ///</summary>
        [Test]
        public static void CheckSetTx1Atten10_95()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            double atten = 10.95;
            Link.Mykonos.setTx1Attenuation(atten);
            byte spiData1 = 0x0;
            double readback = 0;

            //Check Tx Attenuation Registers have been Updated
            Console.WriteLine("Test Setup Config Step Size:" + settings.txAttenStepSize.ToString("d"));

            Link.Mykonos.radioOn();

            spiData1 = Link.spiRead(0x96D); Console.WriteLine("Myk: Tx Atten Step Size:" + spiData1.ToString("X"));
            Assert.AreEqual(settings.txAttenStepSize, ((spiData1 & 0x30) >> 5), "Myk: Tx Atten StepSize not as expected");
            spiData1 = Link.spiRead(0x960); Console.WriteLine("Myk Addr:Tx1 Atten LSB:" + spiData1.ToString("X"));
            Assert.AreEqual(0xDB, spiData1 & 0xFF, "Myk: Tx Attenuation not as expected");
            spiData1 = Link.spiRead(0x961); Console.WriteLine("Myk Addr:Tx1 Atten MSB:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0x03, "Myk: Tx Attenuation not as expected");
            Link.Mykonos.getTx1Attenuation(ref readback); Console.WriteLine("Myk: getTx1Attenuation return value:" + readback.ToString());
            Assert.AreEqual(atten, readback, "Myk: Tx Attenuation readback not as expected");

            Link.Disconnect();

        }
        ///<summary>
        /// API Test Name: 
        ///     SetTx2Atten41_95()
        /// API Under-Test: 
        ///     MYKONOS_setTx2Attenuation	
        /// API Test Description: 
        ///     Call API MYKONOS_setTx2Attenuation to set TX_PLL 
        ///     from  60.95            
        /// API Test Pass Criteria: 
        ///     Check Tx Attenuation Registers are updated. 
        ///     As Expected with the correct Step Size
        /// 
        ///</summary>
        [Test]
        public static void CheckSetTx2Atten41_95()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            double atten = 41.95;
            Link.Mykonos.setTx2Attenuation(atten);
            byte spiData1 = 0x0;
            double readback = 0;

            //Check Tx Attenuation Registers have been Updated
            Console.WriteLine("Test Setup Config Step Size:" + settings.txAttenStepSize.ToString("d"));

            Link.Mykonos.radioOn();

            spiData1 = Link.spiRead(0x96E); Console.WriteLine("Myk: Tx2 Atten Step Size:" + spiData1.ToString("X"));
            Assert.AreEqual(settings.txAttenStepSize, ((spiData1 & 0x30) >> 5), "Myk: Tx Atten StepSize not as expected");
            spiData1 = Link.spiRead(0x962); Console.WriteLine("Myk Addr:Tx2 Atten LSB:" + spiData1.ToString("X"));
            Assert.AreEqual(0x47, spiData1 & 0xFF, "Myk: Tx Attenuation not as expected");
            spiData1 = Link.spiRead(0x963); Console.WriteLine("Myk Addr:Tx2 Atten MSB:" + spiData1.ToString("X"));
            Assert.AreEqual(0x03, spiData1 & 0x03, "Myk: Tx Attenuation not as expected");
            Link.Mykonos.getTx2Attenuation(ref readback); Console.WriteLine("Myk: getTx2Attenuation return value:" + readback.ToString());
            Assert.AreEqual(atten, readback, "Myk: Tx Attenuation readback not as expected");
            Link.Disconnect();

        }
        ///<summary>
        /// API Test Name: 
        ///     SetTx2Atten60_95()
        /// API Under-Test: 
        ///     MYKONOS_setTx2Attenuation	
        /// API Test Description: 
        ///     Call API MYKONOS_setTx2Attenuation to set TX_PLL 
        ///     from  6095            
        /// API Test Pass Criteria: 
        ///     Check Tx Attenuation Registers are updated. 
        ///     As Expected with the correct Step Size
        /// 
        ///</summary>
        [Test]
        public static void CheckSetTx2Atten0_95()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            double atten = 0.95;
            Link.Mykonos.setTx2Attenuation(atten);
            byte spiData1 = 0x0;
            double readback = 0;

            //Check Tx Attenuation Registers have been Updated
            Console.WriteLine("Test Setup Config Step Size:" + settings.txAttenStepSize.ToString("d"));

            Link.Mykonos.radioOn();

            spiData1 = Link.spiRead(0x96E); Console.WriteLine("Myk: Tx2 Atten Step Size:" + spiData1.ToString("X"));
            Assert.AreEqual(settings.txAttenStepSize, ((spiData1 & 0x30) >> 5), "Myk: Tx Atten StepSize not as expected");
            spiData1 = Link.spiRead(0x962); Console.WriteLine("Myk Addr:Tx2 Atten LSB:" + spiData1.ToString("X"));
            Assert.AreEqual(0x13, spiData1 & 0xFF, "Myk: Tx Attenuation not as expected");
            spiData1 = Link.spiRead(0x963); Console.WriteLine("Myk Addr:Tx2 Atten MSB:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0x03, "Myk: Tx Attenuation not as expected");
            Link.Mykonos.getTx2Attenuation(ref readback); Console.WriteLine("Myk: getTx2Attenuation return value:" + readback.ToString());
            Assert.AreEqual(atten, readback, "Myk: Tx Attenuation readback not as expected");

            Link.Disconnect();

        }
       


        ///<summary>
        /// API Test Name: 
        ///     CheckTxFirFilter
        /// API Under-Test: 
        ///     MYKONOS_programFir	
        /// API Test Description: 
        ///     Call MYKONOS_programFir to set RxFilter to 
        ///     from  TxApp47_245p76_BW200_PriSigBW75.ftr
        /// API Test Pass Criteria: 
        ///     Check RTx Filter Registers are updated. 
        ///     As Expected with the cGain, Taps and Coefficients
        /// Notes:
        /// RXFIR_GAIN =-6dB
        /// TXFIR_TAPS = 16
        /// RXFIR_COEFFS :  -15,    33,   -92,   207, -457, 1094, -3342, 21908,
        ///                -4607,  2226,  -862,  376, -169,    72, -29,     19
        ///</summary>
        [Test, Sequential]
        public static void CheckTxFirFilter([Values(Mykonos.FIR.TX1_FIR, Mykonos.FIR.TX2_FIR)]Mykonos.FIR txFirMode)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte firMSB = 0;
            byte firLSB = 0;
            string txFirFile = settings.resourcePath + @"\DigitalFilters\" + "TxApp47_245p76_BW200_PriSigBW75.ftr";
            Link.Mykonos.programFir(txFirMode, txFirFile);

            //Select PFIR to RX1
            if (txFirMode == Mykonos.FIR.TX1_FIR)
            {
                spiData1 = Link.spiRead(0xDFF);
                Link.spiWrite(0xDFF, (byte)(spiData1 & 0xF1));
            }
            else if (txFirMode == Mykonos.FIR.TX2_FIR)
            {
                spiData1 = Link.spiRead(0xDFF);
                Link.spiWrite(0xDFF, (byte)(spiData1 & 0xF2));
            }
            else
                throw new ArgumentException();


            //Readback Filter data

            //Read Configured TX Gain
            //Read Read Number of Taps
            //Cross reference Coeffecients
            spiData1 = Link.spiRead(0x910); Console.WriteLine("Myk:Tx Filter Gain: " + (spiData1 & 0x03).ToString("X"));
            Assert.AreEqual(0x1, ((spiData1 & 0x01)), "Myk: Tx Filter Gain: not as expected");
            spiData1 = Link.spiRead(0x10); Console.WriteLine("Myk: Tx Filter NumOf Taps: " + (spiData1).ToString("X"));
            Assert.AreEqual(0x00, ((spiData1 & 0xE0) >> 5), "Myk: Tx Filter NumOf Taps not as expected");


            Int16[] coefs = new Int16[16];
            for (int i = 0; i < 16; i++)
            {
                Link.spiWrite(0xE01, (byte)(i * 2));
                firLSB = Link.spiRead(0xE00);
                Link.spiWrite(0xE01, (byte)(i * 2 + 1));
                firMSB = Link.spiRead(0xE00);

                coefs[i] = (Int16)(((UInt16)(firMSB) << 8) | (UInt16)(firLSB));
                Console.WriteLine("Coef[" + i + "]: " + coefs[i]);
                Assert.AreEqual(TxFirCoeffs[i], coefs[i], "Myk: Tx FilterCoeff" + i.ToString("d") + "not as expected");
            }
            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     CheckSetTxNcoEnable
        /// API Under-Test: 
        ///     MYKONOS_enableTxNco	
        /// API Test Description: 
        ///     Call MYKONOS_enableTxNco to set TX_PLL 
        ///     from  60.95            
        /// API Test Pass Criteria: 
        ///     Check TxNCO is enabled via spi register cross reference
        /// 
        ///</summary>
        [Test, Sequential]
        public static void CheckSetTxNcoEnable([Values(6000)]int toneFreq1_kHz,[Values(6000)]int toneFreq2_kHz)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.radioOn();

            Link.Mykonos.enableTxNco(1,toneFreq1_kHz,toneFreq2_kHz);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            Int16 tx1Readback =0x0; 
            Int16 tx2Readback =0x0;
            //Check Tx NCO is eanbled
            spiData1 = Link.spiRead(0xC40); 
            Console.WriteLine("Myk: Tx NCO enable:" + spiData1.ToString("X"));
            Assert.AreEqual(0x80, ((spiData1 & 0x80)), "Myk: Tx NOC enable not set");
          
            //Check Frequency Settings are as expected as per register map description
            Int16 tx1NcoTuneWord = (Int16)(((Int64)(toneFreq1_kHz) << 16) / (settings.txProfileData.IqRate_kHz * -1));
            Int16 tx2NcoTuneWord = (Int16)(((Int64)(toneFreq2_kHz) << 16) / (settings.txProfileData.IqRate_kHz * -1));
            spiData1 = Link.spiRead(0x9CB);
            spiData2 = Link.spiRead(0x9CC);
            tx1Readback = (Int16)((Int16)(spiData1 << 8) | (Int16)(spiData2 )) ;
            Console.Write(tx1Readback.ToString("X"));
            Assert.AreEqual(tx1NcoTuneWord, tx1Readback);
            spiData1 = Link.spiRead(0x9CD);
            spiData2 = Link.spiRead(0x9CE);
            tx2Readback = (Int16)((Int16)(spiData1 << 8) | (Int16)(spiData2));
            Assert.AreEqual(tx2NcoTuneWord, tx2Readback);
            Console.Write(tx2Readback.ToString("X"));
            Link.Disconnect();

        }

   

    }
}
