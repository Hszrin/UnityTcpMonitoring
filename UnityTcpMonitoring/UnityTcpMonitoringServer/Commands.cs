namespace UnityTcpMonitoringServer
{
    public static class Commands
    {
        public const string RemoveMachine = "REMOVE_MACHINE";
        public const string ImportExcel = "IMPORT_EXCEL";
        public const string AddMachine = "ADD_MACHINE";
        public const string ControlMachine = "CONTROL_MACHINE";
        public const string InitMachine = "INIT_MACHINE";
        public const string InitLine = "INIT_LINE";
        public const string ControlLine = "CONTROL_LINE";
        public const string RefreshLine = "REFRESH_LINE";
        public const string RefreshMachine = "REFRESH_LIST";
        public const string Heartbeat = "Heartbeat";
        public const string ProductionUpdate = "ProductionUpdate";
    }
}
