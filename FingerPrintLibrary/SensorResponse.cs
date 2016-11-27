using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FingerPrintLibrary
{
    public class SensorResponse
    {
        #region Constructors
        public SensorResponse()
        { }

        /// <summary>
        /// Sets Success and ErrorMessage based on the response code.
        /// </summary>
        /// <param name="responseCode">Response code from sensor return data packet.</param>
        public SensorResponse(byte responseCode)
        {
            ResponseCode = responseCode;
            if (responseCode != SensorCodes.OK)
            {
                string message;
                if (SensorCodes.ConfirmationCodes.TryGetValue(ResponseCode, out message))
                {
                    ErrorMessage = message;
                }

                Success = false;
            }
            else
            {
                ErrorMessage = string.Empty;
                Success = true;
            }
        }

        /// <summary>
        /// Sets success based on response code. Sets error message to passed string + error message associated with response code.
        /// </summary>
        /// <param name="errorMessage">Message to prepend to error message from response code.</param>
        /// <param name="responseCode">Response code from sensor return data packet.</param>
        public SensorResponse(string errorMessage, byte responseCode) 
            : this(responseCode)
        {
            ErrorMessage = $"{errorMessage} {ErrorMessage}";
        }

        /// <summary>
        /// Sets success and error message manually. Byte left at default value.
        /// </summary>
        /// <param name="success">Success</param>
        /// <param name="errorMessage">Error message</param>
        public SensorResponse(bool success, string errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Sets success flag and leaves Response code at default value and sets Error message to empty string.
        /// </summary>
        /// <param name="success">Success</param>
        public SensorResponse(bool success)
        {
            Success = success;
            ErrorMessage = string.Empty;
        }
        #endregion

        public byte ResponseCode { get; set; }

        public string ErrorMessage { get; set; }

        public bool Success { get; set; }
    }

    public class SearchResponse : SensorResponse
    {
        public short PageNumber { get; set; }

        public short MatchLevel { get; set; }

        public SearchResponse(byte confirmationCode, short pageNumber, short matchLevel)
            : base(confirmationCode)
        {
            PageNumber = pageNumber;
            MatchLevel = matchLevel;
        }

        public SearchResponse(SensorResponse response)
        {
            PageNumber = 0;
            MatchLevel = 0;
            ResponseCode = response.ResponseCode;
            ErrorMessage = response.ErrorMessage;
            Success = response.Success;
        }
    }
}
