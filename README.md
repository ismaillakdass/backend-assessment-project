# LÖSEV Benzeri STK Yönetim Sistemi - .NET Backend Take-Home Assessment

Hoş geldiniz! Bu proje, **Mid ve Senior .NET Backend Developer** adaylarının kurumsal yazılım geliştirme, mimari tasarım, kod okuma, hata ayıklama, veritabanı optimizasyonu, güvenlik ve test yetkinliklerini ölçmek amacıyla hazırlanmış gerçekçi bir vaka çalışmasıdır (Take-Home Coding Assessment).

Bu proje boş bir şablon değildir. Gerçek bir kurumsal backend projesini simüle eder:
- Kod tabanının büyük çoğunluğu (%70-80) iyi yapılandırılmış, temiz ve çalışan mimari desenlere sahiptir.
- Ancak sistemin belirli noktalarında (%20-30), gerçek hayattaki legacy sistemlerde veya zamanla teknik borç birikmiş projelerde karşılaşabileceğiniz **bilinçli olarak bırakılmış mimari, performans, güvenlik, transaction ve mantık hataları** yer almaktadır.

> [!IMPORTANT]
> **En Önemli Kural:**
> Proje ilk clone edildiğinde derlenir (build), SQLite veritabanı otomatik oluşur, deterministik seed verisi yüklenir ve API/Swagger ayağa kalkar.
> Amacımız sizi anlamsız compile hatalarıyla boğuşturmak değil; **mevcut kodu okuyabilme, problemleri teşhis edebilme, refactor edebilme ve modern standartlara taşıyabilme** vizyonunuzu değerlendirmektir.

---

## 1. Domain Açıklaması

Sistem, bir sivil toplum kuruluşunun (STK) temel operasyonlarını yönetir:

- **Donor (Bağışçı):** Bireysel veya kurumsal bağışçıların iletişim bilgileri, aktiflik durumları ve kümülatif bağış toplamları.
- **Donation (Bağış):** Bağışçıların yaptığı tek seferlik, düzenli (aylık), ayni veya kurumsal bağış kayıtları (Pending, Approved, Rejected, Cancelled).
- **Volunteer (Gönüllü):** Kuruluş çalışmalarına destek veren gönüllülerin yetenek, iletişim ve şehir bilgileri.
- **Event (Etkinlik):** Farkındalık, kermes, seminer vb. etkinlikler, kontenjan kapasiteleri ve gönüllü atamaları (`EventVolunteer`).
- **User / Role / Permission:** ASP.NET Core Identity ve DB tabanlı granüler yetkilendirme modeli.

---

## 2. Teknoloji Yığını

- **Platform:** .NET 9 (C# 13)
- **Web Framework:** ASP.NET Core Web API
- **ORM / Veritabanı:** Entity Framework Core 9, SQLite (Code-First)
- **Mimari:** Clean Architecture / N-Tier (Domain, Application, Infrastructure, API)
- **Desenler:** Repository Pattern, Unit of Work, Service Layer, DTO
- **Doğrulama & Eşleme:** FluentValidation, AutoMapper
- **Kimlik & Güvenlik:** ASP.NET Core Identity, JWT Bearer Tokens, Policy/Requirement/Handler Tabanlı Dinamik Yetkilendirme
- **API Sürümleme & Dokümantasyon:** Asp.Versioning (V1 & V2), Swashbuckle OpenAPI / Swagger UI
- **Loglama:** Serilog (Structured Logging)
- **Test:** xUnit, Moq, FluentAssertions
- **Konteyner:** Dockerfile & Docker Compose

---

## 3. Projeyi Çalıştırma

Projeyi çalıştırmak için harici bir veritabanı veya bulut servisi kurmanıza gerek yoktur. SQLite veritabanı ve deterministik seed verileri otomatik olarak ayağa kalkar.

### Gereksinimler
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- `dotnet-ef` CLI aracı (`dotnet tool install --global dotnet-ef` veya `dotnet tool update --global dotnet-ef`)

### Adımlar

1. Bağımlılıkları geri yükleyin:
   ```bash
   dotnet restore
   ```

2. Veritabanını güncelleyin (Migration uygulayın):
   ```bash
   dotnet ef database update --project src/Assessment.Infrastructure/Assessment.Infrastructure.csproj --startup-project src/Assessment.Api/Assessment.Api.csproj
   ```

3. API'yi çalıştırın:
   ```bash
   dotnet run --project src/Assessment.Api/Assessment.Api.csproj
   ```

4. Tarayıcınızda Swagger arayüzünü açın:
   - **Swagger UI:** [http://localhost:5000/](http://localhost:5000/) veya [http://localhost:5240/](http://localhost:5240/)
   - Swagger arayüzünde sağ üstteki açılır menüden **v1** ve **v2** API dokümanları arasında geçiş yapabilirsiniz.

5. Testleri çalıştırmak için:
   ```bash
   dotnet test
   ```

---

## 4. Deterministik Seed Veritabanı

Proje ilk ayağa kalktığında otomatik olarak aşağıdaki test verileri oluşturulur:

- **100 Bağışçı (Donor):** Çeşitli şehir ve profillerde.
  - *Edge Case:* Donor ID `1` hiç bağışı olmayan bağışçıdır.
  - *Edge Case:* Donor ID `2` 35'ten fazla düzenli bağışı bulunan sadık bağışçıdır.
  - *Edge Case:* Donor ID `95` ve `96` pasif (`IsActive = false`) bağışçılardır.
- **500 Bağış (Donation):** Onaylı (Approved), Beklemede (Pending), İptal (Cancelled) ve Reddedilmiş (Rejected) statülerinde.
- **80 Gönüllü (Volunteer):** Farklı yetenek ve şehirlerde.
  - *Edge Case:* Volunteer ID `75` pasif (`IsActive = false`) gönüllüdür.
- **20 Etkinlik (Event):**
  - *Edge Case:* Event ID `1` kontenjanı tamamen dolmuş (5/5 gönüllü atanmış) bir etkinliktir.

### Hazır Kullanıcı Hesapları (Kullanıcı Adı / Şifre)
Tüm kullanıcıların şifresi: `P@ssword123!`

| Rol | E-Posta | Açıklama |
|---|---|---|
| **Admin** | `admin@losev.org` | Tüm sistem yetkilerine (`donations.*`, `donors.*`, vb.) tam erişim |
| **Operator** | `operator@losev.org` | Bağış oluşturma, listeleme ve onaylama (`donations.approve`) yetkisi |
| **Viewer** | `viewer@losev.org` | Sadece okuma/görüntüleme yetkileri |

Giriş yapmak için `POST /api/v1/auth/login` endpoint'ini kullanıp dönen JWT token'ı Swagger'daki **Authorize** butonuna `Bearer {token}` formatında yapıştırabilirsiniz.

---

## 5. Değerlendirme Sistemi ve Puan Tablosu

Projede toplam **15 görev** bulunmaktadır. Toplam puan **150**'dir.

> [!NOTE]
> **Tüm görevleri tamamlamanız beklenmemektedir.**  
> İlgi alanınıza, güçlü olduğunu düşündüğünüz alanlara ve zamanınıza göre istediğiniz görevleri seçebilirsiniz.  
> Değerlendirmeye alınmak için **en az 50 puanlık** görevi tamamlamanız gerekmektedir.
> 
> *Örneğin: Sadece Görev 1'i (50 puan) çok iyi bir mimariyle tamamlayabilir ya da Görev 2, 3, 4 ve 6'yı bir arada seçebilirsiniz.*

> [!WARNING]
> Görevlerin puanı yalnızca işin kapsam ve zorluk derecesini ifade eder. Sadece "çalışan" ancak mimari, performans, güvenlik veya temiz kod prensiplerini göz ardı eden çözümler tam puan almayabilir.

| No | Görev Başlığı | Puan |
|:---:|---|:---:|
| **1** | [Legacy Endpoint'i V2'ye Taşıma ve Refactor](#görev-1---legacy-endpointi-v2ye-taşıma-ve-refactor-50-puan) | **50** |
| **2** | [EF Core Raporlama Performans Problemini Çözme](#görev-2---ef-core-raporlama-performans-problemini-çözme-20-puan) | **20** |
| **3** | [Authorization / DB-Backed Permission Sistemi](#görev-3---authorization--db-backed-permission-sistemi-15-puan) | **15** |
| **4** | [Toplu Güncelleme (Bulk Update) Problemi](#görev-4---toplu-güncelleme-bulk-update-problemi-10-puan) | **10** |
| **5** | [Transaction Yönetimi ve Veri Tutarlılığı](#görev-5---transaction-yönetimi-ve-veri-tutarlılığı-10-puan) | **10** |
| **6** | [Eksik Unit Testlerin Yazılması](#görev-6---eksik-unit-testlerin-yazılması-10-puan) | **10** |
| **7** | [FluentValidation Kurallarının Tamamlanması](#görev-7---fluentvalidation-kurallarının-tamamlanması-5-puan) | **5** |
| **8** | [Sunucu Taraflı Sayfalama (Pagination) & Filtreleme](#görev-8---sunucu-taraflı-sayfalama-pagination--filtreleme-5-puan) | **5** |
| **9** | [Merkezi Hata Yönetimi (RFC 7807 ProblemDetails)](#görev-9---merkezi-hata-yönetimi-rfc-7807-problemdetails-5-puan) | **5** |
| **10** | [Güvenli ve Yapısal Loglama (Structured Logging)](#görev-10---güvenli-ve-yapısal-loglama-structured-logging-5-puan) | **5** |
| **11** | [JWT Logout ve Token Kara Liste / İptal Mekanizması](#görev-11---jwt-logout-ve-token-kara-liste--iptal-mekanizması-5-puan) | **5** |
| **12** | [AutoMapper Entegrasyonu ve Kod Tekrarının Giderilmesi](#görev-12---automapper-entegrasyonu-ve-kod-tekrarının-giderilmesi-3-puan) | **3** |
| **13** | [Swagger / OpenAPI Dokümantasyonu](#görev-13---swagger--openapi-dokümantasyonu-3-puan) | **3** |
| **14** | [Docker Konteynerizasyon İyileştirmesi](#görev-14---docker-konteynerizasyon-iyileştirmesi-2-puan) | **2** |
| **15** | [Teknik Borç ve Mimari Kod İncelemesi (TECHNICAL_NOTES.md)](#görev-15---teknik-borç-ve-mimari-kod-incelemesi-2-puan) | **2** |
| | **TOPLAM** | **150** |

---

## 6. Görevlerin Detayları

### Görev 1 - Legacy Endpoint'i V2'ye Taşıma ve Refactor (50 Puan)
- **İlgili Dosyalar:** `src/Assessment.Api/Controllers/V1/DonationsController.cs` (`legacy-process`), `src/Assessment.Api/Controllers/V2/DonationsController.cs` (`Create`), `IDonationService`, `IUnitOfWork`.
- **Durum:** V1 altındaki bağış oluşturma endpoint'i doğrudan `ApplicationDbContext` kullanmakta, iş kurallarını controller içinde çalıştırmakta, N+1 sorgular yapmakta, tracking kullanmakta, validasyon yapmamakta, hata durumunda ham exception dönmekte ve veritabanı entity'sini doğrudan response olarak dönmektedir.
- **Beklenti:** Bu mantığı analiz edip `/api/v2/donations` altında temiz kurumsal mimariyle yeniden tasarlayın. Service Layer, Unit of Work / Repository, DTO, FluentValidation, AutoMapper, asenkron EF Core, transaction güvenliği ve doğru REST yanıtı (`201 Created` + Location header + `ApiResponse`) kullanılması beklenir.

### Görev 2 - EF Core Raporlama Performans Problemini Çözme (20 Puan)
- **İlgili Dosyalar:** `src/Assessment.Application/Services/DonorReportService.cs`, `src/Assessment.Api/Controllers/V1/ReportsController.cs`.
- **Durum:** Bağışçı özet raporu hazırlanırken tüm bağışçılar ve ilişkili tüm bağışlar EF tracking ile hafızaya çekilmekte, hafızada filtrelenmekte ve döngü içerisinde her bağışçı için tekrar veritabanına sorgu (N+1) atılmaktadır.
- **Beklenti:** Sorguyu optimize edin. `AsNoTracking()`, sunucu tarafı IQueryable projeksiyonu (`.Select(...)`), veritabanı düzeyinde filtreleme ve SQL aggregate fonksiyonları (`Sum`, `Count`, `Average`) kullanarak sorguyu tek ve optimize edilmiş bir SQL çıktısına dönüştürün.

### Görev 3 - Authorization / DB-Backed Permission Sistemi (15 Puan)
- **İlgili Dosyalar:** `src/Assessment.Infrastructure/Security/PermissionAuthorizationHandler.cs`, `src/Assessment.Api/Controllers/V2/DonationsController.cs` (`Approve`).
- **Durum:** `/api/v2/donations/{id}/approve` endpoint'i `donations.approve` iznine ihtiyaç duyar. Mevcut `PermissionAuthorizationHandler` sadece `Admin` rolünü kontrol etmekte, token'da claim bulunmadığında veya dinamik izinlerde kullanıcının veritabanındaki rol izinlerini (`RolePermissions` -> `Permissions`) kontrol etmemektedir. Bu nedenle `Operator` kullanıcısı haklı olduğu halde 403 Forbidden hatası almaktadır.
- **Beklenti:** Handler'ı ve yetkilendirme altyapısını hem claim hem de DB düzeyinde izin kontrolü yapacak şekilde tamamlayın.

### Görev 4 - Toplu Güncelleme (Bulk Update) Problemi (10 Puan)
- **İlgili Dosyalar:** `src/Assessment.Application/Services/DonationService.cs` (`BulkUpdateStatusAsync`), `src/Assessment.Infrastructure/Repositories/DonationRepository.cs`.
- **Durum:** Belirli statüdeki yüzlerce bağışı yeni statüye geçiren toplu güncelleme metodu, tüm kayıtları hafızaya çekmekte, `foreach` ile tek tek güncellemekte ve çok sayıda entity tracking maliyeti oluşturmaktadır.
- **Beklenti:** EF Core 9'un `ExecuteUpdateAsync` özelliğini kullanarak kayıtları hafızaya çekmeden tek bir SQL `UPDATE` komutu ile optimize edin.

### Görev 5 - Transaction Yönetimi ve Veri Tutarlılığı (10 Puan)
- **İlgili Dosyalar:** `src/Assessment.Application/Services/DonationService.cs` (`CreateDonationAsync`), `IUnitOfWork`.
- **Durum:** Bağış oluşturulurken sırasıyla bağış kaydedilmekte, bağışçının kümülatif bağış tutarı güncellenmekte ve audit log yazılmaktadır. Ancak her adımda bağımsız `SaveChangesAsync` çağrılmakta ve bir transaction kullanılmamaktadır. İkinci veya üçüncü adım hata verdiğinde veritabanı tutarsız (inconsistent) durumda kalmaktadır.
- **Beklenti:** `IUnitOfWork` üzerinden açık transaction (`BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`) yönetimi ekleyin. Hata durumunda rollback yapıldığını garanti edin.

### Görev 6 - Eksik Unit Testlerin Yazılması (10 Puan)
- **İlgili Dosyalar:** `tests/Assessment.UnitTests/Services/DonationServiceTests.cs`.
- **Durum:** Test dosyasında yalnızca başlangıç testi mevcut olup kritik iş senaryoları `TODO` olarak bırakılmıştır.
- **Beklenti:** `DonationService` için xUnit, Moq ve FluentAssertions kullanarak; başarılı bağış kaydı, bulunamayan bağışçı (`NotFoundException`), pasif bağışçı (`BusinessException`) ve geçersiz onaylama statüsü senaryolarını test eden kapsamlı unit testleri yazın.

### Görev 7 - FluentValidation Kurallarının Tamamlanması (5 Puan)
- **İlgili Dosyalar:** `src/Assessment.Application/Validators/CreateDonationDtoValidator.cs`.
- **Durum:** Bağış oluşturma modeli için yazılan validatörde sadece `DonorId` kontrolü vardır. Tutarın `<= 0` olması veya bağış tarihinin gelecekte olması gibi kritik hatalar engellenmemektedir.
- **Beklenti:** Eksik kuralları (`Amount > 0`, `DonationDate <= DateTime.UtcNow`, geçerli enum kontrolü vb.) ekleyin.

### Görev 8 - Sunucu Taraflı Sayfalama (Pagination) & Filtreleme (5 Puan)
- **İlgili Dosyalar:** `src/Assessment.Api/Controllers/V1/DonorsController.cs` (`GetAll`).
- **Durum:** V1 Donors listesi tüm tabloyu sınır olmaksızın çekmektedir.
- **Beklenti:** `PaginationFilter` modelini kullanarak sunucu taraflı `PageNumber`, `PageSize`, arama (`SearchTerm`) ve sıralama (`SortBy`) desteği ekleyin.

### Görev 9 - Merkezi Hata Yönetimi (RFC 7807 ProblemDetails) (5 Puan)
- **İlgili Dosyalar:** `src/Assessment.Api/Middleware/ExceptionHandlingMiddleware.cs`.
- **Durum:** Hata yakalama mekanizması standart olmayan, rastgele bir JSON formatı dönmektedir ve domain exception'larını (Validation, NotFound, BusinessException) doğru HTTP kodlarıyla eşlememektedir. Bazı controller'larda ise gereksiz `try/catch` blokları bulunmaktadır.
- **Beklenti:** RFC 7807 `ProblemDetails` standardına uygun merkezi hata yönetimi sağlayın.

### Görev 10 - Güvenli ve Yapısal Loglama (Structured Logging) (5 Puan)
- **İlgili Dosyalar:** `src/Assessment.Application/Services/DonationService.cs` (`ApproveDonationAsync`).
- **Durum:** Bağış onaylama loglaması string interpolation ile yapılmakta, log sorgulama sistemlerinde indekslenememekte ve bağışçının e-posta, telefon gibi kişisel verileri (PII) açık metin olarak loglanmaktadır. Ayrıca hata durumunda exception stack trace'i düzgün loglanmamaktadır.
- **Beklenti:** Hassas verileri maskeleyen, yapısal parametreler (`{DonationId}`, `{ApprovedByUserId}`) kullanan ve hata durumunda exception nesnesini doğru aktaran yapısal loglama uygulayın.

### Görev 11 - JWT Logout ve Token Kara Liste / İptal Mekanizması (5 Puan)
- **İlgili Dosyalar:** `src/Assessment.Application/Services/TokenRevocationService.cs`, `src/Assessment.Api/Middleware/TokenRevocationMiddleware.cs`, `src/Assessment.Api/Controllers/V1/AuthController.cs`.
- **Durum:** `POST /api/v1/auth/logout` endpoint'i 200 OK dönmekte ancak token iptal edilmemektedir. Logout olan bir token ile API istekleri yapılmaya devam edilebilmektedir.
- **Beklenti:** İptal edilen token'ları (veritabanındaki `RevokedToken` tablosu veya cache aracılığıyla) saklayıp gelen isteklerde kontrol eden bir mekanizma geliştirin.

### Görev 12 - AutoMapper Entegrasyonu ve Kod Tekrarının Giderilmesi (3 Puan)
- **İlgili Dosyalar:** `src/Assessment.Application/Services/VolunteerService.cs`, `src/Assessment.Application/Mapping/MappingProfile.cs`.
- **Durum:** `VolunteerService` içerisinde `AutoMapper` yerine 4 farklı metotta tekrarlayan manuel `new VolunteerDto { ... }` kodları kullanılmıştır.
- **Beklenti:** `MappingProfile` içerisine `Volunteer <-> VolunteerDto` konfigürasyonunu ekleyin ve servisteki manuel atamaları `IMapper` ile değiştirin.

### Görev 13 - Swagger / OpenAPI Dokümantasyonu (3 Puan)
- **İlgili Dosyalar:** `src/Assessment.Api/Controllers/V2/VolunteersController.cs`.
- **Durum:** V2 DonorsController tam dokümante edilmişken, `VolunteersController` hiçbir XML comment veya `[ProducesResponseType]` özniteliği içermemektedir.
- **Beklenti:** Endpoint'lere XML açıklamaları, parametre açıklamaları ve olası tüm HTTP yanıt kodlarını (`200`, `201`, `400`, `404`, `204`) ekleyin.

### Görev 14 - Docker Konteynerizasyon İyileştirmesi (2 Puan)
- **İlgili Dosyalar:** `Dockerfile`, `docker-compose.yml`.
- **Durum:** Projede bulunan Dockerfile ve docker-compose dosyalarında SQLite dosyasının kalıcılığı (volume mapping) ve çok projeli derleme çıktısı konusunda eksiklikler bulunmaktadır.
- **Beklenti:** Projenin `docker compose up --build` komutuyla hatasız ayağa kalkmasını ve veritabanının konteyner yeniden başladığında kaybolmamasını sağlayın.

### Görev 15 - Teknik Borç ve Mimari Kod İncelemesi (2 Puan)
- **İlgili Dosyalar:** `TECHNICAL_NOTES.md`.
- **Beklenti:** Yukarıdaki görevlerde açıkça belirtilmeyen, kod tabanında tespit ettiğiniz en az 3 adet mimari, tasarım, concurrency veya performans problemini `TECHNICAL_NOTES.md` dosyasına şablona uygun şekilde not edin.

---

## 7. Teslim Şekli ve Checklist

Lütfen tamamladığınız görevleri aşağıdaki listede işaretleyiniz (`[x]`):

```markdown
### Tamamlanan Görevler
- [ ] Görev 1: Legacy Endpoint'i V2'ye Taşıma ve Refactor (50 Puan)
- [ ] Görev 2: EF Core Raporlama Performans Problemini Çözme (20 Puan)
- [ ] Görev 3: Authorization / DB-Backed Permission Sistemi (15 Puan)
- [ ] Görev 4: Toplu Güncelleme (Bulk Update) Problemi (10 Puan)
- [ ] Görev 5: Transaction Yönetimi ve Veri Tutarlılığı (10 Puan)
- [ ] Görev 6: Eksik Unit Testlerin Yazılması (10 Puan)
- [ ] Görev 7: FluentValidation Kurallarının Tamamlanması (5 Puan)
- [ ] Görev 8: Sunucu Taraflı Sayfalama (Pagination) & Filtreleme (5 Puan)
- [ ] Görev 9: Merkezi Hata Yönetimi (RFC 7807 ProblemDetails) (5 Puan)
- [ ] Görev 10: Güvenli ve Yapısal Loglama (Structured Logging) (5 Puan)
- [ ] Görev 11: JWT Logout ve Token Kara Liste / İptal Mekanizması (5 Puan)
- [ ] Görev 12: AutoMapper Entegrasyonu ve Kod Tekrarının Giderilmesi (3 Puan)
- [ ] Görev 13: Swagger / OpenAPI Dokümantasyonu (3 Puan)
- [ ] Görev 14: Docker Konteynerizasyon İyileştirmesi (2 Puan)
- [ ] Görev 15: Teknik Borç ve Mimari Kod İncelemesi (2 Puan)

**Hedeflenen Toplam Puan:** _____ / 150 (Minimum: 50)
```

### Teslim Kuralları
1. Çözümünüzü bir Git repository'si (GitHub, GitLab vb.) üzerinden iletiniz.
2. Commit geçmişiniz değerlendirme kriterlerinden biridir. Tek bir büyük commit yerine mantıksal adımları temsil eden anlamlı commit mesajları tercih ediniz.
3. Proje teslim edildiğinde `dotnet build` ve `dotnet test` komutları hatasız çalışmalıdır.
4. Yeni bir migration eklediyseniz migration dosyalarını repoya dahil ediniz.
5. `README.md` dosyasındaki checklist'i tamamladığınız görevlere göre güncelleyiniz.

Başarılar dileriz!
