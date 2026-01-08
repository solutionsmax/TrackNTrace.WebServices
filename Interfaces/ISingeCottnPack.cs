using TrackNTrace.WebServices.com.Models;

namespace TrackNTrace.WebServices.com.Interfaces
{
    public interface ISingeCottnPack:IDisposable
    {
        int SaveMapedUVSerials(SaveMapedUVSerialModelView saveMapedUVSerials);
    }
}
