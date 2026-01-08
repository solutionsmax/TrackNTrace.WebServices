using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.ProjectModel;
using System.Data;
using System.Text;
using System.Xml.Linq;
using TrackNTrace.Repository.Entities;
using TrackNTrace.WebServices.com.Models;
using TrackNTrace.WebServices.com.Repository;

namespace TrackNTrace.WebServices.com.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase,IDisposable
    {
        private   TrackNTraceContext _TrackNTraceContext;
        public UtilityController(TrackNTraceContext trackNTraceContext)
        {
            _TrackNTraceContext = trackNTraceContext;
        }
        [HttpGet("TranslateText", Name = "TranslateText")]
        public ActionResult<IEnumerable<string>> TranslateText()
        {

            try
            {
                if (HasConnection())
                    GetPendingRecords();
                else
                    UpdateStatusWithNoInternet();

                    return Ok();
         
              
            }
            catch (Exception)
            {

                return BadRequest("");
            }
           
        }
        private void GetPendingRecords()
        {
            try
            {
                foreach (var item in _TrackNTraceContext.XmlAggregrationManagementRegistrations.Where(x => x.WorkflowStatusId == 11 && x.RegulatoryAgencyId==9).ToList())
                {
                    TraslateText(item.Id);
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        private void UpdateStatusWithNoInternet()
        {
            try
            {
                var updateList = _TrackNTraceContext.XmlAggregrationManagementRegistrations.Where(x => x.WorkflowStatusId == 11).ToList();

                updateList.ForEach(update => 
                {
                    update.WorkflowStatusId = 13;
                    update.UpdateDate = DateTime.Now;
                });
                _TrackNTraceContext.XmlAggregrationManagementRegistrations. UpdateRange(updateList);
                _TrackNTraceContext.SaveChanges();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public static bool HasConnection()
        {
            try
            {
                System.Net.IPHostEntry i = System.Net.Dns.GetHostEntry("www.google.com");
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void TraslateText(int Id)
        {
            var xmlData = _TrackNTraceContext.XmlAggregrationManagementRegistrations.Find(Id);
            if (xmlData != null)
            {
                DataSet ds = new DataSet();
                ds.ReadXml(new StringReader(xmlData.XmlData));

                StringBuilder sb = new StringBuilder();
                foreach (var item in ds.Tables[0].AsEnumerable())
                {
                    sb.Clear();
                    sb.Append("<ChinaLanguage>");
                    sb.Append($"<SchemaLocation> {GetTraslatedText(item["SchemaLocation"]?.ToString())} </SchemaLocation>");
                    sb.Append($"<PackType>{GetTraslatedText(item["PackType"]?.ToString())}</PackType>");
                    sb.Append($"<PackUnit>{GetTraslatedText(item["PackUnit"]?.ToString())}</PackUnit>");
                    sb.Append($"<ProductName>{GetTraslatedText(item["ProductName"]?.ToString())}</ProductName>");
                    sb.Append($"<Strength>{GetTraslatedText(item["Strength"]?.ToString())}</Strength>");
                    sb.Append($"<LineManager>{item["LineManager"]?.ToString()}</LineManager>");
                    sb.Append($"<PackageSpec> {GetTraslatedText(item["PackageSpec"]?.ToString())}</PackageSpec>");
                    sb.Append($"<ProductLine>{item["ProductLine"]?.ToString()}</ProductLine>");
                    sb.Append("</ChinaLanguage>");

                }

                xmlData.WorkflowStatusId = 13;
                xmlData.XmlData = sb.ToString();
 
                _TrackNTraceContext.XmlAggregrationManagementRegistrations.Update(xmlData);
                _TrackNTraceContext.SaveChanges();

            }
        }
        private string GetTraslatedText(string input)
        {
            try
            {
                string url = String.Format("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}", "en", "zh", input);
                HttpClient httpClient = new HttpClient();
                string result = httpClient.GetStringAsync(url).Result;
                result = result.Substring(4, result.IndexOf("\"", 4, StringComparison.Ordinal) - 4);
                return result;
            }
            catch (Exception)
            {
                return input;

            }
        }

        public void Dispose()
        {
            if (_TrackNTraceContext != null)
                _TrackNTraceContext = null;
        }
    }
}
