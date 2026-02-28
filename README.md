# MyCourse

A full-featured online course platform built with **ASP.NET Core** and **Entity Framework Core**. It supports course creation and management, lesson organisation, user enrolment, payments, two-factor authentication, and role-based administration.

---

## Features

- **Course catalogue**  browse, search, and sort courses by title, rating, or price
- **Lesson management**  create, edit, reorder, and delete lessons within a course
- **User authentication**  registration, login, email confirmation, password reset, and 2FA via authenticator app or recovery codes
- **Enrolment & payments**  Stripe and PayPal integration for paid course subscriptions
- **Course ratings**  authenticated users can rate courses they are enrolled in
- **Author tools**  course authors can manage their own content; a contact form lets students send questions directly to the author
- **Role-based authorisation**  `Teacher` and `Administrator` roles with custom policy handlers
- **User management**  administrators can assign or revoke roles from any account
- **Image upload**  course cover images are processed and cropped server-side with ImageMagick
- **Optimistic concurrency**  row-version tokens prevent lost updates on courses and lessons
- **reCAPTCHA**  spam protection on the contact form

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC + Razor Pages (.NET 6) |
| ORM | Entity Framework Core (SQLite) |
| Raw SQL layer | ADO.NET (`SqliteDatabaseAccessor`) |
| Identity | ASP.NET Core Identity with custom `ApplicationUser` |
| Authentication | Cookie + external providers (configurable) |
| Payments | Stripe .NET SDK, PayPal Checkout SDK |
| Image processing | Magick.NET |
| Email | MailKit / SMTP |
| Caching | `IMemoryCache` with decorator pattern |
| CAPTCHA | AspNetCore.ReCaptcha |
| HTML sanitisation | HtmlSanitizer |
| Rich text editor | Summernote |
| QR codes (2FA) | qrcode.js |
| Frontend | Bootstrap 4.4.1, jQuery, Font Awesome |

---

## Project Structure

```
MyCourse.sln
docs/ddl/              # Raw SQL DDL scripts
scripts/               # Standalone C# scripts (dotnet-script) for experimentation
src/MyCourse/
  Controllers/         # MVC controllers (Courses, Lessons, Home, Error)
  Models/
    Entities/          # EF Core entity classes
    Services/          # Application and infrastructure services
    ViewModels/        # View-facing read models
    InputModels/       # Form input / validation models
  Views/               # Razor views
  Pages/               # Razor Pages (Admin, Contact)
  Areas/Identity/      # ASP.NET Core Identity scaffolded pages
  Customizations/      # Authorization handlers, model binders, tag helpers
  Migrations/          # EF Core migrations
  wwwroot/             # Static assets
```

---

## Getting Started

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [ImageMagick](https://imagemagick.org/script/download.php) (required for image processing)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/fabioghidella/MyCourse.git
   cd MyCourse
   ```

2. **Restore frontend libraries**
   ```bash
   dotnet tool install -g Microsoft.Web.LibraryManager.Cli
   cd src/MyCourse
   libman restore
   ```

3. **Configure user secrets**

   The following secrets must be set via `dotnet user-secrets` (do not put them in `appsettings.json`):
   ```bash
   cd src/MyCourse
   dotnet user-secrets set "Smtp:Username" "<your-smtp-username>"
   dotnet user-secrets set "Smtp:Password" "<your-smtp-password>"
   dotnet user-secrets set "ReCaptcha:SiteKey" "<your-recaptcha-site-key>"
   dotnet user-secrets set "ReCaptcha:SecretKey" "<your-recaptcha-secret-key>"
   dotnet user-secrets set "Stripe:PrivateKey" "<your-stripe-private-key>"
   dotnet user-secrets set "Paypal:ClientId" "<your-paypal-client-id>"
   dotnet user-secrets set "Paypal:ClientSecret" "<your-paypal-client-secret>"
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```
   The app will be available at `https://localhost:5001`.

### Admin account

The first user to register with the email address configured in `Users:AssignAdministratorRoleOnRegistration` (`appsettings.json`) will automatically be granted the `Administrator` role.

---

## Credits

This project was originally developed as part of a structured course on ASP.NET Core.
The course content and architectural guidance were provided by Moreno Gentili.

---

## License

This repository is for personal learning and portfolio purposes. No license is granted for commercial use.
