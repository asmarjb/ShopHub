# Full-Stack Enterprise E-Commerce Platform

Müasir, genişlənə bilən və yüksək təhlükəsizlik standartlarına malik tam funksional E-Commerce (Elektron Ticarət) platforması. Layihə Clean Architecture prinsipləri əsasında .NET 8 Web API, müasir React 18 (TypeScript + Vite + Tailwind CSS) və Docker infrastrukturunu birləşdirir.

---

## 🌟 Əsas Xüsusiyyətlər

- **Arxitektura:** Clean Architecture (Domain, Application, Infrastructure, API)
- **API Dokumentasiyası:** **Scalar API Reference** (`/scalar/v1`) ilə interaktiv test imkanı və JWT Bearer avtorizasiya axını
- **İstifadəçi və Təhlükəsizlik:** ASP.NET Core Identity, JWT Access & Refresh Token axını, granular icazə əsaslı avtorizasiya (`[HasPermission]`), Rate Limiting
- **Kataloq & Stok İdarəetməsi:** Dinamik atributlu məhsul variantları, çoxsaylı şəkillər, etiketlər (Tags), brendlər və kateqoriyalar
- **Səbət və Sifarişlər:** Qonaq (Anonymous) və qeydiyyatdan keçmiş istifadəçi səbətinin birləşdirilməsi, kupon endirimləri, çatdırılma üsulları, tranzaksiyalı stok azaldılması və sifarişin ləğvi zamanı avtomatik stok bərpası
- **Admin İdarəetmə Paneli:** Gəlir və sifariş statistikası qrafikləri, az qalan stok xəbərdarlıqları, Soft Delete və Zibil Qutusu (Trash) bərpası/həmişəlik silinməsi, Rol və İcazə idarəsi, Audit jurnalı (Audit Logs)
- **Çoxdilli Dəstək:** Azərbaycan və İngilis dili dəstəyi (i18n), qaranlıq/işıqlı rejim (Dark/Light mode)
- **Konteynerləşdirmə:** SQL Server, API və Nginx əsaslı React müştərisi üçün tam Docker Compose sazlaması

---

## 🛠 Texnoloji Stek

| Sahə | Texnologiyalar |
| :--- | :--- |
| **Backend** | .NET 8 Web API, Entity Framework Core 8, FluentValidation, Scalar.AspNetCore |
| **Frontend** | React 18, TypeScript, Vite, Tailwind CSS, React Router v6, Axios, react-i18next, Lucide Icons |
| **Verilənlər Bazası** | Microsoft SQL Server (LocalDB / Docker SQL Server 2022) |
| **Konteynerləşdirmə** | Docker, Docker Compose, Nginx |
| **Testləşdirmə** | xUnit, Moq, EF Core InMemory |

---

## 🚀 Quraşdırma və İşə Salma

### Metod 1: Docker Compose ilə (Tövsiyə olunur)

Layihənin kök qovluğunda aşağıdakı əmri icra edin:

```bash
docker compose up --build -d
```

Xidmətlər:
- **React Frontend:** `http://localhost:3000`
- **Backend API & Scalar:** `http://localhost:5169/scalar/v1`
- **SQL Server:** `localhost:1433`

---

### Metod 2: Yerli Mühitdə (Manual) İşə Salma

#### 1. Backend (.NET 8 API)
```bash
# Kök qovluqda
cd src/ECommerce.API

# Paketləri bərpa edin və layihəni işə salın
dotnet run
```
API avtomatik olaraq `http://localhost:5169` ünvanında işə düşəcək və ilkin məlumatları (Seed Data) bazaya daxil edəcəkdir.
- **Scalar API Sənədləri:** [http://localhost:5169/scalar/v1](http://localhost:5169/scalar/v1)

#### 2. Frontend (React + Vite)
```bash
# Kök qovluqdan client qovluğuna keçin
cd client

# Asılılıqları quraşdırın
npm install

# Layihəni dev rejimində başladın
npm run dev
```
İctimai sayt və Admin panel `http://localhost:5173` ünvanında açılacaq.

---

## 📖 Scalar API Dokumentasiyası & Test Axını

Layihə müasir və sürətli **Scalar API Reference** sənədləşməsindən istifadə edir:
1. Brauzerdə `http://localhost:5169/scalar/v1` ünvanına daxil olun.
2. `POST /api/auth/login` endpoint-ini tapın və administrator və ya müştəri məlumatları ilə giriş edin.
3. Cavabdan qayıdan `accessToken` dəyərini kopyalayın.
4. Yuxarı sağ küncdəki **Authorize / Security** bölməsinə daxil olaraq `Bearer {token}` formatında daxil edin.
5. Qorunan bütün endpointləri (Admin, Orders, Roles, Trash və s.) interaktiv şəkildə test edin.

---

## 🔑 İlkin Giriş Məlumatları (Seed Credentials)

Layihə ilk dəfə işə düşərkən sistemdə avtomatik olaraq aşağıdakı test hesabları yaradılır:

| Rol | E-poçt | Şifrə | İcazələr (Permissions) |
| :--- | :--- | :--- | :--- |
| **SuperAdmin** | `admin@test.com` | `Admin123!` | Tam hüquqlu (Bütün 16 icazə: CRUD, Trash, Roles, Audit və s.) |
| **Customer** | `customer1@test.com` | `Customer123!` | İctimai sayt alış-verişi, səbət, sifariş və izləmə |

---

## 🧪 Avtomatlaşdırılmış xUnit Testləri

Sistemin kritik biznes məntiqini yoxlayan vahid (unit) və inteqrasiya testləri:
- **OrderStockTests:** Sifariş zamanı variant stokunun azalması və `StockMovementType.Out` hərəkətinin qeydiyyatı.
- **CouponValidationTests:** Faiz və sabit endirim hesablanması, müddəti bitmiş və ya istifadə limiti dolmuş kuponların rədd edilməsi.
- **SoftDeleteRestoreTests:** Məhsul və kateqoriyaların təhlükəsiz soft delete edilməsi, məhsulu olan kateqoriyanın məhdudiyyətləri və bərpa (restore) axını.
- **PermissionAuthorizationTests:** Dynamic `[HasPermission]` atributlarının və rol hüquqlarının düzgün yoxlanılması.

Testləri icra etmək üçün:
```bash
dotnet test tests/ECommerce.Tests/ECommerce.Tests.csproj
```

---

## 📌 Əsas API Endpoint-ləri Xülasəsi

### Autentifikasiya (`/api/auth`)
- `POST /api/auth/register` - Yeni müştəri qeydiyyatı
- `POST /api/auth/login` - Giriş (Access və Refresh token təqdim edir)
- `POST /api/auth/refresh-token` - Token yeniləmə
- `POST /api/auth/forgot-password` & `/reset-password` - Şifrənin sıfırlanması

### Kataloq & Axtarış (`/api/products`, `/api/categories`, `/api/brands`)
- `GET /api/products` - Filterləmə, sıralama, pagination və kateqoriya axtarışı
- `GET /api/products/{id}` & `/slug/{slug}` - Məhsul və onun aktiv variant detalları
- `GET /api/products/autocomplete` - Sürətli axtarış təklifləri
- `GET /api/categories` & `GET /api/brands` - Aktiv kateqoriyalar və brendlər

### Səbət & Kuponlar (`/api/cart`, `/api/coupons`)
- `GET /api/cart` - Cari səbət məlumatları
- `POST /api/cart/items` - Səbətə məhsul variantı əlavə etmək
- `PUT /api/cart/items/{id}` - Miqdarı dəyişmək
- `DELETE /api/cart/items/{id}` - Məhsulu səbətdən çıxarmaq
- `POST /api/coupons/validate` - Kupon kodunu yoxlamaq və endirimi hesablamaq

### Sifarişlər (`/api/orders`, `/api/shipping-methods`)
- `POST /api/orders/checkout` - Səbətdən sifariş rəsmiləşdirmək
- `GET /api/orders/my-orders` - Müştərinin sifariş tarixçəsi
- `GET /api/orders/track/{orderNumber}` - Anonim ictimai sifariş izləməsi
- `POST /api/orders/{id}/cancel` - Müştəri tərəfindən sifarişin ləğvi və stok bərpası

### Admin İdarəetmə Paneli (`/api/admin/*`, `/api/roles`)
- `GET /api/admin/statistics/dashboard` - Əsas gəlir, sifariş sayı və az qalan stok xülasəsi
- `GET /api/admin/audit-logs` - Sistemdə baş verən bütün mühüm hadisələrin jurnalı
- `GET|POST|PUT|DELETE /api/admin/products` - Məhsulların CRUD əməliyyatları
- `GET|POST|PUT|DELETE /api/admin/categories` - Kateqoriyaların CRUD əməliyyatları
- `GET /api/admin/trash/categories` & `/products` - Silinmiş elementlər siyahısı
- `POST /api/admin/trash/{entity}/{id}/restore` - Elementin bərpası
- `DELETE /api/admin/trash/{entity}/{id}/hard-delete` - Bazadan həmişəlik silinmə
- `GET|POST|PUT|DELETE /api/roles` - Rol və icazələrin təyini (`Permissions.cs`)
- `GET /api/roles/permissions` - Mövcud 16 sistem hüquqlarının siyahısı
