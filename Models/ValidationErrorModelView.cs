using System.Runtime.Serialization;

namespace TrackNTrace.WebServices.com.Models
{
    public class ValidationErrorModelView
    {
        public int ErrorCode { get; set; }
        [IgnoreDataMember]
        public string ErrorMessage { get; set; }
        public string ErrorValue { get; set; } 

    }
}
