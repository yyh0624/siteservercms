﻿using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using SiteServer.Utils;
using BaiRong.Text.LitJson;
using SiteServer.CMS.Core;
using SiteServer.Utils.Enumerations;

namespace SiteServer.BackgroundPages.Cms
{
    public class ModalTextEditorInsertAudio : BasePageCms
    {
        public TextBox TbPlayUrl;
        public CheckBox CbIsAutoPlay;

        private string _attributeName;

        public static string GetOpenWindowString(int siteId, string attributeName)
        {
            return LayerUtils.GetOpenScript("插入音频", PageUtils.GetCmsUrl(siteId, nameof(ModalTextEditorInsertAudio), new NameValueCollection
            {
                {"AttributeName", attributeName}
            }), 600, 400);
        }

        public string UploadUrl => PageUtils.GetCmsUrl(SiteId, nameof(ModalTextEditorInsertAudio), new NameValueCollection
        {
            {"upload", true.ToString()}
        });

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            if (Body.IsQueryExists("upload"))
            {
                var json = JsonMapper.ToJson(Upload());
                Response.Write(json);
                Response.End();
                return;
            }

            _attributeName = Body.GetQueryString("AttributeName");
        }

        public string TypeCollection => SiteInfo.Additional.VideoUploadTypeCollection;

        private Hashtable Upload()
        {
            var success = false;
            var playUrl = string.Empty;
            var message = "音频上传失败";

            if (Request.Files["filedata"] != null)
            {
                var postedFile = Request.Files["filedata"];
                try
                {
                    if (!string.IsNullOrEmpty(postedFile?.FileName))
                    {
                        var filePath = postedFile.FileName;
                        var fileExtName = PathUtils.GetExtension(filePath);

                        var isAllow = true;
                        if (!PathUtility.IsVideoExtenstionAllowed(SiteInfo, fileExtName))
                        {
                            message = "此格式不允许上传，请选择有效的音频文件";
                            isAllow = false;
                        }
                        if (!PathUtility.IsVideoSizeAllowed(SiteInfo, postedFile.ContentLength))
                        {
                            message = "上传失败，上传文件超出规定文件大小";
                            isAllow = false;
                        }

                        if (isAllow)
                        {
                            var localDirectoryPath = PathUtility.GetUploadDirectoryPath(SiteInfo, fileExtName);
                            var localFileName = PathUtility.GetUploadFileName(SiteInfo, filePath);
                            var localFilePath = PathUtils.Combine(localDirectoryPath, localFileName);

                            postedFile.SaveAs(localFilePath);
                            playUrl = PageUtility.GetSiteUrlByPhysicalPath(SiteInfo, localFilePath, true);
                            playUrl = PageUtility.GetVirtualUrl(SiteInfo, playUrl);
                            success = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
            }

            var jsonAttributes = new Hashtable();
            if (success)
            {
                jsonAttributes.Add("success", "true");
                jsonAttributes.Add("playUrl", playUrl);
            }
            else
            {
                jsonAttributes.Add("success", "false");
                jsonAttributes.Add("message", message);
            }

            return jsonAttributes;
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            var playUrl = TbPlayUrl.Text;

            var script = "parent." + ETextEditorTypeUtils.GetInsertAudioScript(_attributeName, playUrl, CbIsAutoPlay.Checked);
            LayerUtils.CloseWithoutRefresh(Page, script);
        }

    }
}
