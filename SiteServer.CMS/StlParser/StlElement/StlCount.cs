﻿using System;
using System.Collections.Generic;
using SiteServer.Utils;
using SiteServer.CMS.Core;
using SiteServer.CMS.StlParser.Cache;
using SiteServer.CMS.StlParser.Model;
using SiteServer.CMS.StlParser.Utility;
using SiteServer.Utils.Enumerations;

namespace SiteServer.CMS.StlParser.StlElement
{
    [Stl(Usage = "显示数值", Description = "通过 stl:count 标签在模板中显示统计数字")]
    public class StlCount
	{
        private StlCount() { }
		public const string ElementName = "stl:count";

		public const string AttributeType = "type";
        public const string AttributeChannelIndex = "channelIndex";
        public const string AttributeChannelName = "channelName";
        public const string AttributeUpLevel = "upLevel";
        public const string AttributeTopLevel = "topLevel";
        public const string AttributeScope = "scope";
        public const string AttributeSince = "since";

        public static SortedList<string, string> AttributeList => new SortedList<string, string>
        {
            {AttributeType, StringUtils.SortedListToAttributeValueString("需要获取值的类型", TypeList)},
            {AttributeChannelIndex, "栏目索引"},
            {AttributeChannelName, "栏目名称"},
            {AttributeUpLevel, "上级栏目的级别"},
            {AttributeTopLevel, "从首页向下的栏目级别"},
            {AttributeScope, "内容范围"},
            {AttributeSince, "时间段"}
        };

        public const string TypeChannels = "Channels";
        public const string TypeContents = "Contents";
        public const string TypeDownloads = "Downloads";

        public static SortedList<string, string> TypeList => new SortedList<string, string>
        {
            {TypeChannels, "栏目数"},
            {TypeContents, "内容数"},
            {TypeDownloads, "下载次数"}
        };

        public static string Parse(PageInfo pageInfo, ContextInfo contextInfo)
		{
		    var type = string.Empty;
            var channelIndex = string.Empty;
            var channelName = string.Empty;
            var upLevel = 0;
            var topLevel = -1;
            var scope = EScopeType.Self;
            var since = string.Empty;

		    foreach (var name in contextInfo.Attributes.Keys)
		    {
		        var value = contextInfo.Attributes[name];

                if (StringUtils.EqualsIgnoreCase(name, AttributeType))
                {
                    type = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeChannelIndex))
                {
                    channelIndex = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeChannelName))
                {
                    channelName = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeUpLevel))
                {
                    upLevel = TranslateUtils.ToInt(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeTopLevel))
                {
                    topLevel = TranslateUtils.ToInt(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeScope))
                {
                    scope = EScopeTypeUtils.GetEnumType(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeSince))
                {
                    since = value;
                }
            }

            return ParseImpl(pageInfo, contextInfo, type, channelIndex, channelName, upLevel, topLevel, scope, since);
		}

        private static string ParseImpl(PageInfo pageInfo, ContextInfo contextInfo, string type, string channelIndex, string channelName, int upLevel, int topLevel, EScopeType scope, string since)
        {
            var count = 0;

            var sinceDate = DateUtils.SqlMinValue;
            if (!string.IsNullOrEmpty(since))
            {
                sinceDate = DateTime.Now.AddHours(-DateUtils.GetSinceHours(since));
            }

            if (string.IsNullOrEmpty(type) || StringUtils.EqualsIgnoreCase(type, TypeContents))
            {
                var channelId = StlDataUtility.GetNodeIdByLevel(pageInfo.SiteId, contextInfo.ChannelId, upLevel, topLevel);
                channelId = StlDataUtility.GetNodeIdByChannelIdOrChannelIndexOrChannelName(pageInfo.SiteId, channelId, channelIndex, channelName);

                var nodeInfo = ChannelManager.GetChannelInfo(pageInfo.SiteId, channelId);

                //var nodeIdList = DataProvider.ChannelDao.GetIdListByScopeType(nodeInfo.NodeId, nodeInfo.ChildrenCount, scope, string.Empty, string.Empty);
                var nodeIdList = Node.GetIdListByScopeType(nodeInfo.Id, nodeInfo.ChildrenCount, scope, string.Empty, string.Empty);
                foreach (var nodeId in nodeIdList)
                {
                    var tableName = ChannelManager.GetTableName(pageInfo.SiteInfo, nodeId);
                    //count += DataProvider.ContentDao.GetCountOfContentAdd(tableName, pageInfo.SiteId, nodeId, EScopeType.Self, sinceDate, DateTime.Now.AddDays(1), string.Empty);
                    count += Content.GetCountOfContentAdd(tableName, pageInfo.SiteId, nodeId, EScopeType.Self, sinceDate, DateTime.Now.AddDays(1), string.Empty);
                }
            }
            else if (StringUtils.EqualsIgnoreCase(type, TypeChannels))
            {
                var channelId = StlDataUtility.GetNodeIdByLevel(pageInfo.SiteId, contextInfo.ChannelId, upLevel, topLevel);
                channelId = StlDataUtility.GetNodeIdByChannelIdOrChannelIndexOrChannelName(pageInfo.SiteId, channelId, channelIndex, channelName);

                var nodeInfo = ChannelManager.GetChannelInfo(pageInfo.SiteId, channelId);
                count = nodeInfo.ChildrenCount;
            }           
            else if (StringUtils.EqualsIgnoreCase(type, TypeDownloads))
            {
                if (contextInfo.ContentId > 0)
                {
                    count = CountManager.GetCount(pageInfo.SiteInfo.TableName, contextInfo.ContentId.ToString(), ECountType.Download);
                }
            }

            return count.ToString();
        }
	}
}
