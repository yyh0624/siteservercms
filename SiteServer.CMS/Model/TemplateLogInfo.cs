using System;

namespace SiteServer.CMS.Model
{
    public class TemplateLogInfo
    {
        public TemplateLogInfo()
        {
            Id = 0;
            TemplateId = 0;
            SiteId = 0;
            AddDate = DateTime.Now;
            AddUserName = string.Empty;
            ContentLength = 0;
            TemplateContent = string.Empty;
        }

        public TemplateLogInfo(int id, int templateId, int siteId, DateTime addDate, string addUserName, int contentLength, string templateContent)
        {
            Id = id;
            TemplateId = templateId;
            SiteId = siteId;
            AddDate = addDate;
            AddUserName = addUserName;
            ContentLength = contentLength;
            TemplateContent = templateContent;
        }

        public int Id { get; set; }

        public int TemplateId { get; set; }

        public int SiteId { get; set; }

        public DateTime AddDate { get; set; }

        public string AddUserName { get; set; }

        public int ContentLength { get; set; }

        public string TemplateContent { get; set; }
    }
}
