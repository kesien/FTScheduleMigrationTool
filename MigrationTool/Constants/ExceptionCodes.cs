namespace MigrationTool.Constants
{
    public static class ExceptionCodes
    {
        // Auth codes
        public const int InvalidUserNameOrPassword = 3100;
        public const int OldPasswordDidNotMatch = 3101;
        public const int RefreshTokenExpiredOrInvalid = 3102;
        public const int NotEnoughPrivileges = 3103;

        // Not Exist codes
        public const int UserDoesNotExist = 3200;
        public const int LocationDoesNotExist = 3201;
        public static int RequestDoesNotExist = 3202;
        public const int SeatDoesNotExist = 3203;
        public const int AbsenceDoesNotExist = 3204;
        public const int RoleDoesNotExist = 3205;
        public const int WorkScheduleDoesNotExist = 3206;
        public const int ToDoDoesNotExist = 3207;
        public static int DepartmentDoesNotExist = 3208;
        public static int EventDoesNotExist = 3209;


        // Missing data codes
        public const int DateNotProvided = 3300;

        // Already exist codes
        public const int LocationAlreadyExist = 3400;
        public const int AbsenceAlreadyExist = 3401;
        public const int SeatAlreadyExist = 3402;
        public const int WorkScheduleAlreadyExist = 3403;
        public static int RequestForThisDateAlreadyExist = 3404;
        public static int ScheduleAlreadyExist = 3405;
        public static int UserAlreadyExist = 3406;
        public static int SeatAlreadyTaken = 3407;
        public static int RequestForThisSeatAlreadyExist = 3408;
        public static int ToDoAlreadyExist = 3409;
        public static int DepartmentAlreadyExist = 3410;
        public static int EventForThisDateAlreadyExist = 3411;


        // Misc
        public const int UserNotAvailable = 3500;
        public const int TranslationDoesNotExist = 3501;
        public const int ToDoAlreadyAssigned = 3502;

    }
}
