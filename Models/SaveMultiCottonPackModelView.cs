namespace TrackNTrace.WebServices.com.Models
{
    public class SaveMultiCottonPackModelView
    {
        public int ICustomerID { get; set; }
        public int IAgencyID { get; set; }
        public int IPackType { get; set; }
        public string SProductCode { get; set; }
        public string SBatchNum { get; set; }
        public string SBarcodeList { get; set; }
        public int IChildLevelID { get; set; }
        public int? IParentLevelID { get; set; }
        public int ILevelID { get; set; }
        public int IProductID { get; set; }
        public int? IUserID { get; set; }
        public int? ITerminalID { get; set; }
        public string? SBarcode2D { get; set; }
    }
}
