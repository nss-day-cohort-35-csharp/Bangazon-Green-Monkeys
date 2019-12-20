USE MASTER
GO

IF NOT EXISTS (
    SELECT [name]
    FROM sys.databases
    WHERE [name] = N'BangazonAPI35'
)
CREATE DATABASE BangazonAPI35
GO

USE BangazonAPI35
GO


CREATE TABLE Department (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(55) NOT NULL,
	Budget 	INTEGER NOT NULL
);

CREATE TABLE Computer (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	PurchaseDate DATETIME NOT NULL,
	DecomissionDate DATETIME,
	Make VARCHAR(55) NOT NULL,
	Model VARCHAR(55) NOT NULL
);

CREATE TABLE Employee (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	FirstName VARCHAR(55) NOT NULL,
	LastName VARCHAR(55) NOT NULL,
	DepartmentId INTEGER NOT NULL,
	Email VARCHAR(55) NOT NULL,
	IsSupervisor BIT NOT NULL DEFAULT(0),
	ComputerId INTEGER NOT NULL,
    CONSTRAINT FK_EmployeeDepartment FOREIGN KEY(DepartmentId) REFERENCES Department(Id),
	CONSTRAINT FK_EmployeeComputer FOREIGN KEY (ComputerId) REFERENCES Computer(Id)
);

CREATE TABLE TrainingProgram (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(255) NOT NULL,
	StartDate DATETIME NOT NULL,
	EndDate DATETIME NOT NULL,
	MaxAttendees INTEGER NOT NULL
);

CREATE TABLE EmployeeTraining (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	EmployeeId INTEGER NOT NULL,
	TrainingProgramId INTEGER NOT NULL,
    CONSTRAINT FK_EmployeeTraining_Employee FOREIGN KEY(EmployeeId) REFERENCES Employee(Id),
    CONSTRAINT FK_EmployeeTraining_Training FOREIGN KEY(TrainingProgramId) REFERENCES TrainingProgram(Id)
);

CREATE TABLE ProductType (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(55) NOT NULL
);

CREATE TABLE Customer (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	FirstName VARCHAR(55) NOT NULL,
	LastName VARCHAR(55) NOT NULL,
	CreatedDate DATETIME NOT NULL,
	Active BIT NOT NULL DEFAULT(1),
	[Address] VARCHAR(255) NOT NULL,
	City VARCHAR(55) NOT NULL,
	[State] VARCHAR(55) NOT NULL,
	Email VARCHAR(55) NOT NULL,
	Phone VARCHAR(55) NOT NULL
);

CREATE TABLE Product (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	DateAdded DATETIME NOT NULL,
	ProductTypeId INTEGER NOT NULL,
	CustomerId INTEGER NOT NULL,
	Price MONEY NOT NULL,
	Title VARCHAR(255) NOT NULL,
	[Description] VARCHAR(255) NOT NULL,
    CONSTRAINT FK_Product_ProductType FOREIGN KEY(ProductTypeId) REFERENCES ProductType(Id),
    CONSTRAINT FK_Product_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id)
);

CREATE TABLE PaymentType (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(55) NOT NULL,
	Active BIT NOT NULL DEFAULT(1)
);

CREATE TABLE UserPaymentType (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	AcctNumber VARCHAR(55) NOT NULL,
	Active BIT NOT NULL DEFAULT(1),
	CustomerId INTEGER NOT NULL,
	PaymentTypeId INTEGER NOT NULL,
    CONSTRAINT FK_UserPaymentType_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id),
	CONSTRAINT FK_UserPaymentType_PaymentType FOREIGN KEY(PaymentTypeId) REFERENCES PaymentType(Id)
);

CREATE TABLE [Order] (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	CustomerId INTEGER NOT NULL,
	UserPaymentTypeId INTEGER,
    CONSTRAINT FK_Order_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id),
    CONSTRAINT FK_Order_Payment FOREIGN KEY(UserPaymentTypeId) REFERENCES UserPaymentType(Id)
);

CREATE TABLE OrderProduct (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	OrderId INTEGER NOT NULL,
	ProductId INTEGER NOT NULL,
    CONSTRAINT FK_OrderProduct_Product FOREIGN KEY(ProductId) REFERENCES Product(Id),
    CONSTRAINT FK_OrderProduct_Order FOREIGN KEY(OrderId) REFERENCES [Order](Id)
);

INSERT INTO Department([Name],Budget) VALUES ('Marketing',415000);
INSERT INTO Department([Name],Budget) VALUES ('Engineering',57200);
INSERT INTO Department([Name],Budget) VALUES ('Accounting',123000);
INSERT INTO Department([Name],Budget) VALUES ('Legal',923000);

INSERT INTO Computer(PurchaseDate,DecomissionDate,Make,Model) VALUES ('2016-01-01T23:28:56.782Z',NULL,'Apple','Macbook Pro');
INSERT INTO Computer(PurchaseDate,DecomissionDate,Make,Model) VALUES ('2018-02-09T23:28:56.782Z',NULL,'Apple','Macbook Air');
INSERT INTO Computer(PurchaseDate,DecomissionDate,Make,Model) VALUES ('2019-05-05T23:28:56.782Z',NULL,'Microsoft','Suface Pro');
INSERT INTO Computer(PurchaseDate,DecomissionDate,Make,Model) VALUES ('2014-01-01T23:28:56.782Z','2017-03-01T23:28:56.782Z','Lenovo','Thinkpad X1');
INSERT INTO Computer(PurchaseDate,DecomissionDate,Make,Model) VALUES ('2016-01-01T23:28:56.782Z',NULL,'Apple','Macbook Pro');
INSERT INTO Computer(PurchaseDate,DecomissionDate,Make,Model) VALUES ('2016-01-01T23:28:56.782Z',NULL,'System 76','Oryx Pro');
INSERT INTO Computer(PurchaseDate,DecomissionDate,Make,Model) VALUES ('2016-01-01T23:28:56.782Z',NULL,'System 76','Gazelle');
INSERT INTO Computer(PurchaseDate,DecomissionDate,Make,Model) VALUES ('2016-01-01T23:28:56.782Z',NULL,'System 76','Oryx Pro Lite');
INSERT INTO Computer(PurchaseDate,DecomissionDate,Make,Model) VALUES ('2016-01-01T00:00:00.000Z','1970-01-01T00:00:00.000Z','System 76','Oryx Pro Lite');

INSERT INTO Employee(FirstName,LastName,DepartmentId,IsSupervisor,ComputerId,Email) VALUES ('Adam','Sheaffer',1,0,4,'iamtheboss@bangazon.com');
INSERT INTO Employee(FirstName,LastName,DepartmentId,IsSupervisor,ComputerId,Email) VALUES ('Madi','Peper',2,1,3,'everythingisawesome@bangzon.com');
INSERT INTO Employee(FirstName,LastName,DepartmentId,IsSupervisor,ComputerId,Email) VALUES ('Andy','Collins',3,1,2,'losinghorn@bangazon.com');
INSERT INTO Employee(FirstName,LastName,DepartmentId,IsSupervisor,ComputerId,Email) VALUES ('Coach','Steve',4,1,1,'coach@bangazon.com');

INSERT INTO TrainingProgram([Name],StartDate,EndDate,MaxAttendees) VALUES ('GIS Application','2018-09-25T00:00:00.000Z','2018-10-05T00:00:00.000Z',45);
INSERT INTO TrainingProgram([Name],StartDate,EndDate,MaxAttendees) VALUES ('Business Process','2019-05-25T00:00:00.000Z','2018-05-26T00:00:00.000Z',100);
INSERT INTO TrainingProgram([Name],StartDate,EndDate,MaxAttendees) VALUES ('Spanish 101','2019-09-25T00:00:00.000Z','2019-10-05T00:00:00.000Z',20);
INSERT INTO TrainingProgram([Name],StartDate,EndDate,MaxAttendees) VALUES ('Application Architecture','2020-02-15T00:00:00.000Z','2020-02-28T00:00:00.000Z',15);
INSERT INTO TrainingProgram([Name],StartDate,EndDate,MaxAttendees) VALUES ('Ethical Hacking','2020-02-16T00:00:00.000Z','2020-02-28T00:00:00.000Z',15);

INSERT INTO EmployeeTraining(EmployeeId,TrainingProgramId) VALUES (1,1);
INSERT INTO EmployeeTraining(EmployeeId,TrainingProgramId) VALUES (2,1);
INSERT INTO EmployeeTraining(EmployeeId,TrainingProgramId) VALUES (2,2);
INSERT INTO EmployeeTraining(EmployeeId,TrainingProgramId) VALUES (3,3);
INSERT INTO EmployeeTraining(EmployeeId,TrainingProgramId) VALUES (4,2);

INSERT INTO Customer(Active,FirstName,LastName,[Address],City,[State],Email,Phone, CreatedDate) VALUES (1,'Nathanael','Laverenz','401 Nunya Business Dr','Herman','New York','n.lav@sbcglobal.net','6151237584','2018-09-25T00:00:00.000Z');
INSERT INTO Customer(Active,FirstName,LastName,[Address],City,[State],Email,Phone, CreatedDate) VALUES (1,'Chrissy','Vivian','302 Puppy Way','Nashville','Tennessee','vivi@gmail.com','5782036593','2018-09-25T00:00:00.000Z');
INSERT INTO Customer(Active,FirstName,LastName,[Address],City,[State],Email,Phone, CreatedDate) VALUES (1,'Halli','Storten','404 Outtamai Way','Los Angelos','California','halliday@hotmail.com','2893750183','2018-09-25T00:00:00.000Z');
INSERT INTO Customer(Active,FirstName,LastName,[Address],City,[State],Email,Phone, CreatedDate) VALUES (1,'Godfree','Chase','500 Internal Cir','Topeka','Kansas','eerfdogesahc@gmail.com','1238693029','2018-09-25T00:00:00.000Z');
INSERT INTO Customer(Active,FirstName,LastName,[Address],City,[State],Email,Phone, CreatedDate) VALUES (1,'Willi','Warnes','418 Teapot Way','Seattle','Washington','willi.warnes@yahoo.com','7693025473','2018-09-25T00:00:00.000Z');
INSERT INTO Customer(Active,FirstName,LastName,[Address],City,[State],Email,Phone, CreatedDate) VALUES (1,'Salmon','O''''Nions','100 Continue Blvd','Atlanta','Georgia','salmonstriker@gmail.com','6151237584','2018-09-25T00:00:00.000Z');
INSERT INTO Customer(Active,FirstName,LastName,[Address],City,[State],Email,Phone, CreatedDate) VALUES (1,'Kimble','Peskett','508 Loop Cir','Nashville','Tennessee','peskykimble@hotmail.com','5671234567','2018-09-25T00:00:00.000Z');
INSERT INTO Customer(Active,FirstName,LastName,[Address],City,[State],Email,Phone, CreatedDate) VALUES (0,'Laura','Darner','504 Timeout Way','New York City','New York','laura.d@yahoo.com','1987654321','2018-09-25T00:00:00.000Z');

INSERT INTO ProductType([Name]) VALUES ('Tanks');
INSERT INTO ProductType([Name]) VALUES ('Accessories');
INSERT INTO ProductType([Name]) VALUES ('Stands');
INSERT INTO ProductType([Name]) VALUES ('Filters');
INSERT INTO ProductType([Name]) VALUES ('Pebbles');
INSERT INTO ProductType([Name]) VALUES ('Food');
INSERT INTO ProductType([Name]) VALUES ('Cleaning Supplies');
INSERT INTO ProductType([Name]) VALUES ('Bowls');
INSERT INTO ProductType([Name]) VALUES ('Filters');
INSERT INTO ProductType([Name]) VALUES ('Plants');
INSERT INTO ProductType([Name]) VALUES ('Heaters');

INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (2,3,76.91,'morbi ut odio cras mi pede malesuada in imperdiet et commodo','Passat','2019-08-25T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (1,2,62.54,'pede ullamcorper augue a suscipit nulla elit ac nulla sed','Murci�lago LP640','2018-12-25T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (3,1,79.32,'lacinia nisi venenatis tristique fusce congue diam id ornare imperdiet sapien urna pretium nisl ut','Loyale','2018-09-16T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (4,3,10.94,'tristique in tempus sit amet sem fusce consequat nulla nisl nunc nisl','Millenia','2019-12-05T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (5,5,66.57,'eget nunc donec quis orci eget orci vehicula condimentum curabitur','Accord','2019-06-14T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (1,3,79.92,'semper rutrum nulla nunc purus phasellus in felis donec semper sapien a libero','Santa Fe','2019-09-25T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (2,6,9.09,'dolor morbi vel lectus in quam fringilla rhoncus mauris enim leo rhoncus sed vestibulum sit amet','A3','2019-09-25T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (6,7,20.93,'amet turpis elementum ligula vehicula consequat morbi a ipsum integer a nibh in quis justo maecenas rhoncus aliquam lacus','Corvette','2019-09-25T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (1,5,53.45,'faucibus orci luctus et ultrices posuere cubilia curae duis faucibus','Discovery','2019-09-25T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (1,2,18.28,'pellentesque ultrices phasellus id sapien in sapien iaculis congue vivamus metus arcu adipiscing molestie hendrerit at','SLX','2019-09-25T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (4,7,72.25,'sapien varius ut blandit non interdum in ante vestibulum ante ipsum primis','Concorde','2019-09-25T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (4,5,37.99,'eget nunc donec quis orci eget orci vehicula condimentum curabitur','Windstar','2019-09-25T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (2,6,14.14,'curae mauris viverra diam vitae quam suspendisse potenti nullam porttitor lacus at turpis','Sunbird','2019-09-25T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (4,6,51.05,'nec euismod scelerisque quam turpis adipiscing lorem vitae mattis nibh ligula nec sem duis aliquam convallis nunc proin at','XC60','2019-09-25T00:00:00.000Z');
INSERT INTO Product(ProductTypeId,CustomerId,Price,[Description],Title,DateAdded) VALUES (1,5,67.83,'consequat metus sapien ut nunc vestibulum ante ipsum primis in faucibus','Rocky','2019-09-25T00:00:00.000Z');

INSERT INTO PaymentType([Name],Active) VALUES ('Mastercard',1);
INSERT INTO PaymentType([Name],Active) VALUES ('Visa',1);
INSERT INTO PaymentType([Name],Active) VALUES ('Discover',1);
INSERT INTO PaymentType([Name],Active) VALUES ('American Express',1);
INSERT INTO PaymentType([Name],Active) VALUES ('Diners Club',0)

INSERT INTO UserPaymentType(CustomerId,PaymentTypeId,AcctNumber,Active) VALUES (1,5,'2234 56789 0123',1);
INSERT INTO UserPaymentType(CustomerId,PaymentTypeId,AcctNumber,Active) VALUES (2,2,'1234 5678 9012',1);
INSERT INTO UserPaymentType(CustomerId,PaymentTypeId,AcctNumber,Active) VALUES (1,2,'1234 56789 0123',1);
INSERT INTO UserPaymentType(CustomerId,PaymentTypeId,AcctNumber,Active) VALUES (4,1,'1234 56789 0123',1);

INSERT INTO [Order](CustomerId,UserPaymentTypeId) VALUES (1,NULL);
INSERT INTO [Order](CustomerId,UserPaymentTypeId) VALUES (2,2);

INSERT INTO OrderProduct(OrderId,ProductId) VALUES (1,2);
INSERT INTO OrderProduct(OrderId,ProductId) VALUES (1,4);
INSERT INTO OrderProduct(OrderId,ProductId) VALUES (1,3);
INSERT INTO OrderProduct(OrderId,ProductId) VALUES (1,1);
INSERT INTO OrderProduct(OrderId,ProductId) VALUES (2,2);
INSERT INTO OrderProduct(OrderId,ProductId) VALUES (2,1);