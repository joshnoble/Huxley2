﻿// © James Singleton. EUPL-1.2 (see the LICENSE file for the full license governing this code).

using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace OpenLDBWS
{
    public partial class BaseServiceItem
    {
        // new service ID format: 0000000XXXXXXXX, where 0 is any numeric char
        // and X is any alpha char or an underscore. So far, all service IDs
        // appear to end in at least one underscore, but I am uncertain if this
        // is always the case

        // underscores are already URL-safe, so we do not need to encode them
        // (and in fact WebUtility.UrlEncode will not change this string)
        public string ServiceIdPercentEncoded => serviceIDField;

        public Guid ServiceIdGuid => ToGuid(serviceIDField);

        // note that while ServiceID is already URL-safe, the underscore char
        // is not valid base64, so any inputs that previously assumed b64 input
        // may break. As such, we will encode the string to be b64-safe as well
        // as URL-safe
        [SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "Not a URL")]
        public string ServiceIdUrlSafe => WebEncoders.Base64UrlEncode(
            System.Text.Encoding.UTF8.GetBytes(serviceIDField)
        );

        public static Guid ToGuid(string serviceID)
        {
            // because we have 128 bits to use in the GUID (and only ~56 bits
            // from the service ID), we can afford to encode the 7 numeric
            // characters directly and the 8 alpha characters as ASCII instead
            // of finding a more efficient solution, and zero-pad the centre
            string num = serviceID.Substring(0, 7); // digits
            string str = serviceID.Substring(7, 8); // letters
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            string hexString = Convert.ToHexString(bytes);
            string guidString = num + "000000000" + hexString.Substring(0, 16);
            return new Guid(guidString);
        }

        public static string FromGuid(Guid serviceGuid)
        {
            // reverse of ToGuid above
            string guidString = serviceGuid.ToString("N", CultureInfo.InvariantCulture);
            string num = guidString.Substring(0, 7); // get digits
            string hexString = guidString.Substring(16, 16); // get letters
            byte[] bytes = Convert.FromHexString(hexString);
            string str = System.Text.Encoding.UTF8.GetString(bytes);
            return num + str;
        }
    }
}
