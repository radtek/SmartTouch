using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;

namespace SmartTouch.CRM.Automation.Core.States
{
    public class WaitingPeriodState : State
    {
        TimerType timerType;
        int? delayPeriod;
        DateInterval? dateInterval;
        RunOn? runOn;
        RunType? runType;
        TimeSpan? runAt;
        DateTime? runOnDate; 
        DateTime? startDate; 
        DateTime? endDate;
        int workflowId;
        IEnumerable<DayOfWeek> runOnDays;
        IWorkflowService workflowService;

        IPublishSubscribeService pubSubService;

        public WaitingPeriodState(int stateId, TimerType timerType, int? delayPeriod, DateInterval? dateInterval,
            RunOn? runOn, TimeSpan? runAt, RunType? runType, DateTime? runOnDate, DateTime? startDate, DateTime? endDate,
            IEnumerable<DayOfWeek> runOnDays, int workflowId, IWorkflowService workflowService, IPublishSubscribeService pubSubService)
            : base(stateId)
        {
            this.timerType = timerType;
            this.delayPeriod = delayPeriod;
            this.dateInterval = dateInterval;
            this.runOn = runOn;
            this.runAt = runAt;
            this.runType = runType;
            this.runOnDate = runOnDate;
            this.startDate = startDate;
            this.endDate = endDate;
            this.runOnDays = runOnDays;
            this.workflowId = workflowId;
            this.pubSubService = pubSubService;
            this.workflowService = workflowService;
        }

        public WaitingPeriodState(int workflowActionId, IWorkflowService workflowService)
            : base((int)WorkflowActionType.SetTimer)
        {
            this.StateId = workflowActionId;
            this.workflowService = workflowService;
        }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            Logger.Current.Informational("Waiting period state entered" + ", message: " + message.MessageId);
            if (message.LeadScoreConditionType != (int)LeadScoreConditionType.TriggerWorkflow && !CanEnterState(message))                
                    return WorkflowStateTransitionStatus.UnAuthorizedTransition;

            try
            {
                Logger.Current.Verbose("Inside waiting period state for contactId " + message.ContactId + " in " + workflowId);
                bool canTransit = canTransitToNextState(message);
                this.OnEntry(message);
                if (canTransit)
                    return this.TransitionState.Transit(message);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error occured inside wait period state for contact " + message.ContactId + " in workflow :" + workflowId + " messageId : " + message.MessageId);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return WorkflowStateTransitionStatus.TransitionFailed;
            }
            return WorkflowStateTransitionStatus.TransitionDelayed;
        }

        public override void OnEntry(MessageQueues.Message message)
        {
            workflowService.InsertContactWorkflowAudit(new InsertContactWorkflowAuditRequest()
            {
                WorkflowId = message.WorkflowId,
                WorkflowActionId = StateId,
                ContactId = message.ContactId,
                AccountId = message.AccountId,
                MessageId = message.MessageId
            });
        }

        bool canTransitToNextState(Message message)
        {
            DateTime date = DateTime.UtcNow;
            if (timerType == TimerType.TimeDelay)
            {
                if (dateInterval.Value == DateInterval.Years)
                    date = date.AddYears(delayPeriod.Value);
                else if (dateInterval.Value == DateInterval.Months)
                    date = date.AddMonths(delayPeriod.Value);
                else if (dateInterval.Value == DateInterval.Weeks)
                    date = date.AddDays(delayPeriod.Value * 7);
                else if (dateInterval.Value == DateInterval.Days)
                    date = date.AddDays(delayPeriod.Value);
                else if (dateInterval.Value == DateInterval.Hours)
                    date = date.AddHours(delayPeriod.Value);
                else if (dateInterval.Value == DateInterval.Minutes)
                    date = date.AddMinutes(delayPeriod.Value);
                else if (dateInterval.Value == DateInterval.Seconds)
                    date = date.AddSeconds(delayPeriod.Value);

                if (runOn.Value == RunOn.Weekday)
                    date = date.AddDays(daysToWeekday(date.DayOfWeek));

                scheduleMessage(message, date, runAt);
                return false;
            }
            else if (timerType == TimerType.Date)
            {
                if (runOnDate.HasValue)
                {
                    if (runOnDate.Value < date)
                        return true;
                    else if (runOnDate.Value > date)
                    {
                        date = runOnDate.Value;
                        scheduleMessage(message, date, runAt);
                        return false;
                    }
                }
                else
                {
                    if (endDate.Value < date)
                        return true;
                    else if (date > startDate.Value && date < endDate.Value)
                        return true;
                    else if (date < startDate.Value)
                    {
                        scheduleMessage(message, startDate.Value, runAt);
                        return false;
                    }
                }
            }
            else if (timerType == TimerType.Week)
            {
                if (runOnDays.Contains(date.DayOfWeek))
                    return true;
                else
                {
                    IDictionary<DayOfWeek, int> days = new Dictionary<DayOfWeek, int>();
                    foreach (var day in runOnDays)
                    {
                        int daysUntilDayOfWeek = ((int)day - (int)date.DayOfWeek + 7) % 7;
                        days.Add(day, daysUntilDayOfWeek);
                    }
                    var minDays = days.Min(d => d.Value);
                    DateTime nearestDay = date.AddDays(minDays);
                    scheduleMessage(message, nearestDay, null);
                    return false;
                }
            }
            return true;
        }

        void scheduleMessage(Message message, DateTime date, TimeSpan? _runAt)
        {
            if (_runAt.HasValue)
            {
                var runAtTime = new DateTime(_runAt.Value.Ticks);
                var utcRunAt = runAtTime.ToUniversalTime();

                if (date.TimeOfDay < utcRunAt.TimeOfDay)
                    date = date.Add(utcRunAt.TimeOfDay - date.TimeOfDay);
                else
                    date = date.Subtract(date.TimeOfDay - utcRunAt.TimeOfDay);
            }
            date = TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? date.AddHours(-1) : date;
            Message newMessage = message.DeepClone();
            newMessage.LeadScoreConditionType = (int)LeadScoreConditionType.ContactWaitPeriodEnded;
            newMessage.WorkflowId = workflowId;
            pubSubService.SendScheduledMessage(newMessage, date);
        }

        int daysToWeekday(DayOfWeek dayOfWeek)
        {
            if (dayOfWeek == DayOfWeek.Sunday)
                return 1;
            else if (dayOfWeek == DayOfWeek.Saturday)
                return 2;
            else
                return 0;
        }

        public override void OnExit(MessageQueues.Message message)
        {
            throw new NotImplementedException();
        }

    }
}
