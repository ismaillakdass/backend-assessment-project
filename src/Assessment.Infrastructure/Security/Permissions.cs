namespace Assessment.Infrastructure.Security;

public static class Permissions
{
    public static class Donations
    {
        public const string View = "donations.view";
        public const string Create = "donations.create";
        public const string Approve = "donations.approve";
        public const string Export = "donations.export";
    }

    public static class Donors
    {
        public const string View = "donors.view";
        public const string Create = "donors.create";
        public const string Edit = "donors.edit";
        public const string Delete = "donors.delete";
    }

    public static class Volunteers
    {
        public const string View = "volunteers.view";
        public const string Manage = "volunteers.manage";
    }

    public static class Events
    {
        public const string View = "events.view";
        public const string Manage = "events.manage";
    }

    public static class Reports
    {
        public const string View = "reports.view";
    }

    public static IReadOnlyList<string> GetAll()
    {
        return new List<string>
        {
            Donations.View,
            Donations.Create,
            Donations.Approve,
            Donations.Export,
            Donors.View,
            Donors.Create,
            Donors.Edit,
            Donors.Delete,
            Volunteers.View,
            Volunteers.Manage,
            Events.View,
            Events.Manage,
            Reports.View
        };
    }
}
