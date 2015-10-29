using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using NUnit.Framework;
using AdiCmdServerClient;

namespace mykonosUnitTest
{
    class TestSetup
    {
        public const byte MykonosSpi = 0x1;
        public const byte AD9528Spi = 0x2;
        /// <summary>
        /// Defualt Test Setup For Most QA Tests
        /// Setup Parameters:  Defined by TestSetupConfig Settings
        /// </summary>
        public static void TestSetupInit(TestSetupConfig settings)
        {


            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);


            //GetPlatform Info
            Console.Write(Link.hw.Query("GetPcbInfo"));
            Console.Write(Link.identify());
            Console.Write(Link.version());
            UInt32 fpgaVersion = Link.FpgaMykonos.readFpgaVersion();
            Debug.WriteLine("FPGA Version: " + fpgaVersion.ToString("X"));
            UInt32 fpgaDevClock_kHz = Link.FpgaMykonos.readRefClockFrequency();
            Debug.WriteLine("FPGA Device Clock: " + fpgaDevClock_kHz + " kHz");
            Debug.WriteLine("ARM Version :" + Link.Mykonos.getArmVersion());
            //Initialise Platform
            //Configure AD9528 To Provide Reference Clock
            //TODO: should vcxoFrequency_Hz and refclkAFrequency_Hz be made dynamic?
            Link.Mykonos.resetDevice();
            Link.Ad9528.initDeviceDataStructure(122880000, 30720000,
                                    (settings.mykSettings.DeviceClock_kHz * 1000));
            Link.hw.ReceiveTimeout = 0;
            Link.Ad9528.resetDevice();
            Link.Ad9528.initialize();
            System.Threading.Thread.Sleep(500);
            //NOTE: doesn't wait long enough...the LOCK status toggles for a while until stable.
            bool arePllsLocked = Link.Ad9528.waitForPllLock();

            Assert.AreEqual(true, arePllsLocked, "AD9528 PLLs are not locked");
            Link.Ad9528.enableClockOutputs(0x300A);

            //Initialise Mykonos Device
            //First Reset FPGA Link to Mykonos Device
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.resetFPGAIP(0x7);
            Link.FpgaMykonos.resetFPGAIP(0x0);

            MykonosInit(settings);

            // Perform Multi Chip Sync For Platform
            byte mcsStatus = 0;
            MykonosInitMcs(0x1, ref mcsStatus);

            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();

            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);


            System.Threading.Thread.Sleep(100);
            MykonosInitMcs(0x0, ref mcsStatus);
            Assert.AreEqual(0x0B, mcsStatus,
                "Multi Chip Sync failed to complete properly.  MCS Status: " + mcsStatus.ToString("X"));
            Debug.WriteLine("MCS Status: " + mcsStatus.ToString("X"));

            //Load Mykonos Device Firmware
            MykonosInitArm(settings);

            //Set Desired Pll LO Freq For Rx, Tx and Sniffer
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.RX_PLL, settings.rxPllLoFreq_Hz);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, settings.txPllLoFreq_Hz);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.SNIFFER_PLL, settings.obsRxPllLoFreq_Hz);

            System.Threading.Thread.Sleep(200); //wait 200ms for PLLs to lock

            //Run Init Cals Default Cal value in TestSettings
            MykonosInitCals(settings);


            //Enable Mkyonos Rx-FPGA JESD204B Link
            MykonosConfigFPGAJESD(settings);
            FPGADisableDeFramer();
            System.Threading.Thread.Sleep(100);
            MykonosEnRxFramerLink();
            FPGAEnableDeFramer();
            System.Threading.Thread.Sleep(100);
            EvbReqSync();

            //Enable Mkyonos Rx-FPGA JESD204B Link
            FPGADisableDeFramer();
            System.Threading.Thread.Sleep(100);
            MykonosEnRxFramerLink();
            FPGAEnableDeFramer();
            System.Threading.Thread.Sleep(100);

            //Check Mykonos Rx-FPGA JESD204B Link
            UInt32 fpgaReg = 0;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            fpgaReg = Link.fpgaRead(0x400);
            Debug.WriteLine("FPGA Reg x400 = " + fpgaReg.ToString("X"));


            //Enable Mkyonos Tx-FPGA JESD204B Link
            Link.FpgaMykonos.resetFPGAIP(0x01);


            //Disable FPGA Tx framer
            //Allow FPGA Plls to power up.
            //Reset Mykonos Tx DeFramer
            //Enable FPGA Framer 
            //Enable Mykonos Tx DeFamer
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.enableJesd204bFramer(false);
            Link.FpgaMykonos.resetFPGAIP(0x00);
            System.Threading.Thread.Sleep(10);
            MykonosResetTxDeFramerLink();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.enableJesd204bFramer(true);
            MykonosEnTxDeFramerLink();
            EvbReqSync();

            Link.Disconnect();

        }
        /// <summary>
        /// DPD Test Specific Setup
        /// Setup Parameters:  Defined by TestSetupConfig Settings
        /// </summary>
        public static void DpdTestSetupInit(TestSetupConfig settings)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);


            //GetPlatform Info
            Console.Write(Link.hw.Query("GetPcbInfo"));
            Console.Write(Link.identify());
            Console.Write(Link.version());
            UInt32 fpgaVersion = Link.FpgaMykonos.readFpgaVersion();
            Debug.WriteLine("FPGA Version: " + fpgaVersion.ToString("X"));
            UInt32 fpgaDevClock_kHz = Link.FpgaMykonos.readRefClockFrequency();
            Debug.WriteLine("FPGA Device Clock: " + fpgaDevClock_kHz + " kHz");
            Debug.WriteLine("ARM Version :" + Link.Mykonos.getArmVersion());
            //Initialise Platform
            //Configure AD9528 To Provide Reference Clock
            //TODO: should vcxoFrequency_Hz and refclkAFrequency_Hz be made dynamic?
            Link.Mykonos.resetDevice();
            Link.Ad9528.initDeviceDataStructure(122880000, 30720000,
                                    (settings.mykSettings.DeviceClock_kHz * 1000));
            Link.hw.ReceiveTimeout = 0;
            Link.Ad9528.resetDevice();
            Link.Ad9528.initialize();
            System.Threading.Thread.Sleep(500);
            //NOTE: doesn't wait long enough...the LOCK status toggles for a while until stable.
            bool arePllsLocked = Link.Ad9528.waitForPllLock();

            Assert.AreEqual(true, arePllsLocked, "AD9528 PLLs are not locked");
            Link.Ad9528.enableClockOutputs(0x300A);

            //Initialise Mykonos Device As DPD Settings
            //First Reset FPGA Link to Mykonos Device
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.resetFPGAIP(0x7);
            Link.FpgaMykonos.resetFPGAIP(0x0);
            MykonosInitDpd(settings);


            // Perform Multi Chip Sync For Platform
            byte mcsStatus = 0;
            MykonosInitMcs(0x1, ref mcsStatus);

            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();

            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);


            System.Threading.Thread.Sleep(100);
            MykonosInitMcs(0x0, ref mcsStatus);
            Assert.AreEqual(0x0B, mcsStatus,
                "Multi Chip Sync failed to complete properly.  MCS Status: " + mcsStatus.ToString("X"));
            Debug.WriteLine("MCS Status: " + mcsStatus.ToString("X"));

            //Load Mykonos Device Firmware
            MykonosInitArm(settings);

            //Set Desired Pll LO Freq For Rx, Tx and Sniffer
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.RX_PLL, settings.rxPllLoFreq_Hz);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, settings.txPllLoFreq_Hz);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.SNIFFER_PLL, settings.obsRxPllLoFreq_Hz);

            System.Threading.Thread.Sleep(200); //wait 200ms for PLLs to lock

            //Call DPD Config
            Link.Mykonos.configDpd();
            Link.Mykonos.configClgc();

            //Run Init Cals
            MykonosInitCals(settings);


            //Enable Mkyonos Rx-FPGA JESD204B Link
            MykonosConfigFPGAJESD(settings);
            FPGADisableDeFramer();
            System.Threading.Thread.Sleep(100);
            MykonosEnRxFramerLink();
            FPGAEnableDeFramer();
            System.Threading.Thread.Sleep(100);
            EvbReqSync();

            //Enable Mkyonos Rx-FPGA JESD204B Link
            FPGADisableDeFramer();
            System.Threading.Thread.Sleep(100);
            MykonosEnRxFramerLink();
            FPGAEnableDeFramer();
            System.Threading.Thread.Sleep(100);

            //Check Mykonos Rx-FPGA JESD204B Link
            UInt32 fpgaReg = 0;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            fpgaReg = Link.fpgaRead(0x400);
            Debug.WriteLine("FPGA Reg x400 = " + fpgaReg.ToString("X"));


            //Enable Mkyonos Tx-FPGA JESD204B Link
            Link.FpgaMykonos.resetFPGAIP(0x01);


            //Disable FPGA Tx framer
            //Allow FPGA Plls to power up.
            //Reset Mykonos Tx DeFramer
            //Enable FPGA Framer 
            //Enable Mykonos Tx DeFamer
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.enableJesd204bFramer(false);
            Link.FpgaMykonos.resetFPGAIP(0x00);
            System.Threading.Thread.Sleep(10);
            MykonosResetTxDeFramerLink();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.enableJesd204bFramer(true);
            MykonosEnTxDeFramerLink();
            EvbReqSync();

            Link.Disconnect();

        }
        /// <summary>
        /// ARM Tests Specific Test Setup.
        /// Setup Parameters:  Defined by TestSetupConfig Settings
        /// </summary>
        public static void ArmTestSetupInit(TestSetupConfig settings)
        {


            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);


            //GetPlatform Info
            Console.Write(Link.hw.Query("GetPcbInfo"));
            Console.Write(Link.identify());
            Console.Write(Link.version());
            UInt32 fpgaVersion = Link.FpgaMykonos.readFpgaVersion();
            Console.WriteLine("FPGA Version: " + fpgaVersion.ToString("X"));
            UInt32 fpgaDevClock_kHz = Link.FpgaMykonos.readRefClockFrequency();
            Console.WriteLine("FPGA Device Clock: " + fpgaDevClock_kHz + " kHz");
            Debug.WriteLine("ARM Version :" + Link.Mykonos.getArmVersion());
            //Initialise Platform
            //Configure AD9528 To Provide Reference Clock
            //TODO: should vcxoFrequency_Hz and refclkAFrequency_Hz be made dynamic?
            Link.Mykonos.resetDevice();
            Link.Ad9528.initDeviceDataStructure(122880000, 30720000, (settings.mykSettings.DeviceClock_kHz * 1000));
            Link.hw.ReceiveTimeout = 0;
            Link.Ad9528.resetDevice();
            Link.Ad9528.initialize();
            System.Threading.Thread.Sleep(500);
            //NOTE: doesn't wait long enough...the LOCK status toggles for a while until stable.
            bool arePllsLocked = Link.Ad9528.waitForPllLock();
            Link.Ad9528.enableClockOutputs(0x300A);

            //Initialise Mykonos Device
            //First Reset FPGA Link to Mykonos Device
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.resetFPGAIP(0x7);
            Link.FpgaMykonos.resetFPGAIP(0x0);
            MykonosInit(settings);

            // Perform Multi Chip Sync For Platform
            byte mcsStatus = 0;
            MykonosInitMcs(0x1, ref mcsStatus);

            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();

            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);


            System.Threading.Thread.Sleep(100);
            MykonosInitMcs(0x0, ref mcsStatus);
            Assert.AreEqual(0x0B, mcsStatus,
                "Multi Chip Sync failed to complete properly.  MCS Status: " + mcsStatus.ToString("X"));
            Debug.WriteLine("MCS Status: " + mcsStatus.ToString("X"));

            //Load Mykonos Device Firmware
            MykonosInitArm(settings);

            //Set Desired Pll LO Freq For Rx, Tx and Sniffer
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.RX_PLL, settings.rxPllLoFreq_Hz);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, settings.txPllLoFreq_Hz);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.SNIFFER_PLL, settings.obsRxPllLoFreq_Hz);

            System.Threading.Thread.Sleep(200); //wait 200ms for PLLs to lock



#if false
            //Run Init Cals
            MykonosInitCals(settings);
            //Enable Mkyonos Rx-FPGA JESD204B Link
            MykonosConfigFPGAJESD(settings, rxProfile, txProfile, obsRxProfile, srxProfile);
            FPGADisableDeFramer();
            System.Threading.Thread.Sleep(100);
            MykonosEnRxFramerLink();
            FPGAEnableDeFramer();
            System.Threading.Thread.Sleep(100);
            EvbReqSync();

            //Enable Mkyonos Rx-FPGA JESD204B Link
            FPGADisableDeFramer();
            System.Threading.Thread.Sleep(100);
            MykonosEnRxFramerLink();
            FPGAEnableDeFramer();
            System.Threading.Thread.Sleep(100);

            //Check Mykonos Rx-FPGA JESD204B Link
            UInt32 fpgaReg = 0;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            fpgaReg = Link.fpgaRead(0x400);
            Debug.WriteLine("FPGA Reg x400 = " + fpgaReg.ToString("X"));


            //Enable Mkyonos Tx-FPGA JESD204B Link
            Link.FpgaMykonos.resetFPGAIP(0x01);


            //Disable FPGA Tx framer
            //Allow FPGA Plls to power up.
            //Reset Mykonos Tx DeFramer
            //Enable FPGA Framer 
            //Enable Mykonos Tx DeFamer
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.enableJesd204bFramer(false);
            Link.FpgaMykonos.resetFPGAIP(0x00);
            System.Threading.Thread.Sleep(10);
            MykonosResetTxDeFramerLink();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.enableJesd204bFramer(true);
            MykonosEnTxDeFramerLink();
            EvbReqSync();
#endif
            Link.Disconnect();

        }
        /// <summary>
        /// Profile Tests Specific Setup 
        /// Setup Parameters:  Defined by TestSetupConfig Settings
        /// </summary>
        public static void AdcProfileTestSetupInit(TestSetupConfig settings, bool TxValid, bool RxValid)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            //GetPlatform Info
            Console.Write(Link.hw.Query("GetPcbInfo"));
            Console.Write(Link.identify());
            Console.Write(Link.version());
            UInt32 fpgaVersion = Link.FpgaMykonos.readFpgaVersion();
            Debug.WriteLine("FPGA Version: " + fpgaVersion.ToString("X"));
            UInt32 fpgaDevClock_kHz = Link.FpgaMykonos.readRefClockFrequency();
            Debug.WriteLine("FPGA Device Clock: " + fpgaDevClock_kHz + " kHz");
            Debug.WriteLine("ARM Version :" + Link.Mykonos.getArmVersion());
            //Initialise Platform
            //Configure AD9528 To Provide Reference Clock
            //TODO: should vcxoFrequency_Hz and refclkAFrequency_Hz be made dynamic?
            Link.Mykonos.resetDevice();
            Link.Ad9528.initDeviceDataStructure(122880000, 30720000,
                                    (settings.mykSettings.DeviceClock_kHz * 1000));
            Link.hw.ReceiveTimeout = 0;
            Link.Ad9528.resetDevice();
            Link.Ad9528.initialize();
            System.Threading.Thread.Sleep(500);
            //NOTE: doesn't wait long enough...the LOCK status toggles for a while until stable.
            bool arePllsLocked = Link.Ad9528.waitForPllLock();

            Assert.AreEqual(true, arePllsLocked, "AD9528 PLLs are not locked");
            Link.Ad9528.enableClockOutputs(0x300A);

            //Initialise Mykonos Device
            //First Reset FPGA Link to Mykonos Device
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.resetFPGAIP(0x7);
            Link.FpgaMykonos.resetFPGAIP(0x0);

            if (TxValid && RxValid)
                MykonosInit(settings);
            else if (TxValid && !RxValid)
                MykonosInitNoRxProfile(settings);
            else
                MykonosInitInvalidTxProfile(settings);

            // Perform Multi Chip Sync For Platform
            byte mcsStatus = 0;
            MykonosInitMcs(0x1, ref mcsStatus);

            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();

            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);


            System.Threading.Thread.Sleep(100);
            MykonosInitMcs(0x0, ref mcsStatus);
            Assert.AreEqual(0x0B, mcsStatus,
                "Multi Chip Sync failed to complete properly.  MCS Status: " + mcsStatus.ToString("X"));
            Debug.WriteLine("MCS Status: " + mcsStatus.ToString("X"));

            //Load Mykonos Device Firmware
            MykonosInitArm(settings);

            //Set Desired Pll LO Freq For Rx, Tx and Sniffer
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            //Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.RX_PLL, settings.rxPllLoFreq_Hz);
            //Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, settings.txPllLoFreq_Hz);
           // Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.SNIFFER_PLL, settings.obsRxPllLoFreq_Hz);

            System.Threading.Thread.Sleep(200); //wait 200ms for PLLs to lock

            Link.Disconnect();

        }
        /// <summary>
        /// JESD Tests Specific Setup 
        /// Setup Parameters:  Defined by TestSetupConfig Settings
        /// </summary>
        public static void JESDTestSetupInit(TestSetupConfig settings, Mykonos.RXCHANNEL rxChannels, Mykonos.TXCHANNEL txChannels, byte Obsrxchannels)
        {

            MykonosProfileData rxProfile = new MykonosProfileData();
            MykonosProfileData txProfile = new MykonosProfileData();
            MykonosProfileData obsRxProfile = new MykonosProfileData();
            MykonosProfileData srxProfile = new MykonosProfileData();
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);


            //GetPlatform Info
            Console.Write(Link.hw.Query("GetPcbInfo"));
            Console.Write(Link.identify());
            Console.Write(Link.version());
            UInt32 fpgaVersion = Link.FpgaMykonos.readFpgaVersion();
            Debug.WriteLine("FPGA Version: " + fpgaVersion.ToString("X"));
            UInt32 fpgaDevClock_kHz = Link.FpgaMykonos.readRefClockFrequency();
            Debug.WriteLine("FPGA Device Clock: " + fpgaDevClock_kHz + " kHz");
            Debug.WriteLine("ARM Version :" + Link.Mykonos.getArmVersion());
            //Initialise Platform
            //Configure AD9528 To Provide Reference Clock
            //TODO: should vcxoFrequency_Hz and refclkAFrequency_Hz be made dynamic?
            Link.Mykonos.resetDevice();
            Link.Ad9528.initDeviceDataStructure(122880000, 30720000,
                                    (settings.mykSettings.DeviceClock_kHz * 1000));
            Link.hw.ReceiveTimeout = 0;
            Link.Ad9528.resetDevice();
            Link.Ad9528.initialize();
            System.Threading.Thread.Sleep(500);
            //NOTE: doesn't wait long enough...the LOCK status toggles for a while until stable.
            bool arePllsLocked = Link.Ad9528.waitForPllLock();
            Link.Ad9528.enableClockOutputs(0x300A);

            //Initialise Mykonos Device
            //First Reset FPGA Link to Mykonos Device
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.resetFPGAIP(0x7);
            Link.FpgaMykonos.resetFPGAIP(0x0);
            JESDMykonosInit(settings, rxChannels, txChannels, Obsrxchannels);

            // Perform Multi Chip Sync For Platform
            byte mcsStatus = 0;
            MykonosInitMcs(0x1, ref mcsStatus);

            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();

            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);


            System.Threading.Thread.Sleep(100);
            MykonosInitMcs(0x0, ref mcsStatus);
            Assert.AreEqual(0x0B, mcsStatus,
                "Multi Chip Sync failed to complete properly.  MCS Status: " + mcsStatus.ToString("X"));
            Debug.WriteLine("MCS Status: " + mcsStatus.ToString("X"));

            //Load Mykonos Device Firmware
            JESDMykonosInitArm(settings);

            //Set Desired Pll LO Freq For Rx, Tx and Sniffer
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.RX_PLL, settings.rxPllLoFreq_Hz);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, settings.rxPllLoFreq_Hz);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.SNIFFER_PLL, settings.obsRxPllLoFreq_Hz);

            System.Threading.Thread.Sleep(200); //wait 200ms for PLLs to lock

            //Run Init Cals
            //            MykonosInitCals(settings);

            //Enable Mkyonos Rx-FPGA JESD204B Link
            MykonosConfigFPGAJESD(settings);
            FPGADisableDeFramer();
            System.Threading.Thread.Sleep(100);
            MykonosEnRxFramerLink();
            FPGAEnableDeFramer();
            System.Threading.Thread.Sleep(100);
            EvbReqSync();

            //Enable Mkyonos Rx-FPGA JESD204B Link
            FPGADisableDeFramer();
            System.Threading.Thread.Sleep(100);
            MykonosEnRxFramerLink();
            FPGAEnableDeFramer();
            System.Threading.Thread.Sleep(100);

            //Check Mykonos Rx-FPGA JESD204B Link
            UInt32 fpgaReg = 0;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            fpgaReg = Link.fpgaRead(0x400);
            Debug.WriteLine("FPGA Reg x400 = " + fpgaReg.ToString("X"));


            //Enable Mkyonos Tx-FPGA JESD204B Link
            Link.FpgaMykonos.resetFPGAIP(0x01);


            //Disable FPGA Tx framer
            //Allow FPGA Plls to power up.
            //Reset Mykonos Tx DeFramer
            //Enable FPGA Framer 
            //Enable Mykonos Tx DeFamer
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.enableJesd204bFramer(false);
            Link.FpgaMykonos.resetFPGAIP(0x00);
            System.Threading.Thread.Sleep(10);
            MykonosResetTxDeFramerLink();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.enableJesd204bFramer(true);
            MykonosEnTxDeFramerLink();
            EvbReqSync();

            Link.Disconnect();

        }
        /// <summary>
        /// PRBS Rx Tests Setup
        /// Setup Parameters:  Defined by TestSetupConfig Settings
        /// </summary>
        public static void PrbsRxTestSetupInit(TestSetupConfig settings)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);


            //GetPlatform Info
            Console.Write(Link.hw.Query("GetPcbInfo"));
            Console.Write(Link.identify());
            Console.Write(Link.version());
            UInt32 fpgaVersion = Link.FpgaMykonos.readFpgaVersion();
            Debug.WriteLine("FPGA Version: " + fpgaVersion.ToString("X"));
            UInt32 fpgaDevClock_kHz = Link.FpgaMykonos.readRefClockFrequency();
            Debug.WriteLine("FPGA Device Clock: " + fpgaDevClock_kHz + " kHz");
            Debug.WriteLine("ARM Version :" + Link.Mykonos.getArmVersion());
            //Initialise Platform
            //Configure AD9528 To Provide Reference Clock
            //TODO: should vcxoFrequency_Hz and refclkAFrequency_Hz be made dynamic?
            Link.Mykonos.resetDevice();
            Link.Ad9528.initDeviceDataStructure(122880000, 30720000,
                                    (settings.mykSettings.DeviceClock_kHz * 1000));
            Link.hw.ReceiveTimeout = 0;
            Link.Ad9528.resetDevice();
            Link.Ad9528.initialize();
            System.Threading.Thread.Sleep(500);
            //NOTE: doesn't wait long enough...the LOCK status toggles for a while until stable.
            bool arePllsLocked = Link.Ad9528.waitForPllLock();

            Assert.AreEqual(true, arePllsLocked, "AD9528 PLLs are not locked");
            Link.Ad9528.enableClockOutputs(0x300A);

            //Initialise Mykonos Device
            //First Reset FPGA Link to Mykonos Device
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.resetFPGAIP(0x7);
            Link.FpgaMykonos.resetFPGAIP(0x0);

            MykonosInit(settings);

            // Perform Multi Chip Sync For Platform
            byte mcsStatus = 0;
            MykonosInitMcs(0x1, ref mcsStatus);

            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();

            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);


            System.Threading.Thread.Sleep(100);
            MykonosInitMcs(0x0, ref mcsStatus);
            Assert.AreEqual(0x0B, mcsStatus,
                "Multi Chip Sync failed to complete properly.  MCS Status: " + mcsStatus.ToString("X"));
            Debug.WriteLine("MCS Status: " + mcsStatus.ToString("X"));

            //Load Mykonos Device Firmware
            MykonosInitArm(settings);

            //Set Desired Pll LO Freq For Rx, Tx and Sniffer
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.RX_PLL, settings.rxPllLoFreq_Hz);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, settings.txPllLoFreq_Hz);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.SNIFFER_PLL, settings.obsRxPllLoFreq_Hz);

            System.Threading.Thread.Sleep(200); //wait 200ms for PLLs to lock

            //Run Init Cals
            MykonosInitCals(settings);


            //Enable Mkyonos Rx-FPGA JESD204B Link
            MykonosConfigFPGAJESD(settings);
            FPGADisableDeFramer();
            System.Threading.Thread.Sleep(100);
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.enableSysrefToRxFramer(1);
            FPGAEnableDeFramer();
            System.Threading.Thread.Sleep(100);
            EvbReqSync();

            //Enable Mkyonos Rx-FPGA JESD204B Link
            FPGADisableDeFramer();
            System.Threading.Thread.Sleep(100);
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.enableSysrefToRxFramer(1);
            FPGAEnableDeFramer();
            System.Threading.Thread.Sleep(100);

            //Check Mykonos Rx-FPGA JESD204B Link
            UInt32 fpgaReg = 0;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            fpgaReg = Link.fpgaRead(0x400);
            Debug.WriteLine("FPGA Reg x400 = " + fpgaReg.ToString("X"));


            //Enable Mkyonos Tx-FPGA JESD204B Link
            Link.FpgaMykonos.resetFPGAIP(0x01);


            //Disable FPGA Tx framer
            //Allow FPGA Plls to power up.
            //Reset Mykonos Tx DeFramer
            //Enable FPGA Framer 
            //Enable Mykonos Tx DeFamer
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.enableJesd204bFramer(false);
            Link.FpgaMykonos.resetFPGAIP(0x00);
            System.Threading.Thread.Sleep(10);
            MykonosResetTxDeFramerLink();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.enableJesd204bFramer(true);
            MykonosEnTxDeFramerLink();
            EvbReqSync();

            Link.Disconnect();

        }
        /// <summary>
        /// PRBS ORx Tests Setup
        /// Setup Parameters:  Defined by TestSetupConfig Settings
        /// </summary>
        public static void PrbsORxTestSetupInit(TestSetupConfig settings)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);


            //GetPlatform Info
            Console.Write(Link.hw.Query("GetPcbInfo"));
            Console.Write(Link.identify());
            Console.Write(Link.version());
            UInt32 fpgaVersion = Link.FpgaMykonos.readFpgaVersion();
            Debug.WriteLine("FPGA Version: " + fpgaVersion.ToString("X"));
            UInt32 fpgaDevClock_kHz = Link.FpgaMykonos.readRefClockFrequency();
            Debug.WriteLine("FPGA Device Clock: " + fpgaDevClock_kHz + " kHz");
             Debug.WriteLine("ARM Version :" + Link.Mykonos.getArmVersion());
            //Initialise Platform
            //Configure AD9528 To Provide Reference Clock
            //TODO: should vcxoFrequency_Hz and refclkAFrequency_Hz be made dynamic?
            Link.Mykonos.resetDevice();
            Link.Ad9528.initDeviceDataStructure(122880000, 30720000,
                                    (settings.mykSettings.DeviceClock_kHz * 1000));
            Link.hw.ReceiveTimeout = 0;
            Link.Ad9528.resetDevice();
            Link.Ad9528.initialize();
            System.Threading.Thread.Sleep(500);
            //NOTE: doesn't wait long enough...the LOCK status toggles for a while until stable.
            bool arePllsLocked = Link.Ad9528.waitForPllLock();

            Assert.AreEqual(true, arePllsLocked, "AD9528 PLLs are not locked");
            Link.Ad9528.enableClockOutputs(0x300A);

            //Initialise Mykonos Device
            //First Reset FPGA Link to Mykonos Device
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.resetFPGAIP(0x7);
            Link.FpgaMykonos.resetFPGAIP(0x0);

            MykonosInit(settings);

            // Perform Multi Chip Sync For Platform
            byte mcsStatus = 0;
            MykonosInitMcs(0x1, ref mcsStatus);

            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();
            Link.FpgaMykonos.requestSysref();

            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);


            System.Threading.Thread.Sleep(100);
            MykonosInitMcs(0x0, ref mcsStatus);
            Assert.AreEqual(0x0B, mcsStatus,
                "Multi Chip Sync failed to complete properly.  MCS Status: " + mcsStatus.ToString("X"));
            Debug.WriteLine("MCS Status: " + mcsStatus.ToString("X"));

            //Load Mykonos Device Firmware
            MykonosInitArm(settings);

            //Set Desired Pll LO Freq For Rx, Tx and Sniffer
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.RX_PLL, settings.rxPllLoFreq_Hz);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, settings.txPllLoFreq_Hz);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.SNIFFER_PLL, settings.obsRxPllLoFreq_Hz);

            System.Threading.Thread.Sleep(200); //wait 200ms for PLLs to lock

            //Run Init Cals
            MykonosInitCals(settings);


            //Enable Mkyonos Rx-FPGA JESD204B Link
            MykonosConfigFPGAJESD(settings);
            FPGADisableDeFramer();
            System.Threading.Thread.Sleep(100);
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.enableSysrefToObsRxFramer(1);
            FPGAEnableDeFramer();
            System.Threading.Thread.Sleep(100);
            EvbReqSync();

            //Enable Mkyonos Rx-FPGA JESD204B Link
            FPGADisableDeFramer();
            System.Threading.Thread.Sleep(100);
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.enableSysrefToObsRxFramer(1);
            FPGAEnableDeFramer();
            System.Threading.Thread.Sleep(100);

            //Check Mykonos Rx-FPGA JESD204B Link
            UInt32 fpgaReg = 0;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            fpgaReg = Link.fpgaRead(0x400);
            Debug.WriteLine("FPGA Reg x400 = " + fpgaReg.ToString("X"));


            //Enable Mkyonos Tx-FPGA JESD204B Link
            Link.FpgaMykonos.resetFPGAIP(0x01);


            //Disable FPGA Tx framer
            //Allow FPGA Plls to power up.
            //Reset Mykonos Tx DeFramer
            //Enable FPGA Framer 
            //Enable Mykonos Tx DeFamer
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.enableJesd204bFramer(false);
            Link.FpgaMykonos.resetFPGAIP(0x00);
            System.Threading.Thread.Sleep(10);
            MykonosResetTxDeFramerLink();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.enableJesd204bFramer(true);
            MykonosEnTxDeFramerLink();
            EvbReqSync();

            Link.Disconnect();

        }

        //Standard Functions for Initialising Mykonos Blocks
        public static void MykonosInit(TestSetupConfig settings)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            /* Prepare Mykonos Device Data structure Settings*/
            string resourcePath = settings.resourcePath;
            Console.Write(resourcePath);
            Console.Write("Clk Freq before:" + settings.rxProfileData.ClkPllFrequency_kHz);
            settings.rxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.rxProfileName, true,
                                                 settings.mykSettings.DeviceClock_kHz);
            Console.Write("Clk Freq After Rx Profile:" + settings.rxProfileData.ClkPllFrequency_kHz);
            settings.txProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.txProfileName,
                                                 false, settings.mykSettings.DeviceClock_kHz);
            Console.Write("Clk Freq After Tx Profile:" + settings.txProfileData.ClkPllFrequency_kHz);
            settings.obsRxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.orxProfileName, false,
                                                 settings.mykSettings.DeviceClock_kHz);
            Console.Write("Clk Freq After Orx Profile:" + settings.txProfileData.ClkPllFrequency_kHz);
            settings.srxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.srxProfileName, false,
                                                 settings.mykSettings.DeviceClock_kHz);
            Console.Write("Clk Freq After Sniffer Profile:" + settings.txProfileData.ClkPllFrequency_kHz);


            Link.Mykonos.init_rxsettings(settings.mykSettings.rxChannel, settings.rxPllUseExtLo,
                                            settings.rxPllLoFreq_Hz, settings.rxUseRealIfData);
            Link.Mykonos.init_txsettings((Mykonos.TXATTENSTEPSIZE)settings.txAttenStepSize,
                                            settings.mykSettings.txChannel, settings.txPllUseExtLo,
                                            settings.txPllLoFreq_Hz, settings.tx1Atten, settings.tx2Atten);
            Link.Mykonos.init_obsrxsettings((byte)settings.mykSettings.orxChannel, Mykonos.OBSRX_LO_SOURCE.OBSLO_TX_PLL,
                                             settings.obsRxPllLoFreq_Hz, 0, Mykonos.OBSRXCHANNEL.OBS_RXOFF);

            Link.Mykonos.init_jesd204bdeframer(settings.mykTxDeFrmrCfg.deviceID, settings.mykTxDeFrmrCfg.laneID,
                                                settings.mykTxDeFrmrCfg.bankID, settings.mykTxDeFrmrCfg.M,
                                                settings.mykTxDeFrmrCfg.K, settings.mykTxDeFrmrCfg.scramble,
                                                settings.mykTxDeFrmrCfg.externalSysref,
                                                settings.mykTxDeFrmrCfg.deserializerLanesEnabled,
                                                settings.mykTxDeFrmrCfg.deserializerLaneCrossbar,
                                                settings.mykTxDeFrmrCfg.eqSetting,
                                                settings.mykTxDeFrmrCfg.invertLanePolarity,
                                                settings.mykTxDeFrmrCfg.enableAutoChanXbar,
                                                settings.mykTxDeFrmrCfg.lmfcOffset,
                                                settings.mykTxDeFrmrCfg.newSysrefOnRelink);

            Link.Mykonos.init_jesd204bframer(settings.mykRxFrmrCfg.bankId, settings.mykRxFrmrCfg.deviceId,
                                                settings.mykRxFrmrCfg.laneId, settings.mykRxFrmrCfg.M,
                                                settings.mykRxFrmrCfg.K, settings.mykRxFrmrCfg.scramble,
                                                settings.mykRxFrmrCfg.externalSysref,
                                                settings.mykRxFrmrCfg.serializerLanesEnabled,
                                                settings.mykRxFrmrCfg.serializerLaneCrossbar,
                                                settings.mykRxFrmrCfg.serializerAmplitude,
                                                settings.mykRxFrmrCfg.preEmphasis,
                                                settings.mykRxFrmrCfg.invertLanePolarity,
                                                settings.mykRxFrmrCfg.lmfcOffset,
                                                settings.mykRxFrmrCfg.newSysrefOnRelink,
                                                settings.mykRxFrmrCfg.enableAutoChanXbar,
                                                settings.mykRxFrmrCfg.obsRxSyncbSelect,
                                                settings.mykRxFrmrCfg.overSample);

            Link.Mykonos.init_obsRxFramer(settings.mykObsRxFrmrCfg.bankId, settings.mykObsRxFrmrCfg.deviceId,
                                            settings.mykObsRxFrmrCfg.laneId, settings.mykObsRxFrmrCfg.M,
                                            settings.mykObsRxFrmrCfg.K, settings.mykObsRxFrmrCfg.scramble,
                                            settings.mykObsRxFrmrCfg.externalSysref,
                                            settings.mykObsRxFrmrCfg.serializerLanesEnabled,
                                            settings.mykObsRxFrmrCfg.serializerLaneCrossbar,
                                            settings.mykObsRxFrmrCfg.serializerAmplitude,
                                            settings.mykObsRxFrmrCfg.preEmphasis,
                                            settings.mykObsRxFrmrCfg.invertLanePolarity,
                                            settings.mykObsRxFrmrCfg.lmfcOffset,
                                            settings.mykObsRxFrmrCfg.newSysrefOnRelink,
                                            settings.mykObsRxFrmrCfg.enableAutoChanXbar,
                                            settings.mykObsRxFrmrCfg.obsRxSyncbSelect,
                                            settings.mykObsRxFrmrCfg.overSample);

            /* Prepare Mykonos Device Data structure Settings*/
            Link.Mykonos.resetDevice();
            Link.Mykonos.initialize();

        }
        public static void MykonosInitMcs(byte enable, ref byte mcsStatus)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.enableMultichipSync(enable, ref mcsStatus);
            Debug.WriteLine("MCS Status: " + mcsStatus.ToString("X"));

        }
        public static void MykonosInitArm(TestSetupConfig settings)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.initArm();
            String fileName = "";

            fileName = settings.resourcePath + @"ArmBinaries\" + settings.mykSettings.ArmFirmwareName;

            var watch = Stopwatch.StartNew();
            Link.Mykonos.loadArm(fileName);
            watch.Stop();
            Debug.WriteLine("Time to load ARM firmware(ms): " + watch.ElapsedMilliseconds);

        }
        public static void MykonosInitCals(TestSetupConfig settings)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.setSpiChannel(MykonosSpi);

            Console.WriteLine("InitArmCal(0x" + settings.calMask.ToString("X") + ")");
            byte errFlag = 0;
            byte errCode = 0;
            Console.WriteLine("SPIRead x119: " + Link.spiRead(0x119).ToString("X"));

            Link.Mykonos.runInitCals(settings.calMask);
            try
            {
                Link.hw.ReceiveTimeout = 0;
                Link.Mykonos.waitInitCals(60000, ref errFlag, ref errCode);
                Link.hw.ReceiveTimeout = 5000;
            }
            catch
            {
                Console.WriteLine("ARM Init Cal: errorFlag=" + errFlag.ToString("X") + " errorCode=" + errCode.ToString("X"));

                UInt32 errorCode = 0;
                try
                {
                    Link.Mykonos.abortInitCals(5000, ref errorCode);
                }
                catch
                {
                    Console.WriteLine("Aborting Init Cals: Error code: 0x" + errorCode.ToString("X"));
                    Console.WriteLine("xD34: " + Link.spiRead(0xD34).ToString("X"));
                    Assert.Fail();
                }
            }


        }
        public static void MykonosConfigFPGAJESD(TestSetupConfig settings)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.FpgaMykonos.setupJesd204(settings.mykSettings.DeviceClock_kHz,
                                            settings.rxProfileData.IqRate_kHz, settings.mykRxFrmrCfg.M, settings.mykRxFrmrCfg.serializerLanesEnabled,
                                            settings.mykRxFrmrCfg.K, settings.mykRxFrmrCfg.scramble,
                                            settings.txProfileData.IqRate_kHz, settings.mykTxDeFrmrCfg.M, settings.mykTxDeFrmrCfg.deserializerLanesEnabled,
                                            settings.mykTxDeFrmrCfg.K, settings.mykTxDeFrmrCfg.scramble,
                                            settings.obsRxProfileData.IqRate_kHz,
                                            settings.srxProfileData.IqRate_kHz, settings.mykObsRxFrmrCfg.serializerLanesEnabled, settings.mykObsRxFrmrCfg.K,
                                            settings.mykObsRxFrmrCfg.scramble);

            UInt32 fpgaReg = Link.fpgaRead(0x424); Console.WriteLine("FPGA Reg x424 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x418); Console.WriteLine("FPGA Reg x418 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x40C); Console.WriteLine("FPGA Reg x40C = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x4C0); Console.WriteLine("FPGA Reg x4C0 = " + fpgaReg.ToString("X"));

            Link.FpgaMykonos.setupJesd204bObsRxOversampler(FpgaMykonos.FPGA_SAMPLE_DECIMATION.DECIMATE_BY_0, 0);
        }
        public static void MykonosEnRxFramerLink()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.enableSysrefToRxFramer(1);
            Link.Mykonos.enableSysrefToObsRxFramer(1);

            Link.Disconnect();
        }
        public static void MykonosResetTxDeFramerLink()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            //resets deframer and lane fifos
            Link.Mykonos.enableSysrefToDeframer(0);
            Link.Mykonos.resetDeframer();

            Link.Disconnect();
        }
        public static void MykonosEnTxDeFramerLink()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.enableSysrefToDeframer(1);

            Link.Disconnect();
        }

        //Functions to Configure HW external To Mykonos
        public static void FPGADisableDeFramer()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);


            //Disable FPGA PRBS checker (if enabled)
            Link.fpgaWrite(0x404, 0x0);

            //Disable deframers in FPGA
            Link.FpgaMykonos.enableJesd204bRxDeframer(false);
            Link.FpgaMykonos.enableJesd204bObsRxDeframer(false);

            //Reset FPGA deframer IP
            //Reset JESD Rx Reset
            Link.FpgaMykonos.resetFPGAIP(0x06);

            //release reset
            Link.FpgaMykonos.resetFPGAIP(0x00);

            Link.Disconnect();
        }
        public static void FPGAEnableDeFramer()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.FpgaMykonos.enableJesd204bRxDeframer(true);
            Link.FpgaMykonos.enableJesd204bObsRxDeframer(true);

            Link.Disconnect();
        }
        public static void EvbReqSync()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            if (Link.getPcbName().Contains("CE"))
            {
                Link.Ad9528.requestSysref(true);
            }
            else
            {
                Link.FpgaMykonos.requestSysref();
            }
        }

        public static void MykonosInitNoRxProfile(TestSetupConfig settings)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            /* Prepare Mykonos Device Data structure Settings*/
            string resourcePath = settings.resourcePath;
            Console.Write(resourcePath);
            //            settings.rxProfileData = Link.Mykonos.LoadProfile(resourcePath,
            //                                                 settings.mykSettings.rxProfileName, true,
            //                                                 settings.mykSettings.DeviceClock_kHz);
            settings.txProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.txProfileName,
                                                 false, settings.mykSettings.DeviceClock_kHz);
            settings.obsRxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.orxProfileName, false,
                                                 settings.mykSettings.DeviceClock_kHz);
            settings.srxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.srxProfileName, false,
                                                 settings.mykSettings.DeviceClock_kHz);


            Link.Mykonos.init_rxsettings(settings.mykSettings.rxChannel, settings.rxPllUseExtLo,
                                            settings.rxPllLoFreq_Hz, settings.rxUseRealIfData);
            Link.Mykonos.init_txsettings((Mykonos.TXATTENSTEPSIZE)settings.txAttenStepSize,
                                            settings.mykSettings.txChannel, settings.txPllUseExtLo,
                                            settings.txPllLoFreq_Hz, settings.tx1Atten, settings.tx2Atten);
            Link.Mykonos.init_obsrxsettings((byte)settings.mykSettings.orxChannel, Mykonos.OBSRX_LO_SOURCE.OBSLO_TX_PLL,
                                             settings.obsRxPllLoFreq_Hz, 0, Mykonos.OBSRXCHANNEL.OBS_RXOFF);

            Link.Mykonos.init_jesd204bdeframer(settings.mykTxDeFrmrCfg.deviceID, settings.mykTxDeFrmrCfg.laneID,
                                                settings.mykTxDeFrmrCfg.bankID, settings.mykTxDeFrmrCfg.M,
                                                settings.mykTxDeFrmrCfg.K, settings.mykTxDeFrmrCfg.scramble,
                                                settings.mykTxDeFrmrCfg.externalSysref,
                                                settings.mykTxDeFrmrCfg.deserializerLanesEnabled,
                                                settings.mykTxDeFrmrCfg.deserializerLaneCrossbar,
                                                settings.mykTxDeFrmrCfg.eqSetting,
                                                settings.mykTxDeFrmrCfg.invertLanePolarity,
                                                settings.mykTxDeFrmrCfg.enableAutoChanXbar,
                                                settings.mykTxDeFrmrCfg.lmfcOffset,
                                                settings.mykTxDeFrmrCfg.newSysrefOnRelink);

            Link.Mykonos.init_jesd204bframer(settings.mykRxFrmrCfg.bankId, settings.mykRxFrmrCfg.deviceId,
                                                settings.mykRxFrmrCfg.laneId, settings.mykRxFrmrCfg.M,
                                                settings.mykRxFrmrCfg.K, settings.mykRxFrmrCfg.scramble,
                                                settings.mykRxFrmrCfg.externalSysref,
                                                settings.mykRxFrmrCfg.serializerLanesEnabled,
                                                settings.mykRxFrmrCfg.serializerLaneCrossbar,
                                                settings.mykRxFrmrCfg.serializerAmplitude,
                                                settings.mykRxFrmrCfg.preEmphasis,
                                                settings.mykRxFrmrCfg.invertLanePolarity,
                                                settings.mykRxFrmrCfg.lmfcOffset,
                                                settings.mykRxFrmrCfg.newSysrefOnRelink,
                                                settings.mykRxFrmrCfg.enableAutoChanXbar,
                                                settings.mykRxFrmrCfg.obsRxSyncbSelect,                                    
                                                settings.mykRxFrmrCfg.overSample);

            Link.Mykonos.init_obsRxFramer(settings.mykObsRxFrmrCfg.bankId, settings.mykObsRxFrmrCfg.deviceId,
                                            settings.mykObsRxFrmrCfg.laneId, settings.mykObsRxFrmrCfg.M,
                                            settings.mykObsRxFrmrCfg.K, settings.mykObsRxFrmrCfg.scramble,
                                            settings.mykObsRxFrmrCfg.externalSysref,
                                            settings.mykObsRxFrmrCfg.serializerLanesEnabled,
                                            settings.mykObsRxFrmrCfg.serializerLaneCrossbar,
                                            settings.mykObsRxFrmrCfg.serializerAmplitude,
                                            settings.mykObsRxFrmrCfg.preEmphasis,
                                            settings.mykObsRxFrmrCfg.invertLanePolarity,
                                            settings.mykObsRxFrmrCfg.lmfcOffset,
                                            settings.mykObsRxFrmrCfg.newSysrefOnRelink,
                                            settings.mykObsRxFrmrCfg.enableAutoChanXbar,
                                            settings.mykObsRxFrmrCfg.obsRxSyncbSelect,
                                            settings.mykObsRxFrmrCfg.overSample);

            /* Prepare Mykonos Device Data structure Settings*/
            Link.Mykonos.resetDevice();
            //            UInt32 ensmStatus = Link.spiRead(0x1B3);
            //            Console.Write("ENSM sate: " + ensmStatus.ToString("x"));
            Link.Mykonos.initialize();

            //            ensmStatus = Link.spiRead(0x1B3);
            //            Console.Write("ENSM sate: " + ensmStatus.ToString("x"));

        }
        public static void MykonosInitInvalidTxProfile(TestSetupConfig settings)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            /* Prepare Mykonos Device Data structure Settings*/
            string resourcePath = settings.resourcePath;
            Console.Write(resourcePath);
            settings.rxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.rxProfileName, true,
                                                 settings.mykSettings.DeviceClock_kHz);
            //            settings.txProfileData = Link.Mykonos.LoadProfile(resourcePath,
            //                                                 settings.mykSettings.txProfileName,
            //                                                 false, settings.mykSettings.DeviceClock_kHz);
            settings.obsRxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.orxProfileName, false,
                                                 settings.mykSettings.DeviceClock_kHz);
            settings.srxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.srxProfileName, false,
                                                 settings.mykSettings.DeviceClock_kHz);


            Link.Mykonos.init_rxsettings(settings.mykSettings.rxChannel, settings.rxPllUseExtLo,
                                            settings.rxPllLoFreq_Hz, settings.rxUseRealIfData);
            Link.Mykonos.init_txsettings((Mykonos.TXATTENSTEPSIZE)settings.txAttenStepSize,
                                            settings.mykSettings.txChannel, settings.txPllUseExtLo,
                                            settings.txPllLoFreq_Hz, settings.tx1Atten, settings.tx2Atten);
            Link.Mykonos.init_obsrxsettings((byte)settings.mykSettings.orxChannel, Mykonos.OBSRX_LO_SOURCE.OBSLO_TX_PLL,
                                             settings.obsRxPllLoFreq_Hz, 0, Mykonos.OBSRXCHANNEL.OBS_RXOFF);

            Link.Mykonos.init_jesd204bdeframer(settings.mykTxDeFrmrCfg.deviceID, settings.mykTxDeFrmrCfg.laneID,
                                                settings.mykTxDeFrmrCfg.bankID, settings.mykTxDeFrmrCfg.M,
                                                settings.mykTxDeFrmrCfg.K, settings.mykTxDeFrmrCfg.scramble,
                                                settings.mykTxDeFrmrCfg.externalSysref,
                                                settings.mykTxDeFrmrCfg.deserializerLanesEnabled,
                                                settings.mykTxDeFrmrCfg.deserializerLaneCrossbar,
                                                settings.mykTxDeFrmrCfg.eqSetting,
                                                settings.mykTxDeFrmrCfg.invertLanePolarity,
                                                settings.mykTxDeFrmrCfg.enableAutoChanXbar,
                                                settings.mykTxDeFrmrCfg.lmfcOffset,
                                                settings.mykTxDeFrmrCfg.newSysrefOnRelink);

            Link.Mykonos.init_jesd204bframer(settings.mykRxFrmrCfg.bankId, settings.mykRxFrmrCfg.deviceId,
                                                settings.mykRxFrmrCfg.laneId, settings.mykRxFrmrCfg.M,
                                                settings.mykRxFrmrCfg.K, settings.mykRxFrmrCfg.scramble,
                                                settings.mykRxFrmrCfg.externalSysref,
                                                settings.mykRxFrmrCfg.serializerLanesEnabled,
                                                settings.mykRxFrmrCfg.serializerLaneCrossbar,
                                                settings.mykRxFrmrCfg.serializerAmplitude,
                                                settings.mykRxFrmrCfg.preEmphasis,
                                                settings.mykRxFrmrCfg.invertLanePolarity,
                                                settings.mykRxFrmrCfg.lmfcOffset,
                                                settings.mykRxFrmrCfg.obsRxSyncbSelect,
                                                settings.mykRxFrmrCfg.newSysrefOnRelink,
                                                settings.mykRxFrmrCfg.overSample,
                                                settings.mykRxFrmrCfg.enableAutoChanXbar);

            Link.Mykonos.init_obsRxFramer(settings.mykObsRxFrmrCfg.bankId, settings.mykObsRxFrmrCfg.deviceId,
                                            settings.mykObsRxFrmrCfg.laneId, settings.mykObsRxFrmrCfg.M,
                                            settings.mykObsRxFrmrCfg.K, settings.mykObsRxFrmrCfg.scramble,
                                            settings.mykObsRxFrmrCfg.externalSysref,
                                            settings.mykObsRxFrmrCfg.serializerLanesEnabled,
                                            settings.mykObsRxFrmrCfg.serializerLaneCrossbar,
                                            settings.mykObsRxFrmrCfg.serializerAmplitude,
                                            settings.mykObsRxFrmrCfg.preEmphasis,
                                            settings.mykObsRxFrmrCfg.invertLanePolarity,
                                            settings.mykObsRxFrmrCfg.lmfcOffset,
                                            settings.mykObsRxFrmrCfg.newSysrefOnRelink,
                                            settings.mykObsRxFrmrCfg.enableAutoChanXbar,
                                            settings.mykObsRxFrmrCfg.obsRxSyncbSelect,
                                            settings.mykObsRxFrmrCfg.overSample);

            /* Prepare Mykonos Device Data structure Settings*/
            Link.Mykonos.resetDevice();
            //            UInt32 ensmStatus = Link.spiRead(0x1B3);
            //            Console.Write("ENSM sate: " + ensmStatus.ToString("x"));
            Link.Mykonos.initialize();

            //            ensmStatus = Link.spiRead(0x1B3);
            //            Console.Write("ENSM sate: " + ensmStatus.ToString("x"));

        }
        public static void MykonosInitDpd(TestSetupConfig settings)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            /* Prepare Mykonos Device Data structure Settings*/
            string resourcePath = settings.resourcePath;
            Console.Write(resourcePath);
            settings.rxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.rxProfileName, true,
                                                 settings.mykSettings.DeviceClock_kHz);
            settings.txProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.txProfileName,
                                                 false, settings.mykSettings.DeviceClock_kHz);
            settings.obsRxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.orxProfileName, false,
                                                 settings.mykSettings.DeviceClock_kHz);
            settings.srxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.srxProfileName, false,
                                                 settings.mykSettings.DeviceClock_kHz);


            Link.Mykonos.init_rxsettings(settings.mykSettings.rxChannel, settings.rxPllUseExtLo,
                                            settings.rxPllLoFreq_Hz, settings.rxUseRealIfData);
            Link.Mykonos.init_txsettings((Mykonos.TXATTENSTEPSIZE)settings.txAttenStepSize,
                                            settings.mykSettings.txChannel, settings.txPllUseExtLo,
                                            settings.txPllLoFreq_Hz, settings.tx1Atten, settings.tx2Atten);
            Link.Mykonos.init_obsrxsettings((byte)settings.mykSettings.orxChannel, Mykonos.OBSRX_LO_SOURCE.OBSLO_TX_PLL,
                                             settings.obsRxPllLoFreq_Hz, 0, Mykonos.OBSRXCHANNEL.OBS_RXOFF);

            Link.Mykonos.init_jesd204bdeframer(settings.mykTxDeFrmrCfg.deviceID, settings.mykTxDeFrmrCfg.laneID,
                                                settings.mykTxDeFrmrCfg.bankID, settings.mykTxDeFrmrCfg.M,
                                                settings.mykTxDeFrmrCfg.K, settings.mykTxDeFrmrCfg.scramble,
                                                settings.mykTxDeFrmrCfg.externalSysref,
                                                settings.mykTxDeFrmrCfg.deserializerLanesEnabled,
                                                settings.mykTxDeFrmrCfg.deserializerLaneCrossbar,
                                                settings.mykTxDeFrmrCfg.eqSetting,
                                                settings.mykTxDeFrmrCfg.invertLanePolarity,
                                                settings.mykTxDeFrmrCfg.enableAutoChanXbar,
                                                settings.mykTxDeFrmrCfg.lmfcOffset,
                                                settings.mykTxDeFrmrCfg.newSysrefOnRelink);

            Link.Mykonos.init_jesd204bframer(settings.mykRxFrmrCfg.bankId, settings.mykRxFrmrCfg.deviceId,
                                                settings.mykRxFrmrCfg.laneId, settings.mykRxFrmrCfg.M,
                                                settings.mykRxFrmrCfg.K, settings.mykRxFrmrCfg.scramble,
                                                settings.mykRxFrmrCfg.externalSysref,
                                                settings.mykRxFrmrCfg.serializerLanesEnabled,
                                                settings.mykRxFrmrCfg.serializerLaneCrossbar,
                                                settings.mykRxFrmrCfg.serializerAmplitude,
                                                settings.mykRxFrmrCfg.preEmphasis,
                                                settings.mykRxFrmrCfg.invertLanePolarity,
                                                settings.mykRxFrmrCfg.lmfcOffset,
                                                settings.mykRxFrmrCfg.obsRxSyncbSelect,
                                                settings.mykRxFrmrCfg.newSysrefOnRelink,
                                                settings.mykRxFrmrCfg.overSample,
                                                settings.mykRxFrmrCfg.enableAutoChanXbar);

            Link.Mykonos.init_obsRxFramer(settings.mykObsRxFrmrCfg.bankId, settings.mykObsRxFrmrCfg.deviceId,
                                            settings.mykObsRxFrmrCfg.laneId, settings.mykObsRxFrmrCfg.M,
                                            settings.mykObsRxFrmrCfg.K, settings.mykObsRxFrmrCfg.scramble,
                                            settings.mykObsRxFrmrCfg.externalSysref,
                                            settings.mykObsRxFrmrCfg.serializerLanesEnabled,
                                            settings.mykObsRxFrmrCfg.serializerLaneCrossbar,
                                            settings.mykObsRxFrmrCfg.serializerAmplitude,
                                            settings.mykObsRxFrmrCfg.preEmphasis,
                                            settings.mykObsRxFrmrCfg.invertLanePolarity,
                                            settings.mykObsRxFrmrCfg.lmfcOffset,
                                            settings.mykObsRxFrmrCfg.newSysrefOnRelink,
                                            settings.mykObsRxFrmrCfg.enableAutoChanXbar,
                                            settings.mykObsRxFrmrCfg.obsRxSyncbSelect,
                                            settings.mykObsRxFrmrCfg.overSample);




            Link.Mykonos.init_dpdConfigStruct(1, ref settings.mykDpdCfg);
            Link.Mykonos.init_clgcConfigStruct(1, ref settings.mykClgcCfg);
            Link.Mykonos.init_enableDpd(0x1);
            /* Prepare Mykonos Device Data structure Settings*/
            Link.Mykonos.resetDevice();
            //            UInt32 ensmStatus = Link.spiRead(0x1B3);
            //            Console.Write("ENSM sate: " + ensmStatus.ToString("x"));
            Link.Mykonos.initialize();

            //            ensmStatus = Link.spiRead(0x1B3);
            //            Console.Write("ENSM sate: " + ensmStatus.ToString("x"));

        }
        public static void JESDMykonosInit(TestSetupConfig settings, Mykonos.RXCHANNEL rxChannels, Mykonos.TXCHANNEL txChannels, byte Obsrxchannels)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            /* Prepare Mykonos Device Data structure Settings*/
            string resourcePath = settings.resourcePath;
            Console.Write(resourcePath);
            settings.rxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.rxProfileName, true,
                                                 settings.mykSettings.DeviceClock_kHz);
            settings.txProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.txProfileName,
                                                 false, settings.mykSettings.DeviceClock_kHz);
            settings.obsRxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.orxProfileName, false,
                                                 settings.mykSettings.DeviceClock_kHz);
            settings.srxProfileData = Link.Mykonos.LoadProfile(resourcePath,
                                                 settings.mykSettings.srxProfileName, false,
                                                 settings.mykSettings.DeviceClock_kHz);


            Link.Mykonos.init_rxsettings(rxChannels, settings.rxPllUseExtLo,
                                            settings.rxPllLoFreq_Hz, settings.rxUseRealIfData);
            Link.Mykonos.init_txsettings((Mykonos.TXATTENSTEPSIZE)settings.txAttenStepSize,
                                            txChannels, settings.txPllUseExtLo,
                                            settings.txPllLoFreq_Hz, settings.tx1Atten, settings.tx2Atten);
            Link.Mykonos.init_obsrxsettings(Obsrxchannels, Mykonos.OBSRX_LO_SOURCE.OBSLO_TX_PLL,
                                             settings.obsRxPllLoFreq_Hz, 0, Mykonos.OBSRXCHANNEL.OBS_RXOFF);

            Link.Mykonos.init_jesd204bdeframer(settings.mykTxDeFrmrCfg.deviceID, settings.mykTxDeFrmrCfg.laneID,
                                                settings.mykTxDeFrmrCfg.bankID, settings.mykTxDeFrmrCfg.M,
                                                settings.mykTxDeFrmrCfg.K, settings.mykTxDeFrmrCfg.scramble,
                                                settings.mykTxDeFrmrCfg.externalSysref,
                                                settings.mykTxDeFrmrCfg.deserializerLanesEnabled,
                                                settings.mykTxDeFrmrCfg.deserializerLaneCrossbar,
                                                settings.mykTxDeFrmrCfg.eqSetting,
                                                settings.mykTxDeFrmrCfg.invertLanePolarity,
                                                settings.mykTxDeFrmrCfg.enableAutoChanXbar,
                                                settings.mykTxDeFrmrCfg.lmfcOffset,
                                                settings.mykTxDeFrmrCfg.newSysrefOnRelink);

            Link.Mykonos.init_jesd204bframer(settings.mykRxFrmrCfg.bankId, settings.mykRxFrmrCfg.deviceId,
                                                settings.mykRxFrmrCfg.laneId, settings.mykRxFrmrCfg.M,
                                                settings.mykRxFrmrCfg.K, settings.mykRxFrmrCfg.scramble,
                                                settings.mykRxFrmrCfg.externalSysref,
                                                settings.mykRxFrmrCfg.serializerLanesEnabled,
                                                settings.mykRxFrmrCfg.serializerLaneCrossbar,
                                                settings.mykRxFrmrCfg.serializerAmplitude,
                                                settings.mykRxFrmrCfg.preEmphasis,
                                                settings.mykRxFrmrCfg.invertLanePolarity,
                                                settings.mykRxFrmrCfg.lmfcOffset,
                                                settings.mykRxFrmrCfg.obsRxSyncbSelect,
                                                settings.mykRxFrmrCfg.newSysrefOnRelink,
                                                settings.mykRxFrmrCfg.overSample,
                                                settings.mykRxFrmrCfg.enableAutoChanXbar);

            Link.Mykonos.init_obsRxFramer(settings.mykObsRxFrmrCfg.bankId, settings.mykObsRxFrmrCfg.deviceId,
                                            settings.mykObsRxFrmrCfg.laneId, settings.mykObsRxFrmrCfg.M,
                                            settings.mykObsRxFrmrCfg.K, settings.mykObsRxFrmrCfg.scramble,
                                            settings.mykObsRxFrmrCfg.externalSysref,
                                            settings.mykObsRxFrmrCfg.serializerLanesEnabled,
                                            settings.mykObsRxFrmrCfg.serializerLaneCrossbar,
                                            settings.mykObsRxFrmrCfg.serializerAmplitude,
                                            settings.mykObsRxFrmrCfg.preEmphasis,
                                            settings.mykObsRxFrmrCfg.invertLanePolarity,
                                            settings.mykObsRxFrmrCfg.lmfcOffset,
                                            settings.mykObsRxFrmrCfg.newSysrefOnRelink,
                                            settings.mykObsRxFrmrCfg.enableAutoChanXbar,
                                            settings.mykObsRxFrmrCfg.obsRxSyncbSelect,
                                            settings.mykObsRxFrmrCfg.overSample);

            /* Prepare Mykonos Device Data structure Settings*/
            Link.Mykonos.resetDevice();
            Link.Mykonos.initialize();

        }
        public static void JESDMykonosInitArm(TestSetupConfig settings)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.initArm();

            String fileName = "";

            fileName = settings.resourcePath + @"ArmBinaries\" + settings.mykSettings.ArmFirmwareName;

            var watch = Stopwatch.StartNew();
            Link.Mykonos.loadArm(fileName);
            watch.Stop();
            Debug.WriteLine("Time to load ARM firmware(ms): " + watch.ElapsedMilliseconds);

        }

        /// <summary>
        /// Defualt Test Setup For Most QA Tests
        /// Setup Parameters:  Defined by TestSetupConfig Settings
        /// </summary>
        public static void ResetPlatform(TestSetupConfig settings)
        {


            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);


            //GetPlatform Info
            Console.Write(Link.hw.Query("GetPcbInfo"));
            Console.Write(Link.identify());
            Console.Write(Link.version());
            UInt32 fpgaVersion = Link.FpgaMykonos.readFpgaVersion();
            Debug.WriteLine("FPGA Version: " + fpgaVersion.ToString("X"));
            UInt32 fpgaDevClock_kHz = Link.FpgaMykonos.readRefClockFrequency();
            Debug.WriteLine("FPGA Device Clock: " + fpgaDevClock_kHz + " kHz");

            //Initialise Platform
            //Configure AD9528 To Provide Reference Clock
            //TODO: should vcxoFrequency_Hz and refclkAFrequency_Hz be made dynamic?
            Link.Mykonos.resetDevice();
            Link.Ad9528.initDeviceDataStructure(122880000, 30720000,
                                    (settings.mykSettings.DeviceClock_kHz * 1000));
            Link.hw.ReceiveTimeout = 0;
            Link.Ad9528.resetDevice();
            Link.Ad9528.initialize();
            System.Threading.Thread.Sleep(500);
            //NOTE: doesn't wait long enough...the LOCK status toggles for a while until stable.
            bool arePllsLocked = Link.Ad9528.waitForPllLock();

            Assert.AreEqual(true, arePllsLocked, "AD9528 PLLs are not locked");
            Link.Ad9528.enableClockOutputs(0x300A);

            //Initialise Mykonos Device
            //First Reset FPGA Link to Mykonos Device
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.FpgaMykonos.resetFPGAIP(0x7);
            Link.FpgaMykonos.resetFPGAIP(0x0);
            Link.Mykonos.resetDevice();

            //MykonosInit(settings);
            Link.Disconnect();

        }
    }
}
