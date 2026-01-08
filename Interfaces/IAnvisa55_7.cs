using TrackNTrace.Repository.Entities;
using TrackNTrace.Repository.StoredProcedures;
using TrackNTrace.WebServices.com.Models;

namespace TrackNTrace.WebServices.com.Interfaces
{
    public interface IAnvisa55_7 : IDisposable
    {
        List<ValidationErrorModelView> FullPack(SaveMultiCottonPackModelView SaveMultiCottonPackModelView, Batch_Management_Registration_Reports_RetrieveBatchForAPI BatchRegistration, ScanPackagingWorkorderRegistration AggregatedBatch);
        List<ValidationErrorModelView> LoosePack(SaveMultiCottonPackModelView SaveMultiCottonPackModelView, Batch_Management_Registration_Reports_RetrieveBatchForAPI BatchRegistration, ScanPackagingWorkorderRegistration AggregatedBatch);
        List<ValidationErrorModelView> LoosePackNonSSC(SaveMultiCottonPackModelView SaveMultiCottonPackModelView, Batch_Management_Registration_Reports_RetrieveBatchForAPI BatchRegistration, ScanPackagingWorkorderRegistration AggregatedBatch);
    }
}
 