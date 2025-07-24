# AgriChoice - Livestock Marketplace & Management Platform

A comprehensive web-based livestock marketplace and management system built with ASP.NET Core, designed to connect farmers, customers, and delivery drivers in a seamless agricultural ecosystem.

## 🐄 Problem Statement

Traditional livestock trading faces several challenges:
- **Lack of Digital Presence**: Farmers struggle to reach wider markets for their livestock
- **Complex Transaction Management**: Difficulty in managing purchases, deliveries, and refunds
- **Inefficient Delivery Systems**: No centralized system for coordinating livestock deliveries
- **Limited Payment Security**: Lack of secure, standardized payment processing
- **Poor Transaction Tracking**: Inadequate systems for tracking sales, refunds, and driver compensation

## 💡 Solution

AgriChoice provides a modern, role-based platform that digitizes the entire livestock trading process:

### **For Farmers/Admins:**
- Digital livestock inventory management
- Real-time sales tracking and analytics
- Automated driver assignment system
- Comprehensive financial reporting
- Refund request management

### **For Customers:**
- Browse available livestock with detailed information
- Secure cart and checkout system
- Order tracking and history
- Refund request system with file uploads
- Integrated wallet system

### **For Drivers:**
- Job assignment notifications
- Pickup and delivery tracking
- PIN-based verification system
- Earnings and wallet management
- Job history tracking

## 🚀 Key Features

### **Core Functionality**
- **Multi-Role Authentication**: Admin, Customer, and Driver roles with distinct permissions
- **Livestock Management**: Complete CRUD operations for cattle inventory
- **Shopping Cart System**: Add/remove items with automatic pricing calculations
- **Secure Payment Processing**: Braintree integration for safe transactions
- **Delivery Management**: Automated driver assignment and tracking
- **Refund System**: Complete refund workflow with approval process

### **Advanced Features**
- **Real-time Notifications**: Email notifications for order updates
- **Financial Tracking**: Comprehensive transaction management
- **Geographic Integration**: Distance-based shipping cost calculation
- **File Upload Support**: Document management for refund requests
- **PIN Verification**: Secure pickup/delivery confirmation system
- **Responsive Design**: Mobile-friendly interface using Tailwind CSS

## 🛠 Technology Stack

### **Backend**
- **Framework**: ASP.NET Core 8.0 with MVC architecture
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity with role-based authorization
- **Payment Processing**: Braintree Gateway integration
- **Email Service**: MailKit for automated notifications

### **Frontend**
- **UI Framework**: Razor Pages with Tailwind CSS
- **JavaScript**: Vanilla JS for interactive components
- **Responsive Design**: Mobile-first approach
- **Icons**: Heroicons and SVG graphics

### **Architecture**
- **Design Pattern**: Model-View-Controller (MVC)
- **Database Pattern**: Repository pattern with Entity Framework
- **Security**: Anti-forgery tokens, role-based access control
- **Data Seeding**: Automatic role and sample data initialization

## 📊 Database Schema

The system includes comprehensive models for:
- **User Management**: Identity framework with custom roles
- **Livestock**: Cow entities with breed, age, weight, pricing
- **Commerce**: Cart, Purchase, and Transaction models
- **Logistics**: Delivery and RefundRequest entities
- **Reviews**: Customer feedback system

## 🔐 Security Features

- Role-based authorization (Admin, Customer, Driver)
- Anti-forgery token protection
- Secure payment processing with Braintree
- PIN-based verification for deliveries
- SQL injection prevention through Entity Framework
- User data protection and privacy

## 📱 User Interface

- **Modern Design**: Clean, professional interface
- **Responsive Layout**: Works seamlessly on all devices
- **Intuitive Navigation**: Role-specific dashboards and menus
- **Real-time Feedback**: Toast notifications and form validations
- **Rich Media Support**: Image handling for livestock photos

## 🚀 Installation & Setup

1. **Prerequisites**
   ```bash
   - .NET 8.0 SDK
   - SQL Server (LocalDB or full instance)
   - Visual Studio 2022 or VS Code
   ```

2. **Database Setup**
   ```bash
   dotnet ef database update
   ```

3. **Run Application**
   ```bash
   dotnet run
   ```

4. **Default Admin Credentials**
   ```
   Email: admin@gmail.com
   Password: Admin123!
   ```

## 📈 Project Achievements

- **Complete E-commerce Solution**: Full buying cycle from browsing to delivery
- **Multi-stakeholder Platform**: Serves farmers, customers, and drivers
- **Payment Integration**: Secure transaction processing
- **Automated Workflows**: Driver assignment and notification systems
- **Scalable Architecture**: Built for future expansion and maintenance

## 🎯 Future Enhancements

- Mobile application development
- GPS tracking for deliveries
- Advanced analytics and reporting
- Multi-language support
- API development for third-party integrations

## 👥 User Roles

1. **Admin**: Livestock management, order oversight, driver assignment
2. **Customer**: Browse, purchase, track orders, request refunds
3. **Driver**: Accept jobs, manage deliveries, track earnings

## 📞 Contact Information

This project was developed as part of academic coursework at Durban University of Technology, demonstrating practical application of software engineering principles in agricultural technology.

---

*AgriChoice - Bridging Technology and Agriculture* 🌾
