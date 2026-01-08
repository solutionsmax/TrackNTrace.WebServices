using Microsoft.EntityFrameworkCore;
using TrackNTrace.Models.Enum;
using TrackNTrace.Repository.Common;
using TrackNTrace.Repository.Entities;
using TrackNTrace.Repository.Interfaces;
using TrackNTrace.Repository.StoredProcedures;
using TrackNTrace.Repository.Utilities;
using TrackNTrace.WebServices.com.Interfaces;
using TrackNTrace.WebServices.com.Models;

namespace TrackNTrace.WebServices.com.AgencyValidation
{
    public class NMPAChina86 : INMPAChina86
    {
        private bool _disposed = false;
       private readonly  IPackagingLinesSetupHelper packagingLinesSetup;
       private readonly  IPriorScanValidationHelper priorScanValidation;
       private readonly  IGtinHelper gtinHelper;
       private readonly  IValidationRuleHelper validationRuleHelper;
       private readonly  IPackageLebelingHelper packageLebelingHelper;
       private readonly  IPackagingPackSplit packagingPackSplit;
       private readonly  ISerializationHelper serializationHelper;
       private readonly  IBatchHelper batchHelper;
       private readonly  IPackageConfigurationLevel packageConfigurationLevel;
       private readonly  TNTSPsContext _TNTSPsContext;
        private readonly ISharedBLL _SharedBLL;
        public NMPAChina86(IPackagingLinesSetupHelper packagingLinesSetup,
        IPriorScanValidationHelper priorScanValidation,
        IGtinHelper gtinHelper,
        IValidationRuleHelper validationRuleHelper,
        IPackageLebelingHelper packageLebelingHelper,
        IPackagingPackSplit packagingPackSplit,
        ISerializationHelper serializationHelper,
        IBatchHelper batchHelper, IPackageConfigurationLevel packageConfigurationLeve, ISharedBLL sharedBLL, TNTSPsContext tNTSPsContext)
        {

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
            _SharedBLL = sharedBLL;
        }
        public List<ValidationErrorModelView> FullPack(SaveMultiCottonPackModelView saveMultiCottonPack, Batch_Management_Registration_Reports_RetrieveBatchForAPI BatchRegistration, ScanPackagingWorkorderRegistration AggregatedBatch)
        {


            List<ValidationErrorModelView> keyValuePairs = new List<ValidationErrorModelView>();
            try
            {
                int iPackLevel = 0;

                int isValidSerials = 0;

                string sPackSerial = string.Empty;
                string sPackLabelBatch = string.Empty;
                int iTotalRequiredChilds = 0;
                int iTotalSplitCount = 0;
                string SARNumber = BatchRegistration.AMRNUMBER;
                string sAggregationTable = _SharedBLL.GenerateAggregationTable(saveMultiCottonPack.IAgencyID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum);
                HashSet<string> hasSet = new HashSet<string>();
                int iTotalLabelLength = _SharedBLL.FetchSerialLength(saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID);
                var isPriorScan = priorScanValidation.PackageLabelingValidationChecks().Where(x => x.CompanyId == SharedBLL.iGroupID && x.PlantId == SharedBLL.iPlantID && x.ProductSetupId == saveMultiCottonPack.IProductID && x.PackageLevelIndicatorId == saveMultiCottonPack.IParentLevelID && x.ValidationCheckId == 14 && x.CustomerId == saveMultiCottonPack.ICustomerID && x.RegulatoryAgencyId == saveMultiCottonPack.IAgencyID).ToList();
                var levels = packagingLinesSetup.GetPackageLinesByParentLevel(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.IProductID, saveMultiCottonPack.IChildLevelID).ToList();

                var gitinList = gtinHelper.GetGtinsByProduct(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode).ToList();

                var ConfigPriorScan = validationRuleHelper.GetValidationRule(SharedBLL.iGroupID, SharedBLL.iPlantID, (int)ValidationRules.VALID_CHILD_SERIAL_PRIOR_SCAN).ToList();
                string[] multiSerialsList = saveMultiCottonPack.SBarcodeList.Split(',');
                iTotalRequiredChilds = (int)levels.LastOrDefault().NumOfScans;

                var objSplitList = packagingPackSplit.GetPackSizeByProduct(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.IParentLevelID.ToString() + saveMultiCottonPack.IChildLevelID.ToString()).ToList();
                if (objSplitList.Count() <= 0)
                {
                    iTotalSplitCount = iTotalRequiredChilds;
                }
                else
                {
                    if (objSplitList.LastOrDefault().SplitCount > 0)
                        iTotalSplitCount = (int)(objSplitList.LastOrDefault().TotalPackSize / objSplitList.LastOrDefault().SplitCount);
                    else
                        iTotalSplitCount = (int)objSplitList.LastOrDefault().TotalPackSize;

                }
                // Full Pack

                if (iTotalSplitCount == multiSerialsList.Length)
                {
                    foreach (var item in multiSerialsList)
                    {
                        string sFNC1ParseLabel = _SharedBLL.RemoveFNC1Char(item);
                        string sParseLabel = _SharedBLL.RemoveParentheses(sFNC1ParseLabel);

                        if (sParseLabel.Length == iTotalLabelLength) // Check Pack Label Length
                        {
                            if (isPriorScan.Count() > 0 && ConfigPriorScan.Count() > 0)
                                isValidSerials = serializationHelper.CheckForDuplicateSerial(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, saveMultiCottonPack.IChildLevelID, (int)SerializationLifeCycle.AGGREGATION, (int)SeriaLPackTypes.ALL, sParseLabel);
                            else
                                isValidSerials = (int)serializationHelper.CheckForDuplicateSerial(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, saveMultiCottonPack.IChildLevelID, (int)SerializationLifeCycle.ALL, (int)SeriaLPackTypes.ALL, sParseLabel);
                            // Check Serial Number from Serialization management
                            if (isValidSerials > 0)
                            {
                                var childs = _TNTSPsContext.OptomechPackagingCheckDuplicateSerials.FromSqlInterpolated($"Aggregation_Batch_Table_OptomechPackagingCheckDuplicateSerials {SharedBLL.iGroupID}, {SharedBLL.iPlantID}, {saveMultiCottonPack.ICustomerID}, {saveMultiCottonPack.IAgencyID}, {saveMultiCottonPack.SProductCode}, {saveMultiCottonPack.SBatchNum}, {sParseLabel}, {2}, {sAggregationTable}").IgnoreQueryFilters().ToList();
                                isValidSerials = childs.LastOrDefault().TOTAL_COUNT;
                                if (isValidSerials <= 0)
                                {
                                    // Save Packaging Info

                                    hasSet.Add(item);
                                    keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 11, ErrorMessage = "Success", ErrorValue = item });
                                }
                                else
                                {

                                    // Duplicate serial found in Packaging
                                    keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 6, ErrorMessage = "Duplicate serial found in Packaging", ErrorValue = item });

                                }
                            }
                            else
                            {
                                // Serial number not found

                                keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 5, ErrorMessage = "Serial number is not found", ErrorValue = item });

                            }

                        }
                        else
                        {
                            // Invalid Packlevel

                            keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 2, ErrorMessage = "Invalid Packlevel", ErrorValue = item });

                        }
                    }
                }

                else
                {
                    // Pack Quantity does not match

                    keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 1, ErrorMessage = "Pack Quantity does not match", ErrorValue = saveMultiCottonPack.SBarcodeList });
                }

                if (hasSet.Count == iTotalSplitCount)
                {


                    foreach (var item in hasSet)
                    {

                        string sFNC1ParseLabel = _SharedBLL.RemoveFNC1Char(item);
                        string sParseLabel = _SharedBLL.RemoveParentheses(sFNC1ParseLabel);
                        var childs = _TNTSPsContext.OptomechPackagingPostInfos.FromSqlInterpolated($"Aggregation_Batch_Table_OptomechPackagingPostInfo {SharedBLL.iGroupID}, {SharedBLL.iPlantID}, {saveMultiCottonPack.ICustomerID}, {saveMultiCottonPack.IAgencyID}, {saveMultiCottonPack.SProductCode},{""}, {""}, {""}, {""}, {""}, {""}, {sParseLabel}, {sFNC1ParseLabel}, {"-1"}, {"-1"}, {"-1"}, {"-1"}, {" -1"}, {sParseLabel}, {saveMultiCottonPack.IParentLevelID}, {saveMultiCottonPack.IChildLevelID},{""}, {gitinList.Where(x=>x.PackageLevelIndicator==saveMultiCottonPack.IChildLevelID ).LastOrDefault().Gtin},  {BatchRegistration.EXPIRY_DATE_FORMAT}, {0}, {0}, {saveMultiCottonPack.SBatchNum}, {saveMultiCottonPack.ITerminalID}, {-1}, {-1}, {0}, {0}, {-1}, {1}, {saveMultiCottonPack.IUserID}, {""}, {sAggregationTable}").IgnoreQueryFilters().ToList();
                        int iInsertedID = childs.LastOrDefault().ID;
                        packageLebelingHelper.AddOptomechChildLabels(new PackageLabelingRegistration()
                        {
                            CompanyId = SharedBLL.iGroupID,
                            PlantId = SharedBLL.iPlantID,
                            CustomerId = saveMultiCottonPack.ICustomerID,
                            RegulatoryAgencyId = saveMultiCottonPack.IAgencyID,
                            ProductCode = saveMultiCottonPack.SProductCode,
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
                            ChildSerialNumber = sParseLabel,
                            FromLevelId = saveMultiCottonPack.IParentLevelID,
                            ToLevelId = saveMultiCottonPack.IChildLevelID,
                            ParentGtin = "",
                            ChildGtin = gitinList.Where(x => x.PackageLevelIndicator == saveMultiCottonPack.IChildLevelID).LastOrDefault().Gtin,
                            ExpiryDateCode = BatchRegistration.EXPIRY_DATE_FORMAT,
                            ParentQuantity = 0,
                            ChildQuantity = 0,
                            BatchNumber = saveMultiCottonPack.SBatchNum,
                            TerminalId = saveMultiCottonPack.ITerminalID,
                            TerminalStatusId = 0,
                            PackLifecycleId = -1,
                            LooseShipperId = 0,
                            DelinkStatusId = 0,
                            DelinkCount = -1,
                            IsValidLabel = 1,
                            UserId = saveMultiCottonPack.IUserID,
                            UvSerialNumber = "",
                            CreateDate=DateTime.Now ,
                             

                        });
                        // update serialization based on pack level and and Serial number
                        serializationHelper.SetSerialNumStatus(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, (int)saveMultiCottonPack.IChildLevelID, (int)SerializationLifeCycle.AGGREGATION, (int)SeriaLPackTypes.AGGREGATED, sPackSerial);
                        var insertedChilds = _TNTSPsContext.OptomechPackagingCheckDuplicateSerials.FromSqlInterpolated($"Aggregation_Batch_Table_OptomechFetchTotalPackageCount {SharedBLL.iGroupID}, {SharedBLL.iPlantID}, {saveMultiCottonPack.IAgencyID}, {saveMultiCottonPack.ICustomerID}, {saveMultiCottonPack.SProductCode}, {saveMultiCottonPack.SBatchNum}, {-1}, {saveMultiCottonPack.IChildLevelID}, {""}, {-1}, {-1}, {sAggregationTable}").ToList();
                        int iChildCount = insertedChilds.LastOrDefault().TOTAL_COUNT;
                        if (iChildCount == iTotalRequiredChilds)
                        {
                            var parentInfo = new UpdateParentInfo();
                            string sParentSerial = parentInfo.SaveAndUpdateParentSerial(sAggregationTable, iChildCount, iTotalRequiredChilds, gitinList.Where(x=>x.PackageLevelIndicator==saveMultiCottonPack.IChildLevelID ).LastOrDefault().Gtin, saveMultiCottonPack, BatchRegistration, AggregatedBatch, batchHelper, packageConfigurationLevel, serializationHelper, packageLebelingHelper, _SharedBLL);
                            if (sParentSerial.Length > 5)
                            {
                                keyValuePairs = new List<ValidationErrorModelView>
                            {
                                new ValidationErrorModelView() { ErrorCode = 12, ErrorMessage = "Parent", ErrorValue = sParentSerial }
                            };
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                _SharedBLL.LoggError(GetType().FullName, ex.Message, ex.InnerException.Message, System.Reflection.MethodBase.GetCurrentMethod().Name);

            }
            return keyValuePairs;
        }
        public List<ValidationErrorModelView> LoosePack(SaveMultiCottonPackModelView saveMultiCottonPack, Batch_Management_Registration_Reports_RetrieveBatchForAPI BatchRegistration, ScanPackagingWorkorderRegistration AggregatedBatch)
        {

            List<ValidationErrorModelView> keyValuePairs = new List<ValidationErrorModelView>();
            try
            {
               
                int isValidSerials = 0;

                string sPackSerial = string.Empty;
                string sPackLabelBatch = string.Empty;
                int iTotalRequiredChilds = 0;
                int iTotalSplitCount = 0;
                string SARNumber = BatchRegistration.AMRNUMBER;
                string sAggregationTable = _SharedBLL.GenerateAggregationTable(saveMultiCottonPack.IAgencyID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum);
                HashSet<string> hasSet = new HashSet<string>();
                int iTotalLabelLength = _SharedBLL.FetchSerialLength(saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID);
                var isPriorScan = priorScanValidation.PackageLabelingValidationChecks().Where(x => x.CompanyId == SharedBLL.iGroupID && x.PlantId == SharedBLL.iPlantID && x.ProductSetupId == saveMultiCottonPack.IProductID && x.PackageLevelIndicatorId == saveMultiCottonPack.IParentLevelID && x.ValidationCheckId == 14 && x.CustomerId == saveMultiCottonPack.ICustomerID && x.RegulatoryAgencyId == saveMultiCottonPack.IAgencyID).ToList();
                var levels = packagingLinesSetup.GetPackageLinesByParentLevel(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.IProductID, saveMultiCottonPack.IChildLevelID).ToList();

                var gitinList = gtinHelper.GetGtinsByProduct(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode).ToList();

                var ConfigPriorScan = validationRuleHelper.GetValidationRule(SharedBLL.iGroupID, SharedBLL.iPlantID, (int)ValidationRules.VALID_CHILD_SERIAL_PRIOR_SCAN).ToList();
                string[] multiSerialsList = saveMultiCottonPack.SBarcodeList.Split(',');
                iTotalRequiredChilds = (int)levels.LastOrDefault().NumOfScans;

                var objSplitList = packagingPackSplit.GetPackSizeByProduct(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.IParentLevelID.ToString() + saveMultiCottonPack.IChildLevelID.ToString()).ToList();
                if (objSplitList.Count() <= 0)
                {

                    iTotalSplitCount = iTotalRequiredChilds;
                }
                else
                {
                    if (objSplitList.LastOrDefault().SplitCount > 0)
                        iTotalSplitCount = (int)(objSplitList.LastOrDefault().TotalPackSize / objSplitList.LastOrDefault().SplitCount);
                    else
                        iTotalSplitCount = (int)objSplitList.LastOrDefault().TotalPackSize;


                }
                // Full Pack

                if (multiSerialsList.Length <= iTotalSplitCount)
                {
                    foreach (var item in multiSerialsList)
                    {
                        string sFNC1ParseLabel = _SharedBLL.RemoveFNC1Char(item);
                        string sParseLabel = _SharedBLL.RemoveParentheses(sFNC1ParseLabel);

                        if (sParseLabel.Length == iTotalLabelLength) // Check Pack Label Length
                        {
                            if (isPriorScan.Count() > 0 && ConfigPriorScan.Count() > 0)
                                isValidSerials = serializationHelper.CheckForDuplicateSerial(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, saveMultiCottonPack.IChildLevelID, (int)SerializationLifeCycle.AGGREGATION, (int)SeriaLPackTypes.ALL, sParseLabel);
                            else
                                isValidSerials = (int)serializationHelper.CheckForDuplicateSerial(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, saveMultiCottonPack.IChildLevelID, (int)SerializationLifeCycle.ALL , (int)SeriaLPackTypes.ALL, sParseLabel);
                            // Check Serial Number from Serialization management
                            if (isValidSerials > 0)
                            {
                                var childs = _TNTSPsContext.OptomechPackagingCheckDuplicateSerials.FromSqlInterpolated($"Aggregation_Batch_Table_OptomechPackagingCheckDuplicateSerials {SharedBLL.iGroupID}, {SharedBLL.iPlantID}, {saveMultiCottonPack.ICustomerID}, {saveMultiCottonPack.IAgencyID}, {saveMultiCottonPack.SProductCode}, {saveMultiCottonPack.SBatchNum}, {sParseLabel}, {2}, {sAggregationTable}").IgnoreQueryFilters().ToList();
                                isValidSerials = childs.LastOrDefault().TOTAL_COUNT;
                                if (isValidSerials <= 0)
                                {
                                    // Save Packaging Info

                                    hasSet.Add(item);
                                    keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 11, ErrorMessage = "Success", ErrorValue = item });
                                }
                                else
                                {

                                    // Duplicate serial found in Packaging
                                    keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 6, ErrorMessage = "Duplicate serial found in Packaging", ErrorValue = item });

                                }
                            }
                            else
                            {
                                // Serial number not found

                                keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 5, ErrorMessage = "Serial number is not found", ErrorValue = item });

                            }

                        }
                        else
                        {
                            // Invalid Packlevel

                            keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 2, ErrorMessage = "Invalid Packlevel", ErrorValue = item });

                        }
                    }
                }

                else
                {
                    // Pack Quantity does not match

                    keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 1, ErrorMessage = "Pack Quantity does not match", ErrorValue = saveMultiCottonPack.SBarcodeList });
                }
                var insertedChilds = _TNTSPsContext.OptomechPackagingCheckDuplicateSerials.FromSqlInterpolated($"Aggregation_Batch_Table_OptomechFetchTotalPackageCount {SharedBLL.iGroupID}, {SharedBLL.iPlantID}, {saveMultiCottonPack.IAgencyID}, {saveMultiCottonPack.ICustomerID}, {saveMultiCottonPack.SProductCode}, {saveMultiCottonPack.SBatchNum}, {-1}, {saveMultiCottonPack.IChildLevelID}, {""}, {-1}, {-1}, {sAggregationTable}").ToList();
                int iChildCount = insertedChilds.LastOrDefault().TOTAL_COUNT; // Check total quantity required per pack
                if (iChildCount < iTotalRequiredChilds && (iChildCount + hasSet.Count()) < iTotalRequiredChilds)
                {
                    if (hasSet.Count() == multiSerialsList.Length)
                    {

                        foreach (var item in hasSet)
                        {

                            string sFNC1ParseLabel = _SharedBLL.RemoveFNC1Char(item);
                            string sParseLabel = _SharedBLL.RemoveParentheses(sFNC1ParseLabel);


                            var childs = _TNTSPsContext.OptomechPackagingPostInfos.FromSqlInterpolated($"Aggregation_Batch_Table_OptomechPackagingPostInfo {SharedBLL.iGroupID}, {SharedBLL.iPlantID}, {saveMultiCottonPack.ICustomerID}, {saveMultiCottonPack.IAgencyID}, {saveMultiCottonPack.SProductCode},{""}, {""}, {""}, {""}, {""}, {""}, {sParseLabel}, {sFNC1ParseLabel}, {"-1"}, {"-1"}, {"-1"}, {"-1"}, {" -1"}, {sParseLabel}, {saveMultiCottonPack.IParentLevelID}, {saveMultiCottonPack.IChildLevelID},{""}, {gitinList.Where(x => x.PackageLevelIndicator == saveMultiCottonPack.IChildLevelID).LastOrDefault().Gtin},  {BatchRegistration.EXPIRY_DATE_FORMAT}, {0}, {0}, {saveMultiCottonPack.SBatchNum}, {saveMultiCottonPack.ITerminalID}, {-1}, {-1}, {0}, {0}, {-1}, {1}, {saveMultiCottonPack.IUserID}, {""}, {sAggregationTable}").IgnoreQueryFilters().ToList();
                            int iInsertedID = childs.LastOrDefault().ID;
                            packageLebelingHelper.AddOptomechChildLabels(new PackageLabelingRegistration()
                            {
                                CompanyId = SharedBLL.iGroupID,
                                PlantId = SharedBLL.iPlantID,
                                CustomerId = saveMultiCottonPack.ICustomerID,
                                RegulatoryAgencyId = saveMultiCottonPack.IAgencyID,
                                ProductCode = saveMultiCottonPack.SProductCode,
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
                                ChildSerialNumber = sParseLabel,
                                FromLevelId = saveMultiCottonPack.IParentLevelID,
                                ToLevelId = saveMultiCottonPack.IChildLevelID,
                                ParentGtin = "",
                                ChildGtin = gitinList.Where(x => x.PackageLevelIndicator == saveMultiCottonPack.IChildLevelID).LastOrDefault().Gtin,
                                ExpiryDateCode = BatchRegistration.EXPIRY_DATE_FORMAT,
                                ParentQuantity = 0,
                                ChildQuantity = 0,
                                BatchNumber = saveMultiCottonPack.SBatchNum,
                                TerminalId = saveMultiCottonPack.ITerminalID,
                                TerminalStatusId = 0,
                                PackLifecycleId = -1,
                                LooseShipperId = 0,
                                DelinkStatusId = 0,
                                DelinkCount = -1,
                                IsValidLabel = 1,
                                UserId = saveMultiCottonPack.IUserID,
                                UvSerialNumber = "",
                                CreateDate = DateTime.Now

                            });
                            // update serialization based on pack level and and Serial number
                            serializationHelper.SetSerialNumStatus(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, (int)saveMultiCottonPack.IChildLevelID, (int)SerializationLifeCycle.AGGREGATION, (int)SeriaLPackTypes.AGGREGATED, sPackSerial);
                        }
                    }
                    if ((iChildCount < iTotalRequiredChilds && iTotalSplitCount > multiSerialsList.Length && hasSet.Count() == multiSerialsList.Length))
                    {
                        var parentInfo = new UpdateParentInfo();
                        string sParentSerial = parentInfo.SaveAndUpdateParentSerial(sAggregationTable, iChildCount, iTotalRequiredChilds, gitinList.Where(x=>x.PackageLevelIndicator==saveMultiCottonPack.IChildLevelID ).LastOrDefault().Gtin, saveMultiCottonPack, BatchRegistration, AggregatedBatch, batchHelper, packageConfigurationLevel, serializationHelper, packageLebelingHelper, _SharedBLL);
                        if (sParentSerial.Length > 5)
                        {
                            keyValuePairs = new List<ValidationErrorModelView>() {
                             new ValidationErrorModelView() { ErrorCode = 12, ErrorMessage = "Parent", ErrorValue = sParentSerial } };
                        }
                    }

                }
                else
                {
                    // Pack Quantity does not match
                    keyValuePairs = new List<ValidationErrorModelView>();
                    keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 1, ErrorMessage = "Pack Quantity does not match", ErrorValue = saveMultiCottonPack.SBarcodeList });
                }



            }
            catch (Exception ex)
            {
                _SharedBLL.LoggError(GetType().FullName, ex.Message, ex.InnerException.Message, System.Reflection.MethodBase.GetCurrentMethod().Name);

            }
            return keyValuePairs;
        }
        public List<ValidationErrorModelView> LoosePackNonSSC(SaveMultiCottonPackModelView saveMultiCottonPack, Batch_Management_Registration_Reports_RetrieveBatchForAPI BatchRegistration, ScanPackagingWorkorderRegistration AggregatedBatch)
        {

            List<ValidationErrorModelView> keyValuePairs = new List<ValidationErrorModelView>();
            try
            {
                int iPackLevel = 0;

                int isValidSerials = 0;

                string sPackSerial = string.Empty;
                string sPackLabelBatch = string.Empty;
                int iTotalRequiredChilds = 0;
                int iTotalSplitCount = 0;
                string SARNumber = BatchRegistration.AMRNUMBER;
                string sAggregationTable = _SharedBLL.GenerateAggregationTable(saveMultiCottonPack.IAgencyID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum);
                HashSet<string> hasSet = new HashSet<string>();
                int iTotalLabelLength = _SharedBLL.FetchSerialLength(saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID);
                var isPriorScan = priorScanValidation.PackageLabelingValidationChecks().Where(x => x.CompanyId == SharedBLL.iGroupID && x.PlantId == SharedBLL.iPlantID && x.ProductSetupId == saveMultiCottonPack.IProductID && x.PackageLevelIndicatorId == saveMultiCottonPack.IParentLevelID && x.ValidationCheckId == 14 && x.CustomerId == saveMultiCottonPack.ICustomerID && x.RegulatoryAgencyId == saveMultiCottonPack.IAgencyID).ToList();
                var levels = packagingLinesSetup.GetPackageLinesByParentLevel(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.IProductID, saveMultiCottonPack.IChildLevelID).ToList();

                var gitinList = gtinHelper.GetGtinsByProduct(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode).ToList();

                var ConfigPriorScan = validationRuleHelper.GetValidationRule(SharedBLL.iGroupID, SharedBLL.iPlantID, (int)ValidationRules.VALID_CHILD_SERIAL_PRIOR_SCAN).ToList();
                string[] multiSerialsList = saveMultiCottonPack.SBarcodeList.Split(',');
                iTotalRequiredChilds = (int)levels.LastOrDefault().NumOfScans;

                var objSplitList = packagingPackSplit.GetPackSizeByProduct(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.IParentLevelID.ToString() + saveMultiCottonPack.IChildLevelID.ToString()).ToList();
                if (objSplitList.Count() <= 0)
                {

                    iTotalSplitCount = iTotalRequiredChilds;
                }
                else
                {
                    if (objSplitList.LastOrDefault().SplitCount > 0)
                        iTotalSplitCount = (int)(objSplitList.LastOrDefault().TotalPackSize / objSplitList.LastOrDefault().SplitCount);
                    else
                        iTotalSplitCount = (int)objSplitList.LastOrDefault().TotalPackSize;
                }
                // Full Pack

                if (multiSerialsList.Length <= iTotalSplitCount)
                {
                    foreach (var item in multiSerialsList)
                    {
                        string sFNC1ParseLabel = _SharedBLL.RemoveFNC1Char(item);
                        string sParseLabel = _SharedBLL.RemoveParentheses(sFNC1ParseLabel);

                        if (sParseLabel.Length == iTotalLabelLength) // Check Pack Label Length
                        {
                            if (isPriorScan.Count() > 0 && ConfigPriorScan.Count() > 0)
                                isValidSerials = serializationHelper.CheckForDuplicateSerial(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, saveMultiCottonPack.IChildLevelID, (int)SerializationLifeCycle.AGGREGATION, (int)SeriaLPackTypes.ALL, sParseLabel);
                            else
                                isValidSerials = (int)serializationHelper.CheckForDuplicateSerial(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, saveMultiCottonPack.IChildLevelID, (int)SerializationLifeCycle.ALL, (int)SeriaLPackTypes.ALL, sParseLabel);
                            // Check Serial Number from Serialization management
                            if (isValidSerials > 0)
                            {
                                var childs = _TNTSPsContext.OptomechPackagingCheckDuplicateSerials.FromSqlInterpolated($"Aggregation_Batch_Table_OptomechPackagingCheckDuplicateSerials {SharedBLL.iGroupID}, {SharedBLL.iPlantID}, {saveMultiCottonPack.ICustomerID}, {saveMultiCottonPack.IAgencyID}, {saveMultiCottonPack.SProductCode}, {saveMultiCottonPack.SBatchNum}, {sParseLabel}, {2}, {sAggregationTable}").IgnoreQueryFilters().ToList();
                                isValidSerials = childs.LastOrDefault().TOTAL_COUNT;
                                if (isValidSerials <= 0)
                                {
                                    // Save Packaging Info

                                    hasSet.Add(item);
                                    keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 11, ErrorMessage = "Success", ErrorValue = item });
                                }
                                else
                                {

                                    // Duplicate serial found in Packaging
                                    keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 6, ErrorMessage = "Duplicate serial found in Packaging", ErrorValue = item });

                                }
                            }
                            else
                            {
                                // Serial number not found

                                keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 5, ErrorMessage = "Serial number is not found", ErrorValue = item });

                            }

                        }
                        else
                        {
                            // Invalid Packlevel

                            keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 2, ErrorMessage = "Invalid Packlevel", ErrorValue = item });

                        }
                    }
                }

                else
                {
                    // Pack Quantity does not match

                    keyValuePairs.Add(new ValidationErrorModelView() { ErrorCode = 1, ErrorMessage = "Pack Quantity does not match", ErrorValue = saveMultiCottonPack.SBarcodeList });
                }

                var insertedChilds = _TNTSPsContext.OptomechPackagingCheckDuplicateSerials.FromSqlInterpolated($"Aggregation_Batch_Table_OptomechFetchTotalPackageCount {SharedBLL.iGroupID}, {SharedBLL.iPlantID}, {saveMultiCottonPack.IAgencyID}, {saveMultiCottonPack.ICustomerID}, {saveMultiCottonPack.SProductCode}, {saveMultiCottonPack.SBatchNum}, {-1}, {saveMultiCottonPack.IChildLevelID}, {""}, {-1}, {-1}, {sAggregationTable}").ToList();
                int iChildCount = insertedChilds.LastOrDefault().TOTAL_COUNT;// Check total quantity required per pack
                if (iChildCount < iTotalRequiredChilds && (iChildCount + hasSet.Count()) < iTotalRequiredChilds)
                {
                    if (hasSet.Count() == multiSerialsList.Length)
                    {

                        foreach (var item in hasSet)
                        {

                            string sFNC1ParseLabel = _SharedBLL.RemoveFNC1Char(item);
                            string sParseLabel = _SharedBLL.RemoveParentheses(sFNC1ParseLabel);


                            var childs = _TNTSPsContext.OptomechPackagingPostInfos.FromSqlInterpolated($"Aggregation_Batch_Table_OptomechPackagingPostInfo {SharedBLL.iGroupID}, {SharedBLL.iPlantID}, {saveMultiCottonPack.ICustomerID}, {saveMultiCottonPack.IAgencyID}, {saveMultiCottonPack.SProductCode},{""}, {""}, {""}, {""}, {""}, {""}, {sParseLabel}, {sFNC1ParseLabel}, {"-1"}, {"-1"}, {"-1"}, {"-1"}, {" -1"}, {sParseLabel}, {saveMultiCottonPack.IParentLevelID}, {saveMultiCottonPack.IChildLevelID},{""}, {gitinList.Where(x => x.PackageLevelIndicator == saveMultiCottonPack.IChildLevelID).LastOrDefault().Gtin},  {BatchRegistration.EXPIRY_DATE_FORMAT}, {0}, {0}, {saveMultiCottonPack.SBatchNum}, {saveMultiCottonPack.ITerminalID}, {-1}, {-1}, {0}, {0}, {-1}, {1}, {saveMultiCottonPack.IUserID}, {""}, {sAggregationTable}").IgnoreQueryFilters().ToList();
                            int iInsertedID = childs.LastOrDefault().ID;
                            packageLebelingHelper.AddOptomechChildLabels(new PackageLabelingRegistration()
                            {
                                CompanyId = SharedBLL.iGroupID,
                                PlantId = SharedBLL.iPlantID,
                                CustomerId = saveMultiCottonPack.ICustomerID,
                                RegulatoryAgencyId = saveMultiCottonPack.IAgencyID,
                                ProductCode = saveMultiCottonPack.SProductCode,
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
                                ChildSerialNumber = sParseLabel,
                                FromLevelId = saveMultiCottonPack.IParentLevelID,
                                ToLevelId = saveMultiCottonPack.IChildLevelID,
                                ParentGtin = "",
                                ChildGtin = gitinList.Where(x=>x.PackageLevelIndicator==saveMultiCottonPack.IChildLevelID ).LastOrDefault().Gtin,
                                ExpiryDateCode = BatchRegistration.EXPIRY_DATE_FORMAT,
                                ParentQuantity = 0,
                                ChildQuantity = 0,
                                BatchNumber = saveMultiCottonPack.SBatchNum,
                                TerminalId = saveMultiCottonPack.ITerminalID,
                                TerminalStatusId = 0,
                                PackLifecycleId = -1,
                                LooseShipperId = 0,
                                DelinkStatusId = 0,
                                DelinkCount = -1,
                                IsValidLabel = 1,
                                UserId = saveMultiCottonPack.IUserID,
                                UvSerialNumber = "",
                                CreateDate = DateTime.Now

                            });
                            // update serialization based on pack level and and Serial number
                            serializationHelper.SetSerialNumStatus(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, (int)saveMultiCottonPack.IChildLevelID, (int)SerializationLifeCycle.AGGREGATION, (int)SeriaLPackTypes.AGGREGATED, sPackSerial);
                        }
                    }
                    if ((iChildCount < iTotalRequiredChilds && iTotalSplitCount > multiSerialsList.Length && hasSet.Count() == multiSerialsList.Length))
                    {
                        var parentInfo = new UpdateParentInfo();
                        string sParentSerial = parentInfo.SaveAndUpdateParentSerial(sAggregationTable, iChildCount, iTotalRequiredChilds, gitinList.Where(x=>x.PackageLevelIndicator==saveMultiCottonPack.IChildLevelID ).LastOrDefault().Gtin, saveMultiCottonPack, BatchRegistration, AggregatedBatch,batchHelper, packageConfigurationLevel,serializationHelper, packageLebelingHelper, _SharedBLL);
                        if (sParentSerial.Length > 5)
                        {
                            keyValuePairs = new List<ValidationErrorModelView>() {
                             new ValidationErrorModelView() { ErrorCode = 12, ErrorMessage = "Parent", ErrorValue = sParentSerial } };
                        }
                    }

                }
                else
                {
                    // Pack Quantity does not match

                    keyValuePairs = new List<ValidationErrorModelView>() {
               new ValidationErrorModelView() { ErrorCode = 1, ErrorMessage = "Pack Quantity does not match", ErrorValue = saveMultiCottonPack.SBarcodeList }};

                }



            }
            catch (Exception ex)
            {
                _SharedBLL.LoggError(GetType().FullName, ex.Message, ex.InnerException.Message, System.Reflection.MethodBase.GetCurrentMethod().Name);

            }
            return keyValuePairs;
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
   
                }

                // Now disposed of any unmanaged objects
                // ...

                _disposed = true;
            }
        }
    }
}
