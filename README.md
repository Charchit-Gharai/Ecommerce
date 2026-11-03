# Full Stack E-Commerce Application

A complete e-commerce platform built across 3 phases, progressing from a monolithic .NET Web API to a fully containerized microservices architecture using Docker.

# Project Overview

It is a production-ready online shopping application that allows customers to browse products, manage a shopping cart, and place orders. 
Admins can manage product inventory through a dedicated API. 
The project was built in 3 phases — starting from a monolithic backend and React frontend.
It is then broken down into independent microservices and finally containerized using Docker.

# Technology Stack

# Backend (Monolith)
- .NET 10 Web API
- Entity Framework Core 10
- SQL Server LocalDB
- ASP.NET Core Identity
- JWT Authentication with Refresh Tokens
- Swagger / OpenAPI

# Frontend (Phase 2)
- React 18
- React Router v6
- Context API for state management
- Fetch API with custom client
- Responsive CSS design

# Microservices (Phase 3)
- .NET 10 Web API (per service)
- Ocelot API Gateway
- Entity Framework Core 10 (per service database)
- SQL Server (separate database per service)
- Docker and Docker Compose
