﻿using System;
using Dynamsoft;
using Dynamsoft.DBR;

namespace SpeedFirstSettings
{
    class Program
    {
        static public void configSpeedFirst(ref BarcodeReader dbr)
        {
            // Obtain current runtime settings of instance.
            PublicRuntimeSettings settings = dbr.GetRuntimeSettings();
            
            // Parameter 1. Set expected barcode formats
            // The simpler barcode format, the faster decoding speed.
            // Here we use OneD barcode format to demonstrate.
            settings.BarcodeFormatIds = (int)EnumBarcodeFormat.BF_EAN_13;
            
            // Parameter 2. Set expected barcode count
            // The less barcode count, the faster decoding speed.
            settings.ExpectedBarcodesCount = 1;
            
            // Parameter 3. Set the threshold for the image shrinking for localization.
            // The smaller the threshold, the smaller the image shrinks.  The default value is 2300.
            settings.ScaleDownThreshold = 1200;

            // Parameter 4. Set the binarization mode for convert grayscale image to binary image.
            // Mostly, the less binarization modes set, the faster decoding speed.
            settings.BinarizationModes[0] = EnumBinarizationMode.BM_LOCAL_BLOCK;

            // Parameter 5. Set localization mode.
            // LM_SCAN_DIRECTLY: Localizes barcodes quickly. It is both for OneD and TweD barcodes. This mode is recommended in interactive scenario.
            settings.LocalizationModes[0] = EnumLocalizationMode.LM_SCAN_DIRECTLY;
            settings.LocalizationModes[1] = EnumLocalizationMode.LM_SKIP;
            settings.LocalizationModes[2] = EnumLocalizationMode.LM_SKIP;
            settings.LocalizationModes[3] = EnumLocalizationMode.LM_SKIP;

            // LM_ONED_FAST_SCAN: Localizing barcodes quickly. However, it is only for OneD barcodes. It is also recommended in interactive scenario.
        
            // Parameter 6. Reduce deblurModes setting
            // DeblurModes will improve the readability and accuracy but decrease the reading speed.
            // Please update your settings here if you want to enable different Deblurmodes.
            settings.DeblurModes[0] = EnumDeblurMode.DM_BASED_ON_LOC_BIN;
            settings.DeblurModes[1] = EnumDeblurMode.DM_THRESHOLD_BINARIZATION;

            // Parameter 7. Set timeout(ms) if the application is very time sensitive.
            // If timeout, the decoding thread will exit at the next check point.
            settings.Timeout = 100;
            
            dbr.UpdateRuntimeSettings(settings);

            // Other potentially accelerated arguments of various modes.

            // Argument 4.a Disable the EnableFillBinaryVacancy argument.
            // Local block binarization process might cause vacant area in barcode. The barcode reader will fill the vacant black by default (default value 1). Set the value 0 to disable this process.
            string strErrorMessage;
            dbr.SetModeArgument("BinarizationModes", 0, "EnableFillBinaryVacancy", "0", out strErrorMessage);
            
            // Argument 5.a Sets the scan direction when searching barcode.
            // It is valid only when the type of LocalizationMode is LM_ONED_FAST_SCAN or LM_SCAN_DIRECTLY.
            // 0: Both vertical and horizontal direction.
            // 1: Vertical direction.
            // 2: Horizontal direction.
            // Read more about localization mode members: https://www.dynamsoft.com/barcode-reader/parameters/enum/parameter-mode-enums.html?ver=latest#localizationmode
            dbr.SetModeArgument("LocalizationModes", 0, "ScanDirection", "0", out strErrorMessage);
        }
        static public void configSpeedFirstByTemplate(ref BarcodeReader dbr)
        {
            // Compared with PublicRuntimeSettings, parameter templates have a richer ability to control parameter details.
		    // Please refer to the parameter explanation in "SpeedFirstTemplate.json" to understand how to control speed first.
            string strErrorMessage;
            EnumErrorCode ret = dbr.InitRuntimeSettingsWithFile("SpeedFirstTemplate.json", EnumConflictMode.CM_OVERWRITE, out strErrorMessage);
        }

        static public void outputResults(TextResult[] results, long costTime)
        {
            Console.WriteLine("Cost time:{0}ms", costTime);
            if (results != null && results.Length > 0)
            {
                int i = 1;
                foreach (TextResult result in results)
                {
                    string barcodeFormat = result.BarcodeFormat == 0 ? result.BarcodeFormatString_2 : result.BarcodeFormatString;
                    Console.WriteLine("Barcode {0}:{1},{2}", i, barcodeFormat, result.BarcodeText);
                    i++;
                }
            }
            else
            {
                Console.WriteLine("No data detected~");
            }
        }
        static void Main(string[] args)
        {
            try
            {
                // Initialize license
                /*
                // By setting organization ID as "200001", a 7-day trial license will be used for license verification.
                // Note that network connection is required for this license to work.
                //
                // When using your own license, locate the following line and specify your Organization ID.
                // organizationID = "200001";
                //
                // If you don't have a license yet, you can request a trial from https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=samples&package=dotnet
                */
                DMDLSConnectionParameters connectionInfo = BarcodeReader.InitDLSConnectionParameters();
                connectionInfo.OrganizationID = "200001";
                EnumErrorCode errorCode = BarcodeReader.InitLicenseFromDLS(connectionInfo, out string errorMsg);
                if (errorCode != EnumErrorCode.DBR_SUCCESS)
                {
                    Console.WriteLine(errorMsg);
                }

                // Create an instance of Barcode Reader
                BarcodeReader dbr = new BarcodeReader();
                TextResult[] results = null;
                string fileName = "../../../../../images/AllSupportedBarcodeTypes.png";

                // There are two ways to configure runtime parameters. One is through PublicRuntimeSettings, the other is through parameters template.
                Console.WriteLine("Decode through PublicRuntimeSettings:");
                {   
                    // config through PublicRuntimeSettings
                    configSpeedFirst(ref dbr);
                    
                    DateTime beforeRead = DateTime.Now;
                    
                    // Decode barcodes from an image file by current runtime settings. The second parameter value "" means to decode through the current PublicRuntimeSettings.
                    results = dbr.DecodeFile(fileName, "");
                    
                    DateTime afterRead = DateTime.Now;
                    
                    int timeElapsed = (int)(afterRead - beforeRead).TotalMilliseconds;
                    
                    // Output the barcode format and barcode text.
                    outputResults(results, timeElapsed);
                }
                
                Console.WriteLine("\r\n");

                Console.WriteLine("Decode through parameters template:");
                {
                    // config through parameters template
                    configSpeedFirstByTemplate(ref dbr);
                    
                    DateTime beforeRead = DateTime.Now;
                    
                    // Decode barcodes from an image file by template.
                    results = dbr.DecodeFile(fileName, "");
                    
                    DateTime afterRead = DateTime.Now;
                    
                    int timeElapsed = (int)(afterRead - beforeRead).TotalMilliseconds;
                    
                    // Output the barcode format and barcode text.
                    outputResults(results, timeElapsed);
                } 
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            Console.WriteLine("Press any key to quit...");
            Console.ReadKey();
        }
    }
}
