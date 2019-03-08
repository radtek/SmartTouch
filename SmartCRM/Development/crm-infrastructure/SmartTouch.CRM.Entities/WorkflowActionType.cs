using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum WorkflowActionType : byte
    {
        SendCampaign = 1,
        SendText = 2,
        SetTimer = 3,
        AddTag = 4,
        RemoveTag = 5,
        AdjustLeadScore = 6,
        ChangeLifecycle = 7,
        UpdateField = 8,
        AssignToUser = 9,
        NotifyUser = 10,
        WorkflowEndState = 11,
        SendEmail = 12,
        TriggerWorkflow = 13,
        LinkActions = 14
    }
}
