# 📅 Smart Queue - Appointment Service

The **Appointment Service** is a key backend microservice in the **Smart Queue Management System**, responsible for managing patient appointment data. It supports CRUD operations, uses PostgreSQL (hosted on Azure), and integrates with the User Service for authorization. It also triggers real-time queue updates via Redis.

---

## 🚀 Features

* 🧾 Create, Read, Update, Delete Appointments
* 🔐 JWT Authentication via User Service
* 🧠 Redis Pub/Sub integration to broadcast updates
* 🗃️ PostgreSQL database with Entity Framework Core
* 🐳 Dockerized .NET 7 microservice

---

## 🛠️ Tech Stack

* **Language**: C# (.NET 7)
* **Framework**: ASP.NET Core Web API
* **ORM**: Entity Framework Core
* **Database**: PostgreSQL (via Azure Database for PostgreSQL)
* **Auth**: JWT Token Validation
* **Eventing**: Redis Pub/Sub (Azure Cache for Redis)
* **Containerization**: Docker

---

## 📁 Project Structure

```
smart-queue-appointment-service/
├── Appointment.API/             # Web API project
│   ├── Controllers/             # Appointment CRUD endpoints
│   ├── Program.cs               # Entry point and DI setup
│   ├── appsettings.json         # Config including JWT, Redis, DB
│   ├── Dockerfile               # Docker image setup
├── Appointment.Domain/          # Appointment entity
├── Appointment.Application/     # (Optional future services)
├── Appointment.Infrastructure/  # EF Core DB Context, RedisPublisher
│   ├── Data/
│   ├── Repositories/
│   └── Redis/
├── AppointmentService.sln       # Solution file
```

---

## 🔐 JWT Authentication

This service **validates JWT tokens** issued by the User Service. The JWT key and settings are configured in `appsettings.json`.

```json
"Jwt": {
  "Key": "supersecretkey1234567890987654321!",
  "Issuer": "UserService",
  "Audience": "AppointmentService"
}
```

> All routes are protected by `[Authorize]` attribute.

---

## 🔁 Redis Integration (Pub/Sub)

When an appointment is added/updated/deleted, a **message is published** to the Redis `queue_updates` channel to notify the Queue and Notification Services.

Configured in `appsettings.json`:

```json
"Redis": {
  "Host": "smartqueueredis.redis.cache.windows.net",
  "Port": 6380,
  "Password": "<your_password>",
  "UseSsl": true
}
```

---

## 🐳 Docker Instructions

### 🔧 Build the image

```bash
docker build -t smart-queue-appointment-service .
```

### ▶️ Run the container

```bash
docker run -p 5243:80 smart-queue-appointment-service
```

> Note: ASP.NET Core defaults to port 80 inside container.

---

## 🧪 API Endpoints

| Method | Endpoint                    | Description                  |
| ------ | --------------------------- | ---------------------------- |
| GET    | `/api/v1/appointments`      | Get all appointments (JWT)   |
| GET    | `/api/v1/appointments/{id}` | Get appointment by ID        |
| POST   | `/api/v1/appointments`      | Create new appointment (JWT) |
| PUT    | `/api/v1/appointments/{id}` | Update existing appointment  |
| DELETE | `/api/v1/appointments/{id}` | Delete appointment           |

> All endpoints require a valid JWT token from User Service.

---

## 💾 Database Migration (Optional Local)

```bash
dotnet ef migrations add InitialCreate \
  --project Appointment.Infrastructure \
  --startup-project Appointment.API \
  --context AppDbContext

dotnet ef database update \
  --project Appointment.Infrastructure \
  --startup-project Appointment.API \
  --context AppDbContext
```

---

## 📄 License

This project is licensed under the MIT License.
