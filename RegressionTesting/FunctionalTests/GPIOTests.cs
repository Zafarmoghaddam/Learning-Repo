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
    [TestFixture]
    public class GPIOApiFunctionalTests
    {
        public const byte MykonosSpi = 0x1;
        public const byte AD9528Spi = 0x2;
        public static TestSetupConfig settings = new TestSetupConfig();

        /// <summary>
        /// GPIOFunctionalTests Test Functionality of GPIO APIs
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
        public void GPIOTestInit()
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
            Console.WriteLine("GPIOApiFunctional Test Setup: Complete");
        }


        ///<summary>
        /// API Test Name: 
        ///     testSetupGpio3v3
        /// API Under-Test: 
        ///     MYKONOS_setupGpio3v3
        /// API Test Description: 
        ///  Use MYKONOS_setupGpio3v3 To setup GPIO3V3 
        ///  Parameters. Check the SPI registers for correct configuration
        /// API Test Pass Criteria: 
        ///  Verify the in range configuration Values match the 
        ///  SPI Register Readbacks.
        /// Notes:
        ///</summary>
        ///
        [Test, Sequential]
        [Category("GPIO")]
        public static void testSetupGpio3v3([Values(Mykonos.GPIO3V3_MODE.GPIO3V3_LEVELTRANSLATE_MODE, Mykonos.GPIO3V3_MODE.GPIO3V3_INVLEVELTRANSLATE_MODE, Mykonos.GPIO3V3_MODE.GPIO3V3_BITBANG_MODE, Mykonos.GPIO3V3_MODE.GPIO3V3_EXTATTEN_LUT_MODE)] Mykonos.GPIO3V3_MODE gpio3v3Mode3_0,
                                            [Values(Mykonos.GPIO3V3_MODE.GPIO3V3_LEVELTRANSLATE_MODE, Mykonos.GPIO3V3_MODE.GPIO3V3_INVLEVELTRANSLATE_MODE, Mykonos.GPIO3V3_MODE.GPIO3V3_BITBANG_MODE, Mykonos.GPIO3V3_MODE.GPIO3V3_EXTATTEN_LUT_MODE)] Mykonos.GPIO3V3_MODE gpio3v3Mode7_4,
                                            [Values(Mykonos.GPIO3V3_MODE.GPIO3V3_LEVELTRANSLATE_MODE, Mykonos.GPIO3V3_MODE.GPIO3V3_INVLEVELTRANSLATE_MODE, Mykonos.GPIO3V3_MODE.GPIO3V3_BITBANG_MODE, Mykonos.GPIO3V3_MODE.GPIO3V3_EXTATTEN_LUT_MODE)] Mykonos.GPIO3V3_MODE gpio3v3Mode11_8)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            UInt16 gpio3v3Oe = 0xFF;
            Link.Mykonos.setupGpio3v3(gpio3v3Oe, gpio3v3Mode3_0, gpio3v3Mode7_4, gpio3v3Mode11_8);

            //Check GPIO3v3 direction control bit
            spiData1 = Link.spiRead(0xB00); Console.WriteLine("0xB00: " + spiData1.ToString("X"));
            Assert.AreEqual(gpio3v3Oe & 0xFF, spiData1, "GPIO3v3 direction control bit 7-0 not as expected");
            spiData1 = Link.spiRead(0xB01); Console.WriteLine("0xB01: " + spiData1.ToString("X"));
            Assert.AreEqual((gpio3v3Oe >> 8) & 0x0F, spiData1, "GPIO3v3 direction control bit 15-8 not as expected");

            //Check GPIO3v3
            spiData1 = Link.spiRead(0xB06); Console.WriteLine("0xB06: " + spiData1.ToString("X"));
            spiData2 = Link.spiRead(0xB07); Console.WriteLine("0xB07: " + spiData2.ToString("X"));
            Assert.AreEqual(((((byte)gpio3v3Mode7_4) << 4) | (byte)gpio3v3Mode3_0), spiData1, "GPIO3v3 LSB not as expected");
            Assert.AreEqual(((byte)gpio3v3Mode11_8 & 0x0F), spiData2, "GPIO3v3 MSB not as expected");

            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     testSetGpio3v3PinLevel
        /// API Under-Test: 
        ///     MYKONOS_setGpio3v3PinLevel
        /// API Test Description: 
        ///  Call MYKONOS_setGpio3v3PinLevel to configure
        ///  A valid configuration .
        /// API Test Pass Criteria: 
        /// Verify pin level matches SPI register Readback
        /// Notes:
        ///</summary>
        [Test, Sequential]
        [Category("GPIO")]
        public static void testSetGpio3v3PinLevel([Values((UInt16)0x01, (UInt16)0x10)] UInt16 gpio3v3PinLevel)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            UInt16 gpio3v3Oe = 0xFF;
            ushort gpio3v3SetLevelReadback = 0x0;
            Mykonos.GPIO3V3_MODE gpio3v3Mode3_0 = Mykonos.GPIO3V3_MODE.GPIO3V3_BITBANG_MODE;
            Mykonos.GPIO3V3_MODE gpio3v3Mode7_4 = Mykonos.GPIO3V3_MODE.GPIO3V3_BITBANG_MODE;
            Mykonos.GPIO3V3_MODE gpio3v3Mode11_8 = Mykonos.GPIO3V3_MODE.GPIO3V3_BITBANG_MODE;
            Link.Mykonos.setupGpio3v3(gpio3v3Oe, gpio3v3Mode3_0, gpio3v3Mode7_4, gpio3v3Mode11_8);
            Link.Mykonos.setGpio3v3PinLevel(gpio3v3PinLevel);
            Link.Mykonos.getGpio3v3SetLevel(ref gpio3v3SetLevelReadback);

            spiData1 = Link.spiRead(0xB02); Console.WriteLine("0xB02: " + spiData1.ToString("X"));
            spiData2 = Link.spiRead(0xB03); Console.WriteLine("0xB03: " + spiData2.ToString("X"));
            Assert.AreEqual(gpio3v3PinLevel & 0xFF, spiData1, "GPIO3v3 7-0 not as expected");
            Assert.AreEqual((gpio3v3PinLevel >> 8) & 0x0F, spiData2, "GPIO3v3 15-8 not as expected");
            Assert.AreEqual(gpio3v3PinLevel, gpio3v3SetLevelReadback, "GPIO3v3 set level readback not as expexted");

            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     testReadGpio3v3PinLevel
        /// API Under-Test: 
        ///     MYKONOS_getGpio3v3PinLevel
        /// API Test Description: 
        ///  Call MYKONOS_getGpio3v3PinLevel to 
        ///  Read back GPIO configuration.
        /// API Test Pass Criteria: 
        /// Verify pin level matches SPI register Readback
        /// Notes:
        ///</summary>
        [Test, Sequential]
        [Category("GPIO")]
        public static void testReadGpio3v3PinLevel([Values((UInt16)0x01, (UInt16)0x20)] UInt16 gpio3v3PinLevel)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            UInt16 gpio3v3Oe = 0x3F;
            UInt16 gpio3v3PinLevelReadback = 0x0;
            Mykonos.GPIO3V3_MODE gpio3v3Mode3_0 = Mykonos.GPIO3V3_MODE.GPIO3V3_BITBANG_MODE;
            Mykonos.GPIO3V3_MODE gpio3v3Mode7_4 = Mykonos.GPIO3V3_MODE.GPIO3V3_BITBANG_MODE;
            Mykonos.GPIO3V3_MODE gpio3v3Mode11_8 = Mykonos.GPIO3V3_MODE.GPIO3V3_BITBANG_MODE;
            Link.Mykonos.setupGpio3v3(gpio3v3Oe, gpio3v3Mode3_0, gpio3v3Mode7_4, gpio3v3Mode11_8);
            Link.Mykonos.setGpio3v3PinLevel(gpio3v3PinLevel);
            Link.Mykonos.getGpio3v3PinLevel(ref gpio3v3PinLevelReadback);
            Console.WriteLine(gpio3v3PinLevelReadback);
            spiData1 = Link.spiRead(0xB04); Console.WriteLine("0xB04: " + spiData1.ToString("X"));
            spiData2 = Link.spiRead(0xB05); Console.WriteLine("0xB05: " + spiData2.ToString("X"));
            Assert.AreEqual(gpio3v3PinLevelReadback, ((spiData2 & 0x0F) << 8) | spiData1, "GPIO3v3 readback not as expected"); 
            switch (gpio3v3PinLevel)
            {
                case 0x01:
                    Assert.AreEqual(gpio3v3PinLevelReadback, 2048, "GPIO input read not as expected");
                    break;
                case 0x20:
                    Assert.AreEqual(gpio3v3PinLevelReadback, 64, "GPIO input read not as expected");
                    break;
                default:
                    Assert.Fail("output GPIO level not defined in test code");
                    break;

            }
            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     testSetGpioPinLevel
        /// API Under-Test: 
        ///     MYKONOS_setGpioPinLevel
        ///     MYKONOS_getGpioPinLevel
        ///     MYKONOS_getGpioSetLevel
        /// API Test Description: 
        ///  Call MYKONOS_setGpioPinLevel to set GPIO Pin level
        ///  Call MYKONOS_getGpioPinLevel and MYKONOS_getGpioSetLevel to
        ///  Read back GPIO configuration.
        /// API Test Pass Criteria: 
        /// Verify pin level matches SPI register Readback
        /// Notes:
        ///</summary>
        [Test, Sequential]
        [Category("GPIO")]
        public static void testSetGpioPinLevel([Values((UInt32)0x40, (UInt32)0x40000)] UInt32 gpioPinLevel)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            UInt32 spiData1 = 0x0;
            UInt32 spiData2 = 0x0;
            UInt32 spiData3 = 0x0;
            UInt32 gpioSetLevelReadback = 0x0;
            UInt32 gpioReadback = 0x0;

            //set 9 of the GPIO pins as outputs and the other 9 as inputs
            uint gpioOe = 0x55554;

            //set all the source nibbles to the GPIO_BITBANG_MODE 
            Mykonos.GPIO_MODE gpioMode3_0 = Mykonos.GPIO_MODE.GPIO_BITBANG_MODE;
            Mykonos.GPIO_MODE gpioMode7_4 = Mykonos.GPIO_MODE.GPIO_BITBANG_MODE;
            Mykonos.GPIO_MODE gpioMode11_8 = Mykonos.GPIO_MODE.GPIO_BITBANG_MODE;
            Mykonos.GPIO_MODE gpioMode15_12 = Mykonos.GPIO_MODE.GPIO_BITBANG_MODE;
            Mykonos.GPIO_MODE gpioMode18_16 = Mykonos.GPIO_MODE.GPIO_BITBANG_MODE;

            Link.Mykonos.setupGpio(gpioOe, gpioMode3_0, gpioMode7_4, gpioMode11_8, gpioMode15_12, gpioMode18_16);
            Link.Mykonos.setGpioPinLevel(gpioPinLevel);

            spiData1 = Link.spiRead(0xB23); Console.WriteLine("0xB23: " + spiData1.ToString("X"));
            spiData2 = Link.spiRead(0xB24); Console.WriteLine("0xB24: " + spiData2.ToString("X"));
            spiData3 = Link.spiRead(0xB25); Console.WriteLine("0xB25: " + spiData1.ToString("X"));

            Link.Mykonos.getGpioSetLevel(ref gpioSetLevelReadback);
            Link.Mykonos.getGpioPinLevel(ref gpioReadback);

            spiData1 = Link.spiRead(0xB23); Console.WriteLine("0xB23: " + spiData1.ToString("X"));
            spiData2 = Link.spiRead(0xB24); Console.WriteLine("0xB24: " + spiData2.ToString("X"));
            spiData3 = Link.spiRead(0xB25); Console.WriteLine("0xB25: " + spiData1.ToString("X"));

            spiData1 = Link.spiRead(0xB26); Console.WriteLine("0xB26: " + spiData1.ToString("X"));
            spiData2 = Link.spiRead(0xB27); Console.WriteLine("0xB27: " + spiData2.ToString("X"));
            spiData3 = Link.spiRead(0xB28); Console.WriteLine("0xB28: " + spiData1.ToString("X"));

            Assert.AreEqual(gpioPinLevel, gpioSetLevelReadback, "GPIO set Level readback not as expected");
            Assert.AreEqual((((spiData3 & 0x07) << 16) | (spiData2 << 8) | spiData1), gpioReadback, "GPIO readback not as expexted");
            Assert.AreEqual(gpioPinLevel >> 1, gpioReadback, "GPIO input not as expected");


            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     testConfigGpInterrupt
        /// API Under-Test: 
        ///     MYKONOS_configGpInterrupt
        ///     MYKONOS_readGpInterruptStatus
        /// API Test Description: 
        ///  Call MYKONOS_configGpInterrupt to
        ///  Sets the General Purpose (GP) interrupt register bit mask
        ///  Call MYKONOS_readGpInterruptStatus to
        ///  Read back the General Purpose (GP) interrupt status.
        /// API Test Pass Criteria: 
        /// Verify General Purpose (GP) interrupt register bit mask
        /// is configured properly
        /// Verfy General Purpose (GP) interrupt status 
        /// matches SPI register Readback
        /// Notes:
        ///</summary>
        [Test]
        [Category("GPIO")]
        public static void testConfigGpInterrupt([Values((UInt16)0x01, (UInt16)0x02, (UInt16)0x04, (UInt16)0x08, (UInt16)0x1)] UInt16 gpMask)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            UInt16 readStatus = 0;

            Link.Mykonos.configGpInterrupt(gpMask);

            //int status = ((readStatus & 0xF0) | ((~readStatus) & 0x0F));

            spiData1 = Link.spiRead(0xB42); Console.WriteLine("0xB42: " + spiData1.ToString("X"));
            spiData2 = Link.spiRead(0xB43); Console.WriteLine("0xB43: " + spiData2.ToString("X"));
            Assert.AreEqual(gpMask & 0xFF, spiData1, "gpMask register 1 not as expected");
            Assert.AreEqual(((gpMask >> 8) & 0x01), spiData2, "gpMask register 2 not as expected");
            Link.Mykonos.readGpInterruptStatus(ref readStatus);

            spiData1 = Link.spiRead(0xB44); Console.WriteLine("0xB44: " + spiData1.ToString("X"));
            spiData2 = Link.spiRead(0xB45); Console.WriteLine("0xB45: " + spiData2.ToString("X"));
            int status = (int)((((UInt16)(spiData1) & 0xE0U) | ((~(UInt16)(spiData1)) & 0x1F)) | (((UInt16)(spiData2) & 0x0003) << 0x08));
            Assert.AreEqual(status, readStatus, "gpMask readback not as expected");


            Link.Disconnect();
        }


        ///<summary>
        /// API Test Name: 
        ///     testSetRx1GainCtrlPin
        /// API Under-Test: 
        ///     MYKONOS_setRx1GainCtrlPin
        ///     
        /// API Test Description: 
        ///  Call MYKONOS_setRx1GainCtrlPin to
        ///  set the Rx1 gain and reads the wrtStep
        ///  and wrtPin registers to see if the values
        ///  are the same
        /// API Test Pass Criteria: 
        /// Verify that the written values
        /// are the same as the read values
        /// Notes:
        ///</summary>
        [Test]
        [Category("GPIO")]
        public static void testSetRx1GainCtrlPin()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            int spiData2 = 0x0;
            int incStep = 7;
            int decStep = 7;
            uint gainInc = 0x400;
            uint gainDec = 0x800;
            int enCh = 1;
            Link.Mykonos.setRx1GainCtrlPin((byte)incStep, (byte)decStep, gainInc, gainDec, (byte)enCh);
            int wrtStep = (incStep << 0x05) | (decStep << 0x02) | (enCh << 0x00);
            int wrtPin = (0x00 | 0x04 | 0x01);

            spiData2 = Link.spiRead(0x433);
            spiData2 = (spiData2 & ~0x0FD) | (wrtStep & 0x0FD);
            spiData1 = Link.spiRead(0x433); Console.WriteLine("0x433: " + spiData1.ToString("X"));
            Assert.AreEqual(spiData2, spiData1, "wrtStep not as expected");

            spiData2 = Link.spiRead(0x434);
            spiData2 = (spiData2 & ~0x0F) | (wrtPin & 0x0F);
            spiData1 = Link.spiRead(0x434); Console.WriteLine("0x434: " + spiData1.ToString("X"));
            Assert.AreEqual(spiData2, spiData1, "wrtPin not as expected");
            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     testSetRx2GainCtrlPin
        /// API Under-Test: 
        ///     MYKONOS_setRx2GainCtrlPin
        ///     
        /// API Test Description: 
        ///  Call MYKONOS_setRx2GainCtrlPin to
        ///  set the Rx1 gain and reads the wrtStep
        ///  and wrtPin registers to see if the values
        ///  are the same
        /// API Test Pass Criteria: 
        /// Verify that the written values
        /// are the same as the read values
        /// Notes:
        ///</summary>
        [Test]
        [Category("GPIO")]
        public static void testSetRx2GainCtrlPin()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            int spiData2 = 0x0;
            int incStep = 7;
            int decStep = 7;
            uint gainInc = 0x2000;
            uint gainDec = 0x4000;
            int enCh = 1;
            Link.Mykonos.setRx2GainCtrlPin((byte)incStep, (byte)decStep, gainInc, gainDec, (byte)enCh);
            int wrtStep = (incStep << 0x05) | (decStep << 0x02) | (enCh << 0x01);
            int wrtPin = (0x00 | 0x40 | 0x10);

            spiData2 = Link.spiRead(0x433);
            spiData2 = (spiData2 & ~0xFE) | (wrtStep & 0xFE);
            spiData1 = Link.spiRead(0x433); Console.WriteLine("0x433: " + spiData1.ToString("X"));
            Assert.AreEqual(spiData2, spiData1, "wrtStep not as expected");

            spiData2 = Link.spiRead(0x434);
            spiData2 = (spiData2 & ~0xF0) | (wrtPin & 0xF0);
            spiData1 = Link.spiRead(0x434); Console.WriteLine("0x434: " + spiData1.ToString("X"));
            Assert.AreEqual(spiData2, spiData1, "wrtPin not as expected");
            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     testsetTx1AttenCtrlPin
        /// API Under-Test: 
        ///     MYKONOS_setTx1AttenCtrlPin
        ///     MYKONOS_getTx1AttenCtrlPin
        /// API Test Description: 
        ///  Call MYKONOS_setTx1AttenCtrlPin to set 
        ///  the Tx1 attenuation control pin and read back
        ///  the values from the registers. Also read back
        ///  the values using MYKONOS_getTx1AttenCtrlPin
        /// API Test Pass Criteria: 
        /// Verify that the written values
        /// are the same as the read values
        /// Notes:
        ///</summary>
        [Test]
        [Category("GPIO")]
        public static void testsetTx1AttenCtrlPin()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            int spiData2 = 0x0;
            int stepSize = 0x1f;
            byte readstepSize = 0;
            uint tx1AttenIncPin = 0x1000;
            uint readtx1AttenIncPin = 0;
            uint tx1AttenDecPin = 0x2000;
            uint readtx1AttenDecPin = 0;
            int useTx1ForTx2 = 1;
            byte readuseTx1ForTx2 = 0;
            int enable = 1;
            byte readenable = 0;
            int tpcMode = 0;
            int tpcMaskTx2 = 0;



            Link.Mykonos.setTx1AttenCtrlPin((byte)stepSize, tx1AttenIncPin, tx1AttenDecPin, (byte)enable, (byte)useTx1ForTx2);
            int wrtPin = (0x00 | 0x04 | 0x01);


            tpcMode = (enable > 0) ? 0x03 : 0x01;
            if (useTx1ForTx2 > 0)
            {
                tpcMode |= 0x1C;
                tpcMaskTx2 = 0x1F;
            }
            else
            {
                tpcMaskTx2 = 0x13;
            }

            if (enable == 0)
            {
                tpcMode = 0x05;
                tpcMaskTx2 = 0x1F;
                wrtPin = 0;
            }


            spiData2 = Link.spiRead(0x96D);
            spiData2 = (spiData2 & ~0x1F) | (stepSize & 0x1F);
            spiData1 = Link.spiRead(0x96D); Console.WriteLine("0x96D: " + spiData1.ToString("X"));
            Assert.AreEqual(spiData2, spiData1, "stepSize not as expected");

            spiData2 = Link.spiRead(0x96E);
            spiData2 = (spiData2 & ~tpcMaskTx2) | (tpcMode & tpcMaskTx2);
            spiData1 = Link.spiRead(0x96E); Console.WriteLine("0x96E: " + spiData1.ToString("X"));
            Assert.AreEqual(spiData2, spiData1, "tpcMode not as expected");

            spiData2 = Link.spiRead(0x96F);
            spiData2 = (spiData2 & ~0x0f) | (wrtPin & 0x0f);
            spiData1 = Link.spiRead(0x96F); Console.WriteLine("0x96F: " + spiData1.ToString("X"));
            Assert.AreEqual(spiData2, spiData1, "wrtPin not as expected");


            Link.Mykonos.getTx1AttenCtrlPin(ref readstepSize, ref readtx1AttenIncPin, ref readtx1AttenDecPin, ref readenable, ref readuseTx1ForTx2);
            Assert.AreEqual(stepSize, readstepSize, "stepSize is not the same");
            if (enable > 0)
            {
                Assert.AreEqual(tx1AttenIncPin, readtx1AttenIncPin, "tx1AttenIncPin is not the same");
                Assert.AreEqual(tx1AttenDecPin, readtx1AttenDecPin, "tx1AttenDecPin is not the same");
                if (useTx1ForTx2 > 0)
                    Assert.Greater(readuseTx1ForTx2, 0, "useTx1ForTx2 is not the same");
                else
                    Assert.AreEqual(readuseTx1ForTx2, 0, "useTx1ForTx2 is not the same");
            }
            Assert.AreEqual(enable, readenable, "enable is not the same");

            Link.Mykonos.setTx1AttenCtrlPin((byte)stepSize, tx1AttenIncPin, tx1AttenDecPin, (byte)0, (byte)useTx1ForTx2);
            Link.Mykonos.getTx1AttenCtrlPin(ref readstepSize, ref readtx1AttenIncPin, ref readtx1AttenDecPin, ref readenable, ref readuseTx1ForTx2);
            Assert.AreEqual(0, readenable, "enable is not 0");

            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     testsetTx2AttenCtrlPin
        /// API Under-Test: 
        ///     MYKONOS_setTx2AttenCtrlPin
        ///     MYKONOS_getTx2AttenCtrlPin

        /// API Test Description: 
        ///  Call MYKONOS_setTx2AttenCtrlPin to set 
        ///  the Tx1 attenuation control pin and read back
        ///  the values from the registers. Also read back
        ///  the values using MYKONOS_getTx2AttenCtrlPin
        /// API Test Pass Criteria: 
        /// Verify that the written values
        /// are the same as the read values
        /// Notes:
        ///</summary>
        [Test]
        [Category("GPIO")]
        public static void testsetTx2AttenCtrlPin()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            int spiData2 = 0x0;
            int stepSize = 0x1f;
            byte readstepSize = 0;
            uint tx1AttenIncPin = 0x4000;
            uint readtx1AttenIncPin = 0;
            uint tx1AttenDecPin = 0x8000;
            uint readtx1AttenDecPin = 0;
            byte useTx1ForTx2 = 1;
            int enable = 1;
            byte readenable = 0;
            int tcpmode = 0;
            byte readuseTx1ForTx2 = 0;



            Link.Mykonos.setTx2AttenCtrlPin((byte)stepSize, tx1AttenIncPin, tx1AttenDecPin, (byte)enable);
            int wrtPin = (0x00 | 0x40 | 0x10);


            tcpmode = (enable > 0) ? 0x0C : 0x01;
            //tcpmode |= (useTx1ForTx2 > 0) ? 0x10 : 0x00;


            if (enable == 0)
            {
                tcpmode = 0x05;
                wrtPin = 0;
            }
            spiData2 = Link.spiRead(0x96D);
            spiData2 = (spiData2 & ~0x1F) | (stepSize & 0x1F);
            spiData1 = Link.spiRead(0x96D); Console.WriteLine("0x96D: " + spiData1.ToString("X"));
            Assert.AreEqual(spiData2, spiData1, "stepSize not as expected");

            spiData2 = Link.spiRead(0x96E);
            spiData2 = (spiData2 & ~0x1C) | (tcpmode & 0x1C);
            spiData1 = Link.spiRead(0x96E); Console.WriteLine("0x96E: " + spiData1.ToString("X"));
            Assert.AreEqual(spiData2, spiData1, "tcpmode not as expected");

            spiData2 = Link.spiRead(0x96F);
            spiData2 = (spiData2 & ~0xf0) | (wrtPin & 0xf0);
            spiData1 = Link.spiRead(0x96F); Console.WriteLine("0x96F: " + spiData1.ToString("X"));
            Assert.AreEqual(spiData2, spiData1, "wrtPin not as expected");

            Link.Mykonos.getTx2AttenCtrlPin(ref readstepSize, ref readtx1AttenIncPin, ref readtx1AttenDecPin, ref readenable, ref useTx1ForTx2);
            Assert.AreEqual(stepSize, readstepSize, "stepSize is not the same");
            if (enable > 0)
            {
                Assert.AreEqual(tx1AttenIncPin, readtx1AttenIncPin, "tx2AttenIncPin is not the same");
                Assert.AreEqual(tx1AttenDecPin, readtx1AttenDecPin, "tx2AttenDecPin is not the same");
            }
            Assert.AreEqual(enable, readenable, "enable is not the same");


            Link.Mykonos.setTx2AttenCtrlPin((byte)stepSize, tx1AttenIncPin, tx1AttenDecPin, (byte)0);
            Link.Mykonos.getTx2AttenCtrlPin(ref readstepSize, ref readtx1AttenIncPin, ref readtx1AttenDecPin, ref readenable, ref readuseTx1ForTx2);
            Assert.AreEqual(0, readenable, "enable is not 0");

            Link.Disconnect();
        }



        ///<summary>
        /// API Test Name: 
        ///     testSetupGpio
        /// API Under-Test: 
        ///     MYKONOS_setupGpio
        /// API Test Description: 
        ///  Use MYKONOS_setupGpio To setup GPIO 
        ///  Parameters. Check the SPI registers for correct configuration
        /// API Test Pass Criteria: 
        ///  Verify the in range configuration Values match the 
        ///  SPI Register Readbacks.
        /// Notes:
        ///</summary>
        ///
        [Test]
        [Category("GPIO")]
        public static void testSetupGpio()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            uint gpioOe = 0x0;
            int shift = 0;
            uint usedMask = 0x7FFFF;
           // foreach (Mykonos.OBSRXCHANNEL channel in Enum.GetValues(typeof(Mykonos.OBSRXCHANNEL)))
            foreach (Mykonos.GPIO_MODE gpioMode3_0 in Enum.GetValues(typeof(Mykonos.GPIO_MODE)))
            {
                foreach (Mykonos.GPIO_MODE gpioMode7_4 in Enum.GetValues(typeof(Mykonos.GPIO_MODE)))
                {
                    foreach (Mykonos.GPIO_MODE gpioMode11_8 in Enum.GetValues(typeof(Mykonos.GPIO_MODE)))
                    {
                        foreach (Mykonos.GPIO_MODE gpioMode15_12 in Enum.GetValues(typeof(Mykonos.GPIO_MODE)))
                        {
                            foreach (Mykonos.GPIO_MODE gpioMode18_16 in new Mykonos.GPIO_MODE[] {Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_BITBANG_MODE})
                            {

                                for (gpioOe = 0; gpioOe <= 0x7FFFF; gpioOe = gpioOe + 0x7FFFF)
                                {
                                    uint srcWrite = (uint)gpioMode3_0 + ((uint)gpioMode7_4 << 4) + ((uint)gpioMode11_8 << 8) + ((uint)gpioMode15_12 << 12) + ((uint)gpioMode18_16 << 16);
                                    //Console.WriteLine("srcWrite: " + srcWrite);

                                    //gpioOe = (uint)(0x01 << shift);
                                    Link.Mykonos.setupGpio(gpioOe, gpioMode3_0, gpioMode7_4, gpioMode11_8, gpioMode15_12, gpioMode18_16);

                                    //Check GPIO direction control bit
                                    spiData2 = Link.spiRead(0xB20);
                                    spiData2 = (byte)((spiData2 & ~(usedMask & 0xFF)) | ((gpioOe & 0xFF) & (usedMask & 0xFF)));
                                    spiData1 = Link.spiRead(0xB20); Console.WriteLine("0xB20: " + spiData1.ToString("X"));
                                    Assert.AreEqual(spiData2, spiData1, "GPIO direction control bit 7-0 not as expected");

                                    spiData2 = Link.spiRead(0xB21);
                                    spiData2 = (byte)((spiData2 & ~((usedMask >> 8) & 0xFF)) | (((gpioOe >> 8) & 0xFF) & ((usedMask >> 8) & 0xFF)));
                                    spiData1 = Link.spiRead(0xB21); Console.WriteLine("0xB21: " + spiData1.ToString("X"));
                                    Assert.AreEqual(spiData2, spiData1, "GPIO direction control bit 15-8 not as expected");

                                    spiData2 = Link.spiRead(0xB22);
                                    spiData2 = (byte)((spiData2 & ~((usedMask >> 16) & 0xFF)) | (((gpioOe >> 16) & 0xFF) & ((usedMask >> 16) & 0xFF)));
                                    spiData1 = Link.spiRead(0xB22); Console.WriteLine("0xB22: " + spiData1.ToString("X"));
                                    Assert.AreEqual(spiData2, spiData1, "GPIO direction control bit 18-16 not as expected");


                                    //Check GPIO
                                    spiData1 = Link.spiRead(0xB29); Console.WriteLine("0xB29: " + spiData1.ToString("X"));
                                    Assert.AreEqual((srcWrite & 0xFF), spiData1, "GPIO Lower byte not as expected");

                                    spiData1 = Link.spiRead(0xB2A); Console.WriteLine("0xB2A: " + spiData1.ToString("X"));
                                    Assert.AreEqual(((srcWrite >> 8) & 0xFF), spiData1, "GPIO Upper byte not as expected");

                                    spiData1 = Link.spiRead(0xB2B); Console.WriteLine("0xB2B: " + spiData1.ToString("X"));
                                    Assert.AreEqual(((srcWrite >> 16) & 0x07), spiData1, "GPIO Extra bits not as expected");
                                }
                            }
                        }
                    }
                }
            }
            Link.Disconnect();
        }


        ///<summary>
        /// API Test Name: 
        ///     testSetGPIOMonitor
        /// API Under-Test: 
        ///     MYKONOS_setGpioMonitorOut
        ///     
        /// API Test Description: 
        ///  Setup GPIO to have pins 3_0 and 7_4 to be \
        ///  GPIO_MONITOR_MODE and enable those bits
        ///  Call MYKONOS_setGpioMonitorOut 
        ///  Read back the Monitor Index and Monitor Mask
        ///  Values from the register
        ///  Check that they are the same as the values set
        /// API Test Pass Criteria: 
        /// Verify that the written values
        /// are the same as the read values
        /// Notes:
        ///</summary>
        [Test]
        [Category("GPIO")]
        public static void testSetGPIOMonitor()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            byte monitorIndex;
            int monitorMask;
            Link.Mykonos.setupGpio(0x00FF, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE);

            for (monitorIndex = 0; monitorIndex <= 0x35; monitorIndex++)
            {
                for (monitorMask = 0; monitorMask <= 0xFF; monitorMask++)
                {
                    Link.Mykonos.setGpioMonitorOut(monitorIndex, (byte)monitorMask);

                    spiData1 = Link.spiRead(0xB40); Console.WriteLine("0xB40: " + spiData1.ToString("X"));
                    Assert.AreEqual((monitorIndex), spiData1, "Monitor Index not as expected");

                    spiData1 = Link.spiRead(0xB41); Console.WriteLine("0xB41: " + spiData1.ToString("X"));
                    Assert.AreEqual((monitorMask), spiData1, "Monitor Mask not as expected");
                }
            }
            Link.Disconnect();
        }


        ///<summary>
        /// API Test Name: 
        ///     testGetGPIOMonitor
        /// API Under-Test: 
        ///     MYKONOS_getGpioMonitorOut
        ///     
        /// API Test Description: 
        ///  Setup GPIO to have pins 3_0 and 7_4 to be \
        ///  GPIO_MONITOR_MODE and enable those bits
        ///  Manually setup the GPIO monitor by writing
        ///  the Monitor Mask and Index to the corresponding
        ///  registers.
        ///  Call MYKONOS_getGpioMonitorOut to verify that the
        ///  return values are the same as the written values.
        /// API Test Pass Criteria: 
        /// Verify that the written values
        /// are the same as the read values
        /// Notes:
        ///</summary>
        [Test]
        [Category("GPIO")]
        public static void testGetGPIOMonitor()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            byte monitorIndex;
            int monitorMask;
            byte retmonitorIndex = 0;
            byte retmonitorMask = 0;
            Link.Mykonos.setupGpio(0x00FF, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE);

            for (monitorIndex = 0; monitorIndex <= 0x35; monitorIndex++)
            {
                //Console.WriteLine("monitorIndex: " + monitorIndex);
                for (monitorMask = 0; monitorMask <= 0xFF; monitorMask++)
                {
                    Console.WriteLine("monitorIndex: " + monitorIndex);
                    Console.WriteLine("monitorMask: " + monitorMask);
                    Link.spiWrite(0xB40, monitorIndex);
                    Link.spiWrite(0xB41, (byte)monitorMask);
                    
                    
                    Link.Mykonos.getGpioMonitorOut(ref retmonitorIndex, ref retmonitorMask);

                    Assert.AreEqual(monitorIndex, retmonitorIndex, "Monitor Index not as expected");

                    Assert.AreEqual(monitorMask, retmonitorMask, "Monitor Mask not as expected");
                }
                //monitorIndex++;
            }
            Link.Disconnect();
        }

        ///<summary>
        /// API Test testgetGpio3v3SourceCtrl: 
        ///     testgetGPIOMonitor
        /// API Under-Test: 
        ///     MYKONOS_getGpio3v3SourceCtrl
        ///     
        /// API Test Description: 
        ///  Setup GPIO to have pins 3_0 and 7_4 to be \
        ///  GPIO_MONITOR_MODE and enable those bits
        ///  Manually setup the GPIO monitor by writing
        ///  the Monitor Mask and Index to the corresponding
        ///  registers.
        ///  Call MYKONOS_getGpioMonitorOut to verify that the
        ///  return values are the same as the written values.
        /// API Test Pass Criteria: 
        /// Verify that the written values
        /// are the same as the read values
        /// Notes:
        ///</summary>
        [Test]
        [Category("GPIO")]
        public static void testgetGpio3v3SourceCtrl()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            UInt16 gpio3v3Oe = 0x0;
            UInt16 readGpioSrc = 0;

            foreach (Mykonos.GPIO3V3_MODE gpio3v3Mode3_0 in Enum.GetValues(typeof(Mykonos.GPIO3V3_MODE)))
            {
                foreach (Mykonos.GPIO3V3_MODE gpio3v3Mode7_4 in Enum.GetValues(typeof(Mykonos.GPIO3V3_MODE)))
                {
                    foreach (Mykonos.GPIO3V3_MODE gpio3v3Mode11_8 in Enum.GetValues(typeof(Mykonos.GPIO3V3_MODE)))
                    {

                        Link.spiWrite(0xB06, (byte)(((byte)gpio3v3Mode3_0 & 0x0F) | (((byte)gpio3v3Mode7_4 & 0x0F) << 4)));
                        Link.spiWrite(0xB07, (byte)((byte)gpio3v3Mode11_8 & 0x0F));


                        Link.Mykonos.getGpio3v3SourceCtrl(ref readGpioSrc);
                        //spiData1 = Link.spiRead(0xB06); Console.WriteLine("0xB06: " + spiData1.ToString("X"));
                        Assert.AreEqual(gpio3v3Mode3_0, (Mykonos.GPIO3V3_MODE)(readGpioSrc & 0x0F), "lower byte lower nibble readback not as expexted");

                        //spiData1 = Link.spiRead(0xB06); Console.WriteLine("0xB06: " + spiData1.ToString("X"));
                        Assert.AreEqual(gpio3v3Mode7_4, (Mykonos.GPIO3V3_MODE)((readGpioSrc & 0xF0) >> 4), "lower  byte upper nibble  readback not as expexted");

                        //spiData1 = Link.spiRead(0xB07); Console.WriteLine("0xB07: " + spiData1.ToString("X"));
                        Assert.AreEqual(gpio3v3Mode11_8, (Mykonos.GPIO3V3_MODE)((readGpioSrc & 0xF00) >> 8), "upper byte lower nibble readback not as expexted");

                    }
                }
            }
            Link.Disconnect();
        }



        ///<summary>
        /// API Test Name: 
        ///     testsetGpio3v3SourceCtrl
        /// API Under-Test: 
        ///     MYKONOS_setGpio3v3SourceCtrl
        ///     
        /// API Test Description: 
        ///  Iterate throuh all combinations for GIO3v3 Mode for
        ///  pin sets [3:0], [7:4] and [11:8] with 
        ///  MYKONOS_setGpio3v3SourceCtrl(). Readback the registers 
        ///  0xB06 and 0xB07 to verify that the pins are set.
        /// API Test Pass Criteria: 
        /// Verify that the written values
        /// are the same as the read values
        /// Notes:
        ///</summary>
        [Test]
        [Category("GPIO")]
        public static void testsetGpio3v3SourceCtrl()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            UInt16 gpio3v3Oe = 0x0;
            UInt16 readGpioSrc = 0;
            
            foreach (Mykonos.GPIO3V3_MODE gpio3v3Mode3_0 in Enum.GetValues(typeof(Mykonos.GPIO3V3_MODE)))
            {
                foreach (Mykonos.GPIO3V3_MODE gpio3v3Mode7_4 in Enum.GetValues(typeof(Mykonos.GPIO3V3_MODE)))
                {
                    foreach (Mykonos.GPIO3V3_MODE gpio3v3Mode11_8 in Enum.GetValues(typeof(Mykonos.GPIO3V3_MODE)))
                    {

                        Link.Mykonos.setGpio3v3SourceCtrl((UInt16)(((byte)gpio3v3Mode3_0 & 0x0F) | (((byte)gpio3v3Mode7_4 & 0x0F) << 4) | (((byte)gpio3v3Mode11_8 & 0x0F) << 8)));

                        Link.spiWrite(0xB06, (byte)(((byte)gpio3v3Mode3_0 & 0x0F) | (((byte)gpio3v3Mode7_4 & 0x0F) << 4)));
                        Link.spiWrite(0xB07, (byte)((byte)gpio3v3Mode11_8 & 0x0F));

                        spiData1 = Link.spiRead(0xB06); Console.WriteLine("0xB06: " + spiData1.ToString("X"));
                        Assert.AreEqual(gpio3v3Mode3_0, (Mykonos.GPIO3V3_MODE)(spiData1 & 0x0F), "lower byte lower nibble readback not as expexted");

                        spiData1 = Link.spiRead(0xB06); Console.WriteLine("0xB06: " + spiData1.ToString("X"));
                        Assert.AreEqual(gpio3v3Mode7_4, (Mykonos.GPIO3V3_MODE)((spiData1 & 0xF0) >> 4), "lower  byte upper nibble  readback not as expexted");

                        spiData1 = Link.spiRead(0xB07); Console.WriteLine("0xB07: " + spiData1.ToString("X"));
                        Assert.AreEqual(gpio3v3Mode11_8, (Mykonos.GPIO3V3_MODE)(spiData1 & 0x00F), "upper byte lower nibble readback not as expexted");

                    }
                }
            }
            Link.Disconnect();
        }


    }
}
