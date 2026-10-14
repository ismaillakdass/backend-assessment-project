using System.Security.Claims;
using Assessment.Domain.Entities;
using Assessment.Domain.Entities.Identity;
using Assessment.Domain.Enums;
using Assessment.Infrastructure.Persistence;
using Assessment.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Assessment.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        if (await context.Donors.AnyAsync())
        {
            logger.LogInformation("Database already contains data. Skipping seeding.");
            return;
        }

        logger.LogInformation("Starting deterministic database seeding...");

        // 1. Seed Permissions
        var allPermissions = Permissions.GetAll();
        var permissionEntities = new List<Permission>();
        foreach (var permName in allPermissions)
        {
            var parts = permName.Split('.');
            var module = parts.Length > 1 ? parts[0] : "General";
            var action = parts.Length > 1 ? parts[1] : parts[0];

            var perm = new Permission
            {
                Name = permName,
                Module = module,
                Description = $"Grants permission to {action} {module}",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
            permissionEntities.Add(perm);
        }
        await context.Permissions.AddRangeAsync(permissionEntities);
        await context.SaveChangesAsync();

        // 2. Seed Roles
        var adminRole = new ApplicationRole { Name = "Admin", Description = "System Administrator with full access" };
        var operatorRole = new ApplicationRole { Name = "Operator", Description = "Operations staff with approve and manage access" };
        var viewerRole = new ApplicationRole { Name = "Viewer", Description = "Read-only access" };

        await roleManager.CreateAsync(adminRole);
        await roleManager.CreateAsync(operatorRole);
        await roleManager.CreateAsync(viewerRole);

        // 3. Seed RolePermissions
        var rolePermissions = new List<RolePermission>();

        // Admin has all permissions
        foreach (var p in permissionEntities)
        {
            rolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id });
            await roleManager.AddClaimAsync(adminRole, new Claim("Permission", p.Name));
        }

        // Operator has view, create, approve
        var operatorPermNames = new HashSet<string>
        {
            Permissions.Donations.View,
            Permissions.Donations.Create,
            Permissions.Donations.Approve,
            Permissions.Donors.View,
            Permissions.Donors.Create,
            Permissions.Volunteers.View,
            Permissions.Events.View,
            Permissions.Reports.View
        };
        foreach (var p in permissionEntities.Where(p => operatorPermNames.Contains(p.Name)))
        {
            rolePermissions.Add(new RolePermission { RoleId = operatorRole.Id, PermissionId = p.Id });
            await roleManager.AddClaimAsync(operatorRole, new Claim("Permission", p.Name));
        }

        // Viewer has only view permissions
        var viewerPermNames = new HashSet<string>
        {
            Permissions.Donations.View,
            Permissions.Donors.View,
            Permissions.Volunteers.View,
            Permissions.Events.View
        };
        foreach (var p in permissionEntities.Where(p => viewerPermNames.Contains(p.Name)))
        {
            rolePermissions.Add(new RolePermission { RoleId = viewerRole.Id, PermissionId = p.Id });
            await roleManager.AddClaimAsync(viewerRole, new Claim("Permission", p.Name));
        }

        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();

        // 4. Seed Users
        const string defaultPassword = "P@ssword123!";

        var adminUser = new ApplicationUser
        {
            UserName = "admin@losev.org",
            Email = "admin@losev.org",
            FirstName = "Sistem",
            LastName = "Yöneticisi",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        await userManager.CreateAsync(adminUser, defaultPassword);
        await userManager.AddToRoleAsync(adminUser, "Admin");

        var operatorUser = new ApplicationUser
        {
            UserName = "operator@losev.org",
            Email = "operator@losev.org",
            FirstName = "Bağış",
            LastName = "Operatörü",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        await userManager.CreateAsync(operatorUser, defaultPassword);
        await userManager.AddToRoleAsync(operatorUser, "Operator");

        var viewerUser = new ApplicationUser
        {
            UserName = "viewer@losev.org",
            Email = "viewer@losev.org",
            FirstName = "Gözlemci",
            LastName = "Kullanıcı",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        await userManager.CreateAsync(viewerUser, defaultPassword);
        await userManager.AddToRoleAsync(viewerUser, "Viewer");

        // 5. Seed Donors (Deterministic 100 Donors)
        string[] firstNames = { "Ahmet", "Mehmet", "Mustafa", "Ali", "Hüseyin", "Hasan", "İbrahim", "İsmail", "Osman", "Halil",
                                "Ayşe", "Fatma", "Emine", "Hatice", "Zeynep", "Elif", "Meryem", "Şerife", "Zehra", "Sultan" };
        string[] lastNames = { "Yılmaz", "Kaya", "Demir", "Çelik", "Şahin", "Yıldız", "Yıldırım", "Öztürk", "Aydın", "Özdemir",
                               "Arslan", "Doğan", "Kılıç", "Aslan", "Çetin", "Kara", "Koç", "Kurt", "Özkan", "Şimşek" };

        var donors = new List<Donor>();
        var random = new Random(19980527); // Fixed seed for reproducible data

        for (int i = 1; i <= 100; i++)
        {
            var fn = firstNames[(i - 1) % firstNames.Length];
            var ln = lastNames[((i - 1) * 3) % lastNames.Length];
            var email = $"{fn.ToLower()}.{ln.ToLower()}{i}@bagisci.org"
                .Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");

            var donor = new Donor
            {
                Id = i,
                FirstName = fn,
                LastName = ln,
                Email = email,
                Phone = $"+90 532 {100 + i:D3} {i * 11 % 90 + 10:D2} {i * 17 % 90 + 10:D2}",
                IsActive = i != 95 && i != 96, // Donor 95 and 96 are deliberately inactive!
                TotalDonatedAmount = 0m,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(i)
            };
            donors.Add(donor);
        }

        // We temporarily turn off identity insert or set explicit IDs
        await context.Donors.AddRangeAsync(donors);
        await context.SaveChangesAsync();

        // 6. Seed Donations (Deterministic 500 Donations)
        var donations = new List<Donation>();
        decimal[] standardAmounts = { 50m, 100m, 250m, 500m, 750m, 1000m, 2500m, 5000m, 10000m };
        var startDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        int donationIdCounter = 1;

        // Donor 1 gets 0 donations (Edge case)
        // Donor 2 gets 35 donations (High-volume frequent donor)
        for (int d = 1; d <= 35; d++)
        {
            var amt = standardAmounts[random.Next(standardAmounts.Length)];
            var donation = new Donation
            {
                Id = donationIdCounter++,
                DonorId = 2,
                Amount = amt,
                DonationType = DonationType.Monthly,
                DonationDate = startDate.AddDays(d * 8),
                Status = DonationStatus.Approved,
                ApprovedByUserId = operatorUser.Id,
                ApprovedAt = startDate.AddDays(d * 8).AddHours(2),
                Notes = $"Düzenli aylık bağış #{d}",
                CreatedAt = startDate.AddDays(d * 8)
            };
            donations.Add(donation);
        }

        // Distribute remaining 465 donations among Donors 3 through 94
        for (int i = 36; i <= 500; i++)
        {
            var donorId = random.Next(3, 95); // skip inactive 95,96 and empty donor 1
            var amt = standardAmounts[random.Next(standardAmounts.Length)];
            var dType = (DonationType)random.Next(1, 5);

            // Mix statuses: 70% Approved, 20% Pending, 5% Cancelled, 5% Rejected
            var statusRoll = random.Next(100);
            DonationStatus status;
            string? approvedBy = null;
            DateTime? approvedAt = null;

            if (statusRoll < 70)
            {
                status = DonationStatus.Approved;
                approvedBy = operatorUser.Id;
                approvedAt = startDate.AddDays(random.Next(1, 300)).AddHours(4);
            }
            else if (statusRoll < 90)
            {
                status = DonationStatus.Pending;
            }
            else if (statusRoll < 95)
            {
                status = DonationStatus.Cancelled;
            }
            else
            {
                status = DonationStatus.Rejected;
            }

            var donationDate = startDate.AddDays(random.Next(1, 300));
            var donation = new Donation
            {
                Id = donationIdCounter++,
                DonorId = donorId,
                Amount = amt,
                DonationType = dType,
                DonationDate = donationDate,
                Status = status,
                ApprovedByUserId = approvedBy,
                ApprovedAt = approvedAt,
                Notes = status == DonationStatus.Approved ? "Onaylandı." : null,
                CreatedAt = donationDate
            };
            donations.Add(donation);
        }

        await context.Donations.AddRangeAsync(donations);
        await context.SaveChangesAsync();

        // Update each donor's TotalDonatedAmount based on approved donations
        var approvedTotals = donations
            .Where(d => d.Status == DonationStatus.Approved)
            .GroupBy(d => d.DonorId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        foreach (var donor in donors)
        {
            if (approvedTotals.TryGetValue(donor.Id, out var total))
            {
                donor.TotalDonatedAmount = total;
            }
        }
        await context.SaveChangesAsync();

        // 7. Seed Volunteers (80 Volunteers)
        string[] cities = { "Ankara", "İstanbul", "İzmir", "Bursa", "Antalya" };
        string[] skillsList = { "Etkinlik Organizasyonu", "İlk Yardım", "Sosyal Medya ve Fotoğrafçılık", "Tercümanlık", "Lojistik Destek", "Psikolojik Destek" };

        var volunteers = new List<Volunteer>();
        for (int i = 1; i <= 80; i++)
        {
            var fn = firstNames[(i + 5) % firstNames.Length];
            var ln = lastNames[(i * 2) % lastNames.Length];
            var city = cities[(i - 1) % cities.Length];
            var skill = skillsList[(i - 1) % skillsList.Length];

            var vol = new Volunteer
            {
                Id = i,
                FirstName = fn,
                LastName = ln,
                Email = $"{fn.ToLower()}.{ln.ToLower()}{i}@gonullu.org"
                    .Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s").Replace("ö", "o").Replace("ç", "c"),
                Phone = $"+90 542 {200 + i:D3} {i * 13 % 90 + 10:D2} {i * 19 % 90 + 10:D2}",
                City = city,
                Skills = skill,
                IsActive = i != 75, // Volunteer 75 is inactive!
                CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(i)
            };
            volunteers.Add(vol);
        }

        await context.Volunteers.AddRangeAsync(volunteers);
        await context.SaveChangesAsync();

        // 8. Seed Events (20 Events)
        string[] eventNames =
        {
            "LÖSEV Kanserle Savaş Haftası Semineri",
            "Umut Koşusu ve Dayanışma Maratonu",
            "Gönüllü Oryantasyon ve Eğitim Kampı",
            "Mandalina Şenliği ve Kermes",
            "Çocuklar İçin Sanat Atölyesi",
            "Kan ve Kök Hücre Bağışı Farkındalık Günü",
            "Yılbaşı Umut Paketi Dağıtımı",
            "Gençlik Festivali ve Müzik Dinletisi",
            "Sağlıklı Yaşam ve Beslenme Paneli",
            "Lösemili Çocuklar Köyü Bahar Şenliği",
            "Fidan Dikim Etkinliği",
            "Kurumsal Dayanışma Kahvaltısı",
            "Gönüllü Teşekkür Gecesi",
            "Pedal Çevir Umut Ol Bisiklet Turu",
            "Hastaneler Ziyareti ve Hediye Dağıtımı",
            "LÖSEV Dükkan Kermesi",
            "Yaz Kampı Destek Programı",
            "Okul Farkındalık Seminerleri",
            "Uluslararası Kanser Kongresi LÖSEV Standı",
            "Yıl Sonu Değerlendirme ve Planlama Toplantısı"
        };

        var events = new List<Event>();
        for (int i = 1; i <= 20; i++)
        {
            var ev = new Event
            {
                Id = i,
                Name = eventNames[i - 1],
                City = cities[(i - 1) % cities.Length],
                Capacity = i == 1 ? 5 : 20 + i * 2, // Event 1 has capacity 5!
                EventDate = DateTime.UtcNow.AddDays(i * 7 - 35), // Some in the past, some in the future
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(i)
            };
            events.Add(ev);
        }

        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();

        // 9. Seed EventVolunteers
        var eventVolunteers = new List<EventVolunteer>();
        // Event 1 has 5 volunteers assigned -> Reached capacity (5/5)!
        for (int v = 1; v <= 5; v++)
        {
            eventVolunteers.Add(new EventVolunteer
            {
                EventId = 1,
                VolunteerId = v,
                AssignedAt = DateTime.UtcNow.AddDays(-10 + v)
            });
        }

        // Randomly assign a few volunteers to other events
        for (int e = 2; e <= 20; e++)
        {
            int assignedCount = (e * 3) % 7;
            for (int k = 1; k <= assignedCount; k++)
            {
                var volId = (e * 7 + k) % 74 + 1; // avoid inactive 75
                if (!eventVolunteers.Any(ev => ev.EventId == e && ev.VolunteerId == volId))
                {
                    eventVolunteers.Add(new EventVolunteer
                    {
                        EventId = e,
                        VolunteerId = volId,
                        AssignedAt = DateTime.UtcNow.AddDays(-k)
                    });
                }
            }
        }

        await context.EventVolunteers.AddRangeAsync(eventVolunteers);
        await context.SaveChangesAsync();

        logger.LogInformation("Database seeding completed successfully: 100 Donors, 500 Donations, 80 Volunteers, 20 Events, Roles & Permissions.");
    }
}
