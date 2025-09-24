# E-Commerce Inventory API

RESTful API for managing products and categories in an e-commerce inventory system.  
Built with **.NET Core**, following **Domain-Driven Design (DDD)** layered architecture, using **Entity Framework Core**, JWT-based authentication, and optional product image handling.

---

## 📦 Project Directory Structure

| Directory/File Path       | Description                           |
|---------------------------|---------------------------------------|
| **API/**                  | Controllers, Program.cs, Middleware   |
| **Application/**          | DTOs, Services, Mappings, Interfaces  |
| **Core/**                 | Entities, Core Interfaces             |
| **Infrastructure/**       | Repositories, DbContext, UnitOfWork   |
| **wwwroot/uploads/**      | Uploaded product images storage       |





## 🛠️ Tech Stack

- **Backend:** .NET Core 7 / C#
- **Architecture:** Layered DDD, Repository & Unit of Work patterns
- **Database:** SQL Server (LocalDB)
- **ORM:** Entity Framework Core
- **Authentication:** JWT (JSON Web Tokens)
- **File Uploads (Optional):** Product images stored in `wwwroot/uploads`
- **API Documentation:** Swagger (Swashbuckle.AspNetCore)

---


## 🔑 Features

### User Authentication
- **Register:** `POST /api/auth/register`
  - Create user with `username`, `email`, `password`, and optional `role`.
- **Login:** `POST /api/auth/login`
  - Returns a JWT token.
- **Authorization:**
  - All endpoints protected with JWT.
  - Admin-only operations require `Admin` role.


### Product Management
- **Create Product:** `POST /api/products` (Admin only)
  - Fields: `Name`, `Description`, `Price`, `Stock`, `CategoryId`, optional `Image`.
- **Get All Products:** `GET /api/products`
  - Supports filtering: `?categoryId=1&minPrice=10&maxPrice=100`
  - Supports pagination: `?page=1&limit=10`
  - Supports sorting: `?sortBy=Price&desc=true`
- **Search Products:** `GET /api/products/search?search=keyword`
- **Get Product by ID:** `GET /api/products/{id}`
- **Update Product:** `PUT /api/products/{id}` (Admin only)
- **Delete Product:** `DELETE /api/products/{id}` (Admin only)


### Category Management
- **Create Category:** `POST /api/categories` (Admin only)
- **Get All Categories:** `GET /api/categories`
- **Get Category by ID:** `GET /api/categories/{id}`
- **Update Category:** `PUT /api/categories/{id}` (Admin only)
- **Delete Category:** `DELETE /api/categories/{id}` (Admin only)
  - Returns `409 Conflict` if category has linked products.

---


## 🧩 Notes

All passwords are stored as hashed + salted (HMACSHA512)

Product images are stored in wwwroot/uploads and served via static files.

Caching implemented for product queries (MemoryCache, 5 min TTL).


---


## 📖 API Documentation

Swagger UI provides all endpoints with request/response examples.

Include JWT token in Authorization header:
Bearer <token>


---

#####
#####  User Authentication
#####

Email: anshuvo@mail.com

Password: ANShuvo@123!

Role: Admin
