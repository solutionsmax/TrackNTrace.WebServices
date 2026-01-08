
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using TrackNTrace.Repository.Common;
using TrackNTrace.Repository.Entities;
using TrackNTrace.Repository.Interfaces;
using TrackNTrace.Repository.StoredProcedures;
using TrackNTrace.Repository.Utilities;
using TrackNTrace.WebServices.com.AgencyValidation;

using TrackNTrace.WebServices.com.Interfaces;
using TrackNTrace.WebServices.com.Models;

namespace TrackNTrace.WebServices.com.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OptomechController : ControllerBase
    {
        private readonly ISerializationHelper _SerializationHelper = null;
        private readonly IBatchHelper _BatchHelper;
        private readonly ISharedBLL _SharedBLL;
        private readonly ICommonAgency _ICommonAgency;
        private readonly INMPAChina86 _NMPAChina86;
        private readonly IAnvisa55_7 _Anvisa55_7;
        private readonly ISingeCottnPack _ISingeCottnPack;
        private readonly ILogger<OptomechController> _logger;
        IPackageLebelingHelper packageLebelingHelper;
        public OptomechController(ILogger<OptomechController> logger, IBatchHelper batchHelper,
            ISerializationHelper serializationHelper,
            ICommonAgency ICommonAgency, ISingeCottnPack ISingeCottnPack, IAnvisa55_7 anvisa55_7, INMPAChina86 INMPAChina86, ISharedBLL sharedBLL, IPackageLebelingHelper packageLebelingHelper)
        {
            _logger = logger;

            _SharedBLL = sharedBLL;
            _SerializationHelper = serializationHelper;
            _BatchHelper = batchHelper;
            this.packageLebelingHelper = packageLebelingHelper;
            _ICommonAgency = ICommonAgency;
            _NMPAChina86 = INMPAChina86;
            _Anvisa55_7 = anvisa55_7;
            _ISingeCottnPack = ISingeCottnPack;
        }
        [HttpGet("TestAPI", Name = "TestAPI")]
        public ActionResult<IEnumerable<string>> TestAPI()
        {


            return Ok("V.6.0");

        }
        [HttpGet("Retrieve2DBarcodesByBatchList", Name = "Retrieve2DBarcodesByBatchList")]
        public async Task<ActionResult<IEnumerable<string>>> Retrieve2DBarcodesByBatchList(int ICustomerID, int IAgencyID, string SProductCode, string SBatchNum, int ILevelID)
        {

            return Ok(await _SerializationHelper.Retrieve2DBarcodesByBatchList(SharedBLL.iGroupID, SharedBLL.iPlantID, ICustomerID, IAgencyID, SProductCode, SBatchNum, ILevelID));

        }

        [HttpGet("Fetch2DBarcodesByBatch", Name = "Fetch2DBarcodesByBatch")]
        public ActionResult<IEnumerable<string>> Fetch2DBarcodesByBatch(int ICustomerID, int IAgencyID, string SProductCode, string SBatchNum, int ILevelID)
        {

            return Ok(_SerializationHelper.Retrieve2DBarcodesByBatchList(SharedBLL.iGroupID, SharedBLL.iPlantID, ICustomerID, IAgencyID, SProductCode, SBatchNum, ILevelID).Result.OrderBy(x => Guid.NewGuid()).Take(1));
        }

        [HttpGet("RetrieveUVSerialsList", Name = "RetrieveUVSerialsList")]
        public async Task<ActionResult<IEnumerable<string>>> RetrieveUVSerialsList()
        {
            return Ok(await _SerializationHelper.RetrieveUVSerialsList(SharedBLL.iGroupID, SharedBLL.iPlantID));
        }
        [HttpGet("FetchUVSerialNumber", Name = "FetchUVSerialNumber")]
        public async Task<ActionResult<IEnumerable<string>>> FetchUVSerialNumber()
        {

            return Ok(_SerializationHelper.RetrieveUVSerialsList(SharedBLL.iGroupID, SharedBLL.iPlantID).Result.LastOrDefault());
        }
        [HttpGet("SaveMapedUVSerials", Name = "SaveMapedUVSerials")]
        public async Task<ActionResult<IEnumerable<string>>> SaveMapedUVSerials(SaveMapedUVSerialModelView saveMapedUVSerials)
        {

            return Ok(_ISingeCottnPack.SaveMapedUVSerials(saveMapedUVSerials));
        }

        [HttpGet("RetrieveCurrentBatchList", Name = "RetrieveCurrentBatchList")]
        public async Task<ActionResult<IEnumerable<string>>> RetrieveCurrentBatchList()
        {
            return Ok(await _BatchHelper.RetrieveCurrentBatchList(SharedBLL.iGroupID, SharedBLL.iPlantID));
        }

        [HttpPost("SaveMultiCottonSerials", Name = "SaveMultiCottonSerials")]
        public ActionResult<IEnumerable<string>> SaveMultiCottonSerials(SaveMultiCottonPackModelView saveMultiCottonPack)
        {
            try
            {
                List<ValidationErrorModelView> KeyValuePair = new List<ValidationErrorModelView>();
                var aggregatedBatch = _BatchHelper.RetrieveCurrentAggregatedBatch(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum);
                var batch = _BatchHelper.RetrieveBatchForAPI(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum).LastOrDefault();
                if (aggregatedBatch != null && batch != null)
                {
                    saveMultiCottonPack.ITerminalID = aggregatedBatch.TerminalId;
                    saveMultiCottonPack.IUserID = aggregatedBatch.UserId; ;
                    saveMultiCottonPack.IChildLevelID = (int)aggregatedBatch.ChildLevelId;
                    saveMultiCottonPack.IProductID = batch.PRODUCT_ID;
                    saveMultiCottonPack.IParentLevelID = aggregatedBatch.ParentLevelId;
                    switch (saveMultiCottonPack.IAgencyID)
                    {
                        case 7: //Anvisa(ANVIS=7)


                            switch (saveMultiCottonPack.IPackType)
                            {
                                case 1: // ful pack
                                    KeyValuePair.AddRange(_Anvisa55_7.FullPack(saveMultiCottonPack, batch, aggregatedBatch));
                                    break;
                                case 2: // loose pack
                                    KeyValuePair.AddRange(_Anvisa55_7.LoosePack(saveMultiCottonPack, batch, aggregatedBatch));
                                    break;
                                case 3: // loose pack with non ssc
                                    KeyValuePair.AddRange(_Anvisa55_7.LoosePackNonSSC(saveMultiCottonPack, batch, aggregatedBatch));
                                    break;
                            }
                            break;

                        case 9:

                            switch (saveMultiCottonPack.IPackType)
                            {
                                case 1: // ful pack
                                    KeyValuePair.AddRange(_NMPAChina86.FullPack(saveMultiCottonPack, batch, aggregatedBatch));
                                    break;
                                //case 2: // loose pack
                                //    KeyValuePair.AddRange(_NMPAChina86.LoosePack(saveMultiCottonPack, batch, aggregatedBatch));
                                //    break;
                                case 3: // loose pack with non ssc
                                    KeyValuePair.AddRange(_NMPAChina86.LoosePackNonSSC(saveMultiCottonPack, batch, aggregatedBatch));
                                    break;
                            }
                            break;
                        default: //USFDA,DGFT and MHRA (USFDA=1,DGFT=2 and MHRA=4)

                            switch (saveMultiCottonPack.IPackType)
                            {
                                case 1: // ful pack
                                    KeyValuePair.AddRange(_ICommonAgency.FullPack(saveMultiCottonPack, batch, aggregatedBatch));
                                    break;
                                case 2: // loose pack
                                    KeyValuePair.AddRange(_ICommonAgency.LoosePack(saveMultiCottonPack, batch, aggregatedBatch));
                                    break;
                                case 3: // loose pack with non ssc
                                    KeyValuePair.AddRange(_ICommonAgency.LoosePackNonSSC(saveMultiCottonPack, batch, aggregatedBatch));
                                    break;
                            }
                            break;


                    }
                }

                return Ok(KeyValuePair);
            }
            catch (Exception)
            {

                return BadRequest();
            }


        }

        [HttpGet("GetParentLabelsByBatch", Name = "GetParentLabelsByBatch")]
        public async Task<ActionResult<IEnumerable<string>>> GetParentLabelsByBatch(SaveMultiCottonPackModelView saveMultiCottonPack)
        {
            try
            {

                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                foreach (var item in await packageLebelingHelper.GetOptomechPalletLabels(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum))
                {
                    string sFNC1ParseLabel = _SharedBLL.RemoveFNC1Char(item.ParentLabel);
                    string sParseLabel = _SharedBLL.RemoveParentheses(sFNC1ParseLabel);
                    if (sParseLabel.Length == 20)
                    {
                        keyValuePairs.Add(item.ParentLabel, sParseLabel.Substring(2, sParseLabel.Length - 2));
                    }
                    else
                    {
                        ExtractPackLabel extractPackLabel = new ExtractPackLabel(saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, sParseLabel, _SharedBLL);
                        keyValuePairs.Add(item.ParentLabel, extractPackLabel._SERIAL_NUM);
                    }
                }

                var dictList = keyValuePairs.LastOrDefault(x => x.Value == saveMultiCottonPack.SBarcode2D);


                return Ok(dictList.Key);
            }
            catch (Exception ex)
            {

                _SharedBLL.LoggError(GetType().FullName, ex.Message, ex.InnerException.Message, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return BadRequest();
            }

        }

    }
}