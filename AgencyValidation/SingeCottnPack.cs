using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using TrackNTrace.Models.Enum;
using TrackNTrace.Repository.Common;
using TrackNTrace.WebServices.com.Models;
using TrackNTrace.Repository.Entities;
using TrackNTrace.Repository.Interfaces;
using TrackNTrace.Repository.StoredProcedures;
using TrackNTrace.Repository.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TrackNTrace.WebServices.com.Interfaces;

namespace TrackNTrace.WebServices.com.AgencyValidation
{
    public class SingeCottnPack: ISingeCottnPack
    {
        private bool _disposed = false;
        private readonly IPackagingLinesSetupHelper packagingLinesSetup;
        private readonly IPriorScanValidationHelper priorScanValidation;
        private readonly IGtinHelper gtinHelper;
  ExtractPackLabel extractPackLabel = null;
        private readonly IValidationRuleHelper validationRuleHelper;
        private readonly IPackageLebelingHelper packageLebelingHelper;
        private readonly IPackagingPackSplit packagingPackSplit;
        private readonly ISerializationHelper serializationHelper;
        private readonly ISharedBLL _SharedBLL;
        private readonly IBatchHelper batchHelper;
        private readonly IPackageConfigurationLevel packageConfigurationLevel;
        private readonly TNTSPsContext _TNTSPsContext;
        public SingeCottnPack(IPackagingLinesSetupHelper packagingLinesSetup,
        IPriorScanValidationHelper priorScanValidation,
        IGtinHelper gtinHelper,
   IValidationRuleHelper validationRuleHelper,
        IPackageLebelingHelper packageLebelingHelper,
        IPackagingPackSplit packagingPackSplit,
        ISerializationHelper serializationHelper,
        IBatchHelper batchHelper, IPackageConfigurationLevel packageConfigurationLeve, ISharedBLL sharedBLL, TNTSPsContext tNTSPsContext)
        {
            _SharedBLL = sharedBLL;
            _TNTSPsContext = tNTSPsContext;
            this.packagingLinesSetup = packagingLinesSetup;
            this.priorScanValidation = priorScanValidation;
            this.gtinHelper = gtinHelper;
            this.validationRuleHelper = validationRuleHelper;
            this.packagingPackSplit = packagingPackSplit;
            this.packageLebelingHelper = packageLebelingHelper;
            this.batchHelper = batchHelper;
            this.serializationHelper = serializationHelper;
            this.packageConfigurationLevel = packageConfigurationLeve;
        }


        public int SaveMapedUVSerials(SaveMapedUVSerialModelView saveMapedUVSerials)
        {
            try
            {
                if (saveMapedUVSerials.SUVSerial == null)
                    saveMapedUVSerials.SUVSerial = "";
                int iPackLevel = 0;

                int iCheckUVVDuplicatePackaging = 0;
                int isValidSerials = 0;
                int iUserID = -1;
                int iTerminalID = -1;

                var aggregatedBatch = batchHelper.RetrieveCurrentAggregatedBatch(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMapedUVSerials.ICustomerID, saveMapedUVSerials.IAgencyID, saveMapedUVSerials.SProductCode, saveMapedUVSerials.SBatchNum);
                var batch = batchHelper.RetrieveBatchForAPI(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMapedUVSerials.ICustomerID, saveMapedUVSerials.IAgencyID, saveMapedUVSerials.SProductCode, saveMapedUVSerials.SBatchNum).LastOrDefault();
          
                if (aggregatedBatch != null && batch != null)
                {
                    saveMapedUVSerials.IChildLevelID = (int)aggregatedBatch.ChildLevelId;
                    saveMapedUVSerials.IParentLevelID = (int)aggregatedBatch.ParentLevelId;
                    iUserID = (int)aggregatedBatch.UserId ;
                    iTerminalID = (int)aggregatedBatch.TerminalId ;
                }
                string sFNC1ParseLabel = _SharedBLL.RemoveFNC1Char(saveMapedUVSerials.SBarcode2D);
                string sParseLabel = _SharedBLL.RemoveParentheses(sFNC1ParseLabel);
                var levels = packagingLinesSetup.GetPackageLinesByParentLevel(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMapedUVSerials.ICustomerID, saveMapedUVSerials.IAgencyID, batch.PRODUCT_ID, saveMapedUVSerials.IChildLevelID).ToList();

                var gitinList = gtinHelper.GetGtinsByProduct(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMapedUVSerials.ICustomerID, saveMapedUVSerials.IAgencyID, saveMapedUVSerials.SProductCode).ToList();
                int iTotalLabelLength = _SharedBLL.FetchTotalLabelLength(saveMapedUVSerials.ICustomerID, saveMapedUVSerials.IAgencyID, saveMapedUVSerials.SProductCode, saveMapedUVSerials.SBatchNum);
                if (sParseLabel.Length == iTotalLabelLength) // Check Pack Label Length
                {
                    string sAggregationTable = _SharedBLL.GenerateAggregationTable(saveMapedUVSerials.IAgencyID, saveMapedUVSerials.ICustomerID, saveMapedUVSerials.SProductCode, saveMapedUVSerials.SBatchNum);

                    bool iValidPackLevel = SharedBLL.arrayPackLevel.Contains(sParseLabel.Substring(0, 3));
                    // Check Valid Pack Level
                    if (iValidPackLevel)
                    {
                        iPackLevel = Int32.Parse(sParseLabel.Substring(2, 1));
                        if (iPackLevel == saveMapedUVSerials.IChildLevelID)
                        {
                            ExtractPackLabel extractPackLabel = new ExtractPackLabel(saveMapedUVSerials.ICustomerID, saveMapedUVSerials.IAgencyID, saveMapedUVSerials.SProductCode, saveMapedUVSerials.SBatchNum, sParseLabel,_SharedBLL);
                            if (extractPackLabel._BATCH_NUM.ToLower() == saveMapedUVSerials.SBatchNum.ToLower())
                            {


                                var GTIN = gitinList.Where(x => x.CustomerId == saveMapedUVSerials.ICustomerID && x.RegulatoryAgencyId == saveMapedUVSerials.IAgencyID && x.Gtin == extractPackLabel._GTIN_NUM && x.ProductCode == saveMapedUVSerials.SProductCode).LastOrDefault();

                                if (GTIN == null)
                                    return 7;
 
                                // Check Pack Level from Packaging Lines
                                if (levels.Count()>0)
                                {
                                    isValidSerials = (int)serializationHelper.CheckForDuplicateSerial(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMapedUVSerials.ICustomerID, saveMapedUVSerials.IAgencyID, saveMapedUVSerials.SProductCode, saveMapedUVSerials.SBatchNum, (int)SerializationLifeCycle.AGGREGATION, (int)SeriaLPackTypes.ALL, iPackLevel, extractPackLabel._SERIAL_NUM );
                                    // Check Serial Number from Serialization management
                                    if (isValidSerials > 0)
                                    {
                                        iCheckUVVDuplicatePackaging =  (int)_TNTSPsContext.OptomechPackagingCheckDuplicateSerials.FromSqlInterpolated($"Aggregation_Batch_Table_OptomechPackagingCheckDuplicateSerials {SharedBLL.iGroupID}, {SharedBLL.iPlantID}, {saveMapedUVSerials.ICustomerID}, {saveMapedUVSerials.IAgencyID}, {saveMapedUVSerials.SProductCode}, {saveMapedUVSerials.SBatchNum}, {saveMapedUVSerials.SUVSerial}, {2}, {sAggregationTable}").LastOrDefault().TOTAL_COUNT; // (int)packagingEntities.OptomechPackage_Labeling_Registration_Actions_CheckSerials(SharedBLL.iGroupID, _SharedBLL. iPlantID, ICustomerID, IAgencyID, SProductCode, SBatchNum, sBarcode2D).LastOrDefault();

                                        if (iCheckUVVDuplicatePackaging > 0)
                                        {

                                            return 3;
                                        }
                                        else
                                        {
                                            var uvList = Task.Run(() => serializationHelper.RetrieveUVSerialsList(SharedBLL.iGroupID,SharedBLL.iPlantID));
                                            
                                              if (uvList.Result.Contains(saveMapedUVSerials.SUVSerial)) // Check UV Number in Serialization DB    for succes                    
                                            {


                                            }


                                            else // UV not Registered 
                                            {

                                                return 5;
                                            }
                                        }



                                        isValidSerials = (int)_TNTSPsContext.OptomechPackagingCheckDuplicateSerials.FromSqlInterpolated($"Aggregation_Batch_Table_OptomechPackagingCheckDuplicateSerials {SharedBLL.iGroupID}, {SharedBLL.iPlantID}, {saveMapedUVSerials.ICustomerID}, {saveMapedUVSerials.IAgencyID}, {saveMapedUVSerials.SProductCode}, {saveMapedUVSerials.SBatchNum}, {saveMapedUVSerials.SBarcode2D }, {2}, {sAggregationTable}").LastOrDefault().TOTAL_COUNT; // (int)packagingEntities.OptomechPackage_Labeling_Registration_Actions_CheckSerials(SharedBLL.iGroupID, _SharedBLL. iPlantID, ICustomerID, IAgencyID, SProductCode, SBatchNum, sBarcode2D).LastOrDefault();
                                        if (isValidSerials <= 0)
                                        {
                                            // Save Packaging Info

                                            int iInsertedID = (int)_TNTSPsContext.OptomechPackagingPostInfos.FromSqlInterpolated($"Aggregation_Batch_Table_OptomechPackagingPostInfo {SharedBLL.iGroupID}, {SharedBLL.iPlantID}, {saveMapedUVSerials.ICustomerID}, {saveMapedUVSerials.IAgencyID}, {saveMapedUVSerials.SProductCode},{""}, {""}, {""}, {""}, {""}, {""}, {sParseLabel}, {sFNC1ParseLabel}, {"-1"}, {"-1"}, {"-1"}, {"-1"}, {" -1"}, {saveMapedUVSerials.SBarcode2D }, {saveMapedUVSerials.IParentLevelID}, {saveMapedUVSerials.IChildLevelID}, {""}, {extractPackLabel._GTIN_NUM}, {batch.EXPIRY_DATE_FORMAT}, {0}, {0}, {saveMapedUVSerials.SBatchNum}, {iTerminalID}, {-1}, {-1}, {0}, {0}, {-1}, {1}, {iUserID}, {""}, {sAggregationTable}").LastOrDefault().ID;
                                            packageLebelingHelper.AddOptomechChildLabels(new PackageLabelingRegistration()
                                            {
                                                CompanyId = SharedBLL.iGroupID,
                                                PlantId = SharedBLL.iPlantID,
                                                CustomerId = saveMapedUVSerials.ICustomerID,
                                                RegulatoryAgencyId = saveMapedUVSerials.IAgencyID,
                                                ProductCode = saveMapedUVSerials.SProductCode,
                                                ParentFirstLabel = "",
                                                ParentFirstLabelH = "",
                                                ParentSecondLabel = "",
                                                ParentSecondLabelH = "",
                                                ParentLabel = "",
                                                ParentLabelH = "",
                                                ChildLabel = sParseLabel,
                                                ChildLabelH = sFNC1ParseLabel,
                                                ChildFirstLabel = "-1",
                                                ChildFirstLabelH = "-1",
                                                ChildSecondLabel = "-1",
                                                ChildSecondLabelH = "-1",
                                                ParentSerialNumber = "-1",
                                                ChildSerialNumber = saveMapedUVSerials.SBarcode2D,
                                                FromLevelId = saveMapedUVSerials.IParentLevelID,
                                                ToLevelId = saveMapedUVSerials.IChildLevelID,
                                                ParentGtin = "",
                                                ChildGtin = extractPackLabel._GTIN_NUM,
                                                ExpiryDateCode = batch.EXPIRY_DATE_ENTRY,
                                                ParentQuantity = 0,
                                                ChildQuantity = 0,
                                                BatchNumber = saveMapedUVSerials.SBatchNum,
                                                TerminalId = iTerminalID,
                                                TerminalStatusId = 0,
                                                PackLifecycleId = -1,
                                                LooseShipperId = 0,
                                                DelinkStatusId = 0,
                                                DelinkCount = -1,
                                                IsValidLabel = 1,
                                                UserId = iUserID,
                                                UvSerialNumber = ""

                                            });
                                            // update serialization based on pack level and and Serial number
                                            serializationHelper.SetSerialNumStatus(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMapedUVSerials.ICustomerID, saveMapedUVSerials.IAgencyID, saveMapedUVSerials.SProductCode, saveMapedUVSerials.SBatchNum, (int)saveMapedUVSerials.IChildLevelID, (int)SerializationLifeCycle.AGGREGATION, (int)SeriaLPackTypes.AGGREGATED, extractPackLabel._SERIAL_NUM );
                                            // update serialization based on pack level and and Serial number
                                       
                                            _TNTSPsContext .Database.ExecuteSqlRaw("Serialization_Management_Registration_Actions_SetUVSerialStatusFromSerializationDB @UV_SERIAL_NUMBER", saveMapedUVSerials.SUVSerial);
                                              string sParentSerial = SaveAndUpdateParentSerialForSingle(sAggregationTable, 0, 0, extractPackLabel._GTIN_NUM, saveMapedUVSerials, batch, aggregatedBatch);
                                            if (sParentSerial.Length > 5)
                                            {
                                                Dictionary<string, string> dictionary = new Dictionary<string, string>
                                                {
                                                    { "parentLabel", sParentSerial }
                                                };
                                                return 9;// Ok(dictionary);
                                            }

                                        }
                                        else
                                        {

                                            // Duplicate serial found in Packaging
                                            return 6;
                                        }
                                    }
                                    else
                                    {
                                        // Serial number not found

                                        return 2;
                                    }
                                }
                            }
                            else
                            {
                                // Invalid Batch Number

                                return 10;
                            }
                        }
                        else
                        {
                            // Invalid Packlevel
     
                            return 1;
                        }
                    }
                    else
                    {
                        // Invalid Packlevel
 
                    }
                    return 1;
                }
                else
                {

                    return 1;
                }


            }
            catch (Exception ex)
            {

                _SharedBLL.LoggError(GetType().FullName, ex.Message, ex.InnerException.Message, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return 0;
            }

        }
        private string SaveAndUpdateParentSerialForSingle(string sAggregationTable, int iChildCount, int iTotalRequiredChilds, string sChildGITNNum, SaveMapedUVSerialModelView saveMapedUVSerials, Batch_Management_Registration_Reports_RetrieveBatchForAPI BatchRegistration, ScanPackagingWorkorderRegistration AggregatedBatch)
        {
            string sParentSerial = string.Empty;
            try

            {

                int IPackIndicator = packageConfigurationLevel.GetSystemPackageIndicatorId(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMapedUVSerials.ICustomerID, saveMapedUVSerials.IAgencyID, (int)saveMapedUVSerials.IParentLevelID);

                // Check Aggregated Quantity 
                if (iTotalRequiredChilds == iChildCount)
                {
                    var serialList = serializationHelper.FetchAnySerialForParents(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMapedUVSerials.ICustomerID, saveMapedUVSerials.IAgencyID, saveMapedUVSerials.SProductCode, saveMapedUVSerials.SBatchNum, (int)saveMapedUVSerials.IParentLevelID, (int)SerializationLifeCycle.COMMISSIONED, (int)SeriaLPackTypes.ALL).ToList();


                    // Check Pallet level

                    string sParseLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL);
                    packageLebelingHelper.UpdateAggregatedBatchTable(new PackageLabelingRegistration()
                    {
                        CompanyId = SharedBLL.iGroupID,
                        PlantId = SharedBLL.iPlantID,
                        CustomerId = saveMapedUVSerials.ICustomerID,
                        RegulatoryAgencyId = saveMapedUVSerials.IAgencyID,
                        ProductCode = saveMapedUVSerials.SProductCode,
                        BatchNumber = saveMapedUVSerials.SBatchNum,
                        ParentFirstLabel = "",
                        ParentSecondLabel = "",
                        ParentFirstLabelH = "",
                        ParentSecondLabelH = "",
                        ParentLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                        ParentLabelH = serialList.LastOrDefault().PACK_LABEL,
                        ParentSerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                        FromLevelId = saveMapedUVSerials.IParentLevelID,
                        ToLevelId = saveMapedUVSerials.IChildLevelID,
                        ParentGtin = serialList.LastOrDefault().GTIN_NUMBER,
                        ChildGtin = sChildGITNNum,
                        UvSerialNumber = ""

                    }, sAggregationTable);
                    packageLebelingHelper.UpdateOptomechParentLabels(new PackageLabelingRegistration()
                    {
                        CompanyId = SharedBLL.iGroupID,
                        PlantId = SharedBLL.iPlantID,
                        CustomerId = saveMapedUVSerials.ICustomerID,
                        RegulatoryAgencyId = saveMapedUVSerials.IAgencyID,
                        ProductCode = saveMapedUVSerials.SProductCode,
                        BatchNumber = saveMapedUVSerials.SBatchNum,
                        ParentFirstLabel = "",
                        ParentSecondLabel = "",
                        ParentFirstLabelH = "",
                        ParentSecondLabelH = "",
                        ParentLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                        ParentLabelH = serialList.LastOrDefault().PACK_LABEL,
                        ParentSerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                        FromLevelId = saveMapedUVSerials.IParentLevelID,
                        ToLevelId = saveMapedUVSerials.IChildLevelID,
                        ParentGtin = serialList.LastOrDefault().GTIN_NUMBER,
                        ChildGtin = sChildGITNNum

                    });
                    serializationHelper.SetSerialNumStatus(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMapedUVSerials.ICustomerID, saveMapedUVSerials.IAgencyID, saveMapedUVSerials.SProductCode, saveMapedUVSerials.SBatchNum, (int)saveMapedUVSerials.IChildLevelID, (int)SerializationLifeCycle.AGGREGATION, (int)SeriaLPackTypes.AGGREGATED, extractPackLabel._SERIAL_NUM);



                    int IUserID = (int)AggregatedBatch.UserId;

                    int IBatchID = BatchRegistration.ID;
                    string SProductID = BatchRegistration.PRODUCT_ID.ToString();

                    if (batchHelper.GetBatchHistory().Where(x => x.CompanyId == SharedBLL.iGroupID && x.PlantId == SharedBLL.iPlantID && x.CustomerId == saveMapedUVSerials.ICustomerID && x.RegulatoryAgencyId == saveMapedUVSerials.IAgencyID && x.BatchId == IBatchID && x.ProductId == Convert.ToInt32(SProductID) && x.FromStatusId == (int)BatchWorkFlowStatus.APPROVED_FOR_RELEASE && x.ToStatusId == (int)BatchWorkFlowStatus.IN_PROCESS && x.Remarks.Equals("Batch Aggregated Started")).Count() <= 0)
                    {
                        batchHelper.SaveBatchHistory(new BatchManagementRemarksRegistration()
                        {
                            CompanyId = SharedBLL.iGroupID,
                            PlantId = SharedBLL.iPlantID,
                            CustomerId = saveMapedUVSerials.ICustomerID,
                            RegulatoryAgencyId = saveMapedUVSerials.IAgencyID,
                            BatchId = IBatchID,
                            ProductId = Convert.ToInt32(SProductID),
                            FromStatusId = (int)BatchWorkFlowStatus.APPROVED_FOR_RELEASE,
                            ToStatusId = (int)BatchWorkFlowStatus.IN_PROCESS,
                            ApprovalTypeId = 4,
                            Remarks = "Batch Aggregated Started"
                        });
                        batchHelper.SetBatchStatus(IBatchID, (int)BatchWorkFlowStatus.IN_PROCESS);
                    }
                }
            }
            catch (Exception ex)
            {
                _SharedBLL.LoggError(GetType().FullName, ex.Message, ex.InnerException.Message, System.Reflection.MethodBase.GetCurrentMethod().Name);
                throw;
            }
            return sParentSerial;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(Boolean disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                    if (extractPackLabel != null)
                    {
                        extractPackLabel = null;
                    }
                }

                // Now disposed of any unmanaged objects
                // ...

                _disposed = true;
            }
        }
    }
}
