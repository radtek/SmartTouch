using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.IO;

namespace SmartTouchSqli18n
{
    public class Sqli18nService
    {
        private readonly DbConnection _conn;
        public Sqli18nService()
        {
            _conn = DataManager.GetConnection();
            _conn.Open();
        }

        public IEnumerable<i18nMessage> GetMessages()
        {
            var messages = _conn.Query<i18nMessage>(@"select distinct [Name] as MessageId from menuitems
                                                union
                                                select distinct [ToolTip] from MenuItems
                                                union
                                                select distinct [Area] from MenuItems
                                                union
                                                select distinct RoleName from Roles
                                                union
                                                select distinct Name from Statuses
                                                union
                                                select TagTypeName from TagTypes
                                                union
                                                select UserAuditTypeName from UserAuditTypes
                                                union
                                                select TriggerName from WorkflowTriggerTypes
                                                union
                                                select DropDownName from Dropdowns
                                                union
                                                select DropDownValue from DropdownValues
                                                union
                                                select distinct DefaultDescription from DropdownValueTypes
                                                union
                                                select WorkflowActionName as MessageId FROM WorkflowActionTypes
                                                union
                                                select ModuleName from Modules
                                                union
                                                select [Title] from LeadAdapterCommunicationTypes
                                                union
                                                select [Name] from LeadAdapterTypes
                                                union
                                                select [Title] from LeadAdapterJobStatus
                                                union
                                                select [Name] from NotificationStatuses
                                                union
                                                select [ReportName] from Reports
                                                union
                                                select [ActivityName] from UserActivities
                                                order by MessageId Asc");

            return messages.Where(m => (!string.IsNullOrEmpty(m.MessageId) && !m.MessageId.Contains("icon")));
        }

        public IEnumerable<i18nMessage> GetMessages(string path)
        {
            var messages = new List<i18nMessage>();
            using (var sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    var s = sr.ReadLine();
                    if (Compare(s))
                    {
                        messages.Add(new i18nMessage()
                        {
                            MessageId = s,
                            Translation = sr.ReadLine()
                        });
                    }

                }
            }
            messages.Where(x => !string.IsNullOrEmpty(x.MessageId)).ToList().ForEach(m =>
            {
                m.MessageId = m.MessageId.Replace("msgid \"","").Replace("\"","");
            });
            return messages;
        }

        private bool Compare(string p)
        {
            if (p.StartsWith("msgid"))
                return true;
            return false;
        }
    }
}
