namespace SmartTouch.CRM.Entities
{
    public enum TrackActionProcessStatus : short
    {
        Undefined = 0,
        ReadyToProcess = 801,
        Executed = 802,
        Termintaed = 803,
        Paused = 804,
        SubWorkflow = 805,
        Error = 806,
    }
}
