using System;
using System.Diagnostics;


using NUnit.Framework;
using AdiCmdServerClient;

namespace mykonosUnitTest
{
    [Category("ApiFunctional")]
    public class ProfileTests
    {
        public static TestSetupConfig settings = new TestSetupConfig();
        public static  UInt16[,] adcProfileLut = new UInt16[15, 18]
            { /* Max RFBW, ADCCLK_MHz, adcProfile[16]*/
                { 60,  983, 940, 557, 195, 100, 1280, 191, 1889, 165, 1035,  69,  793, 39, 47, 25, 19, 207},
                { 75,  983, 905, 556, 190, 101, 1280, 286, 1889, 256, 1011, 107,  791, 36, 46, 24, 19, 205},
                {100,  983, 885, 556, 196,  98, 1280, 495, 1897, 453, 1050, 202,  745, 32, 44, 24, 18, 203},
                { 75, 1228, 752, 455, 193, 101, 1280, 191, 1929, 168, 1284,  86, 1001, 39, 48, 31, 24, 207},
                {100, 1228, 736, 468, 191,  99, 1280, 331, 1988, 306, 1272, 153,  956, 36, 48, 30, 23, 205},
                {160, 1228, 644, 518, 193,  99, 1280, 733, 2229, 856, 1303, 423,  904, 25, 48, 28, 22, 199},
                {200, 1228, 452, 384, 199, 105, 1280, 694, 1826, 854, 1493, 626,  997, 23, 48, 38, 29, 227},
                { 75, 1474, 636, 378, 192,  98, 1280, 166, 1919, 144, 1526,  87, 1169, 40, 48, 37, 28, 207},
                {100, 1474, 621, 383, 193,  99, 1280, 233, 1947, 209, 1543, 128, 1167, 38, 48, 37, 28, 206},
                {150, 1474, 590, 404, 191,  98, 1280, 495, 2070, 494, 1532, 294, 1118, 32, 48, 35, 27, 203},
                { 40, 1536, 609, 362, 194, 101, 1280, 160, 1917, 139, 1607,  89, 1252, 40, 48, 39, 30, 207},
                {100, 1536, 592, 366, 190, 102, 1280, 214, 1939, 192, 1580, 121, 1251, 38, 48, 38, 30, 206},
                {150, 1536, 556, 385, 192, 101, 1280, 449, 2051, 452, 1608, 283, 1201, 32, 48, 37, 29, 203},
                {200, 1536, 516, 414, 193, 101, 1280, 733, 2229, 856, 1629, 529, 1150, 25, 48, 35, 28, 199},
                {240, 1536, 477, 299, 194, 101, 1280, 967, 1624, 878, 1660, 803, 1100, 19, 32, 33, 27, 194}
            };

        [Test, Combinatorial]
        public static void CheckOrxAdcPrForWithValidTxRxPr([Values("Tx 75/200MHz, IQrate 245.76MHz, Dec5", "Tx 20/100MHz, IQrate  122.88MHz, Dec5")]string TxProfile,
                                          [Values("Rx 20MHz, IQrate 30.72MHz, Dec5", "Rx 40MHz, IQrate 61.44MHz, Dec5", "Rx 100MHz, IQrate 122.88MHz, Dec5")]string RxProfile,
                                          [Values("ORX 100MHz, IQrate 122.88MHz, Dec5", "ORX 200MHz, IQrate 245.75MHz, Dec5")]string OrxProfile)
        {
           settings.mykSettings.rxProfileName = RxProfile;
           settings.mykSettings.txProfileName = TxProfile;
           settings.mykSettings.orxProfileName = OrxProfile;

           //Call Test Setup
           TestSetup.AdcProfileTestSetupInit(settings, true, true);

           AdiCommandServerClient Link = AdiCommandServerClient.Instance;
           Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

           
            //Read Orx Profile settings from ARM
            string LBADCProfile = Helper.readArmAdcProfilesStruct();

            //Calculate Expected OrxProfile Settings: Expect Tx Profile to be used
            Mykonos.VCODIV vcoDiv;
            UInt32 vcoDivTimes10 = 10;

            vcoDiv = settings.txProfileData.ClkPllVcoDiv;
            switch (vcoDiv)
            {
                case Mykonos.VCODIV.VCODIV_1: vcoDivTimes10 = 10; break;
                case Mykonos.VCODIV.VCODIV_1p5: vcoDivTimes10 = 15; break;
                case Mykonos.VCODIV.VCODIV_2: vcoDivTimes10 = 20; break;
                case Mykonos.VCODIV.VCODIV_3: vcoDivTimes10 = 30; break;
                default: vcoDivTimes10 = 10; break;
            }
            Console.Write("test" + settings.txProfileData.ClkPllFrequency_kHz);
            UInt64 hsDigClk_MHz = settings.txProfileData.ClkPllFrequency_kHz / vcoDivTimes10 / 100 / settings.txProfileData.ClkPllHsDiv;
            double ADCClk_MHz = hsDigClk_MHz / settings.obsRxProfileData.adcDacDiv;
            UInt16 adcClk_MHz = (UInt16)ADCClk_MHz;
            
            //Find Expected Orx Profile 
            int profileIndex = 15;
            int i;
            for (i = 0; i < 15; i++)
            {
                /* Find a row in the LUT that matches the ADC clock frequency */
                if ((adcProfileLut[i,1] == adcClk_MHz) && (adcProfileLut[i,0] >= (settings.txProfileData.PrimarySigBw_Hz / 1000000)))
                {
                    profileIndex = i;
                    break;
                }
            }
            //Verify Result against Profile Set in ARM
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (i = 0; i < 16; i++)
            {
                sb.Append((adcProfileLut[profileIndex,i+2].ToString()) + ", ");
            }

            Assert.AreEqual(sb.ToString(), LBADCProfile, "Loopback ADC profile not as expected");
            Console.WriteLine(LBADCProfile);
        }

        //[Test, Combinatorial] From Arm v)0.5 Loopback Profile Not loaded unless Tx Profile Valid
        public static void CheckLoopbackAdcPrForWithInvalidTxProfile([Values("Tx 75/200MHz, IQrate 245.76MHz, Dec5")]string TxProfile,
                                          [Values("Rx 20MHz, IQrate 30.72MHz, Dec5", "Rx 40MHz, IQrate 61.44MHz, Dec5", "Rx 100MHz, IQrate 122.88MHz, Dec5")]string RxProfile,
                                          [Values("ORX 100MHz, IQrate 122.88MHz, Dec5", "ORX 200MHz, IQrate 245.75MHz, Dec5")]string OrxProfile)
        {
            settings.mykSettings.rxProfileName = RxProfile;
            settings.mykSettings.txProfileName = TxProfile;
            settings.mykSettings.orxProfileName = OrxProfile;
            settings.mykSettings.txChannel = Mykonos.TXCHANNEL.TXOFF;
            //Call Test Setup
            TestSetup.AdcProfileTestSetupInit(settings, false, true);

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            //Read Orx Profile settings from ARM
            string LBADCProfile = Helper.readArmAdcProfilesStruct();

            Mykonos.VCODIV vcoDiv;
            UInt32 vcoDivTimes10 = 10;
            
            //Calculate Expected OrxProfile Settings: Expect Rx Profile to be used
            vcoDiv = settings.rxProfileData.ClkPllVcoDiv;
            switch (vcoDiv)
            {
                case Mykonos.VCODIV.VCODIV_1: vcoDivTimes10 = 10; break;
                case Mykonos.VCODIV.VCODIV_1p5: vcoDivTimes10 = 15; break;
                case Mykonos.VCODIV.VCODIV_2: vcoDivTimes10 = 20; break;
                case Mykonos.VCODIV.VCODIV_3: vcoDivTimes10 = 30; break;
                default: vcoDivTimes10 = 10; break;
            }
            Console.Write("Test" + settings.rxProfileData.ClkPllFrequency_kHz);
            uint hsDigClk_MHz = settings.rxProfileData.ClkPllFrequency_kHz / vcoDivTimes10 / 100 / settings.rxProfileData.ClkPllHsDiv;
            Console.Write("Test" + settings.rxProfileData.adcDacDiv);
            double ADCClk_MHz = hsDigClk_MHz / settings.rxProfileData.adcDacDiv;
            Console.Write("Test" + ADCClk_MHz);
            UInt16 adcClk_MHz = (UInt16)ADCClk_MHz;
            Console.Write("Test" + (settings.rxProfileData.RfBw_Hz / 1000000));


            int profileIndex = 15;
            int i;
            for (i = 0; i < 15; i++)
            {
                Console.Write("Index" + i + " " + adcProfileLut[i, 1] + " " + adcProfileLut[i, 0]);
                /* Find a row in the LUT that matches the ADC clock frequency */
                if ((adcProfileLut[i, 1] == adcClk_MHz) && (adcProfileLut[i, 0] >= (settings.rxProfileData.RfBw_Hz / 1000000)))
                {
                    profileIndex = i;
                    break;
                }
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (i = 0; i < 16; i++)
            {
                sb.Append((adcProfileLut[profileIndex, i + 2].ToString()) + ", ");
            }
            Console.WriteLine(LBADCProfile);
            Assert.AreEqual(sb.ToString(), LBADCProfile, "Loopback ADC profile not as expected");

        }

        [Test, Combinatorial]
        public static void CheckOrxAdcPrForNoRxProfile([Values("Tx 75/200MHz, IQrate 245.76MHz, Dec5", "Tx 20/100MHz, IQrate  122.88MHz, Dec5")]string TxProfile,
                                          [Values("Rx 20MHz, IQrate 30.72MHz, Dec5")]string RxProfile,
                                          [Values("ORX 100MHz, IQrate 122.88MHz, Dec5", "ORX 200MHz, IQrate 245.75MHz, Dec5")]string OrxProfile)
        {

            settings.mykSettings.rxProfileName = RxProfile;
            settings.mykSettings.txProfileName = TxProfile;
            settings.mykSettings.orxProfileName = OrxProfile;

            //Call Test Setup
            TestSetup.AdcProfileTestSetupInit(settings, true, false);

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            //Read Orx Profile settings from ARM
            string LBADCProfile = Helper.readArmAdcProfilesStruct();

            //Calculate Expected OrxProfile Settings: Expect Tx Profile to be used
            Mykonos.VCODIV vcoDiv;
            UInt32 vcoDivTimes10 = 10;

            vcoDiv = settings.txProfileData.ClkPllVcoDiv;
            switch (vcoDiv)
            {
                case Mykonos.VCODIV.VCODIV_1: vcoDivTimes10 = 10; break;
                case Mykonos.VCODIV.VCODIV_1p5: vcoDivTimes10 = 15; break;
                case Mykonos.VCODIV.VCODIV_2: vcoDivTimes10 = 20; break;
                case Mykonos.VCODIV.VCODIV_3: vcoDivTimes10 = 30; break;
                default: vcoDivTimes10 = 10; break;
            }
            UInt64 hsDigClk_MHz = settings.txProfileData.ClkPllFrequency_kHz / vcoDivTimes10 / 100 / settings.txProfileData.ClkPllHsDiv;
            double ADCClk_MHz = hsDigClk_MHz / settings.obsRxProfileData.adcDacDiv;
            UInt16 adcClk_MHz = (UInt16)ADCClk_MHz;

            int profileIndex = 15;
            int i;
            for (i = 0; i < 15; i++)
            {
                /* Find a row in the LUT that matches the ADC clock frequency */
                if ((adcProfileLut[i, 1] == adcClk_MHz) && (adcProfileLut[i, 0] >= (settings.txProfileData.PrimarySigBw_Hz / 1000000)))
                {
                    profileIndex = i;
                    break;
                }
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (i = 0; i < 16; i++)
            {
                sb.Append((adcProfileLut[profileIndex, i + 2].ToString()) + ", ");
            }

            Assert.AreEqual(sb.ToString(), LBADCProfile, "Loopback ADC profile not as expected");
            Console.WriteLine(LBADCProfile);
        }
    }
}
