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
    //TODO:
    //DO RF tests
    //GetDacPower
    //GetProtectionErro
    //ClearErrorFlag
    [TestFixture]
    [Category("ApiFunctional")]
    public class PaProtectionApiFunctionalTests
    {
        public const byte MykonosSpi = 0x1;
        public const byte AD9528Spi = 0x2;
        public static TestSetupConfig settings = new TestSetupConfig();

        /// <summary>
        /// PAProtectionFunctionalTests Test Functionality of PA Protection APIs
        /// Setup Parameters:  Refer to Test Settings
        /// From Locally Stored ARM Firmware     @"..\..\..\..\mykonos_resources\";
        /// From Locally Stored Default Profile  @"..\..\..\..\mykonos_resources\"
        ///     Device Clock: 245.760MHz
        ///     Rx Profile: Rx Profile: As Per Test Fixture Parameter
        ///     Tx Profile: Tx 75/200MHz, IQrate 245.76MHz, Dec5
        ///     Rx Data Paths: RX1 and Rx2 Enabled
        ///     Tx Data Paths: TX1 and TX2 Enabled
        ///     ObsRx Data Paths: ObsRx1 Enabled
        ///     Sniffer Data Path: Disabled
        ///     JESD204B Deframer Settings:
        ///         deviceID = 0;
        ///         laneID = 0;
        ///         bankID = 0;
        ///         M = 4;
        ///         K = 32;
        ///         scramble = 1;
        ///         externalSysref = 1;
        ///         deserializerLanesEnabled = 0x0F;
        ///         deserializerLaneCrossbar = 0xE4;
        ///         eqSetting = 1;
        ///         invertLanePolarity = 0;
        ///         enableAutoChanXbar = 0;
        ///         lmfcOffset = 0;
        ///         newSysrefOnRelink = 0;
        ///     JESD204B Framer Settings:
        ///         bankId = 0;
        ///         deviceId = 0;
        ///         laneId = 0;
        ///         M = 4;
        ///         K = 32;
        ///         scramble = 1;
        ///         externalSysref = 1;
        ///         serializerLanesEnabled = 0x3;
        ///         serializerLaneCrossbar = 0xE4;
        ///         serializerAmplitude = 22;
        ///         preEmphasis = 4;
        ///         invertLanePolarity = 0;
        ///         lmfcOffset = 0;
        ///         obsRxSyncbSelect = 0;
        ///         newSysrefOnRelink = 0;
        ///         overSample = 0;
        ///         enableAutoChanXbar = 0;
        ///     JESD204B ObsRx Framer Settings:
        ///         bankId = 1;
        ///         deviceId = 0;
        ///         laneId = 0;
        ///         M = 2;
        ///         K = 32;
        ///         scramble = 1;
        ///         externalSysref = 1;
        ///         serializerLanesEnabled = 0xC;
        ///         serializerLaneCrossbar = 0xE4;
        ///         serializerAmplitude = 22;
        ///         preEmphasis = 4;
        ///         invertLanePolarity = 0;
        ///         lmfcOffset = 0;
        ///         obsRxSyncbSelect = 1;
        ///         newSysrefOnRelink = 0;
        ///         overSample = 1;
        ///         enableAutoChanXbar = 0;
        /// </summary>
        [SetUp]
        public void PaProtectionTestInit()
        {
            //Configure Calibration Mask
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
            Console.WriteLine("PAProtectionApiFunctional Test Setup: Complete");
        }


        ///<summary>
        /// API Test Name: 
        ///     CheckPaProtectionSetup
        /// API Under-Test: 
        ///     MYKONOS_setupPaProtection	
        /// API Test Description: 
        ///  Use MYKONOS_setupPaProtection To set the PA Protection 
        ///  Parameters. Check the SPI registers for correct configuration
        /// API Test Pass Criteria: 
        ///  Verify the in range configuration Values match the 
        ///  SPI Register Readbacks.
        /// Notes:
        ///</summary>
        ///
        [Test, Sequential]
        public static void CheckPaProtectionSetup([Values(0x1E, 0x2BC)] int pwrThres, [Values(0x7, 0x2F)]byte attenStepSz, [Values(0x5, 0xF)]byte avgDur, [Values(0x0, 0x1)] byte enStickyFlg, [Values(0x0, 0x1)] byte enTxAttnCtrl)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            ushort spiData2 = 0x0;
            ushort temp = 0x0;
            Link.Mykonos.SetupPaProtection((ushort)pwrThres, attenStepSz, avgDur, enStickyFlg, enTxAttnCtrl);

            //Check Sticky Flag Enable
            //Check Avg Duration
            spiData1 = Link.spiRead(0x955); Console.WriteLine("PA Cfg 0x955: " + spiData1.ToString("X"));
            Assert.AreEqual(enStickyFlg, ((spiData1 & 0x40) >> 6), "Myk:PA Sticky Err Flg Enable not as expected");
            Assert.AreEqual(avgDur, ((spiData1 & 0x1e) >> 1), "Myk:PA Avg Duration not as expected");

            //Check Tx Attenuation Step Size
            //Check Tx Attn Ctrl Enable
            spiData1 = Link.spiRead(0x956); Console.WriteLine("PA Cfg 0x956: " + spiData1.ToString("X"));
            Assert.AreEqual(attenStepSz, ((spiData1 & 0xFE) >> 1), "Myk:PA Tx Attenuation Step Size not as expected");
            Assert.AreEqual(enTxAttnCtrl, (spiData1 & 0x1), "Myk:PA Tx Attn Ctrl Enable not as expected");
            
            //Check PA Protection Threshold
            spiData1 = Link.spiRead(0x957); Console.WriteLine("PA Cfg 0x957: " + spiData1.ToString("X"));
            spiData2 = Link.spiRead(0x958); Console.WriteLine("PA Cfg 0x958: " + spiData1.ToString("X"));
            temp =  (ushort)(((spiData2 & 0x1F) << 8) | (ushort) (spiData1));
            Assert.AreEqual(pwrThres, temp, "Myk: PA Protection Threshold not as expected");
            
            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     CheckPaProtectionEnable
        /// API Under-Test: 
        ///     MYKONOS_enablePaProtection	
        /// API Test Description: 
        ///  Call MYKONOS_setupPaProtection to configure
        ///  A valid configuration .
        ///  Call PaProtectionEnable    
        /// API Test Pass Criteria: 
        /// Verify Enable status matches SPI register Readback
        /// Notes:
        ///</summary>
        [Test, Sequential]
        public static void CheckPaProtectionEnable([Values(0x0, 0x1)] byte enPaProtection)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            Link.Mykonos.SetupPaProtection(700, 0x5, 0x7, 0x1, 0x1);
            Link.Mykonos.EnablePaProtection(enPaProtection);
            spiData1 = Link.spiRead(0x955); Console.WriteLine("PA Cfg 0x955: " + spiData1.ToString("X"));
            Assert.AreEqual(enPaProtection, (spiData1 & 0x01), "Myk:PA Protection Enable not as expected");
            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     CheckGetDacPower
        /// API Under-Test: 
        ///     MYKONOS_getDacPower	
        /// API Test Description: 
        ///     Call MYKONOS_getDacPower to 
        ///     read back dac power
        /// API Test Pass Criteria: 
        ///     Verify API readback matches SPI register Readback
        /// Notes:
        ///</summary>
        [Test, Sequential]
        public static void CheckGetDacPower([Values(Mykonos.TXCHANNEL.TX1, Mykonos.TXCHANNEL.TX2)]Mykonos.TXCHANNEL TxCh)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            byte channelPowerMsbMask = 0x03;
            UInt16 dacPower = 0;
            Link.Mykonos.GetDacPower(TxCh, ref dacPower);
            spiData1 = Link.spiRead(0x959); Console.WriteLine("power readback LSB: " + spiData1.ToString("X"));
            spiData2 = Link.spiRead(0x95A); Console.WriteLine("power readback MSB: " + spiData1.ToString("X"));
            Assert.AreEqual(dacPower, (spiData1 | (((UInt16)(spiData2 & channelPowerMsbMask)) << 8)), "Myk:PA Protection readback not as expected");
            Link.Disconnect();
        }
#if false

        ///<summary>
        /// API Test Name: 
        ///     CheckGetPaProtectionError
        /// API Under-Test: 
        ///     MYKONOS_getPaProtectionErrorFlagStatus	
        /// API Test Description: 
        ///
        /// API Test Pass Criteria: 
        ///
        /// Notes:
        ///</summary>
        [Test]
        public static void CheckGetPaProtectionError()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     CheckClearPaProtectionError
        /// API Under-Test: 
        ///     MYKONOS_clearPaErrorFlag	
        /// API Test Description: 
        ///
        /// API Test Pass Criteria: 
        ///
        /// Notes:
        ///</summary>
        [Test]
        public static void CheckClearPaProtectionError()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Disconnect();
        }
#endif 
    }
}
