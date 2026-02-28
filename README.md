# MyCourse

A full-featured online course platform built with **ASP.NET Core** and **Entity Framework Core**. It supports course creation and management, lesson organisation, user enrolment, payments, two-factor authentication, and role-based administration.

---

## Features

- **Course catalogue**  browse, search, and sort courses by title, rating, or price
- **Lesson management**  create, edit, reorder, and delete lessons within a course
- **User authentication**  registration, login, email confirmation, password reset, and 2FA via authenticator app or recovery codes
- **Enrolment & payments**  Stripe integration for paid course subscriptions
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
| Framework | ASP.NET Core MVC + Razor Pages (.NET 6+) |
| ORM | Entity Framework Core (SQLite) |
| Raw SQL layer | ADO.NET (`SqliteAccessor`) |
| Identity | ASP.NET Core Identity with custom `ApplicationUser` |
| Authentication | Cookie + external providers (configurable) |
| Payments | Stripe .NET SDK |
| Image processing | Magick.NET |
| Email | MailKit / SMTP |
| Caching | `IMemoryCache` with decorator pattern |
| CAPTCHA | AspNetCore.ReCaptcha |
| Rich text editor | Summernote |
| QR codes (2FA) | qrcode.js |
| Frontend | Bootstrap 5, jQuery, Cropper.js |

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

## Credits

This project was originally developed as part of a structured course on ASP.NET Core.
The course content and architectural guidance were provided by Moreno Gentili.

---

## License

This repository is for personal learning and portfolio purposes. No license is granted for commercial use.
