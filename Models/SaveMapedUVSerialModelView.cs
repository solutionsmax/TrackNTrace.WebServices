namespace TrackNTrace.WebServices.com.Models
{
    public class SaveMapedUVSerialModelView
    {
        public int ICustomerID { get; set; }
        public int IAgencyID { get; set; }
        public string SProductCode { get; set; }
        public string SBatchNum { get; set; }
        public string SBarcode2D { get; set; }
        public string SUVSerial { get; set; }
        public int IChildLevelID { get; set; }
        public int IParentLevelID { get; set; }
        public int ILevelID { get; set; }
    }
}
