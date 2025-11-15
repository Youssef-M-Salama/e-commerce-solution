# E-Commerce Solution – ASP.NET Core Web API

A modular, production‑ready **ASP.NET Core Web API** implementing a complete e‑commerce backend using a layered architecture with secure authentication, SQL Server persistence, Identity-based user management, automated mapping, email verification, and strict separation of concerns.

---

##  Overview

This solution delivers a clean and scalable architecture designed for real-world e‑commerce systems.  
It applies industry-standard patterns to ensure:

- Clear separation of responsibilities  
- High testability  
- Strong authentication & authorization  
- Consistent API responses  
- Reliable email verification via SMTP  
- Maintainable integration with SQL Server and external services  

---

##  Architecture

The system is organized into multiple layers to enforce clean boundaries and maintain long-term scalability.

---

### **Presentation Layer**

Contains the Web API endpoints:

- **Admin API** – Administrative and management features  
- **Customer API** – Public customer-facing endpoints  

This layer exposes only controllers and delegates all logic to the application layer.

---

### **Application Layer**

The primary orchestrator of all business operations:

- Application services implementing business workflows  
- DTO-based request/response models  
- Validation via *FluentValidation*  
- AutoMapper profiles for object transformation  
- Centralized response formatting for unified API output  
- Authentication workflow orchestration  
- Email confirmation workflow  
- Interfaces for file storage, email sending, and token handling  

No database or infrastructure code exists in this layer.

---

### **DependencyInjection Layer**

Centralized management for registering:

- Application services  
- Domain abstractions  
- Infrastructure implementations  
- Mapping profiles  
- FluentValidation validators  
- Authentication & authorization configuration  
- Email (SMTP) services  
- File-handling services  
- SQL Server + Identity configuration  

This creates a single, maintainable hub for wiring the entire solution.

---

### **Domain Layer**

Contains the pure business core:

- Business entities  
- Value objects  
- Repository contracts  
- File storage contracts  
- Email and token service contracts  

The domain layer contains **no external dependencies**.

---

### **Infrastructure Layer**

Provides concrete implementations of all external dependencies:

- **SQL Server database integration**
  - Entity Framework Core  
  - Migrations  
  - Repository implementations  
  - Querying and persistence logic  

- **ASP.NET Core Identity**
  - User management  
  - Password hashing  
  - Claims & roles  
  - Secure credential storage  
  - User and role entities backed by SQL Server  

- **JWT Token Generation**
  - Access token creation  
  - Refresh token management  
  - Claims-based authorization  

- **SMTP Email Service**
  - Email confirmation  
  - Token verification emails  
  - SMTP configuration (host, port, credentials)  

- **File Storage**
  - Local or external file system implementations  
  - Secure storage and retrieval mechanics  

This layer never communicates directly with the presentation layer.

---

##  Authentication & Security

### **Identity-Based Authentication**

The project uses **ASP.NET Core Identity** as the foundation for user management:

- User registration & login  
- Role management  
- Password hashing  
- Email confirmation requirements  
- Lockout, recovery, and other Identity features  

Identity is fully integrated with SQL Server.

---

### **JWT Token Authentication**

Identity is combined with JWT for stateless API authentication:

- Access tokens  
- Refresh tokens  
- Claims-based authorization  
- Custom token lifetimes  
- Secure signing & validation  

---

### **Email Verification (SMTP)**

Upon registration, users receive a secure verification email:

- Unique confirmation token per user  
- Configurable expiration  
- SMTP-based delivery  
- Mandatory account verification before access  

---

##  Object Mapping

All transformations between DTOs and domain entities use **AutoMapper**:

- Minimizes repetitive code  
- Ensures consistent object conversions  
- Simplifies controller and service logic  

---

##  File Management

File operations are abstracted through interfaces, enabling:

- Local storage  
- Cloud storage (future-ready)  
- Easy replacement without modifying upper layers  

---

##  Database – SQL Server

The system uses **Microsoft SQL Server** for persistence:

- Fully normalized schema  
- EF Core migrations  
- Relationship mapping for products, users, orders, and roles  
- Identity tables integrated directly into the DB  

---

##  Getting Started

### **Prerequisites**

- .NET SDK  
- SQL Server  
- SMTP credentials  
- Optional: directory for file storage  

---

### **Clone the Repository**

```bash
git clone https://github.com/Youssef-M-Salama/e-commerce-solution.git
cd e-commerce-solution
```

---

### **Configuration**

Update `appsettings.json`:

- SQL Server connection string  
- JWT settings (issuer, key, expiration)  
- SMTP configuration  
- File storage settings  

---

### **Run the Application**

1. Restore packages  
2. Apply migrations  
3. Start:
   - **Admin API**
   - **Customer API**

Use Swagger to explore and test endpoints.

---

##  API Response Shape

Every endpoint returns a unified response envelope:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { }
}
```

Errors follow the same structure for consistency.

---

##  Contributing

1. Fork the repo  
2. Create a feature branch  
3. Follow clean architecture conventions  
4. Submit a pull request  

---

##  Author

**Youssef M. Salama**  
GitHub: https://github.com/Youssef-M-Salama

