using System;
using OSII.ConversionToolkit;

namespace edpesp_db.Data.EdpespDatabases.EdpespFep
{
    public static class EdpespFepExtensions
    {
        /// <summary>
        /// Helper function to get the name.
        /// Strips the leading substring of "RTU".
        /// </summary>
        /// <param name="rawName">Name value from the input.</param>
        /// <returns>Returns name without RTU on the front.</returns>
        public static string GetNameWithoutRTU(string rawName)
        {
            if (rawName.StartsWith("RTU"))
            {
                return rawName.Replace("RTU ", "");
            }
            if (rawName.StartsWith("FRTU"))
            {
                return rawName.Replace("FRTU ", "");
            }
            else
            {
                Logger.Log("INCORRECT NAME FORMAT", LoggerLevel.INFO, $"Provided Channel name does not fit pattern of 'RTU name':{rawName}\t Setting name to raw value.");
                return rawName;
            }
        }
    }
}
