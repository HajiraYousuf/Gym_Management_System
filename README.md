# 🏋️‍♂️ IronCore - Comprehensive Gym Management System

![Status](https://img.shields.io/badge/Status-Completed-success?style=for-the-badge&logo=git)
![Platform](https://img.shields.io/badge/Platform-Web-blue?style=for-the-badge&logo=windows)
![Tech Stack](https://img.shields.io/badge/Tech-Stack-informational?style=for-the-badge&logo=dotnet)

**IronCore** is a full-featured, enterprise-grade Gym Management System designed to streamline fitness center operations. It features fully integrated **Frontend** and **Backend** architectures, offering dedicated portals and dashboards tailored for different user roles—ensuring seamless management of members, staff, schedules, and billing.

---

## 🚀 Key Features & Portals

The system comes with a robust multi-role access control (RBAC) mechanism powering separate interactive dashboards:

*   **👑 Admin Portal:** Complete control over system analytics, user management, staff assignments, financial tracking, and global settings.
*   **🏋️‍♂️ Trainer Portal:** Manage workout plans, track client progress, view assigned members, and schedule training sessions.
*   **🛎️ Receptionist Portal:** Quick member check-ins, walk-in guest registrations, membership renewals, and front-desk billing.
*   **💪 Member Portal:** Personal dashboard to track active memberships, view workout/diet routines, check attendance history, and monitor fitness goals.
*   **👤 Guest Portal:** Landing and inquiry interface for prospective members to explore facilities and register.
*   **🔐 Authentication:** Secure Login and Registration system with role-based routing and session management.

---

## 🛠️ Tech Stack

*   **Frontend:** HTML5, Tailwind CSS, JavaScript, Chart.js (for analytics and statistics widgets)
*   **Backend:** ASP.NET Core MVC / ASP.NET Web Forms
*   **Database:** Microsoft SQL Server (MSSQL) with optimized schemas and stored procedures
*   **UI/UX Theme:** Dark-themed modern interface optimized with zinc/black backgrounds and vibrant lime-green accent styling (`#10b981` / lime highlights).

---

## 📂 Project Architecture

```text
GYM-MNG-System/
│
├── 📁 Controllers/       # Backend controllers managing business logic & routing
├── 📁 Models/            # Database entities and data transfer objects (DTOs)
├── 📁 Views/             # Razor views / ASPX pages structured by portals
│   ├── 📁 Admin/         # Admin dashboard and management tools
│   ├── 📁 Trainer/       # Trainer portal and client tracking views
│   ├── 📁 Receptionist/  # Front-desk and check-in interfaces
│   └── 📁 Member/        # Member profile and tracking views
├── 📁 wwwroot/           # Static assets (Tailwind CSS, custom JS, images)
└── 📁 Database/          # SQL scripts, migrations, and schema diagrams