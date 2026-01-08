namespace TrackNTrace.WebServices.com.Models
{
    public class OptomechParentLabel
    {
        public int CustomerID { get; set; }
        public int AgencyID { get; set; }
        public string ProductCode { get; set; }
        public string BatchNumber { get; set; }
        public string GtinNumber { get; set; }
        public string ParentLabel { get; set; }
        public string SerialNumber { get; set; }
        public string ExpiryDate { get; set; }
        public int? ChildLevelID { get; set; }
        public int? ParentLevelID { get; set; }
        public int? Quantity { get; set; }
        public int LevelType { get; set; }
    }
}
