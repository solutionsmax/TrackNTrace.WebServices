using Microsoft.Extensions.Configuration.UserSecrets;
using TrackNTrace.Models.Enum;
using TrackNTrace.Repository.Common;
using TrackNTrace.Repository.Entities;
using TrackNTrace.Repository.Interfaces;
using TrackNTrace.Repository.StoredProcedures;
using TrackNTrace.Repository.Utilities;
using TrackNTrace.WebServices.com.Models;

namespace TrackNTrace.WebServices.com.AgencyValidation
{
    public   class UpdateParentInfo
    {
        public   string SaveAndUpdateParentSerial(string sAggregationTable, int iChildCount, int iTotalRequiredChilds, string sChildGITNNum,
            SaveMultiCottonPackModelView saveMultiCottonPack, Batch_Management_Registration_Reports_RetrieveBatchForAPI BatchRegistration,
            ScanPackagingWorkorderRegistration AggregatedBatch, IBatchHelper batchHelper, IPackageConfigurationLevel packageConfigurationLevel,
            ISerializationHelper serializationHelper, IPackageLebelingHelper packageLebelingHelper, ISharedBLL _SharedBLL)
        {
            string sParentSerial = string.Empty;
            try

            {
                int IPackIndicator = packageConfigurationLevel.GetSystemPackageIndicatorId(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, (int)saveMultiCottonPack.IParentLevelID);

                if (saveMultiCottonPack.IPackType == 1) // Full Pack
                {
                    // Check Aggregated Quantity 
                    if (iTotalRequiredChilds == iChildCount)
                    {

                        var serialList = serializationHelper.FetchAnySerialForParents(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, (int)saveMultiCottonPack.IParentLevelID, (int)SerializationLifeCycle.COMMISSIONED, (int)SeriaLPackTypes.ALL).ToList();

                        // Check Pallet level
                        if (!string.IsNullOrEmpty(serialList.LastOrDefault().PALLET_LABEL))
                        {
                            string sParseLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PALLET_LABEL);
                            packageLebelingHelper.UpdateAggregatedBatchTable(new PackageLabelingRegistration()
                            {
                                CompanyId = SharedBLL.iGroupID,
                                PlantId = SharedBLL.iPlantID,
                                CustomerId = saveMultiCottonPack.ICustomerID,
                                RegulatoryAgencyId = saveMultiCottonPack.IAgencyID,
                                ProductCode = saveMultiCottonPack.SProductCode,
                                BatchNumber = saveMultiCottonPack.SBatchNum,
                                ParentFirstLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                                ParentSecondLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                                ParentFirstLabelH = serialList.LastOrDefault().PACK_LABEL,
                                ParentSecondLabelH = serialList.LastOrDefault().PALLET_LABEL,
                                ParentLabel = "",
                                ParentLabelH = "",
                                ParentSerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                                FromLevelId = saveMultiCottonPack.IParentLevelID,
                                ToLevelId = saveMultiCottonPack.IChildLevelID,
                                ParentGtin = serialList.LastOrDefault().GTIN_NUMBER,
                                ChildGtin = sChildGITNNum,
                                UvSerialNumber = ""

                            }, sAggregationTable);
                            packageLebelingHelper.UpdateOptomechParentLabels(new PackageLabelingRegistration()
                            {
                                CompanyId = SharedBLL.iGroupID,
                                PlantId = SharedBLL.iPlantID,
                                CustomerId = saveMultiCottonPack.ICustomerID,
                                RegulatoryAgencyId = saveMultiCottonPack.IAgencyID,
                                ProductCode = saveMultiCottonPack.SProductCode,
                                BatchNumber = saveMultiCottonPack.SBatchNum,
                                ParentFirstLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                                ParentSecondLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                                ParentFirstLabelH = serialList.LastOrDefault().PACK_LABEL,
                                ParentSecondLabelH = serialList.LastOrDefault().PALLET_LABEL,
                                ParentLabel = "",
                                ParentLabelH = "",
                                ParentSerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                                FromLevelId = saveMultiCottonPack.IParentLevelID,
                                ToLevelId = saveMultiCottonPack.IChildLevelID,
                                ParentGtin = serialList.LastOrDefault().GTIN_NUMBER,
                                ChildGtin = sChildGITNNum

                            });
                            serializationHelper.SetSerialNumStatus(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, (int)saveMultiCottonPack.IParentLevelID, (int)SerializationLifeCycle.AGGREGATION, (int)SeriaLPackTypes.ALL, serialList.LastOrDefault().SERIAL_NUMBER);

                            sParentSerial = serialList.LastOrDefault().PALLET_LABEL;

                            SaveOptomechParentLabels(new OptomechParentLabel()
                            {
                                CustomerID = saveMultiCottonPack.ICustomerID,
                                AgencyID = saveMultiCottonPack.IAgencyID,
                                ProductCode = saveMultiCottonPack.SProductCode,
                                BatchNumber = saveMultiCottonPack.SBatchNum,
                                ParentLabel = serialList.LastOrDefault().PALLET_LABEL,
                                SerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                                ChildLevelID = saveMultiCottonPack.IChildLevelID,
                                ParentLevelID = saveMultiCottonPack.IParentLevelID,
                                GtinNumber = serialList.LastOrDefault().GTIN_NUMBER,
                                Quantity = serialList.LastOrDefault().QUANTITY,
                                ExpiryDate = BatchRegistration.EXPIRY_DATE_ENTRY,
                                LevelType = 1
                            }, IPackIndicator,_SharedBLL, packageLebelingHelper);
                        }

                        else
                        {
                            string sParseLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL);
                            packageLebelingHelper.UpdateAggregatedBatchTable(new PackageLabelingRegistration()
                            {
                                CompanyId = SharedBLL.iGroupID,
                                PlantId = SharedBLL.iPlantID,
                                CustomerId = saveMultiCottonPack.ICustomerID,
                                RegulatoryAgencyId = saveMultiCottonPack.IAgencyID,
                                ProductCode = saveMultiCottonPack.SProductCode,
                                BatchNumber = saveMultiCottonPack.SBatchNum,
                                ParentFirstLabel = "",
                                ParentSecondLabel = "",
                                ParentFirstLabelH = "",
                                ParentSecondLabelH = "",
                                ParentLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                                ParentLabelH = serialList.LastOrDefault().PACK_LABEL,
                                ParentSerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                                FromLevelId = saveMultiCottonPack.IParentLevelID,
                                ToLevelId = saveMultiCottonPack.IChildLevelID,
                                ParentGtin = serialList.LastOrDefault().GTIN_NUMBER,
                                ChildGtin = sChildGITNNum,
                                UvSerialNumber = ""

                            }, sAggregationTable);
                            packageLebelingHelper.UpdateOptomechParentLabels(new PackageLabelingRegistration()
                            {
                                CompanyId = SharedBLL.iGroupID,
                                PlantId = SharedBLL.iPlantID,
                                CustomerId = saveMultiCottonPack.ICustomerID,
                                RegulatoryAgencyId = saveMultiCottonPack.IAgencyID,
                                ProductCode = saveMultiCottonPack.SProductCode,
                                BatchNumber = saveMultiCottonPack.SBatchNum,
                                ParentFirstLabel = "",
                                ParentSecondLabel = "",
                                ParentFirstLabelH = "",
                                ParentSecondLabelH = "",
                                ParentLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                                ParentLabelH = serialList.LastOrDefault().PACK_LABEL,
                                ParentSerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                                FromLevelId = saveMultiCottonPack.IParentLevelID,
                                ToLevelId = saveMultiCottonPack.IChildLevelID,
                                ParentGtin = serialList.LastOrDefault().GTIN_NUMBER,
                                ChildGtin = sChildGITNNum

                            });
                            serializationHelper.SetSerialNumStatus(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, (int)saveMultiCottonPack.IParentLevelID, (int)SerializationLifeCycle.AGGREGATION, (int)SeriaLPackTypes.ALL, serialList.LastOrDefault().SERIAL_NUMBER);

                            sParentSerial = serialList.LastOrDefault().PACK_LABEL;
                            SaveOptomechParentLabels(new OptomechParentLabel()
                            {
                                CustomerID = saveMultiCottonPack.ICustomerID,
                                AgencyID = saveMultiCottonPack.IAgencyID,
                                ProductCode = saveMultiCottonPack.SProductCode,
                                BatchNumber = saveMultiCottonPack.SBatchNum,
                                ParentLabel = serialList.LastOrDefault().PACK_LABEL,
                                SerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                                ChildLevelID = saveMultiCottonPack.IChildLevelID,
                                ParentLevelID = saveMultiCottonPack.IParentLevelID,
                                GtinNumber = serialList.LastOrDefault().GTIN_NUMBER,
                                Quantity = serialList.LastOrDefault().QUANTITY,
                                ExpiryDate = BatchRegistration.EXPIRY_DATE_ENTRY
                            }, IPackIndicator, _SharedBLL, packageLebelingHelper);
                        }


                    }
                }

                else if (saveMultiCottonPack.IPackType == 2)
                {
                    var serialList = serializationHelper.FetchAnySerialForParents(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, -1, (int)SerializationLifeCycle.COMMISSIONED, (int)SeriaLPackTypes.ALL).Where(x => x.PALLET_LABEL != "").ToList();
                    if (serialList.Count() > 0)
                    {
                        string sParseLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PALLET_LABEL);
                        packageLebelingHelper.UpdateAggregatedBatchTable(new PackageLabelingRegistration()
                        {
                            CompanyId = SharedBLL.iGroupID,
                            PlantId = SharedBLL.iPlantID,
                            CustomerId = saveMultiCottonPack.ICustomerID,
                            RegulatoryAgencyId = saveMultiCottonPack.IAgencyID,
                            ProductCode = saveMultiCottonPack.SProductCode,
                            BatchNumber = saveMultiCottonPack.SBatchNum,
                            ParentFirstLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                            ParentSecondLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                            ParentFirstLabelH = serialList.LastOrDefault().PACK_LABEL,
                            ParentSecondLabelH = serialList.LastOrDefault().PALLET_LABEL,
                            ParentLabel = "",
                            ParentLabelH = "",
                            ParentSerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                            FromLevelId = saveMultiCottonPack.IParentLevelID,
                            ToLevelId = saveMultiCottonPack.IChildLevelID,
                            ParentGtin = serialList.LastOrDefault().GTIN_NUMBER,
                            ChildGtin = sChildGITNNum,
                            UvSerialNumber = ""

                        }, sAggregationTable);
                        packageLebelingHelper.UpdateOptomechParentLabels(new PackageLabelingRegistration()
                        {
                            CompanyId = SharedBLL.iGroupID,
                            PlantId = SharedBLL.iPlantID,
                            CustomerId = saveMultiCottonPack.ICustomerID,
                            RegulatoryAgencyId = saveMultiCottonPack.IAgencyID,
                            ProductCode = saveMultiCottonPack.SProductCode,
                            BatchNumber = saveMultiCottonPack.SBatchNum,
                            ParentFirstLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                            ParentSecondLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                            ParentFirstLabelH = serialList.LastOrDefault().PACK_LABEL,
                            ParentSecondLabelH = serialList.LastOrDefault().PALLET_LABEL,
                            ParentLabel = "",
                            ParentLabelH = "",
                            ParentSerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                            FromLevelId = saveMultiCottonPack.IParentLevelID,
                            ToLevelId = saveMultiCottonPack.IChildLevelID,
                            ParentGtin = serialList.LastOrDefault().GTIN_NUMBER,
                            ChildGtin = sChildGITNNum

                        });
                        serializationHelper.SetSerialNumStatus(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, (int)saveMultiCottonPack.IParentLevelID, (int)SerializationLifeCycle.AGGREGATION, (int)SeriaLPackTypes.ALL, serialList.LastOrDefault().SERIAL_NUMBER);


                        sParentSerial = serialList.LastOrDefault().PALLET_LABEL;
                        SaveOptomechParentLabels(new OptomechParentLabel()
                        {
                            CustomerID = saveMultiCottonPack.ICustomerID,
                            AgencyID = saveMultiCottonPack.IAgencyID,
                            ProductCode = saveMultiCottonPack.SProductCode,
                            BatchNumber = saveMultiCottonPack.SBatchNum,
                            ParentLabel = serialList.LastOrDefault().PALLET_LABEL,
                            SerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                            ChildLevelID = saveMultiCottonPack.IChildLevelID,
                            ParentLevelID = saveMultiCottonPack.IParentLevelID,
                            GtinNumber = serialList.LastOrDefault().GTIN_NUMBER,
                            Quantity = serialList.LastOrDefault().QUANTITY,
                            ExpiryDate = BatchRegistration.EXPIRY_DATE_ENTRY
                        }, IPackIndicator, _SharedBLL, packageLebelingHelper);
                    }
                }
                else if (saveMultiCottonPack.IPackType == 3)
                {
                    var serialList = serializationHelper.FetchAnySerialForParents(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, (int)saveMultiCottonPack.IParentLevelID, (int)SerializationLifeCycle.COMMISSIONED, (int)SeriaLPackTypes.ALL).Where(x => x.PALLET_LABEL == "").ToList();

                    string sParseLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL);
                    packageLebelingHelper.UpdateAggregatedBatchTable(new PackageLabelingRegistration()
                    {
                        CompanyId = SharedBLL.iGroupID,
                        PlantId = SharedBLL.iPlantID,
                        CustomerId = saveMultiCottonPack.ICustomerID,
                        RegulatoryAgencyId = saveMultiCottonPack.IAgencyID,
                        ProductCode = saveMultiCottonPack.SProductCode,
                        BatchNumber = saveMultiCottonPack.SBatchNum,
                        ParentFirstLabel = "",
                        ParentSecondLabel = "",
                        ParentFirstLabelH = "",
                        ParentSecondLabelH = "",
                        ParentLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                        ParentLabelH = serialList.LastOrDefault().PACK_LABEL,
                        ParentSerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                        FromLevelId = saveMultiCottonPack.IParentLevelID,
                        ToLevelId = saveMultiCottonPack.IChildLevelID,
                        ParentGtin = serialList.LastOrDefault().GTIN_NUMBER,
                        ChildGtin = sChildGITNNum,
                        UvSerialNumber = ""

                    }, sAggregationTable);
                    packageLebelingHelper.UpdateOptomechParentLabels(new PackageLabelingRegistration()
                    {
                        CompanyId = SharedBLL.iGroupID,
                        PlantId = SharedBLL.iPlantID,
                        CustomerId = saveMultiCottonPack.ICustomerID,
                        RegulatoryAgencyId = saveMultiCottonPack.IAgencyID,
                        ProductCode = saveMultiCottonPack.SProductCode,
                        BatchNumber = saveMultiCottonPack.SBatchNum,
                        ParentFirstLabel = "",
                        ParentSecondLabel = "",
                        ParentFirstLabelH = "",
                        ParentSecondLabelH = "",
                        ParentLabel = _SharedBLL.RemoveParentheses(serialList.LastOrDefault().PACK_LABEL),
                        ParentLabelH = serialList.LastOrDefault().PACK_LABEL,
                        ParentSerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                        FromLevelId = saveMultiCottonPack.IParentLevelID,
                        ToLevelId = saveMultiCottonPack.IChildLevelID,
                        ParentGtin = serialList.LastOrDefault().GTIN_NUMBER,
                        ChildGtin = sChildGITNNum

                    });
                    serializationHelper.SetSerialNumStatus(SharedBLL.iGroupID, SharedBLL.iPlantID, saveMultiCottonPack.ICustomerID, saveMultiCottonPack.IAgencyID, saveMultiCottonPack.SProductCode, saveMultiCottonPack.SBatchNum, (int)saveMultiCottonPack.IParentLevelID, (int)SerializationLifeCycle.AGGREGATION, (int)SeriaLPackTypes.ALL, serialList.LastOrDefault().SERIAL_NUMBER);
                    SaveOptomechParentLabels(new OptomechParentLabel()
                    {
                        CustomerID = saveMultiCottonPack.ICustomerID,
                        AgencyID = saveMultiCottonPack.IAgencyID,
                        ProductCode = saveMultiCottonPack.SProductCode,
                        BatchNumber = saveMultiCottonPack.SBatchNum,
                        ParentLabel = serialList.LastOrDefault().PACK_LABEL,
                        SerialNumber = serialList.LastOrDefault().SERIAL_NUMBER,
                        ChildLevelID = saveMultiCottonPack.IChildLevelID,
                        ParentLevelID = saveMultiCottonPack.IParentLevelID,
                        GtinNumber = serialList.LastOrDefault().GTIN_NUMBER,
                        Quantity = serialList.LastOrDefault().QUANTITY,
                        ExpiryDate = BatchRegistration.EXPIRY_DATE_ENTRY
                    }, IPackIndicator, _SharedBLL, packageLebelingHelper);
                    //  await    SqlHelper.ExecuteNonQueryAsync(TNTConnStr, "Optomech_Parent_Label_Packaging_Registration_Actions_PostInfo", keyValuePairs);

                    sParentSerial = serialList.LastOrDefault().PACK_LABEL;

                }


                if (BatchRegistration != null)
                {

                    int IUserID = (int)AggregatedBatch.UserId;
                    int IBatchID = BatchRegistration.ID;
                    string SProductID = saveMultiCottonPack.IProductID.ToString();

                    if (batchHelper.GetBatchHistory().Where(x => x.CompanyId == SharedBLL.iGroupID && x.PlantId == SharedBLL.iPlantID && x.CustomerId == saveMultiCottonPack.ICustomerID && x.RegulatoryAgencyId == saveMultiCottonPack.IAgencyID && x.BatchId == IBatchID && x.ProductId == Convert.ToInt32(SProductID) && x.FromStatusId == (int)BatchWorkFlowStatus.APPROVED_FOR_RELEASE && x.ToStatusId == (int)BatchWorkFlowStatus.IN_PROCESS && x.Remarks.Equals("Batch Aggregated Started")).Count() <= 0)
                    {
                        batchHelper.SaveBatchHistory(new BatchManagementRemarksRegistration()
                        {
                            CompanyId = SharedBLL.iGroupID,
                            PlantId = SharedBLL.iPlantID,
                            CustomerId = saveMultiCottonPack.ICustomerID,
                            RegulatoryAgencyId = saveMultiCottonPack.IAgencyID,
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

        private void SaveOptomechParentLabels(OptomechParentLabel optomechParentLabel, int IPackIndicator, ISharedBLL _SharedBLL, IPackageLebelingHelper packageLebelingHelper)
        {
            try
            {

                if (optomechParentLabel.LevelType == 1)
                    packageLebelingHelper.SaveOptomechParentLebel(new TntPrintParentLabelPackagingRegistrationBundle() { GroupId = SharedBLL.iGroupID, PlantId = SharedBLL.iPlantID, CustomerId = optomechParentLabel.CustomerID, RegulatoryAgencyId = optomechParentLabel.AgencyID, ProductCode = optomechParentLabel.ProductCode, BatchNumber = optomechParentLabel.BatchNumber, ParentLabel = optomechParentLabel.ParentLabel, ParentLevelId = optomechParentLabel.ParentLevelID, ChildLevelId = optomechParentLabel.ChildLevelID, WorkflowStatusId = 1, GtinNumber = optomechParentLabel.GtinNumber, SerailNumber = optomechParentLabel.SerialNumber, ExpairyDate = optomechParentLabel.ExpiryDate, Quantity = optomechParentLabel.Quantity, LabelTypeId = 2 },
                        "Optomech_Parent_Label_Packaging_Registration_Case_Actions_PostInfo");


                else
                {
                    if (optomechParentLabel.AgencyID == 2)
                    {
                        if (optomechParentLabel.ParentLevelID == 1 || optomechParentLabel.ParentLevelID == 2)
                            packageLebelingHelper.SaveOptomechParentLebel(new TntPrintParentLabelPackagingRegistrationBundle() { GroupId = SharedBLL.iGroupID, PlantId = SharedBLL.iPlantID, CustomerId = optomechParentLabel.CustomerID, RegulatoryAgencyId = optomechParentLabel.AgencyID, ProductCode = optomechParentLabel.ProductCode, BatchNumber = optomechParentLabel.BatchNumber, ParentLabel = optomechParentLabel.ParentLabel, ParentLevelId = optomechParentLabel.ParentLevelID, ChildLevelId = optomechParentLabel.ChildLevelID, WorkflowStatusId = 1, GtinNumber = optomechParentLabel.GtinNumber, SerailNumber = optomechParentLabel.SerialNumber, ExpairyDate = optomechParentLabel.ExpiryDate, Quantity = optomechParentLabel.Quantity, LabelTypeId = 2 },
                                    "Optomech_Parent_Label_Packaging_Registration_Bundle_Actions_PostInfo");
                        else if (optomechParentLabel.ParentLevelID == 3)
                            packageLebelingHelper.SaveOptomechParentLebel(new TntPrintParentLabelPackagingRegistrationBundle() { GroupId = SharedBLL.iGroupID, PlantId = SharedBLL.iPlantID, CustomerId = optomechParentLabel.CustomerID, RegulatoryAgencyId = optomechParentLabel.AgencyID, ProductCode = optomechParentLabel.ProductCode, BatchNumber = optomechParentLabel.BatchNumber, ParentLabel = optomechParentLabel.ParentLabel, ParentLevelId = optomechParentLabel.ParentLevelID, ChildLevelId = optomechParentLabel.ChildLevelID, WorkflowStatusId = 1, GtinNumber = optomechParentLabel.GtinNumber, SerailNumber = optomechParentLabel.SerialNumber, ExpairyDate = optomechParentLabel.ExpiryDate, Quantity = optomechParentLabel.Quantity, LabelTypeId = 2 },
                                    "Optomech_Parent_Label_Packaging_Registration_Case_Actions_PostInfo");
                        else if (optomechParentLabel.ParentLevelID == 5 || optomechParentLabel.ParentLevelID == 911)
                            packageLebelingHelper.SaveOptomechParentLebel(new TntPrintParentLabelPackagingRegistrationBundle() { GroupId = SharedBLL.iGroupID, PlantId = SharedBLL.iPlantID, CustomerId = optomechParentLabel.CustomerID, RegulatoryAgencyId = optomechParentLabel.AgencyID, ProductCode = optomechParentLabel.ProductCode, BatchNumber = optomechParentLabel.BatchNumber, ParentLabel = optomechParentLabel.ParentLabel, ParentLevelId = optomechParentLabel.ParentLevelID, ChildLevelId = optomechParentLabel.ChildLevelID, WorkflowStatusId = 1, GtinNumber = optomechParentLabel.GtinNumber, SerailNumber = optomechParentLabel.SerialNumber, ExpairyDate = optomechParentLabel.ExpiryDate, Quantity = optomechParentLabel.Quantity, LabelTypeId = 2 },
                                    "Optomech_Parent_Label_Packaging_Registration_Pallet_Actions_PostInfo");
                    }
                    else
                    {
                        if (IPackIndicator == 2 || IPackIndicator == 11 || IPackIndicator == 15 || IPackIndicator == 20 || IPackIndicator == 24)
                            packageLebelingHelper.SaveOptomechParentLebel(new TntPrintParentLabelPackagingRegistrationBundle() { GroupId = SharedBLL.iGroupID, PlantId = SharedBLL.iPlantID, CustomerId = optomechParentLabel.CustomerID, RegulatoryAgencyId = optomechParentLabel.AgencyID, ProductCode = optomechParentLabel.ProductCode, BatchNumber = optomechParentLabel.BatchNumber, ParentLabel = optomechParentLabel.ParentLabel, ParentLevelId = optomechParentLabel.ParentLevelID, ChildLevelId = optomechParentLabel.ChildLevelID, WorkflowStatusId = 1, GtinNumber = optomechParentLabel.GtinNumber, SerailNumber = optomechParentLabel.SerialNumber, ExpairyDate = optomechParentLabel.ExpiryDate, Quantity = optomechParentLabel.Quantity, LabelTypeId = 2 },
                                "Optomech_Parent_Label_Packaging_Registration_Bundle_Actions_PostInfo");
                        else if (IPackIndicator == 3 || IPackIndicator == 12 || IPackIndicator == 16 || IPackIndicator == 21 || IPackIndicator == 25)
                            packageLebelingHelper.SaveOptomechParentLebel(new TntPrintParentLabelPackagingRegistrationBundle() { GroupId = SharedBLL.iGroupID, PlantId = SharedBLL.iPlantID, CustomerId = optomechParentLabel.CustomerID, RegulatoryAgencyId = optomechParentLabel.AgencyID, ProductCode = optomechParentLabel.ProductCode, BatchNumber = optomechParentLabel.BatchNumber, ParentLabel = optomechParentLabel.ParentLabel, ParentLevelId = optomechParentLabel.ParentLevelID, ChildLevelId = optomechParentLabel.ChildLevelID, WorkflowStatusId = 1, GtinNumber = optomechParentLabel.GtinNumber, SerailNumber = optomechParentLabel.SerialNumber, ExpairyDate = optomechParentLabel.ExpiryDate, Quantity = optomechParentLabel.Quantity, LabelTypeId = 2 },
                                   "Optomech_Parent_Label_Packaging_Registration_Case_Actions_PostInfo");
                        else if (IPackIndicator == 4 || IPackIndicator == 13 || IPackIndicator == 17 || IPackIndicator == 22 || IPackIndicator == 26)
                            packageLebelingHelper.SaveOptomechParentLebel(new TntPrintParentLabelPackagingRegistrationBundle() { GroupId = SharedBLL.iGroupID, PlantId = SharedBLL.iPlantID, CustomerId = optomechParentLabel.CustomerID, RegulatoryAgencyId = optomechParentLabel.AgencyID, ProductCode = optomechParentLabel.ProductCode, BatchNumber = optomechParentLabel.BatchNumber, ParentLabel = optomechParentLabel.ParentLabel, ParentLevelId = optomechParentLabel.ParentLevelID, ChildLevelId = optomechParentLabel.ChildLevelID, WorkflowStatusId = 1, GtinNumber = optomechParentLabel.GtinNumber, SerailNumber = optomechParentLabel.SerialNumber, ExpairyDate = optomechParentLabel.ExpiryDate, Quantity = optomechParentLabel.Quantity, LabelTypeId = 2 },
                                    "Optomech_Parent_Label_Packaging_Registration_Actions_PostParentLabelInfo");

                    }
                }
            }
            catch (Exception ex)
            {

                _SharedBLL.LoggError(GetType().FullName, ex.Message, ex.InnerException.Message, System.Reflection.MethodBase.GetCurrentMethod().Name);
                throw;
            }
        }
    }
}
